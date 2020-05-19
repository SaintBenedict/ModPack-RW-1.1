using System;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_SexQuick : JobDriver_SexBaseInitiator
	{
		private List<Pawn> all_pawns;
		private FloatRange temperature;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, xxx.max_rapists_per_prisoner, 0, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("[RJW]" + this.GetType().ToString() + "::MakeNewToils() called");
			setup_ticks();

			this.FailOnDespawnedNullOrForbidden(iTarget);
			this.FailOn(() => !Partner.health.capacities.CanBeAwake);
			this.FailOn(() => pawn.IsFighting());
			this.FailOn(() => Partner.IsFighting());
			this.FailOn(() => pawn.Drafted);
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			Toil findQuickieSpot = new Toil();
			findQuickieSpot.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			findQuickieSpot.initAction = delegate
			{
				//Needs this earlier to decide if current place is good enough
				all_pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x
				=> x.Position.DistanceTo(pawn.Position) < 100
				&& xxx.is_human(x)
				&& x != pawn
				&& x != Partner
				).ToList();

				temperature = pawn.ComfortableTemperatureRange();
				float cellTemp = pawn.Position.GetTemperature(pawn.Map);

				if (Partner.IsPrisonerInPrisonCell() || (!MightBeSeen(all_pawns,pawn.Position,pawn,Partner) && (cellTemp > temperature.min && cellTemp < temperature.max)))
				{
					ReadyForNextToil();
				}
				else 
				{
					var spot = FindQuickieLocation(pawn, Partner);
					pawn.pather.StartPath(spot, PathEndMode.OnCell);
					Partner.jobs.StopAll();
					Job job = JobMaker.MakeJob(JobDefOf.GotoMindControlled, spot);
					Partner.jobs.StartJob(job, JobCondition.InterruptForced);
				}
			};
			yield return findQuickieSpot;

			Toil WaitForPartner = new Toil();
			WaitForPartner.defaultCompleteMode = ToilCompleteMode.Delay;
			WaitForPartner.initAction = delegate
			{
				ticksLeftThisToil = 5000;
			};
			WaitForPartner.tickAction = delegate
			{
				pawn.GainComfortFromCellIfPossible();
				if (pawn.Position.DistanceTo(Partner.Position) <= 1f)
				{
					ReadyForNextToil();
				}
			};
			yield return WaitForPartner;

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				Job gettingQuickie = JobMaker.MakeJob(xxx.getting_quickie, pawn, Partner);
				Partner.jobs.StartJob(gettingQuickie, JobCondition.InterruptForced);
			};
			yield return StartPartnerJob;


			Toil doQuickie = new Toil();
			doQuickie.defaultCompleteMode = ToilCompleteMode.Never;
			doQuickie.socialMode = RandomSocialMode.Off;
			doQuickie.defaultDuration = duration;
			doQuickie.handlingFacing = true;
			doQuickie.initAction = delegate
			{
				usedCondom = CondomUtility.TryUseCondom(pawn) || CondomUtility.TryUseCondom(Partner);
				Start();
			};
			doQuickie.AddPreTickAction(delegate
			{
				--ticks_left;
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				SexTick(pawn, Partner);
				SexUtility.reduce_rest(Partner, 1);
				SexUtility.reduce_rest(pawn, 1);
				if (ticks_left <= 0)
					ReadyForNextToil();
			});
			doQuickie.AddFinishAction(delegate
			{
				End();
			});
			yield return doQuickie;

			yield return new Toil
			{
				initAction = delegate
				{
					//// Trying to add some interactions and social logs
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, rape: isRape, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		[SyncMethod]
		public virtual IntVec3 FindQuickieLocation(Pawn pawn, Pawn partner)
		{
			IntVec3 position = pawn.Position;
			int bestPosition = -100;
			IntVec3 cell = pawn.Position;
			int maxDistance = 40;

			
			//bool is_somnophile = xxx.has_quirk(pawn, "Somnophile");
			bool is_exhibitionist = xxx.has_quirk(pawn, "Exhibitionist");

			//Log.Message("[RJW] Pawn is " + xxx.get_pawnname(pawn) + ", current cell is " + cell);

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			List<IntVec3> random_cells = new List<IntVec3>();
			for (int loop = 0; loop < 50; ++loop)
			{
				random_cells.Add(position + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.RangeInclusive(1, maxDistance)));
			}

			random_cells = random_cells.Where(x
				=> x.Standable(pawn.Map)
				&& x.InAllowedArea(pawn)
				&& x.GetDangerFor(pawn, pawn.Map) != Danger.Deadly
				&& !x.ContainsTrap(pawn.Map)
				&& !x.ContainsStaticFire(pawn.Map)
				).Distinct().ToList();

			//Log.Message("[RJW] Found " + random_cells.Count + " valid cells.");

			foreach (IntVec3 random_cell in random_cells)
			{
				if (!xxx.can_path_to_target(pawn, random_cell))
					continue;// too far

				int score = 0;
				Room room = random_cell.GetRoom(pawn.Map);

				bool might_be_seen = MightBeSeen(all_pawns, random_cell, pawn, partner);

				if (is_exhibitionist)
				{
					if (might_be_seen)
						score += 5;
					else
						score -= 10;
				}
				else
				{
					if (might_be_seen)
						score -= 30;
				}

				if (random_cell.GetTemperature(pawn.Map) > temperature.min && random_cell.GetTemperature(pawn.Map) < temperature.max)
					score += 20;
				else
					score -= 20;
				if (random_cell.Roofed(pawn.Map))
					score += 5;
				if (random_cell.HasEatSurface(pawn.Map))
					score += 5; // Hide in vegetation.
				if (random_cell.GetDangerFor(pawn, pawn.Map) == Danger.Some)
					score -= 25;
				else if (random_cell.GetDangerFor(pawn, pawn.Map) == Danger.None)
					score += 5;
				if (random_cell.GetTerrain(pawn.Map) == TerrainDefOf.WaterShallow ||
					random_cell.GetTerrain(pawn.Map) == TerrainDefOf.WaterMovingShallow ||
					random_cell.GetTerrain(pawn.Map) == TerrainDefOf.WaterOceanShallow)
					score -= 20;

				if (random_cell.GetThingList(pawn.Map).Any(x => x.def.IsWithinCategory(ThingCategoryDefOf.Corpses)) && !xxx.is_necrophiliac(pawn))
					score -= 20;
				if (random_cell.GetThingList(pawn.Map).Any(x => x.def.IsWithinCategory(ThingCategoryDefOf.Foods)))
					score -= 10;

				if (room == pawn.Position.GetRoom(pawn.MapHeld))
					score -= 10;
				if (room.PsychologicallyOutdoors)
					score += 5;
				if (room.isPrisonCell)
					score += 5;
				if (room.IsHuge)
					score -= 5;
				if (room.ContainedBeds.Any())
					score += 5;
				if (room.IsDoorway)
					score -= 10;
				if (!room.Owners.Any())
					score += 10;
				else if (room.Owners.Contains(pawn))
					score += 20;
				if (room.Role == RoomRoleDefOf.Bedroom || room.Role == RoomRoleDefOf.PrisonCell)
					score += 10;
				else if (room.Role == RoomRoleDefOf.Barracks || room.Role == RoomRoleDefOf.Laboratory || room.Role == RoomRoleDefOf.RecRoom)
					score += 2;
				else if (room.Role == RoomRoleDefOf.DiningRoom || room.Role == RoomRoleDefOf.Hospital || room.Role == RoomRoleDefOf.PrisonBarracks)
					score -= 5;
				if (room.GetStat(RoomStatDefOf.Cleanliness) < 0.01f)
					score -= 5;
				if (room.GetStat(RoomStatDefOf.GraveVisitingJoyGainFactor) > 0.1f)
					score -= 5;

				if (score <= bestPosition) continue;

				bestPosition = score;
				cell = random_cell;
			}

			return cell;

			//Log.Message("[RJW] Best cell is " + cell);
		}

		private bool MightBeSeen(List<Pawn> otherPawns, IntVec3 cell, Pawn pawn, Pawn partner) 
		{
			return otherPawns.Any(x
					=> GenSight.LineOfSight(x.Position, cell, pawn.Map)
					&& x.Position.DistanceTo(cell) < 50
					&& x.Awake()
					&& x != partner
					);
		}
	}
}

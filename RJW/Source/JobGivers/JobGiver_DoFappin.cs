using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;

namespace rjw
{
	public class JobGiver_DoFappin : ThinkNode_JobGiver
	{
		[SyncMethod]
		public virtual IntVec3 FindFapLocation(Pawn pawn)
		{
			IntVec3 position = pawn.Position;
			int bestPosition = -100;
			IntVec3 cell = pawn.Position;
			int maxDistance = 40;

			FloatRange temperature = pawn.ComfortableTemperatureRange();
			bool is_somnophile = xxx.has_quirk(pawn, "Somnophile");
			bool is_exhibitionist = xxx.has_quirk(pawn, "Exhibitionist");
			List<Pawn> all_pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x 
				=> x.Position.DistanceTo(pawn.Position) < 100
				&& xxx.is_human(x)
				&& x != pawn
				).ToList();

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

				bool might_be_seen = all_pawns.Any(x 
					=> GenSight.LineOfSight(x.Position, random_cell, pawn.Map) 
					&& x.Position.DistanceTo(random_cell) < 50 
					&& x.Awake()
					);

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
				if (is_somnophile) // Fap while Watching someone sleep. Not creepy at all!
				{
					if (all_pawns.Any(x 
						=> GenSight.LineOfSight(random_cell, x.Position, pawn.Map) 
						&& x.Position.DistanceTo(random_cell) < 6 
						&& !x.Awake()
						))
						score += 50;
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

		protected override Job TryGiveJob(Pawn pawn)
		{
			//--Log.Message("[RJW] JobGiver_DoFappin::TryGiveJob( " + xxx.get_pawnname(pawn) + " ) called");

			if (pawn.Drafted) return null;

			if (!xxx.can_be_fucked(pawn) && !xxx.can_fuck(pawn)) return null;

			// Whores only fap if frustrated, unless imprisoned.
			if ((SexUtility.ReadyForLovin(pawn) && (!xxx.is_whore(pawn) || pawn.IsPrisoner || xxx.is_slave(pawn))) || xxx.is_frustrated(pawn))
			{
				if (pawn.jobs.curDriver is JobDriver_LayDown && RJWPreferenceSettings.FapInBed)
				{
					Building_Bed bed = ((JobDriver_LayDown)pawn.jobs.curDriver).Bed;
					if (bed != null) return JobMaker.MakeJob(xxx.Masturbate_Bed, null, bed);
				}
				else if ((xxx.is_frustrated(pawn) || xxx.has_quirk(pawn, "Exhibitionist")) && RJWPreferenceSettings.FapEverywhere)
				{
					return JobMaker.MakeJob(xxx.Masturbate_Quick, null, null, FindFapLocation(pawn));
				}
			}
			return null;
		}
	}
}
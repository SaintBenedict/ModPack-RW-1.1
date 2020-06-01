using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobDriver_BestialityForMale : JobDriver_Rape
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, 0, null, errorOnFailed);
		}

		[SyncMethod]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			//--Log.Message("[RJW] JobDriver_BestialityForMale::MakeNewToils() called");

			setup_ticks();

			//this.FailOn (() => (!Partner.health.capacities.CanBeAwake) || (!comfort_prisoners.is_designated (Partner)));
			// Fail if someone else reserves the prisoner before the pawn arrives or colonist can't reach animal
			this.FailOn(() => !pawn.CanReserveAndReach(Partner, PathEndMode.Touch, Danger.Deadly));
			this.FailOn(() => Partner.HostileTo(pawn));
			this.FailOnDespawnedNullOrForbidden(iTarget);
			this.FailOn(() => pawn.Drafted);

			yield return Toils_Reserve.Reserve(iTarget, 1, 0);
			//Log.Message("[RJW] JobDriver_BestialityForMale::MakeNewToils() - moving towards animal");
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(iTarget);

			if (xxx.is_kind(pawn)
				|| (xxx.CTIsActive && xxx.has_traits(pawn) && pawn.story.traits.HasTrait(TraitDef.Named("RCT_AnimalLover"))))
			{
				yield return TalkToAnimal(pawn, Partner);
				yield return TalkToAnimal(pawn, Partner);
			}

			if (Rand.Chance(0.6f))
				yield return TalkToAnimal(pawn, Partner);

			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			SexUtility.RapeTargetAlert(pawn, Partner);

			Toil rape = new Toil();
			rape.defaultCompleteMode = ToilCompleteMode.Delay;
			rape.defaultDuration = duration;
			rape.handlingFacing = true;
			rape.initAction = delegate
			{
				//--Log.Message("[RJW] JobDriver_BestialityForMale::MakeNewToils() - Setting animal job driver");
				if (!(Partner.jobs.curDriver is JobDriver_SexBaseRecieverRaped dri))
				{
					//wild animals may flee or attack
					if (pawn.Faction != Partner.Faction && Partner.RaceProps.wildness > Rand.Range(0.22f, 1.0f)
						&& !(pawn.TicksPerMoveCardinal < (Partner.TicksPerMoveCardinal / 2) && !Partner.Downed && xxx.is_not_dying(Partner)))
					{
						Partner.jobs.StopAll(); // Wake up if sleeping.

						float aggro = Partner.kindDef.RaceProps.manhunterOnTameFailChance;
						if (Partner.kindDef.RaceProps.predator)
							aggro += 0.2f;
						else
							aggro -= 0.1f;

						if (Rand.Chance(aggro) && Partner.CanSee(pawn))
						{
							Partner.rotationTracker.FaceTarget(pawn);
							LifeStageUtility.PlayNearestLifestageSound(Partner, (ls) => ls.soundAngry, 1.4f);
							ThrowMetaIcon(Partner.Position, Partner.Map, ThingDefOf.Mote_IncapIcon);
							ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_ColonistFleeing); //red '!'
							Partner.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
							if (Partner.kindDef.RaceProps.herdAnimal && Rand.Chance(0.2f))
							{ // 20% chance of turning the whole herd hostile...
								List<Pawn> packmates = Partner.Map.mapPawns.AllPawnsSpawned.Where(x =>
									x != Partner && x.def == Partner.def && x.Faction == Partner.Faction &&
									x.Position.InHorDistOf(Partner.Position, 24f) && x.CanSee(Partner)).ToList();

								foreach (Pawn packmate in packmates)
								{
									packmate.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
								}
							}
							Messages.Message(pawn.Name.ToStringShort + " is being attacked by " + xxx.get_pawnname(Partner) + ".", pawn, MessageTypeDefOf.ThreatSmall);
						}
						else
						{
							ThrowMetaIcon(Partner.Position, Partner.Map, ThingDefOf.Mote_ColonistFleeing);
							LifeStageUtility.PlayNearestLifestageSound(Partner, (ls) => ls.soundCall);
							Partner.mindState.StartFleeingBecauseOfPawnAction(pawn);
							Partner.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
						}
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					else
					{
						Job gettin_bred = JobMaker.MakeJob(xxx.gettin_bred, pawn, Partner);
						Partner.jobs.StartJob(gettin_bred, JobCondition.InterruptForced, null, true);
						(Partner.jobs.curDriver as JobDriver_SexBaseRecieverRaped)?.increase_time(ticks_left);
						Start();
					}
				}
				else
				{
					Start();
				}
			};
			rape.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					if (xxx.is_zoophile(pawn))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
					else
						ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
				SexTick(pawn, Partner);
				/*
				if (pawn.IsHashIntervalTick (ticks_between_hits))
					roll_to_hit (pawn, Partner);
					*/
				SexUtility.reduce_rest(Partner, 1);
				SexUtility.reduce_rest(pawn, 2);

			};
			rape.AddFinishAction(delegate
			{
				End();
			});
			yield return rape;

			yield return new Toil
			{
				initAction = delegate
				{
					//Log.Message("[RJW] JobDriver_BestialityForMale::MakeNewToils() - creating aftersex toil");
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}

		[SyncMethod]
		private Toil TalkToAnimal(Pawn pawn, Pawn animal)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				pawn.interactions.TryInteractWith(animal, SexUtility.AnimalSexChat);
			};
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = Rand.Range(120, 220);
			return toil;
		}
	}
}

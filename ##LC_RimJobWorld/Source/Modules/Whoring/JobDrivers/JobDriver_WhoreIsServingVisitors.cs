using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobDriver_WhoreIsServingVisitors : JobDriver_SexBaseInitiator
	{
		public static readonly ThoughtDef thought_free = ThoughtDef.Named("Whorish_Thoughts");
		public static readonly ThoughtDef thought_captive = ThoughtDef.Named("Whorish_Thoughts_Captive");
		public IntVec3 SleepSpot => Bed.SleepPosOfAssignedPawn(pawn);

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, 0, null, errorOnFailed);
		}

		[SyncMethod]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() - making toils");
			setup_ticks();

			this.FailOnDespawnedOrNull(iTarget);
			this.FailOnDespawnedNullOrForbidden(iBed);
			//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() fail conditions check " + !xxx.CanUse(pawn, Bed) + " " + !pawn.CanReserve(Partner));
			this.FailOn(() => !xxx.CanUse(pawn, Bed) || !pawn.CanReserve(Partner));
			this.FailOn(() => pawn.Drafted);
			this.FailOn(() => Partner.IsFighting());
			this.FailOn(() => !Partner.CanReach(pawn, PathEndMode.Touch, Danger.Deadly));

			yield return Toils_Reserve.Reserve(iTarget, 1, 0);
			//yield return Toils_Reserve.Reserve(BedInd, Bed.SleepingSlotsCount, 0);

			//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() - generate toils");
			Toil gotoBed = new Toil();
			gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoBed.FailOnWhorebedNoLongerUsable(iBed, Bed);
			gotoBed.AddFailCondition(() => Partner.Downed);
			gotoBed.FailOn(() => !Partner.CanReach(Bed, PathEndMode.Touch, Danger.Deadly));
			gotoBed.initAction = delegate
			{
				//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() - gotoWhoreBed initAction is called");
				pawn.pather.StartPath(SleepSpot, PathEndMode.OnCell);
				Partner.jobs.StopAll();
				Job job = JobMaker.MakeJob(JobDefOf.GotoMindControlled, SleepSpot);
				Partner.jobs.StartJob(job, JobCondition.InterruptForced);
			};
			yield return gotoBed;

			ticks_left = (int)(2000.0f * Rand.Range(0.30f, 1.30f));

			Toil waitInBed = new Toil();
			waitInBed.initAction = delegate
			{
				ticksLeftThisToil = 5000;
			};
			waitInBed.tickAction = delegate
			{
				pawn.GainComfortFromCellIfPossible();
				if (IsInOrByBed(Bed, Partner) && pawn.PositionHeld == Partner.PositionHeld)
				{
					ReadyForNextToil();
				}
			};
			waitInBed.defaultCompleteMode = ToilCompleteMode.Delay;
			yield return waitInBed;

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() - StartPartnerJob");
				var gettin_loved = JobMaker.MakeJob(xxx.gettin_loved, pawn, Bed);
				Partner.jobs.StartJob(gettin_loved, JobCondition.InterruptForced);
			};
			yield return StartPartnerJob;

			Toil loveToil = new Toil();
			loveToil.AddFailCondition(() => Partner.Dead || Partner.CurJobDef != xxx.gettin_loved);
			loveToil.defaultCompleteMode = ToilCompleteMode.Never;
			loveToil.socialMode = RandomSocialMode.Off;
			loveToil.handlingFacing = true;
			loveToil.initAction = delegate
			{
				//Log.Message("[RJW]JobDriver_WhoreIsServingVisitors::MakeNewToils() - loveToil");
				// TODO: replace this quick n dirty way
				CondomUtility.GetCondomFromRoom(pawn);

				// Try to use whore's condom first, then client's
				usedCondom = CondomUtility.TryUseCondom(pawn) || CondomUtility.TryUseCondom(Partner);

				Start();

				if (xxx.HasNonPolyPartnerOnCurrentMap(Partner))
				{
					Pawn lover = LovePartnerRelationUtility.ExistingLovePartner(Partner);
					// We have to do a few other checks because the pawn might have multiple lovers and ExistingLovePartner() might return the wrong one
					if (lover != null && pawn != lover && !lover.Dead && (lover.Map == Partner.Map || Rand.Value < 0.25))
					{
						lover.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, Partner);
					}
				}
			};
			loveToil.AddPreTickAction(delegate
			{
				--ticks_left;
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					if (xxx.is_nympho(pawn))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
					else
						ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
				SexUtility.reduce_rest(Partner, 1);
				SexUtility.reduce_rest(pawn, 2);
				if (ticks_left <= 0)
					ReadyForNextToil();
			});
			loveToil.AddFinishAction(delegate
			{
				End();
			});
			yield return loveToil;

			Toil afterSex = new Toil
			{
				initAction = delegate
				{
					// Adding interactions, social logs, etc
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, whoring: isWhoring, sextype: sexType);

					if (!(Partner.IsColonist && (pawn.IsPrisonerOfColony || pawn.IsColonist)))
					{
						int price = WhoringHelper.PriceOfWhore(pawn);
						//--Log.Message("JobDriver_WhoreIsServingVisitors::MakeNewToils() - Partner should pay the price now in afterSex.initAction");
						int remainPrice = WhoringHelper.PayPriceToWhore(Partner, price, pawn);
						/*if (remainPrice <= 0)
						{
							--Log.Message("JobDriver_WhoreIsServingVisitors::MakeNewToils() - Paying price is success");
						}
						else
						{
							--Log.Message("JobDriver_WhoreIsServingVisitors::MakeNewToils() - Paying price failed");
						}*/
						xxx.UpdateRecords(pawn, price - remainPrice);
					}
					var thought = (pawn.IsPrisoner || xxx.is_slave(pawn)) ? thought_captive : thought_free;
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
					if (SexUtility.ConsiderCleaning(pawn))
					{
						LocalTargetInfo cum = pawn.PositionHeld.GetFirstThing<Filth>(pawn.Map);

						Job clean = JobMaker.MakeJob(JobDefOf.Clean);
						clean.AddQueuedTarget(TargetIndex.A, cum);

						pawn.jobs.jobQueue.EnqueueFirst(clean);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return afterSex;
		}
	}
}

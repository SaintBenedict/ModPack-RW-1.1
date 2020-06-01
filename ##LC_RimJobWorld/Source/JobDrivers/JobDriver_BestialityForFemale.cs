using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobDriver_BestialityForFemale : JobDriver_SexBaseInitiator
	{
		public IntVec3 SleepSpot => Bed.SleepPosOfAssignedPawn(pawn);
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, 0, null, errorOnFailed);
		}

		[SyncMethod]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();

			this.FailOnDespawnedOrNull(iTarget);
			this.FailOnDespawnedNullOrForbidden(iBed);
			this.FailOn(() => !pawn.CanReserveAndReach(Partner, PathEndMode.Touch, Danger.Deadly));
			this.FailOn(() => pawn.Drafted);
			this.FailOn(() => Partner.IsFighting());
			this.FailOn(() => !Partner.CanReach(pawn, PathEndMode.Touch, Danger.Deadly));

			yield return Toils_Reserve.Reserve(iTarget, 1, 0);
			Toil gotoAnimal = Toils_Goto.GotoThing(iTarget, PathEndMode.Touch);
			yield return gotoAnimal;

			Toil gotoBed = new Toil();
			gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			gotoBed.FailOnBedNoLongerUsable(iBed);
			gotoBed.AddFailCondition(() => Partner.Downed);
			gotoBed.initAction = delegate
			{
				pawn.pather.StartPath(SleepSpot, PathEndMode.OnCell);
				Partner.jobs.StopAll();
				Job job = JobMaker.MakeJob(JobDefOf.GotoMindControlled, SleepSpot);
				Partner.jobs.StartJob(job, JobCondition.InterruptForced);
			};
			yield return gotoBed;

			Toil waitInBed = new Toil();
			waitInBed.FailOn(() => pawn.GetRoom(RegionType.Set_Passable) == null);
			waitInBed.defaultCompleteMode = ToilCompleteMode.Delay;
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
			yield return waitInBed;

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				var gettin_loved = JobMaker.MakeJob(xxx.gettin_loved, pawn, Bed);
				Partner.jobs.StartJob(gettin_loved, JobCondition.InterruptForced);
			};
			yield return StartPartnerJob;

			Toil loveToil = new Toil();
			loveToil.AddFailCondition(() => Partner.Dead || !IsInOrByBed(Bed, Partner));
			loveToil.socialMode = RandomSocialMode.Off;
			loveToil.defaultCompleteMode = ToilCompleteMode.Never;
			loveToil.handlingFacing = true;
			loveToil.initAction = delegate
			{
				usedCondom = CondomUtility.TryUseCondom(pawn);
				Start();
			};
			loveToil.AddPreTickAction(delegate
			{
				--ticks_left;
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					if (xxx.is_zoophile(pawn))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
					else
						ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
				SexTick(pawn, Partner);
				SexUtility.reduce_rest(pawn, 1);
				SexUtility.reduce_rest(Partner, 2);
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
					//Log.Message("JobDriver_BestialityForFemale::MakeNewToils() - Calling aftersex");
					SexUtility.ProcessSex(Partner, pawn, usedCondom: usedCondom, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return afterSex;
		}
	}
}

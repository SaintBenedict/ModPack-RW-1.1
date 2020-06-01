using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_JoinInBed : JobDriver_SexBaseInitiator
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, xxx.max_rapists_per_prisoner, 0, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("[RJW]" + this.GetType().ToString() + "::MakeNewToils() called");

			setup_ticks();

			this.FailOnDespawnedOrNull(iTarget);
			this.FailOnDespawnedOrNull(iBed);
			this.FailOn(() => !Partner.health.capacities.CanBeAwake);
			this.FailOn(() => !(Partner.InBed() || xxx.in_same_bed(Partner, pawn)));
			this.FailOn(() => pawn.Drafted);
			yield return Toils_Reserve.Reserve(iTarget, xxx.max_rapists_per_prisoner, 0);
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				Job gettin_loved = JobMaker.MakeJob(xxx.gettin_loved, pawn, Bed);
				Partner.jobs.StartJob(gettin_loved, JobCondition.InterruptForced);
			};
			yield return StartPartnerJob;

			Toil do_lovin = new Toil();
			do_lovin.FailOn(() => Partner.CurJob.def != xxx.gettin_loved);
			do_lovin.defaultCompleteMode = ToilCompleteMode.Never;
			do_lovin.socialMode = RandomSocialMode.Off;
			do_lovin.handlingFacing = true;
			do_lovin.initAction = delegate
			{
				usedCondom = CondomUtility.TryUseCondom(pawn) || CondomUtility.TryUseCondom(Partner);
				Start();
			};
			do_lovin.AddPreTickAction(delegate
			{
				--ticks_left;
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				SexTick(pawn, Partner);
				SexUtility.reduce_rest(Partner, 1);
				SexUtility.reduce_rest(pawn, 2);
				if (ticks_left <= 0)
					ReadyForNextToil();
			});
			do_lovin.AddFinishAction(delegate
			{
				End();
			});
			yield return do_lovin;

			yield return new Toil
			{
				initAction = delegate
				{
					// Trying to add some interactions and social logs
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, rape: isRape, whoring: isWhoring, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}

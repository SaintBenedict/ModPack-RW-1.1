using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_Rape : JobDriver_SexBaseInitiator
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, xxx.max_rapists_per_prisoner, 0, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			//Log.Message("[RJW]" + this.GetType().ToString() + "::MakeNewToils() called");
			setup_ticks();

			this.FailOnDespawnedNullOrForbidden(iTarget);
			this.FailOn(() => !pawn.CanReserve(Partner, xxx.max_rapists_per_prisoner, 0)); // Fail if someone else reserves the prisoner before the pawn arrives
			this.FailOn(() => pawn.IsFighting());
			this.FailOn(() => Partner.IsFighting());
			this.FailOn(() => pawn.Drafted);
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			SexUtility.RapeTargetAlert(pawn, Partner);

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				var dri = Partner.jobs.curDriver as JobDriver_SexBaseRecieverRaped;
				if (dri == null)
				{
					Job gettin_raped = JobMaker.MakeJob(xxx.gettin_raped, pawn);
					Building_Bed Bed = null;
					if (Partner.GetPosture() == PawnPosture.LayingInBed)
						Bed = Partner.CurrentBed();

					Partner.jobs.StartJob(gettin_raped, JobCondition.InterruptForced, null, false, true, null);
					if (Bed != null)
						(Partner.jobs.curDriver as JobDriver_SexBaseRecieverRaped)?.Set_bed(Bed);
				}
			};
			yield return StartPartnerJob;

			var rape = new Toil();
			rape.FailOn(() => Partner.CurJob == null || Partner.CurJob.def != xxx.gettin_raped || Partner.IsFighting() || pawn.IsFighting());
			rape.defaultCompleteMode = ToilCompleteMode.Delay;
			rape.defaultDuration = duration;
			rape.handlingFacing = true;
			rape.initAction = delegate
			{
				Start();
			};
			rape.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				if (pawn.IsHashIntervalTick(ticks_between_hits))
					Roll_to_hit(pawn, Partner);
				SexTick(pawn, Partner);
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
					//// Trying to add some interactions and social logs
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, rape: isRape, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}

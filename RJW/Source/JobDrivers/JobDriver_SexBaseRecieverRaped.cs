using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_SexBaseRecieverRaped : JobDriver_SexBaseReciever
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();
			parteners.Add(Partner);// add job starter, so this wont fail, before Initiator starts his job

			var get_raped = new Toil();
			get_raped.defaultCompleteMode = ToilCompleteMode.Never;
			get_raped.handlingFacing = true;
			get_raped.initAction = delegate
			{
				pawn.pather.StopDead();
				pawn.jobs.curDriver.asleep = false;

				SexUtility.BeeingRapedAlert(Partner, pawn);
			};
			get_raped.tickAction = delegate
			{
				--ticks_remaining;
				if ((parteners.Count > 0) && (pawn.IsHashIntervalTick(ticks_between_hearts / parteners.Count)))
					if (pawn.IsHashIntervalTick(ticks_between_hearts))
						if (xxx.is_masochist(pawn))
							ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
						else
							ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
			};
			get_raped.AddEndCondition(new Func<JobCondition>(() =>
			{
				if ((ticks_remaining <= 0) || (parteners.Count <= 0))
				{
					return JobCondition.Succeeded;
				}
				return JobCondition.Ongoing;
			}));
			get_raped.AddFinishAction(delegate
			{
				if (xxx.is_human(pawn))
					pawn.Drawer.renderer.graphics.ResolveApparelGraphics();

				if (Bed != null && pawn.Downed)
				{
					Job tobed = JobMaker.MakeJob(JobDefOf.Rescue, pawn, Bed);
					tobed.count = 1;
					Partner.jobs.jobQueue.EnqueueFirst(tobed);
					//Log.Message(xxx.get_pawnname(Initiator) + ": job tobed:" + tobed);
				}
				else if (pawn.HostileTo(Partner))
					pawn.health.AddHediff(HediffDef.Named("Hediff_Submitting"));
				else
					pawn.stances.stunner.StunFor(600, pawn);

			});
			get_raped.socialMode = RandomSocialMode.Off;
			yield return get_raped;

		}
	}
}
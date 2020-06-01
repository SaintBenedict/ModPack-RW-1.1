using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_SexBaseRecieverQuickie : JobDriver_SexBaseReciever
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();
			parteners.Add(Partner);// add job starter, so this wont fail, before Initiator starts his job
			float partner_ability = xxx.get_sex_ability(Partner);

			// More/less hearts based on partner ability.
			if (partner_ability < 0.8f)
				ticks_between_thrusts += 120;
			else if (partner_ability > 2.0f)
				ticks_between_thrusts -= 30;

			// More/less hearts based on opinion.
			if (pawn.relations.OpinionOf(Partner) < 0)
				ticks_between_hearts += 50;
			else if (pawn.relations.OpinionOf(Partner) > 60)
				ticks_between_hearts -= 25;

			this.FailOnDespawnedOrNull(iTarget);
			this.FailOn(() => !Partner.health.capacities.CanBeAwake);

			yield return Toils_Reserve.Reserve(iTarget, 1, 0);

			Toil get_loved = new Toil();
			get_loved.defaultCompleteMode = ToilCompleteMode.Never;
			get_loved.socialMode = RandomSocialMode.Off;
			get_loved.initAction = delegate
			{
				pawn.pather.StopDead();
				pawn.jobs.curDriver.asleep = false;
			};
			get_loved.AddPreTickAction(delegate
			{
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
			});
			get_loved.AddEndCondition(new Func<JobCondition>(() =>
			{
				if ((ticks_remaining <= 0) || (parteners.Count <= 0))
				{
					return JobCondition.Succeeded;
				}
				return JobCondition.Ongoing;
			}));
			get_loved.AddFinishAction(delegate
			{
				if (xxx.is_human(pawn))
					pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
			});
			yield return get_loved;
		}
	}
}
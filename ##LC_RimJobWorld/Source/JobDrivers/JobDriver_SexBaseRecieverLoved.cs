using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System;

namespace rjw
{
	public class JobDriver_SexBaseRecieverLoved : JobDriver_SexBaseReciever
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();
			parteners.Add(Partner);// add job starter, so this wont fail, before Initiator starts his job
			//--Log.Message("[RJW]JobDriver_GettinLoved::MakeNewToils is called");

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

			if (Partner.CurJob.def == xxx.casual_sex)
			{
				this.FailOn(() => pawn.Drafted);
				this.KeepLyingDown(iBed);
				yield return Toils_Reserve.Reserve(iTarget, 1, 0);
				yield return Toils_Reserve.Reserve(iBed, Bed.SleepingSlotsCount, 0);

				Toil get_loved = Toils_LayDown.LayDown(iBed, true, false, false, false);
				get_loved.FailOn(() => Partner.CurJob.def != xxx.casual_sex);
				get_loved.defaultCompleteMode = ToilCompleteMode.Never;
				get_loved.socialMode = RandomSocialMode.Off;
				get_loved.handlingFacing = true;
				get_loved.tickAction = delegate
				{
					if (pawn.IsHashIntervalTick(ticks_between_hearts))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				};
				get_loved.AddFinishAction(delegate
				{
					if (xxx.is_human(pawn))
						pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
				});
				yield return get_loved;
			}
			else if (Partner.CurJob.def == xxx.whore_is_serving_visitors)
			{
				this.FailOn(() => Partner.CurJob == null);
				yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);
				yield return Toils_Reserve.Reserve(iTarget, 1, 0);

				Toil get_loved = new Toil();
				get_loved.FailOn(() => (Partner.CurJob.def != xxx.whore_is_serving_visitors));
				get_loved.defaultCompleteMode = ToilCompleteMode.Never;
				get_loved.socialMode = RandomSocialMode.Off;
				get_loved.handlingFacing = true;
				get_loved.tickAction = delegate
				{
					--ticks_remaining;
					if (pawn.IsHashIntervalTick(ticks_between_hearts))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				};
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
			else if (Partner.CurJob.def == xxx.bestialityForFemale)
			{
				this.FailOn(() => Partner.CurJob == null);
				yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);
				yield return Toils_Reserve.Reserve(iTarget, 1, 0);

				Toil get_loved = new Toil();
				get_loved.FailOn(() => (Partner.CurJob.def != xxx.bestialityForFemale));
				get_loved.defaultCompleteMode = ToilCompleteMode.Never;
				get_loved.socialMode = RandomSocialMode.Off;
				get_loved.handlingFacing = true;
				get_loved.tickAction = delegate
				{
					--ticks_remaining;
					if (pawn.IsHashIntervalTick(ticks_between_hearts))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				};
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
				get_loved.socialMode = RandomSocialMode.Off;
				yield return get_loved;
			}
		}
	}
}
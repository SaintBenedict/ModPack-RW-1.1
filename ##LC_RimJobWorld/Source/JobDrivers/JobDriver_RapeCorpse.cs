using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace rjw
{
	public class JobDriver_ViolateCorpse : JobDriver_Rape
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			//--Log.Message("[RJW] JobDriver_ViolateCorpse::MakeNewToils() called");
			setup_ticks();

			this.FailOnDespawnedNullOrForbidden(iTarget);
			this.FailOn(() => !pawn.CanReserve(Target, 1, 0));  // Fail if someone else reserves the prisoner before the pawn arrives
			this.FailOn(() => pawn.IsFighting());
			this.FailOn(() => pawn.Drafted);
			this.FailOn(Target.IsBurning);

			//--Log.Message("[RJW] JobDriver_ViolateCorpse::MakeNewToils() - moving towards Target");
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			var alert = RJWPreferenceSettings.rape_attempt_alert == RJWPreferenceSettings.RapeAlert.Disabled ? 
				MessageTypeDefOf.SilentInput : MessageTypeDefOf.NeutralEvent;
			Messages.Message(xxx.get_pawnname(pawn) + " is trying to rape a corpse of " + xxx.get_pawnname(Partner), pawn, alert);

			setup_ticks();// re-setup ticks on arrival

			var rape = new Toil();
			rape.defaultCompleteMode = ToilCompleteMode.Delay;
			rape.defaultDuration = duration;
			rape.handlingFacing = true;
			rape.initAction = delegate
			{
				//--Log.Message("[RJW] JobDriver_ViolateCorpse::MakeNewToils() - stripping Target");
				(Target as Corpse).Strip();
				Start();
			};
			rape.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					if (xxx.is_necrophiliac(pawn))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
					else
						ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
				//if (pawn.IsHashIntervalTick (ticks_between_hits))
				//	roll_to_hit (pawn, Target);
				SexTick(pawn, Target);
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
					//--Log.Message("[RJW] JobDriver_ViolateCorpse::MakeNewToils() - creating aftersex toil");
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, rape: isRape, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
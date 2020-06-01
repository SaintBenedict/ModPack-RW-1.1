using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobDriver_Masturbate_Bed : JobDriver_SexBaseInitiator
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Bed, job, 1, 0, null, errorOnFailed);
		}

		[SyncMethod]
		new public void setup_ticks()
		{
			base.setup_ticks();
			// Faster fapping when frustrated.
			duration = (int)(xxx.is_frustrated(pawn) ? 2500.0f * Rand.Range(0.2f, 0.7f) : 2500.0f * Rand.Range(0.2f, 0.4f));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();

			//this.FailOn(() => PawnUtility.PlayerForcedJobNowOrSoon(pawn));
			this.FailOn(() => pawn.health.Downed);
			this.FailOn(() => pawn.IsBurning());
			this.FailOn(() => pawn.IsFighting());
			this.FailOn(() => pawn.Drafted);

			Toil findfapspot = new Toil
			{
				initAction = delegate
				{
					pawn.pather.StartPath(Bed.Position, PathEndMode.OnCell);
				},
				defaultCompleteMode = ToilCompleteMode.PatherArrival
			};
			yield return findfapspot;

			Toil fap = Toils_General.Wait(duration);
			fap.handlingFacing = true;
			fap.initAction = delegate
			{
				Start();
			};
			fap.tickAction = delegate
			{
				--duration;
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				SexTick(pawn, null);
				SexUtility.reduce_rest(pawn, 1);
				if (duration <= 0)
					ReadyForNextToil();
			};
			fap.AddFinishAction(delegate
			{
				End();
			});
			yield return fap;

			yield return new Toil
			{
				initAction = delegate
				{
					SexUtility.Aftersex(pawn, xxx.rjwSextype.Masturbation);
					if (!SexUtility.ConsiderCleaning(pawn)) return;

					LocalTargetInfo own_cum = pawn.PositionHeld.GetFirstThing<Filth>(pawn.Map);

					Job clean = JobMaker.MakeJob(JobDefOf.Clean);
					clean.AddQueuedTarget(TargetIndex.A, own_cum);

					pawn.jobs.jobQueue.EnqueueFirst(clean);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
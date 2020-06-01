using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace rjw
{
	class JobDriver_CleanSelf : JobDriver
	{
		float cleanAmount = 1f;//severity of a single SemenHediff removed per cleaning-round; 1f = remove entirely
		int cleaningTime = 120;//ticks - 120 = 2 real seconds, 3 in-game minutes

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				return !hediffs.Exists(x => x.def == RJW_SemenoOverlayHediffDefOf.Hediff_Bukkake);//fail if bukkake disappears - means that also all the semen is gone
			});
			Toil cleaning = Toils_General.Wait(cleaningTime, TargetIndex.None);//duration of 
			cleaning.WithProgressBarToilDelay(TargetIndex.A);

			yield return cleaning;
			yield return new Toil()
			{
				initAction = delegate ()
				{
					//get one of the semen hediffs, reduce its severity
					Hediff hediff = pawn.health.hediffSet.hediffs.Find(x => (x.def == RJW_SemenoOverlayHediffDefOf.Hediff_Semen || x.def == RJW_SemenoOverlayHediffDefOf.Hediff_InsectSpunk || x.def == RJW_SemenoOverlayHediffDefOf.Hediff_MechaFluids));
					if (hediff != null)
					{
						hediff.Severity -= cleanAmount;
					}
				}
			};
			yield break;

		}
	}
}

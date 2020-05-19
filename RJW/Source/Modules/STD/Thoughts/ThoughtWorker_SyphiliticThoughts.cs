using RimWorld;
using Verse;

namespace rjw
{
	public class ThoughtWorker_SyphiliticThoughts : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			//Log.Message("0");
			var syp = p.health.hediffSet.GetFirstHediffOfDef(std.syphilis.hediff_def);
			//Log.Message("1");
			if (syp != null)
			{
				//Log.Message("2");
				if (syp.Severity >= 0.80f)
				{
					//Log.Message("3");
					return ThoughtState.ActiveAtStage(1);
				}
				else if (syp.Severity >= 0.50f)
				{
					//Log.Message("4");
					return ThoughtState.ActiveAtStage(0);
				}
				//Log.Message("5");
			}
			//Log.Message("6");
			return ThoughtState.Inactive;
		}
	}
}
using RimWorld;
using Verse;

namespace rjw
{
	public class ThoughtWorker_ItchyCrotch : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			int sev = std.genital_rash_severity(p);
			if (sev <= 0)
				return ThoughtState.Inactive;
			else if (sev == 1)
				return ThoughtState.ActiveAtStage(0);
			else
				return ThoughtState.ActiveAtStage(1);
		}
	}
}
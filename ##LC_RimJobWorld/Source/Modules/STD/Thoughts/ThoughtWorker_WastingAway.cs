using RimWorld;
using Verse;

namespace rjw
{
	public class ThoughtWorker_WastingAway : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!std.is_wasting_away(p))
				return ThoughtState.Inactive;
			else
				return ThoughtState.ActiveAtStage(0);
		}
	}
}
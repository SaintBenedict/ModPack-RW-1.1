using RimWorld;
using Verse;

namespace rjw
{
	public class ThoughtWorker_NeedSex : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var sex_need = p.needs.TryGetNeed<Need_Sex>();
			var p_age = p.ageTracker.AgeBiologicalYears;
			if (sex_need != null && (p_age >= RJWSettings.sex_minimum_age || (!xxx.is_human(p) && p.ageTracker.CurLifeStage.reproductive)))
			{
				var lev = sex_need.CurLevel;
				if (lev <= sex_need.thresh_frustrated())
					return ThoughtState.ActiveAtStage(0);
				else if (lev <= sex_need.thresh_horny())
					return ThoughtState.ActiveAtStage(1);
				else if (lev >= sex_need.thresh_ahegao())
					return ThoughtState.ActiveAtStage(3);
				else if (lev >= sex_need.thresh_satisfied())
					return ThoughtState.ActiveAtStage(2);
				else
					return ThoughtState.Inactive;
			}
			else
				return ThoughtState.Inactive;
		}
	}
}
using Verse;

namespace rjw
{
	public class Hediff_ID : Hediff
	{
		public override string LabelBase
		{
			get
			{
				if (!pawn.health.hediffSet.HasHediff(std.hiv.hediff_def))
					return base.LabelBase;
				else
					return "AIDS";
			}
		}
	}
}
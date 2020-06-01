using RimWorld;
using Verse;

namespace rjw
{
	public class WorkGiver_MilkHuman : WorkGiver_GatherHumanBodyResources
	{
		protected override JobDef JobDef => JobDefOfZ.MilkHuman;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return ThingCompUtility.TryGetComp<CompMilkableHuman>(animal);
		}
	}
}

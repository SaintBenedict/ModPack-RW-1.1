using RimWorld;
using Verse;

namespace rjw
{
	public class JobDriver_MilkHuman : JobDriver_GatherHumanBodyResources
	{
		protected override float WorkTotal => 400f;

		protected override CompHasGatherableBodyResource GetComp(Pawn animal)
		{
			return ThingCompUtility.TryGetComp<CompMilkableHuman>(animal);
		}
	}
}

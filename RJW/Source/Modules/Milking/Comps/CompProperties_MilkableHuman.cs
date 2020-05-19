using Verse;

namespace rjw
{
	public class CompProperties_MilkableHuman : CompProperties
	{
		public int milkIntervalDays;

		public int milkAmount = 8;

		public ThingDef milkDef;

		public bool milkFemaleOnly = true;

		public CompProperties_MilkableHuman()
		{
			compClass = typeof(CompMilkableHuman);
		}
	}
}

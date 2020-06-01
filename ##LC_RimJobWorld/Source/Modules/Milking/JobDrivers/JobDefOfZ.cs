using RimWorld;
using Verse;

namespace rjw
{
	[DefOf]
	public static class JobDefOfZ
	{
		public static JobDef MilkHuman;

		static JobDefOfZ()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
		}
	}
}

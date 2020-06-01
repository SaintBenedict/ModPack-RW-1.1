using Verse;
using System.Linq;

namespace rjw
{
	[StaticConstructorOnStartup]
	public static class AddComp
	{
		static AddComp()
		{
			AddRJWComp();
		}

		/// <summary>
		/// This automatically adds the comp to all races on startup.
		/// </summary>
		public static void AddRJWComp()
		{
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
					thingDef.race != null))
			{
				thingDef.comps.Add(new CompProperties_RJW());
				//Log.Message("AddRJWComp to race " + thingDef.label);
			}
		}
	}
}
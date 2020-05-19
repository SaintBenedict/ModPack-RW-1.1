using System.Linq;
using Verse;

namespace rjw
{
	/// <summary>
	/// Patch all races into rjw parts recipes 
	/// </summary>

	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			//summons carpet bombing

			//inject races into rjw recipes
			foreach (RecipeDef x in	DefDatabase<RecipeDef>.AllDefsListForReading.Where(x => x.IsSurgery && (x.targetsBodyPart || !x.appliedOnFixedBodyParts.NullOrEmpty())))
			{
				if (x.appliedOnFixedBodyParts.Contains(xxx.genitalsDef)
					|| x.appliedOnFixedBodyParts.Contains(xxx.breastsDef)
					|| x.appliedOnFixedBodyParts.Contains(xxx.anusDef)
					)

					foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
							thingDef.race != null && (
							thingDef.race.Humanlike ||
							thingDef.race.Animal
							)))
					{
						//filter out something, probably?
						//if (thingDef.race. == "Human")
						//	continue;

						if (!x.recipeUsers.Contains(thingDef))
							x.recipeUsers.Add(item: thingDef);
					}
			}
		}
	}
}

using System.Collections.Generic;
using Verse;
using System.Linq;

namespace rjw
{
	[StaticConstructorOnStartup]
	internal class HediffDef_EnemyImplants : HediffDef
	{
		//single parent eggs
		public string parentDef = "";
		//multiparent eggs
		public List<string> parentDefs = new List<string>();

		//for implanting eggs
		public bool IsParent(string defnam)
		{
			return
					//predefined parent eggs
					parentDef == defnam ||
					parentDefs.Contains(defnam) ||
					//dynamic egg
					(parentDef == "Unknown" && defnam == "Unknown" && RJWPregnancySettings.egg_pregnancy_implant_anyone);
		}
	}

	[StaticConstructorOnStartup]
	internal class HediffDef_InsectEgg : HediffDef_EnemyImplants
	{
		//this is filled from xml
		//1 day = 60000 ticks
		public float eggsize = 1;
		public bool selffertilized = false;
	}

	[StaticConstructorOnStartup]
	internal class HediffDef_MechImplants : HediffDef_EnemyImplants
	{
		public List<string> randomHediffDefs = new List<string>();
		public int minEventInterval = 30000;
		public int maxEventInterval = 90000;
	}
}
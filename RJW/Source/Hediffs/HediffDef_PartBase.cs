using System.Linq;
using Verse;
using RimWorld;
using System.Text;
using Multiplayer.API;
using UnityEngine;

namespace rjw
{
//TODO figure out how this thing works and move eggs to comps

	[StaticConstructorOnStartup]
	public class HediffDef_PartBase : HediffDef
	{
		public bool discovered = false;
		public string Eggs = "";				//for ovi eggs, maybe
		public string FluidType = "";			//cummies/milk - insectjelly/honey etc
		public string DefaultBodyPart = "";		//Bodypart to move this part to, after fucking up with pc or other mod
		public float FluidAmmount = 0;			//ammount of Milk/Ejaculation/Wetness
		public bool produceEggs;				//set in xml
		public int minEggTick = 12000;
		public int maxEggTick = 120000;
	}
}
using HarmonyLib;
using RimWorld;
using Verse;


namespace rjw
{
	///<summary>
	///RJW Designators checks/update
	///update designators only for selected pawn, once, instead of every tick(60 times per sec)
	///</summary>
	[HarmonyPatch(typeof(Selector), "Select")]
	[StaticConstructorOnStartup]
	static class PawnSelect
	{
		[HarmonyPrefix]
		private static bool pawnSelect(Selector __instance, ref object obj)
		{
			if (obj is Pawn)
			{
				//Log.Message("[rjw]Selector patch");
				Pawn pawn = (Pawn)obj;
				//Log.Message("[rjw]pawn: " + xxx.get_pawnname(pawn));
				pawn.UpdatePermissions();
			}
			return true;
		}
	}
}
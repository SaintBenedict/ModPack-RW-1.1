using HarmonyLib;
using RimWorld;
using Verse;

namespace rjw
{
	/// <summary>
	/// disable meditation effects for nymphs
	/// </summary>
	[HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
	internal static class PATCH_JobDriver_Meditate_MeditationTick
	{
		[HarmonyPrefix]
		private static bool on_JobDriver_Meditate(JobDriver_Meditate __instance)
		{
			Pawn pawn = __instance.pawn;
			if (xxx.is_nympho(pawn))
				return false;
			
			return true;
		}
	}

	/// <summary>
	/// disable meditation for nymphs
	/// </summary>
	[HarmonyPatch(typeof(JobGiver_Meditate), "GetPriority")]
	internal static class PATCH_JobGiver_Meditate_GetPriority
	{
		[HarmonyPostfix]
		public static void Postfix(ref float __result, Pawn pawn)
		{
			if (xxx.is_nympho(pawn))
				__result = 0f;
		}
	}
}

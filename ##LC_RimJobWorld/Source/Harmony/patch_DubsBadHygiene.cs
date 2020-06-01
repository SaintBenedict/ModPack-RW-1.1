using System;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace rjw
{
	[HarmonyPatch(typeof(JobDriver), "Cleanup")]
	internal static class PATCH_JobDriver_DubsBadHygiene
	{
		//not very good solution, some other mod can have same named jobdriver but w/e

		//Dubs Bad Hygiene washing
		private readonly static Type JobDriver_useWashBucket = AccessTools.TypeByName("JobDriver_useWashBucket");
		//private readonly static Type JobDriver_washAtCell = AccessTools.TypeByName("JobDriver_washAtCell");

		private readonly static Type JobDriver_UseHotTub = AccessTools.TypeByName("JobDriver_UseHotTub");
		private readonly static Type JobDriver_takeShower = AccessTools.TypeByName("JobDriver_takeShower");
		private readonly static Type JobDriver_takeBath = AccessTools.TypeByName("JobDriver_takeBath");

		[HarmonyPrefix]
		private static bool on_cleanup_driver(JobDriver __instance, JobCondition condition)
		{
			if (__instance == null)
				return true;

			if (condition == JobCondition.Succeeded)
			{
				Pawn pawn = __instance.pawn;

				//Log.Message("[RJW]patches_DubsBadHygiene::on_cleanup_driver" + xxx.get_pawnname(pawn));

				if (xxx.DubsBadHygieneIsActive)
					//clear one instance of semen
					if (
						__instance.GetType() == JobDriver_useWashBucket// ||
						//__instance.GetType() == JobDriver_washAtCell
						)
					{
						Hediff hediff = pawn.health.hediffSet.hediffs.Find(x => (  x.def == RJW_SemenoOverlayHediffDefOf.Hediff_Semen
																				|| x.def == RJW_SemenoOverlayHediffDefOf.Hediff_InsectSpunk
																				|| x.def == RJW_SemenoOverlayHediffDefOf.Hediff_MechaFluids
																				));
						if (hediff != null)
						{
							//Log.Message("[RJW]patches_DubsBadHygiene::" + __instance.GetType()  + " clear => " + hediff.Label);
							hediff.Severity -= 1f;
						}
					}
					//clear all instance of semen
					else if (
							__instance.GetType() == JobDriver_UseHotTub ||
							__instance.GetType() == JobDriver_takeShower ||
							__instance.GetType() == JobDriver_takeBath
							)
						{
							foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
								{
									if (hediff.def == RJW_SemenoOverlayHediffDefOf.Hediff_Semen ||
										hediff.def == RJW_SemenoOverlayHediffDefOf.Hediff_InsectSpunk ||
										hediff.def == RJW_SemenoOverlayHediffDefOf.Hediff_MechaFluids
										)
									{
										//Log.Message("[RJW]patches_DubsBadHygiene::" + __instance.GetType() + " clear => " + hediff.Label);
										hediff.Severity -= 1f;
									}
								}
						}
			}
			return true;
		}
	}
}
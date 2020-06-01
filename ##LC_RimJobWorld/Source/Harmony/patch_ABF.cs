﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Reflection;

namespace rjw
{
	//ABF?? raid rape before leaving?
	[HarmonyPatch(typeof(LordJob_AssaultColony), "CreateGraph")]
	internal static class Patches_AssaultColonyForRape
	{
		public static void Postfix(StateGraph __result)
		{
			//--Log.Message("[ABF]AssaultColonyForRape::CreateGraph");
			if (__result == null) return;
			//--Log.Message("[RJW]AssaultColonyForRape::CreateGraph");
			foreach (var trans in __result.transitions)
			{
				if (HasDesignatedTransition(trans))
				{
					foreach (Trigger t in trans.triggers)
					{
						if (t.filters == null)
						{
							t.filters = new List<TriggerFilter>() { new Trigger_SexSatisfy(0.3f) };
						}
						else
						{
							t.filters.Add(new Trigger_SexSatisfy(0.3f));
						}
					}
					//--Log.Message("[ABF]AssaultColonyForRape::CreateGraph Adding SexSatisfyTrigger to " + trans.ToString());
				}
			}
		}

		private static bool HasDesignatedTransition(Transition t)
		{
			if (t.target == null) return false;
			if (t.target.GetType() == typeof(LordToil_KidnapCover)) return true;

			foreach (Trigger ta in t.triggers)
			{
				if (ta.GetType() == typeof(Trigger_FractionColonyDamageTaken)) return true;
			}
			return false;
		}
	}

	//disable kidnaping
	[HarmonyPatch(typeof(KidnapAIUtility), "TryFindGoodKidnapVictim")]
	internal static class Patches_TryFindGoodKidnapVictim
	{
		public static void Postfix(ref bool __result)
		{
			if (!__result) return;
			//Log.Message("[RJW]KidnapAIUtility::TryFindGoodKidnapVictim");
			//Log.Message("__result " + __result);
			if (!RJWSettings.ForbidKidnap) return;
			__result = false;
		}
	}

	/*
	[HarmonyPatch(typeof(JobGiver_Manhunter), "TryGiveJob")]
	static class Patches_ABF_MunHunt
	{
		public static void Postfix(Job __result, ref Pawn pawn)
		{
			//--Log.Message("[RJW]Patches_ABF_MunHunt::Postfix called");
			if (__result == null) return;

			if (__result.def == JobDefOf.Wait || __result.def == JobDefOf.Goto) __result = null;
		}
	}
	*/

	[HarmonyPatch(typeof(SkillRecord), "CalculateTotallyDisabled")]
	internal static class Patches_SkillRecordDebug
	{
		public static bool Prefix(SkillRecord __instance,ref bool __result)
		{
			var field = __instance.GetType().GetField("pawn", BindingFlags.GetField | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
			Pawn pawn = (field.GetValue(__instance) as Pawn);
			if (__instance.def == null)
			{
				Log.Message("no def!");
				__result = false;
				return false;
			}
			return true;
		}
	}
}
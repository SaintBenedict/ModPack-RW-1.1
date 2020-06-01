using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Multiplayer.API;

namespace rjw
{
	[HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
	internal static class PATCH_Hediff_Pregnant_DoBirthSpawn
	{
		/// <summary>
		/// This one overrides vanilla pregnancy hediff behavior.
		/// 0 - try to find suitable father for debug pregnancy
		/// 1st part if character pregnant and rjw pregnancies enabled - creates rjw pregnancy and instantly births it instead of vanilla
		/// 2nd part if character pregnant with rjw pregnancy - birth it
		/// 3rd part - debug - create rjw/vanila pregnancy and birth it
		/// </summary>
		/// <param name="mother"></param>
		/// <param name="father"></param>
		/// <returns></returns>
		[HarmonyPrefix]
		[SyncMethod]
		private static bool on_begin_DoBirthSpawn(ref Pawn mother, ref Pawn father)
		{
			//--Log.Message("patches_pregnancy::PATCH_Hediff_Pregnant::DoBirthSpawn() called");

			if (mother == null)
			{
				Log.Error("Hediff_Pregnant::DoBirthSpawn() - no mother defined -> exit");
				return false;
			}

			//vanilla debug?
			if (mother.gender == Gender.Male)
			{
				Log.Error("Hediff_Pregnant::DoBirthSpawn() - mother is male -> exit");
				return false;
			}

			// get a reference to the hediff we are applying
			//do birth for vanilla pregnancy Hediff
			//if using rjw pregnancies - add RJW pregnancy Hediff and birth it instead
			Hediff_Pregnant self = (Hediff_Pregnant)mother.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Pregnant"));
			if (self != null)
			{
				return ProcessVanillaPregnancy(self, mother, father);
			}

			// do birth for existing RJW pregnancies
			if (ProcessRJWPregnancy(mother, father))
			{
				return false;
			}

			return ProcessDebugPregnancy(mother, father);
		}


		private static bool ProcessVanillaPregnancy(Hediff_Pregnant pregnancy, Pawn mother, Pawn father)
		{
			void CreateAndBirth<T>() where T : Hediff_BasePregnancy
			{
				T hediff = Hediff_BasePregnancy.Create<T>(mother, father);
				//hediff.Initialize(mother, father);
				hediff.GiveBirth();
				if (pregnancy != null)
					mother.health.RemoveHediff(pregnancy);
			}

			if (father == null)
			{
				father = Hediff_BasePregnancy.Trytogetfather(ref mother);
			}

			Log.Message("patches_pregnancy::PATCH_Hediff_Pregnant::DoBirthSpawn():Vanilla_pregnancy birthing:" + xxx.get_pawnname(mother));
			if (RJWPregnancySettings.animal_pregnancy_enabled && ((father == null || xxx.is_animal(father)) && xxx.is_animal(mother)))
			{
				//RJW Bestial pregnancy animal-animal
				Log.Message(" override as Bestial birthing(animal-animal): Father-" + xxx.get_pawnname(father) + " Mother-" + xxx.get_pawnname(mother));
				CreateAndBirth<Hediff_BestialPregnancy>();
				return false;
			}
			else if (RJWPregnancySettings.bestial_pregnancy_enabled && ((xxx.is_animal(father) && xxx.is_human(mother)) || (xxx.is_human(father) && xxx.is_animal(mother))))
			{
				//RJW Bestial pregnancy human-animal
				Log.Message(" override as Bestial birthing(human-animal): Father-" + xxx.get_pawnname(father) + " Mother-" + xxx.get_pawnname(mother));
				CreateAndBirth<Hediff_BestialPregnancy>();
				return false;
			}
			else if (RJWPregnancySettings.humanlike_pregnancy_enabled && (xxx.is_human(father) && xxx.is_human(mother)))
			{
				//RJW Humanlike pregnancy
				Log.Message(" override as Humanlike birthing: Father-" + xxx.get_pawnname(father) + " Mother-" + xxx.get_pawnname(mother));
				CreateAndBirth<Hediff_HumanlikePregnancy>();
				return false;
			}
			else
			{
				Log.Warning("Hediff_Pregnant::DoBirthSpawn() - rjw checks failed, vanilla pregnancy birth");
				Log.Warning("Hediff_Pregnant::DoBirthSpawn(): Father-" + xxx.get_pawnname(father) + " Mother-" + xxx.get_pawnname(mother));
				//vanilla pregnancy code, no effects on rjw

				return true;
			}
		}

		private static bool ProcessRJWPregnancy(Pawn mother, Pawn father)
		{
			Hediff_BasePregnancy preg =
			   (Hediff_BasePregnancy)mother.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_pregnancy")) ??       //RJW Humanlike pregnancy
			   (Hediff_BasePregnancy)mother.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_pregnancy_beast")) ?? //RJW Bestial pregnancy
			   (Hediff_BasePregnancy)mother.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_pregnancy_mech"));    //RJW Bestial pregnancy

			if (preg != null)
			{
				Log.Message($"patches_pregnancy::{preg.GetType().Name}::DoBirthSpawn() birthing:" + xxx.get_pawnname(mother));
				preg.GiveBirth();
				return true;
			}

			return false;
		}

		private static bool ProcessDebugPregnancy(Pawn mother, Pawn father)
		{
			void CreateAndBirth<T>() where T : Hediff_BasePregnancy
			{
				T hediff = Hediff_BasePregnancy.Create<T>(mother, father);
				//hediff.Initialize(mother, father);
				hediff.GiveBirth();
			}
			//CreateAndBirth<Hediff_HumanlikePregnancy>();
			//CreateAndBirth<Hediff_BestialPregnancy>();
			//CreateAndBirth<Hediff_MechanoidPregnancy>();
			//return false;

			//debug, add RJW pregnancy and birth it
			Log.Message("patches_pregnancy::PATCH_Hediff_Pregnant::DoBirthSpawn():Debug_pregnancy birthing:" + xxx.get_pawnname(mother));
			if (father == null)
			{
				father = Hediff_BasePregnancy.Trytogetfather(ref mother);

				if (RJWPregnancySettings.bestial_pregnancy_enabled && ((xxx.is_animal(father) || xxx.is_animal(mother)))
					|| (xxx.is_animal(mother) && RJWPregnancySettings.animal_pregnancy_enabled))
				{
					//RJW Bestial pregnancy
					Log.Message(" override as Bestial birthing, mother: " + xxx.get_pawnname(mother));
					CreateAndBirth<Hediff_BestialPregnancy>();
				}
				else if (RJWPregnancySettings.humanlike_pregnancy_enabled && ((father == null || xxx.is_human(father)) && xxx.is_human(mother)))
				{
					//RJW Humanlike pregnancy
					Log.Message(" override as Humanlike birthing, mother: " + xxx.get_pawnname(mother));
					CreateAndBirth<Hediff_HumanlikePregnancy>();
				}
				else
				{
					Log.Warning("Hediff_Pregnant::DoBirthSpawn() - debug vanilla pregnancy birth");
					return true;
				}
			}
			return false;
		}
	}


	[HarmonyPatch(typeof(Hediff_Pregnant), "Tick")]
	class PATCH_Hediff_Pregnant_Tick
	{
		[HarmonyPrefix]
		static bool on_begin_Tick(Hediff_Pregnant __instance)
		{
			if (__instance.pawn.IsHashIntervalTick(1000))
			{
				if (!Genital_Helper.has_vagina(__instance.pawn))
				{
					__instance.pawn.health.RemoveHediff(__instance);
				}
			}
			return true;
		}
	}
}
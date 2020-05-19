using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	/// <summary>
	/// Patch:
	/// Core Loving, Mating
	/// Rational Romance LovinCasual
	/// CnP, remove pregnancy if rjw human preg enabled
	/// </summary>

	// Add a fail condition to JobDriver_Lovin that prevents pawns from lovin' if they aren't physically able/have genitals
	[HarmonyPatch(typeof(JobDriver_Lovin), "MakeNewToils")]
	internal static class PATCH_JobDriver_Lovin_MakeNewToils
	{
		[HarmonyPrefix]
		private static bool on_begin_lovin(JobDriver_Lovin __instance)
		{
			Pawn pawn = __instance.pawn;
			Pawn partner = null;
			Building_Bed Bed = null;
			var any_ins = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			partner = (Pawn)(__instance.GetType().GetProperty("Partner", any_ins).GetValue(__instance, null));
			Bed = (Building_Bed)(__instance.GetType().GetProperty("Bed", any_ins).GetValue(__instance, null));

			__instance.FailOn(() => (!(xxx.can_fuck(pawn) || xxx.can_be_fucked(pawn))));
			__instance.FailOn(() => (!(xxx.can_fuck(partner) || xxx.can_be_fucked(partner))));

			if (RJWSettings.override_lovin)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				Job job = JobMaker.MakeJob(xxx.casual_sex, partner, Bed);
				pawn.jobs.jobQueue.EnqueueFirst(job);
			}

			return true;
		}
	}

	// Add a fail condition to JobDriver_Mate that prevents animals from lovin' if they aren't physically able/have genitals
	[HarmonyPatch(typeof(JobDriver_Mate), "MakeNewToils")]
	internal static class PATCH_JobDriver_Mate_MakeNewToils
	{
		[HarmonyPrefix]
		private static bool on_begin_matin(JobDriver_Mate __instance)
		{
			//only reproductive male starts mating job
			Pawn pawn = __instance.pawn;
			Pawn partner = null;
			var any_ins = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			partner = (Pawn)(__instance.GetType().GetProperty("Female", any_ins).GetValue(__instance, null));

			__instance.FailOn(() => (!(xxx.can_fuck(pawn))));
			__instance.FailOn(() => (!(xxx.can_be_fucked(partner))));

			if (RJWSettings.override_matin)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				Job job = JobMaker.MakeJob(xxx.animalBreed, partner);
				pawn.jobs.jobQueue.EnqueueFirst(job);
			}

			return true;
		}
	}

	//// Add a fail condition to JobDriver_Mate that prevents animals from lovin' if they aren't physically able/have genitals
	//[HarmonyPatch(typeof(PawnUtility), "FertileMateTarget")]
	//internal static class PATCH_JobDriver_Mate_MakeNewToils
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(Job __result, Pawn pawn)
	//	{
	//		if (__result == null)
	//		{
	//			if (female.gender != Gender.Female || !female.ageTracker.CurLifeStage.reproductive)
	//			{
	//				return false;
	//			}
	//			CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
	//			if (compEggLayer != null)
	//			{
	//				return !compEggLayer.FullyFertilized;
	//			}
	//			return !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant, false);

	//			__result = JobMaker.MakeJob(JobDefOf.Mate, pawn2);

	//		}
	//	}
	//}

	//Patch for futa animals to initiate mating (vanialla - only male)
	[HarmonyPatch(typeof(JobGiver_Mate), "TryGiveJob")]
	internal static class PATCH_JobGiver_Mate_TryGiveJob
	{
		[HarmonyPostfix]
		public static void Postfix(ref Job __result, Pawn pawn)
		{
			//Log.Message("[RJW]patches_lovin::JobGiver_Mate fail, try rjw for " + xxx.get_pawnname(pawn));
			var partBPR = Genital_Helper.get_genitalsBPR(pawn);
			var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);
			if (!(Genital_Helper.has_penis_fertile(pawn, parts) || Genital_Helper.has_penis_infertile(pawn, parts)) || !pawn.ageTracker.CurLifeStage.reproductive)
			{
				//Log.Message("[RJW]patches_lovin::JobGiver_Mate " + xxx.get_pawnname(pawn) + ", has no penis " + (Genital_Helper.has_penis(pawn) || Genital_Helper.has_penis_infertile(pawn)));
				//Log.Message("[RJW]patches_lovin::JobGiver_Mate " + xxx.get_pawnname(pawn) + ", not reproductive " + !pawn.ageTracker.CurLifeStage.reproductive);
				return;
			}
			Predicate<Thing> validator = delegate (Thing t)
			{
				Pawn pawn3 = t as Pawn;
				var valid = !pawn3.Downed
							&& pawn3 != pawn
							&& pawn3.CanCasuallyInteractNow()
							&& !pawn3.IsForbidden(pawn)
							&& !pawn3.HostileTo(pawn)
							&& PawnUtility.FertileMateTarget(pawn, pawn3);
				if (!valid && pawn3 != pawn)
				{
					//Log.Message("[RJW]patches_lovin::JobGiver_Mate " + xxx.get_pawnname(pawn3) + ", not valid");
					//Log.Message("[RJW]patches_lovin::JobGiver_Mate Downed " + pawn3.Downed);
					//Log.Message("[RJW]patches_lovin::JobGiver_Mate CanCasuallyInteractNow " + pawn3.CanCasuallyInteractNow());
					//Log.Message("[RJW]patches_lovin::JobGiver_Mate IsForbidden " + pawn3.IsForbidden(pawn));
					//Log.Message("[RJW]patches_lovin::JobGiver_Mate FertileMateTarget " + PawnUtility.FertileMateTarget(pawn, pawn3));
				}
				return valid;
			};

			ThingRequest request = ThingRequest.ForDef(pawn.def);			// mate sames species
			if (RJWSettings.WildMode)										// go wild xD
				request = ThingRequest.ForGroup(ThingRequestGroup.Pawn);	// mate everyone
			//add animal check?

			Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.Touch, TraverseParms.For(pawn), 30f, validator);
			if (pawn2 == null)
			{
				//Log.Message("[RJW]patches_lovin::JobGiver_Mate " + xxx.get_pawnname(pawn) + ", no valid partner found");
				return;
			}
			//__result = JobMaker.MakeJob(xxx.animalBreed, pawn2);
			__result = JobMaker.MakeJob(JobDefOf.Mate, pawn2);
			//Log.Message("[RJW]patches_lovin::JobGiver_Mate " + xxx.get_pawnname(pawn) + ", female " + xxx.get_pawnname(pawn2) + ", job " + __result);
		}
	}

	//check if female can be impregnated for animal mating (also check rjw pregnancies)
	[HarmonyPatch(typeof(PawnUtility), "FertileMateTarget")]
	internal static class PATCH_PawnUtility_FertileMateTarget
	{
		[HarmonyPrefix]
		public static bool Prefix(Pawn male, Pawn female, ref bool __state)
		{
			__state = female.IsPregnant();
			//Log.Message("Prefix FertileMateTarget is vanilla/rjw pregnant: " + __state);
			return true;
		}

		[HarmonyPostfix]
		public static bool Postfix(bool __result, ref bool __state)
		{
			if (__result)
			{
				//Log.Message("Postfix FertileMateTarget is fertile and not vanilla pregnant: " + __result);
				if (__state)
				{
					//Log.Message("Postfix FertileMateTarget is fertile and rjw pregnant: " + __state);
					__result = false;
				}
			}
			return __result;
		}
	}

	//Suboptions when starting rjw sex through workgiver
	//[HarmonyPatch(typeof(FloatMenuOption), "Chosen")]
	//internal static class PATCH_test
	//{
	//	[HarmonyPostfix]
	//	public static void Postfix(FloatMenuOption __instance)
	//	{
	//		try
	//		{
	//			if (Find.Selector.NumSelected == 1) //&& (Find.Selector.SingleSelectedThing as Pawn).IsDesignatedHero()
	//			{
	//				//TODO: check if rjw jobdriver,
	//				//Insert another menu or 2 that will modify initiator sex jobdriver to 
	//				//use selected parts - oral/anal/vaginal/etc
	//				//use selectable sex action - give/recieve?
	//				//use selectable sex action - fuck/fisting?
	//				Log.Message("---");
	//				Log.Message("[RJW]FloatMenuOption::Chosen " + __instance);
	//				Log.Message("[RJW]FloatMenuOption::Chosen " + __instance.Label);
	//				Log.Message("[RJW]FloatMenuOption::Chosen " + __instance.action.Target);
	//				Log.Message("[RJW]FloatMenuOption::Chosen initiator - " + Find.Selector.SingleSelectedThing);
	//				Log.Message("[RJW]FloatMenuOption::Chosen target - " + __instance.revalidateClickTarget);
	//				Log.Message("[RJW]FloatMenuOption::Chosen initiator - " + (Find.Selector.SingleSelectedThing as Pawn).CurJob);
	//				Log.Message("---");
	//			}
	//		}
	//		catch
	//		{ }
	//	}
	//}



	//JobDriver_DoLovinCasual from RomanceDiversified should have handled whether pawns can do casual lovin,
	//so I don't bothered to do a check here, unless some bugs occur due to this.
	//this prob needs a patch like above, but who cares

	// Call xxx.aftersex after pawns have finished lovin'
	// You might be thinking, "wouldn't it be easier to add this code as a finish condition to JobDriver_Lovin in the patch above?" I tried that
	// at first but it didn't work because the finish condition is always called regardless of how the job ends (i.e. if it's interrupted or not)
	// and there's no way to find out from within the finish condition how the job ended. I want to make sure not apply the effects of sex if the
	// job was interrupted somehow.
	[HarmonyPatch(typeof(JobDriver), "Cleanup")]
	internal static class PATCH_JobDriver_Loving_Cleanup
	{
		//RomanceDiversified lovin
		//not very good solution, some other mod can have same named jobdrivers, but w/e
		private readonly static Type JobDriverDoLovinCasual = AccessTools.TypeByName("JobDriver_DoLovinCasual");

		//vanilla lovin
		private readonly static Type JobDriverLovin = typeof(JobDriver_Lovin);

		//vanilla mate
		private readonly static Type JobDriverMate = typeof(JobDriver_Mate);

		[HarmonyPrefix]
		private static bool on_cleanup_driver(JobDriver __instance, JobCondition condition)
		{
			if (__instance == null)
				return true;

			if (condition == JobCondition.Succeeded)
			{
				Pawn pawn = __instance.pawn;
				Pawn partner = null;

				//Log.Message("[RJW]patches_lovin::on_cleanup_driver" + xxx.get_pawnname(pawn));

				//[RF] Rational Romance [1.0] loving
				if (xxx.RomanceDiversifiedIsActive && __instance.GetType() == JobDriverDoLovinCasual)
				{
					// not sure RR can even cause pregnancies but w/e
					var any_ins = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					partner = (Pawn)(__instance.GetType().GetProperty("Partner", any_ins).GetValue(__instance, null));
					Log.Message("[RJW]patches_lovin::on_cleanup_driver RomanceDiversified/RationalRomance:" + xxx.get_pawnname(pawn) + "+" + xxx.get_pawnname(partner));
				}
				//Vanilla loving
				else if (__instance.GetType() == JobDriverLovin)
				{
					var any_ins = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					partner = (Pawn)(__instance.GetType().GetProperty("Partner", any_ins).GetValue(__instance, null));
				//CnP loving
					if (xxx.RimWorldChildrenIsActive && RJWPregnancySettings.humanlike_pregnancy_enabled && xxx.is_human(pawn) && xxx.is_human(partner))
					{
						Log.Message("[RJW]patches_lovin:: RimWorldChildren/ChildrenAndPregnancy pregnancy:" + xxx.get_pawnname(pawn) + "+" + xxx.get_pawnname(partner));
						PregnancyHelper.cleanup_CnP(pawn);
						PregnancyHelper.cleanup_CnP(partner);
					}
					else
						Log.Message("[RJW]patches_lovin:: JobDriverLovin pregnancy:" + xxx.get_pawnname(pawn) + "+" + xxx.get_pawnname(partner));
				}
				//Vanilla mating
				else if (__instance.GetType() == JobDriverMate)
				{
					var any_ins = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					partner = (Pawn)(__instance.GetType().GetProperty("Female", any_ins).GetValue(__instance, null));
					Log.Message("[RJW]patches_lovin:: JobDriverMate pregnancy:" + xxx.get_pawnname(pawn) + "+" + xxx.get_pawnname(partner));
				}
				else
					return true;

				// TODO: Doing TryUseCondom here is a bit weird... it should happen before.
				var usedCondom = CondomUtility.TryUseCondom(pawn) || CondomUtility.TryUseCondom(partner);

				//vanilla will probably be fucked up for non humanlikes... but only humanlikes do loving, right?
				//if rjw pregnancy enabled, remove vanilla for:
				//human-human
				//animal-animal
				//bestiality
				//always remove when someone is insect or mech
				if (RJWPregnancySettings.humanlike_pregnancy_enabled && xxx.is_human(pawn) && xxx.is_human(partner)
					|| RJWPregnancySettings.animal_pregnancy_enabled && xxx.is_animal(pawn) && xxx.is_animal(partner)
					|| (RJWPregnancySettings.bestial_pregnancy_enabled && xxx.is_human(pawn) && xxx.is_animal(partner)
					|| RJWPregnancySettings.bestial_pregnancy_enabled && xxx.is_animal(pawn) && xxx.is_human(partner))
					|| xxx.is_insect(pawn) || xxx.is_insect(partner) || xxx.is_mechanoid(pawn) || xxx.is_mechanoid(partner)
					)
				{
					Log.Message("[RJW]patches_lovin::on_cleanup_driver vanilla pregnancy:" + xxx.get_pawnname(pawn) + "+" + xxx.get_pawnname(partner));
					PregnancyHelper.cleanup_vanilla(pawn);
					PregnancyHelper.cleanup_vanilla(partner);
				}

				SexUtility.ProcessSex(pawn, partner, usedCondom, false, true);
			}
			return true;
		}
	}
}

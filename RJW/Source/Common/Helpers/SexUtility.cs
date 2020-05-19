using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using JobDriver_Lovin = rjw_CORE_EXPOSED.JobDriver_Lovin;
using HarmonyLib;
using Multiplayer.API;


namespace rjw
{
	public class SexUtility
	{
		private const float no_partner_ability = 0.8f;
		private const float base_sat_per_fuck = 0.40f;
		private const float base_sat_per_quirk = 0.20f;

		//Anal sex
		private static readonly InteractionDef analSex = DefDatabase<InteractionDef>.GetNamed("AnalSex");
		//Vaginal sex
		private static readonly InteractionDef vaginalSex = DefDatabase<InteractionDef>.GetNamed("VaginalSex");
		//Oral sex
		private static readonly InteractionDef rimming = DefDatabase<InteractionDef>.GetNamed("Rimming");
		private static readonly InteractionDef cunnilingus = DefDatabase<InteractionDef>.GetNamed("Cunnilingus");
		private static readonly InteractionDef fellatio = DefDatabase<InteractionDef>.GetNamed("Fellatio");
		private static readonly InteractionDef sixtynine = DefDatabase<InteractionDef>.GetNamed("Sixtynine");
		//Other sex types
		private static readonly InteractionDef doublePenetration = DefDatabase<InteractionDef>.GetNamed("DoublePenetration");
		private static readonly InteractionDef breastjob = DefDatabase<InteractionDef>.GetNamed("Breastjob");
		private static readonly InteractionDef handjob = DefDatabase<InteractionDef>.GetNamed("Handjob");
		private static readonly InteractionDef footjob = DefDatabase<InteractionDef>.GetNamed("Footjob");
		private static readonly InteractionDef beakjob = DefDatabase<InteractionDef>.GetNamed("Beakjob");
		private static readonly InteractionDef fingering = DefDatabase<InteractionDef>.GetNamed("Fingering");
		private static readonly InteractionDef scissoring = DefDatabase<InteractionDef>.GetNamed("Scissoring");
		private static readonly InteractionDef mutualMasturbation = DefDatabase<InteractionDef>.GetNamed("MutualMasturbation");
		private static readonly InteractionDef fisting = DefDatabase<InteractionDef>.GetNamed("Fisting");
		//Rape types
		private static readonly InteractionDef analRape = DefDatabase<InteractionDef>.GetNamed("AnalRape");
		private static readonly InteractionDef vaginalRape = DefDatabase<InteractionDef>.GetNamed("VaginalRape");
		private static readonly InteractionDef otherRape = DefDatabase<InteractionDef>.GetNamed("OtherRape");
		private static readonly InteractionDef handRape = DefDatabase<InteractionDef>.GetNamed("HandjobRape");
		private static readonly InteractionDef fingeringRape = DefDatabase<InteractionDef>.GetNamed("FingeringRape");
		private static readonly InteractionDef violateCorpse = DefDatabase<InteractionDef>.GetNamed("ViolateCorpse");
		private static readonly InteractionDef mechImplant = DefDatabase<InteractionDef>.GetNamed("MechImplant");
		//Breeding
		private static readonly InteractionDef vaginalBreeding = DefDatabase<InteractionDef>.GetNamed("VaginalBreeding");
		private static readonly InteractionDef analBreeding = DefDatabase<InteractionDef>.GetNamed("AnalBreeding");
		private static readonly InteractionDef oralBreeding = DefDatabase<InteractionDef>.GetNamed("OralBreeding");
		private static readonly InteractionDef forcedOralBreeding = DefDatabase<InteractionDef>.GetNamed("ForcedOralBreeding");
		private static readonly InteractionDef forcedFellatioBreeding = DefDatabase<InteractionDef>.GetNamed("ForcedFellatioBreeding");
		private static readonly InteractionDef fingeringBreeding = DefDatabase<InteractionDef>.GetNamed("FingeringBreeding");
		private static readonly InteractionDef requestBreeding = DefDatabase<InteractionDef>.GetNamed("RequestBreeding");
		private static readonly InteractionDef requestAnalBreeding = DefDatabase<InteractionDef>.GetNamed("RequestAnalBreeding");

		public static readonly InteractionDef AnimalSexChat = DefDatabase<InteractionDef>.GetNamed("AnimalSexChat");

		private static readonly ThingDef cum = ThingDef.Named("FilthCum");
		private static readonly ThingDef girlcum = ThingDef.Named("FilthGirlCum");

		private static readonly Dictionary<InteractionDef, xxx.rjwSextype> sexActs = new Dictionary<InteractionDef, xxx.rjwSextype>
		{
			{analSex, xxx.rjwSextype.Anal },
			{analRape, xxx.rjwSextype.Anal },
			{vaginalSex, xxx.rjwSextype.Vaginal },
			{vaginalRape, xxx.rjwSextype.Vaginal },
			{fellatio, xxx.rjwSextype.Oral },
			{rimming, xxx.rjwSextype.Oral },
			{cunnilingus, xxx.rjwSextype.Oral },
			{sixtynine, xxx.rjwSextype.Oral },
			{handjob, xxx.rjwSextype.Handjob },
			{handRape, xxx.rjwSextype.Handjob },
			{breastjob, xxx.rjwSextype.Boobjob },
			{doublePenetration, xxx.rjwSextype.DoublePenetration },
			{footjob, xxx.rjwSextype.Footjob },
			{fingering, xxx.rjwSextype.Fingering },
			{fingeringRape, xxx.rjwSextype.Fingering },
			{fingeringBreeding, xxx.rjwSextype.Fingering },
			{scissoring, xxx.rjwSextype.Scissoring },
			{mutualMasturbation, xxx.rjwSextype.MutualMasturbation },
			{fisting, xxx.rjwSextype.Fisting },
			{vaginalBreeding, xxx.rjwSextype.Vaginal },
			{analBreeding, xxx.rjwSextype.Anal },
			{oralBreeding, xxx.rjwSextype.Oral },
			{forcedOralBreeding, xxx.rjwSextype.Oral },
			{forcedFellatioBreeding, xxx.rjwSextype.Oral },
			{requestBreeding, xxx.rjwSextype.Vaginal },
			{requestAnalBreeding, xxx.rjwSextype.Anal },
			{otherRape, xxx.rjwSextype.Oral },
			{mechImplant, xxx.rjwSextype.MechImplant },
			{violateCorpse, xxx.rjwSextype.None } // Sextype as none, since this cannot result in pregnancy etc.
		};

		// This and the following array are used to generate text from interactions.
		private static readonly InteractionDef[,] dictionarykeys =
		{
			// 1: Interactions_Sex (consensual), 2: Interactions_Rape (forced), 3: Interactions_Breed (animals)
			{vaginalSex, vaginalRape, vaginalBreeding },			// Vaginal
			{vaginalSex, vaginalRape, requestBreeding },			// Vaginal - reseiving
			{analSex, analRape, analBreeding },						// Anal
			{analSex, analRape, requestAnalBreeding },				// Anal - reseiving
			{cunnilingus, otherRape, oralBreeding },				// Cunnilingus
			{cunnilingus, otherRape, forcedOralBreeding },			// Cunnilingus - receiving
			{rimming, otherRape, oralBreeding },					// Rimming
			{rimming, otherRape, forcedOralBreeding },				// Rimming - receiving
			{fellatio, otherRape, oralBreeding },					// Fellatio
			{fellatio, otherRape, forcedOralBreeding },				// Fellatio - receiving
			{doublePenetration, vaginalRape, vaginalBreeding },		// Double penetration
			{doublePenetration, vaginalRape, vaginalBreeding },		// Double penetration - receiving
			{breastjob, otherRape, breastjob },						// Breastjob
			{breastjob, otherRape, breastjob },						// Breastjob - receiving
			{handjob, handRape, handjob },							// Handjob
			{handjob, handRape, handjob },							// Handjob - receiving
			{footjob, otherRape, footjob },							// Footjob
			{footjob, otherRape, footjob },							// Footjob - receiving
			{fingering, fingeringRape, fingeringBreeding },			// Fingering
			{fingering, otherRape, fingeringBreeding },				// Fingering - receiving
			{scissoring, otherRape, scissoring },					// Scissoring
			{scissoring, otherRape, scissoring },					// Scissoring - receiving
			{mutualMasturbation, otherRape, mutualMasturbation },	// Mutual Masturbation
			{mutualMasturbation, otherRape, mutualMasturbation },	// Mutual Masturbation - receiving
			{fisting, analRape, fisting },							// Fisting
			{fisting, analRape, fisting },							// Fisting - receiving
			{sixtynine, otherRape, sixtynine },						// 69
			{sixtynine, otherRape, sixtynine },						// 69 - receiving
		};

		private static readonly string[,] rulepacks =
		{   
			// Rulepacks for interaction text output.
			// 1: RulePacks_Sex (consensual), 2: RulePacks_Rape (forced), 3: RulePacks_Breeding (animals)
			{"VaginalSexRP", "VaginalRapeRP", "VaginalBreedingRP"},			// Vaginal
			{"VaginalSexRP", "VaginalDomRP", "VaginalBreedingRP"},			// Vaginal - reseiving
			{"AnalSexRP", "AnalRapeRP", "AnalBreedingRP"},					// Anal
			{"AnalSexRP", "AnalDomRP", "AnalBreedingRP"},					// Anal - reseiving
			{"CunnilingusRP", "VaginalRapeRP", "CunnilingusRP"},			// Cunnilingus
			{"CunnilingusRP", "ForcedCunnilingusRP", "CunnilingusRP"},		// Cunnilingus - receiving
			{"RimmingRP", "VaginalRapeRP", "RimmingRP"},					// Rimming
			{"RimmingRP", "ForcedRimmingRP", "RimmingRP"},					// Rimming - receiving
			{"FellatioRP", "FellatioRapeRP", "FellatioRP"},					// Fellatio
			{"FellatioRP", "ForcedFellatioRP", "FellatioRP"},				// Fellatio - receiving
			{"DoublePenetrationRP", "DoubleRapeRP", "VaginalBreedingRP"},	// Double penetration
			{"DoublePenetrationRP", "OtherRapeRP", "VaginalBreedingRP"},	// Double penetration - receiving
			{"BreastjobRP", "MaleRapeRP", "BreastjobRP"},					// Breastjob
			{"BreastjobRP", "ForcedBreastjobRP", "BreastjobRP"},			// Breastjob - receiving
			{"HandjobRP", "HandjobRapeRP", "HandjobRP"},					// Handjob
			{"HandjobRP", "ForcedHandjobRP", "HandjobRP"},					// Handjob - receiving
			{"FootjobRP", "FootjobRapeRP", "FootjobRP"},					// Footjob
			{"FootjobRP", "ForcedFootjobRP", "FootjobRP"},					// Footjob - receiving
			{"FingeringRP", "FingeringRapeRP", "FingeringRP"},				// Fingering
			{"FingeringRP", "ForcedFingeringRP", "FingeringRP"},			// Fingering - receiving
			{"ScissoringRP", "ScissoringRapeRP", "ScissoringRP"},			// Scissoring
			{"ScissoringRP", "ForcedScissoringRP", "ScissoringRP"},			// Scissoring - receiving
			{"MutualMasturbationRP", null, "MutualMasturbationRP"},			// Mutual Masturbation
			{"MutualMasturbationRP", null, "MutualMasturbationRP"},			// Mutual Masturbation - receiving
			{"FistingRP", "FistingRapeRP", "FistingRP"},					// Fisting
			{"FistingRP", null, "FistingRP"},								// Fisting - receiving
			{"SixtynineRP", "ForcedSixtynineRP", "SixtynineRP"},			// 69
			{"SixtynineRP", null, "SixtynineRP"}							// 69 - receiving
		};

		// Alert checker that is called from several jobs. Checks the pawn relation, and whether it should sound alert.
		// notification in top left corner
		// rape attempt
		public static void RapeTargetAlert(Pawn rapist, Pawn target)
		{
			if (target.IsDesignatedComfort() && rapist.jobs.curDriver.GetType() == typeof(JobDriver_RapeComfortPawn))
				if (!RJWPreferenceSettings.ShowForCP)
					return;
			if (target.IsDesignatedComfort() && rapist.jobs.curDriver.GetType() == typeof(JobDriver_Breeding))
				if (target.IsDesignatedBreeding())
					if (!RJWPreferenceSettings.ShowForBreeding)
						return;

			bool silent = false;
			PawnRelationDef relation = rapist.GetMostImportantRelation(target);
			string rapeverb = "rape";

			if (xxx.is_mechanoid(rapist)) rapeverb = "assault";
			else if (xxx.is_animal(rapist) || xxx.is_animal(target)) rapeverb = "breed";

			// TODO: Need to write a cherker method for family relations. Would be useful for other things than just this, such as incest settings/quirk.

			string message = (xxx.get_pawnname(rapist) + " is trying to " + rapeverb + " " + xxx.get_pawnname(target));
			message += relation == null ? "." : (", " + rapist.Possessive() + " " + relation.GetGenderSpecificLabel(target) + ".");

			switch (RJWPreferenceSettings.rape_attempt_alert)
			{
				case RJWPreferenceSettings.RapeAlert.Enabled:
					break;
				case RJWPreferenceSettings.RapeAlert.Humanlikes:
					if (!xxx.is_human(target))
						return;
					break;
				case RJWPreferenceSettings.RapeAlert.Colonists:
					if (!target.IsColonist)
						return;
					break;
				case RJWPreferenceSettings.RapeAlert.Silent:
					silent = true;
					break;
				default:
					return;
			}

			if (!silent)
			{
				Messages.Message(message, rapist, MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				Messages.Message(message, rapist, MessageTypeDefOf.SilentInput);
			}
		}

		// Alert checker that is called from several jobs.
		// notification in top left corner
		// rape started
		public static void BeeingRapedAlert(Pawn rapist, Pawn target)
		{
			if (target.IsDesignatedComfort() && rapist.jobs.curDriver.GetType() == typeof(JobDriver_RapeComfortPawn))
				if (!RJWPreferenceSettings.ShowForCP)
					return;
			if (target.IsDesignatedComfort() && rapist.jobs.curDriver.GetType() == typeof(JobDriver_Breeding))
				if (target.IsDesignatedBreeding())
					if (!RJWPreferenceSettings.ShowForBreeding)
						return;

			bool silent = false;

			switch (RJWPreferenceSettings.rape_alert)
			{
				case RJWPreferenceSettings.RapeAlert.Enabled:
					break;
				case RJWPreferenceSettings.RapeAlert.Humanlikes:
					if (!xxx.is_human(target))
						return;
					break;
				case RJWPreferenceSettings.RapeAlert.Colonists:
					if (!target.IsColonist)
						return;
					break;
				case RJWPreferenceSettings.RapeAlert.Silent:
					silent = true;
					break;
				default:
					return;
			}

			if (!silent)
			{
				Messages.Message(xxx.get_pawnname(target) + " is getting raped.", target, MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				Messages.Message(xxx.get_pawnname(target) + " is getting raped.", target, MessageTypeDefOf.SilentInput);
			}
		}

		// Quick method that return a body part by name. Used for checking if a pawn has a specific body part, etc.
		public static BodyPartRecord GetPawnBodyPart(Pawn pawn, string bodyPart)
		{
			return pawn.RaceProps.body.AllParts.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed(bodyPart));
		}

		public static void CumFilthGenerator(Pawn pawn)
		{
			if (pawn.Dead) return;
			if (xxx.is_slime(pawn)) return;
			if (!RJWSettings.cum_filth) return;

			// Larger creatures, larger messes.
			float pawn_cum = Math.Min(80 / ScaleToHumanAge(pawn), 2.0f) * pawn.BodySize;

			// Increased output if the pawn has the Messy quirk.
			if (xxx.has_quirk(pawn, "Messy"))
				pawn_cum *= 2.0f;

			var partBPR = Genital_Helper.get_genitalsBPR(pawn);
			var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

			if (Genital_Helper.has_vagina(pawn, parts))
				FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, girlcum, pawn.LabelIndefinite(), (int)Math.Max(pawn_cum/2, 1.0f));

			if (Genital_Helper.has_penis_fertile(pawn, parts))
				FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, cum, pawn.LabelIndefinite(), (int)Math.Max(pawn_cum, 1.0f));
		}

		// The pawn may or may not clean up the mess after fapping.
		[SyncMethod]
		public static bool ConsiderCleaning(Pawn fapper)
		{
			if (!RJWSettings.cum_filth) return false;
			if (!xxx.has_traits(fapper) || fapper.story == null) return false;
			if (fapper.WorkTagIsDisabled(WorkTags.Cleaning)) return false;

			float do_cleaning = 0.5f; // 50%

			if (!fapper.PositionHeld.Roofed(fapper.Map))
				do_cleaning -= 0.25f; // Less likely to clean if outdoors.

			if (xxx.CTIsActive && fapper.story.traits.HasTrait(TraitDef.Named("RCT_NeatFreak")))
				do_cleaning += 1.00f;

			if (xxx.has_quirk(fapper, "Messy"))
				do_cleaning -= 0.75f;

			switch (fapper.needs.rest.CurCategory)
			{
				case RestCategory.Exhausted:
					do_cleaning -= 0.5f;
					break;
				case RestCategory.VeryTired:
					do_cleaning -= 0.3f;
					break;
				case RestCategory.Tired:
					do_cleaning -= 0.1f;
					break;
				case RestCategory.Rested:
					do_cleaning += 0.3f;
					break;
			}

			if (fapper.story.traits.DegreeOfTrait(TraitDefOf.NaturalMood) == -2) // Depressive
				do_cleaning -= 0.3f;
			if (fapper.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == 2) // Industrious
				do_cleaning += 1.0f;
			else if (fapper.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == 1) // Hard worker
				do_cleaning += 0.5f;
			else if (fapper.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == -1) // Lazy
				do_cleaning -= 0.5f;
			else if (fapper.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == -2) // Slothful
				do_cleaning -= 1.0f;

			if (xxx.is_ascetic(fapper))
				do_cleaning += 0.2f;

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			return Rand.Chance(do_cleaning);
		}

		// <summary>Handles after-sex trait and thought gain, and fluid creation. Initiator of the act (whore, rapist, female zoophile, etc) should be first.</summary>
		[SyncMethod]
		public static void Aftersex(Pawn pawn, Pawn partner, bool usedCondom = false, bool rape = false, bool isCoreLovin = false, xxx.rjwSextype sextype = xxx.rjwSextype.Vaginal)
		{
			bool bothInMap = false;

			if (!partner.Dead)
				bothInMap = pawn.Map != null && partner.Map != null; //Added by Hoge. false when called this function for despawned pawn: using for background rape like a kidnappee

			//Catch-all timer increase, for ensuring that pawns don't get stuck repeating jobs.

			pawn.rotationTracker.Face(partner.DrawPos);
			pawn.rotationTracker.FaceCell(partner.Position);

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (bothInMap)
			{
				if (!partner.Dead)
				{
					partner.rotationTracker.Face(pawn.DrawPos);
					partner.rotationTracker.FaceCell(pawn.Position);

					if (RJWSettings.sounds_enabled)
					{
						if (rape)
						{
							if (Rand.Value > 0.30f)
								LifeStageUtility.PlayNearestLifestageSound(partner, (ls) => ls.soundAngry, 1.2f);
							else
								LifeStageUtility.PlayNearestLifestageSound(partner, (ls) => ls.soundCall, 1.2f);

							pawn.Drawer.Notify_MeleeAttackOn(partner);
							partner.stances.StaggerFor(Rand.Range(10, 300));
						}
						else
							LifeStageUtility.PlayNearestLifestageSound(partner, (ls) => ls.soundCall);
					}
					if (sextype == xxx.rjwSextype.Vaginal || sextype == xxx.rjwSextype.DoublePenetration)
						if (xxx.is_Virgin(partner))
						{
							//TODO: bind virginity to parts of pawn
							/*
							string thingdef_penis_name = Genital_Helper.get_penis_all(pawn)?.def.defName ?? "";
							ThingDef thingdef_penis = null;

							Log.Message("SexUtility::thingdef_penis_name " + thingdef_penis_name);
							Log.Message("SexUtility::thingdef_penis 1 " + thingdef_penis);

							if (thingdef_penis_name != "")
								thingdef_penis = (from x in DefDatabase<ThingDef>.AllDefs where x.defName == thingdef_penis_name select x).RandomElement();
							Log.Message("SexUtility::thingdef_penis 2 " + thingdef_penis);

							partner.TakeDamage(new DamageInfo(DamageDefOf.Stab, 1, 999, -1.0f, null, xxx.genitals, thingdef_penis));
							*/
						}
				}

				if (RJWSettings.sounds_enabled)
					SoundDef.Named("Cum").PlayOneShot(!partner.Dead
						? new TargetInfo(partner.Position, pawn.Map)
						: new TargetInfo(pawn.Position, pawn.Map));

				if (rape)
				{
					if (Rand.Value > 0.30f)
						LifeStageUtility.PlayNearestLifestageSound(pawn, (ls) => ls.soundAngry, 1.2f);
					else
						LifeStageUtility.PlayNearestLifestageSound(pawn, (ls) => ls.soundCall, 1.2f);
				}
				else
					LifeStageUtility.PlayNearestLifestageSound(pawn, (ls) => ls.soundCall);
			}

			if (!usedCondom)
			{
				CumFilthGenerator(pawn);

				if (bothInMap && !isCoreLovin)
					CumFilthGenerator(partner);

				//apply cum to body:
				SemenHelper.calculateAndApplySemen(pawn, partner, sextype);

				//--Log.Message("SexUtility::aftersex( " + pawn_name + ", " + partner_name + " ) - checking satisfaction");

				if (!pawn.Dead && !partner.Dead)
				{
					PregnancyHelper.impregnate(pawn, partner, sextype);
					//The dead have no hediff, so no need to roll_to_catch; TO DO: add a roll_to_catch_from_corpse to std
					//--Log.Message("SexUtility::aftersex( " + pawn_name + ", " + partner_name + " ) - checking disease");
					if (!(xxx.is_animal(pawn) || xxx.is_animal(partner)))
						std_spreader.roll_to_catch(pawn, partner);
				}
			}
			else
			{
				var newCondom = GenSpawn.Spawn(ThingDef.Named("UsedCondom"), pawn.Position, pawn.Map);
				CondomUtility.useCondom(pawn);
				CondomUtility.useCondom(partner);
			}

			//TODO: below is fucked up, unfuck it someday
			xxx.UpdateRecords(pawn, partner, sextype, rape, isCoreLovin);
			Satisfy(pawn, partner, sextype, rape);

			CheckTraitGain(pawn, partner, rape);
			CheckTraitGain(partner, pawn, rape);
		}

		// <summary>Solo acts.</summary>
		public static void Aftersex(Pawn pawn, xxx.rjwSextype sextype = xxx.rjwSextype.Masturbation)
		{
			IncreaseTicksToNextLovin(pawn);

			//if (Mod_Settings.sounds_enabled)
			//{
			//	SoundDef.Named("Cum").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
			//}

			CumFilthGenerator(pawn);

			//apply cum to body:
			SemenHelper.calculateAndApplySemen(pawn, null, sextype);

			//--Log.Message("SexUtility::aftersex( " + pawn_name + ", " + partner_name + " ) - checking satisfaction");
			xxx.UpdateRecords(pawn, null, sextype);
			Satisfy(pawn, null, sextype);

			// No traits from solo. Enable if some are edded. (Voyerism?)
			//check_trait_gain(pawn);
		}

		private static void Satisfy(Pawn pawn, Pawn partner, xxx.rjwSextype sextype, bool rape = false)
		{
			//--Log.Message("xxx::satisfy( " + pawn_name + ", " + partner_name + ", " + violent + "," + isCoreLovin + " ) called");

			if (pawn == null && partner == null) return; // Unlikely, but checking this here makes other checks simpler.

			float pawn_ability = pawn != null ? Math.Max(xxx.get_sex_ability(pawn), 0.3f) : no_partner_ability;
			float partner_ability = (partner != null && !partner.Dead) ? Math.Max(xxx.get_sex_ability(partner), 0.3f) : no_partner_ability;
			/*
			if(sextype == xxx.rjwSextype.Anal)
			{
				pawn_ability = pawn != null ? pawn.health.hediffSet.GetFirstHediffOfDef(Genital_Helper.average_anus).CurStage.statOffsets.FindLast(DefDatabase<StatDef>.GetNamed("SexAbility")).value : pawn_ability;
				partner_ability = (partner != null && !partner.Dead) ? Math.Max(xxx.get_sex_ability(partner), 0.3f) : partner_ability;
			}
			*/

			//--Log.Message("xxx::satisfy( " + pawn_name + ", " + partner_name + ", " + violent + "," + isCoreLovin + " ) - calculate base satisfaction");
			// Base satisfaction is based on partner's ability
			float pawn_satisfaction = base_sat_per_fuck * partner_ability;
			float partner_satisfaction = base_sat_per_fuck * pawn_ability;

			// Measure by own ability instead if masturbating.
			// Low pawn_ability usually means things like impaired manipulation, which should make masturbation less satisfactory.
			if (sextype == xxx.rjwSextype.Masturbation)
				pawn_satisfaction = base_sat_per_fuck * pawn_ability;

			if (pawn != null && partner != null)
			{
				if (xxx.is_animal(partner) && xxx.is_zoophile(pawn))
				{
					pawn_satisfaction *= 1.5f;
				}
				if (partner.Dead && xxx.is_necrophiliac(pawn))
				{
					pawn_satisfaction *= 1.5f;
				}

				if (!partner.Dead)
				{
					xxx.TransferNutrition(pawn, partner, sextype);
					if (rape)
					{
						//--Log.Message("SexUtility::aftersex( " + pawn_name + ", " + partner_name + " ) - Broken mind updates for rape");
						xxx.processBrokenPawn(partner);
						xxx.ExtraSatisfyForBrokenPawn(partner);
					}
				}
			}

			SatisfyPersonal(pawn, partner, sextype, rape, pawn_satisfaction);
			SatisfyPersonal(partner, pawn, sextype, rape, partner_satisfaction);
			//--Log.Message("xxx::satisfy( " + pawn_name + ", " + partner_name + ", " + rape + " ) - pawn_satisfaction = " + pawn_satisfaction + ", partner_satisfaction = " + partner_satisfaction);
		}

		// Scales alien lifespan to human age. 
		// Some aliens have broken lifespans, that can be manually corrected here.
		public static int ScaleToHumanAge(Pawn pawn, int humanLifespan = 80)
		{
			float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
			float pawnLifespan = pawn.RaceProps.lifeExpectancy;

			if (pawn.def.defName == "Human") return (int)pawnAge; // Human, no need to scale anything.

			// Xen races, all broken and need a fix.
			if (pawn.def.defName.ContainsAny("Alien_Sergal", "Alien_SergalNME", "Alien_Xenn", "Alien_Racc", "Alien_Ferrex", "Alien_Wolvx", "Alien_Frijjid", "Alien_Fennex") && pawnLifespan >= 2000f)
			{
				pawnAge = Math.Min(pawnAge, 80f); // Clamp to 80.
				pawnLifespan = 80f;
			}
			if (pawn.def.defName.ContainsAny("Alien_Gnoll", "Alien_StripedGnoll") && pawnLifespan >= 2000f)
			{
				pawnAge = Math.Min(pawnAge, 60f); // Clamp to 60.
				pawnLifespan = 60f; // Mature faster than humans.
			}

			// Immortal races that mature at similar rate to humans.
			if (pawn.def.defName.ContainsAny("LF_Dragonia", "LotRE_ElfStandardRace", "Alien_Crystalloid", "Alien_CrystalValkyrie"))
			{
				pawnAge = Math.Min(pawnAge, 40f); // Clamp to 40 - never grow 'old'.
				pawnLifespan = 80f;
			}

			float age_scaling = humanLifespan / pawnLifespan;
			float scaled_age = pawnAge * age_scaling;

			if (scaled_age < 1)
				scaled_age = 1;

			return (int)scaled_age;
		}

		// Used in complex impregnation calculation. Pawns/animals with similar parts have better compatibility.
		public static float BodySimilarity(Pawn pawn, Pawn partner)
		{
			float size_adjustment = Mathf.Lerp(0.3f, 1.05f, 1.0f - Math.Abs(pawn.BodySize - partner.BodySize));

			//Log.Message("[RJW] Size adjustment: " + size_adjustment);

			List<BodyPartDef> pawn_partlist = new List<BodyPartDef> { };
			List<BodyPartDef> pawn_mismatched = new List<BodyPartDef> { };
			List<BodyPartDef> partner_mismatched = new List<BodyPartDef> { };

			//Log.Message("[RJW]Checking compatibility for " + xxx.get_pawnname(pawn) + " and " + xxx.get_pawnname(partner));
			bool pawnHasHands = pawn.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(BodyPartGroupDefOf.RightHand) || part.IsInGroup(BodyPartGroupDefOf.LeftHand));

			foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
			{
				pawn_partlist.Add(part.def);
			}
			float pawn_count = pawn_partlist.Count();

			foreach (BodyPartRecord part in partner.RaceProps.body.AllParts)
			{
				partner_mismatched.Add(part.def);
			}
			float partner_count = partner_mismatched.Count();

			foreach (BodyPartDef part in pawn_partlist)
			{
				if (partner_mismatched.Contains(part))
				{
					pawn_mismatched.Add(part);
					partner_mismatched.Remove(part);
				}
			}

			float pawn_mismatch = pawn_mismatched.Count() / pawn_count;
			float partner_mismatch = partner_mismatched.Count() / partner_count;

			//Log.Message("[RJW]Body type similarity for " + xxx.get_pawnname(pawn) + " and " + xxx.get_pawnname(partner) + ": " + Math.Round(((pawn_mismatch + partner_mismatch) * 50) * size_adjustment, 1) + "%");

			return ((pawn_mismatch + partner_mismatch) / 2) * size_adjustment;
		}

		private static void SatisfyPersonal(Pawn pawn, Pawn partner, xxx.rjwSextype sextype, bool violent, float satisfaction)
		{
			//--Log.Message("xxx::satisfy( " + pawn_name + ", " + partner_name + ", " + violent + "," + isCoreLovin + " ) - modifying partner satisfaction");
			if (pawn?.needs?.TryGetNeed<Need_Sex>() == null) return;

			var quirkCount = Quirk.CountSatisfiedQuirks(pawn, partner, sextype, violent);
			satisfaction += quirkCount * base_sat_per_quirk;

			if (xxx.is_rapist(pawn) || xxx.is_bloodlust(pawn))
			{
				// Rapists and Bloodlusts get more satisfaction from violetn encounters, and less from non-violent
				satisfaction *= violent ? 1.20f : 0.8f;
			}
			else
			{
				// non-violent pawns get less satisfaction from violent encounters, and full from non-violent
				satisfaction *= violent ? 0.8f : 1.0f;
			}

			if (pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomAddiction")) && !pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomEffect")))
			{
				//Log.Message("[RJW]xxx::satisfy 0 pawn is " + xxx.get_pawnname(pawn));
				satisfaction = 0.1f;
			}

			//--Log.Message("xxx::satisfy( " + pawn_name + ", " + partner_name + ", " + violent + "," + isCoreLovin + " ) - setting pawn joy");
			pawn.needs.TryGetNeed<Need_Sex>().CurLevel += satisfaction;

			if (pawn.needs.joy != null)
			{
				pawn.needs.joy.CurLevel += satisfaction * 0.50f;   // convert half of satisfaction to joy
			}

			if (quirkCount > 0)
			{
				Quirk.AddThought(pawn);
			}
		}

		[SyncMethod]
		private static void CheckTraitGain(Pawn pawn, Pawn partner, bool rape = false)
		{
			if (!xxx.has_traits(pawn) || pawn.records.GetValue(xxx.CountOfSex) <= 10) return;

			if (RJWSettings.AddTrait_Necrophiliac && !xxx.is_necrophiliac(pawn) && partner.Dead && pawn.records.GetValue(xxx.CountOfSexWithCorpse) > 0.5 * pawn.records.GetValue(xxx.CountOfSex))
			{
				pawn.story.traits.GainTrait(new Trait(xxx.necrophiliac));
				//Log.Message(xxx.get_pawnname(necro) + " aftersex, not necro, adding necro trait");
			}
			if (RJWSettings.AddTrait_Rapist && !xxx.is_rapist(pawn) && !xxx.is_masochist(pawn) && rape && pawn.records.GetValue(xxx.CountOfRapedHumanlikes) > 0.12 * pawn.records.GetValue(xxx.CountOfSex))
			{
				var chance = 0.5f;
				if (xxx.is_kind(pawn)) chance -= 0.25f;
				if (xxx.is_prude(pawn)) chance -= 0.25f;
				if (xxx.is_zoophile(pawn)) chance -= 0.25f; // Less interested in raping humanlikes.
				if (xxx.is_ascetic(pawn)) chance -= 0.2f;
				if (xxx.is_bloodlust(pawn)) chance += 0.2f;
				if (xxx.is_psychopath(pawn)) chance += 0.25f;

				//Rand.PopState();
				//Rand.PushState(RJW_Multiplayer.PredictableSeed());
				if (Rand.Chance(chance))
				{
					pawn.story.traits.GainTrait(new Trait(xxx.rapist));
					//--Log.Message(xxx.get_pawnname(pawn) + " aftersex, not rapist, adding rapist trait");
				}
			}
			if (RJWSettings.AddTrait_Zoophiliac && !xxx.is_zoophile(pawn) && xxx.is_animal(partner) && (pawn.records.GetValue(xxx.CountOfSexWithAnimals) + pawn.records.GetValue(xxx.CountOfSexWithInsects) > 0.5 * pawn.records.GetValue(xxx.CountOfSex)))
			{
				pawn.story.traits.GainTrait(new Trait(xxx.zoophile));
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(xxx.got_bred);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(xxx.got_groped);
				pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(xxx.got_licked);
				//--Log.Message(xxx.get_pawnname(pawn) + " aftersex, not zoo, adding zoo trait");
			}
			if (RJWSettings.AddTrait_Nymphomaniac && !xxx.is_nympho(pawn))
			{
				if (pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomAddiction")))
				{
					pawn.story.traits.GainTrait(new Trait(xxx.nymphomaniac));
					//Log.Message(xxx.get_pawnname(pawn) + " is HumpShroomAddicted, not nymphomaniac, adding nymphomaniac trait");
				}

			}
		}

		// Checks if enough time has passed from previous lovin'.
		public static bool ReadyForLovin(Pawn pawn)
		{
			return Find.TickManager.TicksGame > pawn.mindState.canLovinTick;
		}

		// Checks if enough time has passed from previous search for a hookup.
		// Checks if hookups allowed during working hours, exlcuding nymphs
		public static bool ReadyForHookup(Pawn pawn)
		{
			if (!xxx.is_nympho(pawn) && RJWHookupSettings.NoHookupsDuringWorkHours && ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything) == TimeAssignmentDefOf.Work) return false;
			return Find.TickManager.TicksGame > CompRJW.Comp(pawn).NextHookupTick;
		}

		private static void IncreaseTicksToNextLovin(Pawn pawn)
		{
			if (pawn == null || pawn.Dead) return;
			int currentTime = Find.TickManager.TicksGame;
			if (pawn.mindState.canLovinTick <= currentTime)
				pawn.mindState.canLovinTick = currentTime + GenerateMinTicksToNextLovin(pawn);
		}

		[SyncMethod]
		public static int GenerateMinTicksToNextLovin(Pawn pawn)
		{
			if (DebugSettings.alwaysDoLovin) return 100;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			float interval = JobDriver_Lovin.LovinIntervalHoursFromAgeCurve.Evaluate(ScaleToHumanAge(pawn));
			float rinterval = Math.Max(0.5f, Rand.Gaussian(interval, 0.3f));

			float tick = 1.0f;

			// Nymphs automatically get the tick increase from the trait influence on sex drive.
			if (xxx.is_animal(pawn))
			{
				//var mateMtbHours = pawn.RaceProps.mateMtbHours / 24 * 60000;
				//if (mateMtbHours > 0)
				//	interval = mateMtbHours
				tick = 0.75f;
			}
			else if (xxx.is_prude(pawn))
				tick = 1.5f;

			if (pawn.Has(Quirk.Vigorous))
				tick *= 0.8f;

			float sex_drive = xxx.get_sex_drive(pawn);
			if (sex_drive <= 0.05f)
				sex_drive = 0.05f;

			return (int)(tick * rinterval * (2500.0f / sex_drive));
		}

		public static void IncreaseTicksToNextHookup(Pawn pawn)
		{
			if (pawn == null || pawn.Dead)
				return;

			// There are 2500 ticks per rimworld hour.  Sleeping an hour between checks seems like a good start.
			// We could get fancier and weight it by sex drive and stuff, but would people even notice?
			const int TicksBetweenHookups = 2500;

			int currentTime = Find.TickManager.TicksGame;
			CompRJW.Comp(pawn).NextHookupTick = currentTime + TicksBetweenHookups;
		}

		// <summary>
		// Determines the sex type and handles the log output.
		// "Pawn" should be initiator of the act (rapist, whore, etc), "Partner" should be the target.
		// </summary>
		public static void ProcessSex(Pawn pawn, Pawn partner, bool usedCondom = false, bool rape = false, bool isCoreLovin = false, bool whoring = false, xxx.rjwSextype sextype = xxx.rjwSextype.None)
		{
			//Log.Message("usedCondom=" + usedCondom);
			if (pawn == null || partner == null)
			{
				if (pawn == null)
					Log.Error("[RJW][SexUtility] ERROR: pawn is null.");
				if (partner == null)
					Log.Error("[RJW][SexUtility] ERROR: partner is null.");
				return;
			}

			// Re-draw clothes.
			if (xxx.is_human(pawn))
				pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
			if (xxx.is_human(partner))
				partner.Drawer.renderer.graphics.ResolveApparelGraphics();

			IncreaseTicksToNextLovin(pawn);
			IncreaseTicksToNextLovin(partner);

			rape = rape || pawn?.CurJob.def == xxx.gettin_raped || (!partner.Dead && partner.CurJob.def == xxx.gettin_raped); // Double-checking.

			Pawn receiving = partner;
			if(sextype == xxx.rjwSextype.None)
				sextype = DetermineSextype(pawn, partner, rape, whoring, receiving);
			Aftersex(pawn, partner, usedCondom, rape, isCoreLovin, sextype);

			//--Log.Message("SexUtility::processsex( " + pawn_name + ", " + partner_name + " ) - checking thoughts");
			xxx.think_about_sex(pawn, partner, receiving == pawn, rape, sextype, isCoreLovin, whoring);
			xxx.think_about_sex(partner, pawn, receiving == partner, rape, sextype, isCoreLovin, false);

			pawn.Drawer.Notify_MeleeAttackOn(partner);
		}

		[SyncMethod]
		public static xxx.rjwSextype DetermineSextype(Pawn pawn, Pawn partner, bool rape, bool whoring, Pawn receiving)
		{
			//--Log.Message("[RJW]SexUtility::processSex is called for pawn" + xxx.get_pawnname(pawn) + " and partner " + xxx.get_pawnname(partner));
			var pawnpartBPR = Genital_Helper.get_genitalsBPR(pawn);
			var pawnparts = Genital_Helper.get_PartsHediffList(pawn, pawnpartBPR);
			var partenerpartBPR = Genital_Helper.get_genitalsBPR(partner);
			var partenerparts = Genital_Helper.get_PartsHediffList(partner, partenerpartBPR);

			bool pawnHasMouth = Genital_Helper.has_mouth(pawn) && !Genital_Helper.oral_blocked(pawn);
			bool pawnHasAnus = Genital_Helper.has_anus(pawn) && !Genital_Helper.anus_blocked(pawn);
			bool pawnHasBreasts = Genital_Helper.has_breasts(pawn) && !Genital_Helper.breasts_blocked(pawn) && Genital_Helper.can_do_breastjob(pawn);
			bool pawnHasVagina = Genital_Helper.has_vagina(pawn, pawnparts) && !Genital_Helper.vagina_blocked(pawn);
			bool pawnHasPenis = (Genital_Helper.has_penis_fertile(pawn, pawnparts) || Genital_Helper.has_penis_infertile(pawn, pawnparts) || Genital_Helper.has_ovipositorF(pawn, pawnparts)) && !Genital_Helper.penis_blocked(pawn);
			bool pawnHasMultiPenis = Genital_Helper.has_multipenis(pawn, pawnparts) && !Genital_Helper.penis_blocked(pawn);
			//--Log.Message("[RJW]SexUtility::processSex is pawnHasPenis " + pawnHasPenis);
			//--Log.Message("[RJW]SexUtility::processSex is is_female(Partner) " + is_female(Partner));
			bool partnerHasMouth = Genital_Helper.has_mouth(partner) && !Genital_Helper.oral_blocked(partner);
			bool partnerHasAnus = Genital_Helper.has_anus(partner) && !Genital_Helper.anus_blocked(partner);
			bool partnerHasBreasts = Genital_Helper.has_breasts(partner) && !Genital_Helper.breasts_blocked(partner) && Genital_Helper.can_do_breastjob(partner);
			bool partnerHasVagina = Genital_Helper.has_vagina(partner, partenerparts) && !Genital_Helper.vagina_blocked(partner);
			bool partnerHasPenis = (Genital_Helper.has_penis_fertile(partner, partenerparts) || Genital_Helper.has_penis_infertile(partner, partenerparts)) && !Genital_Helper.penis_blocked(partner);
			bool partnerHasMultiPenis = Genital_Helper.has_multipenis(partner, partenerparts) && !Genital_Helper.penis_blocked(partner);

			bool pawnHasHands = pawn.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(BodyPartGroupDefOf.RightHand) || part.IsInGroup(BodyPartGroupDefOf.LeftHand)) && !Genital_Helper.hands_blocked(pawn);
			bool partnerHasHands = partner.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(BodyPartGroupDefOf.RightHand) || part.IsInGroup(BodyPartGroupDefOf.LeftHand)) && !Genital_Helper.hands_blocked(partner);
			if (pawnHasHands)
				pawnHasHands = pawn.health?.capacities?.GetLevel(PawnCapacityDefOf.Manipulation) > 0;
			if (partnerHasHands)
				partnerHasHands = partner.health?.capacities?.GetLevel(PawnCapacityDefOf.Manipulation) > 0;

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			/*Things to keep in mind:
			 - Both the initiator and the partner can be female, male, or futa.
			 - Can be rape or consensual.
			 - Includes pawns with blocked or no genitalia.
			
			 Need to add support here when new types get added.
			 Types to be added: 69, spooning...?

			 This would be much 'better' code as arrays, but that'd hurt readability and make it harder to modify.
			 If this weren't 3.5, I'd use tuples.*/

			// Range 1.0 to 0.0 [100% to 0%].
			float vagiIni = RJWPreferenceSettings.vaginal;				// Vaginal
			float VagiRec = RJWPreferenceSettings.vaginal;				// Vaginal - receiving
			float analIni = RJWPreferenceSettings.anal;					// Anal
			float analRec = RJWPreferenceSettings.anal;					// Anal - receiving
			float cunnIni = RJWPreferenceSettings.cunnilingus;		 	// Cunnilingus
			float cunnRec = RJWPreferenceSettings.cunnilingus;		 	// Cunnilingus - receiving
			float rimmIni = RJWPreferenceSettings.rimming;				// Rimming
			float rimmRec = RJWPreferenceSettings.rimming;				// Rimming - receiving
			float fellIni = RJWPreferenceSettings.fellatio;				// Fellatio
			float fellRec = RJWPreferenceSettings.fellatio;				// Fellatio - receiving
			float doubIni = RJWPreferenceSettings.double_penetration;	// DoublePenetration
			float doubRec = RJWPreferenceSettings.double_penetration;	// DoublePenetration - receiving
			float bresIni = RJWPreferenceSettings.breastjob;			// Breastjob
			float bresRec = RJWPreferenceSettings.breastjob;			// Breastjob - receiving
			float handIni = RJWPreferenceSettings.handjob;				// Handjob
			float handRec = RJWPreferenceSettings.handjob;				// Handjob - receiving
			float footIni = RJWPreferenceSettings.footjob;				// Footjob
			float footRec = RJWPreferenceSettings.footjob;				// Footjob - receiving
			float fingIni = RJWPreferenceSettings.fingering;			// Fingering
			float fingRec = RJWPreferenceSettings.fingering;			// Fingering - receiving
			float scisIni = RJWPreferenceSettings.scissoring;			// Scissoring
			float scisRec = RJWPreferenceSettings.scissoring;			// Scissoring - receiving
			float mutuIni = RJWPreferenceSettings.mutual_masturbation;	// MutualMasturbation
			float mutuRec = RJWPreferenceSettings.mutual_masturbation;	// MutualMasturbation - receiving
			float fistIni = RJWPreferenceSettings.fisting;				// Fisting
			float fistRec = RJWPreferenceSettings.fisting;				// Fisting - receiving
			float sixtIni = RJWPreferenceSettings.sixtynine;			// 69
			float sixtRec = RJWPreferenceSettings.sixtynine;			// 69 - receiving

			string pawn_quirks = CompRJW.Comp(pawn).quirks.ToString();
			string partner_quirks = CompRJW.Comp(partner).quirks.ToString();

			// Modifiers > 1.0f = higher chance of being picked
			// Modifiers < 1.0f = lower chance of being picked
			// 0 = disables types.

			// Pawn does not need sex, or is not horny. Mostly whores, sexbots, etc.
			if (xxx.need_some_sex(pawn) < 1.0f)
			{
				vagiIni *= 0.6f;
				analIni *= 0.6f;
				cunnRec *= 0.6f;
				rimmRec *= 0.6f;
				fellRec *= 0.6f;
				doubIni *= 0.6f;
				bresRec *= 0.6f;
				handRec *= 0.6f;
				footRec *= 0.6f;
				fingRec *= 0.6f;
				sixtIni *= 0.6f;
				sixtRec *= 0.6f;
			}

			// Adjusts initial chances
			if (pawnHasPenis)
			{
				vagiIni *= 1.5f;
				analIni *= 1.5f;
				fellRec *= 1.5f;
				doubIni *= 1.5f;
				if (partnerHasVagina)
				{
					fistRec *= 0.5f;
					rimmIni *= 0.8f;
					rimmRec *= 0.5f;
				}
			}
			else if (pawnHasVagina)
			{
				VagiRec *= 1.2f;
				scisRec *= 1.2f;
			}

			//Size adjustments. Makes pawns reluctant to have penetrative sex if there's large size difference.
			if (partner.BodySize > pawn.BodySize * 2 && !rape && !xxx.is_animal(pawn))
			{
				VagiRec *= 0.6f;
				analRec *= 0.6f;
				fistRec *= 0.2f;
				sixtIni *= 0.2f;
				sixtRec *= 0.2f;
			}
			else if (pawn.BodySize > partner.BodySize * 2 && !rape && !xxx.is_animal(pawn) && !xxx.is_psychopath(pawn))
			{
				vagiIni *= 0.6f;
				analIni *= 0.6f;
				fistIni *= 0.3f;
				sixtIni *= 0.2f;
				sixtRec *= 0.2f;
			}

			if (partner.Dead || partner.Downed || !partner.health.capacities.CanBeAwake) // This limits options a lot, for obvious reason.
			{
				VagiRec = 0f;
				analRec = 0f;
				cunnIni *= 0.3f;
				cunnRec = 0f;
				rimmIni *= 0.1f;
				rimmRec = 0f;
				fellRec *= 0.2f;
				doubRec = 0f;
				bresRec = 0f;
				handRec = 0f;
				footRec = 0f;
				fingRec = 0f;
				fingIni *= 0.5f;
				scisIni *= 0.2f;
				scisRec = 0f;
				mutuIni = 0f;
				mutuRec = 0f;
				fistRec = 0f;
				sixtRec = 0f;
				if (pawnHasPenis)
				{
					sixtIni *= 0.5f; // Can facefuck the unconscious (or corpse). :/
				}
				else
				{
					sixtIni = 0f;
				}
				if (partner.Dead)
				{
					fellIni = 0f;
					handIni = 0f;
					footIni = 0f;
					bresIni = 0f;
					fingIni = 0f;
					fistIni *= 0.2f; // Fisting a corpse? Whatever floats your boat, I guess.
				}
				else
				{
					fellIni *= 0.4f;
					handIni *= 0.5f;
					footIni *= 0.2f;
					bresIni *= 0.2f;
					fistIni *= 0.6f;
				}
			}

			if (rape)
			{
				// This makes most types less likely to happen during rape, but doesn't disable them. 
				// Things like forced blowjob can happen, so it shouldn't be impossible in rjw.
				VagiRec *= 0.5f; //Forcing vaginal on male.
				analRec *= 0.3f; //Forcing anal on male.
				cunnIni *= 0.3f; //Forced cunnilingus.
				cunnRec *= 0.6f;
				rimmIni *= 0.1f;
				fellIni *= 0.4f;
				doubRec *= 0.2f; //Rapist forcing the target to double-penetrate her - unlikely.
				bresIni *= 0.2f;
				bresRec *= 0.2f;
				handIni *= 0.6f;
				handRec *= 0.2f;
				footIni *= 0.2f;
				footRec *= 0.1f;
				fingIni *= 0.8f;
				fingRec *= 0.1f;
				scisIni *= 0.6f;
				scisRec *= 0.1f;
				mutuIni = 0f;
				mutuRec = 0f;
				fistIni *= 1.2f;
				fistRec = 0f;
				sixtIni *= 0.5f;
				sixtRec = 0f;
			}

			if (xxx.is_animal(pawn))
			{
				if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, partner))
				{   //Bond animals
					VagiRec *= 1.8f; //Presenting
					analRec *= 1.2f;
					fellIni *= 1.2f;
					cunnIni *= 1.2f;
				}
				else
				{
					VagiRec *= 0.3f;
					analRec *= 0.3f;
				}
				vagiIni *= 1.8f;
				analIni *= 0.9f;
				cunnRec *= 0.2f;
				rimmRec *= 0.1f;
				fellRec *= 0.1f;
				doubIni *= 0.6f;
				doubRec *= 0.1f;
				bresIni = 0f;
				bresRec *= 0.1f;
				handIni *= 0.4f; //Enabled for primates.
				handRec *= 0.1f;
				footIni = 0f;
				footRec *= 0.1f;
				fingIni *= 0.3f; //Enabled for primates.
				fingRec *= 0.2f;
				scisIni *= 0.2f;
				scisRec *= 0.1f;
				mutuIni *= 0.1f;
				mutuRec *= 0.1f;
				fistIni *= 0.2f; //Enabled for primates...
				fistRec *= 0.6f;
				sixtIni *= 0.2f;
				sixtRec *= 0.2f;
			}

			if (xxx.is_animal(partner)) // Zoophilia and animal-on-animal
			{
				if (pawn.Faction != partner.Faction && rape) // Wild animals && animals from other factions
				{
					cunnRec *= 0.1f; // Wild animals bite, colonists should be smart enough to not try to force oral from them.
					rimmRec *= 0.1f;
					fellRec *= 0.1f;
				}
				else
				{
					cunnRec *= 0.5f;
					rimmRec *= 0.4f;
					fellRec *= 0.4f;
				}
				cunnIni *= 0.7f;
				rimmIni *= 0.1f;
				fellIni *= 1.2f;
				doubIni *= 0.6f;
				doubRec *= 0.1f;
				bresIni *= 0.3f; //Giving a breastjob to animals - unlikely.
				bresRec = 0f;
				handIni *= 1.2f;
				handRec *= 0.4f; //Animals are not known for giving handjobs, but enabled for primates and such.
				footIni *= 0.3f;
				footRec = 0f;
				fingIni *= 0.8f;
				fingRec *= 0.2f; //Enabled for primates.
				scisIni *= 0.1f;
				scisRec = 0f;
				mutuIni *= 0.6f;
				mutuRec *= 0.1f;
				fistIni *= 0.6f;
				fistRec *= 0.1f;
				sixtIni *= 0.2f;
				sixtRec *= 0.2f;
			}

			//Quirks
			if (pawn_quirks.Contains("Podophile")) // Foot fetish
			{
				footIni *= 2.0f;
				footRec *= 2.5f;
			}
			if (partner_quirks.Contains("Podophile"))
			{
				footIni *= 2.5f;
				footRec *= 2.0f;
			}
			if (pawn_quirks.Contains("Impregnation fetish") && (PregnancyHelper.CanImpregnate(pawn, partner) || PregnancyHelper.CanImpregnate(partner, pawn)))
			{
				vagiIni *= 2.5f;
				VagiRec *= 2.5f;
			}

			if (whoring) // Paid sex
			{
				VagiRec *= 1.5f;
				analIni *= 0.7f; //Some customers may pay for this.
				analRec *= 1.2f;
				cunnIni *= 1.2f;
				cunnRec *= 0.3f; //Customer paying to lick the whore - uncommon.
				rimmRec *= 0.2f;
				fellIni *= 1.5f; //Classic.
				fellRec *= 0.2f;
				doubIni *= 0.8f;
				doubRec *= 1.2f;
				bresIni *= 1.2f;
				bresRec *= 0.1f;
				handIni *= 1.5f;
				handRec *= 0.1f;
				footIni *= 0.6f;
				footRec *= 0.1f;
				fingIni *= 0.6f;
				fingRec *= 0.2f;
				scisRec *= 0.2f;
				mutuIni *= 0.2f;
				mutuRec *= 0.2f;
				fistIni *= 0.6f;
				fistRec *= 0.7f;
				sixtIni *= 0.7f;
				sixtRec *= 0.7f;
			}

			// Pawn lacks vagina, disable related types.
			if (!pawnHasVagina)
			{
				VagiRec = 0f;
				cunnRec = 0f;
				doubRec = 0f;
				fingRec = 0f;
				scisIni = 0f;
				scisRec = 0f;
			}
			if (!partnerHasVagina)
			{
				vagiIni = 0f;
				cunnIni = 0f;
				doubIni = 0f;
				fingIni = 0f;
				scisIni = 0f;
				scisRec = 0f;
			}

			// Pawn lacks penis, disable related types.
			if (!pawnHasPenis)
			{
				vagiIni = 0f;
				analIni = 0f;
				fellRec = 0f;
				doubIni = 0f;
				bresRec = 0f;
				handRec = 0f;
				footRec = 0f;
			}
			else if (pawnHasMultiPenis && partnerHasVagina && partnerHasAnus)
			{
				// Pawn has multi-penis and can use it. Single-penetration chance down.
				vagiIni *= 0.8f;
				analIni *= 0.8f;
				doubIni *= 1.5f;
			}
			else
			{
				doubIni = 0f;
			}

			if (!partnerHasPenis)
			{
				VagiRec = 0f;
				analRec = 0f;
				fellIni = 0f;
				doubRec = 0f;
				bresIni = 0f;
				handIni = 0f;
				footIni = 0f;
			}
			else if (partnerHasMultiPenis && pawnHasVagina && pawnHasAnus)
			{
				// Pawn has multi-penis and can use it. Single-penetration chance down.
				VagiRec *= 0.8f;
				analRec *= 0.8f;
				doubRec *= 1.5f;
			}
			else
			{
				doubRec = 0f;
			}

			// One pawn lacks genitalia: no mutual masturbation or 69.
			if (!(pawnHasPenis || pawnHasVagina) || !(partnerHasPenis || partnerHasVagina))
			{
				mutuIni = 0f;
				mutuRec = 0f;
				sixtIni = 0f;
				sixtRec = 0f;
			}

			// Pawn lacks anus... 
			if (!pawnHasAnus)
			{
				analRec = 0f;
				rimmRec = 0f;
				doubRec = 0f;
				fistRec = 0f;
			}
			if (!partnerHasAnus)
			{
				analIni = 0f;
				rimmIni = 0f;
				doubIni = 0f;
				fistIni = 0f;
			}

			// Pawn lacks boobs
			if (!pawnHasBreasts)
			{
				bresIni = 0f;
			}
			if (!partnerHasBreasts)
			{
				bresRec = 0f;
			}

			// Pawn lacks hands
			if (!pawnHasHands)
			{
				handIni = 0f;
				fingIni = 0f;
				mutuIni = 0f;
				fistIni = 0f;
			}
			if (!partnerHasHands)
			{
				handRec = 0f;
				fingRec = 0f;
				mutuRec = 0f;
				fistRec = 0f;
			}

			// Pawn lacks mouth
			if (!pawnHasMouth)
			{
				cunnIni = 0f;
				rimmIni = 0f;
				fellIni = 0f;
				sixtIni = 0f;
			}
			if (!partnerHasMouth)
			{
				cunnIni = 0f;
				rimmIni = 0f;
				fellIni = 0f;
				sixtIni = 0f;
			}

			List<float> sexTypes = new List<float> {
				vagiIni, VagiRec,	//  0,  1
				analIni, analRec,	//  2,  3
				cunnIni, cunnRec,	//  4,  5
				rimmIni, rimmRec,	//  6,  7 
				fellIni, fellRec,	//  8,  9
				doubIni, doubRec,	// 10, 11
				bresIni, bresRec,	// 12, 13
				handIni, handRec,	// 14, 15
				footIni, footRec,	// 16, 17
				fingIni, fingRec,	// 18, 19
				scisIni, scisRec,	// 20, 21
				mutuIni, mutuRec,	// 22, 23
				fistIni, fistRec,	// 24, 25
				sixtIni, sixtRec	// 26, 27
			};

			// Bit of randomization..
			for (int i = 0; i < sexTypes.Count; i++)
			{
				sexTypes[i] = Rand.Range(0f, sexTypes[i]);
			}

			float maxValue = sexTypes.Max();

			if (!(maxValue > 0f))
			{
				Log.Warning("[RJW] ERROR: No available sex types for " + xxx.get_pawnname(pawn) + " and " + xxx.get_pawnname(partner));
				Log.Warning("[RJW] ERROR: your pawns missing parts hediffs");
				return xxx.rjwSextype.None;
			}

			List<RulePackDef> extraSentencePacks = new List<RulePackDef>();

			string rulepack; //defName of the rulePackDef (see RulePacks_Sex.xml, etc.)
			InteractionDef dictionaryKey;

			Pawn giving = pawn;

			if (sexTypes.IndexOf(maxValue) % 2 != 0 && !rape)
			{
				giving = partner;
				receiving = pawn;
			}

			int typeIndex = sexTypes.IndexOf(maxValue);

			if (xxx.is_animal(pawn))
			{
				rulepack = rulepacks[typeIndex, 2];
				dictionaryKey = dictionarykeys[typeIndex, 2];
			}
			else if (!rape)
			{
				rulepack = rulepacks[typeIndex, 0];
				dictionaryKey = dictionarykeys[typeIndex, 0];
			}
			else // is rape
			{
				rulepack = rulepacks[typeIndex, 1];
				dictionaryKey = dictionarykeys[typeIndex, 1];
			}

			// Override for mechanoid implanting. Add the defNames for species that should be allowed to do it.
			// Currently only includes the core mechanoids, plus some from Orion's More Mechanoids mod.
			if (pawn.kindDef.race.defName.ContainsAny("Mech_Centipede", "Mech_Lancer", "Mech_Scyther", "Mech_Crawler", "Mech_Skullywag", "Mech_Flamebot", "Mech_Mammoth", "Mech_Assaulter"))
			{
				dictionaryKey = mechImplant;
				rulepack = "MechImplantingRP";
			}

			if (rulepack == "FellatioRP")
			{
				if (GetPawnBodyPart(pawn, "Beak") != null && pawn == giving || GetPawnBodyPart(partner, "Beak") != null && pawn == receiving)
				{
					rulepack = "BeakjobRP";
					dictionaryKey = beakjob;
				}
			}

			xxx.rjwSextype sextype = sexActs[dictionaryKey];

			// Text override for corpse violation.
			if (pawn.CurJob.def == xxx.RapeCorpse)
				dictionaryKey = violateCorpse;

			extraSentencePacks.Add(RulePackDef.Named(rulepack));
			PlayLogEntry_Interaction playLogEntry = new PlayLogEntry_Interaction(dictionaryKey, giving, receiving, extraSentencePacks);
			Find.PlayLog.Add(playLogEntry);

			return sextype;
		}

		[SyncMethod]
		public static void Sex_Beatings(Pawn pawn, Pawn partner, bool isRape = false)
		{
			if (isRape && !RJWSettings.rape_beating || (xxx.is_animal(pawn) && xxx.is_animal(partner)))
				return;

			//dont remember what it does, probably manhunter stuff or not? disable and wait reports
			//if (!xxx.is_human(pawn))
			//	return;

			//If a pawn is incapable of violence/has low melee, they most likely won't beat their partner
			if (pawn.skills?.GetSkill(SkillDefOf.Melee).Level < 1)
				return;

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float rand_value = Rand.Value;
			//float rand_value = RJW_Multiplayer.RJW_MP_RAND();
			float victim_pain = partner.health.hediffSet.PainTotal;
			// bloodlust makes the aggressor more likely to hit the prisoner
			float beating_chance = xxx.config.base_chance_to_hit_prisoner * (xxx.is_bloodlust(pawn) ? 1.5f : 1.0f);
			// psychopath makes the aggressor more likely to hit the prisoner past the significant_pain_threshold
			float beating_threshold = xxx.is_psychopath(pawn) ? xxx.config.extreme_pain_threshold : pawn.HostileTo(partner) ? xxx.config.significant_pain_threshold : xxx.config.minor_pain_threshold;

			//--Log.Message("roll_to_hit:  rand = " + rand_value + ", beating_chance = " + beating_chance + ", victim_pain = " + victim_pain + ", beating_threshold = " + beating_threshold);
			if ((victim_pain < beating_threshold && rand_value < beating_chance) || (rand_value < (beating_chance / 2) && xxx.is_bloodlust(pawn)))
			{
				//--Log.Message("   done told her twice already...");
				if (InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out Verb v))
				{
					//Log.Message("   v. : " + v);
					//Log.Message("   v.GetDamageDef : " + v.GetDamageDef());
					//Log.Message("   v.v.tool - " + v.tool.label);
					//Log.Message("   v.v.tool.power base - " + v.tool.power);
					var orgpower = v.tool.power;
					//in case something goes wrong
					try
					{
						//Log.Message("   v.v.tool.power base - " + v.tool.power);
						if (RJWSettings.gentle_rape_beating)
						{
							v.tool.power = 0;
							//partner.stances.stunner.StunFor(600, pawn);
						}
						//Log.Message("   v.v.tool.power mod - " + v.tool.power);
						pawn.meleeVerbs.TryMeleeAttack(partner, v);
					}
					catch
					{ }
					v.tool.power = orgpower;
					//Log.Message("   v.v.tool.power reset - " + v.tool.power);
				}
			}
		}

		// Overrides the current clothing. Defaults to nude, with option to keep headgear on.
		public static void DrawNude(Pawn pawn, bool keep_hat_on = false)
		{
			if (!xxx.is_human(pawn)) return;
			if (RJWPreferenceSettings.sex_wear == RJWPreferenceSettings.Clothing.Clothed) return;

			pawn.Drawer.renderer.graphics.ClearCache();
			pawn.Drawer.renderer.graphics.apparelGraphics.Clear();
			foreach (Apparel current in pawn.apparel.WornApparel.Where(x 
				=> x.def is bondage_gear_def
				|| RJWPreferenceSettings.sex_wear == RJWPreferenceSettings.Clothing.Headgear || keep_hat_on
				&& (x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)
				|| x.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))))
			{
				ApparelGraphicRecord item;
				if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, pawn.story.bodyType, out item))
				{
					pawn.Drawer.renderer.graphics.apparelGraphics.Add(item);
				}
			}
			pawn.Draw();
		}
		
		public static void reduce_rest(Pawn pawn, int x = 1)
		{
			if (pawn.Has(Quirk.Vigorous)) x -= 1;

			Need_Rest need_rest = pawn.needs.TryGetNeed<Need_Rest>();
			if (need_rest == null)
				return;

			need_rest.CurLevel -= need_rest.RestFallPerTick * x;
		}
	}
}

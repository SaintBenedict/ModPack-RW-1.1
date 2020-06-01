using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace rjw
{
	// TODO: Add option for logging pregnancy chance after sex (dev mode only?)
	// TODO: Add an alernate more complex system for pregnancy calculations, by using bodyparts, genitalia, and size (similar species -> higher chance, different -> lower chance)

	// TODO: Old settings that are not in use - evalute if they're needed and either convert these to settings, or delete them.
	/*public bool std_show_roll_to_catch; // Updated
	public float std_min_severity_to_pitch; // Updated
	public float std_env_pitch_cleanliness_exaggeration; // Updated
	public float std_env_pitch_dirtiness_exaggeration; // Updated
	public float std_outdoor_cleanliness; // Updated
	public float significant_pain_threshold; // Updated
	public float extreme_pain_threshold; // Updated
	public float base_chance_to_hit_prisoner; // Updated
	public int min_ticks_between_hits; // Updated
	public int max_ticks_between_hits; // Updated
	public float max_nymph_fraction; // Updated
	public float opp_inf_initial_immunity; // Updated
	public float comfort_prisoner_rape_mtbh_mul; // Updated
	public float whore_mtbh_mul; // Updated
	public float nymph_spawn_with_std_mul; // Updated
	public static bool comfort_prisoners_enabled; // Updated //this one is in config.cs as well!
	public static bool ComfortColonist; // New
	public static bool ComfortAnimal; // New
	public static bool rape_me_sticky_enabled; // Updated
	public static bool bondage_gear_enabled; // Updated
	public static bool nymph_joiners_enabled; // Updated
	public static bool always_accept_whores; // Updated
	public static bool nymphs_always_JoinInBed; // Updated
	public static bool zoophis_always_rape; // Updated
	public static bool rapists_always_rape; // Updated
	public static bool pawns_always_do_fapping; // Updated
	public static bool whores_always_findjob; // Updated
	public bool show_regular_dick_and_vag; // Updated
	*/

	public class RJWPreferenceSettings : ModSettings
	{
		public static float vaginal = 1.20f;
		public static float anal = 0.80f;
		public static float fellatio = 0.80f;
		public static float cunnilingus = 0.80f;
		public static float rimming = 0.40f;
		public static float double_penetration = 0.60f;
		public static float breastjob = 0.50f;
		public static float handjob = 0.80f;
		public static float mutual_masturbation = 0.70f;
		public static float fingering = 0.50f;
		public static float footjob = 0.30f;
		public static float scissoring = 0.50f;
		public static float fisting = 0.30f;
		public static float sixtynine = 0.69f;

		public static float asexual_ratio = 0.02f;
		public static float pansexual_ratio = 0.03f;
		public static float heterosexual_ratio = 0.7f;
		public static float bisexual_ratio = 0.15f;
		public static float homosexual_ratio = 0.1f;

		public static bool FapEverywhere = false;
		public static bool FapInBed = true;
		public static bool ShowForCP = true;
		public static bool ShowForBreeding = true;

		public static Clothing sex_wear = Clothing.Nude;
		public static RapeAlert rape_attempt_alert = RapeAlert.Humanlikes;
		public static RapeAlert rape_alert = RapeAlert.Humanlikes;
		public static Rjw_sexuality sexuality_distribution = Rjw_sexuality.Vanilla;

		public static AllowedSex Malesex = AllowedSex.All;
		public static AllowedSex FeMalesex = AllowedSex.All;

		public static int MaxQuirks = 1;

		public enum AllowedSex
		{
			All,
			Homo,
			Nohomo
		};

		public enum Clothing
		{
			Nude,
			Headgear,
			Clothed
		};
		
		public enum RapeAlert
		{
			Enabled,
			Colonists,
			Humanlikes,
			Silent,
			Disabled
		};

		public enum Rjw_sexuality
		{
			Vanilla,
			RimJobWorld,
			Psychology,
			SYRIndividuality
		};

		public static void DoWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.ColumnWidth = inRect.width / 3.15f;
			listingStandard.Begin(inRect);
			listingStandard.Gap(4f);
			listingStandard.Label("SexTypeFrequency".Translate());
			listingStandard.Gap(6f);
			listingStandard.Label("  " + "vaginal".Translate() + ": " + Math.Round(vaginal * 100, 0), -1, "vaginal_desc".Translate());
			vaginal = listingStandard.Slider(vaginal, 0.01f, 3.0f);
			listingStandard.Label("  " + "anal".Translate() + ": " + Math.Round(anal * 100, 0), -1, "anal_desc".Translate());
			anal = listingStandard.Slider(anal, 0.01f, 3.0f);
			listingStandard.Label("  " + "double_penetration".Translate() + ": " + Math.Round(double_penetration * 100, 0), -1, "double_penetration_desc".Translate());
			double_penetration = listingStandard.Slider(double_penetration, 0.01f, 3.0f);
			listingStandard.Label("  " + "fellatio".Translate() + ": " + Math.Round(fellatio * 100, 0), -1, "fellatio_desc".Translate());
			fellatio = listingStandard.Slider(fellatio, 0.01f, 3.0f);
			listingStandard.Label("  " + "cunnilingus".Translate() + ": " + Math.Round(cunnilingus * 100, 0), -1, "cunnilingus_desc".Translate());
			cunnilingus = listingStandard.Slider(cunnilingus, 0.01f, 3.0f);
			listingStandard.Label("  " + "rimming".Translate() + ": " + Math.Round(rimming * 100, 0), -1, "rimming_desc".Translate());
			rimming = listingStandard.Slider(rimming, 0.01f, 3.0f);
			listingStandard.Label("  " + "sixtynine".Translate() + ": " + Math.Round(sixtynine * 100, 0), -1, "sixtynine_desc".Translate());
			sixtynine = listingStandard.Slider(sixtynine, 0.01f, 3.0f);
			listingStandard.CheckboxLabeled("FapEverywhere".Translate(), ref FapEverywhere, "FapEverywhere_desc".Translate());
			listingStandard.CheckboxLabeled("FapInBed".Translate(), ref FapInBed, "FapInBed_desc".Translate());

			listingStandard.NewColumn();
			listingStandard.Gap(4f);
			if (listingStandard.ButtonText("Reset".Translate()))
			{
				vaginal = 1.20f;
				anal = 0.80f;
				fellatio = 0.80f;
				cunnilingus = 0.80f;
				rimming = 0.40f;
				double_penetration = 0.60f;
				breastjob = 0.50f;
				handjob = 0.80f;
				mutual_masturbation = 0.70f;
				fingering = 0.50f;
				footjob = 0.30f;
				scissoring = 0.50f;
				fisting = 0.30f;
				sixtynine = 0.69f;
			}
			listingStandard.Gap(6f);
			listingStandard.Label("  " + "breastjob".Translate() + ": " + Math.Round(breastjob * 100, 0), -1, "breastjob_desc".Translate());
			breastjob = listingStandard.Slider(breastjob, 0.01f, 3.0f);
			listingStandard.Label("  " + "handjob".Translate() + ": " + Math.Round(handjob * 100, 0), -1, "handjob_desc".Translate());
			handjob = listingStandard.Slider(handjob, 0.01f, 3.0f);
			listingStandard.Label("  " + "fingering".Translate() + ": " + Math.Round(fingering * 100, 0), -1, "fingering_desc".Translate());
			fingering = listingStandard.Slider(fingering, 0.01f, 3.0f);
			listingStandard.Label("  " + "fisting".Translate() + ": " + Math.Round(fisting * 100, 0), -1, "fisting_desc".Translate());
			fisting = listingStandard.Slider(fisting, 0.01f, 3.0f);
			listingStandard.Label("  " + "mutual_masturbation".Translate() + ": " + Math.Round(mutual_masturbation * 100, 0), -1, "mutual_masturbation_desc".Translate());
			mutual_masturbation = listingStandard.Slider(mutual_masturbation, 0.01f, 3.0f);
			listingStandard.Label("  " + "footjob".Translate() + ": " + Math.Round(footjob * 100, 0), -1, "footjob_desc".Translate());
			footjob = listingStandard.Slider(footjob, 0.01f, 3.0f);
			listingStandard.Label("  " + "scissoring".Translate() + ": " + Math.Round(scissoring * 100, 0), -1, "scissoring_desc".Translate());
			scissoring = listingStandard.Slider(scissoring, 0.01f, 3.0f);

			if (listingStandard.ButtonText("Malesex".Translate() + Malesex.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
				  new FloatMenuOption("AllowedSex.All".Translate(), (() => Malesex = AllowedSex.All)),
				  new FloatMenuOption("AllowedSex.Homo".Translate(), (() => Malesex = AllowedSex.Homo)),
				  new FloatMenuOption("AllowedSex.Nohomo".Translate(), (() => Malesex = AllowedSex.Nohomo))
				}));
			}
			if (listingStandard.ButtonText("FeMalesex".Translate() + FeMalesex.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
				  new FloatMenuOption("AllowedSex.All".Translate(), (() => FeMalesex = AllowedSex.All)),
				  new FloatMenuOption("AllowedSex.Homo".Translate(), (() => FeMalesex = AllowedSex.Homo)),
				  new FloatMenuOption("AllowedSex.Nohomo".Translate(), (() => FeMalesex = AllowedSex.Nohomo))
				}));
			}

			listingStandard.NewColumn();
			listingStandard.Gap(4f);
			// TODO: Add translation
			if (listingStandard.ButtonText("SexClothing".Translate() + sex_wear.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
				  new FloatMenuOption("SexClothingNude".Translate(), (() => sex_wear = Clothing.Nude)),
				  new FloatMenuOption("SexClothingHeadwear".Translate(), (() => sex_wear = Clothing.Headgear)),
				  new FloatMenuOption("SexClothingFull".Translate(), (() => sex_wear = Clothing.Clothed))
				}));
			}
			listingStandard.Gap(4f);
			if (listingStandard.ButtonText("RapeAttemptAlert".Translate() + rape_attempt_alert.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
				  new FloatMenuOption("RapeAttemptAlertAlways".Translate(), (() => rape_attempt_alert = RapeAlert.Enabled)),
				  new FloatMenuOption("RapeAttemptAlertHumanlike".Translate(), (() => rape_attempt_alert = RapeAlert.Humanlikes)),
				  new FloatMenuOption("RapeAttemptAlertColonist".Translate(), (() => rape_attempt_alert = RapeAlert.Colonists)),
				  new FloatMenuOption("RapeAttemptAlertSilent".Translate(), (() => rape_attempt_alert = RapeAlert.Silent)),
				  new FloatMenuOption("RapeAttemptAlertDisabled".Translate(), (() => rape_attempt_alert = RapeAlert.Disabled))
				}));
			}
			if (listingStandard.ButtonText("RapeAlert".Translate() + rape_alert.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
				  new FloatMenuOption("RapeAlertAlways".Translate(), (() => rape_alert = RapeAlert.Enabled)),
				  new FloatMenuOption("RapeAlertHumanlike".Translate(), (() => rape_alert = RapeAlert.Humanlikes)),
				  new FloatMenuOption("RapeAlertColonist".Translate(), (() => rape_alert = RapeAlert.Colonists)),
				  new FloatMenuOption("RapeAlertSilent".Translate(), (() => rape_alert = RapeAlert.Silent)),
				  new FloatMenuOption("RapeAlertDisabled".Translate(), (() => rape_alert = RapeAlert.Disabled))
				}));
			}
			listingStandard.CheckboxLabeled("RapeAlertCP".Translate(), ref ShowForCP, "RapeAlertCP_desc".Translate());
			listingStandard.CheckboxLabeled("RapeAlertBreeding".Translate(), ref ShowForBreeding, "RapeAlertBreeding_desc".Translate());

			listingStandard.Gap(26f);
			listingStandard.Label("SexualitySpread1".Translate(), -1, "SexualitySpread2".Translate());
			if (listingStandard.ButtonText(sexuality_distribution.ToString()))
			{
				Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
				{
					new FloatMenuOption("Vanilla", () => sexuality_distribution = Rjw_sexuality.Vanilla),
					//new FloatMenuOption("RimJobWorld", () => sexuality_distribution = Rjw_sexuality.RimJobWorld),
					new FloatMenuOption("SYRIndividuality", () => sexuality_distribution = Rjw_sexuality.SYRIndividuality),
					new FloatMenuOption("Psychology", () => sexuality_distribution = Rjw_sexuality.Psychology)
				}));
			}
			if (sexuality_distribution == Rjw_sexuality.RimJobWorld)
			{
				listingStandard.Label("SexualityAsexual".Translate() + Math.Round((asexual_ratio / GetTotal()) * 100, 1) + "%", -1, "SexualityAsexual_desc".Translate());
				asexual_ratio = listingStandard.Slider(asexual_ratio, 0.0f, 1.0f);
				listingStandard.Label("SexualityPansexual".Translate() + Math.Round((pansexual_ratio / GetTotal()) * 100, 1) + "%", -1, "SexualityPansexual_desc".Translate());
				pansexual_ratio = listingStandard.Slider(pansexual_ratio, 0.0f, 1.0f);
				listingStandard.Label("SexualityHeterosexual".Translate() + Math.Round((heterosexual_ratio / GetTotal()) * 100, 1) + "%", -1, "SexualityHeterosexual_desc".Translate());
				heterosexual_ratio = listingStandard.Slider(heterosexual_ratio, 0.0f, 1.0f);
				listingStandard.Label("SexualityBisexual".Translate() + Math.Round((bisexual_ratio / GetTotal()) * 100, 1) + "%", -1, "SexualityBisexual_desc".Translate());
				bisexual_ratio = listingStandard.Slider(bisexual_ratio, 0.0f, 1.0f);
				listingStandard.Label("SexualityGay".Translate() + Math.Round((homosexual_ratio / GetTotal()) * 100, 1) + "%", -1, "SexualityGay_desc".Translate());
				homosexual_ratio = listingStandard.Slider(homosexual_ratio, 0.0f, 1.0f);
			}
			else
			{
				if (!xxx.IndividualityIsActive && sexuality_distribution == Rjw_sexuality.SYRIndividuality)
					sexuality_distribution = Rjw_sexuality.Vanilla;
				else if (sexuality_distribution == Rjw_sexuality.SYRIndividuality)
					listingStandard.Label("SexualitySpreadIndividuality".Translate());

				else if (!xxx.PsychologyIsActive && sexuality_distribution == Rjw_sexuality.Psychology)
					sexuality_distribution = Rjw_sexuality.Vanilla;
				else if (sexuality_distribution == Rjw_sexuality.Psychology)
					listingStandard.Label("SexualitySpreadPsychology".Translate());

				else
					listingStandard.Label("SexualitySpreadVanilla".Translate());
			}
			listingStandard.Label("MaxQuirks".Translate() + ": " + MaxQuirks, -1f, "MaxQuirks_desc".Translate());
			MaxQuirks = (int)listingStandard.Slider(MaxQuirks, 0, 10);

			listingStandard.End();
		}

		public static float GetTotal()
		{
			return asexual_ratio + pansexual_ratio + heterosexual_ratio + bisexual_ratio + homosexual_ratio;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref vaginal, "vaginal_frequency");
			Scribe_Values.Look(ref anal, "anal_frequency");
			Scribe_Values.Look(ref fellatio, "fellatio_frequency");
			Scribe_Values.Look(ref cunnilingus, "cunnilingus_frequency");
			Scribe_Values.Look(ref rimming, "rimming_frequency");
			Scribe_Values.Look(ref double_penetration, "double_penetration_frequency");
			Scribe_Values.Look(ref sixtynine, "sixtynine_frequency");
			Scribe_Values.Look(ref breastjob, "breastjob_frequency");
			Scribe_Values.Look(ref handjob, "handjob_frequency");
			Scribe_Values.Look(ref footjob, "footjob_frequency");
			Scribe_Values.Look(ref fingering, "fingering_frequency");
			Scribe_Values.Look(ref fisting, "fisting_frequency");
			Scribe_Values.Look(ref mutual_masturbation, "mutual_masturbation_frequency");
			Scribe_Values.Look(ref scissoring, "scissoring_frequency");
			Scribe_Values.Look(ref asexual_ratio, "asexual_ratio");
			Scribe_Values.Look(ref pansexual_ratio, "pansexual_ratio");
			Scribe_Values.Look(ref heterosexual_ratio, "heterosexual_ratio");
			Scribe_Values.Look(ref bisexual_ratio, "bisexual_ratio");
			Scribe_Values.Look(ref homosexual_ratio, "homosexual_ratio");
			Scribe_Values.Look(ref FapEverywhere, "FapEverywhere");
			Scribe_Values.Look(ref FapInBed, "FapInBed");
			Scribe_Values.Look(ref sex_wear, "sex_wear");
			Scribe_Values.Look(ref rape_attempt_alert, "rape_attempt_alert");
			Scribe_Values.Look(ref rape_alert, "rape_alert");
			Scribe_Values.Look(ref ShowForCP, "ShowForCP");
			Scribe_Values.Look(ref ShowForBreeding, "ShowForBreeding");
			Scribe_Values.Look(ref sexuality_distribution, "sexuality_distribution");
			Scribe_Values.Look(ref Malesex, "Malesex");
			Scribe_Values.Look(ref FeMalesex, "FeMalesex");
			Scribe_Values.Look(ref MaxQuirks, "MaxQuirks");
		}
	}
}

using System;
using UnityEngine;
using Verse;

namespace rjw
{
	public class RJWPregnancySettings : ModSettings
	{
		public static bool humanlike_pregnancy_enabled = true;
		public static bool animal_pregnancy_enabled = true;
		public static bool bestial_pregnancy_enabled = true;
		public static bool insect_pregnancy_enabled = true;
		public static bool egg_pregnancy_implant_anyone = true;
		public static bool egg_pregnancy_fertilize_anyone = false;
		public static bool mechanoid_pregnancy_enabled = true;

		public static bool trait_filtering_enabled = true;
		public static bool use_parent_method = true;
		public static bool complex_interspecies = true;

		public static int animal_impregnation_chance = 25;
		public static int humanlike_impregnation_chance = 25;
		public static float interspecies_impregnation_modifier = 0.2f;
		public static float humanlike_DNA_from_mother = 0.5f;
		public static float bestial_DNA_from_mother = 1.0f;
		public static float bestiality_DNA_inheritance = 0.5f;
		public static float fertility_endage_male = 1.2f;
		public static float fertility_endage_female_humanlike = 0.58f;
		public static float fertility_endage_female_animal = 0.96f;


		private static Vector2 scrollPosition;
		private static float height_modifier = 100f;

		public static void DoWindowContents(Rect inRect)
		{

			//30f for top page description and bottom close button
			Rect outRect = new Rect(0f, 30f, inRect.width, inRect.height - 30f);
			//-16 for slider, height_modifier - additional height for options
			Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + height_modifier);

			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.maxOneColumn = true;
			listingStandard.ColumnWidth = viewRect.width / 2.05f;
			listingStandard.BeginScrollView(outRect, ref scrollPosition, ref viewRect);
			listingStandard.Begin(viewRect);
			listingStandard.Gap(4f);
			listingStandard.CheckboxLabeled("RJWH_pregnancy".Translate(), ref humanlike_pregnancy_enabled, "RJWH_pregnancy_desc".Translate());
			if (humanlike_pregnancy_enabled)
			{
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("  " + "genetic_trait_filter".Translate(), ref trait_filtering_enabled, "genetic_trait_filter_desc".Translate());
			}
			else
			{
				trait_filtering_enabled = false;
			}
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("RJWA_pregnancy".Translate(), ref animal_pregnancy_enabled, "RJWA_pregnancy_desc".Translate());
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("RJWB_pregnancy".Translate(), ref bestial_pregnancy_enabled, "RJWB_pregnancy_desc".Translate());
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("RJWI_pregnancy".Translate(), ref insect_pregnancy_enabled, "RJWI_pregnancy_desc".Translate());
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("egg_pregnancy_implant_anyone".Translate(), ref egg_pregnancy_implant_anyone, "egg_pregnancy_implant_anyone_desc".Translate());
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("egg_pregnancy_fertilize_anyone".Translate(), ref egg_pregnancy_fertilize_anyone, "egg_pregnancy_fertilize_anyone_desc".Translate());
			listingStandard.Gap(12f);

			listingStandard.CheckboxLabeled("UseParentMethod".Translate(), ref use_parent_method, "UseParentMethod_desc".Translate());
			listingStandard.Gap(5f);
			if (use_parent_method)
			{
				if (humanlike_DNA_from_mother == 0.0f)
				{
					listingStandard.Label("  " + "OffspringLookLikeTheirMother".Translate() + ": " + "AlwaysFather".Translate(), -1f, "OffspringLookLikeTheirMother_desc".Translate());
					humanlike_DNA_from_mother = listingStandard.Slider(humanlike_DNA_from_mother, 0.0f, 1.0f);
				}
				else if (humanlike_DNA_from_mother == 1.0f)
				{
					listingStandard.Label("  " + "OffspringLookLikeTheirMother".Translate() + ": " + "AlwaysMother".Translate(), -1f, "OffspringLookLikeTheirMother_desc".Translate());
					humanlike_DNA_from_mother = listingStandard.Slider(humanlike_DNA_from_mother, 0.0f, 1.0f);
				}
				else
				{
					int value = (int)(humanlike_DNA_from_mother * 100);
					listingStandard.Label("  " + "OffspringLookLikeTheirMother".Translate() + ": " + value + "%", -1f, "OffspringLookLikeTheirMother_desc".Translate());
					humanlike_DNA_from_mother = listingStandard.Slider(humanlike_DNA_from_mother, 0.0f, 1.0f);
				}

				if (bestial_DNA_from_mother == 0.0f)
				{
					listingStandard.Label("  " + "OffspringIsHuman".Translate() + ": " + "AlwaysFather".Translate(), -1f, "OffspringIsHuman_desc".Translate());
					bestial_DNA_from_mother = listingStandard.Slider(bestial_DNA_from_mother, 0.0f, 1.0f);
				}
				else if (bestial_DNA_from_mother == 1.0f)
				{
					listingStandard.Label("  " + "OffspringIsHuman".Translate() + ": " + "AlwaysMother".Translate(), -1f, "OffspringIsHuman_desc".Translate());
					bestial_DNA_from_mother = listingStandard.Slider(bestial_DNA_from_mother, 0.0f, 1.0f);
				}
				else
				{
					int value = (int)(bestial_DNA_from_mother * 100);
					listingStandard.Label("  " + "OffspringIsHuman".Translate() + ": " + value + "%", -1f, "OffspringIsHuman_desc".Translate());
					bestial_DNA_from_mother = listingStandard.Slider(bestial_DNA_from_mother, 0.0f, 1.0f);
				}
				
				if (bestiality_DNA_inheritance == 0.0f)
				{
					listingStandard.Label("  " + "OffspringIsHuman2".Translate() + ": " + "AlwaysBeast".Translate(), -1f, "OffspringIsHuman2_desc".Translate());
					bestiality_DNA_inheritance = listingStandard.Slider(bestiality_DNA_inheritance, 0.0f, 1.0f);
				}
				else if (bestiality_DNA_inheritance == 1.0f)
				{
					listingStandard.Label("  " + "OffspringIsHuman2".Translate() + ": " + "AlwaysHumanlike".Translate(), -1f, "OffspringIsHuman2_desc".Translate());
					bestiality_DNA_inheritance = listingStandard.Slider(bestiality_DNA_inheritance, 0.0f, 1.0f);
				}
				else
				{
					listingStandard.Label("  " + "OffspringIsHuman2".Translate() + ": <--->", -1f, "OffspringIsHuman2_desc".Translate());
					bestiality_DNA_inheritance = listingStandard.Slider(bestiality_DNA_inheritance, 0.0f, 1.0f);
				}
			}
			else
				humanlike_DNA_from_mother = 100;
			listingStandard.CheckboxLabeled("MechanoidImplanting".Translate(), ref mechanoid_pregnancy_enabled, "MechanoidImplanting_desc".Translate());
			listingStandard.Gap(5f);
			listingStandard.CheckboxLabeled("ComplexImpregnation".Translate(), ref complex_interspecies, "ComplexImpregnation_desc".Translate());
			listingStandard.Gap(10f);

			GUI.contentColor = Color.cyan;
			listingStandard.Label("Base pregnancy chances:");
			listingStandard.Gap(5f);
			if (humanlike_pregnancy_enabled)
				listingStandard.Label("  Humanlike/Humanlike (same race): " + humanlike_impregnation_chance + "%");
			else
				listingStandard.Label("  Humanlike/Humanlike (same race): -DISABLED-");
			if (humanlike_pregnancy_enabled && !(humanlike_impregnation_chance * interspecies_impregnation_modifier <= 0.0f) && !complex_interspecies)
				listingStandard.Label("  Humanlike/Humanlike (different race): " + Math.Round(humanlike_impregnation_chance * interspecies_impregnation_modifier, 1) + "%");
			else if (complex_interspecies)
				listingStandard.Label("  Humanlike/Humanlike (different race): -DEPENDS ON SPECIES-");
			else
				listingStandard.Label("  Humanlike/Humanlike (different race): -DISABLED-");
			if (animal_pregnancy_enabled)
				listingStandard.Label("  Animal/Animal (same race): " + animal_impregnation_chance + "%");
			else
				listingStandard.Label("  Animal/Animal (same race): -DISABLED-");
			if (animal_pregnancy_enabled && !(animal_impregnation_chance * interspecies_impregnation_modifier <= 0.0f) && !complex_interspecies)
				listingStandard.Label("  Animal/Animal (different race): " + Math.Round(animal_impregnation_chance * interspecies_impregnation_modifier, 1) + "%");
			else if (complex_interspecies)
				listingStandard.Label("  Animal/Animal (different race): -DEPENDS ON SPECIES-");
			else
				listingStandard.Label("  Animal/Animal (different race): -DISABLED-");
			if (RJWSettings.bestiality_enabled && bestial_pregnancy_enabled && !(animal_impregnation_chance * interspecies_impregnation_modifier <= 0.0f) && !complex_interspecies)
				listingStandard.Label("  Humanlike/Animal: " + Math.Round(animal_impregnation_chance * interspecies_impregnation_modifier, 1) + "%");
			else if (complex_interspecies)
				listingStandard.Label("  Humanlike/Animal: -DEPENDS ON SPECIES-");
			else
				listingStandard.Label("  Humanlike/Animal: -DISABLED-");
			if (RJWSettings.bestiality_enabled && bestial_pregnancy_enabled && !(animal_impregnation_chance * interspecies_impregnation_modifier <= 0.0f) && !complex_interspecies)
				listingStandard.Label("  Animal/Humanlike: " + Math.Round(humanlike_impregnation_chance * interspecies_impregnation_modifier, 1) + "%");
			else if (complex_interspecies)
				listingStandard.Label("  Animal/Humanlike: -DEPENDS ON SPECIES-");
			else
				listingStandard.Label("  Animal/Humanlike: -DISABLED-");
			GUI.contentColor = Color.white;

			listingStandard.NewColumn();
			listingStandard.Gap(4f);
			listingStandard.Label("PregnantCoeffecientForHuman".Translate() + ": " + humanlike_impregnation_chance + "%", -1f, "PregnantCoeffecientForHuman_desc".Translate());
			humanlike_impregnation_chance = (int)listingStandard.Slider(humanlike_impregnation_chance, 0.0f, 100f);
			listingStandard.Label("PregnantCoeffecientForAnimals".Translate() + ": " + animal_impregnation_chance + "%", -1f, "PregnantCoeffecientForAnimals_desc".Translate());
			animal_impregnation_chance = (int)listingStandard.Slider(animal_impregnation_chance, 0.0f, 100f);
			if (!complex_interspecies)
			{
				switch (interspecies_impregnation_modifier)
				{
					case 0.0f:
						GUI.contentColor = Color.grey;
						listingStandard.Label("InterspeciesImpregnantionModifier".Translate() + ": " + "InterspeciesDisabled".Translate(), -1f, "InterspeciesImpregnantionModifier_desc".Translate());
						GUI.contentColor = Color.white;
						break;
					case 1.0f:
						GUI.contentColor = Color.cyan;
						listingStandard.Label("InterspeciesImpregnantionModifier".Translate() + ": " + "InterspeciesMaximum".Translate(), -1f, "InterspeciesImpregnantionModifier_desc".Translate());
						GUI.contentColor = Color.white;
						break;
					default:
						listingStandard.Label("InterspeciesImpregnantionModifier".Translate() + ": " + Math.Round(interspecies_impregnation_modifier * 100, 1) + "%", -1f, "InterspeciesImpregnantionModifier_desc".Translate());
						break;
				}
				interspecies_impregnation_modifier = listingStandard.Slider(interspecies_impregnation_modifier, 0.0f, 1.0f);
			}
			listingStandard.Label("RJW_fertility_endAge_male".Translate() + ": " + (int)(fertility_endage_male * 80) + "In_human_years".Translate(), -1f, "RJW_fertility_endAge_male_desc".Translate());
			fertility_endage_male = listingStandard.Slider(fertility_endage_male, 0.1f, 3.0f);
			listingStandard.Label("RJW_fertility_endAge_female_humanlike".Translate() + ": " + (int)(fertility_endage_female_humanlike * 80) + "In_human_years".Translate(), -1f, "RJW_fertility_endAge_female_humanlike_desc".Translate());
			fertility_endage_female_humanlike = listingStandard.Slider(fertility_endage_female_humanlike, 0.1f, 3.0f);
			listingStandard.Label("RJW_fertility_endAge_female_animal".Translate() + ": " + (int)(fertility_endage_female_animal * 100) + "XofLifeExpectancy".Translate(), -1f, "RJW_fertility_endAge_female_animal_desc".Translate());
			fertility_endage_female_animal = listingStandard.Slider(fertility_endage_female_animal, 0.1f, 3.0f);

			listingStandard.EndScrollView(ref viewRect);
			listingStandard.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref humanlike_pregnancy_enabled, "humanlike_pregnancy_enabled");
			Scribe_Values.Look(ref animal_pregnancy_enabled, "animal_enabled");
			Scribe_Values.Look(ref bestial_pregnancy_enabled, "bestial_pregnancy_enabled");
			Scribe_Values.Look(ref insect_pregnancy_enabled, "insect_pregnancy_enabled");
			Scribe_Values.Look(ref egg_pregnancy_implant_anyone, "egg_pregnancy_implant_anyone");
			Scribe_Values.Look(ref egg_pregnancy_fertilize_anyone, "egg_pregnancy_fertilize_anyone");
			Scribe_Values.Look(ref mechanoid_pregnancy_enabled, "mechanoid_enabled");
			Scribe_Values.Look(ref trait_filtering_enabled, "trait_filtering_enabled");
			Scribe_Values.Look(ref use_parent_method, "use_parent_method");
			Scribe_Values.Look(ref humanlike_DNA_from_mother, "humanlike_DNA_from_mother");
			Scribe_Values.Look(ref bestial_DNA_from_mother, "bestial_DNA_from_mother");
			Scribe_Values.Look(ref bestiality_DNA_inheritance, "bestiality_DNA_inheritance");
			Scribe_Values.Look(ref humanlike_impregnation_chance, "humanlike_impregnation_chance");
			Scribe_Values.Look(ref animal_impregnation_chance, "animal_impregnation_chance");
			Scribe_Values.Look(ref interspecies_impregnation_modifier, "interspecies_impregnation_chance");
			Scribe_Values.Look(ref complex_interspecies, "complex_interspecies");
			Scribe_Values.Look(ref fertility_endage_male, "RJW_fertility_endAge_male");
			Scribe_Values.Look(ref fertility_endage_female_humanlike, "fertility_endage_female_humanlike");
			Scribe_Values.Look(ref fertility_endage_female_animal, "fertility_endage_female_animal");
		}
	}
}
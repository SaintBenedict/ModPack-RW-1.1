using System;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace rjw
{
	public class RJWDebugSettings : ModSettings
	{
		public static void DoWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.ColumnWidth = inRect.width / 2.05f;
			listingStandard.Begin(inRect);
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("whoringtab_enabled".Translate(), ref RJWSettings.whoringtab_enabled);
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("submit_button_enabled".Translate(), ref RJWSettings.submit_button_enabled, "submit_button_enabled_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("RJW_designation_box".Translate(), ref RJWSettings.show_RJW_designation_box, "RJW_designation_box_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("designated_freewill".Translate(), ref RJWSettings.designated_freewill, "designated_freewill_desc".Translate());
				listingStandard.Gap(5f);
				if (listingStandard.ButtonText("Rjw Parts " + RJWSettings.ShowRjwParts))
				{
					Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
					{
					  new FloatMenuOption("Show", (() => RJWSettings.ShowRjwParts = RJWSettings.ShowParts.Show)),
					  //new FloatMenuOption("Known".Translate(), (() => RJWSettings.ShowRjwParts = RJWSettings.ShowParts.Known)),
					  new FloatMenuOption("Hide", (() => RJWSettings.ShowRjwParts = RJWSettings.ShowParts.Hide))
					}));
				}
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("StackRjwParts_name".Translate(), ref RJWSettings.StackRjwParts, "StackRjwParts_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.Label("maxDistancetowalk_name".Translate() + ": " + (RJWSettings.maxDistancetowalk), -1f, "maxDistancetowalk_desc".Translate());
				RJWSettings.maxDistancetowalk = listingStandard.Slider((int)RJWSettings.maxDistancetowalk, 0, 5000);
				listingStandard.Gap(30f);

			GUI.contentColor = Color.yellow;
			listingStandard.Label("YOU PATHETIC CHEATER ");
			GUI.contentColor = Color.white;
				listingStandard.CheckboxLabeled("override_RJW_designation_checks_name".Translate(), ref RJWSettings.override_RJW_designation_checks, "override_RJW_designation_checks_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("override_control".Translate(), ref RJWSettings.override_control, "override_control_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Rapist".Translate(), ref RJWSettings.AddTrait_Rapist, "AddTrait_Rapist_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Masocist".Translate(), ref RJWSettings.AddTrait_Masocist, "AddTrait_Masocist_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Nymphomaniac".Translate(), ref RJWSettings.AddTrait_Nymphomaniac, "AddTrait_Nymphomaniac_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Necrophiliac".Translate(), ref RJWSettings.AddTrait_Necrophiliac, "AddTrait_Necrophiliac_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Nerves".Translate(), ref RJWSettings.AddTrait_Nerves, "AddTrait_Nerves_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("AddTrait_Zoophiliac".Translate(), ref RJWSettings.AddTrait_Zoophiliac, "AddTrait_Zoophiliac_desc".Translate());
				listingStandard.Gap(5f);

			listingStandard.NewColumn();
				listingStandard.Gap(4f);
				GUI.contentColor = Color.yellow;
				listingStandard.CheckboxLabeled("ForbidKidnap".Translate(), ref RJWSettings.ForbidKidnap, "ForbidKidnap_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("override_lovin".Translate(), ref RJWSettings.override_lovin, "override_lovin_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("override_matin".Translate(), ref RJWSettings.override_matin, "override_matin_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("WildMode_name".Translate(), ref RJWSettings.WildMode, "WildMode_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("GenderlessAsFuta_name".Translate(), ref RJWSettings.GenderlessAsFuta, "GenderlessAsFuta_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("DevMode_name".Translate(), ref RJWSettings.DevMode, "DevMode_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("DebugLogJoinInBed".Translate(), ref RJWSettings.DebugLogJoinInBed, "DebugLogJoinInBed_desc".Translate());
				listingStandard.Gap(5f);
				listingStandard.CheckboxLabeled("DebugWhoring".Translate(), ref RJWSettings.DebugWhoring, "DebugWhoring_desc".Translate());
				listingStandard.Gap(5f);
				GUI.contentColor = Color.white;
				
			listingStandard.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref RJWSettings.whoringtab_enabled, "whoringtab_enabled");
			Scribe_Values.Look(ref RJWSettings.submit_button_enabled, "submit_button_enabled");
			Scribe_Values.Look(ref RJWSettings.show_RJW_designation_box, "show_RJW_designation_box");
			Scribe_Values.Look(ref RJWSettings.ShowRjwParts, "ShowRjwParts");
			Scribe_Values.Look(ref RJWSettings.StackRjwParts, "StackRjwParts");
			Scribe_Values.Look(ref RJWSettings.maxDistancetowalk, "maxDistancetowalk");

			Scribe_Values.Look(ref RJWSettings.AddTrait_Rapist, "AddTrait_Rapist");
			Scribe_Values.Look(ref RJWSettings.AddTrait_Masocist, "AddTrait_Masocist");
			Scribe_Values.Look(ref RJWSettings.AddTrait_Nymphomaniac, "AddTrait_Nymphomaniac");
			Scribe_Values.Look(ref RJWSettings.AddTrait_Necrophiliac, "AddTrait_Necrophiliac");
			Scribe_Values.Look(ref RJWSettings.AddTrait_Nerves, "AddTrait_Nerves");
			Scribe_Values.Look(ref RJWSettings.AddTrait_Zoophiliac, "AddTrait_Zoophiliac");

			Scribe_Values.Look(ref RJWSettings.ForbidKidnap, "ForbidKidnap", false, true);
			Scribe_Values.Look(ref RJWSettings.GenderlessAsFuta, "GenderlessAsFuta");
			Scribe_Values.Look(ref RJWSettings.override_lovin, "override_lovin");
			Scribe_Values.Look(ref RJWSettings.override_matin, "override_mayin");
			Scribe_Values.Look(ref RJWSettings.WildMode, "Wildmode");
			Scribe_Values.Look(ref RJWSettings.override_RJW_designation_checks, "override_RJW_designation_checks");
			Scribe_Values.Look(ref RJWSettings.override_control, "override_control");
			Scribe_Values.Look(ref RJWSettings.DevMode, "DevMode");
			Scribe_Values.Look(ref RJWSettings.DebugLogJoinInBed, "DebugLogJoinInBed");
			Scribe_Values.Look(ref RJWSettings.DebugWhoring, "DebugWhoring");
		}
	}
}

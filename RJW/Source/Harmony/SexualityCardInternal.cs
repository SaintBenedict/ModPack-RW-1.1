using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;

namespace rjw
{
	//TODO: check slime stuff working in mp
	//TODO: separate menus in sub scripts
	//TODO: add demon switch parts
	//TODO: figure out and make interface?
	//TODO: add vibration toggle for bionics(+50% partner satisfaction?)
	public class Dialog_Sexcard : Window
	{
		private readonly Pawn pawn;

		public Dialog_Sexcard(Pawn editFor)
		{
			pawn = editFor;
		}

		public static breasts Breasts;
		public enum breasts
		{
			selectone,
			none,
			featureless_chest,
			flat_breasts,
			small_breasts,
			average_breasts,
			large_breasts,
			huge_breasts,
			slime_breasts,
		};

		public static anuses Anuses;
		public enum anuses
		{
			selectone,
			none,
			micro_anus,
			tight_anus,
			average_anus,
			loose_anus,
			gaping_anus,
			slime_anus,
		};

		public static vaginas Vaginas;
		public enum vaginas
		{
			selectone,
			none,
			micro_vagina,
			tight_vagina,
			average_vagina,
			loose_vagina,
			gaping_vagina,
			slime_vagina,
			feline_vagina,
			canine_vagina,
			equine_vagina,
			dragon_vagina,
		};

		public static penises Penises;
		public enum penises
		{
			selectone,
			none,
			micro_penis,
			small_penis,
			average_penis,
			big_penis,
			huge_penis,
			slime_penis,
			feline_penis,
			canine_penis,
			equine_penis,
			dragon_penis,
			raccoon_penis,
			hemipenis,
			crocodilian_penis,
		};

		public void SexualityCard(Rect rect, Pawn pawn)
		{
			CompRJW comp = pawn.TryGetComp<CompRJW>();
			if (pawn == null || comp == null) return;

			Text.Font = GameFont.Medium;
			Rect rect1 = new Rect(8f, 4f, rect.width - 8f, rect.height - 20f);
			Widgets.Label(rect1, "RJW");//rjw

			Text.Font = GameFont.Tiny;
			float num = rect1.y + 40f;
			Rect row1 = new Rect(10f, num, rect.width - 8f, 24f);//sexuality
			Rect row2 = new Rect(10f, num + 24, rect.width - 8f, 24f);//quirks
			Rect row3 = new Rect(10f, num + 48, rect.width - 8f, 24f);//whore price

			//Rect sexuality_button = new Rect(10f, rect1.height - 0f, rect.width - 8f, 24f);//change sex pref
			Rect button1 = new Rect(10f, rect1.height - 10f, rect.width - 8f, 24f);//re sexualize
			Rect button2 = new Rect(10f, rect1.height - 34f, rect.width - 8f, 24f);//archtech toggle
			Rect button3 = new Rect(10f, rect1.height - 58f, rect.width - 8f, 24f);//breast
			Rect button4 = new Rect(10f, rect1.height - 82f, rect.width - 8f, 24f);//anus
			Rect button5 = new Rect(10f, rect1.height - 106f, rect.width - 8f, 24f);//vagina
			Rect button6 = new Rect(10f, rect1.height - 130f, rect.width - 8f, 24f);//penis 1
			Rect button7 = new Rect(10f, rect1.height - 154f, rect.width - 8f, 24f);//penis 2

			DrawSexuality(pawn, row1);
			DrawQuirks(pawn, row2);
			DrawWhoring(pawn, row3);

			// TODO: Add translations. or not
			if (Prefs.DevMode || Current.ProgramState != ProgramState.Playing)
			{
				if (Widgets.ButtonText(button1, Current.ProgramState != ProgramState.Playing ? "Reroll sexuality" : "[DEV] Reroll sexuality"))
				{
					Re_sexualize(pawn);
				}
			}
			if (pawn.health.hediffSet.HasHediff(Genital_Helper.archotech_penis) || pawn.health.hediffSet.HasHediff(Genital_Helper.archotech_vagina))
			{
				if (pawn.health.hediffSet.HasHediff(HediffDef.Named("ImpregnationBlocker")))
				{
					if (Widgets.ButtonText(button2, "[Archotech genitalia] Enable reproduction"))
					{
						Change_Archotechmode(pawn);
					}
				}
				else if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("FertilityEnhancer")))
				{
					if (Widgets.ButtonText(button2, "[Archotech genitalia] Enchance fertility"))
					{
						Change_Archotechmode(pawn);
					}
				}
				else if (Widgets.ButtonText(button2, "[Archotech genitalia] Disable reproduction"))
				{
					Change_Archotechmode(pawn);
				}
			}
			// TODO: add mp synchronizers
			// TODO: clean that mess
			// TODO: add demon toggles
			if (MP.IsInMultiplayer)
				return;

			//List<String> Parts = null;
			//if (xxx.is_slime(pawn))
			//	Parts = new List<string>(DefDatabase<StringListDef>.GetNamed("SlimeMorphFilters").strings);
			//if (xxx.is_demon(pawn))
			//	Parts = new List<string>(DefDatabase<StringListDef>.GetNamed("DemonMorphFilters").strings);
			// TODO: replace that mess with floating menu
			//if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && Mouse.IsOver(rect))
			//{
				//Event.current.Use();//?
				//DrawMedOperationsTab(0, pawn, Parts,0);
			//}

			//if (Parts.Any() && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony))
			//if (xxx.is_slime(pawn) && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony))
			//{
			//	BodyPartRecord bpr_genitalia = Genital_Helper.get_genitals(pawn);
			//	BodyPartRecord bpr_breasts = Genital_Helper.get_breasts(pawn);
			//	BodyPartRecord bpr_anus = Genital_Helper.get_anus(pawn);
			//	BodyPartRecord bpr = null;
			//	HediffDef hed = null;

			//	if (Widgets.ButtonText(button3, "Morph breasts"))
			//	{
			//		Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
			//	{
			//		new FloatMenuOption("none", (() => Breasts = breasts.none), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.featureless_chest.label.CapitalizeFirst(), (() => Breasts = breasts.featureless_chest), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.flat_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.flat_breasts), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.small_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.small_breasts), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.average_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.average_breasts), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.large_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.large_breasts), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.huge_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.huge_breasts), MenuOptionPriority.Default),
			//		new FloatMenuOption(Genital_Helper.slime_breasts.label.CapitalizeFirst(), (() => Breasts = breasts.slime_breasts), MenuOptionPriority.Default),
			//	}));
			//	}
			//	switch (Breasts)
			//	{
			//		case breasts.none:
			//			bpr = bpr_breasts;
			//			break;
			//		case breasts.featureless_chest:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.featureless_chest;
			//			break;
			//		case breasts.flat_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.flat_breasts;
			//			break;
			//		case breasts.small_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.small_breasts;
			//			break;
			//		case breasts.average_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.average_breasts;
			//			break;
			//		case breasts.large_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.large_breasts;
			//			break;
			//		case breasts.huge_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.huge_breasts;
			//			break;
			//		case breasts.slime_breasts:
			//			bpr = bpr_breasts;
			//			hed = Genital_Helper.slime_breasts;
			//			break;
			//		default:
			//			break;
			//	}

			//	if (Widgets.ButtonText(button4, "Morph anus"))
			//	{
			//		Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
			//		{
			//			new FloatMenuOption("none", (() => Anuses = anuses.none), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.micro_anus.label.CapitalizeFirst(), (() => Anuses = anuses.micro_anus), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.tight_anus.label.CapitalizeFirst(), (() => Anuses = anuses.tight_anus), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.average_anus.label.CapitalizeFirst(), (() => Anuses = anuses.average_anus), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.loose_anus.label.CapitalizeFirst(), (() => Anuses = anuses.loose_anus), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.gaping_anus.label.CapitalizeFirst(), (() => Anuses = anuses.gaping_anus), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.slime_anus.label.CapitalizeFirst(), (() => Anuses = anuses.slime_anus), MenuOptionPriority.Default),
			//		}));
			//	}
			//	switch (Anuses)
			//	{
			//		case anuses.none:
			//			bpr = bpr_anus;
			//			break;
			//		case anuses.micro_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.micro_anus;
			//			break;
			//		case anuses.tight_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.tight_anus;
			//			break;
			//		case anuses.average_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.average_anus;
			//			break;
			//		case anuses.loose_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.loose_anus;
			//			break;
			//		case anuses.gaping_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.gaping_anus;
			//			break;
			//		case anuses.slime_anus:
			//			bpr = bpr_anus;
			//			hed = Genital_Helper.slime_anus;
			//			break;
			//		default:
			//			break;
			//	}

			//	if (Widgets.ButtonText(button5, "Morph vagina"))
			//	{
			//		Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
			//		{
			//			new FloatMenuOption("none", (() => Vaginas = vaginas.none), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.micro_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.micro_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.tight_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.tight_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.average_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.average_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.loose_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.loose_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.gaping_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.gaping_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.slime_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.slime_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.feline_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.feline_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.canine_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.canine_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.equine_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.equine_vagina), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.dragon_vagina.label.CapitalizeFirst(), (() => Vaginas = vaginas.dragon_vagina), MenuOptionPriority.Default),
			//		}));
			//	}
			//	switch (Vaginas)
			//	{
			//		case vaginas.none:
			//			bpr = bpr_genitalia;
			//			break;
			//		case vaginas.micro_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.micro_vagina;
			//			break;
			//		case vaginas.tight_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.tight_vagina;
			//			break;
			//		case vaginas.average_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.average_vagina;
			//			break;
			//		case vaginas.loose_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.loose_vagina;
			//			break;
			//		case vaginas.gaping_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.gaping_vagina;
			//			break;
			//		case vaginas.slime_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.slime_vagina;
			//			break;
			//		case vaginas.feline_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.feline_vagina;
			//			break;
			//		case vaginas.canine_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.canine_vagina;
			//			break;
			//		case vaginas.equine_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.equine_vagina;
			//			break;
			//		case vaginas.dragon_vagina:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.dragon_vagina;
			//			break;
			//		default:
			//			break;
			//	}

			//	if (Widgets.ButtonText(button6, "Morph penis"))
			//	{
			//		Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()
			//		{
			//			new FloatMenuOption("none", (() => Penises = penises.none), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.micro_penis.label.CapitalizeFirst(), (() => Penises = penises.micro_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.small_penis.label.CapitalizeFirst(), (() => Penises = penises.small_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.average_penis.label.CapitalizeFirst(), (() => Penises = penises.average_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.big_penis.label.CapitalizeFirst(), (() => Penises = penises.big_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.huge_penis.label.CapitalizeFirst(), (() => Penises = penises.huge_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.slime_penis.label.CapitalizeFirst(), (() => Penises = penises.slime_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.feline_penis.label.CapitalizeFirst(), (() => Penises = penises.feline_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.canine_penis.label.CapitalizeFirst(), (() => Penises = penises.canine_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.equine_penis.label.CapitalizeFirst(), (() => Penises = penises.equine_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.dragon_penis.label.CapitalizeFirst(), (() => Penises = penises.dragon_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.raccoon_penis.label.CapitalizeFirst(), (() => Penises = penises.raccoon_penis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.hemipenis.label.CapitalizeFirst(), (() => Penises = penises.hemipenis), MenuOptionPriority.Default),
			//			new FloatMenuOption(Genital_Helper.crocodilian_penis.label.CapitalizeFirst(), (() => Penises = penises.crocodilian_penis), MenuOptionPriority.Default),
			//		}));
			//	}
			//	switch (Penises)
			//	{
			//		case penises.none:
			//			bpr = bpr_genitalia;
			//			break;
			//		case penises.micro_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.micro_penis;
			//			break;
			//		case penises.small_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.small_penis;
			//			break;
			//		case penises.average_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.average_penis;
			//			break;
			//		case penises.big_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.big_penis;
			//			break;
			//		case penises.huge_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.huge_penis;
			//			break;
			//		case penises.slime_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.slime_penis;
			//			break;
			//		case penises.feline_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.feline_penis;
			//			break;
			//		case penises.canine_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.canine_penis;
			//			break;
			//		case penises.equine_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.equine_penis;
			//			break;
			//		case penises.dragon_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.dragon_penis;
			//			break;
			//		case penises.raccoon_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.raccoon_penis;
			//			break;
			//		case penises.hemipenis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.hemipenis;
			//			break;
			//		case penises.crocodilian_penis:
			//			bpr = bpr_genitalia;
			//			hed = Genital_Helper.crocodilian_penis;
			//			break;
			//		default:
			//			break;
			//	}

			//	if (bpr != null)
			//	{
			//		//Log.Message("start ");
			//		if (bpr != bpr_genitalia)
			//		{
			//			if (pawn.needs.TryGetNeed<Need_Food>().CurLevel > 0.5f)
			//			{
			//				pawn.needs.food.CurLevel -= 0.5f;
			//				pawn.health.RestorePart(bpr);
			//				if (hed != null)
			//					pawn.health.AddHediff(hed, bpr);
			//			}
			//			Anuses = anuses.selectone;
			//			Breasts = breasts.selectone;
			//		}
			//		else if (bpr == bpr_genitalia && Vaginas != vaginas.selectone)
			//		{
			//			if (pawn.needs.TryGetNeed<Need_Food>().CurLevel > 0.5f)
			//			{
			//				pawn.needs.food.CurLevel -= 0.5f;
			//				List<Hediff> list = new List<Hediff>();
			//				foreach (Hediff heddif in pawn.health.hediffSet.hediffs.Where(x =>
			//						x.Part == bpr &&
			//						x.def.defName.ToLower().Contains("vagina")))
			//					list.Add(heddif);

			//				foreach (Hediff heddif in list)
			//					pawn.health.hediffSet.hediffs.Remove(heddif);

			//				if (hed != null)
			//					pawn.health.AddHediff(hed, bpr);
			//			}
			//			Vaginas = vaginas.selectone;
			//		}
			//		else if (bpr == bpr_genitalia && Penises != penises.selectone)
			//		{
			//			if (pawn.needs.TryGetNeed<Need_Food>().CurLevel > 0.5f)
			//			{
			//				pawn.needs.food.CurLevel -= 0.5f;
			//				List<Hediff> list = new List<Hediff>();
			//				foreach (Hediff heddif in pawn.health.hediffSet.hediffs.Where(x =>
			//						x.Part == bpr &&
			//						x.def.defName.ToLower().Contains("penis") ||
			//						x.def.defName.ToLower().Contains("tentacle")))
			//					list.Add(heddif);

			//				foreach (Hediff heddif in list)
			//					pawn.health.hediffSet.hediffs.Remove(heddif);

			//				if (hed != null)
			//					pawn.health.AddHediff(hed, bpr);
			//			}
			//			Penises = penises.selectone;
			//		}
			//		//Log.Message("end ");
			//	}
			//}
		}

		static void DrawSexuality(Pawn pawn, Rect row)
		{
			string sexuality;

			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Vanilla)
				CompRJW.VanillaTraitCheck(pawn);
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.SYRIndividuality)
				CompRJW.CopyIndividualitySexuality(pawn);
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Psychology)
				CompRJW.CopyPsychologySexuality(pawn);

			switch (CompRJW.Comp(pawn).orientation)
			{
				case Orientation.Asexual:
					sexuality = "Asexual";
					break;
				case Orientation.Bisexual:
					sexuality = "Bisexual";
					break;
				case Orientation.Heterosexual:
					sexuality = "Hetero";
					break;
				case Orientation.Homosexual:
					sexuality = "Gay";
					break;
				case Orientation.LeaningHeterosexual:
					sexuality = "Bisexual, leaning hetero";
					break;
				case Orientation.LeaningHomosexual:
					sexuality = "Bisexual, leaning gay";
					break;
				case Orientation.MostlyHeterosexual:
					sexuality = "Mostly hetero";
					break;
				case Orientation.MostlyHomosexual:
					sexuality = "Mostly gay";
					break;
				case Orientation.Pansexual:
					sexuality = "Pansexual";
					break;
				default:
					sexuality = "None";
					break;
			}
			
			//allow to change pawn sexuality for:
			//own hero, game start
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.RimJobWorld &&
				(((Current.ProgramState == ProgramState.Playing &&
				pawn.IsDesignatedHero() && pawn.IsHeroOwner()) ||
				Prefs.DevMode) ||
				Current.ProgramState == ProgramState.Entry))

			{
				if (Widgets.ButtonText(row, "Sexuality: " + sexuality, false))
				{
					Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>()			//this needs fixing in 1.1 with vanilla orientation traits
					{
						new FloatMenuOption("Asexual", (() => Change_orientation(pawn, Orientation.Asexual)), MenuOptionPriority.Default),
						new FloatMenuOption("Pansexual", (() => Change_orientation(pawn, Orientation.Pansexual)), MenuOptionPriority.Default),
						new FloatMenuOption("Heterosexual", (() => Change_orientation(pawn, Orientation.Heterosexual)), MenuOptionPriority.Default),
						new FloatMenuOption("MostlyHeterosexual", (() => Change_orientation(pawn, Orientation.MostlyHeterosexual)), MenuOptionPriority.Default),
						new FloatMenuOption("LeaningHeterosexual", (() => Change_orientation(pawn, Orientation.LeaningHeterosexual)), MenuOptionPriority.Default),
						new FloatMenuOption("Bisexual", (() => Change_orientation(pawn, Orientation.Bisexual)), MenuOptionPriority.Default),
						new FloatMenuOption("LeaningHomosexual", (() => Change_orientation(pawn, Orientation.LeaningHomosexual)), MenuOptionPriority.Default),
						new FloatMenuOption("MostlyHomosexual", (() => Change_orientation(pawn, Orientation.MostlyHomosexual)), MenuOptionPriority.Default),
						new FloatMenuOption("Homosexual", (() => Change_orientation(pawn, Orientation.Homosexual)), MenuOptionPriority.Default),
					}));
				}
			}
			else
			{
				Widgets.Label(row, "Sexuality: " + sexuality);
				if (Mouse.IsOver(row))
					Widgets.DrawHighlight(row);
			}
		}

		static void DrawQuirks(Pawn pawn, Rect row)
		{

			var quirks = Quirk.All
				.Where(quirk => pawn.Has(quirk))
				.OrderBy(quirk => quirk.Key)
				.ToList();

			// Not actually localized.
			var quirkString = quirks.Select(quirk => quirk.Key).ToCommaList();
			
			if ((Current.ProgramState == ProgramState.Playing &&
				pawn.IsDesignatedHero() && pawn.IsHeroOwner() ||
				Prefs.DevMode) ||
				Current.ProgramState == ProgramState.Entry)

			{
				var quirksAll = Quirk.All
								.OrderBy(quirk => quirk.Key)
								.ToList();

				if (!RJWSettings.DevMode)
				{
					quirksAll.Remove(Quirk.Breeder);
					quirksAll.Remove(Quirk.Incubator);
				}

				if (xxx.is_insect(pawn))
					quirksAll.Add(Quirk.Incubator);

				if (Widgets.ButtonText(row, "Quirks".Translate() + quirkString, false))
				{
					var list = new List<FloatMenuOption>();
					list.Add(new FloatMenuOption("Reset", (() => QuirkAdder.Clear(pawn)), MenuOptionPriority.Default));
					foreach (Quirk quirk in quirksAll)
					{
						list.Add(new FloatMenuOption(quirk.Key, (() => QuirkAdder.Add(pawn, quirk))));
						//TODO: fix quirk description box in 1.1 menus
						//list.Add(new FloatMenuOption(quirk.Key, (() => QuirkAdder.Add(pawn, quirk)), MenuOptionPriority.Default, delegate 
						//{
						//	TooltipHandler.TipRegion(row, quirk.LocaliztionKey.Translate(pawn.Named("pawn")));
						//}
						//));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			}
			else
			{
				// TODO: long quirk list support
				// This can be too long and line wrap.
				// Should probably be a vertical list like traits.
				Widgets.Label(row, "Quirks".Translate() + quirkString);

			}

			if (!Mouse.IsOver(row)) return;
			
			Widgets.DrawHighlight(row);
			if (quirks.NullOrEmpty())
			{
				TooltipHandler.TipRegion(row, "NoQuirks".Translate());
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (var q in quirks)
				{
					stringBuilder.AppendLine(q.Key.Colorize(Color.yellow));
					stringBuilder.AppendLine(q.LocaliztionKey.Translate(pawn.Named("pawn")).AdjustedFor(pawn).Resolve());
					stringBuilder.AppendLine("");
				}
				string str = stringBuilder.ToString().TrimEndNewlines();
				TooltipHandler.TipRegion(row, str);
			}
		}

		static string DrawWhoring(Pawn pawn, Rect row)
		{
			string price;
			if (pawn.ageTracker.AgeBiologicalYears < RJWSettings.sex_minimum_age)
				price = "Inapplicable (too young)";
			else if (pawn.ownership.OwnedRoom == null)
			{
				if (Current.ProgramState == ProgramState.Playing)
					price = WhoringHelper.WhoreMinPrice(pawn) + " - " + WhoringHelper.WhoreMaxPrice(pawn) + " (base, needs suitable bedroom)";
				else
					price = WhoringHelper.WhoreMinPrice(pawn) + " - " + WhoringHelper.WhoreMaxPrice(pawn) + " (base, modified by bedroom quality)";
			}
			else if (xxx.is_animal(pawn))
				price = "Incapable of whoring";
			else
				price = WhoringHelper.WhoreMinPrice(pawn) + " - " + WhoringHelper.WhoreMaxPrice(pawn);

			Widgets.Label(row, "WhorePrice".Translate() + price);
			if (Mouse.IsOver(row))
				Widgets.DrawHighlight(row);
			return price;
		}


		[SyncMethod]
		static void Change_orientation(Pawn pawn, Orientation orientation)
		{
			CompRJW.Comp(pawn).orientation = orientation;
		}

		[SyncMethod]
		static void Change_Archotechmode(Pawn pawn)
		{
			BodyPartRecord genitalia = Genital_Helper.get_genitalsBPR(pawn);
			Hediff blocker = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("ImpregnationBlocker"));
			Hediff enhancer = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("FertilityEnhancer"));

			if (blocker != null)
			{
				pawn.health.RemoveHediff(blocker);
			}
			else if (enhancer == null)
			{
				pawn.health.AddHediff(HediffDef.Named("FertilityEnhancer"), genitalia);
			}
			else 
			{
				if (enhancer != null)
					pawn.health.RemoveHediff(enhancer);
				pawn.health.AddHediff(HediffDef.Named("ImpregnationBlocker"), genitalia);
			}
		}

		[SyncMethod]
		static void Re_sexualize(Pawn pawn)
		{
			CompRJW.Comp(pawn).Sexualize(pawn, true);
		}

		public override void DoWindowContents(Rect inRect)
		{
			bool flag = false;
			soundClose = SoundDefOf.InfoCard_Close;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = false;
			forcePause = true;
			preventCameraMotion = false;
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
			{
				flag = true;
				Event.current.Use();
			}
			Rect windowRect = inRect.ContractedBy(17f);
			Rect mainRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height - 20f);
			Rect okRect = new Rect(inRect.width / 3, mainRect.yMax + 10f, inRect.width / 3f, 30f);
			SexualityCard(mainRect, pawn);
			if (Widgets.ButtonText(okRect, "CloseButton".Translate()) || flag)
			{
				Close();
			}
		}
	}
}
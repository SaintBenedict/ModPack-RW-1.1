namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_OperationDropDown : PawnColumnWorker
    {
        public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>
            (T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public static Func<Pawn, Thing, RecipeDef, IEnumerable<ThingDef>, BodyPartRecord, FloatMenuOption>
            GenerateSurgeryOptionFunc =
                (Func<Pawn, Thing, RecipeDef, IEnumerable<ThingDef>, BodyPartRecord, FloatMenuOption>) Delegate
                    .CreateDelegate(
                        typeof(Func<Pawn, Thing, RecipeDef, IEnumerable<ThingDef>, BodyPartRecord, FloatMenuOption>),
                        null,
                        typeof(HealthCardUtility).GetMethod("GenerateSurgeryOption",
                                                            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod) ?? throw new InvalidOperationException("GenerateSurgeryOption is null."));

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (Widgets.ButtonText(rect, "AddBill".Translate()))
            {
                Find.WindowStack.Add(new FloatMenu(RecipeOptionsMaker(pawn)));
            }

            UIHighlighter.HighlightOpportunity(rect, "AddBill");
        }

        public override int GetMinCellHeight(Pawn pawn)
        {
            return 30;
        }

        public override int GetMinWidth(PawnTable table)
        {
            return 150;
        }

        private static List<FloatMenuOption> RecipeOptionsMaker(Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (RecipeDef current in pawn.def.AllRecipes.Where(x => x.AvailableNow))
            {
                List<ThingDef> missingIngredientsList =
                    current.PotentiallyMissingIngredients(null, pawn.Map).ToList();
                if (missingIngredientsList.Count == 0 || (!current.dontShowIfAnyIngredientMissing &&
                                                          !missingIngredientsList.Any(x => x.isTechHediff || x.IsDrug)))
                {
                    if (current.targetsBodyPart)
                    {
                        list.AddRange(current.Worker.GetPartsToApplyOn(pawn, current).Select(current2 =>
                            GenerateSurgeryOptionFunc(pawn, pawn, current, missingIngredientsList, current2)));
                    }
                    else
                    {
                        list.Add(GenerateSurgeryOptionFunc(pawn, pawn, current, missingIngredientsList, null));
                    }
                }
            }

            return list;
        }
    }
}
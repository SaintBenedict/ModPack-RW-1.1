namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    public class PawnColumnWorker_Equipment : PawnColumnWorker
    {
        private int width;
        private static readonly int baseWidth = 6 * 28; //6 boxes, 28 wide each.
        private const float gWidth  = 28f;
        private const float gHeight = 28f;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            GUI.BeginGroup(rect);

            float x = 0;

            if (pawn.equipment != null)
            {
                foreach (ThingWithComps thing in pawn.equipment.AllEquipmentListForReading)
                {
                    Rect rect2 = new Rect(x, 0, gWidth, gHeight);
                    DrawThing(rect2, thing, pawn);
                    x += gWidth;
                }
            }

            if (pawn.apparel != null)
            {
                foreach (Apparel thing in pawn.apparel.WornApparel.OrderByDescending(ap => ap.def.apparel.bodyPartGroups[0].listOrder))
                {
                    Rect rect2 = new Rect(x, 0, gWidth, gHeight);
                    DrawThing(rect2, thing, pawn);
                    x += gWidth;
                    if (x > width)
                        width = (int)x;
                }
            }

            GUI.EndGroup();
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(width, baseWidth);

        private void DrawThing(Rect rect, Thing thing, Pawn selPawn)
        {
            if (Widgets.ButtonInvisible(rect) && Event.current.button == 1)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>
                {
                    new FloatMenuOption("ThingInfo".Translate(), () => Find.WindowStack.Add(new Dialog_InfoCard(thing)))
                };

                if (selPawn.IsColonistPlayerControlled)
                {
                    Action action = DropThing(thing, selPawn);
                    list.Add(new FloatMenuOption("DropThing".Translate(), action));
                }
                FloatMenu window = new FloatMenu(list, thing.LabelCap);
                Find.WindowStack.Add(window);
            }
            GUI.BeginGroup(rect);
            if (thing.def.DrawMatSingle?.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(3f, 3f, 27f, 27f), thing);
            }
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect, new TipSignal(thing.LabelCap));
        }

        private static Action DropThing(Thing thing, Pawn selPawn)
        {
            Action action = null;
            ThingWithComps eq = thing as ThingWithComps;
            if (thing is Apparel ap && selPawn.apparel != null && selPawn.apparel.WornApparel.Contains(ap))
            {
                action = delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.RemoveApparel, ap));
                };
            }
            else if (eq != null && selPawn.equipment.AllEquipmentListForReading.Contains(eq))
            {
                action = delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, eq));
                };
            }
            else if (!thing.def.destroyOnDrop)
            {
                action = delegate
                {
                    selPawn.inventory.innerContainer.TryDrop(thing, selPawn.Position, selPawn.Map, ThingPlaceMode.Near, out Thing unused);
                };
            }

            return action;
        }

        public override int Compare(Pawn a, Pawn b)
            => (a.equipment.HasAnything() ? a.equipment.AllEquipmentListForReading.First().LabelCap : string.Empty)
                .CompareTo(b.equipment.HasAnything() ? b.equipment.AllEquipmentListForReading.First().LabelCap : string.Empty);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace Compilatron
{
    public class CompProperties_PowerTracker : CompProperties
    {
        public CompProperties_PowerTracker()
        {
            compClass = typeof(CompPowerTracker);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            /* if (parentDef.tickerType == TickerType.Never || parentDef.tickerType == TickerType.Long)
            {
                yield return "{0} tickerType set to {1}, must be normal or rare".Formatted(parentDef.defName, parentDef.tickerType.ToString());
                parentDef.tickerType = TickerType.Rare;
            } */
            yield break;
        }
    }

    public class CompPowerTracker : ThingComp
    {
        public Dictionary<int, float> HistoricalPowerUsage = new Dictionary<int, float>();
        public Dictionary<int, bool> HistoricalUptime = new Dictionary<int, bool>();
        public Dictionary<int, bool> HistoricalUsetime = new Dictionary<int, bool>();

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref HistoricalPowerUsage, "HistoricalPowerUsage");
            Scribe_Collections.Look(ref HistoricalUptime, "HistoricalUptime");
            Scribe_Collections.Look(ref HistoricalUsetime, "HistoricalUsetime");
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (HistoricalPowerUsage == null) HistoricalPowerUsage = new Dictionary<int, float>();
            if (HistoricalUptime == null) HistoricalUptime = new Dictionary<int, bool>();
            if (HistoricalUsetime == null) HistoricalUsetime = new Dictionary<int, bool>();
        }

        public enum PowerType
        {
            None = 0,
            Producer = 1,
            Consumer = 2,
            Storage = 3
        }

        static public string[] powerTypeString = { "None", "Producers", "Consumers", "Storage" };

        static public PowerType powerTypeFor(ThingDef def)
        {
            CompProperties_Battery powerBattery = def.GetCompProperties<CompProperties_Battery>();
            if (powerBattery != null)
                return PowerType.Storage;

            CompProperties_Power power = def.GetCompProperties<CompProperties_Power>();
            if (power != null)
                return power.basePowerConsumption > 0 ? PowerType.Consumer : PowerType.Producer;

            return PowerType.None;
        }
/*
        public override void CompTick()
        {
            if (GenTicks.TicksGame % GenTicks.TickLongInterval == 0)
            {
                HistoricalPowerUsage.Add(GenTicks.TicksGame, PowerUsage);
                HistoricalUptime.Add(GenTicks.TicksGame, Uptime);
                HistoricalUsetime.Add(GenTicks.TicksGame, Usetime);
            }
        }

        public override void CompTickRare()
        {
            if (GenTicks.TicksGame % GenTicks.TickLongInterval == 0)
            {
                HistoricalPowerUsage.Add(GenTicks.TicksGame, PowerUsage);
                HistoricalUptime.Add(GenTicks.TicksGame, Uptime);
                HistoricalUsetime.Add(GenTicks.TicksGame, Usetime);
            }
        }
*/
        public override void Initialize(CompProperties props)
        {
            CompPowerBattery powerBattery = parent.GetComp<CompPowerBattery>();
            if (powerBattery != null)
                intMaxPowerUsage = Math.Max(Math.Abs(powerBattery.Props.storedEnergyMax), maxPowerUsage);

            CompPowerPlant powerPlant = parent.GetComp<CompPowerPlant>();
            if (powerPlant != null)
                intMaxPowerUsage = Math.Max(Math.Abs(powerPlant.Props.basePowerConsumption), maxPowerUsage);

            CompPowerTrader powerTrader = parent.GetComp<CompPowerTrader>();
            if (powerTrader != null)
                intMaxPowerUsage = Math.Max(Math.Abs(powerTrader.Props.basePowerConsumption), maxPowerUsage);
        }

        float intMaxPowerUsage = 1;

        public PowerType powerType
        {
            get
            {
                CompPowerBattery powerBattery = parent.GetComp<CompPowerBattery>();
                if (powerBattery != null)
                    return PowerType.Storage;

                CompPowerPlant powerPlant = parent.GetComp<CompPowerPlant>();
                if (powerPlant != null)
                    return PowerType.Producer;

                CompPowerTrader powerTrader = parent.GetComp<CompPowerTrader>();
                if (powerTrader != null)
                    return PowerType.Consumer;

                return PowerType.None;
            }
        }

        public float maxPowerUsage
        {
            get
            {
                if (Math.Abs(PowerUsage) > intMaxPowerUsage) intMaxPowerUsage = Math.Abs(PowerUsage);
                return intMaxPowerUsage;
            }
        }

        public float PowerUsage
        {
            get
            {

                CompPowerBattery powerBattery = parent.GetComp<CompPowerBattery>();
                if (powerBattery != null)
                    return powerBattery.StoredEnergy;

                CompPowerPlant powerPlant = parent.GetComp<CompPowerPlant>();
                if (powerPlant != null)
                    return powerPlant.PowerOn ? powerPlant.PowerOutput : 0;
                
                CompPowerTrader powerTrader = parent.GetComp<CompPowerTrader>();
                if (powerTrader != null)
                    return powerTrader.PowerOn ? powerTrader.PowerOutput : 0;

                return 0;
            }
        }

        public bool Uptime
        { 
            get {
                CompPowerTrader powerTrader = parent.GetComp<CompPowerTrader>();
                return powerTrader != null ? powerTrader.PowerOn : false;
            }
        }

        public bool Usetime
        {
            get
            {
                if (!parent.Spawned)
                    return false;

                CompDeepDrill deepDrill = parent.GetComp<CompDeepDrill>();
                if (deepDrill != null) return deepDrill.UsedLastTick();

                CompTempControl tempControl = parent.GetComp<CompTempControl>();
                if (tempControl != null) return tempControl.operatingAtHighPower;

                Building_AncientCryptosleepCasket ancientCryptosleepCasket = parent as Building_AncientCryptosleepCasket;
                if (ancientCryptosleepCasket != null) return ancientCryptosleepCasket.HasAnyContents;

                Building_CommsConsole commsConsole = parent as Building_CommsConsole;
                if (commsConsole != null) return true;

                Building_Door door = parent as Building_Door;
                if (door != null) return door.Open;

                Building_PlantGrower plantGrower = parent as Building_PlantGrower;
                if (plantGrower != null) return plantGrower.PlantsOnMe.Count() > 0;

                Building_Turret turret = parent as Building_Turret;
                if (turret != null) return turret.CurrentTarget.IsValid;

                // Building_WorkTable workTable = parent as Building_WorkTable;
                // if (workTable != null) return workTable.;

                // Building_NutrientPasteDispenser nutrientPasteDispenser = parent as Building_NutrientPasteDispenser;
                // if (nutrientPasteDispenser != null) return nutrientPasteDispenser;

                // Building_ResearchBench researchBench = parent as Building_ResearchBench;
                // if (researchBench != null) return researchBench;

                return parent.Map.physicalInteractionReservationManager.IsReserved(new LocalTargetInfo(parent));
            }
        }

        public PowerNet powerNet
        {
            get
            {
                CompPowerBattery powerBattery = parent.GetComp<CompPowerBattery>();
                if (powerBattery != null)
                    return powerBattery.PowerNet;

                CompPowerPlant powerPlant = parent.GetComp<CompPowerPlant>();
                if (powerPlant != null)
                    return powerPlant.PowerNet;

                CompPowerTrader powerTrader = parent.GetComp<CompPowerTrader>();
                if (powerTrader != null)
                    return powerTrader.PowerNet;

                CompPower power = parent.GetComp<CompPower>();
                if (power != null)
                    return power.PowerNet;

                return null;
            }
        }

        public void DrawGUI(ref float yref, float width, float maxPowerUsage)
        {
            GUI.BeginGroup(new Rect(GenUI.GapWide, yref, width - GenUI.Gap - GenUI.GapTiny * 2, GenUI.ListSpacing));
            Rect rect = new Rect(0, 0, width - GenUI.Gap * 2 - GenUI.ScrollBarWidth, GenUI.ListSpacing);
            Widgets.DrawHighlightIfMouseover(rect);
            Widgets.ThingIcon(rect.LeftPartPixels(rect.height), parent);
            if (Widgets.ButtonInvisible(rect)) CameraJumper.TryJumpAndSelect(new GlobalTargetInfo(parent));
            rect.xMin += rect.height;
            Widgets.Label(rect.LeftPartPixels(150 - GenUI.GapWide).ContractedBy(GenUI.GapTiny), parent.LabelShortCap);
            rect.xMin += 150 - GenUI.GapWide;
            Widgets.FillableBarLabeled(rect.ContractedBy(GenUI.GapTiny), Math.Abs(PowerUsage) / maxPowerUsage, 50, "Power");
            string Label = "{0}W".Formatted(PowerUsage.ToString("0"));
            float Width = GenUI.GetWidthCached(Label);
            rect = new RectOffset(-58, 0, -4, -4).Add(rect).LeftPartPixels(Width + 4);
            Widgets.DrawRectFast(new RectOffset(0, 4, -4, -4).Add(rect), Color.black);
            Widgets.Label(new RectOffset(-4, 4, 0, 0).Add(rect), Label);
            yref += GenUI.ListSpacing + GenUI.GapTiny;
            GUI.EndGroup();
        }
    }

    public class ITab_Power : ITab
    {
        public ITab_Power()
        {
            size = new Vector2(450f, 450f);
            labelKey = "PowerSwitch_Power";
        }

        protected override float PaneTopY => base.PaneTopY;

        Vector2 scrollPos;
        float lastY = 0f;
        Dictionary<ThingDef, bool> collapseTab = new Dictionary<ThingDef, bool>();

        protected override void FillTab()
        {
            CompPowerTracker compPower = SelThing.TryGetComp<CompPowerTracker>();
            if (compPower == null) return;
            PowerNet powerNet = compPower.powerNet;
            if (powerNet == null) return;

            Widgets.BeginScrollView(
                new Rect(default(Vector2), size).ContractedBy(GenUI.GapTiny),
                ref scrollPos,
                new Rect(default(Vector2), new Vector2(size.x - GenUI.GapTiny * 2 - GenUI.ScrollBarWidth, lastY))
            );

            float yref = 10;

            IEnumerable< IGrouping <CompPowerTracker.PowerType, IGrouping <ThingDef, CompPowerTracker>>> categories =
                powerNet.batteryComps.ConvertAll((c) => c.parent.GetComp<CompPowerTracker>())
                .Concat(powerNet.powerComps.ConvertAll((c) => c.parent.GetComp<CompPowerTracker>()))
                .GroupBy((c) => c.parent.def)
                .GroupBy((d) => CompPowerTracker.powerTypeFor(d.Key));

            foreach (IGrouping<CompPowerTracker.PowerType, IGrouping<ThingDef, CompPowerTracker>> type in categories) {
                
                float p = type.Sum((g) => g.Sum((t) => t.PowerUsage));
                float m = type.Sum((g) => g.Sum((t) => t.maxPowerUsage));

                Rect rect = new Rect(150, yref, size.x - 172, Text.SmallFontHeight);

                Widgets.FillableBarLabeled(rect, Math.Abs(p / m), 0, "");
                string Label = "{0}W".Formatted(p.ToString("0"));
                float Width = GenUI.GetWidthCached(Label);
                Widgets.DrawRectFast(new RectOffset(-4, -4, -4, -4).Add(rect).LeftPartPixels(Width + 4), Color.black);
                Widgets.Label(new RectOffset(-4, 0, 0, 0).Add(rect), Label);

                Widgets.ListSeparator(ref yref, rect.width, "{0}".Formatted(CompPowerTracker.powerTypeString[(int)type.Key]));

                float maxPowerUsage = type.Max((g) => g.Sum((c) => Math.Abs(c.maxPowerUsage)));

                yref += GenUI.GapTiny;

                foreach (IGrouping<ThingDef, CompPowerTracker> defs in type)
                {
                    // Begin group; All future GUI elements are relative to this group
                    GUI.BeginGroup(new Rect(0, yref, size.x, Text.SmallFontHeight + GenUI.GapTiny * 2));

                    // Make a rect that is the size of our group
                    rect = new Rect(0, 0, size.x - GenUI.GapTiny * 2 - GenUI.ScrollBarWidth, Text.SmallFontHeight + GenUI.GapTiny * 2);

                    // Draw a background behind our group
                    Widgets.DrawOptionSelected(rect);

                    if (!collapseTab.ContainsKey(defs.Key)) collapseTab.Add(defs.Key, false);
                    if (Widgets.ButtonText(rect.LeftPartPixels(rect.height).ContractedBy(GenUI.GapTiny), collapseTab[defs.Key] ? "-" : "+")) collapseTab[defs.Key] = !collapseTab[defs.Key];

                    rect.xMin += rect.height;
                    Widgets.Label(rect.LeftPartPixels(150).ContractedBy(GenUI.GapTiny), "{0} {1}".Formatted(defs.Count(), defs.Key.LabelCap));
                    rect.xMin += 150;
                    Widgets.FillableBarLabeled(rect.ContractedBy(GenUI.GapTiny), defs.Sum((c) => Math.Abs(c.PowerUsage)) / maxPowerUsage, 50, "Power");
                    Label = "{0}W".Formatted(defs.Sum((c) => c.PowerUsage).ToString("0"));
                    Width = GenUI.GetWidthCached(Label);
                    rect = new RectOffset(-58, 0, -4, -4).Add(rect).LeftPartPixels(Width + 4);
                    Widgets.DrawRectFast(new RectOffset(0, 0, -4, -4).Add(rect), Color.black);
                    Widgets.Label(rect, Label);
                    yref += GenUI.ListSpacing + GenUI.GapTiny;

                    GUI.EndGroup();

                    if (collapseTab[defs.Key])
                    {
                        foreach (CompPowerTracker comp in defs)
                        {
                            comp.DrawGUI(ref yref, size.x, defs.Max((c) => c.maxPowerUsage));
                        }
                    }
                }
            }

            Widgets.EndScrollView();

            lastY = yref;
        }
    }
}

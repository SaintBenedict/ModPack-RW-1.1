using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace VFEF
{
    public class CompSprinkler : ThingComp
    {
        public CompProperties_Sprinkler Props => (CompProperties_Sprinkler)base.props;
        private int hashOffset = 0;
        public int TickInterval => Props.ticksPerPulse;
        public float Radius => Props.effectRadius;
        public float GrowthAmount => Props.growthAmount;
        public bool IsCheapIntervalTick => (int)(Find.TickManager.TicksGame + hashOffset) % TickInterval == 0;
        public Map map => base.parent.Map;

        private bool CurrentlySprinklingMotes = false;
        public float curRot;
        public long MoteSprinkleEndTick;
        public long LastSprinkledMotesTick;

        public CompRefuelable fuel => base.parent.GetComp<CompRefuelable>();
        public bool fueled => fuel == null || fuel.HasFuel;
        public CompPowerTrader power => base.parent.GetComp<CompPowerTrader>();
        public bool powered => power == null || power.PowerOn;
        public bool Active => fueled && powered;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hashOffset = parent.thingIDNumber.HashOffset();
            LastSprinkledMotesTick = GenTicks.TicksAbs - GenDate.TicksPerDay;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Active && IsCheapIntervalTick)
            {
                GrowPlants();
            }
            if(Active && Props.shouldSprinkleMotes){
                if (GenLocalDate.HourOfDay(base.parent) == Props.sprinkleHour && !CurrentlySprinklingMotes && (GenTicks.TicksAbs - LastSprinkledMotesTick) >= GenDate.TicksPerHour * 23)
                {
                    StartSprinklingMotes();
                }
                else if (CurrentlySprinklingMotes) SprinkleMotes();
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            GrowPlants();
        }
        #region growplants
        public void GrowPlants()
        {
            foreach(Plant p in AllPlantsInRadius(base.parent.Position, Radius))
            {
                p.Growth += GrowthAmount * (p.GrowthRate / (GenDate.TicksPerDay * p.def.plant.growDays));
            }
        }

        public IEnumerable<Plant> AllPlantsInRadius(IntVec3 center, float radius)
        {
            foreach(IntVec3 cell in map.AllCells)
            {
                if(IntVec3Utility.DistanceTo(base.parent.Position, cell) < Radius)
                {
                    foreach(Thing t in map.thingGrid.ThingsAt(cell))
                    {
                        Plant p = t as Plant;
                        if (p != null) yield return p;
                    }
                }
            }
        }
        #endregion growplants

        public void StartSprinklingMotes()
        {
            curRot = 0f;
            CurrentlySprinklingMotes = true;
            MoteSprinkleEndTick = GenTicks.TicksAbs + Props.sprinkleDurationTicks;
            SprinkleMotes();
            LastSprinkledMotesTick = GenTicks.TicksAbs;
        }

        public void SprinkleMotes()
        {
            if (GenTicks.TicksAbs > MoteSprinkleEndTick) CurrentlySprinklingMotes = false;
            if (GenTicks.TicksAbs % Props.moteMod == 0) MoteSprinkler.ThrowWaterSpray(base.parent.TrueCenter(), base.parent.Map, curRot, Props.moteThingDef);
            curRot += Props.degreesPerTick;
        }
    }
}

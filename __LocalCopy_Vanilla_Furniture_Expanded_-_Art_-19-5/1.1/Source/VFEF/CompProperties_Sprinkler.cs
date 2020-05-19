using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace VFEF
{
    public class CompProperties_Sprinkler : CompProperties
    {
        public int ticksPerPulse;
        public float effectRadius, growthAmount;

        public bool shouldSprinkleMotes = false;
        public int sprinkleHour, sprinkleDurationTicks, moteMod;
        public float degreesPerTick;
        public ThingDef moteThingDef;

        public CompProperties_Sprinkler()
        {
            base.compClass = typeof(CompSprinkler);
        }
    }
}

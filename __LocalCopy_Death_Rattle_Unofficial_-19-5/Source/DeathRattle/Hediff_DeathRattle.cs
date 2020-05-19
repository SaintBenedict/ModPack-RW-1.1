using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace DeathRattle
{
    public class Hediff_DeathRattle : HediffWithComps
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref cause, "cause");
        }

        public override void PostTick()
        {
            base.PostTick();
            if(pawn.health.capacities.CapableOf(cause))
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public PawnCapacityDef cause;
    }
}

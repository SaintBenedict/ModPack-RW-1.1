using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace VFEF
{
    class Building_PlantGrower_NoEmptyLines : Building_PlantGrower
    {
        public override string GetInspectString()
        {
            if (base.Spawned) return !PlantUtility.GrowthSeasonNow(base.Position, base.Map, true) ? "CannotGrowBadSeasonTemperature".Translate() : "GrowSeasonHereNow".Translate();
            return "VFEF.Building_PlantGrower_NoEmptyLines: GetInspectString() called but building is not spawned.";
        }
    }
}

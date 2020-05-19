using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ToggleBuildingDefOnPower
{
    // Token: 0x02000020 RID: 32
    public class ThoughtGiverDefExtension : DefModExtension
    {
        public ThingDef ThingToGiveThought = null;
        public float DistanceToGiveThought = 15f;
    }

}

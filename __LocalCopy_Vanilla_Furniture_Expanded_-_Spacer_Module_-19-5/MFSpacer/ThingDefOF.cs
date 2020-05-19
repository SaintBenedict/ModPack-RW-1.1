using System;
using RimWorld;
using Verse;

namespace MFSpacer
{
    [DefOf]
    public static class ThingDefOf
    {
        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        public static ThingDef Shelf_RepairRack;
        
        public static ThingDef Table_interactive_1x1c;

        public static ThingDef Table_interactive_2x2c;
    }
}
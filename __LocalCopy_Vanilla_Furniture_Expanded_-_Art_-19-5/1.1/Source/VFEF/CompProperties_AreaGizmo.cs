using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFEF
{
    class CompProperties_AreaGizmo : CompProperties
    {
# pragma warning disable CS0649
        public int radius;
        public string iconPath;

        public CompProperties_AreaGizmo()
        {
            base.compClass = typeof(CompAreaGizmo);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parent)
        {
            base.ConfigErrors(parent);
            if (radius < 0) yield return "CompProperties_AreaGizmo: radius cannot be less than 0.";
        }
    }
}

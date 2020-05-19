using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFEF
{
    public class CompProperties_ScaresAnimals : CompProperties
    {
        public int ticksPerPulse;
        public float effectRadius, minFleeDistance = 23f, maxBodySizeToScare = -1f;
        public bool AffectColonyAnimals = true;

        public CompProperties_ScaresAnimals()
        {
            base.compClass = typeof(CompScaresAnimals);
        }
    }
}

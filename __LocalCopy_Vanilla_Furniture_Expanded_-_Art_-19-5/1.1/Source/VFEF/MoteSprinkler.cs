using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEF
{
    static class MoteSprinkler
    {
        public static float minVelocity = 1.7f, maxVelocity = 2f;

        public static MoteThrown NewMote(ThingDef def)
        {
            MoteThrown ret = ThingMaker.MakeThing(def, null) as MoteThrown;
            ret.Scale = 1.5f;
            ret.rotationRate = (float)Rand.RangeInclusive(-30, 30);
            return ret;
        }
        public static void ThrowWaterSpray(Vector3 loc, Map map, float angle, ThingDef def)
        {
            if(loc.ShouldSpawnMotesAt(map) && !map.moteCounter.SaturatedLowPriority)
            {                
                MoteThrown left = NewMote(def);
                left.exactPosition = loc;
                left.SetVelocity(angle, Rand.Range(minVelocity, maxVelocity));
                MoteThrown right = NewMote(def);
                right.exactPosition = loc;
                right.SetVelocity(angle + 180, Rand.Range(minVelocity, maxVelocity));
                GenSpawn.Spawn(left, loc.ToIntVec3(), map);
                GenSpawn.Spawn(right, loc.ToIntVec3(), map);
            }
        }
    }
}

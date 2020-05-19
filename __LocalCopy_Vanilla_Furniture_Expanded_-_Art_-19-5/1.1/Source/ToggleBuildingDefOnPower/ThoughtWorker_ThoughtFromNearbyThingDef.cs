using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ToggleBuildingDefOnPower
{
    // Token: 0x02000231 RID: 561
    public class ThoughtWorker_ThoughtFromNearbyThingDef : ThoughtWorker
    {
        // Token: 0x06000A58 RID: 2648 RVA: 0x000506F0 File Offset: 0x0004EAF0
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
            {
                return false;
            }
            if (!this.def.HasModExtension<ThoughtGiverDefExtension>())
            {
                return false;
            }
            ThoughtGiverDefExtension defExtension = this.def.GetModExtension<ThoughtGiverDefExtension>();
            if (defExtension.ThingToGiveThought == null)
            {
                return false;
            }
            List<Thing> list = p.Map.listerThings.ThingsOfDef(defExtension.ThingToGiveThought);
            for (int i = 0; i < list.Count; i++)
            {
                CompPowerTrader compPowerTrader = list[i].TryGetComp<CompPowerTrader>();
                if ((compPowerTrader == null || compPowerTrader.PowerOn) && list[i] != p)
                {
                    if (p.Position.InHorDistOf(list[i].Position, defExtension.DistanceToGiveThought))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Token: 0x04000400 RID: 1024
        private const float Radius = 15f;
    }
}
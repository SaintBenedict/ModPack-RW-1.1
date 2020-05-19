using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MFSpacer
{
    public class Building_RepairStored : Building_Storage
    {
        public override void Tick()
        {
            List<Thing> list = this.Map.listerThings.ThingsOfDef(ThingDefOf.Shelf_RepairRack);
            for (int i = 0; i < list.Count; i++)
            {
                CompPowerTrader compPowerTrader = ThingCompUtility.TryGetComp<CompPowerTrader>(list[i]);
                if (compPowerTrader == null || compPowerTrader.PowerOn && GridsUtility.GetFirstItem(this.Position, this.Map) != null)
                {
                    TicksCounted++;
                    if (TicksCounted == 2500)
                    {
                        Thing RepairedItem = GridsUtility.GetFirstItem(this.Position, this.Map);
                        if (RepairedItem != null && RepairedItem.HitPoints != RepairedItem.MaxHitPoints && RepairedItem.def.IsWithinCategory(ThingCategoryDefOf.Weapons) || RepairedItem != null && RepairedItem.HitPoints != RepairedItem.MaxHitPoints && RepairedItem.def.IsWithinCategory(ThingCategoryDefOf.Apparel) == true)
                        {
                            RepairedItem.HitPoints++;
                            
                        }
                        TicksCounted = 0;
                    }
                    base.Tick();
                }
            }
        }
        int TicksCounted = 0;
    }
}

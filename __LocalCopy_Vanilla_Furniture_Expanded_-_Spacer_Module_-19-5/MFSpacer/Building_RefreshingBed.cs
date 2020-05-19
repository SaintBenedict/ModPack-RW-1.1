using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MFSpacer
{
    public class Building_RefreshingBed : Building_Bed
    {
        public override void Tick()
        {
            for (int i = 0; i < SleepingSlotsCount; i++)
            {
                if (base.GetCurOccupant(i) != null)
                {
                    Pawn curOccupant = base.GetCurOccupant(i);
                    TicksCounted[i]++;
                    if (TicksCounted[i] == 1250 && PawnUtility.GetPosture(curOccupant) == PawnPosture.LayingInBed)
                    {
                        Hediff firstHediffOfDef = curOccupant.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Bed_RefreshingSleep, false);
                        if (firstHediffOfDef == null)
                        {
                            curOccupant.health.AddHediff(HediffDefOf.Bed_RefreshingSleep, null, null, null);
                        }
                        else
                        {
                            firstHediffOfDef.Severity += 0.05f;
                        }
                        TicksCounted[i] = 0;
                    }
                }
            }

            base.Tick();
        }
        int[] TicksCounted = { 0, 0 };
    }
}

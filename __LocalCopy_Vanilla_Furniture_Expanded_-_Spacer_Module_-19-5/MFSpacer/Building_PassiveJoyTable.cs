using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;
using AOMoreFurniture;

namespace MFSpacer
{
    public class Building_PassiveJoyTable : Building
    {
        public override void Tick()
        {
                CompPowerTrader compPowerTrader = ThingCompUtility.TryGetComp<CompPowerTrader>(this);
                if (compPowerTrader == null || compPowerTrader.PowerOn)
                {
                    List<Pawn> PawnList = CollectPawns();
                    for (int i = 0; i < PawnList.Count; i++)
                    {
                        if (PawnList[i] != null && (PawnList[i].Position + PawnList[i].Rotation.FacingCell).HasEatSurface(this.Map) && PawnList[i].GetPosture() == PawnPosture.Standing && PawnList[i].CurJobDef == JobDefOf.Ingest)
                        {
                            TicksCounted++;
                            PawnList[i].needs.joy.GainJoy(0.0003f, JoyKindDefOf.Gaming_Electronic);
                            if (TicksCounted == 70 + rnd.Next(10, 20))
                            {
                                SoundStarter.PlayOneShot(AOMoreFurniture.SoundDefOf.Computer_SFXTwo, new TargetInfo(this.Position, this.Map, false));
                            }
                        }
                    }
                }
            base.Tick();
        }
        public List<Pawn> CollectPawns()
        {
            List<Pawn> TmpList = new List<Pawn>();

            if (this.Map.thingGrid.CellContains(this.Position, ThingDefOf.Table_interactive_2x2c))
            {
                //Top Cells
                AdjacentCellsAround_2x2[0] = new IntVec3(0, 0, 2);
                AdjacentCellsAround_2x2[5] = new IntVec3(1, 0, 2);
                //Right Cells
                AdjacentCellsAround_2x2[1] = new IntVec3(2, 0, 0);
                AdjacentCellsAround_2x2[6] = new IntVec3(2, 0, 1);
                //Bottom Cells
                AdjacentCellsAround_2x2[2] = new IntVec3(0, 0, -1);
                AdjacentCellsAround_2x2[4] = new IntVec3(1, 0, -1);
                //Left Cells
                AdjacentCellsAround_2x2[3] = new IntVec3(-1, 0, 0);
                AdjacentCellsAround_2x2[7] = new IntVec3(-1, 0, 1);

                for (int i = 0; i < AdjacentCellsAround_2x2.Length; i++)
                {
                    if (GridsUtility.GetFirstPawn(this.Position + AdjacentCellsAround_2x2[i], this.Map) != null)
                    {
                        TmpList.Add(GridsUtility.GetFirstPawn(this.Position + AdjacentCellsAround_2x2[i], this.Map));
                    }
                }
            }
            else
            {
                for (int i = 0; i < GenAdj.CardinalDirectionsAround.Length; i++)
                {
                    if (GridsUtility.GetFirstPawn(this.Position + GenAdj.CardinalDirectionsAround[i], this.Map) != null)
                    {
                        TmpList.Add(GridsUtility.GetFirstPawn(this.Position + GenAdj.CardinalDirectionsAround[i], this.Map));
                    }
                }
            }          
            return TmpList;
        }
        Random rnd = new Random();

        public static IntVec3[] AdjacentCellsAround_2x2 = new IntVec3[8];

        int TicksCounted = 0;


    }
}

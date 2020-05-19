using RimWorld;
using Verse;

namespace RimWorld
{
    // AdeptusMechanicus.Building_AdvancedArt
    public class Building_AdvancedArt : Building_Art
    {
        private Graphic offGraphic;
        public CompFlickable flickableComp
        {
            get
            {
                return this.TryGetComp<CompFlickable>();
            }
        }

        public CompPowerTrader CompPower
        {
            get
            {
                return this.TryGetComp<CompPowerTrader>();
            }
        }

        public CompPoweredStatChange statChange
        {
            get
            {
                return this.TryGetComp<CompPoweredStatChange>();
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (flickableComp != null)
                {
                    if (CompPower != null)
                    {
                        if (flickableComp.SwitchIsOn && CompPower.PowerOn)
                        {
                            return this.DefaultGraphic;
                        }
                        if (this.offGraphic == null)
                        {
                            this.offGraphic = GraphicDatabase.Get(typeof(Graphic_Single), this.def.graphicData.texPath + "_Off", this.def.graphicData.shaderType.Shader, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
                        }
                        return this.offGraphic;
                    }
                }
                return base.DefaultGraphic;
            }

        }
        
        /*
        // Token: 0x06002373 RID: 9075 RVA: 0x0010F20C File Offset: 0x0010D60C
        public override string GetInspectString()
        {
            string inspectString = base.GetInspectString();
            string text = inspectString;
            return string.Concat(new string[]
            {
                text,
                "\n",
                StatDefOf.Beauty.LabelCap,
                ": ",
                StatDefOf.Beauty.ValueToString(this.GetStatValue(StatDefOf.Beauty, true), ToStringNumberSense.Absolute)
            });
        }
        */
    }

}

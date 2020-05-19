using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
    // Token: 0x02000260 RID: 608
    public class CompProperties_PoweredStatChange : CompProperties
    {
        // Token: 0x06000ACE RID: 2766 RVA: 0x000563B9 File Offset: 0x000547B9
        public CompProperties_PoweredStatChange()
        {
            this.compClass = typeof(CompPoweredStatChange);
        }
        public StatDef stat = null;
        public float On = 0f;
        public float Off = 0f;
    }
    // Token: 0x0200042B RID: 1067
    public class CompPoweredStatChange : ThingComp
    {
        // Token: 0x17000282 RID: 642
        // (get) Token: 0x06001281 RID: 4737 RVA: 0x0008EE6E File Offset: 0x0008D26E
        public CompProperties_PoweredStatChange Props
        {
            get
            {
                return (CompProperties_PoweredStatChange)this.props;
            }
        }
        /*
        public Graphic CurrentGraphic
        {
            get
            {
                if (this.SwitchIsOn)
                {
                    return this.parent.DefaultGraphic;
                }
                if (this.offGraphic == null)
                {
                    this.offGraphic = GraphicDatabase.Get(this.parent.def.graphicData.graphicClass, this.parent.def.graphicData.texPath + "_Off", this.parent.def.graphicData.shaderType.Shader, this.parent.def.graphicData.drawSize, this.parent.DrawColor, this.parent.DrawColorTwo);
                }
                return this.offGraphic;
            }
        }
        */

        public CompFlickable flickableComp
        {
            get
            {
                return this.parent.TryGetComp<CompFlickable>();
            }
        }

        public CompPowerTrader CompPower
        {
            get
            {
                return this.parent.TryGetComp<CompPowerTrader>();
            }
        }
        
        public bool Active
        {
            get
            {
                bool active = CompPower != null ? CompPower.PowerOn : false && flickableComp != null ? flickableComp.SwitchIsOn : false;
                return active;
            }
        }
    }
}

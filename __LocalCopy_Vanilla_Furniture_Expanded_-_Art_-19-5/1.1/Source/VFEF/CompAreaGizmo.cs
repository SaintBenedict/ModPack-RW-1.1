using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEF
{
    class CompAreaGizmo : ThingComp
    {
        public CompProperties_AreaGizmo Props => (CompProperties_AreaGizmo)base.props;
        public int Radius => Props.radius;
        public string TexturePath => Props.iconPath;
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            base.CompGetGizmosExtra();
            if (DesignatorUtility.FindAllowedDesignator<Designator_AreaAllowedExpand>() != null)
            {
                yield return new Command_Action
                {
                    action = new Action(this.DesignateArea),
                    hotKey = KeyBindingDefOf.Misc1,
                    defaultDesc = "VFEF_CommandMakeCompAreaDesc".Translate(base.parent.Label),
                    icon = ContentFinder<Texture2D>.Get(TexturePath, true),
                    defaultLabel = "VFEF_CommandMakeCompAreaLabel".Translate(base.parent.Label)
                };
            }//end if
        }//end CompGetGizmosExtra

        private void DesignateArea()
        {
            List<FloatMenuOption> dropdown = new List<FloatMenuOption>();
            Designator_AreaAllowedExpand des = DesignatorUtility.FindAllowedDesignator<Designator_AreaAllowedExpand>() as Designator_AreaAllowedExpand;
            foreach (Area_Allowed a in base.parent.Map.areaManager.AllAreas.OfType<Area_Allowed>())
            {
                dropdown.Add(new FloatMenuOption(a.Label, delegate
                {
                    SetArea(des, a);
                    des.DesignateMultiCell(CellsInRectangularRadius());
                }));
            }
            Find.WindowStack.Add(new FloatMenu(dropdown));  
        }//end MakeArea

        private IEnumerable<IntVec3> CellsInRectangularRadius()
        {
            IntVec3 bottomLeft = base.parent.OccupiedRect().BottomLeft;
            int minX = bottomLeft.x - Radius, minZ = bottomLeft.z - Radius;
            IntVec3 topRight = base.parent.OccupiedRect().TopRight;
            int maxX = topRight.x + Radius, maxZ = topRight.z + Radius;
            foreach (IntVec3 cell in base.parent.Map.AllCells)
            {
                if (cell.x >= minX && cell.x <= maxX && cell.z >= minZ && cell.z <= maxZ) yield return cell;
            }
        }

        private void SetArea(Designator_AreaAllowedExpand des, Area_Allowed a)
        {
            if (a == null) return;
            Type desType = typeof(Designator_AreaAllowed);
            FieldInfo areaToSet = desType.GetField("selectedArea", BindingFlags.NonPublic | BindingFlags.Static);
            if (areaToSet != null)
            {
                areaToSet.SetValue(null, a);
            }            
        }
    }//end CompAreaGizmo
}

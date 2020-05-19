using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace DeathRattle.Harmony
{
    [HarmonyPatch(typeof(CompUseEffect_FixWorstHealthCondition), nameof(CompUseEffect_FixWorstHealthCondition.DoEffect))]
    public static class CompUseEffect_FixWorstHealthCondition_DestroyedOrganPatch
    {
        [HarmonyPrefix]
        public static bool PrioritizeDestroyedOrgans(CompUseEffect_FixWorstHealthCondition __instance, Pawn usedBy)
        {
            BodyPartRecord bodyPartRecord = FindMissingOrgan(usedBy);
            if (bodyPartRecord != null)
            {
                Traverse.Create(__instance).Method("Cure", new Type[] { typeof(BodyPartRecord), typeof(Pawn) }).GetValue(new object[] { bodyPartRecord, usedBy });
                return false;
            }
            return true;
        }

        private static BodyPartRecord FindMissingOrgan(Pawn pawn)
        {
            BodyPartRecord bodyPartRecord = null;
            List<string> sourceDefs = new List<string>(new string[] { "ConsciousnessSource", "BloodPumpingSource", "BreathingSource", "BloodFiltrationLiver", "BloodFiltrationKidney", "MetabolismSource" });
            foreach (Hediff_MissingPart current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                BodyPartTagDef sourceTag = current.Part.def.tags.Find(x => sourceDefs.Contains(x.defName));
                if (sourceTag != null)
                {
                    if (bodyPartRecord == null || sourceDefs.IndexOf(sourceTag.defName) < sourceDefs.IndexOf(bodyPartRecord.def.tags.Find(x => sourceDefs.Contains(x.defName)).defName))
                    {
                        bodyPartRecord = current.Part;
                    }
                }
            }
            return bodyPartRecord;
        }
    }
}

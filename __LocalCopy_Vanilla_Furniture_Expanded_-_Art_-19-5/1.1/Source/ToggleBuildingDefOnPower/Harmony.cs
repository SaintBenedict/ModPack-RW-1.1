using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System;

namespace ToggleBuildingDefOnPower
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.ToggleBuildingDefOnPower");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(StatExtension), "GetStatValue")]
    public static class VFEA_StatExtension_GetStatValue_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, Thing thing, StatDef stat, bool applyPostProcess)
        {
            float num = 0f;
            try
            {
                CompPoweredStatChange statChange = thing.TryGetComp<CompPoweredStatChange>();
                if (statChange != null)
                {
                    if (statChange.Props.stat != null && stat == statChange.Props.stat)
                    {
                        if (statChange.Active)
                        {
                            num = statChange.Props.On;
                        }
                        else
                        {
                            num = statChange.Props.Off;
                        }
                        __result *= num;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to add stats for " + thing.Label + "\n" + ex.ToString(), false);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace CarryCapacityFixed
{
    [StaticConstructorOnStartup]
    internal static class CCFHarmonyPatch
    {
        static CCFHarmonyPatch()
        {
            var harmonyCCF = new Harmony("smashphil.ccfbutbetter.rimworld");
            //Harmony.DEBUG = true;

            harmonyCCF.Patch(original: AccessTools.Method(typeof(MassUtility), nameof(MassUtility.Capacity)), prefix: null, postfix: null,
                transpiler: new HarmonyMethod(typeof(CCFHarmonyPatch), nameof(CarryCapacityChange)));
        }

        public static IEnumerable<CodeInstruction> CarryCapacityChange(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            for(int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];

                if(instruction.opcode == OpCodes.Stloc_0)
                {
                    //Check if pawn belongs to a def that is allowed to apply the CCF Patch to
                    Label brlabel = ilg.DefineLabel();
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(typeof(CCFHarmonyPatch), nameof(CCFHarmonyPatch.ContainedWithinDoNotApplyList)));
                    yield return new CodeInstruction(opcode: OpCodes.Brtrue, brlabel);

                    //Replace formula
                    yield return new CodeInstruction(opcode: OpCodes.Pop);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.BodySize)));
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(typeof(CCFHarmonyPatch), nameof(CCFHarmonyPatch.GetCarryCapacityShortcut)));

                    yield return new CodeInstruction(opcode: OpCodes.Mul);

                    instruction.labels.Add(brlabel);
                }

                yield return instruction;
            }
        }

        public static bool ContainedWithinDoNotApplyList(Pawn p) => p?.def?.HasModExtension<DoNotApply_ModExtension>() ?? true;

        public static float GetCarryCapacityShortcut(Pawn p) => p.GetStatValue(StatDefOf.CarryingCapacity);
    }
}

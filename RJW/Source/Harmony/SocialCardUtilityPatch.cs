using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace rjw
{
	class SocialCardUtilityPatch
	{
		[HarmonyPatch(typeof(SocialCardUtility))]
		[HarmonyPatch("DrawDebugOptions")]
		public static class Patch_SocialCardUtility_DrawDebugOptions
		{
			//Create a new menu option that will contain some of the relevant data for debugging RJW
			//Window space is limited, so keep to one line per pawn. Additional data may need a separate menu
			static FloatMenuOption newMenuOption(Pawn pawn) {
				return new FloatMenuOption("RJW WouldFuck", () =>
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("canFuck: " + xxx.can_fuck(pawn) + ", canBeFucked: " + xxx.can_be_fucked(pawn) + ", loving: " + xxx.can_do_loving(pawn));
					stringBuilder.AppendLine("canRape: " + xxx.can_rape(pawn) + ", canBeRaped: " + xxx.can_get_raped(pawn));

					if (!pawn.IsColonist)
						if (pawn.Faction != null)
						{
							int pawnAmountOfSilvers = pawn.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Silver);
							int caravanAmountOfSilvers = 0;
							Lord lord = pawn.GetLord();
							List<Pawn> caravanMembers = pawn.Map.mapPawns.PawnsInFaction(pawn.Faction).Where(x => x.GetLord() == lord && x.inventory?.innerContainer?.TotalStackCountOfDef(ThingDefOf.Silver) > 0).ToList();

							caravanAmountOfSilvers += caravanMembers.Sum(member => member.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Silver));

							stringBuilder.AppendLine("pawnSilvers: " + pawnAmountOfSilvers + ", caravanSilvers: " + caravanAmountOfSilvers);
						}
					stringBuilder.AppendLine();

					stringBuilder.AppendLine("Humans - Colonists:");
					List<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x != pawn && x.RaceProps.Humanlike && x.IsColonist).OrderBy(x => xxx.get_pawnname(x)).ToList();
					foreach (Pawn partner in pawns)
					{
						stringBuilder.AppendLine(partner.LabelShort + " (" + partner.gender.GetLabel() +
							", age: " + partner.ageTracker.AgeBiologicalYears +
							", " + CompRJW.Comp(partner).orientation +
							"): (fuck) " + SexAppraiser.would_fuck(pawn, partner).ToString("F3") +
							"): (fucked) " + SexAppraiser.would_fuck(partner, pawn).ToString("F3") +
							": (rape) " + SexAppraiser.would_rape(pawn, partner));
					}
					stringBuilder.AppendLine();

					stringBuilder.AppendLine("Humans - Prisoners:");
					pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x != pawn && x.RaceProps.Humanlike && !x.IsColonist && x.IsPrisonerOfColony).OrderBy(x => xxx.get_pawnname(x)).ToList();
					foreach (Pawn partner in pawns)
					{
						stringBuilder.AppendLine(partner.LabelShort + " (" + partner.gender.GetLabel() +
							", age: " + partner.ageTracker.AgeBiologicalYears +
							", " + CompRJW.Comp(partner).orientation +
							"): (fuck) " + SexAppraiser.would_fuck(pawn, partner).ToString("F3") +
							"): (fucked) " + SexAppraiser.would_fuck(partner, pawn).ToString("F3") +
							": (rape) " + SexAppraiser.would_rape(pawn, partner));
					}
					stringBuilder.AppendLine();

					stringBuilder.AppendLine("Humans - non Colonists:");
					pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x != pawn && x.RaceProps.Humanlike && !x.IsColonist && !x.IsPrisonerOfColony).OrderBy(x => xxx.get_pawnname(x)).ToList();
					foreach (Pawn partner in pawns)
					{
						stringBuilder.AppendLine(partner.LabelShort + " (" + partner.gender.GetLabel() +
							", age: " + partner.ageTracker.AgeBiologicalYears +
							", " + CompRJW.Comp(partner).orientation +
							"): (fuck) " + SexAppraiser.would_fuck(pawn, partner).ToString("F3") +
							"): (fucked) " + SexAppraiser.would_fuck(partner, pawn).ToString("F3") +
							": (rape) " + SexAppraiser.would_rape(pawn, partner));
					}
					stringBuilder.AppendLine();

					stringBuilder.AppendLine("Animals - Colony:");
					pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x != pawn && x.RaceProps.Animal && x.Faction == Faction.OfPlayer).OrderBy(x => xxx.get_pawnname(x)).ToList();
					foreach (Pawn partner in pawns)
					{
						stringBuilder.AppendLine(partner.LabelShort + " (" + partner.gender.GetLabel() +
							", age: " + partner.ageTracker.AgeBiologicalYears +
							", " + CompRJW.Comp(partner).orientation +
							"): (fuck) " + SexAppraiser.would_fuck(pawn, partner).ToString("F3") +
							"): (fucked) " + SexAppraiser.would_fuck(partner, pawn).ToString("F3") +
							": (rape) " + SexAppraiser.would_rape(pawn, partner));
					}
					stringBuilder.AppendLine();

					stringBuilder.AppendLine("Animals - non Colony:");
					pawns = pawn.Map.mapPawns.AllPawnsSpawned.Where(x => x != pawn && x.RaceProps.Animal && x.Faction != Faction.OfPlayer).OrderBy(x => xxx.get_pawnname(x)).ToList();
					foreach (Pawn partner in pawns)
					{
						stringBuilder.AppendLine(partner.LabelShort + " (" + partner.gender.GetLabel() +
							", age: " + partner.ageTracker.AgeBiologicalYears +
							", " + CompRJW.Comp(partner).orientation +
							"): (fuck) " + SexAppraiser.would_fuck(pawn, partner).ToString("F3") +
							"): (fucked) " + SexAppraiser.would_fuck(partner, pawn).ToString("F3") +
							": (rape) " + SexAppraiser.would_rape(pawn, partner));
					}
					Find.WindowStack.Add(new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false, null, null));
				}, MenuOptionPriority.Default, null, null, 0.0f, null, null);
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> codes = instructions.ToList();
				var addFunc = typeof(List<FloatMenuOption>).GetMethod("Add", new Type[] { typeof(FloatMenuOption) });
				//Identify the last time options.Add() is called so we can place our new option afterwards
				CodeInstruction lastAdd = codes.FindLast(ins => ins.opcode == OpCodes.Callvirt && ins.operand == addFunc);
				for (int i = 0; i < codes.Count; ++i) {
					yield return codes[i];
					if (codes[i] == lastAdd) {
						yield return new CodeInstruction(OpCodes.Ldloc_1);
						yield return new CodeInstruction(OpCodes.Ldarg_1);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_SocialCardUtility_DrawDebugOptions), nameof(newMenuOption)));
						yield return new CodeInstruction(OpCodes.Callvirt, addFunc);
					}
				}
			}
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace rjw
{
	class RaceGroupDef_Helper
	{
		/// <summary>
		/// Cache for TryGetRaceGroupDef.
		/// </summary>
		static readonly IDictionary<PawnKindDef, RaceGroupDef> RaceGroupByPawnKind = new Dictionary<PawnKindDef, RaceGroupDef>();

		public static bool TryGetRaceGroupDef(Pawn pawn, out RaceGroupDef raceGroupDef)
		{
			if (RaceGroupByPawnKind.TryGetValue(pawn.kindDef, out raceGroupDef))
			{
				return raceGroupDef != null;
			}
			else
			{
				raceGroupDef = GetRaceGroupDefInternal(pawn);
				RaceGroupByPawnKind.Add(pawn.kindDef, raceGroupDef);
				return raceGroupDef != null;
			}
		}

		/// <summary>
		/// Returns the best match RaceGroupDef for the given pawn, or null if none is found.
		/// </summary>
		static RaceGroupDef GetRaceGroupDefInternal(Pawn pawn)
		{
			var kindDef = pawn.kindDef;
			var raceName = kindDef.race.defName;
			var pawnKindName = kindDef.defName;
			var groups = DefDatabase<RaceGroupDef>.AllDefs;

			var kindMatches = groups.Where(group => group.pawnKindNames?.Contains(pawnKindName) ?? false).ToList();
			var raceMatches = groups.Where(group => group.raceNames?.Contains(raceName) ?? false).ToList();
			var count = kindMatches.Count() + raceMatches.Count();
			if (count == 0)
			{
				//Log.Message($"[RJW] Pawn named '{pawn.Name}' matched no RaceGroupDef. If you want to create a matching RaceGroupDef you can use the raceName '{raceName}' or the pawnKindName '{pawnKindName}'.");
				return null;
			}
			else if (count == 1)
			{
				// Log.Message($"[RJW] Pawn named '{pawn.Name}' matched 1 RaceGroupDef.");
				return kindMatches.Concat(raceMatches).Single();
			}
			else
			{
				// Log.Message($"[RJW] Pawn named '{pawn.Name}' matched {count} RaceGroupDefs.");

				// If there are multiple RaceGroupDef matches, choose one of them.
				// First prefer defs NOT defined in rjw.
				// Then prefer a match by kind over a match by race.
				return kindMatches.FirstOrDefault(match => !IsThisMod(match))
					?? raceMatches.FirstOrDefault(match => !IsThisMod(match))
					?? kindMatches.FirstOrDefault()
					?? raceMatches.FirstOrDefault();
			}
		}

		static bool IsThisMod(Def def)
		{
			var rjwContent = LoadedModManager.RunningMods.Single(pack => pack.Name == "RimJobWorld");
			return rjwContent.AllDefs.Contains(def);
		}

		/// <summary>
		/// Returns true if a race part was chosen (even if that part is "no part").
		/// </summary>
		public static bool TryAddRacePart(Pawn pawn, SexPartType sexPartType)
		{
			if (!TryGetRaceGroupDef(pawn, out var raceGroupDef))
			{
				// No race, so nothing was chosen.
				return false;
			}

			if (!RacePartDef_Helper.TryChooseRacePartDef(raceGroupDef, sexPartType, out var racePartDef))
			{
				// Failed to find a part, so nothing was chosen.
				return false;
			}

			if (racePartDef.IsNone)
			{
				// "no part" was explicitly chosen.
				return true;
			}

			var target = sexPartType.GetBodyPartDef();
			var bodyPartRecord = pawn.RaceProps.body.AllParts.Find(bpr => bpr.def == target);
			if (!racePartDef.TryGetHediffDef(out var hediffDef))
			{
				// Failed to find hediffDef.
				return false;
			}

			var hediff = RacePartDef_Helper.MakePart(hediffDef, pawn, bodyPartRecord, racePartDef);
			pawn.health.AddHediff(hediff, bodyPartRecord);
			// A part was chosen and added.
			return true;
		}
	}
}

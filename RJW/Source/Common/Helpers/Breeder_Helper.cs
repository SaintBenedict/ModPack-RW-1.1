// #define TESTMODE // Uncomment to enable logging.

using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System.Diagnostics;
using Multiplayer.API;

namespace rjw
{
	/// <summary>
	/// Helper for sex with animals
	/// </summary>
	public class BreederHelper
	{
		public const int max_animals_at_once = 1; // lets not forget that the purpose is procreation, not sodomy

		[Conditional("TESTMODE")]
		private static void DebugText(string msg)
		{
			Log.Message(msg);
		}

		//animal try to find best designated pawn to breed
		[SyncMethod]
		public static Pawn find_designated_breeder(Pawn pawn, Map m)
		{
			if (!DesignatorsData.rjwBreeding.Any())
				return null;

			DebugText("BreederHelper::find_designated_breeder( " + xxx.get_pawnname(pawn) + " ) called");

			float min_fuckability = 0.10f;                          // Don't rape pawns with <10% fuckability
			float avg_fuckability = 0f;                             // Average targets fuckability, choose target higher than that
			var valid_targets = new Dictionary<Pawn, float>();      // Valid pawns and their fuckability
			Pawn chosentarget = null;                               // Final target pawn

			if (pawn.Faction == null)// HostileTo causes error on fresh colony(w/o name)
				return null;

			IEnumerable<Pawn> targets = DesignatorsData.rjwBreeding.Where(x
				=> x != pawn 
				&& xxx.is_not_dying(x) 
				&& xxx.can_get_raped(x) 
				&& !x.IsForbidden(pawn)
				&& !x.Suspended
				&& !x.HostileTo(pawn)
				&& !(x.IsPregnant() && xxx.is_animal(x))
				&& pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Some, max_animals_at_once)
				&& ((RJWSettings.bestiality_enabled && xxx.is_human(x)) || (RJWSettings.animal_on_animal_enabled && xxx.is_animal(x)))
				);

			foreach (Pawn target in targets)
			{
				if (!xxx.can_path_to_target(pawn, target.Position))
					continue;// too far

				var partBPR = Genital_Helper.get_genitalsBPR(pawn);
				var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

				var fuc = SexAppraiser.would_fuck(pawn, target, invert_opinion: true, ignore_gender: (Genital_Helper.has_penis_fertile(pawn, parts) || Genital_Helper.has_penis_infertile(pawn, parts) || xxx.is_insect(pawn)));
				DebugText("BreederHelper::find_designated_breeder( " + xxx.get_pawnname(pawn) + " -> " + xxx.get_pawnname(target) + " (" + fuc.ToString() + " / " + min_fuckability.ToString() + ")");

				if (fuc > min_fuckability)
					valid_targets.Add(target, fuc);
			}

			if (valid_targets.Any())
			{
				//avg_fuckability = valid_targets.Average(x => x.Value);

				// choose pawns to fuck with above average fuckability
				var valid_targetsFilteredAnimals = valid_targets.Where(x => x.Value >= avg_fuckability);

				if (valid_targetsFilteredAnimals.Any())
					chosentarget = valid_targetsFilteredAnimals.RandomElement().Key;
			}

			return chosentarget;
		}

		//animal or human try to find animals to breed (non designation)
		//public static Pawn find_breeder_animal(Pawn pawn, Map m, bool SameRace = true)
		[SyncMethod]
		public static Pawn find_breeder_animal(Pawn pawn, Map m)
		{
			DebugText("BreederHelper::find_breeder_animal( " + xxx.get_pawnname(pawn) + " ) called");

			float min_fuckability = 0.10f;                          // Don't rape pawns with <10% fuckability
			float avg_fuckability = 0f;                             // Average targets fuckability, choose target higher than that
			var valid_targets = new Dictionary<Pawn, float>();      // Valid pawns and their fuckability
			Pawn chosentarget = null;                               // Final target pawn

			//Pruning initial pawn list.
			IEnumerable<Pawn> targets = m.mapPawns.AllPawnsSpawned.Where(x
				=> x != pawn
				&& xxx.is_animal(x)
				&& xxx.can_get_raped(x)
				&& !x.IsForbidden(pawn)
				&& !x.Suspended
				&& !x.HostileTo(pawn)
				&& pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Some, max_animals_at_once)
				//&& SameRace ? pawn.kindDef.race == x.kindDef.race : true
				);

			if (targets.Any())
			{
				var partBPR = Genital_Helper.get_genitalsBPR(pawn);
				var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

				//filter pawns for female, who can fuck her
				//not sure if faction check should be but w/e
				if (!Genital_Helper.has_penis_fertile(pawn, parts) && !Genital_Helper.has_penis_infertile(pawn, parts) && (Genital_Helper.has_vagina(pawn, parts) || Genital_Helper.has_anus(pawn)))
				{
					targets = targets.Where(x => xxx.can_fuck(x) && x.Faction == pawn.Faction);
				}

				//for humans, animals dont have need - always = 3f
				//if not horny, seek only targets with safe temp
				if (xxx.need_some_sex(pawn) < 3.0f)
				{
					targets = targets.Where(x => pawn.CanReach(x, PathEndMode.Touch, Danger.None));
				}

				//Used for interspecies animal-on-animal.
				//Animals will only go for targets they can see.
				if (xxx.is_animal(pawn))
				{
					targets = targets.Where(x => pawn.CanSee(x) && pawn.def.defName != x.def.defName);
				}
				else
				{
					// Pickier about the targets if the pawn has no prior experience.
					if (pawn.records.GetValue(xxx.CountOfSexWithAnimals) < 3 && !xxx.is_zoophile(pawn))
					{
						min_fuckability *= 2f;
					}

					if (xxx.need_some_sex(pawn) > 2f)
					{   // Less picky when frustrated...
						min_fuckability *= 0.6f;
					}
					else if (xxx.need_some_sex(pawn) < 2f)
					{   // ...and far more picky when satisfied.
						min_fuckability *= 2.5f;
					}
				}
			}

			DebugText("[RJW]BreederHelper::find_breeder_animal::" + targets.Count() + " targets found on map.");

			if (!targets.Any())
			{
				return null; //None found.
			}

			foreach (Pawn target in targets)
			{
				DebugText("[RJW]BreederHelper::find_breeder_animal::Checking target " + target.kindDef.race.defName.ToLower());

				if (!xxx.can_path_to_target(pawn, target.Position))
					continue;// too far

				float fuc = SexAppraiser.would_fuck_animal(pawn, target); // 0.0 to ~3.0, orientation checks etc.

				if (!(fuc > min_fuckability)) continue;
				DebugText("Adding target" + target.kindDef.race.defName.ToLower());
				valid_targets.Add(target, fuc);
			}

			DebugText(valid_targets.Count() + " valid targets found on map.");
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (valid_targets.Any())
			{
				avg_fuckability = valid_targets.Average(x => x.Value);

				// choose pawns to fuck with above average fuckability
				var valid_targetsFilteredAnimals = valid_targets.Where(x => x.Value >= avg_fuckability);

				if (valid_targetsFilteredAnimals.Any())
					chosentarget = valid_targetsFilteredAnimals.RandomElement().Key;
			}

			return chosentarget;
		}
	}
}
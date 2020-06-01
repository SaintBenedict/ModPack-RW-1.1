using Multiplayer.API;
using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace rjw
{
	/// <summary>
	/// Responsible for spreading STDs (adding STD hediffs). Usually happens during sex.
	/// </summary>
	public static class std_spreader
	{
		/// <summary>
		/// Check for spreading of every STD the pitcher has to the catcher.
		/// Includes a small chance to spread STDs that pitcher doesn't have.
		/// </summary>
		[SyncMethod]
		public static void roll_to_catch(Pawn catcher, Pawn pitcher)
		{
			if (std.IsImmune(catcher) || std.IsImmune(pitcher))
			{
				return;
			}
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			float cleanliness_factor = GetCleanlinessFactor(catcher);

			foreach (std_def sd in std.all)
			{
				if (!catcher.health.hediffSet.HasHediff(sd.hediff_def))
				{
					if (catcher.health.immunity.GetImmunity(sd.hediff_def) <= 0.0f)
					{
						var bodyPartRecord = std.GetRelevantBodyPartRecord(catcher, sd);
						var artificial = bodyPartRecord != null && catcher.health.hediffSet.HasDirectlyAddedPartFor(bodyPartRecord);

						float catch_chance = GetCatchChance(catcher, sd);
						float catch_rv = Rand.Value;
						//if (xxx.config.std_show_roll_to_catch)
						//--Log.Message("  Chance to catch " + sd.label + ": " + catch_chance.ToStringPercent() + "; rolled: " + catch_rv.ToString());
						if (catch_rv < catch_chance)
						{
							string pitch_source; float pitch_chance;
							{
								if (get_severity(pitcher, sd) >= xxx.config.std_min_severity_to_pitch)
								{
									pitch_source = xxx.get_pawnname(pitcher);
									pitch_chance = 1.0f;
								}
								else
								{
									pitch_source = "the environment";
									pitch_chance = sd.environment_pitch_chance * cleanliness_factor;
									if (!RJWSettings.std_floor)
									{
										pitch_chance = -9001f;
									}
								}
							}
							float pitch_rv = Rand.Value;

							//if (xxx.config.std_show_roll_to_catch)
							//--Log.Message("	Chance to pitch (from " + pitch_source + "): " + pitch_chance.ToStringPercent() + "; rolled: " + pitch_rv.ToString());
							if (pitch_rv < pitch_chance)
							{
								infect(catcher, sd);
								show_infection_letter(catcher, sd, pitch_source, catch_chance * pitch_chance);
								//if (xxx.config.std_show_roll_to_catch)
								//--Log.Message("	  INFECTED!");
							}
						}
					}
					//else
					//if (xxx.config.std_show_roll_to_catch)
					//--Log.Message("  Still immune to " + sd.label);
				}
				//else
				//if (xxx.config.std_show_roll_to_catch)
				//--Log.Message("  Already infected with " + sd.label);
			}
		}

		public static float get_severity(Pawn p, std_def sd)
		{
			Hediff hed = std.get_infection(p, sd);
			return hed?.Severity ?? 0.0f;
		}

		public static Hediff infect(Pawn p, std_def sd, bool include_coinfection = true)
		{
			Hediff existing = std.get_infection(p, sd);
			if (existing != null)
			{
				return existing;
			}

			BodyPartRecord part = std.GetRelevantBodyPartRecord(p, sd);
			p.health.AddHediff(sd.hediff_def, part);
			if (include_coinfection && sd.cohediff_def != null)
			{
				p.health.AddHediff(sd.cohediff_def, part);
			}
			//--Log.Message("[RJW]std::infect genitals std");
			return std.get_infection(p, sd);
		}

		static float GetCatchChance(Pawn pawn, std_def sd)
		{
			var bodyPartRecord = std.GetRelevantBodyPartRecord(pawn, sd);
			float artificialFactor = 1f;

			if (bodyPartRecord == null && pawn.health.hediffSet.HasDirectlyAddedPartFor(Genital_Helper.get_genitalsBPR(pawn)))
			{
				artificialFactor = .15f;
			}
			else if (pawn.health.hediffSet.HasDirectlyAddedPartFor(bodyPartRecord))
			{
				artificialFactor = 0f;
			}

			return sd.catch_chance * artificialFactor;
		}

		public static void show_infection_letter(Pawn p, std_def sd, string source = null, float? chance = null)
		{
			StringBuilder info;
			{
				info = new StringBuilder();
				info.Append(xxx.get_pawnname(p) + " has caught " + sd.label + (source != null ? " from " + source + "." : ""));
				if (chance.HasValue)
					info.Append(" (" + chance.Value.ToStringPercent() + " chance)");
				info.AppendLine(); info.AppendLine();
				info.Append(sd.description);
			}
			Find.LetterStack.ReceiveLetter("Infection: " + sd.label, info.ToString(), LetterDefOf.ThreatSmall, p);
		}

		static float GetCleanlinessFactor(Pawn catcher)
		{
			Room room = catcher.GetRoom();
			float cle = room?.GetStat(RoomStatDefOf.Cleanliness) ?? xxx.config.std_outdoor_cleanliness;
			float exa = cle >= 0.0f ? xxx.config.std_env_pitch_cleanliness_exaggeration : xxx.config.std_env_pitch_dirtiness_exaggeration;
			return Mathf.Max(0.0f, 1.0f - exa * cle);
		}

		// Not called anywhere?
		[SyncMethod]
		public static void generate_on(Pawn p)
		{
			if (p == null) return;
			//prevent error on world gen for pawns with broken bodies(no genitals)
			if (p.RaceProps.body.HasPartWithTag(BodyPartTagDefOf.RJW_Fertility))
				return;
			if (!xxx.is_human(p))
				return;
			float nymph_mul = !xxx.is_nympho(p) ? 1.0f : xxx.config.nymph_spawn_with_std_mul;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			foreach (std_def sd in std.all)
				if (Rand.Value < sd.spawn_chance * nymph_mul)
				{
					Hediff hed = infect(p, sd, false);
					float sev;
					{
						float r = Rand.Range(sd.hediff_def.minSeverity, sd.hediff_def.maxSeverity);
						sev = Mathf.Clamp(sd.spawn_severity * r, sd.hediff_def.minSeverity, sd.hediff_def.maxSeverity);
					}
					hed.Severity = sev;
				}
		}
	}
}

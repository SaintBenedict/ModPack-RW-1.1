using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace rjw
{
	/// <summary>
	/// Responsible for judging the sexiness of potential partners.
	/// </summary>
	public static class SexAppraiser
	{
		public const float base_sat_per_fuck = 0.40f;
		public const float base_attraction = 0.60f;
		public const float no_partner_ability = 0.8f;

		static readonly SimpleCurve fuckability_per_reserved = new SimpleCurve
		{
			new CurvePoint(0f, 1.0f),
			new CurvePoint(0.3f, 0.4f),
			new CurvePoint(1f, 0.2f)
		};

		static readonly SimpleCurve attractiveness_from_age_male = new SimpleCurve
		{
			new CurvePoint(0f,  0.0f),
			new CurvePoint(4f,  0.1f),
			new CurvePoint(5f,  0.2f),
			new CurvePoint(15f, 0.8f),
			new CurvePoint(20f, 1.0f),
			new CurvePoint(32f, 1.0f),
			new CurvePoint(40f, 0.9f),
			new CurvePoint(45f, 0.77f),
			new CurvePoint(50f, 0.7f),
			new CurvePoint(55f, 0.5f),
			new CurvePoint(75f, 0.1f),
			new CurvePoint(100f, 0f)
		};

		//These were way too low and could be increased further. Anything under 0.7f pretty much stops sex from happening.
		static readonly SimpleCurve attractiveness_from_age_female = new SimpleCurve
		{
			new CurvePoint(0f,  0.0f),
			new CurvePoint(4f,  0.1f),
			new CurvePoint(5f,  0.2f),
			new CurvePoint(14f, 0.8f),
			new CurvePoint(28f, 1.0f),
			new CurvePoint(30f, 1.0f),
			new CurvePoint(45f, 0.7f),
			new CurvePoint(55f, 0.3f),
			new CurvePoint(75f, 0.1f),
			new CurvePoint(100f, 0f)
		};

		/// <summary>
		///	Returns boolean, no more messing around with floats.
		/// Just a simple 'Would rape? True/false'.
		/// </summary>
		[SyncMethod]
		public static bool would_rape(Pawn rapist, Pawn rapee)
		{
			float rape_factor = 0.3f; // start at 30%

			float vulnerabilityFucker = xxx.get_vulnerability(rapist); //0 to 3
			float vulnerabilityPartner = xxx.get_vulnerability(rapee); //0 to 3

			// More inclined to rape someone from another faction.
			if (rapist.HostileTo(rapee) || rapist.Faction != rapee.Faction)
				rape_factor += 0.25f;

			// More inclined to rape if the target is designated as CP.
			if (rapee.IsDesignatedComfort())
				rape_factor += 0.25f;

			// More inclined to rape when horny.
			Need_Sex horniness = rapist.needs.TryGetNeed<Need_Sex>();
			if (!xxx.is_animal(rapist) && horniness?.CurLevel <= horniness?.thresh_horny())
			{
				rape_factor += 0.25f;
			}

			if (xxx.is_animal(rapist))
			{
				if (vulnerabilityFucker < vulnerabilityPartner)
					rape_factor -= 0.1f;
				else
					rape_factor += 0.25f;
			}
			else if (xxx.is_animal(rapee))
			{
				if (xxx.is_zoophile(rapist))
					rape_factor += 0.5f;
				else
					rape_factor -= 0.2f;
			}
			else
			{
				rape_factor *= 0.5f + Mathf.InverseLerp(vulnerabilityFucker, 3f, vulnerabilityPartner);
			}

			if (rapist.health.hediffSet.HasHediff(HediffDef.Named("AlcoholHigh")))
				rape_factor *= 1.25f; //too drunk to care

			// Increase factor from traits.
			if (xxx.is_rapist(rapist))
				rape_factor *= 1.5f;
			if (xxx.is_nympho(rapist))
				rape_factor *= 1.25f;
			if (xxx.is_bloodlust(rapist))
				rape_factor *= 1.2f;
			if (xxx.is_psychopath(rapist))
				rape_factor *= 1.2f;
			if (xxx.is_masochist(rapee))
				rape_factor *= 1.2f;

			// Lower factor from traits.
			if (xxx.is_masochist(rapist))
				rape_factor *= 0.8f;

			if (rapist.needs.joy != null && rapist.needs.joy.CurLevel < 0.1f) // The rapist is really bored...
				rape_factor *= 1.2f;

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (rapist.relations == null || xxx.is_animal(rapist)) return Rand.Chance(rape_factor);
			int opinion = rapist.relations.OpinionOf(rapee);

			// Won't rape friends, unless rapist or psychopath.
			if (xxx.is_kind(rapist))
			{   //<-80: 1f /-40: 0.5f / 0+: 0f
				rape_factor *= 1f - Mathf.Pow(GenMath.InverseLerp(-80, 0, opinion), 2);
			}
			else if (xxx.is_rapist(rapist) || xxx.is_psychopath(rapist))
			{   //<40: 1f /80: 0.5f / 120+: 0f
				rape_factor *= 1f - Mathf.Pow(GenMath.InverseLerp(40, 120, opinion), 2); // This can never be 0, since opinion caps at 100.
			}
			else
			{   //<-60: 1f /-20: 0.5f / 40+: 0f
				rape_factor *= 1f - Mathf.Pow(GenMath.InverseLerp(-60, 40, opinion), 2);
			}

			//Log.Message("rjw::xxx rape_factor for " + get_pawnname(rapee) + " is " + rape_factor);

			return Rand.Chance(rape_factor);
		}

		public static float would_fuck(Pawn fucker, Corpse fucked, bool invert_opinion = false, bool ignore_bleeding = false, bool ignore_gender = false)
		{
			CompRottable comp = fucked.GetComp<CompRottable>();

			//--Log.Message("rotFactor:" + rotFactor);

			// Things that don't rot, such as mechanoids and weird mod-added stuff such as Rimworld of Magic's elementals.
			if (comp == null)
			{
				// Trying to necro the weird mod-added stuff causes an error, so skipping those for now.
				return 0.0f;
			}

			float maxRot = ((CompProperties_Rottable)comp.props).TicksToDessicated;
			float rotFactor = (maxRot - comp.RotProgress) / maxRot;
			//--Log.Message("rotFactor:" + rotFactor);
			return would_fuck(fucker, fucked.InnerPawn, invert_opinion, ignore_bleeding, ignore_gender) * rotFactor;
		}

		public static float would_fuck_animal(Pawn pawn, Pawn target, bool invert_opinion = false, bool ignore_bleeding = false, bool ignore_gender = false)
		{
			float wildness_modifier = 1.0f;
			List<float> size_preference = new List<float>() { pawn.BodySize * 0.75f, pawn.BodySize * 1.6f };
			float fuc = would_fuck(pawn, target, invert_opinion, ignore_bleeding, ignore_gender); // 0.0 to ~3.0, orientation checks etc.

			if (fuc < 0.1f)
			{   // Would not fuck
				return 0;
			}

			if (xxx.has_quirk(pawn, "Teratophile"))
			{   // Teratophiles prefer more 'monstrous' partners.
				size_preference[0] = pawn.BodySize * 0.8f;
				size_preference[1] = pawn.BodySize * 2.0f;
				wildness_modifier = 0.3f;
			}
			if (pawn.health.hediffSet.HasHediff(HediffDef.Named("AlcoholHigh")))
			{
				wildness_modifier = 0.5f; //Drunk and making poor judgments.
				size_preference[1] *= 1.5f;
			}
			else if (pawn.health.hediffSet.HasHediff(HediffDef.Named("YayoHigh")))
			{
				wildness_modifier = 0.2f; //This won't end well.
				size_preference[1] *= 2.5f;
			}
			var partBPR = Genital_Helper.get_genitalsBPR(pawn);
			var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);
			if (Genital_Helper.has_vagina(pawn, parts) || Genital_Helper.has_anus(pawn))
			{
				if (!(Genital_Helper.has_penis_fertile(pawn, parts) || Genital_Helper.has_penis_infertile(pawn, parts)))
				{
					size_preference[1] = pawn.BodySize * 1.3f;
				}
			}
			if (xxx.is_animal(pawn))
			{
				size_preference[1] = pawn.BodySize * 1.3f;
				wildness_modifier = 0.4f;
			}
			else
			{
				if (pawn.story.traits.HasTrait(TraitDefOf.Tough) || pawn.story.traits.HasTrait(TraitDefOf.Brawler))
				{
					size_preference[1] += 0.2f;
					wildness_modifier -= 0.2f;
				}
				else if (pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
				{
					size_preference[0] -= 0.2f;
					size_preference[1] -= 0.2f;
					wildness_modifier += 0.25f;
				}
			}

			float wildness = target.RaceProps.wildness; // 0.0 to 1.0
			float petness = target.RaceProps.petness; // 0.0 to 1.0
			float distance = pawn.Position.DistanceToSquared(target.Position);

			//Log.Message("[RJW]would_fuck_animal:: base: " + fuc + ", wildness: " + wildness + ", petness: " + petness + ", distance: " + distance);

			fuc = fuc + fuc * petness - fuc * wildness * wildness_modifier;

			if (fuc < 0.1f)
			{   // Would not fuck
				return 0;
			}

			// Adjust by distance, nearby targets preferred.
			fuc *= 1.0f - Mathf.Max(distance / 10000, 0.1f);

			// Adjust by size difference.
			if (target.BodySize < size_preference[0])
			{
				fuc *= Mathf.Lerp(0.1f, size_preference[0], target.BodySize);
			}
			else if (target.BodySize > size_preference[1])
			{
				fuc *= Mathf.Lerp(size_preference[1] * 10, size_preference[1], target.BodySize);
			}

			if (target.Faction != pawn.Faction)
			{
				//Log.Message("[RJW]would_fuck_animal(NT):: base: " + fuc + ", bound1: " + fuc * 0.75f);
				//Log.Message("[RJW]would_fuck_animal(NT):: base: " + fuc + ", bound2: " + fuc + 0.25f);
				fuc *= 0.75f; // Less likely to target wild animals.
			}
			else if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, target))
			{
				//Log.Message("[RJW]would_fuck_animal(T):: base: " + fuc + ", bound1: " + fuc * 1.25f);
				//Log.Message("[RJW]would_fuck_animal(T):: base: " + fuc + ", bound2: " + fuc + 0.25f);
				fuc *= 1.25f; // Bonded animals preferred.
			}

			return fuc;
		}

		// Returns how fuckable 'fucker' thinks 'p' is on a scale from 0.0 to 1.0
		public static float would_fuck(Pawn fucker, Pawn fucked, bool invert_opinion = false, bool ignore_bleeding = false, bool ignore_gender = false)
		{
			//--Log.Message("[RJW]would_fuck("+xxx.get_pawnname(fucker)+","+xxx.get_pawnname(fucked)+","+invert_opinion.ToString()+") is called");
			if (!xxx.is_healthy_enough(fucker) && !xxx.is_psychopath(fucker)) // Not healthy enough to have sex, shouldn't have got this far.
				return 0f;
			if ((xxx.is_animal(fucker) || xxx.is_animal(fucked)) && (!xxx.is_animal(fucker) || !xxx.is_animal(fucked)) && !RJWSettings.bestiality_enabled)
				return 0f; // Animals disabled.
			if (fucked.Dead && !RJWSettings.necrophilia_enabled)
				return 0f; // Necrophilia disabled.
			if (fucker.Dead || fucker.Suspended || fucked.Suspended)
				return 0f; // Target unreachable. Shouldn't have got this far, but doesn't hurt to double-check.
			if (xxx.is_starved(fucked) && fucked.Faction == fucker.Faction && !xxx.is_psychopath(fucker) && !xxx.is_rapist(fucker))
				return 0f;
			if (!ignore_bleeding && !xxx.is_not_dying(fucked) && !xxx.is_psychopath(fucker) && !xxx.is_rapist(fucker) && !xxx.is_bloodlust(fucker))
				return 0f; // Most people wouldn't fuck someone who's dying.

			if (!IsGenderOk(fucker, fucked))
			{
				return 0.0f;
			}

			int fucker_age = fucker.ageTracker.AgeBiologicalYears;
			string fucker_quirks = CompRJW.Comp(fucker).quirks.ToString();
			int p_age = fucked.ageTracker.AgeBiologicalYears;

			// --- Age checks ---
			if (!IsAgeOk(fucker, fucked, fucker_age, p_age))
			{
				return 0.0f;
			}
			float age_factor = GetAgeFactor(fucker, fucked, p_age);

			// --- Orientation checks ---
			float orientation_factor = GetOrientationFactor(fucker, fucked, ignore_gender);
			if (orientation_factor == 0.0f)
			{
				orientation_factor = fucker.relations.SecondaryLovinChanceFactor(fucked);
				//Log.Message("would_fuck() SecondaryLovinChanceFactor:" + orientation_factor);
				if (orientation_factor <= 0)
					return 0.0f;
			}

			// --- Body and appearance checks ---
			float body_factor = GetBodyFactor(fucker, fucked);

			// --- Opinion checks ---
			float opinion_factor = GetOpinionFactor(fucker, fucked, invert_opinion);
			float horniness_factor = GetHorninessFactor(fucker);

			float reservedPercentage = (fucked.Dead ? 1f : fucked.ReservedCount()) / xxx.max_rapists_per_prisoner;
			//Log.Message("would_fuck() reservedPercentage:" + reservedPercentage + "fuckability_per_reserved"+ fuckability_per_reserved.Evaluate(reservedPercentage));
			//Log.Message("would_fuck() - horniness_factor = " + horniness_factor.ToString());

			float prenymph_att = Mathf.InverseLerp(0f, 2.8f, base_attraction * orientation_factor * horniness_factor * age_factor * body_factor * opinion_factor);
			float final_att = !(xxx.is_nympho(fucker) || fucker.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomEffect"))) ? prenymph_att : 0.2f + 0.8f * prenymph_att;
			//Log.Message("would_fuck( " + xxx.get_pawnname(fucker) + ", " + xxx.get_pawnname(fucked) + " ) - prenymph_att = " + prenymph_att.ToString() + ", final_att = " + final_att.ToString());

			return Mathf.Min(final_att, fuckability_per_reserved.Evaluate(reservedPercentage));
		}

		private static bool IsGenderOk(Pawn fucker, Pawn fucked)
		{
			if (fucker.gender == Gender.Male)
			{
				switch (RJWPreferenceSettings.Malesex)
				{
					case RJWPreferenceSettings.AllowedSex.All:
						break;
					case RJWPreferenceSettings.AllowedSex.Homo:
						if (fucked.gender != Gender.Male)
							return false;
						break;
					case RJWPreferenceSettings.AllowedSex.Nohomo:
						if (fucked.gender == Gender.Male)
							return false;
						break;
				}
			}
			if (fucker.gender == Gender.Female)
			{
				switch (RJWPreferenceSettings.FeMalesex)
				{
					case RJWPreferenceSettings.AllowedSex.All:
						break;
					case RJWPreferenceSettings.AllowedSex.Homo:
						if (fucked.gender != Gender.Female)
							return false;
						break;
					case RJWPreferenceSettings.AllowedSex.Nohomo:
						if (fucked.gender == Gender.Female)
							return false;
						break;
				}
			}
			return true;
		}

		private static float GetHorninessFactor(Pawn fucker)
		{
			float horniness_factor; // 1 to 1.6
			{
				float need_sex = xxx.need_some_sex(fucker);
				switch (need_sex)
				{
					case 3:
						horniness_factor = 1.6f;
						break;

					case 2:
						horniness_factor = 1.3f;
						break;

					case 1:
						horniness_factor = 1.1f;
						break;

					default:
						horniness_factor = 1f;
						break;
				}
			}
			//Log.Message("would_fuck() - horniness_factor = " + horniness_factor.ToString());
			return horniness_factor;
		}

		private static float GetOpinionFactor(Pawn fucker, Pawn fucked, bool invert_opinion)
		{
			float opinion_factor;
			{
				if (fucked.relations != null && fucker.relations != null && !xxx.is_animal(fucker) && !xxx.is_animal(fucked))
				{
					float opi = !invert_opinion ? fucker.relations.OpinionOf(fucked) : 100 - fucker.relations.OpinionOf(fucked); // -100 to 100
					opinion_factor = 0.8f + (opi + 100.0f) * (.45f / 200.0f); // 0.8 to 1.25
				}
				else if ((xxx.is_animal(fucker) || xxx.is_animal(fucked)) && fucker.relations.DirectRelationExists(PawnRelationDefOf.Bond, fucked))
				{
					opinion_factor = 1.3f;
				}
				else
				{
					opinion_factor = 1.0f;
				}

				// More likely to take advantege of CP.
				if (fucked.IsDesignatedComfort() || (fucked.IsDesignatedBreeding() && xxx.is_animal(fucker)))
					opinion_factor += 0.25f;
				else if (fucked.IsDesignatedService())
					opinion_factor += 0.1f;

				// Less picky if designated for whorin'.
				if (fucker.IsDesignatedService())
					opinion_factor += 0.1f;

				if (Quirk.Sapiosexual.IsSatisfiedBy(fucker, fucked))
				{
					opinion_factor *= 1.4f;
				}
			}
			//Log.Message("would_fuck() - opinion_factor = " + opinion_factor.ToString());
			return opinion_factor;
		}

		private static float GetBodyFactor(Pawn fucker, Pawn fucked)
		{
			float body_factor; //0.4 to 1.6
			{
				if (fucker.health.hediffSet.HasHediff(HediffDef.Named("AlcoholHigh")))
				{
					if (!xxx.is_zoophile(fucker) && xxx.is_animal(fucked))
						body_factor = 0.8f;
					else
						body_factor = 1.25f; //beer lens
				}
				else if (xxx.is_zoophile(fucker) && !xxx.is_animal(fucked))
				{
					body_factor = 0.7f;
				}
				else if (!xxx.is_zoophile(fucker) && xxx.is_animal(fucked))
				{
					body_factor = 0.45f;
				}
				else if (Quirk.Teratophile.IsSatisfiedBy(fucker, fucked))
				{
					body_factor = 1.4f;
				}
				else if (fucked.story != null)
				{
					if (fucked.story.bodyType == BodyTypeDefOf.Female) body_factor = 1.25f;
					else if (fucked.story.bodyType == BodyTypeDefOf.Fat) body_factor = 1.0f;
					else body_factor = 1.1f;

					if (fucked.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
						body_factor *= 0.9f;

					if (fucked.story.traits.HasTrait(TraitDefOf.Beauty))
					{
						switch (fucked.story.traits.DegreeOfTrait(TraitDefOf.Beauty))
						{
							case 2: // Beautiful
								body_factor *= 1.25f;
								break;
							case 1: // Pretty
								body_factor *= 1.1f;
								break;
							case -1: // Ugly
								body_factor *= 0.8f;
								break;
							case -2: // Staggeringly Ugly
								body_factor *= 0.5f;
								break;
						}
					}

					if (RelationsUtility.IsDisfigured(fucked))
					{
						body_factor *= 0.8f;
					}

					// Nude target is more tempting.
					if (!fucked.Dead && fucked.apparel.PsychologicallyNude && fucker.CanSee(fucked))
						body_factor *= 1.1f;
				}
				else
				{
					body_factor = 1.1f;
				}

				if (Quirk.Somnophile.IsSatisfiedBy(fucker, fucked))
				{
					body_factor *= 1.25f;
				}

				if (Quirk.PregnancyFetish.IsSatisfiedBy(fucker, fucked))
				{
					body_factor *= 1.25f;
				}

				if (Quirk.ImpregnationFetish.IsSatisfiedBy(fucker, fucked))
				{
					body_factor *= 1.25f;
				}

				if (xxx.AlienFrameworkIsActive && !xxx.is_animal(fucker))
				{
					if (xxx.is_xenophile(fucker))
					{
						if (fucker.def.defName == fucked.def.defName)
							body_factor *= 0.5f; // Same species, xenophile less interested.
					}
					else if (xxx.is_xenophobe(fucker))
					{
						if (fucker.def.defName != fucked.def.defName)
							body_factor *= 0.25f; // Different species, xenophobe less interested.
					}
				}

				if (fucked.Dead && !xxx.is_necrophiliac(fucker))
				{
					body_factor *= 0.5f;
				}
			}
			//Log.Message("would_fuck() - body_factor = " + body_factor.ToString());
			return body_factor;
		}

		private static float GetOrientationFactor(Pawn fucker, Pawn fucked, bool ignore_gender)
		{
			float orientation_factor; //0 or 1
			{
				orientation_factor = 1.0f;

				if (!ignore_gender && !xxx.is_animal(fucker))
				{
					if (!CompRJW.CheckPreference(fucker, fucked))
					{
						//Log.Message("would_fuck( " + xxx.get_pawnname(fucker) + ", " + xxx.get_pawnname(fucked) + " )");
						//Log.Message("would_fuck() - preference fail");
						orientation_factor = 0.0f;
					}
				}
			}
			//Log.Message("would_fuck() - orientation_factor = " + orientation_factor.ToString());
			return orientation_factor;
		}

		private static float GetAgeFactor(Pawn fucker, Pawn fucked, int p_age)
		{
			float age_factor;

			//The human age curve needs work. Currently pawns refuse to have sex with anyone over age of ~50 no matter what the other factors are, which is just silly...
			age_factor = fucked.gender == Gender.Male ? attractiveness_from_age_male.Evaluate(SexUtility.ScaleToHumanAge(fucked)) : attractiveness_from_age_female.Evaluate(SexUtility.ScaleToHumanAge(fucked));
			//--Log.Message("would_fuck() - age_factor = " + age_factor.ToString());

			if (xxx.is_animal(fucker))
			{
				age_factor = 1.0f;  //using flat factors, since human age is not comparable to animal ages
			}
			else if (xxx.is_animal(fucked))
			{
				if (p_age <= 1 && fucked.RaceProps.lifeExpectancy > 8)
					age_factor = 0.5f;
				else
					age_factor = 1.0f;
				//--Log.Message("would_fuck() - animal age_factor = " + age_factor.ToString());
			}
			if (Quirk.Gerontophile.IsSatisfiedBy(fucker, fucked))
			{
				age_factor = 1.0f;
			}

			return age_factor;
		}

		private static bool IsAgeOk(Pawn fucker, Pawn fucked, int fucker_age, int p_age)
		{
			bool age_ok;
			{
				if (xxx.is_animal(fucker) && p_age >= RJWSettings.sex_minimum_age)
				{
					age_ok = true;
				}
				else if (xxx.is_animal(fucked) && fucker_age >= RJWSettings.sex_minimum_age)
				{
					// don't check the age of animals when they are the victim
					age_ok = true;
				}
				else if (fucker_age >= RJWSettings.sex_free_for_all_age && p_age >= RJWSettings.sex_free_for_all_age)
				{
					age_ok = true;
				}
				else if (fucker_age < RJWSettings.sex_minimum_age || p_age < RJWSettings.sex_minimum_age)
				{
					age_ok = false;
				}
				else
				{
					age_ok = Math.Abs(fucker.ageTracker.AgeBiologicalYearsFloat - fucked.ageTracker.AgeBiologicalYearsFloat) < 2.05f;
				}
			}
			//Log.Message("would_fuck() - age_ok = " + age_ok.ToString());

			return age_ok;
		}

		static int ReservedCount(this Thing pawn)
		{
			int ret = 0;
			if (pawn == null) return 0;
			try
			{
				ReservationManager reserver = pawn.Map.reservationManager;
				IList reservations = (IList)AccessTools.Field(typeof(ReservationManager), "reservations").GetValue(reserver);

				if (reservations.Count == 0) return 0;
				Type reserveType = reservations[0].GetType();
				ret += (from object t in reservations
						where t != null
						let target = (LocalTargetInfo)AccessTools.Field(reserveType, "target").GetValue(t)
						let claimant = (Pawn)AccessTools.Field(reserveType, "claimant").GetValue(t)
						where target != null
						where target.Thing != null
						where target.Thing.ThingID == pawn.ThingID
						select (int)AccessTools.Field(reserveType, "stackCount").GetValue(t)).Count();
			}
			catch (Exception e)
			{
				Log.Warning(e.ToString());
			}
			return ret;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Multiplayer.API;
//using static RimWorld.Planet.CaravanInventoryUtility;
//using RimWorldChildren;

namespace rjw
{
	public static class xxx
	{
		public static readonly BindingFlags ins_public_or_no = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static readonly config config = DefDatabase<config>.GetNamed("the_one");

		//HARDCODED MAGIC USED ACROSS DOZENS OF FILES, this is as bad place to put it as any other
		//Should at the very least be encompassed in the related designation type
		public static readonly int max_rapists_per_prisoner = 6;

		public static readonly TraitDef nymphomaniac = TraitDef.Named("Nymphomaniac");
		public static readonly TraitDef rapist = TraitDef.Named("Rapist");
		public static readonly TraitDef masochist = TraitDef.Named("Masochist");
		public static readonly TraitDef necrophiliac = TraitDef.Named("Necrophiliac");
		public static readonly TraitDef zoophile = TraitDef.Named("Zoophile");

		//CombatExtended Traits
		public static HediffDef MuscleSpasms;
		public static bool CombatExtendedIsActive;

		//RomanceDiversified Traits
		public static TraitDef straight;
		public static TraitDef faithful;
		public static TraitDef philanderer;
		public static TraitDef polyamorous;
		public static bool RomanceDiversifiedIsActive; //A dirty way to check if the mod is active

		//Psychology Traits
		public static bool PsychologyIsActive;
		public static TraitDef prude;
		public static TraitDef lecher;
		public static TraitDef polygamous;

		//[SYR] Individuality
		public static bool IndividualityIsActive;
		public static TraitDef SYR_CreativeThinker;
		public static TraitDef SYR_Haggler;

		//Rimworld of Magic
		public static bool RoMIsActive;

		//Consolidated Traits
		public static bool CTIsActive;

		//SimpleSlavery
		public static bool SimpleSlaveryIsActive;

		//Dubs Bad Hygiene
		public static bool DubsBadHygieneIsActive;

		//Alien Framework Traits
		public static TraitDef xenophobia; // Degrees: 1: xenophobe, -1: xenophile
		public static bool AlienFrameworkIsActive;

		//Children&Pregnancy Hediffs
		public static HediffDef babystate;
		public static bool RimWorldChildrenIsActive; //A dirty way to check if the mod is active

		//Children&Pregnancy Hediffs
		public static HediffDef IH_Immortal;
		public static bool ImmortalsIsActive; //A dirty way to check if the mod is active

		//The Hediff to prevent reproduction
		public static readonly HediffDef sterilized = HediffDef.Named("Sterilized");

		//The Hediff for broken body(resulted from being raped as CP for too many times)
		public static readonly HediffDef feelingBroken = HediffDef.Named("FeelingBroken");

		public static PawnCapacityDef reproduction = DefDatabase<PawnCapacityDef>.GetNamed("RJW_Fertility");

		public static readonly BodyPartDef genitalsDef = DefDatabase<BodyPartDef>.GetNamed("Genitals");
		public static readonly BodyPartDef breastsDef = DefDatabase<BodyPartDef>.GetNamed("Chest");
		public static readonly BodyPartDef anusDef = DefDatabase<BodyPartDef>.GetNamed("Anus");

		public static readonly ThoughtDef saw_rash_1 = DefDatabase<ThoughtDef>.GetNamed("SawDiseasedPrivates1");
		public static readonly ThoughtDef saw_rash_2 = DefDatabase<ThoughtDef>.GetNamed("SawDiseasedPrivates2");
		public static readonly ThoughtDef saw_rash_3 = DefDatabase<ThoughtDef>.GetNamed("SawDiseasedPrivates3");

		public static readonly ThoughtDef got_raped = DefDatabase<ThoughtDef>.GetNamed("GotRaped");
		public static readonly ThoughtDef got_raped_unconscious = DefDatabase<ThoughtDef>.GetNamed("GotRapedUnconscious");
		public static readonly ThoughtDef masochist_got_raped_unconscious = DefDatabase<ThoughtDef>.GetNamed("MasochistGotRapedUnconscious");
		public static readonly ThoughtDef got_bred = DefDatabase<ThoughtDef>.GetNamed("GotBredByAnimal");
		public static readonly ThoughtDef got_licked = DefDatabase<ThoughtDef>.GetNamed("GotLickedByAnimal");
		public static readonly ThoughtDef got_groped = DefDatabase<ThoughtDef>.GetNamed("GotGropedByAnimal");

		public static readonly ThoughtDef masochist_got_raped = DefDatabase<ThoughtDef>.GetNamed("MasochistGotRaped");
		public static readonly ThoughtDef masochist_got_bred = DefDatabase<ThoughtDef>.GetNamed("MasochistGotBredByAnimal");
		public static readonly ThoughtDef masochist_got_licked = DefDatabase<ThoughtDef>.GetNamed("MasochistGotLickedByAnimal");
		public static readonly ThoughtDef masochist_got_groped = DefDatabase<ThoughtDef>.GetNamed("MasochistGotGropedByAnimal");
		public static readonly ThoughtDef allowed_animal_to_breed = DefDatabase<ThoughtDef>.GetNamed("AllowedAnimalToBreed");
		public static readonly ThoughtDef allowed_animal_to_lick = DefDatabase<ThoughtDef>.GetNamed("AllowedAnimalToLick");
		public static readonly ThoughtDef allowed_animal_to_grope = DefDatabase<ThoughtDef>.GetNamed("AllowedAnimalToGrope");
		public static readonly ThoughtDef zoophile_got_bred = DefDatabase<ThoughtDef>.GetNamed("ZoophileGotBredByAnimal");
		public static readonly ThoughtDef zoophile_got_licked = DefDatabase<ThoughtDef>.GetNamed("ZoophileGotLickedByAnimal");
		public static readonly ThoughtDef zoophile_got_groped = DefDatabase<ThoughtDef>.GetNamed("ZoophileGotGropedByAnimal");
		public static readonly ThoughtDef hate_my_rapist = DefDatabase<ThoughtDef>.GetNamed("HateMyRapist");
		public static readonly ThoughtDef kinda_like_my_rapist = DefDatabase<ThoughtDef>.GetNamed("KindaLikeMyRapist");
		public static readonly ThoughtDef allowed_me_to_get_raped = DefDatabase<ThoughtDef>.GetNamed("AllowedMeToGetRaped");
		public static readonly ThoughtDef stole_some_lovin = DefDatabase<ThoughtDef>.GetNamed("StoleSomeLovin");
		public static readonly ThoughtDef bloodlust_stole_some_lovin = DefDatabase<ThoughtDef>.GetNamed("BloodlustStoleSomeLovin");
		public static readonly ThoughtDef violated_corpse = DefDatabase<ThoughtDef>.GetNamed("ViolatedCorpse");
		public static readonly ThoughtDef gave_virginity = DefDatabase<ThoughtDef>.GetNamed("FortunateGaveVirginity");
		public static readonly ThoughtDef lost_virginity = DefDatabase<ThoughtDef>.GetNamed("UnfortunateLostVirginity");
		public static readonly ThoughtDef took_virginity = DefDatabase<ThoughtDef>.GetNamed("TookVirginity");

		public static readonly JobDef Masturbate_Bed = DefDatabase<JobDef>.GetNamed("Masturbate_Bed");
		public static readonly JobDef Masturbate_Quick = DefDatabase<JobDef>.GetNamed("Masturbate_Quick");
		public static readonly JobDef casual_sex = DefDatabase<JobDef>.GetNamed("JoinInBed");
		public static readonly JobDef gettin_loved = DefDatabase<JobDef>.GetNamed("GettinLoved");
		public static readonly JobDef gettin_raped = DefDatabase<JobDef>.GetNamed("GettinRaped");
		public static readonly JobDef gettin_bred = DefDatabase<JobDef>.GetNamed("GettinBred");
		public static readonly JobDef RapeCP = DefDatabase<JobDef>.GetNamed("RapeComfortPawn");
		public static readonly JobDef RapeEnemy = DefDatabase<JobDef>.GetNamed("RapeEnemy");
		public static readonly JobDef RapeRandom = DefDatabase<JobDef>.GetNamed("RandomRape");
		public static readonly JobDef RapeCorpse = DefDatabase<JobDef>.GetNamed("ViolateCorpse");
		public static readonly JobDef bestiality = DefDatabase<JobDef>.GetNamed("Bestiality");
		public static readonly JobDef bestialityForFemale = DefDatabase<JobDef>.GetNamed("BestialityForFemale");
		public static readonly JobDef whore_inviting_visitors = DefDatabase<JobDef>.GetNamed("WhoreInvitingVisitors");
		public static readonly JobDef whore_is_serving_visitors = DefDatabase<JobDef>.GetNamed("WhoreIsServingVisitors");
		public static readonly JobDef animalBreed = DefDatabase<JobDef>.GetNamed("Breed");
		public static readonly JobDef quick_sex = DefDatabase<JobDef>.GetNamed("Quickie");
		public static readonly JobDef getting_quickie = DefDatabase<JobDef>.GetNamed("GettingQuickie");
		public static readonly JobDef struggle_in_BondageGear = DefDatabase<JobDef>.GetNamed("StruggleInBondageGear");
		public static readonly JobDef unlock_BondageGear = DefDatabase<JobDef>.GetNamed("UnlockBondageGear");
		public static readonly JobDef give_BondageGear = DefDatabase<JobDef>.GetNamed("GiveBondageGear");

		public static readonly ThingDef mote_noheart = ThingDef.Named("Mote_NoHeart");

		public static readonly StatDef sex_stat = StatDef.Named("SexAbility");
		public static readonly StatDef vulnerability_stat = StatDef.Named("Vulnerability");
		public static readonly StatDef sex_drive_stat = StatDef.Named("SexFrequency");

		public static readonly RecordDef GetRapedAsComfortPawn = DefDatabase<RecordDef>.GetNamed("GetRapedAsComfortPrisoner");
		public static readonly RecordDef CountOfFappin = DefDatabase<RecordDef>.GetNamed("CountOfFappin");
		public static readonly RecordDef CountOfWhore = DefDatabase<RecordDef>.GetNamed("CountOfWhore");
		public static readonly RecordDef EarnedMoneyByWhore = DefDatabase<RecordDef>.GetNamed("EarnedMoneyByWhore");
		public static readonly RecordDef CountOfSex = DefDatabase<RecordDef>.GetNamed("CountOfSex");
		public static readonly RecordDef CountOfSexWithHumanlikes = DefDatabase<RecordDef>.GetNamed("CountOfSexWithHumanlikes");
		public static readonly RecordDef CountOfSexWithAnimals = DefDatabase<RecordDef>.GetNamed("CountOfSexWithAnimals");
		public static readonly RecordDef CountOfSexWithInsects = DefDatabase<RecordDef>.GetNamed("CountOfSexWithInsects");
		public static readonly RecordDef CountOfSexWithOthers = DefDatabase<RecordDef>.GetNamed("CountOfSexWithOthers");
		public static readonly RecordDef CountOfSexWithCorpse = DefDatabase<RecordDef>.GetNamed("CountOfSexWithCorpse");
		public static readonly RecordDef CountOfRapedHumanlikes = DefDatabase<RecordDef>.GetNamed("CountOfRapedHumanlikes");
		public static readonly RecordDef CountOfBeenRapedByHumanlikes = DefDatabase<RecordDef>.GetNamed("CountOfBeenRapedByHumanlikes");
		public static readonly RecordDef CountOfRapedAnimals = DefDatabase<RecordDef>.GetNamed("CountOfRapedAnimals");
		public static readonly RecordDef CountOfBeenRapedByAnimals = DefDatabase<RecordDef>.GetNamed("CountOfBeenRapedByAnimals");
		public static readonly RecordDef CountOfRapedInsects = DefDatabase<RecordDef>.GetNamed("CountOfRapedInsects");
		public static readonly RecordDef CountOfBeenRapedByInsects = DefDatabase<RecordDef>.GetNamed("CountOfBeenRapedByInsects");
		public static readonly RecordDef CountOfRapedOthers = DefDatabase<RecordDef>.GetNamed("CountOfRapedOthers");
		public static readonly RecordDef CountOfBeenRapedByOthers = DefDatabase<RecordDef>.GetNamed("CountOfBeenRapedByOthers");
		public static readonly RecordDef CountOfBirthHuman = DefDatabase<RecordDef>.GetNamed("CountOfBirthHuman");
		public static readonly RecordDef CountOfBirthAnimal = DefDatabase<RecordDef>.GetNamed("CountOfBirthAnimal");
		public static readonly RecordDef CountOfBirthEgg = DefDatabase<RecordDef>.GetNamed("CountOfBirthEgg");

		public enum rjwSextype { None, Vaginal, Anal, Oral, Masturbation, DoublePenetration, Boobjob, Handjob, Footjob, Fingering, Scissoring, MutualMasturbation, Fisting, MechImplant }

		public static void bootstrap(Map m)
		{
			if (m.GetComponent<MapCom_Injector>() == null)
				m.components.Add(new MapCom_Injector(m));
		}

		//<Summary>Simple method that quickly checks for match from a list.</Summary>
		public static bool ContainsAny(this string haystack, params string[] needles) { return needles.Any(haystack.Contains); }

		public static bool has_traits(Pawn pawn)
		{
			return pawn?.story?.traits != null;
		}

		public static bool has_quirk(Pawn pawn, string quirk)
		{
			return pawn != null && is_human(pawn) && CompRJW.Comp(pawn).quirks.ToString().Contains(quirk);
		}

		[SyncMethod]
		public static string random_pick_a_trait(this Pawn pawn)
		{
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			return has_traits(pawn) ? pawn.story.traits.allTraits.RandomElement().def.defName : null;
		}

		public static bool is_psychopath(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDefOf.Psychopath);
		}

		public static bool is_ascetic(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDefOf.Ascetic);
		}

		public static bool is_bloodlust(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDefOf.Bloodlust);
		}

		public static bool is_brawler(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDefOf.Brawler);
		}

		public static bool is_kind(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDefOf.Kind);
		}

		public static bool is_rapist(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(rapist);
		}

		public static bool is_necrophiliac(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(necrophiliac);
		}

		public static bool is_zoophile(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(zoophile);
		}

		public static bool is_masochist(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(TraitDef.Named("Masochist"));
		}

		/// <summary>
		/// Returns true if the given pawn has the nymphomaniac trait.
		/// This may or may not be a nymph pawnKind.
		/// </summary>
		public static bool is_nympho(Pawn pawn)
		{
			return has_traits(pawn) && pawn.story.traits.HasTrait(nymphomaniac);
		}

		public static bool is_slime(Pawn pawn)
		{
			string racename = pawn.kindDef.race.defName.ToLower();
			//if (Prefs.DevMode) Log.Message("[RJW] is_slime " + xxx.get_pawnname(pawn) + " " + racename.Contains("slime"));

			return racename.Contains("slime");
		}

		public static bool is_demon(Pawn pawn)
		{
			string racename = pawn.kindDef.race.defName.ToLower();
			//if (Prefs.DevMode) Log.Message("[RJW] is_demon " + xxx.get_pawnname(pawn) + " " + racename.Contains("demon"));

			return racename.Contains("demon");
		}

		public static bool is_asexual(Pawn pawn) => CompRJW.Comp(pawn).orientation == Orientation.Asexual;
		public static bool is_bisexual(Pawn pawn) => CompRJW.Comp(pawn).orientation == Orientation.Bisexual;
		public static bool is_homosexual(Pawn pawn) => (CompRJW.Comp(pawn).orientation == Orientation.Homosexual || CompRJW.Comp(pawn).orientation == Orientation.MostlyHomosexual);
		public static bool is_heterosexual(Pawn pawn) => (CompRJW.Comp(pawn).orientation == Orientation.Heterosexual || CompRJW.Comp(pawn).orientation == Orientation.MostlyHeterosexual);
		public static bool is_pansexual(Pawn pawn) => CompRJW.Comp(pawn).orientation == Orientation.Pansexual;

		public static bool is_slave(Pawn pawn)
		{
			if (SimpleSlaveryIsActive)
				return pawn?.health.hediffSet.HasHediff(HediffDef.Named("Enslaved")) ?? false;
			else
				return false;
		}

		public static bool is_nympho_or_rapist_or_zoophile(Pawn pawn)
		{
			if (!has_traits(pawn)) { return false; }
			return (is_rapist(pawn) || is_nympho(pawn) || is_zoophile(pawn));
		}

		//Humanoid Alien Framework traits
		public static bool is_xenophile(Pawn pawn)
		{
			if (!has_traits(pawn) || !AlienFrameworkIsActive) { return false; }
			return pawn.story.traits.DegreeOfTrait(xenophobia) == -1;
		}

		public static bool is_xenophobe(Pawn pawn)
		{
			if (!has_traits(pawn) || !AlienFrameworkIsActive) { return false; }
			return pawn.story.traits.DegreeOfTrait(xenophobia) == 1;
		}

		public static bool is_whore(Pawn pawn)
		{
			if (!has_traits(pawn)) { return false; }
			return pawn != null && pawn.IsDesignatedService() && !pawn.story.traits.HasTrait(TraitDefOf.Asexual);
			//return (pawn != null && pawn.ownership != null && pawn.ownership.OwnedBed is Building_WhoreBed && (!xxx.RomanceDiversifiedIsActive || !pawn.story.traits.HasTrait(xxx.asexual)));
		}

		public static bool is_lecher(Pawn pawn)
		{
			if (!has_traits(pawn)) { return false; }
			return RomanceDiversifiedIsActive && pawn.story.traits.HasTrait(philanderer) || PsychologyIsActive && pawn.story.traits.HasTrait(lecher);
		}

		public static bool is_prude(Pawn pawn)
		{
			if (!has_traits(pawn)) { return false; }
			return RomanceDiversifiedIsActive && pawn.story.traits.HasTrait(faithful) || PsychologyIsActive && pawn.story.traits.HasTrait(prude);
		}

		public static bool is_animal(Pawn pawn)
		{
			return pawn?.RaceProps?.Animal ?? false;
		}

		public static bool is_insect(Pawn pawn)
		{
			if (pawn == null) return false;
			bool isit = pawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid
						|| pawn.RaceProps.FleshType.corpseCategory.ToString().Contains("CorpsesInsect")
						//genetic rim
						|| pawn.RaceProps.FleshType.defName.Contains("GR_Insectoid");
			//Log.Message("is_insect " + get_pawnname(pawn) + " - " + isit);
			return isit;
		}

		public static bool is_mechanoid(Pawn pawn)
		{
			if (pawn == null) return false;
			if (AndroidsCompatibility.IsAndroid(pawn)) return false;

			bool isit = pawn.RaceProps.IsMechanoid
						|| pawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid
						|| pawn.RaceProps.FleshType.corpseCategory.ToString().Contains("CorpsesMechanoid")
						//genetic rim
						|| pawn.RaceProps.FleshType.defName.Contains("GR_Mechanoid")
						//android tiers
						|| pawn.RaceProps.FleshType.defName.Contains("MechanisedInfantry")
						|| pawn.RaceProps.FleshType.defName.Contains("Android")
						;
			//Log.Message("is_mechanoid " + get_pawnname(pawn) + " - " + isit);
			return isit;
		}

		public static bool is_tooluser(Pawn pawn)
		{
			return pawn.RaceProps.ToolUser;
		}

		public static bool is_human(Pawn pawn)
		{
			if (pawn == null) return false;
			return pawn.RaceProps.Humanlike;
		}

		public static bool is_female(Pawn pawn)
		{
			return pawn.gender == Gender.Female;
		}
		public static bool is_male(Pawn pawn)
		{
			return pawn.gender == Gender.Male;
		}

		public static bool is_healthy(Pawn pawn)
		{
			return !pawn.Dead &&
				pawn.health.capacities.CanBeAwake &&
				pawn.health.hediffSet.BleedRateTotal <= 0.0f &&
				pawn.health.hediffSet.PainTotal < config.significant_pain_threshold;
		}

		/// <summary>
		/// not going to die soon
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static bool is_healthy_enough(Pawn pawn)
		{
			return !pawn.Dead &&
				pawn.health.capacities.CanBeAwake &&
				pawn.health.hediffSet.BleedRateTotal <= 0.1f;
		}

		/// <summary>
		/// pawn can initiate action or respond - whoring, etc
		/// </summary>
		public static bool IsTargetPawnOkay(Pawn pawn)
		{
			return xxx.is_healthy(pawn) && !pawn.Downed && !pawn.Suspended;
		}

		public static bool is_not_dying(Pawn pawn)
		{
			return !pawn.Dead &&
				pawn.health.hediffSet.BleedRateTotal <= 0.3f;
		}

		public static bool is_starved(Pawn pawn)
		{
			return pawn?.needs?.food != null &&
				pawn.needs.food.Starving;
		}
		public static float bleedingRate(Pawn pawn)
		{
			return pawn?.health?.hediffSet?.BleedRateTotal ?? 0f;
		}

		public static bool is_Virgin(Pawn pawn)
		{
			//if (RJWSettings.DevMode) Log.Message("[RJW]xxx::is_Virgin check:" +get_pawnname(pawn));
			if (pawn.relations != null)
				if (pawn.relations.ChildrenCount > 0)
				{
					//if (RJWSettings.DevMode) Log.Message("[RJW]xxx::is_Virgin " + " ChildrenCount " + pawn.relations.ChildrenCount);
					return false;
				}

			if (!(
				pawn.records.GetValue(GetRapedAsComfortPawn) == 0 &&
				pawn.records.GetValue(CountOfSex) == 0 &&
				pawn.records.GetValue(CountOfSexWithHumanlikes) == 0 &&
				pawn.records.GetValue(CountOfSexWithAnimals) == 0 &&
				pawn.records.GetValue(CountOfSexWithInsects) == 0 &&
				pawn.records.GetValue(CountOfSexWithOthers) == 0 &&
				pawn.records.GetValue(CountOfSexWithCorpse) == 0 &&
				pawn.records.GetValue(CountOfWhore) == 0 &&
				pawn.records.GetValue(CountOfRapedHumanlikes) == 0 &&
				pawn.records.GetValue(CountOfBeenRapedByHumanlikes) == 0 &&
				pawn.records.GetValue(CountOfRapedAnimals) == 0 &&
				pawn.records.GetValue(CountOfBeenRapedByAnimals) == 0 &&
				pawn.records.GetValue(CountOfRapedInsects) == 0 &&
				pawn.records.GetValue(CountOfBeenRapedByInsects) == 0 &&
				pawn.records.GetValue(CountOfRapedOthers) == 0 &&
				pawn.records.GetValue(CountOfBeenRapedByOthers) == 0 &&
				pawn.records.GetAsInt(xxx.CountOfBirthHuman) == 0 &&
				pawn.records.GetAsInt(xxx.CountOfBirthAnimal) == 0 &&
				pawn.records.GetAsInt(xxx.CountOfBirthEgg) == 0
				))
			{
				//if (RJWSettings.DevMode) Log.Message("[RJW]xxx::is_Virgin " + "records check fail");
				return false;
			}
			return true;
		}

		public static string get_pawnname(Pawn who)
		{
			//Log.Message("[RJW]xxx::get_pawnname is "+ who.KindLabelDefinite());
			//Log.Message("[RJW]xxx::get_pawnname is "+ who.KindLabelIndefinite());
			if (who == null) return "null";

			string name = who.Label;
			if (name != null)
			{
				if (who.Name?.ToStringShort != null)
					name = who.Name.ToStringShort;
			}
			else
				name = "noname";

			return name;
		}

		public static bool is_gettin_rapedNow(Pawn pawn)
		{
			if (pawn?.jobs?.curDriver != null)
			{
				return pawn.jobs.curDriver.GetType() == typeof(JobDriver_SexBaseRecieverRaped);
			}
			return false;
		}

		public static bool can_path_to_target(Pawn pawn, IntVec3 Position)
		{
			//probably less taxing, but ignores walls
			if (RJWSettings.maxDistancetowalk < 100)
				return pawn.Position.DistanceToSquared(Position) < RJWSettings.maxDistancetowalk;

			//probably more taxing, using real pathing
			bool canit = true;
			PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, Position, pawn);
			if (pawnPath.TotalCost > RJWSettings.maxDistancetowalk)
				canit = false;// too far
			pawnPath.Dispose();
			return canit;
		}

		public static float need_some_sex(Pawn pawn)
		{
			// 3=> always horny for non humanlikes
			float horniness_degree = 3f;
			Need_Sex need_sex = pawn.needs.TryGetNeed<Need_Sex>();
			if (need_sex == null) return horniness_degree;
			if (need_sex.CurLevel < need_sex.thresh_frustrated()) horniness_degree = 3f;
			else if (need_sex.CurLevel < need_sex.thresh_horny()) horniness_degree = 2f;
			else if (need_sex.CurLevel < need_sex.thresh_satisfied()) horniness_degree = 1f;
			else horniness_degree = 0f;
			return horniness_degree;
		}
		public enum SexNeed
		{
			Frustrated,
			Horny,
			Neutral,
			Satisfied
		};

		public static SexNeed need_sex(Pawn pawn)
		{
			// 3=> always horny for non humanlikes
			Need_Sex need_sex = pawn.needs.TryGetNeed<Need_Sex>();
			if (need_sex == null) return SexNeed.Frustrated;
			if (need_sex.CurLevel >= need_sex.thresh_satisfied())
				return SexNeed.Satisfied;
			else if (need_sex.CurLevel >= need_sex.thresh_neutral())
				return SexNeed.Neutral;
			else if (need_sex.CurLevel >= need_sex.thresh_horny())
				return SexNeed.Horny;
			else
				return SexNeed.Frustrated;
		}

		public static bool is_frustrated(Pawn pawn)
		{
			return need_sex(pawn) == SexNeed.Frustrated;
		}

		public static bool is_horny(Pawn pawn)
		{
			return need_sex(pawn) == SexNeed.Horny;
		}

		public static bool is_normal(Pawn pawn)
		{
			return need_sex(pawn) == SexNeed.Neutral;
		}
		public static bool is_satisfied(Pawn pawn)
		{
			return need_sex(pawn) == SexNeed.Satisfied;
		}

		/// <summary> Checks to see if the pawn has any partners who don't have a Polyamorous/Polygamous trait; aka someone who'd get mad about sleeping around. </summary>
		/// <returns> True if the pawn has at least one romantic partner who does not have a poly trait. False if no partners or all partners are poly. </returns>
		public static bool HasNonPolyPartnerOnCurrentMap(Pawn pawn)
		{
			// If they don't have a partner at all we can bail right away.
			if (!LovePartnerRelationUtility.HasAnyLovePartner(pawn))
				return false;

			// They have a partner and a mod that adds a poly trait, so check each partner to see if they're poly.
			foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
			{
				if (relation.def != PawnRelationDefOf.Lover &&
					relation.def != PawnRelationDefOf.Fiance &&
					relation.def != PawnRelationDefOf.Spouse)
				{
					continue;
				}

				// Dead partners don't count.  And stasis'd partners will never find out!
				if (relation.otherPawn.Dead || relation.otherPawn.Suspended)
					continue;

				// Neither does anyone on another map because cheating away from home is obviously never ever discovered
				if (pawn.Map == null || relation.otherPawn.Map == null || relation.otherPawn.Map != pawn.Map)
					continue;

				if ((RomanceDiversifiedIsActive && relation.otherPawn.story.traits.HasTrait(polyamorous)) ||
					(PsychologyIsActive && relation.otherPawn.story.traits.HasTrait(polygamous)))
				{
					// We have a partner who has the poly trait!  But they could have multiple partners so keep checking.
					continue;
				}

				// We found a partner who doesn't have a poly trait.
				return true;
			}

			// If we got here then we checked every partner and all of them had a poly trait, so they don't have a non-poly partner.
			return false;
		}

		public static Gender opposite_gender(Gender g)
		{
			switch (g)
			{
				case Gender.Male:
					return Gender.Female;
				case Gender.Female:
					return Gender.Male;
				default:
					return Gender.None;
			}
		}

		//TODO: change to take parts into account during sex
		public static float get_sex_ability(Pawn pawn)
		{
			try
			{
				return pawn.GetStatValue(sex_stat, false);
			}
			catch (NullReferenceException)
			//not seeded with stats, error for non humanlikes/corpses
			//this and below should probably be rewriten to do calculations here
			{
				//Log.Warning(e.ToString());
				return 1f;
			}
		}

		public static float get_vulnerability(Pawn pawn)
		{
			try
			{
				return pawn.GetStatValue(vulnerability_stat, false);
			}
			catch (NullReferenceException)
			//not seeded with stats, error for non humanlikes/corpses
			{
				//Log.Warning(e.ToString());
				return 1f;
			}
		}

		public static float get_sex_drive(Pawn pawn)
		{
			try
			{
				return pawn.GetStatValue(sex_drive_stat, false);
			}
			catch (NullReferenceException)
			//not seeded with stats, error for non humanlikes/corpses
			{
				//Log.Warning(e.ToString());
				return 1f;
			}
		}

		public static bool isSingleOrPartnerNotHere(Pawn pawn)
		{
			return LovePartnerRelationUtility.ExistingLovePartner(pawn) == null || LovePartnerRelationUtility.ExistingLovePartner(pawn).Map != pawn.Map;
		}

		//base check
		public static bool can_do_loving(Pawn pawn)
		{
			if (is_mechanoid(pawn))
				return false;

			if (is_human(pawn))
			{
				int age = pawn.ageTracker.AgeBiologicalYears;

				if (age < RJWSettings.sex_minimum_age)
					return false;

				return true;
			}
			if (is_animal(pawn))
			{
				if (!(RJWSettings.bestiality_enabled || RJWSettings.animal_on_animal_enabled))
					return false;

				//CurLifeStageIndex for insects since they are not reproductive
				return (pawn.ageTracker.CurLifeStageIndex >= 2 || pawn.ageTracker.CurLifeStage.reproductive);
			}
			return false;
		}
		
		// Penetrative organ check.
		public static bool can_fuck(Pawn pawn)
		{
			//this may cause problems with human mechanoids, like misc. bots or other custom race mechanoids
			if (is_mechanoid(pawn))
				return true;

			if (!can_do_loving(pawn))
				return false;

			var partBPR = Genital_Helper.get_genitalsBPR(pawn);
			var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

			if (Genital_Helper.penis_blocked(pawn) || (!Genital_Helper.has_penis_fertile(pawn, parts) && !Genital_Helper.has_penis_infertile(pawn, parts) && !Genital_Helper.has_ovipositorF(pawn, parts))) return false;

			return true;
		}
		
		// Orifice check.
		public static bool can_be_fucked(Pawn pawn)
		{
			if (is_mechanoid(pawn))
				return false;

			if (!can_do_loving(pawn))
				return false;

			if (Genital_Helper.has_anus(pawn) && !Genital_Helper.anus_blocked(pawn))
				return true;
			if (Genital_Helper.has_vagina(pawn) && !Genital_Helper.vagina_blocked(pawn))
				return true;
			if (Genital_Helper.has_mouth(pawn) && !Genital_Helper.oral_blocked(pawn))
				return true;
			//not sure about below, when female rape male, need to check all code so meh
			//if ((Genital_Helper.has_penis(pawn) || Genital_Helper.has_penis_infertile(pawn) || Genital_Helper.has_ovipositorF(pawn)) && !Genital_Helper.penis_blocked(pawn))
			//	return true;
			//if (Genital_Helper.has_breasts(pawn) && !Genital_Helper.breasts_blocked(pawn))
			//	return true;
			//if (pawn.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(BodyPartGroupDefOf.RightHand) || part.IsInGroup(BodyPartGroupDefOf.LeftHand)) && !Genital_Helper.hands_blocked(pawn))
			//	return true;

			return false;
		}

		public static bool can_rape(Pawn pawn)
		{
			if (!RJWSettings.rape_enabled)
				return false;

			if (is_mechanoid(pawn))
				return true;

			if (!(can_fuck(pawn) ||
				(!is_male(pawn) && get_vulnerability(pawn) < RJWSettings.nonFutaWomenRaping_MaxVulnerability && can_be_fucked(pawn))))
				return false;

			if (is_human(pawn))
			{
				if (RJWSettings.WildMode)
					return true;

				return need_some_sex(pawn) > 0;
			}

			return true;
		}

		public static bool can_get_raped(Pawn pawn)
		{
			if (!RJWSettings.rape_enabled)
				return false;

			if (!can_be_fucked(pawn))
				return false;

			if (is_human(pawn))
			{
				if (RJWSettings.WildMode)
					return true;

				if (!(RJWSettings.rapee_MinVulnerability_human >= 0 && get_vulnerability(pawn) > RJWSettings.rapee_MinVulnerability_human))
					return false;
			}

			return true;
		}

		//Takes the nutrition away from the one penetrating and injects it to the one on the receiving end
		//As with everything in the mod, this could be greatly extended, current focus though is to prevent starvation of those caught in a huge horde of rappers (that may happen with some mods) 
		public static void TransferNutrition(Pawn pawn, Pawn partner, rjwSextype sextype)
		{
			//Log.Message("xxx::TransferNutrition:: " + xxx.get_pawnname(pawn) + " => " + xxx.get_pawnname(partner)); 
			if (partner?.needs == null)
			{
				//Log.Message("xxx::TransferNutrition() failed due to lack of transfer equipment or pawn ");
				return;
			}

			if (sextype == xxx.rjwSextype.Oral && Genital_Helper.has_penis_fertile(pawn))
			{
				Need_Food need = pawn.needs.TryGetNeed<Need_Food>();
				if (need == null)
				{
					//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " doesn't track nutrition in itself, probably shouldn't feed the others");
					return;
				}
				float nutrition_amount = Math.Min(need.MaxLevel / 15f, need.CurLevel); //body size is taken into account implicitly by need.MaxLevel
				pawn.needs.food.CurLevel = need.CurLevel - nutrition_amount;
				//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " sent " + nutrition_amount + " of nutrition");

				if (partner?.needs?.TryGetNeed<Need_Food>() != null)
				{
					//Log.Message("xxx::TransferNutrition() " +  xxx.get_pawnname(partner) + " can receive");
					partner.needs.food.CurLevel += nutrition_amount;
				}

				if (xxx.DubsBadHygieneIsActive)
				{
					Need DBHThirst = partner.needs.AllNeeds.Find(x => x.def == DefDatabase<NeedDef>.GetNamed("DBHThirst"));
					if (DBHThirst != null)
					{
						//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(partner) + " decreasing thirst");
						partner.needs.TryGetNeed(DBHThirst.def).CurLevel += 0.1f;
					}
				}
			}
			if (xxx.RoMIsActive && (sextype == xxx.rjwSextype.Oral || sextype == xxx.rjwSextype.Vaginal || sextype == xxx.rjwSextype.Anal || sextype == xxx.rjwSextype.DoublePenetration))
			{
				if (has_traits(partner))
					if ((partner.story.traits.HasTrait(TraitDef.Named("Succubus")) || partner.story.traits.HasTrait(TraitDef.Named("Warlock"))))
					{
						Need TM_Mana = partner.needs.AllNeeds.Find(x => x.def == DefDatabase<NeedDef>.GetNamed("TM_Mana"));
						if (TM_Mana != null)
						{
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(partner) + " increase mana");
							partner.needs.TryGetNeed(TM_Mana.def).CurLevel += 0.1f;
						}

						Need_Rest need = pawn.needs.TryGetNeed<Need_Rest>();
						if (need != null)
						{
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(partner) + " increase rest");
							partner.needs.TryGetNeed(need.def).CurLevel += 0.25f;
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " decrease rest");
							pawn.needs.TryGetNeed(need.def).CurLevel -= 0.25f;
						}
					}

				if (has_traits(pawn))
					if ((pawn.story.traits.HasTrait(TraitDef.Named("Succubus")) || pawn.story.traits.HasTrait(TraitDef.Named("Warlock"))))
					{
						Need TM_Mana = pawn.needs.AllNeeds.Find(x => x.def == DefDatabase<NeedDef>.GetNamed("TM_Mana"));
						if (TM_Mana != null)
						{
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " increase mana");
							pawn.needs.TryGetNeed(TM_Mana.def).CurLevel += 0.1f;
						}

						Need_Rest need = pawn.needs.TryGetNeed<Need_Rest>();
						if (need != null)
						{
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " increase rest");
							pawn.needs.TryGetNeed(need.def).CurLevel += 0.25f;
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(partner) + " decrease rest");
							partner.needs.TryGetNeed(need.def).CurLevel -= 0.25f;
						}
					}
			}
		}

		public static bool bed_has_at_least_two_occupants(Building_Bed bed)
		{
			return bed.CurOccupants.Count() >= 2;
		}


		public static bool in_same_bed(Pawn pawn, Pawn partner)
		{
			if (pawn.InBed() && partner.InBed())
			{
				if (pawn.CurrentBed() == partner.CurrentBed())
					return true;
			}
			return false;
		}

		public static bool is_laying_down_alone(Pawn pawn)
		{
			if (pawn == null || !pawn.InBed()) return false;

			if (pawn.CurrentBed() != null)
				return !bed_has_at_least_two_occupants(pawn.CurrentBed());
			return true;
		}

		//violent - mark true when pawn rape partner
		//Note: violent is not reliable, since either pawn could be the rapist. Check jobdrivers instead, they're still active since this is called before ending the job.
		public static void think_about_sex(Pawn pawn, Pawn partner, bool isReceiving, bool violent = false, rjwSextype sextype = rjwSextype.None, bool isCoreLovin = false, bool whoring = false)
		{
			// Partner should never be null, but just in case something gets changed elsewhere..
			if (partner == null)
			{
				Log.Message("xxx::think-after_sex( ERROR: " + get_pawnname(pawn) + " has no partner. This should not be called from solo acts. Sextype: " + sextype);
				return;
			}

			// Both pawns are now checked individually, instead of giving thoughts to the partner.
			//Can just return if the currently checked pawn is dead or can't have thoughts, which simplifies the checks.
			if (pawn.Dead || !is_human(pawn))
				return;


			bool masochist = is_masochist(pawn);
			bool zoophile = is_zoophile(pawn);
			if (pawn.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer) || partner.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer))
			{
				masochist = true;
				zoophile = true;
			}
			if (masochist)
			{
				if(RJWSettings.rape_beating)
					pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HarmedMe, partner);
			}

			//--Log.Message("xxx::think_about_sex( " + xxx.get_pawnname(pawn) + ", " + xxx.get_pawnname(partner) + ", " + violent + " ) called");
			//--Log.Message("xxx::think_about_sex( " + xxx.get_pawnname(pawn) + ", " + xxx.get_pawnname(partner) + ", " + violent + " ) - setting pawn thoughts");

			//unconscious pawns has no thoughts
			//and if they has sex, its probably rape, since they have no choice
			//			pawn.Awake();
			if (!pawn.health.capacities.CanBeAwake)
			{
				ThoughtDef pawn_thought = is_masochist(pawn) ? masochist_got_raped_unconscious : got_raped_unconscious;
				pawn.needs.mood.thoughts.memories.TryGainMemory(pawn_thought);
				return;
			}

			// Thoughts for animal-on-colonist.
			if (is_animal(partner) && isReceiving)
			{
				if (!zoophile && !violent)
				{
					if (sextype == rjwSextype.Oral)
						pawn.needs.mood.thoughts.memories.TryGainMemory(allowed_animal_to_lick);
					else if (sextype == rjwSextype.Anal || sextype == rjwSextype.Vaginal)
						pawn.needs.mood.thoughts.memories.TryGainMemory(allowed_animal_to_breed);
					else //Other rarely seen sex types, such as fingering (by primates, monster girls, etc)
						pawn.needs.mood.thoughts.memories.TryGainMemory(allowed_animal_to_grope);
				}
				else
				{
					if (!zoophile)
					{
						if (sextype == rjwSextype.Oral)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(masochist ? masochist_got_licked : got_licked);
						}
						else if (sextype == rjwSextype.Anal || sextype == rjwSextype.Vaginal)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(masochist ? masochist_got_bred : got_bred);
						}
						else //Other types
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(masochist ? masochist_got_groped : got_groped);
						}
					}
					else
					{
						if (sextype == rjwSextype.Oral)
							pawn.needs.mood.thoughts.memories.TryGainMemory(zoophile_got_licked);
						else if (sextype == rjwSextype.Anal || sextype == rjwSextype.Vaginal)
							pawn.needs.mood.thoughts.memories.TryGainMemory(zoophile_got_bred);
						else //Other types
							pawn.needs.mood.thoughts.memories.TryGainMemory(zoophile_got_groped);
					}
				}

				if (!partner.Dead && zoophile && pawn.CurJob.def != gettin_raped && partner.Faction == null && pawn.Faction == Faction.OfPlayer)
				{
					InteractionDef intDef = !partner.AnimalOrWildMan() ? InteractionDefOf.RecruitAttempt : InteractionDefOf.TameAttempt;
					pawn.interactions.TryInteractWith(partner, intDef);
				}
			}

			// Edited by nizhuan-jjr:The two types of stole_sone_lovin are violent due to the description, so I make sure the thought would only trigger after violent behaviors.
			// Edited by hoge: !is_animal is include mech. mech has no mood.
			// Edited by Zaltys: Since this is checked for both pawns, checking violent doesn't work. 
			if (partner.Dead || partner.CurJob.def == gettin_raped)
			{ // Rapist
				ThoughtDef pawn_thought = is_rapist(pawn) || is_bloodlust(pawn) ? bloodlust_stole_some_lovin : stole_some_lovin;
				pawn.needs.mood.thoughts.memories.TryGainMemory(pawn_thought);

				if ((is_necrophiliac(pawn) || is_psychopath(pawn)) && partner.Dead)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(violated_corpse);
				}
			}
			else if (pawn.CurJob.def == gettin_raped) // Rape by animals handled earlier.
			{ // Raped
				if (is_human(partner))
				{
					ThoughtDef pawn_thought = masochist ? masochist_got_raped : got_raped;
					pawn.needs.mood.thoughts.memories.TryGainMemory(pawn_thought);

					ThoughtDef pawn_thought_about_rapist = masochist ? kinda_like_my_rapist : hate_my_rapist;
					pawn.needs.mood.thoughts.memories.TryGainMemory(pawn_thought_about_rapist, partner);
				}

				if (pawn.Faction != null && pawn.Map != null && !masochist && !(is_animal(partner) && zoophile))
				{
					foreach (Pawn bystander in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Where(x => !is_animal(x) && x != pawn && x != partner && !x.Downed && !x.Suspended))
					{
						// dont see through walls, dont see whole map, only 15 cells around
						if (pawn.CanSee(bystander) && pawn.Position.DistanceToSquared(bystander.Position) < 15)
						{
							pawn.needs.mood.thoughts.memories.TryGainMemory(allowed_me_to_get_raped, bystander);
						}
					}
				}
			}
			else if (is_human(partner))
			{
				if (!isCoreLovin && !whoring)
				{
					// human partner and not part of rape or necrophilia. add the vanilla GotSomeLovin thought
					Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.GotSomeLovin);
					if (pawn.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer) || partner.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer))
					{
						thought_Memory.moodPowerFactor = 1.5f;
					}
					if (pawn.needs.mood != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, partner);
					}
				}
			}

			//--Log.Message("xxx::think_about_sex( " + xxx.get_pawnname(pawn) + ", " + xxx.get_pawnname(partner) + ", " + violent + " ) - setting disease thoughts");

			ThinkAboutDiseases(pawn, partner);
		}

		private static void ThinkAboutDiseases(Pawn pawn, Pawn partner)
		{
			// Dead and non-humans have no diseases (yet).
			if (partner.Dead || !is_human(partner)) return;

			// check for visible diseases
			// Add negative relation for visible diseases on the genitals
			int pawn_rash_severity = std.genital_rash_severity(pawn) - std.genital_rash_severity(partner);
			ThoughtDef pawn_thought_about_rash;
			if (pawn_rash_severity == 1) pawn_thought_about_rash = saw_rash_1;
			else if (pawn_rash_severity == 2) pawn_thought_about_rash = saw_rash_2;
			else if (pawn_rash_severity >= 3) pawn_thought_about_rash = saw_rash_3;
			else return;
			Thought_Memory memory = (Thought_Memory)ThoughtMaker.MakeThought(pawn_thought_about_rash);
			partner.needs.mood.thoughts.memories.TryGainMemory(memory, pawn);
		}

		// <summary>Updates records for whoring.</summary>
		public static void UpdateRecords(Pawn pawn, int price)
		{
			pawn.records.AddTo(EarnedMoneyByWhore, price);
			pawn.records.Increment(CountOfWhore);
			//this is added by normal outcome
			//pawn.records.Increment(CountOfSex);
		}

		// <summary>Updates records. "Pawn" should be initiator, and "partner" should be the target.</summary>
		public static void UpdateRecords(Pawn pawn, Pawn partner, rjwSextype sextype, bool isRape = false, bool isLoveSex = false)
		{
			if (!pawn.Dead)
				UpdateRecordsInternal(pawn, partner, isRape, isLoveSex, true, sextype);

			if (partner == null || partner.Dead)
				return;

			UpdateRecordsInternal(partner, pawn, isRape, isLoveSex, false, sextype);
		}

		private static void UpdateRecordsInternal(Pawn pawn, Pawn partner, bool isRape, bool isLoveSex, bool pawnIsRaper, rjwSextype sextype)
		{
			if (pawn == null) return;
			if (pawn.health.Dead) return;

			if (sextype == rjwSextype.Masturbation)
			{
				pawn.records.Increment(CountOfFappin);
				return;
			}

			bool isVirginSex = is_Virgin(pawn); //need copy value before count increase.
			ThoughtDef currentThought = null;

			pawn.records.Increment(CountOfSex);
			bool masochist = is_masochist(pawn);
			bool zoophile = is_zoophile(pawn);
			if (pawn.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer) || partner.health.hediffSet.hediffs.Any((Hediff h) => h.def == HediffDefOf.LoveEnhancer))
			{
				masochist = true;
				zoophile = true;
			}

			if (!isRape)
			{
				if (is_human(partner))
				{
					pawn.records.Increment(partner.health.Dead ? CountOfSexWithCorpse : CountOfSexWithHumanlikes);
					currentThought = isLoveSex ? gave_virginity : null;
				}
				else if (is_insect(partner))
				{
					pawn.records.Increment(CountOfSexWithInsects);
				}
				else if (is_animal(partner))
				{
					pawn.records.Increment(CountOfSexWithAnimals);
					currentThought = zoophile ? gave_virginity : null;
				}
				else
				{
					pawn.records.Increment(CountOfSexWithOthers);
				}
			}
			else
			{
				if (!pawnIsRaper)
				{
					currentThought = masochist ? gave_virginity : lost_virginity;
				}

				if (is_human(partner))
				{
					pawn.records.Increment(pawnIsRaper ? partner.health.Dead ? CountOfSexWithCorpse : CountOfRapedHumanlikes : CountOfBeenRapedByHumanlikes);
					if (pawnIsRaper && (is_rapist(pawn) || is_bloodlust(pawn)))
						currentThought = gave_virginity;
				}
				else if (is_insect(partner))
				{
					pawn.records.Increment(CountOfSexWithInsects);
					pawn.records.Increment(pawnIsRaper ? CountOfRapedInsects : CountOfBeenRapedByInsects);
				}
				else if (is_animal(partner))
				{
					pawn.records.Increment(CountOfSexWithAnimals);
					pawn.records.Increment(pawnIsRaper ? CountOfRapedAnimals : CountOfBeenRapedByAnimals);
					if (zoophile) currentThought = gave_virginity;
				}
				else
				{
					pawn.records.Increment(CountOfSexWithOthers);
					pawn.records.Increment(pawnIsRaper ? CountOfRapedOthers : CountOfBeenRapedByOthers);
				}
			}
			
			//TODO: someday only loose virginity only during vaginal sex
			//if (isVirginSex) //&& (sextype == rjwSextype.Vaginal || sextype == rjwSextype.DoublePenetration))
			//{
			//	Log.Message(xxx.get_pawnname(pawn) + " | " + xxx.get_pawnname(partner) + " | " + currentThought);
			//	Log.Message("1");
			//	if (!is_animal(partner))//passive
			//	{
			//		if (currentThought != null)
			//			partner.needs.mood.thoughts.memories.TryGainMemory(currentThought);
			//	}
			//	Log.Message("2");
			//	if (!is_animal(pawn))//active
			//	{
			//		currentThought = took_virginity;
			//		pawn.needs.mood.thoughts.memories.TryGainMemory(currentThought);
			//	}
			//}
		}

		//============↓======Section of processing the whoring system===============↓=============

		public static Building_Bed FindBed(Pawn pawn)
		{
			if (pawn.ownership.OwnedBed != null)
			{
				return pawn.ownership.OwnedBed;
			}
			return null;
		}

		public static bool CanUse(Pawn pawn, Building_Bed bed)
		{
			bool flag = pawn.CanReserveAndReach(bed, PathEndMode.InteractionCell, Danger.Unspecified) && !bed.IsForbidden(pawn) && bed.OwnersForReading.Contains(pawn);
			return flag;
		}

		public static void FailOnWhorebedNoLongerUsable(this Toil toil, TargetIndex bedIndex, Building_Bed bed)
		{
			if (toil == null)
			{
				throw new ArgumentNullException(nameof(toil));
			}

			toil.FailOnDespawnedOrNull(bedIndex);
			toil.FailOn(bed.IsBurning);
			toil.FailOn(() => HealthAIUtility.ShouldSeekMedicalRestUrgent(toil.actor));
			toil.FailOn(() => toil.actor.IsColonist && !toil.actor.CurJob.ignoreForbidden && !toil.actor.Downed && bed.IsForbidden(toil.actor));
		}

		public static IntVec3 SleepPosOfAssignedPawn(this Building_Bed bed, Pawn pawn)
		{
			if (!bed.OwnersForReading.Contains(pawn))
			{
				Log.Error("[RJW]xxx::SleepPosOfAssignedPawn - pawn is not an owner of the bed;returning bed.position");
				return bed.Position;
			}

			int slotIndex = 0;
			for (byte i = 0; i < bed.OwnersForReading.Count; i++)
			{
				if (bed.OwnersForReading[i] == pawn)
				{
					slotIndex = i;
				}
			}
			return bed.GetSleepingSlotPos(slotIndex);
		}

		//============↓======Section of processing the broken body system===============↓=============
		public static bool BodyIsBroken(Pawn pawn)
		{
			return pawn.health.hediffSet.HasHediff(feelingBroken);
		}

		[SyncMethod]
		public static bool BadlyBroken(Pawn pawn)
		{
			if (!BodyIsBroken(pawn))
				return false;

			int stage = pawn.health.hediffSet.GetFirstHediffOfDef(feelingBroken).CurStageIndex;
			if (stage >= 3)
			{
				//when broken make character masochist
				//todo remove/replace social/needs dubuffs
				if (RJWSettings.AddTrait_Masocist && !is_masochist(pawn))
				{
					var chance = 0.05f;
					if (Rand.Chance(chance))
					{
						if (!is_rapist(pawn))
						{
							pawn.story.traits.GainTrait(new Trait(masochist));
							//Log.Message(xxx.get_pawnname(pawn) + " BadlyBroken, not masochist, adding masochist trait");
						}
						else
						{
							pawn.story.traits.allTraits.Remove(pawn.story.traits.GetTrait(rapist));
							pawn.story.traits.GainTrait(new Trait(masochist));
							//Log.Message(xxx.get_pawnname(pawn) + " BadlyBroken, switch rapist -> masochist");
						}
						pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(got_raped);
						pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(got_licked);
						pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(hate_my_rapist);
						pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(allowed_me_to_get_raped);
					}
				}
				if (pawn.IsPrisonerOfColony)
				{
					pawn.guest.resistance = Mathf.Max(pawn.guest.resistance - 1f, 0f);
					//Log.Message(xxx.get_pawnname(pawn) + " BadlyBroken, reduce prisoner resistance");
				}
			}
			return stage > 1;
		}
		//add variant for eggs?
		public static void processBrokenPawn(Pawn pawn)
		{
			// Called after rape/breed
			if (pawn is null)
				return;

			if (is_human(pawn) && !pawn.Dead && pawn.records != null)
			{
				if (has_traits(pawn))
				{
					if (xxx.is_slime(pawn))
							return;

					if (!BodyIsBroken(pawn))
						pawn.health.AddHediff(feelingBroken);
					else
					{
						float num = feelingBroken.initialSeverity;
						int feelingBrokenStage = pawn.health.hediffSet.GetFirstHediffOfDef(feelingBroken).CurStageIndex;

						if (xxx.RoMIsActive)
							if (pawn.story.traits.HasTrait(TraitDef.Named("Succubus")))
								num *= 0.25f;

						if (pawn.story.traits.HasTrait(TraitDefOf.Tough))
						{
							num *= 0.5f;
						}
						if (pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
						{
							num *= 2.0f;
						}
						if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
						{
							int td = pawn.story.traits.DegreeOfTrait(TraitDefOf.Nerves);
							if (RJWSettings.AddTrait_Nerves && feelingBrokenStage >= 2 && td > -1)
							{
								pawn.story.traits.allTraits.Remove(pawn.story.traits.GetTrait(TraitDefOf.Nerves));
								pawn.story.traits.GainTrait(new Trait(TraitDefOf.Nerves, -1));
							}
							if (RJWSettings.AddTrait_Nerves && feelingBrokenStage >= 3 && td > -2)
							{
								pawn.story.traits.allTraits.Remove(pawn.story.traits.GetTrait(TraitDefOf.Nerves));
								pawn.story.traits.GainTrait(new Trait(TraitDefOf.Nerves, -2));
							}
							switch (td)
							{
								case -2:
									num *= 2.0f;
									break;
								case -1:
									num *= 1.5f;
									break;
								case 1:
									num *= 0.5f;
									break;
								case 2:
									num *= 0.25f;
									break;
							}
						}
						else if (RJWSettings.AddTrait_Nerves && feelingBrokenStage > 1)
						{
							pawn.story.traits.GainTrait(new Trait(TraitDefOf.Nerves, -1));
						}
						pawn.health.hediffSet.GetFirstHediffOfDef(feelingBroken).Severity += num;
					}
					BadlyBroken(pawn);
				}
			}
		}

		public static void ExtraSatisfyForBrokenPawn(Pawn pawn)
		{
			if (!BodyIsBroken(pawn) || pawn.needs?.joy is null)
				return;
			float pawn_satisfaction = 0.2f;
			//Log.Message("Current stage " + pawn.health.hediffSet.GetFirstHediffOfDef(feelingBroken).CurStageIndex);
			switch (pawn.health.hediffSet.GetFirstHediffOfDef(feelingBroken).CurStageIndex)
			{
				case 0:
					break;

				case 1:
					pawn.needs.TryGetNeed<Need_Sex>().CurLevel += pawn_satisfaction;
					pawn.needs.joy.CurLevel += pawn_satisfaction * 0.50f;   // convert half of satisfaction to joy
					break;

				case 2:
					pawn_satisfaction *= 2f;
					pawn.needs.TryGetNeed<Need_Sex>().CurLevel += pawn_satisfaction;
					pawn.needs.joy.CurLevel += pawn_satisfaction * 0.50f;   // convert half of satisfaction to joy
					break;
			}
		}

		//============↑======Section of processing the broken body system===============↑=============
	}
}
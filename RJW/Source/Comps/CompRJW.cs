using Psychology;
using SyrTraits;
using System.Text;
using Verse;
using RimWorld;
using Multiplayer.API;

namespace rjw
{
	public class CompRJW : ThingComp
	{
		/// <summary>
		/// Core comp for genitalia and sexuality tracking.
		/// </summary>
		public CompRJW()
		{
			orientation = Orientation.None;
			quirks = new StringBuilder();
		}

		public CompProperties_RJW Props => (CompProperties_RJW)props;
		public Orientation orientation;
		public StringBuilder quirks;
		public string quirksave; // Not the most elegant way to do this, but it minimizes the save bloat.
		public int NextHookupTick;

		// This automatically checks that genitalia has been added to all freshly spawned pawns.
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);

			if (!(parent is Pawn pawn))
				return;

			if (pawn.kindDef.race.defName.Contains("AIRobot") // No genitalia/sexuality for roombas.
				|| pawn.kindDef.race.defName.Contains("AIPawn") // ...nor MAI.
				|| pawn.kindDef.race.defName.Contains("RPP_Bot")
				) return;

			// No genitalia
			//if (!pawn.RaceProps.body.AllParts.Exists(x => x.def == DefDatabase<BodyPartDef>.GetNamed("Genitals")))
			//	return;

			if (Comp(pawn).orientation == Orientation.None)
			{
				Sexualize(pawn);
				Sexualize(pawn, true);
			}

			//Log.Message("PostSpawnSetup for " + pawn?.Name);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			if (!(parent is Pawn))
				return;

			// Saves the data.
			Scribe_Values.Look(ref orientation, "RJW_Orientation");
			Scribe_Values.Look(ref quirksave, "RJW_Quirks");
			Scribe_Values.Look(ref NextHookupTick, "RJW_NextHookupTick");

			//Log.Message("PostExposeData for " + pawn?.Name);

			// Restore quirk data from the truncated save version.
			quirks = new StringBuilder(quirksave);
		}

		public static CompRJW Comp(Pawn pawn)
		{
			// Call CompRJW.Comp(pawn).<method> to check sexuality, etc.
			return pawn.GetComp<CompRJW>();
		}

		public static void CopyPsychologySexuality(Pawn pawn)
		{
			try
			{
				int kinsey = pawn.TryGetComp<CompPsychology>().Sexuality.kinseyRating;
				//Orientation originalOrientation = Comp(pawn).orientation;

				if (kinsey == 0)
					Comp(pawn).orientation = Orientation.Heterosexual;
				else if (kinsey == 1)
					Comp(pawn).orientation = Orientation.MostlyHeterosexual;
				else if (kinsey == 2)
					Comp(pawn).orientation = Orientation.LeaningHeterosexual;
				else if (kinsey == 3)
					Comp(pawn).orientation = Orientation.Bisexual;
				else if (kinsey == 4)
					Comp(pawn).orientation = Orientation.LeaningHomosexual;
				else if (kinsey == 5)
					Comp(pawn).orientation = Orientation.MostlyHomosexual;
				else if (kinsey == 6)
					Comp(pawn).orientation = Orientation.Homosexual;
				/*else
					Log.Error("RJW::ERRROR - unknown kinsey scale value: " + kinsey);/*

				/*if (Comp(pawn).orientation != originalOrientation)
					Log.Message("RJW + Psychology: Inherited pawn " + xxx.get_pawnname(pawn) + " sexuality from Psychology - " + Comp(pawn).orientation);*/
			}
			catch
			{
				if (!pawn.IsAnimal())
					Log.Warning("CopyPsychologySexuality " + pawn?.Name + ", def: " + pawn?.def?.defName + ", kindDef: " + pawn?.kindDef?.race.defName);
			}
		}

		public static void CopyIndividualitySexuality(Pawn pawn)
		{
			try
			{
				CompIndividuality.Sexuality individualitySexuality = pawn.TryGetComp<CompIndividuality>().sexuality;
				//Orientation originalOrientation = Comp(pawn).orientation;

				if (individualitySexuality == CompIndividuality.Sexuality.Straight && Comp(pawn).orientation != Orientation.Heterosexual)
					Comp(pawn).orientation = Orientation.Heterosexual;
				else if (individualitySexuality == CompIndividuality.Sexuality.Bisexual && Comp(pawn).orientation != Orientation.Bisexual)
					Comp(pawn).orientation = Orientation.Bisexual;
				else if (individualitySexuality == CompIndividuality.Sexuality.Gay && Comp(pawn).orientation != Orientation.Homosexual)
					Comp(pawn).orientation = Orientation.Homosexual;
				else if (individualitySexuality == CompIndividuality.Sexuality.Asexual && Comp(pawn).orientation != Orientation.Asexual)
					Comp(pawn).orientation = Orientation.Asexual;

				/*if (Comp(pawn).orientation != originalOrientation)
					Log.Message("RJW + [SYR]Individuality: Inherited pawn " + xxx.get_pawnname(pawn) + " sexuality from Individuality - " + Comp(pawn).orientation);*/
			}
			catch
			{
				if (!pawn.IsAnimal())
					Log.Warning("CopyIndividualitySexuality " + pawn?.Name + ", def: " + pawn?.def?.defName + ", kindDef: " + pawn?.kindDef?.race.defName);
			}
		}

		[SyncMethod]
		public static void VanillaTraitCheck(Pawn pawn)
		{
			//Orientation originalOrientation = Comp(pawn).orientation;
			//if (Comp(pawn).orientation == Orientation.Asexual || Comp(pawn).orientation == Orientation.None)
			if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
				Comp(pawn).orientation = Orientation.Homosexual;
			else if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
				Comp(pawn).orientation = Orientation.Bisexual;
			else if (pawn.story.traits.HasTrait(TraitDefOf.Asexual) || (pawn.kindDef.race.defName.ToLower().Contains("droid") && !AndroidsCompatibility.IsAndroid(pawn)))
				Comp(pawn).orientation = Orientation.Asexual;
			//else if (!Genital_Helper.has_genitals(pawn) && AndroidsCompatibility.IsAndroid(pawn))
			//	Comp(pawn).orientation = Orientation.Asexual;
			else
				Comp(pawn).orientation = Orientation.Heterosexual;
		}

		// The main method for adding genitalia and orientation.
		public void Sexualize(Pawn pawn, bool reroll = false)
		{
			if (reroll)
			{
				Comp(pawn).orientation = Orientation.None;

				if (xxx.has_quirk(pawn, "Fertile"))
				{
					Hediff fertility = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("IncreasedFertility"));
					if (fertility != null )
						pawn.health.RemoveHediff(fertility);
				}
				if (xxx.has_quirk(pawn, "Infertile"))
				{
					Hediff fertility = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("DecreasedFertility"));
					if (fertility != null)
						pawn.health.RemoveHediff(fertility);
				}
				quirks = new StringBuilder();
			}
			else if (Comp(pawn).orientation != Orientation.None)
				return;

			//roll random RJW orientation
			Comp(pawn).orientation = xxx.is_animal(pawn) ? RollAnimalOrientation() : RollOrientation();

			//Asexual nymp re-roll
			//if (xxx.is_nympho(pawn))
			//	while (Comp(pawn).orientation == Orientation.Asexual)
			//	{
			//		Comp(pawn).orientation = RollOrientation();
			//	}

			//Log.Message("Sexualizing pawn " + pawn?.Name + ", def: " + pawn?.def?.defName);

			if (!reroll)
				Sexualizer.sexualize_pawn(pawn);
			//Log.Message("Orientation for pawn " + pawn?.Name + " is " + orientation);

			if (xxx.has_traits(pawn) && Genital_Helper.has_genitals(pawn) && !(pawn.kindDef.race.defName.ToLower().Contains("droid") && !AndroidsCompatibility.IsAndroid(pawn)))
			{
				if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Vanilla)
					VanillaTraitCheck(pawn);
				if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Psychology)
					CopyPsychologySexuality(pawn);
				if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.SYRIndividuality)
					CopyIndividualitySexuality(pawn);
			}
			else if ((pawn.kindDef.race.defName.ToLower().Contains("droid") && !AndroidsCompatibility.IsAndroid(pawn)) || !Genital_Helper.has_genitals(pawn))
			{
			    // Droids with no genitalia are set as asexual.
			    // If player later adds genitalia to the droid, the droid 'sexuality' gets rerolled.
				Comp(pawn).orientation = Orientation.Asexual;
			}

			QuirkAdder.Generate(pawn);

			if (quirks.Length == 0)
			{
				quirks.Append("None");
				quirksave = quirks.ToString();
			}
		}

		[SyncMethod]
		public static bool CheckPreference(Pawn pawn, Pawn partner)
		{
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Vanilla)
			{
				if (xxx.has_traits(pawn))
					VanillaTraitCheck(pawn);
				if (xxx.has_traits(partner))
					VanillaTraitCheck(partner);
			}
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.SYRIndividuality)
			{
					CopyIndividualitySexuality(pawn);
					CopyIndividualitySexuality(partner);
			}
			if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Psychology)
			{
				if (RJWPreferenceSettings.sexuality_distribution == RJWPreferenceSettings.Rjw_sexuality.Psychology)
					CopyPsychologySexuality(pawn);
					CopyPsychologySexuality(partner);
			}

			//if (xxx.is_mechanoid(pawn))
			//	return false;

			Orientation ori = Orientation.None;
			try
			{
				ori = Comp(pawn).orientation;
			}
			catch
			{
				//Log.Message("[RJW]Error, pawn:" + pawn + " doesnt have orientation comp, modded race?");
				return false;
			}

			if (ori == Orientation.Pansexual || ori == Orientation.Bisexual)
				return true;

			if (ori == Orientation.Asexual)
				return false;

			var pawnpartBPR = Genital_Helper.get_genitalsBPR(pawn);
			var pawnparts = Genital_Helper.get_PartsHediffList(pawn, pawnpartBPR);
			var pawn_has_vagina = Genital_Helper.has_vagina(pawn, pawnparts);
			var pawn_has_penis_fertile = Genital_Helper.has_penis_fertile(pawn, pawnparts);
			var pawn_has_penis_infertile = Genital_Helper.has_penis_infertile(pawn, pawnparts);

			var partnerpartBPR = Genital_Helper.get_genitalsBPR(partner);
			var partnerparts = Genital_Helper.get_PartsHediffList(partner, partnerpartBPR);
			var partner_has_vagina = Genital_Helper.has_vagina(partner, partnerparts);
			var partner_has_penis_fertile = Genital_Helper.has_penis_fertile(partner, partnerparts);
			var partner_has_penis_infertile = Genital_Helper.has_penis_infertile(partner, partnerparts);


			bool isHetero = (pawn_has_vagina && (partner_has_penis_fertile || partner_has_penis_infertile)) ||
						  (partner_has_vagina && (pawn_has_penis_fertile || pawn_has_penis_infertile));

			bool isHomo = (pawn_has_vagina && partner_has_vagina) ||
						   ((partner_has_penis_fertile || partner_has_penis_infertile) && (pawn_has_penis_fertile || pawn_has_penis_infertile));

			if (isHetero && isHomo)
			{
				// Oh you crazy futas.  We could probably do a check against the pawn's gender, but eh.  They've got so many parts available, they'll find something to do.
				return true;
			}

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			switch (ori)
			{
				case Orientation.Heterosexual:
					return !isHomo;
				case Orientation.MostlyHeterosexual:
					return (!isHomo || Rand.Chance(0.2f));
				case Orientation.LeaningHeterosexual:
					return (!isHomo || Rand.Chance(0.6f));
				case Orientation.LeaningHomosexual:
					return (!isHetero || Rand.Chance(0.6f));
				case Orientation.MostlyHomosexual:
					return (!isHetero || Rand.Chance(0.2f));
				case Orientation.Homosexual:
					return !isHetero;
				default:
					Log.Error("RJW::ERROR - tried to check preference for undetermined sexuality.");
					return false;
			}
		}

		[SyncMethod]
		public Orientation RollOrientation()
		{
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float random = Rand.Range(0f, 1f);
			float checkpoint = RJWPreferenceSettings.asexual_ratio / RJWPreferenceSettings.GetTotal();

			float checkpoint_pan = checkpoint + (RJWPreferenceSettings.pansexual_ratio / RJWPreferenceSettings.GetTotal());
			float checkpoint_het = checkpoint_pan + (RJWPreferenceSettings.heterosexual_ratio / RJWPreferenceSettings.GetTotal());
			float checkpoint_bi = checkpoint_het + (RJWPreferenceSettings.bisexual_ratio / RJWPreferenceSettings.GetTotal());
			float checkpoint_gay = checkpoint_bi + (RJWPreferenceSettings.homosexual_ratio / RJWPreferenceSettings.GetTotal());

			if (random < checkpoint)
				return Orientation.Asexual;
			else if (random < checkpoint_pan)
				return Orientation.Pansexual;
			else if (random < checkpoint_het)
				return Orientation.Heterosexual;
			else if (random < checkpoint_het + ((checkpoint_bi - checkpoint_het) * 0.33f))
				return Orientation.MostlyHeterosexual;
			else if (random < checkpoint_het + ((checkpoint_bi - checkpoint_het) * 0.66f))
				return Orientation.LeaningHeterosexual;
			else if (random < checkpoint_bi)
				return Orientation.Bisexual;
			else if (random < checkpoint_bi + ((checkpoint_gay - checkpoint_bi) * 0.33f))
				return Orientation.LeaningHomosexual;
			else if (random < checkpoint_bi + ((checkpoint_gay - checkpoint_bi) * 0.66f))
				return Orientation.MostlyHomosexual;
			else
				return Orientation.Homosexual;
		}

		// Simpler system for animals, with most of them being heterosexual.
		// Don't want to disturb player breeding projects by adding too many gay animals.
		[SyncMethod]
		public Orientation RollAnimalOrientation()
		{
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float random = Rand.Range(0f, 1f);

			if (random < 0.03f)
				return Orientation.Asexual;
			else if (random < 0.85f)
				return Orientation.Heterosexual;
			else if (random < 0.96f)
				return Orientation.Bisexual;
			else
				return Orientation.Homosexual;
		}
	}
}
using Multiplayer.API;
using RimWorld;
using Verse;

namespace rjw
{
	/// <summary>
	/// Responsible for handling the periodic effects of having an STD hediff.
	/// Not technically tied to the infection vector itself,
	/// but some of the STD effects are weird and complicated.
	/// </summary>
	public static class std_updater
	{
		public const float UpdatesPerDay = 60000f / 150f / (float)Need_Sex.needsex_tick_timer;

		public static void update(Pawn p)
		{
			update_immunodeficiency(p);

			// Check if any infections are below the autocure threshold and cure them if so
			foreach (std_def sd in std.all)
			{
				Hediff inf = std.get_infection(p, sd);
				if (inf != null && (inf.Severity < sd.autocure_below_severity || std.IsImmune(p)))
				{
					p.health.RemoveHediff(inf);
					if (sd.cohediff_def != null)
					{
						Hediff coinf = p.health.hediffSet.GetFirstHediffOfDef(sd.cohediff_def);
						if (coinf != null)
							p.health.RemoveHediff(coinf);
					}
				}
			}

			UpdateBoobitis(p);
			roll_for_syphilis_damage(p);
		}

		[SyncMethod]
		public static void roll_for_syphilis_damage(Pawn p)
		{
			Hediff syp = p.health.hediffSet.GetFirstHediffOfDef(std.syphilis.hediff_def);
			if (syp == null || !(syp.Severity >= 0.60f) || syp.FullyImmune()) return;

			// A 30% chance per day of getting any permanent damage works out to ~891 in 1 million for each roll
			// The equation is (1-x)^(60000/150)=.7
			// Important Note:
			// this function is called from Need_Sex::NeedInterval(), where it involves a needsex_tick and a std_tick to actually trigger this roll_for_syphilis_damage.
			// j(this is not exactly the same as the value in Need_Sex, that value is 0, but here j should be 1) std_ticks per this function called, k needsex_ticks per std_tick, 150 ticks per needsex_tick, and x is the chance per 150 ticks,
			// The new equation should be .7 = (1-x)^(400/kj)
			// 1-x = .7^(kj/400), x =1-.7^(kj/400)
			// Since k=10,j=1, so kj=10, new x is 1-.7^(10/400)=0.0088772362, let it be 888/100000
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (Rand.RangeInclusive(1, 100000) <= 888)
			{
				BodyPartRecord part;
				float sev;
				var parts = p.RaceProps.body.AllParts;

				float rv = Rand.Value;
				if (rv < 0.10f)
				{
					part = parts.Find(bpr => string.Equals(bpr.def.defName, "Brain"));
					sev = 1.0f;
				}
				else if (rv < 0.50f)
				{
					part = parts.Find(bpr => string.Equals(bpr.def.defName, "Liver"));
					sev = Rand.RangeInclusive(1, 3);
				}
				else if (rv < 0.75f)
				{
					//LeftKidney, probably
					part = parts.Find(bpr => string.Equals(bpr.def.defName, "Kidney"));
					sev = Rand.RangeInclusive(1, 2);
				}
				else
				{
					//RightKidney, probably
					part = parts.FindLast(bpr => string.Equals(bpr.def.defName, "Kidney"));
					sev = Rand.RangeInclusive(1, 2);
				}

				if (part != null && !p.health.hediffSet.PartIsMissing(part) && !p.health.hediffSet.HasDirectlyAddedPartFor(part))
				{
					DamageDef vir_dam = DefDatabase<DamageDef>.GetNamed("ViralDamage");
					HediffDef dam_def = HealthUtility.GetHediffDefFromDamage(vir_dam, p, part);
					Hediff_Injury inj = (Hediff_Injury)HediffMaker.MakeHediff(dam_def, p, null);
					inj.Severity = sev;
					inj.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
					p.health.AddHediff(inj, part, null);
					string message_title = std.syphilis.label + " Damage";
					string baby_pronoun = p.gender == Gender.Male ? "his" : "her";
					string message_text = "RJW_Syphilis_Damage_Message".Translate(xxx.get_pawnname(p), baby_pronoun, part.def.label, std.syphilis.label);
					Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.ThreatSmall, p);
				}
			}
		}

		[SyncMethod]
		public static void update_immunodeficiency(Pawn p)
		{
			float min_bf_for_id = 1.0f - std.immunodeficiency.minSeverity;
			Hediff id = p.health.hediffSet.GetFirstHediffOfDef(std.immunodeficiency);
			float bf = p.health.capacities.GetLevel(PawnCapacityDefOf.BloodFiltration);
			bool has = id != null;
			bool should_have = bf <= min_bf_for_id;

			if (has && !should_have)
			{
				p.health.RemoveHediff(id);
				id = null;
			}
			else if (!has && should_have)
			{
				p.health.AddHediff(std.immunodeficiency);
				id = p.health.hediffSet.GetFirstHediffOfDef(std.immunodeficiency);
			}

			if (id == null) return;

			id.Severity = 1.0f - bf;

			// Roll for and apply opportunistic infections:
			// Pawns will have a 90% chance for at least one infection each year at 0% filtration, and a 0%
			// chance at 40% filtration, scaling linearly.
			// Let x = chance infected per roll
			// Then chance not infected per roll = 1 - x
			// And chance not infected on any roll in one day = (1 - x) ^ (60000 / 150) = (1 - x) ^ 400
			// And chance not infected on any roll in one year = (1 - x) ^ (400 * 60) = (1 - x) ^ 24000
			// So 0.10 = (1 - x) ^ 24000
			//	log (0.10) = 24000 log (1 - x)
			//	x = 0.00009593644334648975435114691213 = ~96 in 1 million
			// Important Note:
			// this function is called from Need_Sex::NeedInterval(), where it involves a needsex_tick and a std_tick to actually trigger this update_immunodeficiency.
			// j(this is not exactly the same as the value in Need_Sex, that value is 0, but here j should be 1) std_ticks per this function called, k needsex_ticks per std_tick, 150 ticks per needsex_tick, and x is the chance per 150 ticks,
			// The new equation should be .1 = (1-x)^(24000/kj)
			// log(.1) = (24000/kj) log(1-x),  so log(1-x)= (kj/24000) log(.1), 1-x = .1^(kj/24000), x= 1-.1^(kj/24000)
			// Since k=10,j=1, so kj=10, new x is 1-.1^(10/24000)=0.0009589504, let it be 959/1000000
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (Rand.RangeInclusive(1, 1000000) <= 959 && Rand.Value < bf / min_bf_for_id)
			{
				BodyPartRecord part;
				{
					float rv = Rand.Value;
					var parts = p.RaceProps.body.AllParts;
					if (rv < 0.25f)
						part = parts.Find(bpr => string.Equals(bpr.def.defName, "Jaw"));
					else if (rv < 0.50f)
						part = parts.Find(bpr => string.Equals(bpr.def.defName, "Lung"));
					else if (rv < 0.75f)
						part = parts.FindLast(bpr => string.Equals(bpr.def.defName, "Lung"));
					else
						part = parts.RandomElement();
				}

				if (part != null &&
					!p.health.hediffSet.PartIsMissing(part) && !p.health.hediffSet.HasDirectlyAddedPartFor(part) &&
					p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WoundInfection) == null && // If the pawn already has a wound infection, we can't properly set the immunity for the new one
					p.health.immunity.GetImmunity(HediffDefOf.WoundInfection) <= 0.0f)
				{ // Dont spawn infection if pawn already has immunity
					p.health.AddHediff(HediffDefOf.WoundInfection, part);
					p.health.HealthTick(); // Creates the immunity record
					ImmunityRecord ir = p.health.immunity.GetImmunityRecord(HediffDefOf.WoundInfection);
					if (ir != null)
						ir.immunity = xxx.config.opp_inf_initial_immunity;
					const string message_title = "Opportunistic Infection";
					string message_text = "RJW_Opportunistic_Infection_Message".Translate(xxx.get_pawnname(p));
					Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.ThreatSmall);
				}
			}
		}

		/// <summary>
		/// For meanDays = 1.0, will return true on average once per day. For 2.0, will return true on average once every two days.
		/// </summary>
		[SyncMethod]
		static bool RollFor(float meanDays)
		{
			return Rand.Chance(1.0f / meanDays / UpdatesPerDay);
		}

		public static void UpdateBoobitis(Pawn pawn)
		{
			var hediff = std.get_infection(pawn, std.boobitis);
			if (hediff == null
				|| !(hediff.Severity >= 0.20f)
				|| hediff.FullyImmune()
				|| !BreastSize_Helper.TryGetBreastSize(pawn, out var oldSize, out var oldBoobs)
				|| oldSize >= BreastSize_Helper.MaxSize
				|| !RollFor(hediff.Severity > 0.90f ? 5f : 15f))
			{
				return;
			}
			var chest = Genital_Helper.get_breastsBPR(pawn);
			var newSize = oldSize + 1;
			var newBoobs = BreastSize_Helper.GetHediffDef(newSize);

			GenderHelper.ChangeSex(pawn, () =>
			{
				if (oldBoobs != null)
				{
					pawn.health.RemoveHediff(oldBoobs);
				}
				pawn.health.AddHediff(newBoobs, chest);
			});

			var message = "RJW_BreastsHaveGrownFromBoobitis".Translate(pawn);
			Messages.Message(message, pawn, MessageTypeDefOf.SilentInput);
		}
	}
}

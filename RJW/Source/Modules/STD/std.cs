using System.Collections.Generic;
using System.Linq;
using Verse;

namespace rjw
{
	/// <summary>
	/// Common functions and constants relevant to STDs.
	/// </summary>
	public static class std
	{
		public static std_def hiv = DefDatabase<std_def>.GetNamed("HIV");
		public static std_def herpes = DefDatabase<std_def>.GetNamed("Herpes");
		public static std_def warts = DefDatabase<std_def>.GetNamed("Warts");
		public static std_def syphilis = DefDatabase<std_def>.GetNamed("Syphilis");
		public static std_def boobitis = DefDatabase<std_def>.GetNamed("Boobitis");

		public static readonly HediffDef immunodeficiency = DefDatabase<HediffDef>.GetNamed("Immunodeficiency");

		public static List<std_def> all => DefDatabase<std_def>.AllDefsListForReading;

		// Returns how severely affected this pawn's crotch is by rashes and warts, on a scale from 0 to 3.
		public static int genital_rash_severity(Pawn p)
		{
			int tr = 0;

			Hediff her = p.health.hediffSet.GetFirstHediffOfDef(herpes.hediff_def);
			if (her != null && her.Severity >= 0.25f)
				++tr;

			Hediff war = p.health.hediffSet.GetFirstHediffOfDef(warts.hediff_def);
			if (war != null)
				tr += war.Severity < 0.40f ? 1 : 2;

			return tr;
		}

		public static Hediff get_infection(Pawn p, std_def sd)
		{
			return p.health.hediffSet.GetFirstHediffOfDef(sd.hediff_def);
		}

		public static BodyPartRecord GetRelevantBodyPartRecord(Pawn pawn, std_def std)
		{
			if (std.appliedOnFixedBodyParts == null)
			{
				return null;
			}

			BodyPartDef target = std.appliedOnFixedBodyParts.Single();
			return pawn?.RaceProps.body.GetPartsWithDef(target).Single();
			//return pawn?.RaceProps.body.GetPartsWithDef(std.appliedOnFixedBodyParts.Single()).Single();
		}

		public static bool is_wasting_away(Pawn p)
		{
			Hediff id = p.health.hediffSet.GetFirstHediffOfDef(immunodeficiency);
			return id != null && id.CurStageIndex > 0;
		}

		public static bool IsImmune(Pawn pawn)
		{
			// Archotech genitalia automagically purge STDs.
			return !RJWSettings.stds_enabled
				|| pawn.health.hediffSet.HasHediff(Genital_Helper.archotech_vagina)
				|| pawn.health.hediffSet.HasHediff(Genital_Helper.archotech_penis)
				|| xxx.is_demon(pawn)
				|| xxx.is_slime(pawn)
				|| xxx.is_mechanoid(pawn);
		}
	}
}

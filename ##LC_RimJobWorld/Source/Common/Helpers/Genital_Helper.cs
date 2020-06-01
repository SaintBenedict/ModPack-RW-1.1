using System.Collections.Generic;
using System.Linq;
using Verse;

namespace rjw
{
	public static class Genital_Helper
	{
		public static HediffDef generic_anus = HediffDef.Named("GenericAnus");
		public static HediffDef generic_penis = HediffDef.Named("GenericPenis");
		public static HediffDef generic_vagina = HediffDef.Named("GenericVagina");
		public static HediffDef generic_breasts = HediffDef.Named("GenericBreasts");

		public static HediffDef average_penis = HediffDef.Named("Penis");
		public static HediffDef hydraulic_penis = HediffDef.Named("HydraulicPenis");
		public static HediffDef bionic_penis = HediffDef.Named("BionicPenis");
		public static HediffDef archotech_penis = HediffDef.Named("ArchotechPenis");

		public static HediffDef average_vagina = HediffDef.Named("Vagina");
		public static HediffDef hydraulic_vagina = HediffDef.Named("HydraulicVagina");
		public static HediffDef bionic_vagina = HediffDef.Named("BionicVagina");
		public static HediffDef archotech_vagina = HediffDef.Named("ArchotechVagina");

		public static HediffDef average_breasts = HediffDef.Named("Breasts");
		public static HediffDef hydraulic_breasts = HediffDef.Named("HydraulicBreasts");
		public static HediffDef bionic_breasts = HediffDef.Named("BionicBreasts");
		public static HediffDef archotech_breasts = HediffDef.Named("ArchotechBreasts");
		public static HediffDef featureless_chest = HediffDef.Named("FeaturelessChest");
		public static HediffDef udder_breasts = HediffDef.Named("UdderBreasts");

		public static HediffDef average_anus = HediffDef.Named("Anus");
		public static HediffDef hydraulic_anus = HediffDef.Named("HydraulicAnus");
		public static HediffDef bionic_anus = HediffDef.Named("BionicAnus");
		public static HediffDef archotech_anus = HediffDef.Named("ArchotechAnus");

		public static HediffDef peg_penis = HediffDef.Named("PegDick");

		public static HediffDef insect_anus = HediffDef.Named("InsectAnus");
		public static HediffDef ovipositorM = HediffDef.Named("OvipositorM");
		public static HediffDef ovipositorF = HediffDef.Named("OvipositorF");

		public static HediffDef demonT_penis = HediffDef.Named("DemonTentaclePenis");
		public static HediffDef demon_penis = HediffDef.Named("DemonPenis");
		public static HediffDef demon_vagina = HediffDef.Named("DemonVagina");
		public static HediffDef demon_anus = HediffDef.Named("DemonAnus");

		public static HediffDef slime_breasts = HediffDef.Named("SlimeBreasts");
		public static HediffDef slime_penis = HediffDef.Named("SlimeTentacles");
		public static HediffDef slime_vagina = HediffDef.Named("SlimeVagina");
		public static HediffDef slime_anus = HediffDef.Named("SlimeAnus");

		public static HediffDef feline_penis = HediffDef.Named("CatPenis");
		public static HediffDef feline_vagina = HediffDef.Named("CatVagina");

		public static HediffDef canine_penis = HediffDef.Named("DogPenis");
		public static HediffDef canine_vagina = HediffDef.Named("DogVagina");

		public static HediffDef equine_penis = HediffDef.Named("HorsePenis");
		public static HediffDef equine_vagina = HediffDef.Named("HorseVagina");

		public static HediffDef dragon_penis = HediffDef.Named("DragonPenis");
		public static HediffDef dragon_vagina = HediffDef.Named("DragonVagina");

		public static HediffDef raccoon_penis = HediffDef.Named("RaccoonPenis");

		public static HediffDef hemipenis = HediffDef.Named("HemiPenis");

		public static HediffDef crocodilian_penis = HediffDef.Named("CrocodilianPenis");

		public static BodyPartRecord get_genitalsBPR(Pawn pawn)
		{
			//--Log.Message("Genital_Helper::get_genitals( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord Part = pawn?.RaceProps.body.AllParts.Find(bpr => bpr.def.defName == "Genitals");

			if (Part == null)
			{
				//--Log.Message("[RJW] get_genitals( " + xxx.get_pawnname(pawn) + " ) - Part is null");
				return null;
			}
			return Part;
		}

		public static BodyPartRecord get_breastsBPR(Pawn pawn)
		{
			//--Log.Message("[RJW] get_breasts( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord Part = pawn?.RaceProps.body.AllParts.Find(bpr => bpr.def.defName == "Chest");

			if (Part == null)
			{
				//--Log.Message("[RJW] get_breasts( " + xxx.get_pawnname(pawn) + " ) - Part is null");
				return null;
			}
			return Part;
		}

		public static BodyPartRecord get_mouthBPR(Pawn pawn)
		{
			//--Log.Message("[RJW] get_mouth( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord Part = pawn?.RaceProps.body.AllParts.Find(bpr => bpr.def.defName.ToLower().ContainsAny("mouth", "teeth", "jaw", "beak"));

			if (Part == null)
			{
				//--Log.Message("[RJW] get_mouth( " + xxx.get_pawnname(pawn) + " ) - Part is null");
				return null;
			}
			return Part;
		}

		public static BodyPartRecord get_anusBPR(Pawn pawn)
		{
			//--Log.Message("[RJW] get_anus( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord Part = pawn?.RaceProps.body.AllParts.Find(bpr => bpr.def.defName == "Anus");

			if (Part == null)
			{
				//--Log.Message("[RJW] get_anus( " + xxx.get_pawnname(pawn) + " ) - Part is null");
				return null;
			}
			return Part;
		}

		public static bool breasts_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_breasts))
						return true;
				}
			}
			return false;
		}

		public static bool anus_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_anus))
						return true;
				}
			}
			return false;
		}

		public static bool genitals_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_penis || gear_def.blocks_vagina))
						return true;
				}
			}
			return false;
		}

		public static bool hands_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_hands))
						return true;
				}
			}
			return false;
		}

		public static bool penis_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_penis))
						return true;
				}
			}
			return false;
		}

		public static bool oral_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_oral))
						return true;
				}
			}
			return false;
		}

		public static bool vagina_blocked(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				foreach (var app in pawn.apparel.WornApparel)
				{
					if ((app.def is bondage_gear_def gear_def) && (gear_def.blocks_vagina))
						return true;
				}
			}
			return false;
		}

		public static List<Hediff> get_PartsHediffList(Pawn pawn, BodyPartRecord Part)
		{
			if (Part is null)
				return null;

			return pawn.health.hediffSet.hediffs.FindAll((Hediff hed) =>
					hed.Part == Part &&
					(hed is Hediff_PartBaseNatural || hed is Hediff_PartBaseArtifical));
		}

		public static bool has_genitals(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_breastsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return true;
		}

		public static bool has_breasts(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_breastsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					!hed.def.defName.ToLower().Contains("featureless"));
		}

		public static bool has_male_breasts(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_breastsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(((hed is Hediff_PartBaseNatural) &&
					(hed as Hediff_PartBaseNatural).CurStageIndex == 0)
					||
					((hed is Hediff_PartBaseArtifical) &&
					(hed as Hediff_PartBaseArtifical).CurStageIndex == 0)
					));
		}

		/// <summary>
		/// Can do breastjob if breasts are average or bigger
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static bool can_do_breastjob(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_breastsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(((hed is Hediff_PartBaseNatural) &&
					(hed as Hediff_PartBaseNatural).CurStageIndex > 1)
					||
					((hed is Hediff_PartBaseArtifical) &&
					(hed as Hediff_PartBaseArtifical).CurStageIndex > 1)
					));
		}

		public static bool has_mouth(Pawn pawn)
		{
			BodyPartRecord Part = get_mouthBPR(pawn);
			if (Part is null)
				return false;

			return !pawn.health.hediffSet.hediffs.Any((Hediff hed) =>
					(hed.Part == Part) && (hed is Hediff_MissingPart));
		}

		public static bool has_anus(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_anusBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					hed.def.defName.ToLower().Contains("anus"));
		}

		/// <summary>
		/// Insertable, this is both vagina and ovipositorf
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="parts"></param>
		/// <returns></returns>
		public static bool has_vagina(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(hed.def.defName.ToLower().Contains("vagina") || hed.def.defName.ToLower().Contains("ovipositorf")));
		}

		public static bool has_penis_fertile(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(hed.def.defName.ToLower().Contains("penis") || hed.def.defName.ToLower().Contains("ovipositorm")));
		}
		public static bool has_penis_infertile(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(hed.def.defName.ToLower().Contains("pegdick") || (hed.def.defName.ToLower().Contains("tentacle") && !hed.def.defName.ToLower().Contains("penis"))));
		}

		public static bool has_multipenis(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;
			if (parts.Count == 1)
				return false;

			int count = 0;
			foreach(Hediff hed in parts)
			{
				if (hed.def.defName.ToLower().Contains("hemipenis"))
					return true;
				if (hed.def.defName.ToLower().Contains("penis") ||
					hed.def.defName.ToLower().Contains("pegdick") ||
					hed.def.defName.ToLower().Contains("ovipositorm") ||
					hed.def.defName.ToLower().Contains("tentacle")
					)
					count += 1;
			}

			return count > 1;
		}

		public static bool has_ovipositorM(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					hed.def.defName.ToLower().Contains("ovipositorm"));
		}

		public static bool has_ovipositorF(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					hed.def.defName.ToLower().Contains("ovipositorf"));

		}
		/// <summary>
		/// Can do autofelatio if penis is huge or bigger
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static bool can_do_autofelatio(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return parts.Any((Hediff hed) =>
					(((hed is Hediff_PartBaseNatural) &&
					(hed as Hediff_PartBaseNatural).CurStageIndex > 3)
					||
					((hed is Hediff_PartBaseArtifical) &&
					(hed as Hediff_PartBaseArtifical).CurStageIndex > 3)) &&
					(!hed.def.defName.ToLower().Contains("vagina")) &&
					(
					hed.def.defName.ToLower().Contains("penis") ||
					hed.def.defName.ToLower().Contains("pegdick") ||
					hed.def.defName.ToLower().Contains("ovipositorm") ||
					hed.def.defName.ToLower().Contains("tentacle")
					));
		}

		/// <summary>
		/// Count only fertile penises
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public static bool is_futa(Pawn pawn, List<Hediff> parts = null)
		{
			if (parts == null)
			{
				parts = get_PartsHediffList(pawn, get_genitalsBPR(pawn));
			}
			if (parts.NullOrEmpty())
				return false;

			return (Genital_Helper.has_penis_fertile(pawn, parts) && has_vagina(pawn, parts));
		}
	}
}
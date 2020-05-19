using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;
using System;
using Multiplayer.API;

namespace rjw
{
	public static class BreastSize_Helper
	{
		public static int NipplesOnlyBreastSize = -1;

		static HediffDef[] BreastsInOrder = new[] {
			//Genital_Helper.flat_breasts,
			//Genital_Helper.small_breasts,
			Genital_Helper.average_breasts,
			//Genital_Helper.large_breasts,
			//Genital_Helper.huge_breasts
		};

		public static int MaxSize = BreastsInOrder.Length - 1;

		static IDictionary<HediffDef, int> SizeByHediffDef = BreastsInOrder
			.Select((hed, i) => new { hed, i })
			.ToDictionary(pair => pair.hed, pair => pair.i);


		/// <summary>
		/// Returns true and sets size if pawn has natural breasts.
		/// Otherwise returns false.
		/// </summary>
		public static bool TryGetBreastSize(Pawn pawn, out int size)
		{
			return TryGetBreastSize(pawn, out size, out var hediff);
		}

		/// <summary>
		/// Returns true and sets size and hediff if pawn has natural breasts.
		/// Hediff will still be null for nipples only.
		/// Otherwise returns false.
		/// </summary>
		public static bool TryGetBreastSize(Pawn pawn, out int size, out Hediff hediff)
		{
			var chest = Genital_Helper.get_breastsBPR(pawn);

			if (pawn.health.hediffSet.PartIsMissing(chest))
			{
				size = 0;
				hediff = null;
				return false;
			}

			foreach(var candidate_hediff in pawn.health.hediffSet.hediffs)
			{
				if (SizeByHediffDef.TryGetValue(candidate_hediff.def, out size))
				{
					hediff = candidate_hediff;
					return true;
				}
			}

			if (HasNipplesOnly(pawn, chest))
			{
				size = NipplesOnlyBreastSize;
				hediff = null;
				return true;
			}

			size = 0;
			hediff = null;
			return false;
		}

		public static int GetSize(HediffDef hediffDef)
		{
			return SizeByHediffDef[hediffDef];
		}

		public static HediffDef GetHediffDef(int size)
		{
			return BreastsInOrder[size];
		}

		public static bool IsMammal(Pawn pawn)
		{
			// In theory should not apply to lizards etc either.
			return !xxx.is_mechanoid(pawn);
		}

		/// <summary>
		/// Returns true if pawn has nipples but no breast development or implants.
		/// </summary>
		public static bool HasNipplesOnly(Pawn pawn, BodyPartRecord chest)
		{
			var alreadyHasBoobs = pawn.health.hediffSet.hediffs.Any(hediff =>
				hediff.Part == chest &&
				(hediff is Hediff_Implant || hediff is Hediff_AddedPart));
			return IsMammal(pawn) && !alreadyHasBoobs;
		}

		/// <summary>
		/// Adds two bruises with amount up to the given max.
		/// </summary>
		[SyncMethod]
		public static void HurtBreasts(Pawn pawn, BodyPartRecord part, int max)
		{
			if (max <= 0)
			{
				return;
			}

			// Two bruises.
			for (var i = 0; i < 2; i++)
			{
				pawn.TakeDamage(new DamageInfo(
					DamageDefOf.Blunt,
					Rand.RangeInclusive(max / 4, max),
					999f,
					-1f,
					null,
					part,
					null,
					DamageInfo.SourceCategory.ThingOrUnknown,
					null));
			}
		}
	}
}
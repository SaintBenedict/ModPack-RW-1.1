using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace rjw.MainTab
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_PriceRangeOfWhore : PawnColumnWorker_TextCenter
	{
		protected internal int min;
		protected internal int max;

		protected override string GetTextFor(Pawn pawn)
		{
			min = WhoringHelper.WhoreMinPrice(pawn);
			max = WhoringHelper.WhoreMaxPrice(pawn);
			return string.Format("{0} - {1}", min, max);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			return min;
		}
	}
}

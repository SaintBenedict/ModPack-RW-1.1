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
	public class PawnColumnWorker_AverageMoneyByWhore : PawnColumnWorker_TextCenter
	{
		public static readonly RecordDef EarnedMoneyByWhore = DefDatabase<RecordDef>.GetNamed("EarnedMoneyByWhore");
		public static readonly RecordDef CountOfWhore = DefDatabase<RecordDef>.GetNamed("CountOfWhore");

		protected internal float score;

		protected override string GetTextFor(Pawn pawn)
		{
			float total = pawn.records.GetValue(EarnedMoneyByWhore);
			float count = pawn.records.GetValue(CountOfWhore);
			if ((int)count == 0)
			{
				return "-";
			}
			score = (total / count);
			return ((int)score).ToString();
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
		}

		private float GetValueToCompare(Pawn pawn)
		{
			return score;
		}
	}
}

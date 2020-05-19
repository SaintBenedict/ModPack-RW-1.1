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
	public class PawnColumnWorker_EarnedMoneyByWhore : PawnColumnWorker_TextCenter
	{
		public static readonly RecordDef EarnedMoneyByWhore = DefDatabase<RecordDef>.GetNamed("EarnedMoneyByWhore");

		protected internal int score;

		protected override string GetTextFor(Pawn pawn)
		{
			score = pawn.records.GetAsInt(EarnedMoneyByWhore);
			return pawn.records.GetValue(EarnedMoneyByWhore).ToString();
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			return score;
		}
	}
}

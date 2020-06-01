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
	public class PawnColumnWorker_CountOfWhore : PawnColumnWorker_TextCenter
	{
		public static readonly RecordDef CountOfWhore = DefDatabase<RecordDef>.GetNamed("CountOfWhore");

		protected internal int score;

		protected override string GetTextFor(Pawn pawn)
		{
			score = pawn.records.GetAsInt(CountOfWhore);
			return pawn.records.GetValue(CountOfWhore).ToString();
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

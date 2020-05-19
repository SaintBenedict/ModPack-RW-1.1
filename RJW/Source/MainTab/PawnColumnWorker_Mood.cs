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
	public class PawnColumnWorker_Mood : PawnColumnWorker_TextCenter
	{
		protected internal float score;

		protected override string GetTextFor(Pawn pawn)
		{
			score = pawn.needs.mood.CurLevelPercentage;
			return score.ToStringPercent();
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

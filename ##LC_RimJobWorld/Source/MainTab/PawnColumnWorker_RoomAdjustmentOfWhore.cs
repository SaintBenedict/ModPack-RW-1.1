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
	public class PawnColumnWorker_RoomAdjustmentOfWhore : PawnColumnWorker_TextCenter
	{
		protected internal float score;

		protected override string GetTextFor(Pawn pawn)
		{
			score = WhoringHelper.WhoreRoomAdjustment(pawn);
			Room ownedRoom = pawn.ownership.OwnedRoom;
			int scoreStageIndex;
			if (ownedRoom == null)
			{	
				scoreStageIndex = 0;
			}
			else
			{
				scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(ownedRoom.GetStat(RoomStatDefOf.Impressiveness));
			}
			return string.Format("{0} -> {1}", scoreStageIndex, score.ToStringPercent());
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

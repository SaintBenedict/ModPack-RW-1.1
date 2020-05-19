using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace rjw
{
	public abstract class WorkGiver_GatherHumanBodyResources : WorkGiver_GatherAnimalBodyResources
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			//List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
			//int i = 0;
			//if (i < pawns.Count)
			//{
			//	yield return (Thing)pawns[i];
				/*Error: Unable to find new state assignment for yield return*/
			//	;
			//}
			foreach (Pawn targetpawn in pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned)
			{
				yield return targetpawn;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !pawn2.RaceProps.Humanlike)
			{
				return false;
			}
			CompHasGatherableBodyResource comp = GetComp(pawn2);
			if (comp != null && comp.ActiveAndFull && PawnUtility.CanCasuallyInteractNow(pawn2, false) && pawn2 != pawn)
			{
				LocalTargetInfo target = pawn2;
				bool ignoreOtherReservations = forced;
				if (ReservationUtility.CanReserve(pawn, target, 1, -1, null, ignoreOtherReservations))
				{
					return true;
				}
			}
			return false;
		}
	}
}

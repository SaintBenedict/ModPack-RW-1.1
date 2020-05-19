using Verse;
using Multiplayer.API;

namespace rjw
{
	public static class PawnDesignations_Milking
	{
		public static bool UpdateCanDesignateMilking(this Pawn pawn)
		{
			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateMilking = false;
		}
		public static bool CanDesignateMilking(this Pawn pawn)
		{
			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateMilking = false;
		}
		public static void ToggleMilking(this Pawn pawn)
		{
			if (pawn.CanDesignateMilking())
			{
				if (!pawn.IsDesignatedMilking())
					DesignateMilking(pawn);
				else
					UnDesignateMilking(pawn);
			}
		}
		public static bool IsDesignatedMilking(this Pawn pawn)
		{
			if (SaveStorage.DataStore.GetPawnData(pawn).Milking)
			{
				if (!pawn.IsDesignatedHero())
					if (!(pawn.IsColonist || pawn.IsPrisonerOfColony || xxx.is_slave(pawn)))
						UnDesignateMilking(pawn);

				if (pawn.Dead)
					pawn.UnDesignateMilking();
			}

			return SaveStorage.DataStore.GetPawnData(pawn).Milking;
		}
		[SyncMethod]
		public static void DesignateMilking(this Pawn pawn)
		{
			DesignatorsData.rjwMilking.AddDistinct(pawn);
			SaveStorage.DataStore.GetPawnData(pawn).Milking = true;
		}
		[SyncMethod]
		public static void UnDesignateMilking(this Pawn pawn)
		{
			DesignatorsData.rjwMilking.Remove(pawn);
			SaveStorage.DataStore.GetPawnData(pawn).Milking = false;
		}
	}
}

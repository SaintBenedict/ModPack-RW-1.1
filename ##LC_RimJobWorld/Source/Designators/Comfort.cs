using Verse;
using Multiplayer.API;

namespace rjw
{
	public static class PawnDesignations_Comfort
	{
		public static bool UpdateCanDesignateComfort(this Pawn pawn)
		{
			//rape disabled
			if (!RJWSettings.rape_enabled)
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = false;

			//no permission to change designation for NON prisoner hero/ other player
			if (!pawn.CanChangeDesignationPrisoner() && !pawn.CanChangeDesignationColonist())
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = false;

			//no permission to change designation for prisoner hero/ self
			if (!pawn.CanChangeDesignationPrisoner())
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = false;

			//cant sex
			if (!(xxx.can_fuck(pawn) || xxx.can_be_fucked(pawn)))
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = false;

			if (!pawn.IsDesignatedHero())
			{
				if ((xxx.is_masochist(pawn) || (RJWSettings.override_RJW_designation_checks && !MP.IsInMultiplayer)) && pawn.IsColonist)
					return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = true;
			}
			else if (pawn.IsHeroOwner())
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = true;

			if (pawn.IsPrisonerOfColony || xxx.is_slave(pawn))
				return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = true;

			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort = false;
		}
		public static bool CanDesignateComfort(this Pawn pawn)
		{
			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateComfort;
		}
		public static void ToggleComfort(this Pawn pawn)
		{
			pawn.UpdateCanDesignateComfort();
			if (pawn.CanDesignateComfort())
			{
				if (!pawn.IsDesignatedComfort())
					DesignateComfort(pawn);
				else
					UnDesignateComfort(pawn);
			}
		}
		public static bool IsDesignatedComfort(this Pawn pawn)
		{
			if (SaveStorage.DataStore.GetPawnData(pawn).Comfort)
			{
				if (!pawn.IsDesignatedHero())
				{
					if (!pawn.IsPrisonerOfColony)
						if (!(xxx.is_masochist(pawn) || xxx.is_slave(pawn)))
						{
							if (!pawn.IsColonist)
								UnDesignateComfort(pawn);
							else if (!(RJWSettings.WildMode || (RJWSettings.override_RJW_designation_checks && !MP.IsInMultiplayer)))
								UnDesignateComfort(pawn);
						}
				}

				if (pawn.Dead)
					pawn.UnDesignateComfort();
			}

			return SaveStorage.DataStore.GetPawnData(pawn).Comfort;
		}
		[SyncMethod]
		public static void DesignateComfort(this Pawn pawn)
		{
			DesignatorsData.rjwComfort.AddDistinct(pawn);
			SaveStorage.DataStore.GetPawnData(pawn).Comfort = true;
		}
		[SyncMethod]
		public static void UnDesignateComfort(this Pawn pawn)
		{
			DesignatorsData.rjwComfort.Remove(pawn);
			SaveStorage.DataStore.GetPawnData(pawn).Comfort = false;
		}
	}
}

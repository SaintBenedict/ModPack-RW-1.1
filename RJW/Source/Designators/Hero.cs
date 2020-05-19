using Verse;
using RimWorld;
using Multiplayer.API;

namespace rjw
{
	public static class PawnDesignations_Hero
	{
		public static bool UpdateCanDesignateHero(this Pawn pawn)
		{
			if ((RJWSettings.RPG_hero_control)
				&& xxx.is_human(pawn)
				&& pawn.IsColonist)
			{
				if (!pawn.IsDesignatedHero())
				{
					foreach (Pawn item in DesignatorsData.rjwHero)
					{
						if (item.IsHeroOwner())
						{
							if (RJWSettings.RPG_hero_control_Ironman && !SaveStorage.DataStore.GetPawnData(item).Ironman)
								SetHeroIronman(item);
							if (item.Dead && !SaveStorage.DataStore.GetPawnData(item).Ironman)
							{
								UnDesignateHero(item);
								//Log.Warning("CanDesignateHero:: "  + MP.PlayerName + " hero is dead remove hero tag from " + item.Name);
							}
							else
							{
								//Log.Warning("CanDesignateHero:: "  + MP.PlayerName + " already has hero - " + item.Name);
								return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateHero = false;
							}

						}
						else
							continue;
					}
					return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateHero = true;
				}
			}
			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateHero = false;
		}
		public static bool CanDesignateHero(this Pawn pawn)
		{
			return SaveStorage.DataStore.GetPawnData(pawn).CanDesignateHero;
		}
		public static void ToggleHero(this Pawn pawn)
		{
			pawn.UpdateCanDesignateHero();
			if (pawn.CanDesignateHero() && Find.Selector.NumSelected == 1)
			{
				if (!pawn.IsDesignatedHero())
					DesignateHero(pawn);
			}
		}
		public static bool IsDesignatedHero(this Pawn pawn)
		{
			return SaveStorage.DataStore.GetPawnData(pawn).Hero;
		}
		public static void DesignateHero(this Pawn pawn)
		{
			MyMethod(pawn, MP.PlayerName);
		}
		[SyncMethod]
		public static void UnDesignateHero(this Pawn pawn)
		{
			DesignatorsData.rjwHero.Remove(pawn);
			SaveStorage.DataStore.GetPawnData(pawn).Hero = false;
		}
		public static bool IsHeroOwner(this Pawn pawn)
		{
			if (!MP.enabled)
				return SaveStorage.DataStore.GetPawnData(pawn).HeroOwner == "Player" || SaveStorage.DataStore.GetPawnData(pawn).HeroOwner == null || SaveStorage.DataStore.GetPawnData(pawn).HeroOwner == "";
			else
				return SaveStorage.DataStore.GetPawnData(pawn).HeroOwner == MP.PlayerName;
		}
		[SyncMethod]
		static void MyMethod(Pawn pawn, string theName)
		{
			if (!MP.enabled)
				theName = "Player";
			SaveStorage.DataStore.GetPawnData(pawn).Hero = true;
			SaveStorage.DataStore.GetPawnData(pawn).HeroOwner = theName;
			SaveStorage.DataStore.GetPawnData(pawn).Ironman = RJWSettings.RPG_hero_control_Ironman;
			DesignatorsData.rjwHero.AddDistinct(pawn);
			string text = pawn.Name + " is now hero of " + theName;
			Messages.Message(text, pawn, MessageTypeDefOf.NeutralEvent);
			//Log.Message(MP.PlayerName + "  set " + pawn.Name + " to hero:" + SaveStorage.DataStore.GetPawnData(pawn).Hero);

			pawn.UpdateCanChangeDesignationPrisoner();
			pawn.UpdateCanChangeDesignationColonist();
			pawn.UpdateCanDesignateService();
			pawn.UpdateCanDesignateComfort();
			pawn.UpdateCanDesignateBreedingAnimal();
			pawn.UpdateCanDesignateBreeding();
			pawn.UpdateCanDesignateHero();
		}
		[SyncMethod]
		public static void SetHeroIronman(this Pawn pawn)
		{
			SaveStorage.DataStore.GetPawnData(pawn).Ironman = true;
		}
	}
}

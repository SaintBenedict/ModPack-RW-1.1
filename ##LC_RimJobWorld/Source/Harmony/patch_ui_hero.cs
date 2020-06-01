using System.Linq;
using Verse;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace rjw
{
	/// <summary>
	/// Patch ui for hero mode
	/// - disable pawn control for non owned hero
	/// - disable equipment management for non owned hero
	/// hardcore mode:
	/// - disable equipment management for non hero
	/// - disable pawn rmb menu for non hero
	/// - remove drafting widget for non hero
	/// </summary>

	//disable forced works(rmb workgivers)
	[HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
	[StaticConstructorOnStartup]
	static class disable_FloatMenuMakerMap
	{
		[HarmonyPostfix]
		static void this_is_postfix(ref bool __result, Pawn pawn)
		{
			if (RJWSettings.RPG_hero_control)
			{
				if ((pawn.IsDesignatedHero() && !pawn.IsHeroOwner()))
				{
					__result = false;   //not hero owner, disable menu
					return;
				}

				if (!pawn.IsDesignatedHero() && RJWSettings.RPG_hero_control_HC)
				{
					if (pawn.Drafted && pawn.CanChangeDesignationPrisoner() && pawn.CanChangeDesignationColonist())
					{
						//allow control over drafted pawns, this is limited by below disable_Gizmos patch
					}
					else
					{
						__result = false; //not hero, disable menu
					}
				}
			}
		}
	}

	//TODO: disable equipment management
	/*
	//disable equipment management
	[HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControl")]
	static class disable_equipment_management
	{
		[HarmonyPostfix]
		static bool this_is_postfix(ref bool __result, Pawn selPawnForGear)
		{
			Pawn pawn = selPawnForGear;

			if (RJWSettings.RPG_hero_control)
			{
				if ((pawn.IsDesignatedHero() && !pawn.IsHeroOwner()))    //not hero owner, disable drafting
				{
					__result = false;   //not hero owner, disable menu
				}
				else if (!pawn.IsDesignatedHero() && RJWSettings.RPG_hero_control_HC)   //not hero, disable drafting
				{
					if (false)
					{
						//add some filter for bots and stuff? if there is such stuff
						//so it can be drafted and controlled for fighting
					}
					else
					{
						__result = false; //not hero, disable menu
					}
				}
			}
			return true;
		}
	}
	*/

	//TODO: fix error
	//TODO: allow shared control over non colonists(droids, etc)?
	//disable drafting
	[HarmonyPatch(typeof(Pawn), "GetGizmos")]
	[StaticConstructorOnStartup]
	static class disable_Gizmos
	{
		[HarmonyPostfix]
		static void this_is_postfix(ref IEnumerable<Gizmo> __result, ref Pawn __instance)
		{
			Pawn pawn = __instance;
			List<Gizmo> gizmos = __result.ToList();

			if (RJWSettings.RPG_hero_control)
			{
				if ((pawn.IsDesignatedHero() && !pawn.IsHeroOwner()))    //not hero owner, disable drafting
				{
					foreach (var x in __result.ToList())
					{
						try
						{
							//Log.Message("disable_drafter gizmos: " + x);
							if ((x as Command).defaultLabel == "Draft")
							gizmos.Remove(x as Gizmo);
						}
						catch
						{ }
					};
				}
				else if (!pawn.IsDesignatedHero() && RJWSettings.RPG_hero_control_HC)   //not hero, disable drafting
				{
					//no permission to change designation for NON prisoner hero/ other player
					if (pawn.CanChangeDesignationPrisoner() && pawn.CanChangeDesignationColonist()
							&& (pawn.kindDef.race.defName.Contains("AIRobot")
							|| (pawn.kindDef.race.defName.Contains("Droid") && !pawn.kindDef.race.defName.Contains("AndDroid"))
							|| pawn.kindDef.race.defName.Contains("RPP_Bot")
							))
					//if (false) 
					{
						//add some filter for bots and stuff? if there is such stuff
						//so it can be drafted and controlled for fighting
					}
					else
					{
						foreach (var x in __result.ToList())
						{
							try
							{
								//this may cause error
								//ie pawn with shield, or maybe other equipment added gizmos
								//maybe because they are not Command?
								//w/e just catch error and ignore

								//Log.Message("disable_drafter gizmos: " + x);
								if ((x as Command).defaultLabel == "Draft")
									gizmos.Remove(x);
							}
							catch
							{ }
						};
					}
				}
			}
			__result = gizmos.AsEnumerable();
		}
	}
}

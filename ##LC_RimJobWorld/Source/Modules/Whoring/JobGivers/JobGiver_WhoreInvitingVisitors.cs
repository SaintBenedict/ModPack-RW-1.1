using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobGiver_WhoreInvitingVisitors : ThinkNode_JobGiver
	{
		// Checks if pawn has a memory. 
		// Maybe not the best place for function, might be useful elsewhere too.
		public static bool MemoryChecker(Pawn pawn, ThoughtDef thought)
		{
			Thought_Memory val = pawn.needs.mood.thoughts.memories.Memories.Find((Thought_Memory x) => (object)x.def == thought);
			return val == null ? false : true;
		}

		[SyncMethod]
		private static bool Roll_to_skip(Pawn client, Pawn whore)
		{
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float fuckability = SexAppraiser.would_fuck(client, whore); // 0.0 to 1.

			// More likely to skip other whores, because they're supposed to be working.
			if (client.IsDesignatedService())
				fuckability *= 0.6f;
			return fuckability >= 0.1f && xxx.need_some_sex(client) >= 1f && Rand.Chance(0.5f);
		}

		/*
		public static Pawn Find_pawn_to_fuck(Pawn whore, Map map)
		{
			Pawn best_fuckee = null;
			float best_distance = 1.0e6f;
			foreach (Pawn q in map.mapPawns.FreeColonists)
				if ((q != whore) &&
					xxx.need_some_sex(q)>0 &&
					whore.CanReserve(q, 1, 0) &&
					q.CanReserve(whore, 1, 0) &&
					Roll_to_skip(whore, q) &&
					(!q.Position.IsForbidden(whore)) &&
					xxx.is_healthy(q))
				{
					var dis = whore.Position.DistanceToSquared(q.Position);
					if (dis < best_distance)
					{
						best_fuckee = q;
						best_distance = dis;
					}
				}
			return best_fuckee;
		}
		*/

		private sealed class FindAttractivePawnHelper
		{
			internal Pawn whore;

			internal bool TraitCheckFail(Pawn client)
			{
				if (!xxx.is_human(client))
					return true;
				if (!xxx.has_traits(client))
					return true;
				if (!(xxx.can_fuck(client) || xxx.can_be_fucked(client)) || !xxx.IsTargetPawnOkay(client))
					return true;

				//Log.Message("client:" + client + " whore:" + whore);
				if (CompRJW.CheckPreference(client, whore) == false)
					return true;
				return false; // Everything ok.
			}

			//Use this check when client is not in the same faction as the whore
			internal bool FactionCheckPass(Pawn client)
			{
				return ((client.Map == whore.Map) && (client.Faction != null && client.Faction != whore.Faction) && !client.IsPrisoner && !client.HostileTo(whore));
			}

			//Use this check when client is in the same faction as the whore
			[SyncMethod]
			internal bool RelationCheckPass(Pawn client)
			{
				//Rand.PopState();
				//Rand.PushState(RJW_Multiplayer.PredictableSeed());
				if (xxx.isSingleOrPartnerNotHere(client) || xxx.is_lecher(client) || Rand.Value < 0.9f)
				{
					if (client != LovePartnerRelationUtility.ExistingLovePartner(whore))
					{ //Exception for prisoners to account for PrisonerWhoreSexualEmergencyTree, which allows prisoners to try to hook up with anyone who's around (mostly other prisoners or warden)
						return (client != whore) & (client.Map == whore.Map) && (client.Faction == whore.Faction || whore.IsPrisoner) && (client.IsColonist || whore.IsPrisoner) && WhoringHelper.IsHookupAppealing(whore, client);
					}
				}
				return false;
			}
		}

		[SyncMethod]
		public static Pawn FindAttractivePawn(Pawn whore, out int price)
		{
			price = 0;
			if (whore == null || xxx.is_asexual(whore))
			{
				if (RJWSettings.DebugWhoring) Log.Message($" {xxx.get_pawnname(whore)} is asexual, abort");
				return null;
			}
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			FindAttractivePawnHelper client = new FindAttractivePawnHelper
			{
				whore = whore
			};
			price = WhoringHelper.PriceOfWhore(whore);
			int priceOfWhore = price;

			IntVec3 pos = whore.Position;

			IEnumerable<Pawn> potentialClients = whore.Map.mapPawns.AllPawnsSpawned;
			potentialClients = potentialClients.Where(x
				=> x != whore
				&& !x.IsForbidden(whore)
				&& !x.HostileTo(whore)
				&& !x.IsPrisoner
				&& x.Position.DistanceTo(pos) < 100 
				&& whore.CanReserveAndReach(x, PathEndMode.ClosestTouch, Danger.Some, 1) 
				&& xxx.is_healthy_enough(x));

			potentialClients = potentialClients.Except(potentialClients.Where(client.TraitCheckFail));

			if (!potentialClients.Any()) return null;

			if (RJWSettings.DebugWhoring) Log.Message($" FindAttractivePawn number of all potential clients {potentialClients.Count()}");

			List<Pawn> valid_targets = new List<Pawn>();

			foreach (Pawn target in potentialClients)
			{
				if(xxx.can_path_to_target(whore, target.Position))
					valid_targets.Add(target);
			}

			IEnumerable<Pawn> guestsSpawned = valid_targets.Where(x => x.Faction != whore.Faction
				&& WhoringHelper.CanAfford(x, whore, priceOfWhore)
				&& !MemoryChecker(x, ThoughtDef.Named("RJWFailedSolicitation"))
				&& x != LovePartnerRelationUtility.ExistingLovePartner(whore));

			if (guestsSpawned.Any())
			{
				if (RJWSettings.DebugWhoring) Log.Message($" FindAttractivePawn number of all acceptable Guests {guestsSpawned.Count()}");
				return guestsSpawned.RandomElement();
			}

			return null;
			//use casual sex for colonist hooking
			if (RJWSettings.DebugWhoring) Log.Message($" FindAttractivePawn found no guests, trying colonists");

			if (!WhoringHelper.WillPawnTryHookup(whore))// will hookup colonists?
			{
				return null;
			}
			IEnumerable<Pawn> freeColonists = valid_targets.Where(x => x.Faction == whore.Faction
				&& Roll_to_skip(x, whore));

			if (RJWSettings.DebugWhoring) Log.Message($" FindAttractivePawn number of free colonists {freeColonists.Count()}");

			freeColonists = freeColonists.Where(x => client.RelationCheckPass(x) && !MemoryChecker(x, ThoughtDef.Named("RJWTurnedDownWhore")));

			if (freeColonists.Any())
			{
				if (RJWSettings.DebugWhoring) Log.Message($" FindAttractivePawn number of all acceptable Colonists {freeColonists.Count()}");
				return freeColonists.RandomElement();
			}

			return null;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{

			// Most things are now checked in the ThinkNode_ConditionalWhore.

			if (pawn.Drafted) return null;
			if (MP.IsInMultiplayer) return null; //fix error someday, maybe

			if (!SexUtility.ReadyForLovin(pawn))
			{
				//Whores need rest too, but this'll reduxe the wait a bit every it triggers.
				pawn.mindState.canLovinTick -= 40;
				return null;
			}

			if (RJWSettings.DebugWhoring) Log.Message($"[RJW] JobGiver_WhoreInvitingVisitors.TryGiveJob:({xxx.get_pawnname(pawn)})");
			Building_Bed whorebed = xxx.FindBed(pawn);
			if (whorebed == null || !xxx.CanUse(pawn, whorebed))
			{
				if (RJWSettings.DebugWhoring) Log.Message($" {xxx.get_pawnname(pawn)} has no bed or can use it");
				return null;
			}

			int price;
			Pawn client = FindAttractivePawn(pawn, out price);
			if (client == null)
			{
				if (RJWSettings.DebugWhoring) Log.Message($" no clients found");
				return null;
			}

			if (RJWSettings.DebugWhoring) Log.Message($" {xxx.get_pawnname(client)} is client");

			if (!client.CanReach(whorebed, PathEndMode.OnCell, Danger.Some))
			{
				if (RJWSettings.DebugWhoring) Log.Message($" {xxx.get_pawnname(client)} cant reach bed");
				return null;
			}
			//whorebed.priceOfWhore = price;
			return JobMaker.MakeJob(xxx.whore_inviting_visitors, client, whorebed);
		}
	}
}
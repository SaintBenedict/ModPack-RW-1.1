using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	public class JobGiver_DoQuickie : ThinkNode_JobGiver
	{
		private const int MaxDistanceSquaredToFuck = 10000;

		private static bool CanFuck(Pawn target)
		{
			return xxx.can_fuck(target) || xxx.can_be_fucked(target);
		}

		[SyncMethod]
		private static bool roll_to_skip(Pawn pawn, Pawn target, out float fuckability)
		{
			fuckability = SexAppraiser.would_fuck(pawn, target); // 0.0 to 1.0
			if (fuckability < RJWHookupSettings.MinimumFuckabilityToHookup)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] roll_to_skip(I, {xxx.get_pawnname(pawn)} won't fuck {xxx.get_pawnname(target)}), ({fuckability})");
				return false;
			}

			float reciprocity = xxx.is_animal(target) ? 1.0f : SexAppraiser.would_fuck(target, pawn);
			if (reciprocity < RJWHookupSettings.MinimumFuckabilityToHookup)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] roll_to_skip({xxx.get_pawnname(target)} won't fuck me, {xxx.get_pawnname(pawn)}), ({reciprocity})");
				return false;
			}
			
			float chance_to_skip = 0.9f - 0.7f * fuckability;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			return Rand.Value < chance_to_skip;
		}

		[SyncMethod]
		public static Pawn find_pawn_to_fuck(Pawn pawn, Map map)
		{
			string pawnName = xxx.get_pawnname(pawn);
			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): starting.");

			bool pawnIsNympho = xxx.is_nympho(pawn);
			bool pawnCanPickAnyone = RJWSettings.WildMode || (pawnIsNympho && RJWHookupSettings.NymphosCanPickAnyone);
			bool pawnCanPickAnimals = (pawnCanPickAnyone || xxx.is_zoophile(pawn)) && RJWSettings.bestiality_enabled;

			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): nympho:{pawnIsNympho}, ignores rules:{pawnCanPickAnyone}, zoo:{pawnCanPickAnimals}");

			if (!RJWHookupSettings.ColonistsCanHookup && pawn.IsFreeColonist && !pawnCanPickAnyone)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): is a colonist and colonist hookups are disabled in mod settings");
				return null;
			}

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			// Check AllPawns, not just colonists, to include guests.
			List<Pawn> targets = map.mapPawns.AllPawns.Where(x 
				=> x != pawn
				&& !x.InBed()
				&& !x.IsForbidden(pawn)
				&& xxx.IsTargetPawnOkay(x) 
				&& CanFuck(x)
				&& x.Map == pawn.Map 
				&& !x.HostileTo(pawn)
				//&& (pawnCanPickAnimals || !xxx.is_animal(x))
				&& !xxx.is_animal(x)
				).ToList();

			if (!targets.Any())
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): no eligible targets");
				return null;
			}

			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck quickie({pawnName}): considering {targets.Count} targets");

			// find lover/partner on same map
			List<Pawn> partners = targets.Where(x 
				=> pawn.relations.DirectRelationExists(PawnRelationDefOf.Lover, x) 
				|| pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, x) 
				|| pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, x)
				).ToList();

			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): considering {partners.Count} partners");

			if (partners.Any())
			{
				partners.Shuffle(); //Randomize order.
				foreach (Pawn target in partners)
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): checking lover {xxx.get_pawnname(target)}");

					//interruptible jobs
					if (target.jobs?.curJob !=null &&
						(target.jobs.curJob.playerForced ||
						JobDriver_SexQuick.quickieAllowedJobs.Contains(target.jobs.curJob.def)
						))
					{
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): lover has important job, skipping");
						continue;
					}
					if (pawn.Position.DistanceToSquared(target.Position) < MaxDistanceSquaredToFuck
						&& pawn.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Some, 1, 0)
						&& target.CanReserve(pawn, 1, 0)
						&& SexAppraiser.would_fuck(pawn, target) > 0.1f
						&& SexAppraiser.would_fuck(target, pawn) > 0.1f)
					{
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): banging lover {xxx.get_pawnname(target)}");
						return target;
					}
				}
			}

			// No lovers around... see if the pawn fancies a hookup.  Nymphos and frustrated pawns always do!
			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): no partners available.  checking canHookup");
			bool canHookup = pawnIsNympho || pawnCanPickAnyone || xxx.is_frustrated(pawn) || (xxx.is_horny(pawn) && Rand.Value < RJWHookupSettings.HookupChanceForNonNymphos);
			if (!canHookup)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): no hookup today");
				return null;
			}

			// No cheating from casual hookups... would probably make colony relationship management too annoying
			if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): checking canHookupWithoutCheating");
			bool hookupWouldBeCheating = xxx.HasNonPolyPartnerOnCurrentMap(pawn);
			if (hookupWouldBeCheating)
			{
				if (RJWHookupSettings.NymphosCanCheat && pawnIsNympho && xxx.is_frustrated(pawn))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): I'm a nympho and I'm so frustrated that I'm going to cheat");
					// No return here so they continue searching for hookup
				}
				else
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({pawnName}): I want to bang but that's cheating");
					return null;
				}
			}

			Pawn best_fuckee = FindBestPartner(pawn, targets, pawnCanPickAnyone, pawnIsNympho);
			return best_fuckee;
		}

		/// <summary> Checks all of our potential partners to see if anyone's eligible, returning the most attractive and convenient one. </summary>
		protected static Pawn FindBestPartner(Pawn pawn, List<Pawn> targets, bool pawnCanPickAnyone, bool pawnIsNympho)
		{
			string pawnName = xxx.get_pawnname(pawn);

			Pawn best_fuckee = null;
			float best_fuckability_score = 0;

			foreach (Pawn targetPawn in targets)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): checking hookup {xxx.get_pawnname(targetPawn)}");

				// Check to see if the mod settings for hookups allow this pairing
				if (!pawnCanPickAnyone && !HookupAllowedViaSettings(pawn, targetPawn))
					continue;

				//interruptible jobs
				if (targetPawn.jobs?.curJob != null &&
					(targetPawn.jobs.curJob.playerForced ||
					JobDriver_SexQuick.quickieAllowedJobs.Contains(targetPawn.jobs.curJob.def)
					))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): targetPawn has important job, skipping");
					continue;
				}

				// Check for homewrecking (banging a pawn who's in a relationship)
				if (!xxx.is_animal(targetPawn) &&
					xxx.HasNonPolyPartnerOnCurrentMap(targetPawn))
				{
					if (RJWHookupSettings.NymphosCanHomewreck && pawnIsNympho && xxx.is_frustrated(pawn))
					{
						// Hookup allowed... rip colony mood
					}
					else if (RJWHookupSettings.NymphosCanHomewreckReverse && xxx.is_nympho(targetPawn) && xxx.is_frustrated(targetPawn))
					{
						// Hookup allowed... rip colony mood
					}
					else
					{
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): not hooking up with {xxx.get_pawnname(targetPawn)} to avoid homewrecking");
						continue;
					}
				}

				// If the pawn has had sex recently and isn't horny right now, skip them.
				if (!SexUtility.ReadyForLovin(targetPawn) && !xxx.is_hornyorfrustrated(targetPawn))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)} isn't ready for lovin'");
					continue;
				}

				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)} is sufficiently single");

				if (!xxx.is_animal(targetPawn))
				{
					float relations = pawn.relations.OpinionOf(targetPawn);
					if (relations < RJWHookupSettings.MinimumRelationshipToHookup)
					{
						if (!(relations > 0 && xxx.is_nympho(pawn)))
						{
							if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)}, i dont like them:({relations})");
							continue;
						}
					}

					relations = targetPawn.relations.OpinionOf(pawn);
					if (relations < RJWHookupSettings.MinimumRelationshipToHookup)
					{
						if (!(relations > 0 && xxx.is_nympho(targetPawn)))
						{
							if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)}, dont like me:({relations})");
							continue;
						}
					}

					float attraction = pawn.relations.SecondaryRomanceChanceFactor(targetPawn);
					if (attraction < RJWHookupSettings.MinimumAttractivenessToHookup)
					{
						if (!(attraction > 0 && xxx.is_nympho(pawn)))
						{
							if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)}, i dont find them attractive:({attraction})");
							continue;
						}
					}
					attraction = targetPawn.relations.SecondaryRomanceChanceFactor(pawn);
					if (attraction < RJWHookupSettings.MinimumAttractivenessToHookup)
					{
						if (!(attraction > 0 && xxx.is_nympho(targetPawn)))
						{
							if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)}, doesnt find me attractive:({attraction})");
							continue;
						}
					}
				}

				// Check to see if the two pawns are willing to bang, and if so remember how much attractive we find them
				float fuckability = 0f;
				if (pawn.CanReserveAndReach(targetPawn, PathEndMode.OnCell, Danger.Some, 1, 0) &&
					targetPawn.CanReserve(pawn, 1, 0) &&
					roll_to_skip(pawn, targetPawn, out fuckability)) // do NOT check pawnIgnoresRules here - these checks, particularly roll_to_skip, are critical
				{
					int dis = pawn.Position.DistanceToSquared(targetPawn.Position);

					if (dis <= 4)
					{
						// Right next to me (in my bed)?  You'll do.
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)} is right next to me.  we'll bang, ok?");
						best_fuckability_score = 1.0e6f;
						best_fuckee = targetPawn;
					}
					else if (dis > MaxDistanceSquaredToFuck)
					{
						// too far
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)} is too far... distance:{dis} max:{MaxDistanceSquaredToFuck}");
						continue;
					}
					else
					{
						// scaling fuckability by distance may give us more varied results and give the less attractive folks a chance
						float fuckability_score = fuckability / GenMath.Sqrt(GenMath.Sqrt(dis));
						if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): hookup {xxx.get_pawnname(targetPawn)} is totally bangable.  attraction: {fuckability}, score:{fuckability_score}");

						if (fuckability_score > best_fuckability_score)
						{
							best_fuckee = targetPawn;
							best_fuckability_score = fuckability_score;
						}
					}
				}
			}

			if (best_fuckee == null)
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): couldn't find anyone to bang");
			}
			else
			{
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] FindBestPartner({pawnName}): found rando {xxx.get_pawnname(best_fuckee)} with score {best_fuckability_score}");
			}

			return best_fuckee;
		}

		/// <summary> Checks to see if the mod settings allow the two pawns to hookup. </summary>
		protected static bool HookupAllowedViaSettings(Pawn pawn, Pawn targetPawn)
		{
			// Can prisoners hook up?
			if (pawn.IsPrisonerOfColony || pawn.IsPrisoner || xxx.is_slave(pawn))
			{
				if (!RJWHookupSettings.PrisonersCanHookupWithNonPrisoner && !(targetPawn.IsPrisonerOfColony || targetPawn.IsPrisoner || xxx.is_slave(targetPawn)))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting PrisonersCanHookupWithNonPrisoner");
					return false;
				}

				if (!RJWHookupSettings.PrisonersCanHookupWithPrisoner && (targetPawn.IsPrisonerOfColony || targetPawn.IsPrisoner || xxx.is_slave(targetPawn)))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting PrisonersCanHookupWithPrisoner");
					return false;
				}
			}
			else
			{
				// Can non prisoners hook up with prisoners?
				if (!RJWHookupSettings.CanHookupWithPrisoner && (targetPawn.IsPrisonerOfColony || targetPawn.IsPrisoner || xxx.is_slave(targetPawn)))
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting CanHookupWithPrisoner");
					return false;
				}
			}

			// Can colonist hook up with visitors?
			if (pawn.IsFreeColonist && !xxx.is_slave(pawn))
			{
				if (!RJWHookupSettings.ColonistsCanHookupWithVisitor && targetPawn.Faction != Faction.OfPlayer && !targetPawn.IsPrisonerOfColony)
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting ColonistsCanHookupWithVisitor");
					return false;
				}
			}

			// Can visitors hook up?
			if (pawn.Faction != Faction.OfPlayer && !pawn.IsPrisonerOfColony)
			{
				// visitors vs colonist
				if (!RJWHookupSettings.VisitorsCanHookupWithColonists && targetPawn.IsColonist)
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting VisitorsCanHookupWithColonists");
					return false;
				}

				// visitors vs visitors
				if (!RJWHookupSettings.VisitorsCanHookupWithVisitors && targetPawn.Faction != Faction.OfPlayer)
				{
					if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] find_pawn_to_fuck({xxx.get_pawnname(pawn)}): not hooking up with {xxx.get_pawnname(targetPawn)} due to mod setting VisitorsCanHookupWithVisitors");
					return false;
				}
			}

			// TODO: Not sure if this handles all the pawn-on-animal cases.

			return true;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!RJWHookupSettings.HookupsEnabled || !RJWHookupSettings.QuickHookupsEnabled)
				return null;

			if (pawn.Drafted)
				return null;

			if (!SexUtility.ReadyForHookup(pawn))
				return null;

			// We increase the time right away to prevent the fairly expensive check from happening too frequently
			SexUtility.IncreaseTicksToNextHookup(pawn);

			// If the pawn is a whore, or recently had sex, skip the job unless they're really horny
			if (!xxx.is_frustrated(pawn) && (xxx.is_whore(pawn) || !SexUtility.ReadyForLovin(pawn)))
				return null;

			// This check attempts to keep groups leaving the map, like guests or traders, from turning around to hook up
			if (pawn.mindState?.duty?.def == DutyDefOf.TravelOrLeave)
			{
				// TODO: Some guest pawns keep the TravelOrLeave duty the whole time, I think the ones assigned to guard the pack animals.
				// That's probably ok, though it wasn't the intention.
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJWQ] Quickie.TryGiveJob:({xxx.get_pawnname(pawn)}): has TravelOrLeave, no time for lovin!");
				return null;
			}

			if (pawn.CurJob == null)
			{
				//--Log.Message("   checking pawn and abilities");
				if (xxx.can_fuck(pawn) || xxx.can_be_fucked(pawn))
				{
					//--Log.Message("   finding partner");
					Pawn partner = find_pawn_to_fuck(pawn, pawn.Map);

					//--Log.Message("   checking partner");
					if (partner == null)
						return null;

					// Interrupt current job.
					if (pawn.CurJob != null && pawn.jobs.curDriver != null)
						pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);

					//--Log.Message("   returning job");
					return JobMaker.MakeJob(xxx.quick_sex, partner);
				}
			}

			return null;
		}
	}
}

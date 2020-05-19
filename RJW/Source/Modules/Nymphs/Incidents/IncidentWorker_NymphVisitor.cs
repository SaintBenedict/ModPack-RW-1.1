using System.Linq;
using RimWorld;
using Verse;
using Multiplayer.API;

namespace rjw
{
	public class IncidentWorker_NymphVisitor : IncidentWorker
	{
		[SyncMethod]
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			//--Log.Message("IncidentWorker_NymphVisitor::TryExecute() called");

			if (!RJWSettings.NymphWild) return false;
			if (MP.IsInMultiplayer) return false;
			Map map = (Map) parms.target;

			if (map == null)
			{
				//--Log.Message("IncidentWorker_NymphVisitor::TryExecute() - map is null, abort!");
				return false;
			}
			else
			{
				//--Log.Message("IncidentWorker_NymphVisitor::TryExecute() - map is ok");
			}

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 loc, map, CellFinder.EdgeRoadChance_Friendly + 0.2f))
			{
				//--Log.Message("IncidentWorker_NymphVisitor::TryExecute() - no entry, abort!");
				return false;
			}

			Pawn pawn = Nymph_Generator.GenerateNymph(loc, ref map); //generates with null faction, mod conflict ?!
			GenSpawn.Spawn(pawn, loc, map);

			pawn.ChangeKind(PawnKindDefOf.WildMan);
			//if (pawn.Faction != null)
			//	pawn.SetFaction(null);

			if (RJWSettings.NymphPermanentManhunter)
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
			else
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);

			Find.LetterStack.ReceiveLetter("Nymph! ", "A wandering nymph has decided to visit your colony.", LetterDefOf.ThreatSmall, pawn);

			return true;
		}
	}
}
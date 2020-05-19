using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Linq;
using UnityEngine;


namespace rjw
{
	///<summary>
	///This hediff class simulates pregnancy with mechanoids, mother may be human. It is not intended to be reasonable.
	///Differences from bestial pregnancy are that ... it is lethal
	///TODO: extend with something "friendlier"? than Mech_Scyther.... two Mech_Scyther's? muhahaha
	///</summary>	
	[RJWAssociatedHediff("RJW_pregnancy_mech")]
	public class Hediff_MechanoidPregnancy : Hediff_BasePregnancy
	{
		public override bool canBeAborted
		{
			get
			{
				return false;
			}
		}

		public override bool canMiscarry
		{
			get
			{
				return false;
			}
		}

		public override void PregnancyMessage()
		{
			string message_title = "RJW_PregnantTitle".Translate(pawn.LabelIndefinite());
			string message_text1 = "RJW_PregnantText".Translate(pawn.LabelIndefinite());
			string message_text2 = "RJW_PregnantMechStrange".Translate();
			Find.LetterStack.ReceiveLetter(message_title, message_text1 + "\n" + message_text2, LetterDefOf.ThreatBig, pawn);
		}

		public void Hack()
		{
			is_hacked = true;
		}

		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			GiveBirth();
		}

		//Handles the spawning of pawns
		public override void GiveBirth()
		{
			Pawn mother = pawn;
			if (mother == null)
				return;

			if (!babies.NullOrEmpty())
				foreach (Pawn baby in babies)
					baby.Discard(true);

			Faction spawn_faction = null;

			if (!is_hacked)
				spawn_faction = Faction.OfMechanoids;

			PawnGenerationRequest request = new PawnGenerationRequest(
				kind: PawnKindDef.Named("Mech_Scyther"),
				faction: spawn_faction,
				forceGenerateNewPawn: true,
				newborn: true
				);

			Pawn mech = PawnGenerator.GeneratePawn(request);
			PawnUtility.TrySpawnHatchedOrBornPawn(mech, mother);
			if (!is_hacked)
			{
				LordJob_MechanoidsDefend lordJob = new LordJob_MechanoidsDefend();
				Lord lord = LordMaker.MakeNewLord(mech.Faction, lordJob, mech.Map);
				lord.AddPawn(mech);
			}
			FilthMaker.TryMakeFilth(mech.PositionHeld, mech.MapHeld, mother.RaceProps.BloodDef, mother.LabelIndefinite());

			IEnumerable<BodyPartRecord> source = from x in mother.health.hediffSet.GetNotMissingParts() where 
												x.IsInGroup(BodyPartGroupDefOf.Torso)
												&& !x.IsCorePart
												//&& x.groups.Contains(BodyPartGroupDefOf.Torso)
												//&& x.depth == BodyPartDepth.Inside
												//&& x.height == BodyPartHeight.Bottom
												//someday include depth filter
												//so it doesnt cut out external organs (breasts)?
												//vag  is genital part and genital is external
												//anal is internal
												//make sep part of vag?
												//&& x.depth == BodyPartDepth.Inside
												select x;

			if (source.Any())
			{
				foreach (BodyPartRecord part in source)
				{
					mother.health.DropBloodFilth();
				}
				foreach (BodyPartRecord part in source)
				{
					Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, mother, part);
					hediff_MissingPart.lastInjury = HediffDefOf.Cut;
					hediff_MissingPart.IsFresh = true;
					mother.health.AddHediff(hediff_MissingPart);
				}
			}
			mother.health.RemoveHediff(this);
		}
	}
}

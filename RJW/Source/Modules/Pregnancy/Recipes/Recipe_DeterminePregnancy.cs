using RimWorld;
using System;
using System.Linq;
using Verse;
using System.Collections.Generic;

namespace rjw
{
	public class Recipe_DeterminePregnancy : RecipeWorker
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			/* Males can be impregnated by mechanoids, probably
			if (!xxx.is_female(pawn))
			{
				yield break;
			}
			*/
			BodyPartRecord part = pawn.RaceProps.body.corePart;
			if (recipe.appliedOnFixedBodyParts[0] != null)
				part = pawn.RaceProps.body.AllParts.Find(x => x.def == recipe.appliedOnFixedBodyParts[0]);
			if (part != null && (pawn.ageTracker.CurLifeStage.reproductive)
				|| pawn.IsPregnant(true))
			{
				yield return part;
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			var preg = (PregnancyHelper.GetPregnancy(pawn) as Hediff_BasePregnancy);

			if (preg != null)
			{
				preg.CheckPregnancy();
			}
			else
			{
				Messages.Message(xxx.get_pawnname(billDoer) + " has determined " + xxx.get_pawnname(pawn) + " is not pregnant.", MessageTypeDefOf.NeutralEvent);
			}
		}
	}
	public class Recipe_DeterminePaternity : RecipeWorker
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
				BodyPartRecord part = pawn.RaceProps.body.corePart;
			if (recipe.appliedOnFixedBodyParts[0] != null)
				part = pawn.RaceProps.body.AllParts.Find(x => x.def == recipe.appliedOnFixedBodyParts[0]);
			if (part != null && (pawn.ageTracker.CurLifeStage.reproductive)
				|| pawn.IsPregnant(true))
			{
				yield return part;
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			var preg = (PregnancyHelper.GetPregnancy(pawn) as Hediff_BasePregnancy);
			
			if (preg != null)
			{
				Messages.Message(xxx.get_pawnname(billDoer) + " has determined "+ xxx.get_pawnname(pawn) + " is pregnant and " + preg.father + " is the father.", MessageTypeDefOf.NeutralEvent);
				
				preg.CheckPregnancy();
			}
			else
			{
				Messages.Message(xxx.get_pawnname(billDoer) + " has determined " + xxx.get_pawnname(pawn) + " is not pregnant.", MessageTypeDefOf.NeutralEvent);
			}
		}
	}
}

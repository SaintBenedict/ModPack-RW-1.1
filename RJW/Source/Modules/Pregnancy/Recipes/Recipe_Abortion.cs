﻿using RimWorld;
using System;
using System.Linq;
using Verse;
using System.Collections.Generic;

namespace rjw
{
	public class Recipe_Abortion : Recipe_RemoveHediff
	{

		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			BodyPartRecord part = pawn.RaceProps.body.corePart;
			if (recipe.appliedOnFixedBodyParts[0] != null)
				part = pawn.RaceProps.body.AllParts.Find(x => x.def == recipe.appliedOnFixedBodyParts[0]);
			if (part != null)
			{
				bool isMatch = Hediff_BasePregnancy.KnownPregnancies() // For every known pregnancy
					.Where(x => pawn.health.hediffSet.HasHediff(HediffDef.Named(x), true) && recipe.removesHediff == HediffDef.Named(x)) // Find matching bodyparts 
					.Select(x => (Hediff_BasePregnancy)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named(x))) // get pregnancy hediff
					.Where(pregnancy => pregnancy.is_checked) // Find checked pregnancies (should be visible pregnancies there?)
					.Any(); // return true if found something
				if (isMatch)
				{
					yield return part;
				}
			}
		}
	}
}

namespace rjw
{
	//This class is not used now.
	/*
	public class ThinkNode_ChancePerHour_Whore : ThinkNode_ChancePerHour
	{
		protected override float MtbHours(Pawn pawn)
		{
			// Use the fappin mtb hours as the base number b/c it already accounts for things like age
			var base_mtb = xxx.config.whore_mtbh_mul * ThinkNode_ChancePerHour_Fappin.get_fappin_mtb_hours(pawn);
			if (base_mtb < 0.0f)
				return -1.0f;

			float desire_factor;
			{
				var need_sex = pawn.needs.TryGetNeed<Need_Sex>();
				if (need_sex != null)
				{
					if (need_sex.CurLevel <= need_sex.thresh_frustrated())
						desire_factor = 0.15f;
					else if (need_sex.CurLevel <= need_sex.thresh_horny())
						desire_factor = 0.60f;
					else
						desire_factor = 1.00f;
				}
				else
					desire_factor = 1.00f;
			}

			float personality_factor;
			{
				personality_factor = 1.0f;
				if (pawn.story != null)
				{
					foreach (var trait in pawn.story.traits.allTraits)
					{
						if (trait.def == xxx.nymphomaniac) personality_factor *= 0.25f;
						else if (trait.def == TraitDefOf.Greedy) personality_factor *= 0.50f;
						else if (xxx.RomanceDiversifiedIsActive&&(trait.def==xxx.philanderer || trait.def == xxx.polyamorous)) personality_factor *= 0.75f;
						else if (xxx.RomanceDiversifiedIsActive && (trait.def == xxx.faithful)&& LovePartnerRelationUtility.HasAnyLovePartner(pawn)) personality_factor *= 30f;
					}
				}
			}

			float fun_factor;
			{
				if ((pawn.needs.joy != null) && (xxx.is_nympho(pawn)))
					fun_factor = Mathf.Clamp01(0.50f + pawn.needs.joy.CurLevel);
				else
					fun_factor = 1.00f;
			}

			var gender_factor = (pawn.gender == Gender.Male) ? 1.0f : 3.0f;

			return base_mtb * desire_factor * personality_factor * fun_factor * gender_factor;
		}
	}
	*/
}
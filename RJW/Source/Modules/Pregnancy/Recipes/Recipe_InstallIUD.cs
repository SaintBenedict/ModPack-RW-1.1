using System.Collections.Generic;
using Verse;

namespace rjw
{
	/// <summary>
	/// IUD - prevent pregnancy
	/// </summary>
	public class Recipe_InstallIUD : Recipe_InstallImplantToExistParts
	{
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			if (!xxx.is_female(pawn))
			{
				return new List<BodyPartRecord>();
			}
			return base.GetPartsToApplyOn(pawn, recipe);
		}
	}
}

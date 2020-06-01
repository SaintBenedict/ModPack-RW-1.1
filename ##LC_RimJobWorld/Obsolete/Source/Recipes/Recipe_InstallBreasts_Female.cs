//unused anymore
//using System.Collections.Generic;
//using RimWorld;
//using Verse;

//namespace rjw
//{
//	public class Recipe_InstallBreasts_Female : Recipe_InstallArtificialBodyPart
//	{
//		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn p, RecipeDef r)
//		{
//			var gen_blo = Genital_Helper.breasts_blocked(p);
//			if (p.gender == Gender.Female)
//			{
//				foreach (var part in base.GetPartsToApplyOn(p, r))
//					if ((!gen_blo) || (part != xxx.breasts))
//						yield return part;
//			}
//		}
//	}
//}
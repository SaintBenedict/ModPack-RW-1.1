using HarmonyLib;
using Verse;

/// <summary>
/// patches PawnGenerator to add genitals to pawns and spawn nymph when needed
/// </summary>
namespace rjw
{
	[HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
	static class Patch_PawnGenerator
	{
		[HarmonyPrefix]
		static void Before_GenerateNewPawnInternal(ref PawnGenerationRequest request)
		{
			if (Nymph_Generator.IsNymph(request))
			{
				request = new PawnGenerationRequest(
					kind: request.KindDef = Nymph_Generator.GetFixedNymphPawnKindDef(),
					canGeneratePawnRelations: request.CanGeneratePawnRelations = false,
					validatorPreGear: Nymph_Generator.IsNymphBodyType,
					validatorPostGear: Nymph_Generator.IsNymphBodyType,
					fixedGender: request.FixedGender = Nymph_Generator.RandomNymphGender()
					);
			}
		}

		[HarmonyPostfix]
		static void After_GenerateNewPawnInternal(ref PawnGenerationRequest request, ref Pawn __result)
		{
			if (Nymph_Generator.IsNymph(request))
			{
				Nymph_Generator.set_story(__result);
				Nymph_Generator.set_skills(__result);
			}

			//Log.Message("[RJW]After_GenerateNewPawnInternal:: " + xxx.get_pawnname(__result));
			if (CompRJW.Comp(__result) != null && CompRJW.Comp(__result).orientation == Orientation.None)
			{
				//Log.Message("[RJW]After_GenerateNewPawnInternal::Sexualize " + xxx.get_pawnname(__result));
				CompRJW.Comp(__result).Sexualize(__result);
			}
		}
	}
}

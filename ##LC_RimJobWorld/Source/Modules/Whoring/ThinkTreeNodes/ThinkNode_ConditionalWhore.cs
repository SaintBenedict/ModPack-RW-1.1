using Verse;
using Verse.AI;
using RimWorld;

namespace rjw
{
	/// <summary>
	/// Whore/prisoner look for customers
	/// </summary>
	public class ThinkNode_ConditionalWhore : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn p)
		{
			// No animal whorin' for now.
			if (xxx.is_animal(p))
				return false;

			if (!InteractionUtility.CanInitiateInteraction(p))
				return false;

			return xxx.is_whore(p);
		}
	}
}
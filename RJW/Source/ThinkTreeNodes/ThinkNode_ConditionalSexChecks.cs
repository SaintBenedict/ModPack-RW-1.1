using Verse;
using Verse.AI;

namespace rjw
{
	/// <summary>
	/// Called to determine if the pawn can engage in sex. 
	/// This should be used as the first conditional for sex-related thinktrees.
	/// </summary>
	public class ThinkNode_ConditionalSexChecks : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn p)
		{
			//Log.Message("[RJW]ThinkNode_ConditionalSexChecks " + xxx.get_pawnname(p));
			//if (p.Faction != null && p.Faction.IsPlayer)
			//	Log.Message("[RJW]ThinkNode_ConditionalSexChecks " + xxx.get_pawnname(p) + " is animal: " + xxx.is_animal(p));

			// Downed, Drafted and Awake are checked in core ThinkNode_ConditionalCanDoConstantThinkTreeJobNow.
			if (p.Map == null)
				return false;
				
			// Setting checks.
			if (xxx.is_human(p) && p.ageTracker.AgeBiologicalYears < RJWSettings.sex_minimum_age)
				return false;
			else if (xxx.is_animal(p) && !RJWSettings.bestiality_enabled && !RJWSettings.animal_on_animal_enabled)
				return false;

			// No sex while starving or badly hurt.
			return ((!p.needs?.food?.Starving) ?? true && xxx.is_healthy_enough(p));
		}
	}
}

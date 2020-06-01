using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace rjw
{
	public class CondomUtility
	{
		public static readonly RecordDef CountOfCondomsUsed = DefDatabase<RecordDef>.GetNamed("CountOfCondomsUsed");

		// TryUseCondom returns whether it was able to remove one condom from the pawn
		public static bool TryUseCondom(Pawn pawn)
		{
			if (!xxx.is_human(pawn)) return false;
			if (xxx.has_quirk(pawn, "ImpregnationFetish")) return false;
			List<Thing> pawn_condoms = pawn.inventory.innerContainer.ToList().FindAll(obj => obj.def == ThingDef.Named("Condom"));
			if (pawn_condoms.Any())
			{
				var stack = pawn_condoms.Pop();
				stack.stackCount--;
				if (stack.stackCount <= 0)
				{
					stack.Destroy();
				}
				return true;
			}
			return false;
		}

		// UseCondom applies the effects of having used a condom.
		public static void useCondom(Pawn pawn)
		{
			if (!xxx.is_human(pawn)) return;
			pawn.records.Increment(CountOfCondomsUsed);
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("SexWithCondom"));
		}

		public static void GetCondomFromRoom(Pawn pawn)
		{
			if (!xxx.is_human(pawn)) return;
			if (xxx.has_quirk(pawn, "ImpregnationFetish")) return;
			List<Thing> condoms_in_room = pawn.GetRoom().ContainedAndAdjacentThings.FindAll(obj => obj.def == ThingDef.Named("Condom") && pawn.Position.DistanceToSquared(obj.Position) < 10);
			//List<Thing> condoms_in_room = pawn.ownership.OwnedRoom?.ContainedAndAdjacentThings.FindAll(obj => obj.def == ThingDef.Named("Condom"));
			if (condoms_in_room.Any())
			{
				pawn.inventory.innerContainer.TryAdd(condoms_in_room.Pop().SplitOff(1));
			}
		}
	}
}

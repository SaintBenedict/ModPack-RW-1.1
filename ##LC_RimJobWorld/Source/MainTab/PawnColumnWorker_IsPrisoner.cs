using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace rjw.MainTab
{
	[StaticConstructorOnStartup]
	public class PawnColumnWorker_IsPrisoner : PawnColumnWorker_Icon
	{
		private static readonly Texture2D comfortOn = ContentFinder<Texture2D>.Get("UI/Tab/ComfortPrisoner_on");
		private readonly Texture2D comfortOff = ContentFinder<Texture2D>.Get("UI/Tab/ComfortPrisoner_off");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.IsPrisonerOfColony || xxx.is_slave(pawn) ? comfortOn : null;
		}
	}
}
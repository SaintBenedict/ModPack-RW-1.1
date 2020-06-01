using System.Collections.Generic;
using System.Linq;
using System;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace rjw.MainTab
{
	public class MainTabWindow_Brothel : MainTabWindow_PawnTable
	{
		private static PawnTableDef pawnTableDef;

		protected override PawnTableDef PawnTableDef => pawnTableDef ?? (pawnTableDef = DefDatabase<PawnTableDef>.GetNamed("Brothel"));

		protected override IEnumerable<Pawn> Pawns => Find.CurrentMap.mapPawns.AllPawns.Where(p => xxx.is_human(p) && (p.IsColonist || p.IsPrisonerOfColony));

		public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
	}
}

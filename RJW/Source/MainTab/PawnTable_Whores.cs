using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace rjw.MainTab
{
	public class PawnTable_Whores : PawnTable
	{
		public PawnTable_Whores(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight) { }

		protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
		{
			//return input.OrderBy(p => p.Name?.Numerical != false).ThenBy(p => (p.Name as NameSingle)?.Number ?? 0).ThenBy(p => p.def.label);
			return input.OrderBy(p => xxx.get_pawnname(p));
		}

		protected override IEnumerable<Pawn> PrimarySortFunction(IEnumerable<Pawn> input)
		{
			///return input.OrderByDescending(p => p.Faction?.Name);
			//return input.OrderBy(p => xxx.get_pawnname(p));
			return input.OrderByDescending(p => p.IsColonist);
		}
	}
}
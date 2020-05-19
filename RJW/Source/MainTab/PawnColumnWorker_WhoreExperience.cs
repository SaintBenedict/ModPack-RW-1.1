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
	public class PawnColumnWorker_WhoreExperience : PawnColumnWorker_TextCenter
	{
		public static readonly RecordDef CountOfWhore = DefDatabase<RecordDef>.GetNamed("CountOfWhore");
		
		public static readonly HashSet<string> backstories = new HashSet<string>(DefDatabase<StringListDef>.GetNamed("WhoreBackstories").strings);

		protected override string GetTextFor(Pawn pawn)
		{

			int b = backstories.Contains(pawn.story?.adulthood?.titleShort) ? 30 : 0;
			int score = pawn.records.GetAsInt(CountOfWhore);
			return (score + b).ToString();
		}
	}
}
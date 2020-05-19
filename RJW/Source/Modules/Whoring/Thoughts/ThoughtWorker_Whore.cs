using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace rjw
{
	/// <summary>
	/// Extends the standard thought to add a counter for the whore stages
	/// </summary>
	class ThoughtDef_Whore : ThoughtDef
	{
		public List<int> stageCounts = new List<int>();
		public int storyOffset = 0;
	}

	class ThoughtWorker_Whore : Thought_Memory
	{
		public static readonly HashSet<string> backstories = new HashSet<string>(DefDatabase<StringListDef>.GetNamed("WhoreBackstories").strings);
		protected readonly RecordDef whore_count = DefDatabase<RecordDef>.GetNamed("CountOfWhore");
		protected List<int> stages { get {return ((ThoughtDef_Whore)def).stageCounts; } } 
		protected int storyOffset { get { return ((ThoughtDef_Whore)def).storyOffset; } }
		//protected virtual readonly List<int> stages = new List<int>() { 10, 40};
		//protected virtual readonly int story_offset = 10;

		public override int CurStageIndex
		{
			get
			{
				//Log.Message("Static fields are not null " + !(backstories is null) + !(whore_count is null));
				var c = pawn.records.GetAsInt(whore_count);
				//Log.Message("Whore count of " + pawn + " is " + c);
				var b = backstories.Contains(pawn.story?.adulthood?.titleShort) ? storyOffset : 0;
				//Log.Message("Backstory offset " + b);
				var score = c + b;
				if (score > stages[stages.Count-1])
				{
					return stages.Count - 1;
				}
				//Log.Message("Starting search");
				var stage = stages.FindLastIndex(v => score > v)+1;
				//Log.Message("Search done, stage is " + stage);
				return stage;
			}
		}
	}
}

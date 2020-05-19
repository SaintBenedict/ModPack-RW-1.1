using RimWorld;
using UnityEngine;
using Verse;

namespace rjw
{
	public class IncidentWorker_TestInc2 : IncidentWorker
	{
		// Testing the mechanism of some build-in functions
		public static void test_funcion()
		{
			float a = Mathf.InverseLerp(0, 2, 3); //gives 1
			float b = Mathf.InverseLerp(0.2f, 2, 0.1f); //gives 0
			float c = Mathf.InverseLerp(2f, 1, 2.5f); //gives 0
													  //--Log.Message("[RJW]TestInc2::test_function is called - value a is " + a);
													  //--Log.Message("[RJW]TestInc2::test_function is called - value b is " + b);
													  //--Log.Message("[RJW]TestInc2::test_function is called - value c is " + c);
		}

		// Gives the wanted information of the selected thing
		public static void info_on_select(Map m)
		{
			Pawn p = Find.Selector.SingleSelectedThing as Pawn;
			if (p != null)
			{
				//--Log.Message("[RJW]TestInc2::info_on_select is called");
				foreach (var q in m.mapPawns.AllPawns)
				{
					SexAppraiser.would_fuck(p, q, true);
				}
			}
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			var m = (Map)parms.target;

			info_on_select(m);

			return true;
		}
	}
}
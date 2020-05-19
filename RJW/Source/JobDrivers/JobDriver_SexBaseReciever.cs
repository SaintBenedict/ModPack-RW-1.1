using System.Collections.Generic;
using Verse;

namespace rjw
{
	public class JobDriver_SexBaseReciever : JobDriver_Sex
	{
		//give this poor driver some love other than (Partner.jobs?.curDriver is JobDriver_SexBaseReciever)
		public List<Pawn> parteners = new List<Pawn>();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref parteners, "parteners", LookMode.Reference);
		}
	}
}
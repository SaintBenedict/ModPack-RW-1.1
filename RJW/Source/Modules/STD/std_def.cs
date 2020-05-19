using System.Collections.Generic;
using Verse;

namespace rjw
{
	/// <summary>
	/// Defines a disease that has a chance to spread during sex.
	/// </summary>
	public class std_def : Def
	{
		public HediffDef hediff_def;
		public HediffDef cohediff_def = null;
		public float catch_chance;
		public float environment_pitch_chance = 0.0f;
		public float spawn_chance = 0.0f;
		public float spawn_severity = 0.0f;
		public float autocure_below_severity = -1.0f;
		public List<BodyPartDef> appliedOnFixedBodyParts = null;
	}
}

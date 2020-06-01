using Verse;
using UnityEngine;
using Multiplayer.API;

namespace rjw
{
	[StaticConstructorOnStartup]
	public static class BukkakeContent
	{
		//UI:
		public static readonly Texture2D SemenIcon_little = ContentFinder<Texture2D>.Get("Bukkake/SemenIcon_little", true);
		public static readonly Texture2D SemenIcon_some = ContentFinder<Texture2D>.Get("Bukkake/SemenIcon_some", true);
		public static readonly Texture2D SemenIcon_dripping = ContentFinder<Texture2D>.Get("Bukkake/SemenIcon_dripping", true);
		public static readonly Texture2D SemenIcon_drenched = ContentFinder<Texture2D>.Get("Bukkake/SemenIcon_drenched", true);

		//on pawn:
		public static readonly Material semenSplatch1 = MaterialPool.MatFrom("Bukkake/splatch_1", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch2 = MaterialPool.MatFrom("Bukkake/splatch_2", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch3 = MaterialPool.MatFrom("Bukkake/splatch_3", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch4 = MaterialPool.MatFrom("Bukkake/splatch_4", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch5 = MaterialPool.MatFrom("Bukkake/splatch_5", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch6 = MaterialPool.MatFrom("Bukkake/splatch_6", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch7 = MaterialPool.MatFrom("Bukkake/splatch_7", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch8 = MaterialPool.MatFrom("Bukkake/splatch_8", ShaderDatabase.Cutout);
		public static readonly Material semenSplatch9 = MaterialPool.MatFrom("Bukkake/splatch_9", ShaderDatabase.Cutout);


		[SyncMethod]
		public static Material pickRandomSplatch()
		{
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			int rand = Rand.Range(0, 8);
			switch (rand)
			{
				case 0:
					return semenSplatch1;
				case 1:
					return semenSplatch2;
				case 2:
					return semenSplatch3;
				case 3:
					return semenSplatch4;
				case 4:
					return semenSplatch5;
				case 5:
					return semenSplatch6;
				case 6:
					return semenSplatch7;
				case 7:
					return semenSplatch8;
				case 8:
					return semenSplatch9;
			}
			return null;
		}
	}
}

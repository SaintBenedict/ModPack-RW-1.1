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
	public class PawnColumnWorker_IsWhore : PawnColumnCheckbox_Whore
	{
		protected override bool GetDisabled(Pawn pawn)
		{
			return !pawn.CanDesignateService();
		}

		protected override bool GetValue(Pawn pawn)
		{
			return pawn.IsDesignatedService() && xxx.is_human(pawn);
		}

		protected override void SetValue(Pawn pawn, bool value)
		{
			if (value == this.GetValue(pawn)) return;
			
			pawn.ToggleService();
		}
		/*
		private static readonly Texture2D serviceOn = ContentFinder<Texture2D>.Get("UI/Tab/Service_on");
		private static readonly Texture2D serviceOff = ContentFinder<Texture2D>.Get("UI/Tab/Service_off");

		protected override Texture2D GetIconFor(Pawn pawn)
		{
			return pawn.IsDesignatedService() ? serviceOn : null;
		}*/
	}
}

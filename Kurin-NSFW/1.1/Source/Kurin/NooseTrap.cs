using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace NineTailedFox
{
	public class NooseTrap : Building_Trap
	{
		protected override void SpringSub(Pawn p)
		{
			bool flag = p == null;
			if (!flag)
			{
				SoundStarter.PlayOneShot(SoundDefOf.TrapSpring, new TargetInfo(base.Position, base.Map, false));
				bool flag2 = p.BodySize <= 1f && !p.def.race.IsMechanoid;
				if (flag2)
				{
					Hediff hediff = HediffMaker.MakeHediff(Kurin_DefOf.Kurin_NooseTrapHediff, p, null);
					hediff.Severity = 1f;
					p.health.AddHediff(hediff, null, null, null);
				}
			}
		}
	}
}
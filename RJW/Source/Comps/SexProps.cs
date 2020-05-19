using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace rjw
{
	public class SexProps
	{
		public Pawn Pawn { get; }
		public Pawn Partner { get; }
		public xxx.rjwSextype SexType { get; }
		public bool Violent { get; }

		public bool HasPartner => Partner != null;

		public SexProps(Pawn pawn, Pawn partner, xxx.rjwSextype sexType, bool violent)
		{
			Pawn = pawn;
			Partner = partner;
			SexType = sexType;
			Violent = violent;
		}
	}
}

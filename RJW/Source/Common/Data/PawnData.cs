using System;
using Verse;
using System.Linq;

namespace rjw
{
	/// <summary>
	/// Utility data object and a collection of extension methods for Pawn
	/// </summary>
	public class PawnData : IExposable
	{
		public Pawn Pawn = null;
		public bool Comfort = false;
		public bool Service = false;
		public bool Breeding = false;
		public bool Milking = false;
		public bool Hero = false;
		public bool Ironman = false;
		public string HeroOwner = "";
		public bool BreedingAnimal = false;
		public bool CanChangeDesignationColonist = false;
		public bool CanChangeDesignationPrisoner = false;
		public bool CanDesignateService = false;
		public bool CanDesignateMilking = false;
		public bool CanDesignateComfort = false;
		public bool CanDesignateBreedingAnimal = false;
		public bool CanDesignateBreeding = false;
		public bool CanDesignateHero = false;

		public bool isSlime = false;
		public bool isDemon = false;
		public bool oviPregnancy = false;

		public PawnData() { }

		public PawnData(Pawn pawn)
		{
			//Log.Message("Creating pawndata for " + pawn);
			Pawn = pawn;
			//Log.Message("This data is valid " + this.IsValid);

			if (RaceGroupDef_Helper.TryGetRaceGroupDef(Pawn, out var raceGroupDef))
			{
				oviPregnancy = raceGroupDef.oviPregnancy;
			}

			isDemon = Pawn.Has(RaceTag.Demon);
			isSlime = Pawn.Has(RaceTag.Slime);

			//Log.Warning("PawnData:: Pawn:" + xxx.get_pawnname(pawn));
			//Log.Warning("PawnData:: isSlime:" + isSlime);
			//Log.Warning("PawnData:: isDemon:" + isDemon);
			//Log.Warning("PawnData:: oviPregnancy:" + oviPregnancy);
		}

		public void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.Pawn, "Pawn");
			Scribe_Values.Look<bool>(ref Comfort, "Comfort", false, true);
			Scribe_Values.Look<bool>(ref Service, "Service", false, true);
			Scribe_Values.Look<bool>(ref Breeding, "Breeding", false, true);
			Scribe_Values.Look<bool>(ref Milking, "Milking", false, true);
			Scribe_Values.Look<bool>(ref Hero, "Hero", false, true);
			Scribe_Values.Look<bool>(ref Ironman, "Ironman", false, true);
			Scribe_Values.Look<String>(ref HeroOwner, "HeroOwner", "", true);
			Scribe_Values.Look<bool>(ref BreedingAnimal, "BreedingAnimal", false, true);
			Scribe_Values.Look<bool>(ref CanChangeDesignationColonist, "CanChangeDesignationColonist", false, true);
			Scribe_Values.Look<bool>(ref CanChangeDesignationPrisoner, "CanChangeDesignationPrisoner", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateService, "CanDesignateService", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateMilking, "CanDesignateMilking", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateComfort, "CanDesignateComfort", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateBreedingAnimal, "CanDesignateBreedingAnimal", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateBreeding, "CanDesignateBreeding", false, true);
			Scribe_Values.Look<bool>(ref CanDesignateHero, "CanDesignateHero", false, true);
			Scribe_Values.Look<bool>(ref isSlime, "isSlime", false, true);
			Scribe_Values.Look<bool>(ref isDemon, "isDemon", false, true);
			Scribe_Values.Look<bool>(ref oviPregnancy, "oviPregnancy", false, true);
	}

	public bool IsValid { get { return Pawn != null; } }
	}
}

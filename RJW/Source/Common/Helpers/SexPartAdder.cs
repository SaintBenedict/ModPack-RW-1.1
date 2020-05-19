using Multiplayer.API;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace rjw
{
	class SexPartAdder
	{
		/// <summary>
		/// return true if going to set penis,
		/// return false for vagina
		/// </summary>
		public static bool IsAddingPenis(Pawn pawn, Gender gender)
		{
			return (pawn.gender == Gender.Male) ? (gender != Gender.Female) : (gender == Gender.Male);
		}

		/// <summary>
		/// generate part hediff
		/// </summary>
		public static Hediff MakePart(HediffDef def, Pawn pawn, BodyPartRecord bpr)
		{
			//Log.Message("SexPartAdder::PartMaker ( " + xxx.get_pawnname(pawn) + " ) " + def.defName);
			Hediff hd = HediffMaker.MakeHediff(def, pawn, bpr);
			//Log.Message("SexPartAdder::PartMaker ( " + xxx.get_pawnname(pawn) + " ) " + hd.def.defName);
			CompHediffBodyPart compHediff = hd.TryGetComp<CompHediffBodyPart>();
			if (compHediff != null)
			{
				//Log.Message("SexPartAdder::PartMaker init comps");
				compHediff.initComp(pawn);
				compHediff.updatesize();
			}
			return hd;
		}

		/// <summary>
		/// operation - move part data from thing to hediff
		/// </summary>
		public static Hediff recipePartAdder(RecipeDef recipe, Pawn pawn, BodyPartRecord part, List<Thing> ingredients)
		{
			Hediff hd = HediffMaker.MakeHediff(recipe.addsHediff, pawn, part);
			Thing thing = ingredients.Find(x => x.def.defName == recipe.addsHediff.defName);

			CompThingBodyPart CompThing = thing.TryGetComp<rjw.CompThingBodyPart>();
			CompHediffBodyPart CompHediff = hd.TryGetComp<rjw.CompHediffBodyPart>();

			if (CompHediff != null && CompThing != null)
				if (CompThing.SizeBase > 0)
				{
					CompHediff.FluidType = CompThing.FluidType;
					CompHediff.FluidAmmount = CompThing.FluidAmmount;
					CompHediff.FluidModifier = CompThing.FluidModifier;
					CompHediff.SizeBase = CompThing.SizeBase;
					CompHediff.SizeOwner = CompThing.SizeOwner;
					CompHediff.RaceOwner = CompThing.RaceOwner;
					CompHediff.PreviousOwner = CompThing.PreviousOwner;
					CompHediff.Eggs = CompThing.Eggs;
					CompHediff.updatesize();
				}
				else  //not initialised things, drop pods, trader?  //TODO: figure out how to populate rjw parts at gen
					hd = MakePart(hd.def, pawn, part);

			return hd;
		}

		/// <summary>
		/// operation - move part data from hediff to thing
		/// </summary>
		public static Thing recipePartRemover(Hediff hd)
		{
			Thing thing = ThingMaker.MakeThing(hd.def.spawnThingOnRemoved);

			CompThingBodyPart CompThing = thing.TryGetComp<rjw.CompThingBodyPart>();
			CompHediffBodyPart CompHediff = hd.TryGetComp<rjw.CompHediffBodyPart>();

			if (CompThing != null && CompHediff != null)
			{
					CompThing.FluidType = CompHediff.FluidType;
					CompThing.FluidAmmount = CompHediff.FluidAmmount;
					CompThing.FluidModifier = CompHediff.FluidModifier;
					CompThing.Size = CompHediff.Size;
					CompThing.SizeBase = CompHediff.SizeBase;
					CompThing.SizeOwner = CompHediff.SizeOwner;
					CompThing.RaceOwner = CompHediff.RaceOwner;
					CompThing.PreviousOwner = xxx.get_pawnname(CompHediff.Pawn);
					CompThing.Eggs = CompHediff.Eggs;
			}
			return thing;
		}

		[SyncMethod]
		public static void add_genitals(Pawn pawn, Pawn parent = null, Gender gender = Gender.None)
		{
			//--Log.Message("Genital_Helper::add_genitals( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord partBPR = Genital_Helper.get_genitalsBPR(pawn);
			var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

			//--Log.Message("Genital_Helper::add_genitals( " + xxx.get_pawnname(pawn) + " ) - checking genitals");
			if (partBPR == null)
			{
				//--Log.Message("[RJW] add_genitals( " + xxx.get_pawnname(pawn) + " ) doesn't have a genitals");
				return;
			}
			else if (pawn.health.hediffSet.PartIsMissing(partBPR))
			{
				//--Log.Message("[RJW] add_genitals( " + xxx.get_pawnname(pawn) + " ) had a genital but was removed.");
				return;
			}
			if (Genital_Helper.has_genitals(pawn, parts) && gender == Gender.None)//allow to add gender specific genitals(futa)
			{
				//--Log.Message("[RJW] add_genitals( " + xxx.get_pawnname(pawn) + " ) already has genitals");
				return;
			}

			HediffDef part;

			// maybe add some check based on bodysize of pawn for genitals size limit
			//Log.Message("Genital_Helper::add_genitals( " + pawn.RaceProps.baseBodySize + " ) - 1");
			//Log.Message("Genital_Helper::add_genitals( " + pawn.kindDef.race.size. + " ) - 2");

			part = (IsAddingPenis(pawn, gender)) ? Genital_Helper.generic_penis : Genital_Helper.generic_vagina;

			if (Genital_Helper.has_vagina(pawn, parts) && part == Genital_Helper.generic_vagina)
			{
				//--Log.Message("[RJW] add_genitals( " + xxx.get_pawnname(pawn) + " ) already has vagina");
				return;
			}
			if ((Genital_Helper.has_penis_fertile(pawn, parts) || Genital_Helper.has_penis_infertile(pawn, parts)) && part == Genital_Helper.generic_penis)
			{
				//--Log.Message("[RJW] add_genitals( " + xxx.get_pawnname(pawn) + " ) already has penis");
				return;
			}

			//override race genitals
			if (part == Genital_Helper.generic_vagina && pawn.TryAddRacePart(SexPartType.FemaleGenital))
			{
				return;
			}
			if (part == Genital_Helper.generic_penis && pawn.TryAddRacePart(SexPartType.MaleGenital))
			{
				return;
			}
			LegacySexPartAdder.AddGenitals(pawn, parent, gender, partBPR, part);
		}

		public static void add_breasts(Pawn pawn, Pawn parent = null, Gender gender = Gender.None)
		{
			//--Log.Message("[RJW] add_breasts( " + xxx.get_pawnname(pawn) + " ) called");
			BodyPartRecord partBPR = Genital_Helper.get_breastsBPR(pawn);

			if (partBPR == null)
			{
				//--Log.Message("[RJW] add_breasts( " + xxx.get_pawnname(pawn) + " ) - pawn doesn't have a breasts");
				return;
			}
			else if (pawn.health.hediffSet.PartIsMissing(partBPR))
			{
				//--Log.Message("[RJW] add_breasts( " + xxx.get_pawnname(pawn) + " ) had breasts but were removed.");
				return;
			}
			if (Genital_Helper.has_breasts(pawn))
			{
				//--Log.Message("[RJW] add_breasts( " + xxx.get_pawnname(pawn) + " ) - pawn already has breasts");
				return;
			}

			//TODO: figure out how to add (flat) breasts to males
			var racePartType = (pawn.gender == Gender.Female || gender == Gender.Female)
				? SexPartType.FemaleBreast
				: SexPartType.MaleBreast;
			if (pawn.TryAddRacePart(racePartType))
			{
				return;
			}

			LegacySexPartAdder.AddBreasts(pawn, partBPR, parent);
		}

		public static void add_anus(Pawn pawn, Pawn parent = null)
		{
			BodyPartRecord partBPR = Genital_Helper.get_anusBPR(pawn);

			if (partBPR == null)
			{
				//--Log.Message("[RJW] add_anus( " + xxx.get_pawnname(pawn) + " ) doesn't have an anus");
				return;
			}
			else if (pawn.health.hediffSet.PartIsMissing(partBPR))
			{
				//--Log.Message("[RJW] add_anus( " + xxx.get_pawnname(pawn) + " ) had an anus but was removed.");
				return;
			}
			if (Genital_Helper.has_anus(pawn))
			{
				//--Log.Message("[RJW] add_anus( " + xxx.get_pawnname(pawn) + " ) already has an anus");
				return;
			}

			if (pawn.TryAddRacePart(SexPartType.Anus))
			{
				return;
			}

			LegacySexPartAdder.AddAnus(pawn, partBPR, parent);
		}
	}
}

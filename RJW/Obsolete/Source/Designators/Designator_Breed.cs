//using System;
//using RimWorld;
//using UnityEngine;
//using Verse;

//namespace rjw
//{
//	//I have no idea what this shit means, I'm just reusing the comfort designator
//	public class Designator_Breed : Designator_ComfortPrisoner
//	{
//		//private static readonly MiscTranslationDef MTdef = DefDatabase<MiscTranslationDef>.GetNamed("ForBreeding");
//		public Designator_Breed()
//		{
//			//Log.Message("RJW breed designator constructor" );
//			//Log.Message("object is null " + (this ==null));
//			//Log.Message("RJW breed designator constructor" + MTdef);
//			defaultLabel = "ForBreeding".Translate();
//			defaultDesc = "ForBreedingDesc".Translate();
//			//Log.Message("RJW breed designator constructor pre icon");
//			icon = ContentFinder<Texture2D>.Get("UI/Commands/BreedingBuisiness", true);
//			// TODO: Can this be null?
//			hotKey = KeyBindingDefOf.Misc12;
//			//Log.Message("RJW breed designator constructor exit");
//		}

//		public override AcceptanceReport CanDesignateThing(Thing t)
//		{
//			if (t == null) { return false; };
//			var pawn = t as Pawn;
//			if (pawn == null) { return false; }//fuck this 
//			var enabled = Mod_Settings.cross_species_breeding;
//			var marked = BreederHelper.is_designated(pawn);
//			return (enabled && !pawn.Dead && pawn.IsPrisonerOfColony && !marked && xxx.can_get_raped(pawn));
//		}
//		public override void DesignateThing(Thing t)
//		{
//			DesignationDef designation_def = BreederHelper.designationDef;
			
//			base.Map.designationManager.AddDesignation(new Designation(t, designation_def));
//		}
//	}
//}
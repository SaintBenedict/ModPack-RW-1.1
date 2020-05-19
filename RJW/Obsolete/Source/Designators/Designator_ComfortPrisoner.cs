////This code provided the "comfort prisoner" button functionality for very long time
////Now it lays here in well-deserved rest. 
////It was a good designator, its code may still be used as an example for something else
//using System;
//using RimWorld;
//using UnityEngine;
//using Verse;

//namespace rjw
//{
//	public abstract class Designator_Toggle : Command
//	{
//		public Designator_Toggle()
//		{
//			this.activateSound = SoundDefOf.SelectDesignator;
//		}

//		public Func<bool> isActive;

//		public Action toggleAction;

//		public SoundDef turnOnSound = SoundDefOf.CheckboxTurnedOn;

//		public SoundDef turnOffSound = SoundDefOf.CheckboxTurnedOff;

//		public override SoundDef CurActivateSound
//		{
//			get
//			{
//				if (this.isActive())
//				{
//					return this.turnOffSound;
//				}
//				return this.turnOnSound;
//			}
//		}

//		public override void ProcessInput(Event ev)
//		{
//			base.ProcessInput(ev);
//			this.toggleAction();
//		}

//		public override GizmoResult GizmoOnGUI(Vector2 loc)
//		{
//			GizmoResult result = base.GizmoOnGUI(loc);
//			Rect rect = new Rect(loc.x, loc.y, this.Width, 75f);
//			Rect position = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
//			Texture2D image = (!this.isActive()) ? Widgets.CheckboxOffTex : Widgets.CheckboxOnTex;
//			GUI.DrawTexture(position, image);
//			return result;
//		}

//		public override bool InheritInteractionsFrom(Gizmo other)
//		{
//			Command_Toggle command_Toggle = other as Command_Toggle;
//			return command_Toggle != null && command_Toggle.isActive() == this.isActive();
//		}
//	}

//	public class Designator_ComfortPrisoner : Designator
//	{
//		private static readonly MiscTranslationDef MTdef = DefDatabase<MiscTranslationDef>.GetNamedSilentFail("DesignatorComfortPrisoner");

//		public Designator_ComfortPrisoner()
//		{
//			defaultLabel = MTdef.label;
//			defaultDesc = MTdef.description;

//			icon = ContentFinder<Texture2D>.Get("UI/Commands/comfort_prisoner_true");

//			// TODO: Can this be null?
//			hotKey = KeyBindingDefOf.Misc12;

//			// These don't matter but set them just in case
//			// soundDragSustain = SoundDefOf.DesignateDragStandard;
//			// soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
//			// useMouseIcon = false;
//			// soundSucceeded = SoundDefOf.DesignateClaim;
//		}

//		public Func<bool> isActive;

//		public override GizmoResult GizmoOnGUI(Vector2 loc)
//		{
//			GizmoResult result = base.GizmoOnGUI(loc);
//			Rect rect = new Rect(loc.x, loc.y, this.Width, 75f);
//			Rect position = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
//			Texture2D image = (!this.isActive()) ? Widgets.CheckboxOffTex : Widgets.CheckboxOnTex;
//			GUI.DrawTexture(position, image);
//			return result;
//		}

//		public override AcceptanceReport CanDesignateCell(IntVec3 c)
//		{
//			return false;
//		}

//		public override void DesignateSingleCell(IntVec3 c)
//		{
//		}

//		public override AcceptanceReport CanDesignateThing(Thing t)
//		{
//			var p = t as Pawn;
//			//return (p != null) && (p.IsPrisonerOfColony || p.IsColonist || p.Faction == Faction.OfPlayer) && xxx.can_get_raped(p) && (!comfort_prisoners.is_designated(p));
//			if (p != null)
//			{
//				//Log.Message("[RJW]CandDesignateThing run on" + p);
//				if (p.IsPrisonerOfColony)
//				{
//					defaultLabel = "comfort prisoner"; //TODO: adds translationdef.
//				}
//				else if (xxx.is_animal(p))
//				{
//					defaultLabel = "comfort animal";
//				}
//				else
//				{
//					defaultLabel = "comfort colonist";
//				}
//				//Log.Message("[RJW]CandDesignateThing label is " + defaultLabel);

//			}
//			return ((p != null) && (!comfort_prisoners.is_designated(p)) && ((p.IsColonist || p.Faction == Faction.OfPlayer) || (xxx.is_animal(p)) || (p.IsPrisonerOfColony))); //TODO: Rape target button?
//		}

//		public override void DesignateThing(Thing t)
//		{
//			DesignationDef designation_def = comfort_prisoners.designation_def_no_sticky;
//			if (xxx.config.rape_me_sticky_enabled)
//			{
//				designation_def = comfort_prisoners.designation_def;
//				//comfort_prisoners.designation_def = comfort_prisoners.designation_def_no_sticky;
//			}

//			base.Map.designationManager.AddDesignation(new Designation(t, designation_def));
//		}
//	}
//}
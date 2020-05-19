using System.Collections.Generic;
using Verse;
using HarmonyLib;
using UnityEngine;
using System;
using RimWorld;
using Multiplayer.API;

namespace rjw
{
	[HarmonyPatch(typeof(RimWorld.PawnWoundDrawer))]
	[HarmonyPatch("RenderOverBody")]
	[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mesh), typeof(Quaternion), typeof(bool) })]
	class patch_semenOverlay
	{

		static void Postfix(RimWorld.PawnWoundDrawer __instance, Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, bool forPortrait)
		{
			Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();//get local variable

			//TODO add support for animals? unlikely as they has weird meshes
			//for now, only draw humans
			if (pawn.RaceProps.Humanlike && RJWSettings.cum_overlays)
			{
				//find bukkake hediff. if it exists, use its draw function
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				if (hediffs.Exists(x => x.def == RJW_SemenoOverlayHediffDefOf.Hediff_Bukkake))
				{
					Hediff_Bukkake h = hediffs.Find(x => x.def == RJW_SemenoOverlayHediffDefOf.Hediff_Bukkake) as Hediff_Bukkake;

					quat.ToAngleAxis(out float angle, out Vector3 axis);//angle changes when pawn is e.g. downed

					//adjustments if the pawn is sleeping in a bed:
					bool inBed = false;
					Building_Bed building_Bed = pawn.CurrentBed();
					if (building_Bed != null)
					{
						inBed = !building_Bed.def.building.bed_showSleeperBody;
						AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 15);
						Vector3 vector2 = pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
						vector2.y += 0.02734375f+0.01f;//just copied from rimworld code+0.01f
						drawLoc.y = vector2.y;
					}

					h.DrawSemen(drawLoc, quat, forPortrait, angle);
				}
			}

		}
	}

	//adds new gizmo for adding semen for testing
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("GetGizmos")]
	class Patch_AddGizmo
	{
		static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
		{

			List<Gizmo> NewList = new List<Gizmo>();

			// copy vanilla entries into the new list
			foreach (Gizmo entry in __result)
			{
				NewList.Add(entry);
			}

			if (Prefs.DevMode && RJWSettings.DevMode && !MP.IsInMultiplayer)
			{
				Command_Action addSemen = new Command_Action();
				addSemen.defaultDesc = "AddSemenHediff";
				addSemen.defaultLabel = "AddSemen";
				addSemen.action = delegate ()
				{
					Addsemen(__instance);
				};

				NewList.Add(addSemen);
			}

			IEnumerable<Gizmo> output = NewList;

			// make caller use the list
			__result = output;

		}

		[SyncMethod]
		static void Addsemen(Pawn pawn)
		{
			//Log.Message("add semen button is pressed for " + pawn);

			if (!pawn.Dead && pawn.records != null)
			{
				//get all acceptable body parts:
				IEnumerable<BodyPartRecord> filteredParts = SemenHelper.getAvailableBodyParts(pawn);

				//select random part:
				BodyPartRecord randomPart;
				//filteredParts.TryRandomElement<BodyPartRecord>(out randomPart);
				//for testing - choose either genitals or anus:
				//Rand.PopState();
				//Rand.PushState(RJW_Multiplayer.PredictableSeed());
				if (Rand.Value > 0.5f)
				{
					randomPart = pawn.RaceProps.body.AllParts.Find(x => x.def == xxx.anusDef);
				}
				else
				{
					randomPart = pawn.RaceProps.body.AllParts.Find(x => x.def == xxx.genitalsDef);
				}

				if (randomPart != null)
				{
					SemenHelper.cumOn(pawn, randomPart, 0.2f, null, SemenHelper.CUM_NORMAL);
				}
			};
		}
	}
}

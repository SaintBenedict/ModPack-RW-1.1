using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using Multiplayer.API;

namespace rjw
{
	[StaticConstructorOnStartup]
	public static class SemenHelper
	{
		/*
		contains many important functions of use to the other classes
		*/

		//amount of semen per sex act:

		public static readonly Dictionary<key, values> splatchAdjust;//saves x (horizontal) and z (vertical) adjustments of texture positiion for each unique combination of bodyType and bodyPart
		public static readonly Dictionary<key_layer, values_layer> layerAdjust;//saves y adjustments (drawing plane) for left/right appendages + bodyPart combinations to hide spunk if pawn looks in the wrong direction

		//structs are used to pack related variables together - used as keys for the dictionaries
		public struct key//allows to save all unique combinations of bodyType and bodyPart
		{
			public readonly BodyTypeDef bodyType;
			public readonly BodyPartDef bodyPart;
			public key(BodyTypeDef bodyType, BodyPartDef bodyPart)
			{
				this.bodyType = bodyType;
				this.bodyPart = bodyPart;
			}
		}

		//for the 4 directions, use arrays to store the different adjust for north, east, south, west (in that order)
		public struct values
		{
			public readonly float[] x;
			public readonly float[] z;
			//public readonly bool over_clothing;//on gentials: hide when clothes are worn - in case of the other body parts it can't be said (for now) if it was added on the clothing or not
			public values(float[] xAdjust, float[] zAdjust)
			{
				x = xAdjust;
				z = zAdjust;
				//this.over_clothing = over_clothing;
			}
		}


		public struct key_layer//used to save left/right appendage + bodyPart combinations
		{
			public readonly bool left_side;
			public readonly BodyPartDef bodyPart;

			public key_layer(bool left_side, BodyPartDef bodyPart)
			{
				this.left_side = left_side;
				this.bodyPart = bodyPart;
			}
		}

		public struct values_layer//saves the y-adjustments for different body parts and sides -> e.g. allows hiding spunk on the right arm if pawn is looking to the left (aka west)
		{
			public readonly float[] y;

			public values_layer(float[] yAdjust)
			{
				y = yAdjust;
			}
		}

		//get defs of the rjw parts
		public static BodyPartDef genitalsDef = BodyDefOf.Human.AllParts.Find(bpr => string.Equals(bpr.def.defName, "Genitals")).def;
		public static BodyPartDef anusDef = BodyDefOf.Human.AllParts.Find(bpr => string.Equals(bpr.def.defName, "Anus")).def;
		public static BodyPartDef chestDef = BodyDefOf.Human.AllParts.Find(bpr => string.Equals(bpr.def.defName, "Chest")).def;


		static SemenHelper()
		{
			splatchAdjust = new Dictionary<key, values>();
			//maybe there is a more elegant way to save and load this data, but I don't know about it

			//structure explained: 
			//1) key: struct consisting of bodyType + bodyPart (= unique key for every combination of bodyType + part)
			//2) values: struct containing positioning information (xAdjust: horizontal positioning, yAdjust: vertical positioning, zAdjust: whether to draw above or below pawn
			//note: arms, hands, and legs (which are only visible from one direction) values need not be inverted between west and east

			//BodyType Thin
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Arm), new values(new float[] { -0.13f, 0.05f, 0.13f, 0.05f }, new float[] { 0f, 0f, 0f, 0f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Hand), new values(new float[] { -0.12f, 0.15f, 0.12f, 0.15f }, new float[] { -0.25f, -0.25f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Head), new values(new float[] { 0f, -0.23f, 0f, 0.23f }, new float[] { 0.37f, 0.35f, 0.33f, 0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Jaw), new values(new float[] { 0f, -0.19f, 0f, 0.19f }, new float[] { 0.15f, 0.15f, 0.15f, 0.15f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Leg), new values(new float[] { -0.1f, 0.1f, 0.1f, 0.1f }, new float[] { -0.4f, -0.4f, -0.4f, -0.4f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Neck), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0.06f, 0.06f, 0.06f, 0.06f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, BodyPartDefOf.Torso), new values(new float[] { 0f, 0f, 0f, 0f }, new float[] { -0.18f, -0.20f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, genitalsDef), new values(new float[] { 0f, 0.01f, 0f, -0.01f }, new float[] { 0, -0.35f, -0.35f, -0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, anusDef), new values(new float[] { 0, 0.18f, 0, -0.18f }, new float[] { -0.42f, -0.35f, 0, -0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Thin, chestDef), new values(new float[] { 0f, -0.1f, 0f, 0.1f }, new float[] { -0.06f, -0.05f, -0.06f, -0.05f }));

			//BodyType Female
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Arm), new values(new float[] { -0.17f, 0f, 0.17f, 0f }, new float[] { 0f, 0f, 0f, 0f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Hand), new values(new float[] { -0.17f, 0.1f, 0.17f, 0.1f }, new float[] { -0.25f, -0.25f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Head), new values(new float[] { 0f, -0.23f, 0f, 0.23f }, new float[] { 0.37f, 0.35f, 0.33f, 0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Jaw), new values(new float[] { 0f, -0.19f, 0f, 0.19f }, new float[] { 0.15f, 0.15f, 0.15f, 0.15f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Leg), new values(new float[] { -0.2f, 0.1f, 0.2f, 0.1f }, new float[] { -0.4f, -0.4f, -0.4f, -0.4f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Neck), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0.06f, 0.06f, 0.06f, 0.06f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, BodyPartDefOf.Torso), new values(new float[] { 0f, -0.05f, 0f, 0.05f }, new float[] { -0.20f, -0.25f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, genitalsDef), new values(new float[] { 0f, -0.10f, 0f, 0.10f }, new float[] { 0, -0.42f, -0.45f, -0.42f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, anusDef), new values(new float[] { 0, 0.26f, 0, -0.26f }, new float[] { -0.42f, -0.35f, 0, -0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Female, chestDef), new values(new float[] { 0f, -0.12f, 0f, 0.12f }, new float[] { -0.06f, -0.05f, -0.06f, -0.05f }));

			//BodyType Male
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Arm), new values(new float[] { -0.21f, 0.05f, 0.21f, 0.05f }, new float[] { 0f, -0.02f, 0f, -0.02f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Hand), new values(new float[] { -0.17f, 0.07f, 0.17f, 0.07f }, new float[] { -0.25f, -0.25f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Head), new values(new float[] { 0f, -0.23f, 0f, 0.23f }, new float[] { 0.37f, 0.35f, 0.33f, 0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Jaw), new values(new float[] { 0f, -0.19f, 0f, 0.19f }, new float[] { 0.15f, 0.15f, 0.15f, 0.15f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Leg), new values(new float[] { -0.17f, 0.07f, 0.17f, 0.07f }, new float[] { -0.4f, -0.4f, -0.4f, -0.4f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Neck), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0.06f, 0.06f, 0.06f, 0.06f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, BodyPartDefOf.Torso), new values(new float[] { 0f, -0.05f, 0f, 0.05f }, new float[] { -0.20f, -0.25f, -0.25f, -0.25f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, genitalsDef), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0, -0.35f, -0.42f, -0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, anusDef), new values(new float[] { 0, 0.17f, 0, -0.17f }, new float[] { -0.42f, -0.35f, 0, -0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Male, chestDef), new values(new float[] { 0f, -0.16f, 0f, 0.16f }, new float[] { -0.06f, -0.05f, -0.06f, -0.05f }));

			//BodyType Hulk
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Arm), new values(new float[] { -0.3f, 0.05f, 0.3f, 0.05f }, new float[] { 0f, -0.02f, 0f, -0.02f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Hand), new values(new float[] { -0.22f, 0.07f, 0.22f, 0.07f }, new float[] { -0.28f, -0.28f, -0.28f, -0.28f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Head), new values(new float[] { 0f, -0.23f, 0f, 0.23f }, new float[] { 0.37f, 0.35f, 0.33f, 0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Jaw), new values(new float[] { 0f, -0.19f, 0f, 0.19f }, new float[] { 0.15f, 0.15f, 0.15f, 0.15f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Leg), new values(new float[] { -0.17f, 0.07f, 0.17f, 0.07f }, new float[] { -0.5f, -0.5f, -0.5f, -0.5f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Neck), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0.06f, 0.06f, 0.06f, 0.06f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, BodyPartDefOf.Torso), new values(new float[] { 0f, -0.05f, 0f, 0.05f }, new float[] { -0.20f, -0.3f, -0.3f, -0.3f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, genitalsDef), new values(new float[] { 0f, -0.02f, 0f, 0.02f }, new float[] { 0, -0.55f, -0.55f, -0.55f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, anusDef), new values(new float[] { 0, 0.35f, 0, -0.35f }, new float[] { -0.5f, -0.5f, 0, -0.5f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Hulk, chestDef), new values(new float[] { 0f, -0.22f, 0f, 0.22f }, new float[] { -0.06f, -0.05f, -0.06f, -0.05f }));

			//BodyType Fat
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Arm), new values(new float[] { -0.3f, 0.05f, 0.3f, 0.05f }, new float[] { 0f, -0.02f, 0f, -0.02f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Hand), new values(new float[] { -0.32f, 0.07f, 0.32f, 0.07f }, new float[] { -0.28f, -0.28f, -0.28f, -0.28f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Head), new values(new float[] { 0f, -0.23f, 0f, 0.23f }, new float[] { 0.37f, 0.35f, 0.33f, 0.35f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Jaw), new values(new float[] { 0f, -0.19f, 0f, 0.19f }, new float[] { 0.15f, 0.15f, 0.15f, 0.15f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Leg), new values(new float[] { -0.17f, 0.07f, 0.17f, 0.07f }, new float[] { -0.45f, -0.45f, -0.45f, -0.45f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Neck), new values(new float[] { 0f, -0.07f, 0f, 0.07f }, new float[] { 0.06f, 0.06f, 0.06f, 0.06f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, BodyPartDefOf.Torso), new values(new float[] { 0f, -0.15f, 0f, 0.15f }, new float[] { -0.20f, -0.3f, -0.3f, -0.3f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, genitalsDef), new values(new float[] { 0f, -0.25f, 0f, 0.25f }, new float[] { 0, -0.45f, -0.50f, -0.45f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, anusDef), new values(new float[] { 0, 0.35f, 0, -0.35f }, new float[] { -0.5f, -0.4f, 0, -0.4f }));
			splatchAdjust.Add(new key(BodyTypeDefOf.Fat, chestDef), new values(new float[] { 0f, -0.27f, 0f, 0.27f }, new float[] { -0.07f, -0.05f, -0.07f, -0.05f }));


			//now for the layer/plane adjustments:
			layerAdjust = new Dictionary<key_layer, values_layer>();

			//left body parts: 
			//in theory, all body parts not coming in pairs should have the bool as false -> be listed as right, so I wouldn't need to add them here, but it doesn't hurt to be safe
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Arm), new values_layer(new float[] { 0f, -99f, 0f, 0f }));//0.00 = drawn over body (=visible) if the pawn looks in any direction except west, in which case it's hidden (-99)
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Hand), new values_layer(new float[] { 0f, -99f, 0f, 0f }));
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Leg), new values_layer(new float[] { 0f, -99f, 0f, 0f }));

			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Head), new values_layer(new float[] { 0.02f, 0.02f, 0.02f, 0.02f }));//drawn from all directions, 0.02 = over hair
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Jaw), new values_layer(new float[] { -9f, 0.01f, 0.01f, 0.01f }));//0.01 = drawn over head but under hair, only hidden if facing north
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Neck), new values_layer(new float[] { 0f, 0f, 0f, 0f }));
			layerAdjust.Add(new key_layer(true, BodyPartDefOf.Torso), new values_layer(new float[] { 0f, 0f, 0f, 0f }));
			layerAdjust.Add(new key_layer(true, genitalsDef), new values_layer(new float[] { -99f, 0f, 0f, 0f }));//only hidden if facing north
			layerAdjust.Add(new key_layer(true, anusDef), new values_layer(new float[] { 0f, 0f, -99f, 0f }));
			layerAdjust.Add(new key_layer(true, chestDef), new values_layer(new float[] { -99f, 0f, 0f, 0f }));

			//right body parts:
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Arm), new values_layer(new float[] { 0f, 0f, 0f, -99f }));
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Hand), new values_layer(new float[] { 0f, 0f, 0f, -99f }));
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Leg), new values_layer(new float[] { 0f, 0f, 0f, -99f }));

			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Head), new values_layer(new float[] { 0.02f, 0.02f, 0.02f, 0.02f }));
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Jaw), new values_layer(new float[] { -99f, 0.01f, 0.01f, 0.01f }));
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Neck), new values_layer(new float[] { 0f, 0f, 0f, 0f }));
			layerAdjust.Add(new key_layer(false, BodyPartDefOf.Torso), new values_layer(new float[] { 0f, 0f, 0f, 0f }));
			layerAdjust.Add(new key_layer(false, genitalsDef), new values_layer(new float[] { -99f, 0f, 0f, 0f }));
			layerAdjust.Add(new key_layer(false, anusDef), new values_layer(new float[] { 0f, 0f, -99f, 0f }));
			layerAdjust.Add(new key_layer(false, chestDef), new values_layer(new float[] { -99f, 0f, 0f, 0f }));

		}

		//all body parts that semen can theoretically be applied to:
		public static List<BodyPartDef> getAllowedBodyParts()
		{
			List<BodyPartDef> allowedParts = new List<BodyPartDef>();

			allowedParts.Add(BodyPartDefOf.Arm);
			allowedParts.Add(BodyPartDefOf.Hand);
			allowedParts.Add(BodyPartDefOf.Leg);
			allowedParts.Add(BodyPartDefOf.Head);
			allowedParts.Add(BodyPartDefOf.Jaw);
			allowedParts.Add(BodyPartDefOf.Neck);
			allowedParts.Add(BodyPartDefOf.Torso);
			allowedParts.Add(genitalsDef);
			allowedParts.Add(anusDef);
			allowedParts.Add(chestDef);

			return allowedParts;
		}

		//get valid body parts for a specific pawn
		public static IEnumerable<BodyPartRecord> getAvailableBodyParts(Pawn pawn)
		{
			//get all non-missing body parts:
			IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside, null, null);
			BodyPartRecord anus = pawn.def.race.body.AllParts.Find(bpr => string.Equals(bpr.def.defName, "Anus"));//not found by above function since depth is "inside"

			if (anus != null)
			{
				bodyParts = bodyParts.AddItem(anus);
			}

			//filter for allowed body parts (e.g. no single fingers/toes):
			List<BodyPartDef> filterParts = SemenHelper.getAllowedBodyParts();

			IEnumerable<BodyPartRecord> filteredParts = bodyParts.Where(item1 => filterParts.Any(item2 => item2.Equals(item1.def)));
			return filteredParts;
		}


		public const int CUM_NORMAL = 0;
		public const int CUM_INSECT = 1;
		public const int CUM_MECHA = 2;

		public static readonly Color color_normal = new Color(0.95f, 0.95f, 0.95f);
		public static readonly Color color_insect = new Color(0.6f, 0.83f, 0.35f);//green-yellowish
		public static readonly Color color_mecha = new Color(0.37f, 0.71f, 0.82f);//cyan-ish

		//name should be self-explanatory:
		public static void cumOn(Pawn receiver, BodyPartRecord bodyPart, float amount = 0.2f, Pawn giver = null, int semenType = CUM_NORMAL)
		{
			Hediff_Semen hediff;
			if (semenType == CUM_NORMAL)
			{
				hediff = (Hediff_Semen)HediffMaker.MakeHediff(RJW_SemenoOverlayHediffDefOf.Hediff_Semen, receiver, null);
			}
			else if (semenType == CUM_INSECT)
			{
				hediff = (Hediff_Semen)HediffMaker.MakeHediff(RJW_SemenoOverlayHediffDefOf.Hediff_InsectSpunk, receiver, null);
			}
			else
			{
				hediff = (Hediff_Semen)HediffMaker.MakeHediff(RJW_SemenoOverlayHediffDefOf.Hediff_MechaFluids, receiver, null);
			}

			hediff.Severity = amount;//if this body part is already maximally full -> spill over to other parts

			//idea: here, a log entry that can act as source could be linked to the hediff - maybe reuse the playlog entry of rjw:
			hediff.semenType = semenType;

			try
			{
				//error when adding to missing part
				receiver.health.AddHediff(hediff, bodyPart, null, null);
			}
			catch
			{

			}

			//Log.Message(xxx.get_pawnname(receiver) + " cum ammount" + amount);
			//causes significant memory leak, fixx someday
			//if (amount > 1f)//spillover in case of very large amounts: just apply hediff a second time
			//{
			//	Hediff_Semen hediff2 = (Hediff_Semen)HediffMaker.MakeHediff(hediff.def, receiver, null);
			//	hediff2.semenType = semenType;
			//	hediff2.Severity = amount - 1f;
			//	receiver.health.AddHediff(hediff2, bodyPart, null, null);
			//}

			//always also add bukkake hediff as manager
			receiver.health.AddHediff(RJW_SemenoOverlayHediffDefOf.Hediff_Bukkake);
		}

		//if spunk on one body part reaches a certain level, it can spill over to others, this function returns from where to where
		[SyncMethod]
		public static BodyPartDef spillover(BodyPartDef sourcePart)
		{
			//caution: danger of infinite loop if circular spillover between 2 full parts -> don't define possible circles
			BodyPartDef newPart = null;
			int sel;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (sourcePart == BodyPartDefOf.Torso)
			{
				sel = Rand.Range(0, 4);
				if (sel == 0)
				{
					newPart = BodyPartDefOf.Arm;
				}
				else if (sel == 1)
				{
					newPart = BodyPartDefOf.Leg;
				}
				else if (sel == 2)
				{
					newPart = BodyPartDefOf.Neck;
				}
				else if (sel == 3)
				{
					newPart = chestDef;
				}
			}
			else if (sourcePart == BodyPartDefOf.Jaw)
			{
				sel = Rand.Range(0, 4);
				if (sel == 0)
				{
					newPart = BodyPartDefOf.Head;
				}
				else if (sel == 1)
				{
					newPart = BodyPartDefOf.Torso;
				}
				else if (sel == 2)
				{
					newPart = BodyPartDefOf.Neck;
				}
				else if (sel == 3)
				{
					newPart = chestDef;
				}
			}
			else if (sourcePart == genitalsDef)
			{
				sel = Rand.Range(0, 2);
				if (sel == 0)
				{
					newPart = BodyPartDefOf.Leg;
				}
				else if (sel == 1)
				{
					newPart = BodyPartDefOf.Torso;
				}
			}
			else if (sourcePart == anusDef)
			{
				sel = Rand.Range(0, 2);
				if (sel == 0)
				{
					newPart = BodyPartDefOf.Leg;
				}
				else if (sel == 1)
				{
					newPart = BodyPartDefOf.Torso;
				}
			}
			else if (sourcePart == chestDef)
			{
				sel = Rand.Range(0, 3);
				if (sel == 0)
				{
					newPart = BodyPartDefOf.Arm;
				}
				else if (sel == 1)
				{
					newPart = BodyPartDefOf.Torso;
				}
				else if (sel == 2)
				{
					newPart = BodyPartDefOf.Neck;
				}
			}
			return newPart;
		}

		//determines who is the active male (or equivalent) in the exchange and the amount of semen dispensed and where to
		[SyncMethod]
		public static void calculateAndApplySemen(Pawn pawn, Pawn partner, xxx.rjwSextype sextype)
		{
			if (!RJWSettings.cum_on_body) return;

			Pawn giver, receiver;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());

			List<Hediff> giverparts;
			var pawnparts = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn));
			var partnerparts = Genital_Helper.get_PartsHediffList(partner, Genital_Helper.get_genitalsBPR(partner));

			//dispenser of the seed
			if (Genital_Helper.has_penis_fertile(pawn, pawnparts) || xxx.is_mechanoid(pawn) || xxx.is_insect(pawn))
			{
				giver = pawn;
				giverparts = pawnparts;
				receiver = partner;
			}
			else if (partner != null && (Genital_Helper.has_penis_fertile(partner, partnerparts) || xxx.is_mechanoid(partner) || xxx.is_insect(partner)))
			{
				giver = partner;
				giverparts = partnerparts;
				receiver = pawn;
			}
			else//female on female or genderless - no semen dispensed; maybe add futa support?
			{
				return;
			}

			//slimes do not waste fluids?
			//if (xxx.is_slime(giver)) return;

			//determine entity:
			int entityType = SemenHelper.CUM_NORMAL;
			if (xxx.is_mechanoid(giver))
			{
				entityType = SemenHelper.CUM_MECHA;
			}
			else if (xxx.is_insect(giver))
			{
				entityType = SemenHelper.CUM_INSECT;
			}

			//get pawn genitalia:
			BodyPartRecord genitals;
			if (xxx.is_mechanoid(giver))
			{
				genitals = giver.RaceProps.body.AllParts.Find(x => string.Equals(x.def.defName, "MechGenitals"));
			}
			else//insects, animals, humans
			{
				genitals = giver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.genitalsDef);

			}
			//no cum without genitals
			if (genitals == null)
			{
				return;
			}

			float cumAmount = giver.BodySize; //fallback for mechanoinds and w/e without hediffs
			float horniness = 1f;
			float ageScale = Math.Min(80 / SexUtility.ScaleToHumanAge(giver), 1.0f);//calculation lifted from rjw

			if (xxx.is_mechanoid(giver) && giverparts.NullOrEmpty())
			{
				//use default above
			}
			else if (giverparts.NullOrEmpty())
				return;
			else
			{
				var penisHediff = giverparts.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("penis")).InRandomOrder().FirstOrDefault();

				if (penisHediff == null)
					penisHediff = giverparts.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorf")).InRandomOrder().FirstOrDefault();
				if (penisHediff == null)
					penisHediff = giverparts.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorm")).InRandomOrder().FirstOrDefault();
				if (penisHediff == null)
					penisHediff = giverparts.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("tentacle")).InRandomOrder().FirstOrDefault();

				if (penisHediff != null)
				{
					cumAmount = penisHediff.Severity * giver.BodySize;

					CompHediffBodyPart chdf = penisHediff.TryGetComp<rjw.CompHediffBodyPart>();
					if (chdf != null)
					{
						cumAmount = chdf.FluidAmmount * chdf.FluidModifier;
					}

					Need sexNeed = giver?.needs?.AllNeeds.Find(x => string.Equals(x.def.defName, "Sex"));
					if (sexNeed != null)//non-humans don't have it - therefore just use the default value
					{
						horniness = 1f - sexNeed.CurLevel;
					}
				}
				else 
				{ 
					//something is wrong... vagina?
					return; 
				}
			}

			cumAmount = cumAmount * horniness * ageScale * RJWSettings.cum_on_body_amount_adjust;
			cumAmount /= 100;

			//TODO: SemenHelper Autofellatio
			//if no partner -> masturbation, apply some cum on self:
			//if (partner == null && sextype == xxx.rjwSextype.Autofellatio)
			//{
			//	if (!xxx.is_slime(giver))
			//		SemenHelper.cumOn(giver, BodyPartDefOf.Jaw, cumAmount, giver);
			//	return;
			//}
			if (partner == null && sextype == xxx.rjwSextype.Masturbation)
			{
				if (!xxx.is_slime(giver))
					SemenHelper.cumOn(giver, genitals, cumAmount * 0.3f, giver);//pawns are usually not super-messy -> only apply 30%
				return;
			}
			else if (partner != null)
			{
				List<BodyPartRecord> targetParts = new List<BodyPartRecord>();//which to apply semen on
				IEnumerable<BodyPartRecord> availableParts = SemenHelper.getAvailableBodyParts(receiver);
				BodyPartRecord randomPart;//not always needed

				switch (sextype)
				{
					case rjw.xxx.rjwSextype.Anal:
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.anusDef));
						break;
					case rjw.xxx.rjwSextype.Boobjob:
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.chestDef));
						break;
					case rjw.xxx.rjwSextype.DoublePenetration:
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.anusDef));
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.genitalsDef));
						break;
					case rjw.xxx.rjwSextype.Fingering:
						cumAmount = 0;
						break;
					case rjw.xxx.rjwSextype.Fisting:
						cumAmount = 0;
						break;
					case rjw.xxx.rjwSextype.Footjob:
						//random part:
						availableParts.TryRandomElement<BodyPartRecord>(out randomPart);
						targetParts.Add(randomPart);
						break;
					case rjw.xxx.rjwSextype.Handjob:
						//random part:
						availableParts.TryRandomElement<BodyPartRecord>(out randomPart);
						targetParts.Add(randomPart);
						break;
					case rjw.xxx.rjwSextype.Masturbation:
						cumAmount *= 2f;
						break;
					case rjw.xxx.rjwSextype.MechImplant:
						//one of the openings:
						int random = Rand.Range(0, 3);
						if (random == 0)
						{
							targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.genitalsDef));
						}
						else if (random == 1)
						{
							targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.anusDef));
						}
						else if (random == 2)
						{
							targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Jaw));
						}
						break;
					case rjw.xxx.rjwSextype.MutualMasturbation:
						//random
						availableParts.TryRandomElement<BodyPartRecord>(out randomPart);
						targetParts.Add(randomPart);
						break;
					case rjw.xxx.rjwSextype.None:
						cumAmount = 0;
						break;
					case rjw.xxx.rjwSextype.Oral:
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Jaw));
						break;
					case rjw.xxx.rjwSextype.Scissoring:
						//I guess if it came to here, a male must be involved?
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.genitalsDef));
						break;
					case rjw.xxx.rjwSextype.Vaginal:
						targetParts.Add(receiver.RaceProps.body.AllParts.Find(x => x.def == SemenHelper.genitalsDef));
						break;
				}

				if (cumAmount > 0)
				{
					if (xxx.is_slime(receiver))
					{
						//slime absorb cum
						//this needs balancing, since cumamount ranges 0-10(?) which is fine for cum/hentai but not very realistic for feeding
						//using TransferNutrition for now
						//Log.Message("cumAmount " + cumAmount);
						//float nutrition_amount = cumAmount/10;

						Need_Food need = need = giver.needs.TryGetNeed<Need_Food>();
						if (need == null)
						{
							//Log.Message("xxx::TransferNutrition() " + xxx.get_pawnname(pawn) + " doesn't track nutrition in itself, probably shouldn't feed the others");
							return;
						}

						if (receiver?.needs?.TryGetNeed<Need_Food>() != null)
						{
							//Log.Message("xxx::TransferNutrition() " +  xxx.get_pawnname(partner) + " can receive");
							float nutrition_amount = Math.Min(need.MaxLevel / 15f, need.CurLevel); //body size is taken into account implicitly by need.MaxLevel
							receiver.needs.food.CurLevel += nutrition_amount;
						}
					}
					else
					{
						SemenHelper.cumOn(giver, genitals, cumAmount * 0.3f, giver, entityType);//cum on self - smaller amount
						foreach (BodyPartRecord bpr in targetParts)
						{
							if (bpr != null)
							{
								SemenHelper.cumOn(receiver, bpr, cumAmount, giver, entityType);//cum on partner
							}
						}
					}
				}
			}
		}

	}
}

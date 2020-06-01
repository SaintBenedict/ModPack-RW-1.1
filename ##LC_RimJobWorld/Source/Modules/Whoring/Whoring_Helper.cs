// #define TESTMODE // Uncomment to enable logging.

using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using System.Diagnostics;
using System;
using UnityEngine;
using Verse.AI.Group;
using Multiplayer.API;

namespace rjw
{
	/// <summary>
	/// Helper for whoring related stuff
	/// </summary>
	public class WhoringHelper
	{
		private static readonly SimpleCurve whoring_age_curve = new SimpleCurve
		{
			// life expectancy to price modifier
			new CurvePoint(0.15f,  0f), // 12 by human age, No whoring allowed for underage.
			new CurvePoint(0.22f,  1.5f), // 18
			new CurvePoint(0.3f, 1.4f), // 24
			new CurvePoint(0.4f, 1f), // 32
			new CurvePoint(0.6f, 0.75f), // 48
			new CurvePoint(1.0f, 0.5f), // 80
			new CurvePoint(5.0f, 0.5f), // Lifespan extended by bionics, misconfigurated alien races, etc. 
		};

		public static int WhoreMinPrice(Pawn whore)
		{
			float min_price = 20f;
			min_price *= whoring_age_curve.Evaluate(SexUtility.ScaleToHumanAge(whore));

			if (xxx.has_traits(whore))
			{
				if (xxx.IndividualityIsActive && whore.story.traits.HasTrait(xxx.SYR_Haggler))
					min_price *= 1.5f;
				if (whore.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == 2) // Industrious
					min_price *= 1.5f;
				if (whore.story.traits.DegreeOfTrait(TraitDefOf.NaturalMood) == -2) // Depressive
					min_price *= 0.5f;
				if (whore.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
					min_price *= 0.75f;

				//StatDef PawnBeauty
				if (whore.story.traits.HasTrait(TraitDefOf.Beauty))
				{
					switch (whore.story.traits.DegreeOfTrait(TraitDefOf.Beauty))
					{
						case 2:
							min_price *= 3f;
							break;
						case 1:
							min_price *= 1.5f;
							break;
						case -1:
							min_price *= 0.5f;
							break;
						case -2:
							min_price *= 0.2f;
							break;
					}
				}
				if (xxx.is_masochist(whore)) // Won't haggle, may settle for low price
					min_price *= 0.7f;
				if (xxx.is_nympho(whore)) // Same as above
					min_price *= 0.4f;
			}

			min_price *= WhoreInjuryAdjustment(whore);
			if (whore.ownership.OwnedRoom != null)
				min_price *= WhoreRoomAdjustment(whore);
			return (int)min_price;
		}

		public static int WhoreMaxPrice(Pawn whore)
		{
			float max_price = 40f;
			max_price *= whoring_age_curve.Evaluate(SexUtility.ScaleToHumanAge(whore));

			if (whore.gender == Gender.Female)
				max_price *= 1.2f;
			if (xxx.has_traits(whore))
			{
				if (whore.story.traits.HasTrait(TraitDefOf.Greedy))
					max_price *= 2f;
				if (xxx.IndividualityIsActive && whore.story.traits.HasTrait(xxx.SYR_Haggler))
					max_price *= 1.5f;
				if (whore.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == 2) // Industrious
					max_price *= 1.5f;
				if (whore.story.traits.DegreeOfTrait(TraitDefOf.Industriousness) == 1) // Hard Worker
					max_price *= 1.2f;
				if (whore.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
					max_price *= 0.75f;
				if (whore.story.traits.HasTrait(TraitDefOf.Beauty))
				{
					switch (whore.story.traits.DegreeOfTrait(TraitDefOf.Beauty))
					{
						case 2:
							max_price *= 3.5f;
							break;
						case 1:
							max_price *= 2f;
							break;
						case -1:
							max_price *= 0.8f;
							break;
						case -2:
							max_price *= 0.6f;
							break;
					}
				}
			}

			max_price *= WhoreInjuryAdjustment(whore);
			if (whore.ownership.OwnedRoom != null)
				max_price *= WhoreRoomAdjustment(whore);
			return (int)max_price;
		}

		public static float WhoreInjuryAdjustment(Pawn whore)
		{
			float modifier = 1.0f;
			int injuries = whore.health.hediffSet.hediffs.Count(x => x.Visible && x.def.everCurableByItem && x.IsPermanent());

			if (injuries == 0) return modifier;

			while (injuries > 0)
			{
				modifier *= 0.85f;
				injuries--;
			}
			return modifier;
		}

		public static float WhoreRoomAdjustment(Pawn whore)
		{
			float room_multiplier = 1f;
			Room ownedRoom = whore.ownership.OwnedRoom;

			if (ownedRoom == null) return 0f;

			//Room sharing is not liked by patrons
			room_multiplier = room_multiplier / (2 * (ownedRoom.Owners.Count() - 1) + 1);
			int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(ownedRoom.GetStat(RoomStatDefOf.Impressiveness));
			//Room impressiveness factor
			//0 < scoreStageIndex < 10 (Last time checked)
			//3 is mediocore
			if (scoreStageIndex == 0) { room_multiplier *= 0.3f; }
			if (scoreStageIndex > 3) { room_multiplier *= 1 + (scoreStageIndex - 3) / 3; } //top room triples the price
			return room_multiplier;
		}

		[SyncMethod]
		public static int PriceOfWhore(Pawn whore)
		{
			float NeedSexFactor = xxx.is_hornyorfrustrated(whore) ? 1 - xxx.need_some_sex(whore) / 8 : 1f;

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float price = Rand.Range(WhoreMinPrice(whore), WhoreMaxPrice(whore));

			price *= NeedSexFactor;
			//--Log.Message("[RJW] xxx::PriceOfWhore - price is " + price);

			return (int)Math.Round(price);
		}

		public static bool CanAfford(Pawn targetPawn, Pawn whore, int priceOfWhore = -1)
		{
			if (targetPawn.Faction == whore.Faction) return true;

			int price = priceOfWhore < 0 ? PriceOfWhore(whore) : priceOfWhore;
			if (price == 0)
				return true;

			Lord lord = targetPawn.GetLord();
			Faction faction = targetPawn.Faction;
			int totalAmountOfSilvers = targetPawn.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Silver);

			if (faction != null)
			{
				List<Pawn> caravanMembers = targetPawn.Map.mapPawns.PawnsInFaction(targetPawn.Faction).Where(x => x.GetLord() == lord && x.inventory?.innerContainer?.TotalStackCountOfDef(ThingDefOf.Silver) > 0).ToList();
				if (!caravanMembers.Any())
				{
					//--Log.Message("[RJW]CanAfford::(" + xxx.get_pawnname(targetPawn) + "," + xxx.get_pawnname(whore) + ") - totalAmountOfSilvers is " + totalAmountOfSilvers);
					return totalAmountOfSilvers >= price;
				}

				totalAmountOfSilvers += caravanMembers.Sum(member => member.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Silver));
			}

			//Log.Message("[RJW]CanAfford:: caravan can afford the price: " + (totalAmountOfSilvers >= price));
			return totalAmountOfSilvers >= price;
		}

		//priceOfWhore is assumed >=0, and targetPawn is assumed to be able to pay the price(either by caravan, or by himself)
		//This means that targetPawn has total stackcount of silvers >= priceOfWhore.
		public static int PayPriceToWhore(Pawn targetPawn, int priceOfWhore, Pawn whore)
		{
			int AmountLeft = priceOfWhore;
			if (targetPawn.Faction == whore.Faction || priceOfWhore == 0)
			{
				//--Log.Message("[RJW] xxx::PayPriceToWhore - No need to pay price");
				return AmountLeft;
			}
			Lord lord = targetPawn.GetLord();
			//Caravan guestCaravan = Find.WorldObjects.Caravans.Where(x => x.Spawned && x.ContainsPawn(targetPawn) && x.Faction == targetPawn.Faction && !x.IsPlayerControlled).FirstOrDefault();
			List<Pawn> caravanAnimals = targetPawn.Map.mapPawns.PawnsInFaction(targetPawn.Faction).Where(x => x.GetLord() == lord && x.inventory?.innerContainer != null && x.inventory.innerContainer.TotalStackCountOfDef(ThingDefOf.Silver) > 0).ToList();

			IEnumerable<Thing> TraderSilvers;
			if (!caravanAnimals.Any())
			{
				TraderSilvers = targetPawn.inventory.innerContainer.Where(x => x.def == ThingDefOf.Silver);
				foreach (Thing silver in TraderSilvers)
				{
					if (AmountLeft <= 0)
						return AmountLeft;
					int dropAmount = silver.stackCount >= AmountLeft ? AmountLeft : silver.stackCount;
					if (targetPawn.inventory.innerContainer.TryDrop(silver, whore.Position, whore.Map, ThingPlaceMode.Near, dropAmount, out Thing resultingSilvers))
					{
						if (resultingSilvers is null)
						{
							//--Log.Message("[RJW] xxx::PayPriceToWhore - silvers is null0");
							return AmountLeft;
						}
						AmountLeft -= resultingSilvers.stackCount;
						if (AmountLeft <= 0)
						{
							return AmountLeft;
						}
					}
					else
					{
						//--Log.Message("[RJW] xxx::PayPriceToWhore - TryDrop failed0");
						return AmountLeft;
					}
				}
				return AmountLeft;
			}

			foreach (Pawn animal in caravanAnimals)
			{
				TraderSilvers = animal.inventory.innerContainer.Where(x => x.def == ThingDefOf.Silver);
				foreach (Thing silver in TraderSilvers)
				{
					if (AmountLeft <= 0)
						return AmountLeft;
					int dropAmount = silver.stackCount >= AmountLeft ? AmountLeft : silver.stackCount;
					if (animal.inventory.innerContainer.TryDrop(silver, whore.Position, whore.Map, ThingPlaceMode.Near, dropAmount, out Thing resultingSilvers))
					{
						if (resultingSilvers is null)
						{
							//--Log.Message("[RJW] xxx::PayPriceToWhore - silvers is null1");
							return AmountLeft;
						}
						AmountLeft -= resultingSilvers.stackCount;
						if (AmountLeft <= 0)
						{
							return AmountLeft;
						}
					}
				}
			}
			return AmountLeft;
		}

		[SyncMethod]
		public static bool IsHookupAppealing(Pawn target, Pawn whore)
		{
			if (PawnUtility.WillSoonHaveBasicNeed(target))
			{
				//Log.Message("IsHookupAppealing - fail: " + xxx.get_pawnname(target) + " has need to do");
				return false;
			}
			float num = target.relations.SecondaryRomanceChanceFactor(whore) / 1.5f;
			if (xxx.is_frustrated(target))
			{
				num *= 3.0f;
			}
			else if (xxx.is_hornyorfrustrated(target))
			{
				num *= 2.0f;
			}
			if (xxx.is_zoophile(target) && !xxx.is_animal(whore))
			{
				num *= 0.5f;
			}
			if (xxx.AlienFrameworkIsActive)
			{
				if (xxx.is_xenophile(target))
				{
					if (target.def.defName == whore.def.defName)
						num *= 0.5f; // Same species, xenophile less interested.
					else
						num *= 1.5f; // Different species, xenophile more interested.
				}
				else if (xxx.is_xenophobe(target))
				{
					if (target.def.defName != whore.def.defName)
						num *= 0.25f; // Different species, xenophobe less interested.
				}
			}

			num *= 0.8f + ((float)whore.skills.GetSkill(SkillDefOf.Social).Level / 40); // 0.8 to 1.3
			num *= Mathf.InverseLerp(-100f, 0f, target.relations.OpinionOf(whore)); // 1 to 0 reduce score by negative opinion/relations to whore
			//Log.Message("IsHookupAppealing - score: " + num);
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			return Rand.Range(0.05f, 1f) < num;
		}

		// Summary:
		//   Check if the pawn is willing to hook up. Checked for both target and the whore(when hooking colonists).
		[SyncMethod]
		public static bool WillPawnTryHookup(Pawn target)
		{
			if (target.story.traits.HasTrait(TraitDefOf.Asexual))
			{
				return false;
			}
			Pawn lover = LovePartnerRelationUtility.ExistingMostLikedLovePartner(target, false);
			if (lover == null)
			{
				return true;
			}
			float num = target.relations.OpinionOf(lover);
			float num2 = Mathf.InverseLerp(30f, -80f, num);

			if (xxx.is_prude(target))
			{
				num2 = 0f;
			}
			else if (xxx.is_lecher(target))
			{
				//Lechers are always up for it.
				num2 = Mathf.InverseLerp(100f, 50f, num);
			}
			else if (target.Map == lover.Map)
			{
				//Less likely to cheat if the lover is on the same map.
				num2 = Mathf.InverseLerp(70f, 15f, num);
			}
			//else default values

			if (xxx.is_frustrated(target))
			{
				num2 *= 1.8f;
			}
			else if (xxx.is_hornyorfrustrated(target))
			{
				num2 *= 1.4f;
			}
			num2 /= 1.5f;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			return Rand.Range(0f, 1f) < num2;
		}
	}
}
using System.Linq;
using Verse;
using RimWorld;
using System.Text;
using Multiplayer.API;
using UnityEngine;

namespace rjw
{
	public class Hediff_PartBaseArtifical : Hediff_Implant 
	{
		public override bool ShouldRemove => false;

		public bool discovered = false;

		// Used for ovipositors.
		public int nextEggTick = -1;
		
		public override void ExposeData()
		{
			base.ExposeData();
			
			Scribe_Values.Look(ref this.nextEggTick, "nextEggTick");
//			Scribe_Values.Look(ref this.produceEggs, "produceEggs");
//			Scribe_Defs.Look(ref this.pawnKindDefOverride, "pawnKindDefOverride");
//			Scribe_Values.Look(ref this.genitalType, "genitalType");
		}


		public override string LabelBase
		{
			get
			{
				/*
				 * make patch to make/save capmods?
				if (CapMods.Count < 5)
				{
					PawnCapacityModifier pawnCapacityModifier = new PawnCapacityModifier();
					pawnCapacityModifier.capacity = PawnCapacityDefOf.Moving;
					pawnCapacityModifier.offset += 0.5f;
					CapMods.Add(pawnCapacityModifier);
				}
				*/

				//name/kind
				return this.def.label;
			}
		}

		//public override string LabelInBrackets
		//{
		//	get
		//	{
		//		string size = "on fire!";
		//		size = (this.comps.Find(x => x is CompHediffBodyPart) as CompHediffBodyPart).Size;
		//		return size;

		//		//vanilla
		//		//return (this.CurStage != null && !this.CurStage.label.NullOrEmpty()) ? this.CurStage.label : null;
		//	}
		//}

		//overrides comps
		//public override string TipStringExtra
		//{
		//	get
		//	{
		//		StringBuilder stringBuilder = new StringBuilder();
		//		foreach (StatDrawEntry current in HediffStatsUtility.SpecialDisplayStats(this.CurStage, this))
		//		{
		//			if (current.ShouldDisplay)
		//			{
		//				stringBuilder.AppendLine(current.LabelCap + ": " + current.ValueString);
		//			}
		//		}
		//		//stringBuilder.AppendLine("Size: " + this.TryGetComp<CompHediffBodyPart>.Size);
		//		//stringBuilder.AppendLine("1");// size?
		//		//stringBuilder.AppendLine("2");// erm something?
		//		return stringBuilder.ToString();
		//	}
		//}

		/// <summary>
		/// stack hediff in health tab?
		/// </summary>
		public override int UIGroupKey
		{
			get
			{
				if (RJWSettings.StackRjwParts)
					//(Label x count)
					return this.Label.GetHashCode();
				else
					//dont
					return loadID;
			}
		}

		/// <summary>
		/// do not merge same rjw parts into one
		/// </summary>
		public override bool TryMergeWith(Hediff other)
		{
			return false;
		}

		/// <summary>
		/// show rjw parts in health tab or not
		/// </summary>
		public override bool Visible
		{
			get
			{
				if (RJWSettings.ShowRjwParts == RJWSettings.ShowParts.Hide)
				{
					discovered = false;
				}
				else if (!discovered)
				{
					if (RJWSettings.ShowRjwParts == RJWSettings.ShowParts.Show)
					{
						discovered = true;
						return discovered;
					}

					//show at game start
					if (Current.ProgramState != ProgramState.Playing && Prefs.DevMode)
						return true;
					
					//show for hero
					if (pawn.IsDesignatedHero() && pawn.IsHeroOwner())
					{
						discovered = true;
						return discovered;
					}

					//show if no clothes
					if (pawn.apparel != null)// animals?
					{
						bool hasPants;
						bool hasShirt;
						pawn.apparel.HasBasicApparel(out hasPants, out hasShirt);// naked?

						if (!hasPants)
						{
							bool flag3 = false;
							foreach (BodyPartRecord current in this.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
							{
								if (current.IsInGroup(BodyPartGroupDefOf.Legs))
								{
									flag3 = true;
									break;
								}
							}
							if (!flag3)
							{
								hasPants = true;
							}
						}

						if (this.def.defName.ToLower().Contains("breast") || this.def.defName.ToLower().Contains("chest"))
							discovered = !hasShirt;
						else
							discovered = !hasPants;

					}
				}

				return discovered;
			}
		}

		/// <summary>
		/// egg production ticks
		/// </summary>
		[SyncMethod]
		public override void Tick()
		{
			ageTicks++;
			if (!pawn.IsHashIntervalTick(10000)) // run every ~3min
			{
				return;
			}

			var partBase = def as HediffDef_PartBase;
			if (partBase != null)
			{
				if (partBase.produceEggs)
				{
					var IsPlayerFaction = pawn.Faction?.IsPlayer ?? false; //colonists/animals
					if (pawn.Map.IsPlayerHome || IsPlayerFaction || pawn.IsPrisonerOfColony)
					{
						ageTicks++;
				
						if (nextEggTick < 0)
						{
							nextEggTick = Rand.Range(partBase.minEggTick, partBase.maxEggTick);
							return;
						}

						if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) <= 0.5)
						{
							return;
						}

						if (nextEggTick > 0 && ageTicks >= nextEggTick)
						{
							float maxEggsSize = (pawn.BodySize / 5) * (xxx.has_quirk(pawn, "Incubator") ? 2f : 1f) *
												(Genital_Helper.has_ovipositorF(pawn) ? 2f : 0.5f);
							float eggedsize = 0;
							foreach (var ownEgg in pawn.health.hediffSet.GetHediffs<Hediff_InsectEgg>())
							{
								if (ownEgg.father != null)
									eggedsize += ownEgg.father.RaceProps.baseBodySize / 5;
								else
									eggedsize += ownEgg.implanter.RaceProps.baseBodySize / 5;
							}

							if (RJWSettings.DevMode) Log.Message($"{xxx.get_pawnname(pawn)} filled with {eggedsize} out of max capacity of {maxEggsSize} eggs.");
							if (eggedsize < maxEggsSize)
							{
								HediffDef_InsectEgg egg = null;
								string defname = "";

								while (egg == null)
								{
									if (defname == "")
									{
										if (RJWSettings.DevMode) Log.Message(" trying to find " + pawn.kindDef.defName + " egg");
										defname = pawn.kindDef.defName;
									}
									else
									{

										if (RJWSettings.DevMode) Log.Message(" no " + defname + " egg found, defaulting to Unknown egg");
										defname = "Unknown";
									}

									//Rand.PopState();
									//Rand.PushState(RJW_Multiplayer.PredictableSeed());
									egg = (from x in DefDatabase<HediffDef_InsectEgg>.AllDefs where x.IsParent(defname) select x)
										.RandomElement();
								}

								if (RJWSettings.DevMode) Log.Message("I choose you " + egg + "!");

								var genitals = Genital_Helper.get_genitalsBPR(pawn);
								if (genitals != null)
								{
									var addedEgg = pawn.health.AddHediff(egg, genitals) as Hediff_InsectEgg;
									addedEgg?.Implanter(pawn);
								}
							}

							// Reset for next egg.
							ageTicks = 0;
							nextEggTick = -1;
						}
					}
				}
			}
		}
	}
}
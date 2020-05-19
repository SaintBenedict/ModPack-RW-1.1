using System.Text;
using Verse;
using RimWorld;
using Multiplayer.API;

namespace rjw
{
	/// <summary>
	/// Comp for rjw hediff parts
	/// </summary>
	public class CompHediffBodyPart : HediffComp
	{
		public string Size => parent.CurStage?.label ?? "";
		public string RaceOwner = "Unknows species";        //base race when part created
		public string PreviousOwner = "Unknows";			//base race when part created
		public float SizeBase;								//base size when part created, someday alter by operation
		public float SizeOwner = 0;							//modifier of 1st owner race body size
		public float EffSize;								//SizeBase x SizeOwner = current size | hediff.severity
		public string FluidType = "";						//cummies/milk - insectjelly/honey etc
		public float FluidAmmount;							//ammount of Milk/Ejaculation/Wetness
		public float FluidModifier = 1f;					//
		public string Eggs;									//for ovi eggs, maybe

		/// <summary>
		/// part size in labels
		/// </summary>

		//public override string CompLabelInBracketsExtra
		//{
		//	get
		//	{
		//		if (Size != "")
		//			return Size;

		//		return null;
		//	}
		//}

		/// <summary>
		/// save data
		/// </summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref SizeBase, "SizeBase");
			Scribe_Values.Look(ref SizeOwner, "SizeOwner");
			Scribe_Values.Look(ref RaceOwner, "RaceOwner");
			Scribe_Values.Look(ref PreviousOwner, "PreviousOwner");
			Scribe_Values.Look(ref FluidType, "FluidType");
			Scribe_Values.Look(ref FluidAmmount, "FluidAmmount");
			Scribe_Values.Look(ref FluidModifier, "FluidModifier");
			Scribe_Values.Look(ref Eggs, "Eggs");
		}

		/// <summary>
		/// show part info in healthab
		/// </summary>
		public override string CompTipStringExtra
		{
			get
			{
				if (SizeOwner == 0)
				{
					initComp();
					updatesize();
					updatepartposition();
				}

				//Log.Message("[RJW] CompTipStringExtra " + xxx.get_pawnname(Pawn) + " " + parent.def.defName);
				StringBuilder stringBuilder = new StringBuilder();
				if (parent.def.defName.ToLower().Contains("breasts"))
				{
					if (FluidAmmount != 0)
					{
						if (FluidType != "")
						{
							stringBuilder.AppendLine("Producing: " + FluidType);
							stringBuilder.AppendLine("Amount: " + (FluidAmmount*FluidModifier).ToString("F2"));
						}
					}
				}
				else if (parent.def.defName.ToLower().Contains("penis") || parent.def.defName.ToLower().Contains("vagina"))
				{
					if (FluidAmmount != 0)
					{
						if (FluidType != "")
							stringBuilder.AppendLine("Cum: " + FluidType);
						{
							if (parent.def.defName.ToLower().Contains("penis"))
								stringBuilder.AppendLine("Ejaculation: " + (FluidAmmount * FluidModifier).ToString("F2") + "ml");
							if (parent.def.defName.ToLower().Contains("vagina"))
								stringBuilder.AppendLine("Wetness: " + (FluidAmmount * FluidModifier).ToString("F2"));
						}
					}
				}
				else if (parent.def.defName.ToLower().Contains("anus"))
				{
					if (FluidType != "" && FluidAmmount != 0)
					{
						stringBuilder.AppendLine("Lube: " + FluidType);
						if (parent.def.defName.ToLower().Contains("anus"))
							stringBuilder.AppendLine("Wetness: " + (FluidAmmount * FluidModifier).ToString("F2"));
					}
				}
				else if (parent.def.defName.ToLower().Contains("ovi"))
				{
					if (Eggs != "")
						stringBuilder.AppendLine("Eggs: " + Eggs);
				}

				return stringBuilder.ToString();
			}
		}

		//TODO: somday part enlager operations
		public void updatesize(float value = 0)
		{
			if (value == 0)
			{
				// CompHediff.Size = CompThing.Size;
				//Log.Message("CompHediffBodyPart::updatesize increase( " + (SizeOwner > parent.pawn.BodySize) + " )");
				if (SizeOwner > parent.pawn.BodySize)
				{
					// decrease
					value = (SizeOwner - parent.pawn.BodySize) / SizeOwner;
					//Log.Message("CompHediffBodyPart::updatesize - decrease");
					//Log.Message("CompHediffBodyPart::updatesize( " + SizeOwner + " )");
					//Log.Message("CompHediffBodyPart::updatesize( " + parent.pawn.BodySize + " )");
					//Log.Message("CompHediffBodyPart::updatesize - value");
					//Log.Message("CompHediffBodyPart::updatesize( " + value + " )");

					value = SizeBase * (1 + value);
				}
				else
				{
					// increase
					value = (parent.pawn.BodySize - SizeOwner) / parent.pawn.BodySize;
					//Log.Message("CompHediffBodyPart::updatesize - increase");
					//Log.Message("CompHediffBodyPart::updatesize( " + SizeOwner + " )");
					//Log.Message("CompHediffBodyPart::updatesize( " + parent.pawn.BodySize + " )");
					//Log.Message("CompHediffBodyPart::updatesize - value");
					//Log.Message("CompHediffBodyPart::updatesize( " + value + " )");

					value = SizeBase * (1 - value);
				}

				//Log.Message("CompHediffBodyPart::updatesize - value offset");
				//Log.Message("CompHediffBodyPart::updatesize( " + value + " )");
			}
			else
			{
			// idk do something here? (operation etc)
			}
			//Log.Message("CompHediffBodyPart::updatesize - severity");
			//Log.Message("CompHediffBodyPart::updatesize( " + parent.Severity + " )");
			if (value <= 0)
			{
				//Log.Message("CompHediffBodyPart::updatesize( " + parent.pawn.Name + " )");
				value = 0.01f;
			}

			parent.Severity = value;
			//Log.Message("CompHediffBodyPart::updatesize( " + parent.Severity + " )");
			//Log.Message("CompHediffBodyPart::updatesize( " + xxx.get_pawnname(parent.pawn) + " ) kinddef: " + parent.pawn.kindDef.defName + " ) def: " + parent.def.defName);
		}

		public void updatepartposition()
		{
			var partBase = parent.def as HediffDef_PartBase;
			if (partBase != null)
			{
				if (parent.Part == null || partBase.DefaultBodyPart != "" && parent.Part.def.defName != partBase.DefaultBodyPart)
				{
					var bp = parent.pawn.RaceProps.body.AllParts.Find(x => x.def.defName.Contains(partBase.DefaultBodyPart));
					//if (pawn.IsColonist)
					//{
					//	Log.Message(xxx.get_pawnname(pawn) + " has broken hediffs, removing " + this.ToString());
					//	Log.Message(Part.ToString());
					//	Log.Message(bp.def.defName);
					//	Log.Message(partBase.DefaultBodyPart.ToString());
					//}
					parent.Part = bp;
				}
			}
		}

		/// <summary>
		/// fill comp data
		/// </summary>
		[SyncMethod]
		public void initComp(Pawn pawn = null, bool reroll = false)
		{
			if (pawn == null)
				pawn = parent.pawn;

			double value = Rand.Range(0.01f, 1);
			bool trap = false;
			if (reroll == true)
				value = SizeBase;
			{
				if (parent.def.defName.ToLower().Contains("breast"))
				{
					//FluidType = "Milk";
					FluidAmmount = 0;

					if (pawn != null)
					{
						if (pawn.gender == Gender.Male && RJWSettings.MaleTrap)
							if (pawn.Faction != null && !xxx.is_animal(pawn)) //null faction throws error
							{
								//natives/spacer futa
								float chance = (int)pawn.Faction.def.techLevel < 5 ? RJWSettings.futa_natives_chance : RJWSettings.futa_spacers_chance;
								//nymph futa gender
								chance = xxx.is_nympho(pawn) ? RJWSettings.futa_nymph_chance : chance;

								if (Rand.Chance(chance))
								{
									//make trap
									trap = true;
								}
							}

						if (pawn.gender == Gender.Male && !trap && reroll == false)
							value = 0.01f;

					}
				}
				else if (parent.def.defName.ToLower().Contains("penis"))
				{
					FluidAmmount = (float)value * pawn.RaceProps.baseBodySize * pawn.RaceProps.baseBodySize * 10 * 2 * Rand.Range(0.75f, 1.25f);
				}
				else if (parent.def.defName.ToLower().Contains("vagina"))
				{
					FluidAmmount = (float)value * pawn.RaceProps.baseBodySize * pawn.RaceProps.baseBodySize * 10 * Rand.Range(0.75f, 1.25f);
				}
				else if (parent.def.defName.ToLower().Contains("anus"))
				{
					FluidAmmount = 0;
				}
				else if (parent.def.defName.ToLower().Contains("tentacle"))
				{
					value *= 2;
					FluidAmmount = (float)value * pawn.RaceProps.baseBodySize * pawn.RaceProps.baseBodySize * 10 * 2 * Rand.Range(0.75f, 1.25f);
				}
				else if (parent.def.defName.ToLower().Contains("ovi"))
				{
					Eggs = pawn?.kindDef?.label ?? "";
					FluidAmmount = 0;
				}

				FluidType = (parent.def as HediffDef_PartBase).FluidType;
				SizeBase = (float)value;
				SizeOwner = pawn?.BodySize ?? 1.0f;
				RaceOwner = pawn?.kindDef?.race.LabelCap ?? "Unknows species";
				PreviousOwner = xxx.get_pawnname(pawn);
				EffSize = SizeOwner*SizeBase;
				FluidModifier = 1f;
			}
		}
	}
}
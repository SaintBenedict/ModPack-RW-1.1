using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using Multiplayer.API;

namespace rjw
{
	public class Hediff_Semen : HediffWithComps
	{
		public int semenType = SemenHelper.CUM_NORMAL;//-> different colors

		public string giverName = null;//not utilized right now, maybe in the future save origin of the semen

		public override string LabelInBrackets
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.LabelInBrackets);
				if (this.sourceHediffDef != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.sourceHediffDef.label);
				}
				else if (this.source != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.source.label);
					if (this.sourceBodyPartGroup != null)
					{
						stringBuilder.Append(" ");
						stringBuilder.Append(this.sourceBodyPartGroup.LabelShort);
					}
				}
				return stringBuilder.ToString();
			}
		}


		public override string SeverityLabel
		{
			get
			{
				if (this.Severity == 0f)
				{
					return null;
				}
				return this.Severity.ToString("F1");
			}
		}

		[SyncMethod]
		public override bool TryMergeWith(Hediff other)
		{
			//if a new Semen hediff is added to the same body part, they are combined. if severity reaches more than 1, spillover to other body parts occurs

			Hediff_Semen hediff_Semen = other as Hediff_Semen;
			if (hediff_Semen != null && hediff_Semen.def == this.def && hediff_Semen.Part == base.Part && this.def.injuryProps.canMerge)
			{
				semenType = hediff_Semen.semenType;//take over new creature color

				float totalAmount = hediff_Semen.Severity + this.Severity;
				if (totalAmount > 1.0f)
				{
					BodyPartDef spillOverTo = SemenHelper.spillover(this.Part.def);//SemenHelper saves valid other body parts for spillover
					if (spillOverTo != null)
					{
						//Rand.PopState();
						//Rand.PushState(RJW_Multiplayer.PredictableSeed());
						IEnumerable<BodyPartRecord> availableParts = SemenHelper.getAvailableBodyParts(pawn);//gets all non missing, valid body parts
						IEnumerable<BodyPartRecord> filteredParts = availableParts.Where(x => x.def == spillOverTo);//filters again for valid spill target
						BodyPartRecord spillPart = filteredParts.RandomElement<BodyPartRecord>();//then pick one
						if (spillPart != null)
						{
							SemenHelper.cumOn(pawn, spillPart, totalAmount - this.Severity, null, semenType);
						}
					}
				}

				return (base.TryMergeWith(other));

			}
			return (false);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref semenType, "semenType", SemenHelper.CUM_NORMAL);

			if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
			{
				//Log.Error("Hediff_Semen has null part after loading.", false);
				this.pawn.health.hediffSet.hediffs.Remove(this);
				return;
			}
		}

		//handles the icon in the health tab and its color
		public override TextureAndColor StateIcon
		{
			get
			{
				TextureAndColor tex = TextureAndColor.None;
				Color color = Color.white;
				switch (semenType)
				{
					case SemenHelper.CUM_NORMAL:
						color = SemenHelper.color_normal;
						break;
					case SemenHelper.CUM_INSECT:
						color = SemenHelper.color_insect;
						break;
					case SemenHelper.CUM_MECHA:
						color = SemenHelper.color_mecha;
						break;
				}

				Texture2D tex2d = BukkakeContent.SemenIcon_little;
				switch (this.CurStageIndex)
				{
					case 0:
						tex2d = BukkakeContent.SemenIcon_little;
						break;
					case 1:
						tex2d = BukkakeContent.SemenIcon_some;
						break;
					case 2:
						tex2d = BukkakeContent.SemenIcon_dripping;
						break;
					case 3:
						tex2d = BukkakeContent.SemenIcon_drenched;
						break;
				}

				tex = new TextureAndColor(tex2d, color);

				return tex;
			}
		}

	}
}

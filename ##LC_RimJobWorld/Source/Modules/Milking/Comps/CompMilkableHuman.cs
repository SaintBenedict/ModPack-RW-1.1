using RimWorld;
using Verse;

namespace rjw
{
	public class CompMilkableHuman : CompHasGatherableBodyResource
	{
		protected override int GatherResourcesIntervalDays => Props.milkIntervalDays;

		protected override int ResourceAmount => Props.milkAmount;

		protected override ThingDef ResourceDef => Props.milkDef;

		protected override string SaveKey => "milkFullness";

		public CompProperties_MilkableHuman Props => (CompProperties_MilkableHuman)props;

		protected override bool Active
		{
			get
			{
				if (!Active)
				{
					return false;
				}
				Pawn pawn = parent as Pawn;
				if (pawn != null)
				{
					//idk should probably remove non rjw stuff
					//should merge Lactating into .cs hediff?
					//vanilla
					//C&P?
					//rjw human
					//rjw animal
					if ((!pawn.health.hediffSet.HasHediff(HediffDef.Named("RJW_lactating"), false))
					&& ((pawn.health.hediffSet.HasHediff(HediffDef.Named("Pregnant"), false) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Pregnant"), false).Visible)
					//|| (pawn.health.hediffSet.HasHediff(HediffDef.Named("HumanPregnancy"), false) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("HumanPregnancy"), false).Visible)
					|| (pawn.health.hediffSet.HasHediff(HediffDef.Named("RJW_pregnancy"), false) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_pregnancy"), false).Visible)
					|| (pawn.health.hediffSet.HasHediff(HediffDef.Named("RJW_pregnancy_beast"), false) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_pregnancy_beast"), false).Visible)))
					{
						pawn.health.AddHediff(HediffDef.Named("RJW_lactating"), null, null, null);
					}
					if ((!Props.milkFemaleOnly || pawn.gender == Gender.Female)
					&& (pawn.ageTracker.CurLifeStage.reproductive)
					&& (pawn.RaceProps.Humanlike)
					&& (pawn.health.hediffSet.HasHediff(HediffDef.Named("RJW_lactating"), false)
						//|| pawn.health.hediffSet.HasHediff(HediffDef.Named("Lactating_Permanent"), false)
						//|| pawn.health.hediffSet.HasHediff(HediffDef.Named("Lactating_Natural"), false)
						//|| pawn.health.hediffSet.HasHediff(HediffDef.Named("Lactating_Drug"), false)
						))
					{
						return true;
					}
				}
				return false;
			}
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			return Translator.Translate("MilkFullness") + ": " + GenText.ToStringPercent(this.Fullness);
		}
	}
}

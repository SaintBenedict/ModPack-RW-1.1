using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace rjw
{
	public class Need_Sex : Need_Seeker
	{
		private bool isInvisible => pawn.Map == null;

		private bool BootStrapTriggered = false;

		private int needsex_tick = needsex_tick_timer;
		public const int needsex_tick_timer = 10;
		private static float decay_per_day = 0.3f;
		private float decay_rate_modifier = RJWSettings.sexneed_decay_rate;

		public float thresh_frustrated()
		{
			return 0.05f;
		}

		public float thresh_horny()
		{
			return 0.25f;
		}

		public float thresh_neutral()
		{
			return 0.50f;
		}

		public float thresh_satisfied()
		{
			return 0.75f;
		}

		public float thresh_ahegao()
		{
			return 0.95f;
		}

		public Need_Sex(Pawn pawn) : base(pawn)
		{
			//if (xxx.is_mechanoid(pawn)) return; //Added by nizhuan-jjr:Misc.Robots are not allowed to have sex, so they don't need sex actually.
			threshPercents = new List<float>
			{
				thresh_frustrated(),
				thresh_horny(),
				thresh_neutral(),
				thresh_satisfied(),
				thresh_ahegao()
			};
		}

		public static float brokenbodyfactor(Pawn pawn)
		{
			//This adds in the broken body system
			float broken_body_factor = 1f;
			if (pawn.health.hediffSet.HasHediff(xxx.feelingBroken))
			{
				switch (pawn.health.hediffSet.GetFirstHediffOfDef(xxx.feelingBroken).CurStageIndex)
				{
					case 0:
						return 0.75f;
					case 1:
						return 1.4f;
					case 2:
						return 2f;
				}
			}
			return broken_body_factor;
		}

		//public override bool ShowOnNeedList
		//{
		//	get
		//	{
		//		if (Genital_Helper.has_genitals(pawn))
		//			return true;

		//		Log.Message("[RJW]curLevelInt " + curLevelInt);
		//		return false;
		//	}
		//}

		//public override string GetTipString()
		//{
		//	return string.Concat(new string[]
		//	{
		//		this.LabelCap,
		//		": ",
		//		this.CurLevelPercentage.ToStringPercent(),
		//		"\n",
		//		this.def.description,
		//		"\n",
		//	});
		//}

		public static float druggedfactor(Pawn pawn)
		{
			if (pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomAddiction")) && !pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomEffect")))
			{
				//Log.Message("[RJW]Need_Sex::druggedfactor 0.5 pawn is " + xxx.get_pawnname(pawn));
				return 0.5f;
			}

			if (pawn.health.hediffSet.HasHediff(HediffDef.Named("HumpShroomEffect")))
			{
				//Log.Message("[RJW]Need_Sex::druggedfactor 3 pawn is " + xxx.get_pawnname(pawn));
				return 3f;
			}

			//Log.Message("[RJW]Need_Sex::druggedfactor 1 pawn is " + xxx.get_pawnname(pawn));
			return 1f;
		}

		static float diseasefactor(Pawn pawn)
		{
			if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Boobitis")))
			{
				return 3f;
			}

			return 1f;
		}

		static float agefactor(Pawn pawn)
		{
			if (xxx.is_human(pawn))
			{
				int age = pawn.ageTracker.AgeBiologicalYears;

				Need_Sex horniness = pawn.needs.TryGetNeed<Need_Sex>();
				if (horniness.CurLevel > 0.5f)
					return 1f;

				if (age < RJWSettings.sex_minimum_age)
					return 0f;
			}
			return 1f;
		}

		public override void NeedInterval() //150 ticks between each calls
		{
			if (isInvisible) return;

			if (needsex_tick <= 0)
			{
				std_updater.update(pawn);

				if (xxx.is_asexual(pawn)) return;
				
				//--Log.Message("[RJW]Need_Sex::NeedInterval is called0 - pawn is "+xxx.get_pawnname(pawn));
				needsex_tick = needsex_tick_timer;
				//age = age / maxage;

				if (!def.freezeWhileSleeping || pawn.Awake())
				{
					decay_rate_modifier = RJWSettings.sexneed_decay_rate * xxx.get_sex_drive(pawn);

					var partBPR = Genital_Helper.get_genitalsBPR(pawn);
					var parts = Genital_Helper.get_PartsHediffList(pawn, partBPR);

					//every 200 calls will have a real functioning call
					var fall_per_tick =
						//def.fallPerDay *
						decay_per_day *
						brokenbodyfactor(pawn) *
						druggedfactor(pawn) *
						diseasefactor(pawn) *
						agefactor(pawn) *
						(((Genital_Helper.has_penis_fertile(pawn, parts) || Genital_Helper.has_penis_infertile(pawn, parts)) && Genital_Helper.has_vagina(pawn, parts)) ? 2.0f : 1.0f) /
						60000.0f;
					//--Log.Message("[RJW]Need_Sex::NeedInterval is called - pawn is " + xxx.get_pawnname(pawn) + " is has both genders " + (Genital_Helper.has_penis(pawn) && Genital_Helper.has_vagina(pawn)));
					//Log.Message("[RJW] " + xxx.get_pawnname(pawn) + "'s sex need stats:: fall_per_tick: " + fall_per_tick + ", sex_need_factor_from_lifestage: " + sex_need_factor_from_lifestage(pawn) );

					var fall_per_call =
						150 *
						fall_per_tick *
						needsex_tick_timer;
					CurLevel -= fall_per_call * decay_rate_modifier;
					// Each day has 60000 ticks, each hour has 2500 ticks, so each hour has 50/3 calls, in other words, each call takes .06 hour.
					//Log.Message("[RJW] " + xxx.get_pawnname(pawn) + "'s sex need stats:: Decay/call: " + fall_per_call * decay_rate_modifier + ", Cur.lvl: " + CurLevel + ", Dec. rate: " + decay_rate_modifier);
				}

				//--Log.Message("[RJW]Need_Sex::NeedInterval is called1");

				//If they need it, they should seek it
				if (CurLevel < thresh_horny() && (pawn.mindState.canLovinTick - Find.TickManager.TicksGame > 300) )
				{
					pawn.mindState.canLovinTick = Find.TickManager.TicksGame + 300;
				}

				// the bootstrap of the mapInjector will only be triggered once per visible pawn.
				if (!BootStrapTriggered)
				{
					//--Log.Message("[RJW]Need_Sex::NeedInterval::calling boostrap - pawn is " + xxx.get_pawnname(pawn));
					xxx.bootstrap(pawn.Map);
					BootStrapTriggered = true;
				}
			}
			else
			{
				needsex_tick--;
			}
			//--Log.Message("[RJW]Need_Sex::NeedInterval is called2 - needsex_tick is "+needsex_tick);
		}
	}
}
using System;
using RimWorld;
using Verse;
using Multiplayer.API;

namespace rjw
{
	public class IncidentWorker_TestInc : IncidentWorker
	{
		public static void list_backstories()
		{
			foreach (var bs in BackstoryDatabase.allBackstories.Values)
				Log.Message("Backstory \"" + bs.title + "\" has identifier \"" + bs.identifier + "\"");
		}

		public static void inject_designator()
		{
			//var des = new Designator_ComfortPrisoner();
			//Find.ReverseDesignatorDatabase.AllDesignators.Add(des);
			//Find.ReverseDesignatorDatabase.AllDesignators.Add(new Designator_Breed());
		}


		// Applies permanent damage to a randomly chosen colonist, to test that this works
		[SyncMethod]
		public static void damage_virally(Map m)
		{
			var vir_dam = DefDatabase<DamageDef>.GetNamed("ViralDamage");
			var p = m.mapPawns.FreeColonists.RandomElement();
			var lun = p.RaceProps.body.AllParts.Find((BodyPartRecord bpr) => String.Equals(bpr.def.defName, "LeftLung"));
			var dam_def = HealthUtility.GetHediffDefFromDamage(vir_dam, p, lun);
			var inj = (Hediff_Injury)HediffMaker.MakeHediff(dam_def, p, null);
			inj.Severity = 2.0f;
			inj.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
			p.health.AddHediff(inj, lun, null);
		}

		// Gives all colonists on the map a severe syphilis or HIV infection
		[SyncMethod]
		public static void infect_the_colonists(Map m)
		{
			foreach (var p in m.mapPawns.FreeColonists)
			{
				if (Rand.Value < 0.50f)
					std_spreader.infect(p, std.syphilis);
				// var std_hed_def = (Rand.Value < 0.50f) ? std.syphilis.hediff_def : std.hiv.hediff_def;
				// p.health.AddHediff (std_hed_def);
				// p.health.hediffSet.GetFirstHediffOfDef (std_hed_def).Severity = Rand.Range (0.50f, 0.90f);
			}
		}

		// Reduces the sex need of the selected pawn
		public static void reduce_sex_need_on_select(Map m)
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn != null)
			{
				if (pawn.needs.TryGetNeed<Need_Sex>() != null)
				{
					//--Log.Message("[RJW]TestInc::reduce_sex_need_on_select is called");
					pawn.needs.TryGetNeed<Need_Sex>().CurLevel -= 0.5f;
				}
			}
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			var m = (Map)parms.target;

			// list_backstories ();
			// inject_designator ();
			// spawn_nymphs (m);
			// damage_virally (m);
			//infect_the_colonists(m);
			reduce_sex_need_on_select(m);

			return true;
		}
	}
}
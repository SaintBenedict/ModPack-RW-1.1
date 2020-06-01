using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	internal class JobDef_RapeEnemy : JobDef
	{
		public List<string> TargetDefNames = new List<string>();
		public int priority = 0;

		protected JobDriver_RapeEnemy instance
		{
			get
			{
				if (_tmpInstance == null)
				{
					_tmpInstance = (JobDriver_RapeEnemy)Activator.CreateInstance(driverClass);
				}
				return _tmpInstance;
			}
		}

		private JobDriver_RapeEnemy _tmpInstance;

		public virtual bool CanUseThisJobForPawn(Pawn rapist)
		{
			if (rapist.CurJob != null && rapist.CurJob.def != JobDefOf.LayDown)
				return false;

			return instance.CanUseThisJobForPawn(rapist);// || TargetDefNames.Contains(rapist.def.defName);
		}

		public virtual Pawn FindVictim(Pawn rapist, Map m)
		{
			return instance.FindVictim(rapist, m);
		}
	}



	public class JobDriver_RapeEnemy : JobDriver_Rape
	{
		private static readonly HediffDef is_submitting = HediffDef.Named("Hediff_Submitting");//used in find_victim

		//override can_rape mechanics
		protected bool requireCanRape = true;

		public virtual bool CanUseThisJobForPawn(Pawn rapist)
		{
			return xxx.is_human(rapist);
		}

		// this is probably useseless, maybe there be something in future
		public virtual bool considerStillAliveEnemies => true;

		[SyncMethod]
		public virtual Pawn FindVictim(Pawn rapist, Map m)
		{
			//Log.Message("[RJW]" + this.GetType().ToString() + "::TryGiveJob( " + xxx.get_pawnname(rapist) + " ) map " + m?.ToString());
			if (rapist == null || m == null) return null;
			//Log.Message("[RJW]" + this.GetType().ToString() + "::TryGiveJob( " + xxx.get_pawnname(rapist) + " ) can rape " + xxx.can_rape(rapist));
			if (requireCanRape && !xxx.can_rape(rapist)) return null;

			List<Pawn> validTargets = new List<Pawn>();
			float min_fuckability = 0.10f;                          // Don't rape pawns with <10% fuckability
			float avg_fuckability = 0f;                             // Average targets fuckability, choose target higher than that
			var valid_targets = new Dictionary<Pawn, float>();      // Valid pawns and their fuckability
			Pawn chosentarget = null;                               // Final target pawn

			IEnumerable<Pawn> targets = m.mapPawns.AllPawnsSpawned.Where(x
				=> !x.IsForbidden(rapist) && x != rapist && x.HostileTo(rapist)
				&& rapist.CanReserveAndReach(x, PathEndMode.Touch, Danger.Some, xxx.max_rapists_per_prisoner, 0)
				&& IsValidTarget(rapist, x))
				.ToList();

			if (targets.Any(x => IsBlocking(rapist, x)))
			{
				return null;
			}

			foreach (var target in targets)
			{
				if (!xxx.can_path_to_target(rapist, target.Position))
					continue;// too far

				float fuc = GetFuckability(rapist, target);

				if (fuc > min_fuckability)
					valid_targets.Add(target, fuc);
			}

			if (valid_targets.Any())
			{
				avg_fuckability = valid_targets.Average(x => x.Value);

				// choose pawns to fuck with above average fuckability
				var valid_targetsFiltered = valid_targets.Where(x => x.Value >= avg_fuckability);

				if (valid_targetsFiltered.Any())
					chosentarget = valid_targetsFiltered.RandomElement().Key;
			}

			return chosentarget;
		}

		bool IsBlocking(Pawn rapist, Pawn target)
		{
			return considerStillAliveEnemies && !target.Downed && rapist.CanSee(target);
		}

		bool IsValidTarget(Pawn rapist, Pawn target)
		{
			if (!RJWSettings.bestiality_enabled)
			{
				if (xxx.is_animal(target) && xxx.is_human(rapist))
				{
					//bestiality disabled, skip.
					return false;
				}
				if (xxx.is_animal(rapist) && xxx.is_human(target))
				{
					//bestiality disabled, skip.
					return false;
				}
			}

			if (!RJWSettings.animal_on_animal_enabled)
				if ((xxx.is_animal(target) && xxx.is_animal(rapist)))
				{
					//animal_on_animal disabled, skip.
					return false;
				}

			if (target.CurJob?.def == xxx.gettin_raped || target.CurJob?.def == xxx.gettin_loved)
			{
				//already having sex with someone, skip, give chance to other victims.
				return false;
			}

			return Can_rape_Easily(target) &&
				(xxx.is_human(target) || xxx.is_animal(target)) &&
				rapist.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Some, xxx.max_rapists_per_prisoner, 0);
		}

		public virtual float GetFuckability(Pawn rapist, Pawn target)
		{
			//Log.Message("[RJW]JobDriver_RapeEnemy::GetFuckability(" + rapist.ToString() + "," + target.ToString() + ")");
			if (target.health.hediffSet.HasHediff(is_submitting))//it's not about attractiveness anymore, it's about showing who's whos bitch
			{
				return 2 * SexAppraiser.would_fuck(rapist, target, invert_opinion: true, ignore_bleeding: true, ignore_gender: true);
			}
			return !SexAppraiser.would_rape(rapist, target) ? 0f
				: SexAppraiser.would_fuck(rapist, target, invert_opinion: true, ignore_bleeding: true, ignore_gender: true);
		}

		protected bool Can_rape_Easily(Pawn pawn)
		{
			return xxx.can_get_raped(pawn) && !pawn.IsBurning();
		}
	}
}
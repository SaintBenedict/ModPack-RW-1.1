using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace rjw
{
	public abstract class JobDriver_GatherHumanBodyResources : JobDriver_GatherAnimalBodyResources
	{
		private float gatherProgress;

		/*
		//maybe change?
		protected abstract int GatherResourcesIntervalDays
		{
			get;
		}

		//add breastsize modifier?
		protected abstract int ResourceAmount
		{
			get;
		}
		//add more  milks?
		protected abstract ThingDef ResourceDef
		{
			get;
		}
		*/

		protected override IEnumerable<Toil> MakeNewToils()
		{
			ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_GatherHumanBodyResources>(this, TargetIndex.A);
			ToilFailConditions.FailOnNotCasualInterruptible<JobDriver_GatherHumanBodyResources>(this, TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil wait = new Toil();
			wait.initAction = delegate
			{
				Pawn milker = base.pawn;
				LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
				Pawn target2 = (Pawn)target.Thing;
				milker.pather.StopDead();
				PawnUtility.ForceWait(target2, 15000, null, true);
			};
			wait.tickAction = delegate
			{
				Pawn milker = base.pawn;
				milker.skills.Learn(SkillDefOf.Animals, 0.13f, false);
				gatherProgress += StatExtension.GetStatValue(milker, StatDefOf.AnimalGatherSpeed, true);
				if (gatherProgress >= WorkTotal)
				{
					GetComp((Pawn)base.job.GetTarget(TargetIndex.A)).Gathered(base.pawn);
					milker.jobs.EndCurrentJob(JobCondition.Succeeded, true);
				}
			};
			wait.AddFinishAction((Action)delegate
			{
				Pawn milker = base.pawn;
				LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
				Pawn target2 = (Pawn)target.Thing;
				if (target2 != null && target2.CurJobDef == JobDefOf.Wait_MaintainPosture)
				{
					milker.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			});
			ToilFailConditions.FailOnDespawnedOrNull<Toil>(wait, TargetIndex.A);
			ToilFailConditions.FailOnCannotTouch<Toil>(wait, TargetIndex.A, PathEndMode.Touch);
			wait.AddEndCondition((Func<JobCondition>)delegate
			{
				if (GetComp((Pawn)base.job.GetTarget(TargetIndex.A)).ActiveAndFull)
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Incompletable;
			});
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			ToilEffects.WithProgressBar(wait, TargetIndex.A, (Func<float>)(() => gatherProgress / WorkTotal), false, -0.5f);
			wait.activeSkill = (() => SkillDefOf.Animals);
			yield return wait;
		}
	}
}

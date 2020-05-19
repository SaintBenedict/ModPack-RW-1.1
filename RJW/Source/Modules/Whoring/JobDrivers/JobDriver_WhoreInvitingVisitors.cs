using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace rjw
{
	public class JobDriver_WhoreInvitingVisitors : JobDriver
	{
		// List of jobs that can be interrupted by whores. 
		public static readonly List<JobDef> allowedJobs = new List<JobDef> { null, JobDefOf.Wait_Wander, JobDefOf.GotoWander, JobDefOf.Clean, JobDefOf.ClearSnow,
			JobDefOf.CutPlant, JobDefOf.HaulToCell, JobDefOf.Deconstruct, JobDefOf.Harvest, JobDefOf.LayDown, JobDefOf.Research, JobDefOf.SmoothFloor, JobDefOf.SmoothWall,
			JobDefOf.SocialRelax, JobDefOf.StandAndBeSociallyActive, JobDefOf.RemoveApparel, JobDefOf.Strip, JobDefOf.Tame, JobDefOf.Wait, JobDefOf.Wear, JobDefOf.FixBrokenDownBuilding,
			JobDefOf.FillFermentingBarrel, JobDefOf.DoBill, JobDefOf.Sow, JobDefOf.Shear, JobDefOf.BuildRoof, JobDefOf.DeliverFood,	JobDefOf.HaulToContainer, JobDefOf.Hunt, JobDefOf.Mine,
			JobDefOf.OperateDeepDrill, JobDefOf.OperateScanner, JobDefOf.RearmTurret, JobDefOf.Refuel, JobDefOf.RefuelAtomic, JobDefOf.RemoveFloor, JobDefOf.RemoveRoof, JobDefOf.Repair,
			JobDefOf.TakeBeerOutOfFermentingBarrel, JobDefOf.Train, JobDefOf.Uninstall, xxx.Masturbate_Bed};
			
		public bool successfulPass = true;

		private Pawn Whore => GetActor();
		private Pawn TargetPawn => TargetThingA as Pawn;
		private Building_Bed TargetBed => TargetThingB as Building_Bed;

		private readonly TargetIndex TargetPawnIndex = TargetIndex.A;
		private readonly TargetIndex TargetBedIndex = TargetIndex.B;

		private bool DoesTargetPawnAcceptAdvance()
		{
			if (RJWSettings.DebugWhoring) Log.Message($"JobDriver_InvitingVisitors::DoesTargetPawnAcceptAdvance() - {xxx.get_pawnname(TargetPawn)}");
			//if (RJWSettings.WildMode) return true;

			if (PawnUtility.EnemiesAreNearby(TargetPawn))
			{
				if (RJWSettings.DebugWhoring) Log.Message($" fail - enemy near");
				return false;
			}
			if (!allowedJobs.Contains(TargetPawn.jobs.curJob.def))
			{
				if (RJWSettings.DebugWhoring) Log.Message($" fail - not allowed job");
				return false;
			}

			if (RJWSettings.DebugWhoring)
			{
				Log.Message("Will try hookup " + WhoringHelper.WillPawnTryHookup(TargetPawn));
				Log.Message("Is whore appealing  " + WhoringHelper.IsHookupAppealing(TargetPawn, Whore));
				Log.Message("Can afford whore " + WhoringHelper.CanAfford(TargetPawn, Whore));
				Log.Message("Need sex " + (xxx.need_some_sex(TargetPawn) >= 1));
			}
			if (WhoringHelper.WillPawnTryHookup(TargetPawn) && WhoringHelper.IsHookupAppealing(TargetPawn, Whore) && WhoringHelper.CanAfford(TargetPawn, Whore) && xxx.need_some_sex(TargetPawn) >= 1f)
			{
				Whore.skills.Learn(SkillDefOf.Social, 1.2f);
				return true;
			}
			return false;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetPawnIndex);
			this.FailOnDespawnedNullOrForbidden(TargetBedIndex);
			this.FailOn(() => Whore is null || !xxx.CanUse(Whore, TargetBed));//|| !Whore.CanReserve(TargetPawn)
			this.FailOn(() => pawn.Drafted);
			yield return Toils_Goto.GotoThing(TargetPawnIndex, PathEndMode.Touch);

			Toil TryItOn = new Toil();
			TryItOn.AddFailCondition(() => !xxx.IsTargetPawnOkay(TargetPawn));
			TryItOn.defaultCompleteMode = ToilCompleteMode.Delay;
			TryItOn.initAction = delegate
			{
				//Log.Message("[RJW]JobDriver_InvitingVisitors::MakeNewToils - TryItOn - initAction is called");
				Whore.jobs.curDriver.ticksLeftThisToil = 50;
				MoteMaker.ThrowMetaIcon(Whore.Position, Whore.Map, ThingDefOf.Mote_Heart);
			};
			yield return TryItOn;

			Toil AwaitResponse = new Toil();
			AwaitResponse.AddFailCondition(() => !successfulPass);
			AwaitResponse.defaultCompleteMode = ToilCompleteMode.Instant;
			AwaitResponse.initAction = delegate
			{
				List<RulePackDef> extraSentencePacks = new List<RulePackDef>();
				successfulPass = DoesTargetPawnAcceptAdvance();
				//Log.Message("[RJW]JobDriver_InvitingVisitors::MakeNewToils - AwaitResponse - initAction is called");
				if (successfulPass)
				{
					MoteMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, ThingDefOf.Mote_Heart);
                    TargetPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					if (xxx.RomanceDiversifiedIsActive)
					{
						extraSentencePacks.Add(RulePackDef.Named("HookupSucceeded"));
					}
					if (Whore.health.HasHediffsNeedingTend())
					{
						successfulPass = false;
						const string key = "RJW_VisitorSickWhore";
						string text = key.Translate(TargetPawn.LabelIndefinite(), Whore.LabelIndefinite()).CapitalizeFirst();
						Messages.Message(text, Whore, MessageTypeDefOf.TaskCompletion);
					}
					else
					{
						const string key = "RJW_VisitorAcceptWhore";
						string text = key.Translate(TargetPawn.LabelIndefinite(), Whore.LabelIndefinite()).CapitalizeFirst();
						Messages.Message(text, TargetPawn, MessageTypeDefOf.TaskCompletion);
					}
				}
				else
				{
					MoteMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, ThingDefOf.Mote_IncapIcon);
					TargetPawn.needs.mood.thoughts.memories.TryGainMemory(
						TargetPawn.Faction == Whore.Faction
							? ThoughtDef.Named("RJWTurnedDownWhore")
							: ThoughtDef.Named("RJWFailedSolicitation"), Whore);

					if (xxx.RomanceDiversifiedIsActive)
					{
						Whore.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("RebuffedMyHookupAttempt"), TargetPawn);
						TargetPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("FailedHookupAttemptOnMe"), Whore);
						extraSentencePacks.Add(RulePackDef.Named("HookupFailed"));
					}
					//Disabled rejection notifications
					//Messages.Message("RJW_VisitorRejectWhore".Translate(new object[] { xxx.get_pawnname(TargetPawn), xxx.get_pawnname(Whore) }), TargetPawn, MessageTypeDefOf.SilentInput);
				}
				if (xxx.RomanceDiversifiedIsActive)
				{
					Find.PlayLog.Add(new PlayLogEntry_Interaction(DefDatabase<InteractionDef>.GetNamed("TriedHookupWith"), pawn, TargetPawn, extraSentencePacks));
				}
			};
			yield return AwaitResponse;

			Toil BothGoToBed = new Toil();
			BothGoToBed.AddFailCondition(() => !successfulPass || !xxx.CanUse(Whore, TargetBed));
			BothGoToBed.defaultCompleteMode = ToilCompleteMode.Instant;
			BothGoToBed.initAction = delegate
			{
				//Log.Message("[RJW]JobDriver_InvitingVisitors::MakeNewToils - BothGoToBed - initAction is called0");
				if (!successfulPass) return;
				if (!xxx.CanUse(Whore, TargetBed) && Whore.CanReserve(TargetPawn, 1, 0))
				{
					//Log.Message("[RJW]JobDriver_InvitingVisitors::MakeNewToils - BothGoToBed - cannot use the whore bed");
					return;
				}
				//Log.Message("[RJW]JobDriver_InvitingVisitors::MakeNewToils - BothGoToBed - initAction is called1");
				Whore.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(xxx.whore_is_serving_visitors, TargetPawn, TargetBed));
				//TargetPawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("WhoreIsServingVisitors"), Whore, TargetBed, (TargetBed.MaxAssignedPawnsCount>1)?TargetBed.GetSleepingSlotPos(1): TargetBed.)), null);
				Whore.jobs.curDriver.EndJobWith(JobCondition.InterruptOptional);
				//TargetPawn.jobs.curDriver.EndJobWith(JobCondition.InterruptOptional);
			};
			yield return BothGoToBed;
		}
	}
}
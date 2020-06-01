using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Multiplayer.API;

namespace rjw
{
	/// <summary>
	/// This is the driver for animals mounting breeders.
	/// </summary>
	public class JobDriver_Breeding : JobDriver_Rape
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Target, job, BreederHelper.max_animals_at_once, 0);
		}

		[SyncMethod]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			setup_ticks();

			//--Log.Message("JobDriver_Breeding::MakeNewToils() - setting fail conditions");
			this.FailOnDespawnedNullOrForbidden(iTarget);
			this.FailOn(() => !pawn.CanReserve(Partner, BreederHelper.max_animals_at_once, 0)); // Fail if someone else reserves the target before the animal arrives.
			this.FailOn(() => !pawn.CanReach(Partner, PathEndMode.Touch, Danger.Some)); // Fail if animal cannot reach target.
			this.FailOn(() => pawn.Drafted);

			// Path to target
			yield return Toils_Goto.GotoThing(iTarget, PathEndMode.OnCell);

			//if (!(pawn.IsDesignatedBreedingAnimal() && Partner.IsDesignatedBreeding()));
			if (!(pawn.IsAnimal() && Partner.IsAnimal()))
				SexUtility.RapeTargetAlert(pawn, Partner);

			Toil StartPartnerJob = new Toil();
			StartPartnerJob.defaultCompleteMode = ToilCompleteMode.Instant;
			StartPartnerJob.socialMode = RandomSocialMode.Off;
			StartPartnerJob.initAction = delegate
			{
				var dri = Partner.jobs.curDriver as JobDriver_SexBaseRecieverRaped;
				if (dri == null)
				{
					Job gettin_raped = JobMaker.MakeJob(xxx.gettin_raped, pawn);
					Building_Bed Bed = null;
					if (Partner.GetPosture() == PawnPosture.LayingInBed)
						Bed = Partner.CurrentBed();

					Partner.jobs.StartJob(gettin_raped, JobCondition.InterruptForced, null, false, true, null);
					if (Bed != null)
						(Partner.jobs.curDriver as JobDriver_SexBaseRecieverRaped)?.Set_bed(Bed);
				}
			};
			yield return StartPartnerJob;

			// Breed target
			var breed = new Toil();
			breed.defaultCompleteMode = ToilCompleteMode.Delay;
			breed.defaultDuration = duration;
			breed.handlingFacing = true;
			breed.initAction = delegate
			{
				Start();
			};
			breed.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(ticks_between_hearts))
					if (xxx.is_zoophile(pawn) || xxx.is_animal(pawn))
						ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
					else
						ThrowMetaIcon(pawn.Position, pawn.Map, xxx.mote_noheart);
				if (pawn.IsHashIntervalTick(ticks_between_hits) && !xxx.is_zoophile(Partner))
					Roll_to_hit(pawn, Partner);
				SexTick(pawn, Partner);
				if (!Partner.Dead)
					SexUtility.reduce_rest(Partner, 1);
				SexUtility.reduce_rest(pawn, 2);
			};
			breed.AddFinishAction(delegate
			{
				End();
			});
			yield return breed;

			yield return new Toil
			{
				initAction = delegate
				{
					//Log.Message("JobDriver_Breeding::MakeNewToils() - Calling aftersex");
					//// Trying to add some interactions and social logs
					bool isRape = !(pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, Partner) ||
					 	(xxx.is_animal(pawn) && (pawn.RaceProps.wildness - pawn.RaceProps.petness + 0.18f) > Rand.Range(0.36f, 1.8f)));
					SexUtility.ProcessSex(pawn, Partner, usedCondom: usedCondom, rape: isRape, sextype: sexType);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}

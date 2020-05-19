using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using Multiplayer.API;

namespace rjw
{
	public abstract class JobDriver_Sex : JobDriver
	{

		public readonly TargetIndex iTarget = TargetIndex.A;	//pawn or corpse
		public readonly TargetIndex iBed = TargetIndex.B;		//bed(maybe some furniture in future?)
		public readonly TargetIndex iCell = TargetIndex.C;		//cell/location to have sex at(fapping)

		public float satisfaction = 1.0f;

		public bool shouldreserve = true;

		public int stackCount = 0;

		public int ticks_between_hearts = 60;
		public int ticks_between_hits = 60;
		public int ticks_between_thrusts = 60;
		public int ticks_left = 1000;
		public int duration = 5000;
		public int ticks_remaining = 10;

		public bool usedCondom = false;
		public bool isRape = false;
		public bool isWhoring = false;
		public bool face2face = false;

		public Thing Target         // for reservation
		{
			get
			{
				return (Thing)job.GetTarget(TargetIndex.A);
			}
		}
		public Pawn Partner
		{
			get
			{
				if (Target is Pawn)
					return (Pawn)job.GetTarget(TargetIndex.A);
				else if (Target is Corpse)
					return ((Corpse)job.GetTarget(TargetIndex.A)).InnerPawn;
				else
					return null;
			}
		}

		public Building_Bed Bed
		{
			get
			{
				if (pBed != null)
					return pBed;
				else if ((Thing)job.GetTarget(TargetIndex.B) is Building_Bed)
					return (Building_Bed)job.GetTarget(TargetIndex.B);
				else
					return null;
			}
		}
		//not bed; chair, maybe something else in future
		public Building Building
		{
			get
			{
				if ((Thing)job.GetTarget(TargetIndex.B) is Building && !((Thing)job.GetTarget(TargetIndex.B) is Building_Bed))
					return (Building)job.GetTarget(TargetIndex.B);
				else
					return null;
			}
		}

		public Building_Bed pBed = null;

		//public SexProps props;
		public xxx.rjwSextype sexType = xxx.rjwSextype.None;

		[SyncMethod]
		public void setup_ticks()
		{
			ticks_left = (int)(2000.0f * Rand.Range(0.50f, 0.90f));
			ticks_between_hearts = Rand.RangeInclusive(70, 130);
			ticks_between_hits = Rand.Range(xxx.config.min_ticks_between_hits, xxx.config.max_ticks_between_hits);
			if (xxx.is_bloodlust(pawn))
				ticks_between_hits = (int)(ticks_between_hits * 0.75);
			if (xxx.is_brawler(pawn))
				ticks_between_hits = (int)(ticks_between_hits * 0.90);
			ticks_between_thrusts = 120;
			duration = ticks_left;
		}

		public void increase_time(int min_ticks_remaining)
		{
			if (min_ticks_remaining > ticks_remaining)
				ticks_remaining = min_ticks_remaining;
		}

		public void Set_bed(Building_Bed newBed)
		{
			pBed = newBed;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticks_left, "ticks_left", 0, false);
			Scribe_Values.Look(ref ticks_between_hearts, "ticks_between_hearts", 0, false);
			Scribe_Values.Look(ref ticks_between_hits, "ticks_between_hits", 0, false);
			Scribe_Values.Look(ref ticks_between_thrusts, "ticks_between_thrusts", 0, false);
			Scribe_Values.Look(ref duration, "duration", 0, false);
			Scribe_Values.Look(ref ticks_remaining, "ticks_remaining", 10, false);

			Scribe_References.Look(ref pBed, "pBed");
			//Scribe_Values.Look(ref props, "props");
			Scribe_Values.Look(ref usedCondom, "usedCondom");
			Scribe_Values.Look(ref isRape, "isRape");
			Scribe_Values.Look(ref isWhoring, "isWhoring");
			Scribe_Values.Look(ref sexType, "sexType");
			Scribe_Values.Look(ref face2face, "face2face");
		}

		public void SexTick(Pawn pawn, Thing target, bool pawnnude = true, bool partnernude = true)
		{
			var partner = target as Pawn;

			if (pawn.IsHashIntervalTick(ticks_between_thrusts))
			{
				Animate(pawn, partner);
				PlaySexSound();
				if (!isRape)
				{
					pawn.GainComfortFromCellIfPossible();
					if (partner != null)
						partner.GainComfortFromCellIfPossible();
				}
			}

			//refresh DrawNude after beating and Notify_MeleeAttackOn
			// Endytophiles prefer clothed sex, everyone else gets nude.
			if (!xxx.has_quirk(pawn, "Endytophile"))
			{
				if (pawnnude)
				{
					SexUtility.DrawNude(pawn);
				}

				if (partner != null)
					if (partnernude)
					{
						SexUtility.DrawNude(partner);
					}
			}
		}

		/// <summary>
		/// simple rjw thrust animation
		/// </summary>
		public void Animate(Pawn pawn, Thing target)
		{
			RotatePawns(pawn, Partner);
			//attack/ride 1x2 cell cocksleeve/dildo?
			//if (Building != null)
			//	target = Building;
			if (target != null)
			{
				pawn.Drawer.Notify_MeleeAttackOn(target);

				var partner = target as Pawn;
				if (partner != null && !isRape)
					partner.Drawer.Notify_MeleeAttackOn(pawn);
			}
		}

		/// <summary>
		/// rotate pawns
		/// </summary>
		public void RotatePawns(Pawn pawn, Thing target)
		{
			if (Building != null)
			{
				if (face2face)
					pawn.Rotation = Building.Rotation.Opposite;
				else
					pawn.Rotation = Building.Rotation;

				return;
			}
			if (target == null) // solo
			{
				//pawn.Rotation = Rot4.South;
				return;
			}
			var partner = target as Pawn;
			if (partner == null || partner.Dead) // necro
			{
				pawn.rotationTracker.Face(target.DrawPos);
				return;
			}
			if (((JobDriver_SexBaseReciever)partner.jobs.curDriver as JobDriver_SexBaseReciever).parteners.Count > 1) return;

			//maybe could do a hand check for monster girls but w/e
			//bool partnerHasHands = Receiver.health.hediffSet.GetNotMissingParts().Any(part => part.IsInGroup(BodyPartGroupDefOf.RightHand) || part.IsInGroup(BodyPartGroupDefOf.LeftHand));

			// most of animal sex is likely doggystyle.
			if (xxx.is_animal(pawn) && xxx.is_animal(partner))
			{
				if (sexType == xxx.rjwSextype.Anal || sexType == xxx.rjwSextype.Vaginal || sexType == xxx.rjwSextype.DoublePenetration)
				{
					//>>
					//Log.Message("animal doggy");
					pawn.rotationTracker.Face(partner.DrawPos);
					partner.Rotation = pawn.Rotation;
				}
				else
				{
					//><
					//Log.Message("animal non doggy");
					pawn.rotationTracker.Face(target.DrawPos);
					partner.rotationTracker.Face(pawn.DrawPos);
				}
			}
			else
			{
				if (this is JobDriver_BestialityForFemale)
				{
					if (sexType == xxx.rjwSextype.Anal || sexType == xxx.rjwSextype.Vaginal || sexType == xxx.rjwSextype.DoublePenetration)
					{
						//<<
						//Log.Message("bestialityFF doggy");
						partner.rotationTracker.Face(pawn.DrawPos);
						pawn.Rotation = partner.Rotation;
					}
					else
					{
						//><
						//Log.Message("bestialityFF non doggy");
						pawn.rotationTracker.Face(target.DrawPos);
						partner.rotationTracker.Face(pawn.DrawPos);
					}
				}
				else if (partner.GetPosture() == PawnPosture.LayingInBed)
				{
					//x^
					//Log.Message("loving/casualsex in bed");
					// this could use better handling for cowgirl/reverse cowgirl and who pen who, if such would be implemented
					//until then...

					if (!face2face && sexType == xxx.rjwSextype.Anal ||
										sexType == xxx.rjwSextype.Vaginal ||
										sexType == xxx.rjwSextype.DoublePenetration ||
										sexType == xxx.rjwSextype.Fisting)
						//if (xxx.is_female(pawn) && xxx.is_female(partner))
					{
						// in bed loving face down
						pawn.Rotation = partner.CurrentBed().Rotation.Opposite;
					}
					//else if (!(xxx.is_male(pawn) && xxx.is_male(partner)))
					//{
					//	// in bed loving face down
					//	pawn.Rotation = partner.CurrentBed().Rotation.Opposite;
					//}
					else
					{
						// in bed loving, face up
						pawn.Rotation = partner.CurrentBed().Rotation;
					}
				}
				// 30% chance of face-to-face regardless, for variety.
				else if (!face2face && (sexType == xxx.rjwSextype.Anal || 
										sexType == xxx.rjwSextype.Vaginal || 
										sexType == xxx.rjwSextype.DoublePenetration || 
										sexType == xxx.rjwSextype.Fisting))
				{
					//>>
					//Log.Message("doggy");
					pawn.rotationTracker.Face(target.DrawPos);
					partner.Rotation = pawn.Rotation;
				}
				// non doggystyle, or face-to-face regardless
				else
				{
					//><
					//Log.Message("non doggy");
					pawn.rotationTracker.Face(target.DrawPos);
					partner.rotationTracker.Face(pawn.DrawPos);
				}
			}
		}

		[SyncMethod]
		public void Rollface2face(float chance = 0.3f)
		{
			Setface2face(Rand.Chance(chance));
		}
		public void Setface2face(bool chance)
		{
			face2face = chance;
		}

		public static void Roll_to_hit(Pawn Pawn, Pawn Partner, bool isRape = true)
		{
			SexUtility.Sex_Beatings(Pawn, Partner, isRape);
		}
		public void ThrowMetaIcon(IntVec3 pos, Map map, ThingDef icon)
		{
			MoteMaker.ThrowMetaIcon(pos, map, icon);
		}
		public void PlaySexSound()
		{
			if (RJWSettings.sounds_enabled)
				SoundDef.Named("Sex").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
		}
		public void PlayCumSound()
		{
			if (RJWSettings.sounds_enabled)
				SoundDef.Named("Cum").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
		}
		public void PlaySexVoice()
		{
			//if (RJWSettings.sounds_enabled)
			//{
			//	SoundDef.Named("Sex").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			//}
		}
		public void PlayOrgasmVoice()
		{
			//if (RJWSettings.sounds_enabled)
			//{
			//	SoundDef.Named("Sex").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			//}
		}
		public void CalculateSatisfactionPerTick()
		{
			satisfaction = 1.0f;
		}

		public static bool IsInOrByBed(Building_Bed b, Pawn p)
		{
			for (int i = 0; i < b.SleepingSlotsCount; i++)
			{
				if (b.GetSleepingSlotPos(i).InHorDistOf(p.Position, 1f))
				{
					return true;
				}
			}
			return false;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true; // No reservations needed.
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			return null;
		}
	}
}
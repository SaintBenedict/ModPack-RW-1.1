using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Text;
using Verse.AI.Group;
using Multiplayer.API;
using System.Linq;

namespace rjw
{
	public class Hediff_InsectEgg : HediffWithComps
	{
		public int bornTick = 5000;
		public int abortTick = 0;

		public string parentDef
		{
			get
			{
				return ((HediffDef_InsectEgg)def).parentDef;
			}
		}

		public List<string> parentDefs
		{
			get
			{
				return ((HediffDef_InsectEgg)def).parentDefs;
			}
		}

		public Pawn father;					//can be parentkind defined in egg
		public Pawn implanter;					//can be any pawn
		public bool canbefertilized = true;
		public bool fertilized => father != null;
		public float eggssize = 0.1f;
		protected List<Pawn> babies;

		///Contractions duration, effectively additional hediff stage, a dirty hack to make birthing process notable
		//protected const int TicksPerHour = 2500;
		protected int contractions = 0;

		public override string LabelBase
		{
			get
			{
				if (Prefs.DevMode)
				{
					if (father != null)
						return father.kindDef.race.label + " egg";
					else if (implanter != null)
						return implanter.kindDef.race.label + " egg";
				}

				if (eggssize <= 0.10f)
					return "Small egg";
				if (eggssize <= 0.3f)
					return "Medium egg";
				else if (eggssize <= 0.5f)
					return "Big egg";
				else
					return "Huge egg";

				//return Label;
			}
		}

		public override string LabelInBrackets
		{
			get
			{
				if (Prefs.DevMode)
				{
					if (fertilized)
						return "Fertilized";
					else
						return "Unfertilized";
				}
				return null;
			}
		}

		public float GestationProgress
		{
			get => Severity;
			private set => Severity = value;
		}

		public override bool TryMergeWith(Hediff other)
		{
			return false;
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			//--Log.Message("[RJW]Hediff_InsectEgg::PostAdd() - added parentDef:" + parentDef+"");
			base.PostAdd(dinfo);
		}

		public override void Tick()
		{
			ageTicks++;
			if (pawn.IsHashIntervalTick(1000))
			{
				//birthing takes an hour
				if (ageTicks >= bornTick - 2500 && contractions == 0)
				{
					if (PawnUtility.ShouldSendNotificationAbout(pawn) && (pawn.IsColonist || pawn.IsPrisonerOfColony))
					{
						string key = "RJW_EggContractions";
						string text = TranslatorFormattedStringExtensions.Translate(key, pawn.LabelIndefinite());
						Messages.Message(text, pawn, MessageTypeDefOf.NeutralEvent);
					}
					contractions++;
					pawn.health.AddHediff(HediffDef.Named("Hediff_Submitting"));
				}

				if (ageTicks >= bornTick && (pawn.CarriedBy == null || pawn.CarriedByCaravan()))
				{
					if (PawnUtility.ShouldSendNotificationAbout(pawn) && (pawn.IsColonist || pawn.IsPrisonerOfColony))
					{
						string key1 = "RJW_GaveBirthEggTitle";
						string message_title = TranslatorFormattedStringExtensions.Translate(key1, pawn.LabelIndefinite());
						string key2 = "RJW_GaveBirthEggText";
						string message_text = TranslatorFormattedStringExtensions.Translate(key2, pawn.LabelIndefinite());
						//Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.NeutralEvent, pawn, null);
						Messages.Message(message_text, pawn, MessageTypeDefOf.SituationResolved);
					}
					GiveBirth();
					//someday add dmg to vag?
					//var dam = Rand.RangeInclusive(0, 1);
					//p.TakeDamage(new DamageInfo(DamageDefOf.Burn, dam, 999, -1.0f, null, rec, null));
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref bornTick, "bornTick");
			Scribe_Values.Look<int>(ref abortTick, "abortTick");
			Scribe_References.Look<Pawn>(ref father, "father", false);
			Scribe_References.Look<Pawn>(ref implanter, "implanter", false);
			Scribe_Collections.Look(ref babies, saveDestroyedThings: true, label: "babies", lookMode: LookMode.Deep, ctorArgs: new object[0]);
		}
		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			GiveBirth();
		}

		protected virtual void GenerateBabies()
		{

		}

		//should someday remake into birth eggs and then within few ticks hatch them
		[SyncMethod]
		public void GiveBirth()
		{
			Pawn mother = pawn;
			Pawn baby = null;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			if (fertilized)
			{
				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - Egg of " + parentDef + " in " + mother.ToString() + " birth!");
				PawnKindDef spawn_kind_def = father.kindDef;
				//egg mostlikely insect or implanter spawned factionless through debug, set to insect
				Faction spawn_faction = Faction.OfInsects;

				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - insect " + (implanter.Faction == Faction.OfInsects || father.Faction == Faction.OfInsects || mother.Faction == Faction.OfInsects));
				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - human " + (xxx.is_human(implanter) && xxx.is_human(father)));
				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - animal1 " + (!xxx.is_human(implanter) && !(implanter.Faction?.IsPlayer ?? false)));
				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - animal2 ");

				//this is probably fucked up, idk how to filter insects from non insects/spiders etc
				//core Hive Insects... probably
				if (implanter.Faction == Faction.OfInsects || father.Faction == Faction.OfInsects || mother.Faction == Faction.OfInsects)
				{
					//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() - insect ");
					spawn_faction = Faction.OfInsects;
					int chance = 5;

					//random chance to make insect neutral/tamable
					if (father.Faction == Faction.OfInsects)
						chance = 5;
					if (father.Faction != Faction.OfInsects)
						chance = 10;
					if (father.Faction == Faction.OfPlayer)
						chance = 25;
					if (implanter.Faction == Faction.OfPlayer)
						chance += 25;
					if (implanter.Faction == Faction.OfPlayer && xxx.is_human(implanter))
						chance += (int)(25 * implanter.GetStatValue(StatDefOf.PsychicSensitivity));
					if (Rand.Range(0, 100) <= chance)
						spawn_faction = null;

					//chance tame insect on birth 
					if (spawn_faction == null)
						if (implanter.Faction == Faction.OfPlayer && xxx.is_human(implanter))
							if (Rand.Range(0, 100) <= (int)(50 * implanter.GetStatValue(StatDefOf.PsychicSensitivity)))
								spawn_faction = Faction.OfPlayer;
				}
				//humanlikes
				else if (xxx.is_human(implanter) && xxx.is_human(father))
				{
					spawn_faction = implanter.Faction;
				}
				//TODO: humnlike + animal, merge with insect stuff?
				//else if (xxx.is_human(implanter) && !xxx.is_human(father))
				//{
				//	spawn_faction = implanter.Faction;
				//}
				//animal, spawn implanter faction (if not player faction/not tamed)
				else if (!xxx.is_human(implanter) && !(implanter.Faction?.IsPlayer ?? false))
				{
					spawn_faction = implanter.Faction;
				}
				//spawn factionless(tamable, probably)
				else
				{
					spawn_faction = null;
				}

				if (spawn_kind_def.defName == "Nymph")
					spawn_kind_def = PawnKindDefOf.Colonist;

				//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() " + spawn_kind_def + " of " + spawn_faction + " in " + (int)(50 * implanter.GetStatValue(StatDefOf.PsychicSensitivity)) + " chance!");
				PawnGenerationRequest request = new PawnGenerationRequest(
					kind: spawn_kind_def,
					faction: spawn_faction,
					forceGenerateNewPawn: true,
					newborn: true,
					canGeneratePawnRelations: false,
					colonistRelationChanceFactor: 0,
					allowFood: false,
					allowAddictions: false,
					relationWithExtraPawnChanceFactor: 0
					);

				baby = PawnGenerator.GeneratePawn(request);

				Hediff_BasePregnancy.BabyPostBirth(mother, father, baby);
				Sexualizer.sexualize_pawn(baby);

				if (spawn_faction == Faction.OfInsects || (spawn_faction != null && (spawn_faction.def.defName.Contains("insect") || spawn_faction == implanter.Faction)))
				{
					//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() GetLord");
					//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() " + implanter.GetLord());
					//add ai to pawn?
					//LordManager.lords
					Lord lord = implanter.GetLord();
					if (lord != null)
					{
						//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() lord: " +lord);
						//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() LordJob: " + lord.LordJob);
						//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() CurLordToil: " + lord.CurLordToil);
						lord.AddPawn(baby);
					}
					else
					{
						//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() lord null");
						//LordJob_DefendAndExpandHive lordJob = new LordJob_DefendAndExpandHive();
						//lord = LordMaker.MakeNewLord(baby.Faction, lordJob, mother.Map);
						//lord.AddPawn(baby);
						//lord.SetJob(lordJob);
					}
					//Log.Message("[RJW]Hediff_InsectEgg::BirthBaby() " + baby.GetLord().DebugString());
				}

				if (PawnUtility.TrySpawnHatchedOrBornPawn(baby, mother))
				{
				}
				else
				{
					if (spawn_faction == mother.Faction)
					{
						if (mother.IsPlayerControlledCaravanMember())
							mother.GetCaravan().AddPawn(baby, true);
						else if (mother.IsCaravanMember())
							mother.GetCaravan().AddPawn(baby, false);
					}
					else
						Find.WorldPawns.PassToWorld(baby, PawnDiscardDecideMode.Discard);
				}

				// Move the baby in front of the mother, rather than on top
				if (mother.Spawned)
					if (mother.CurrentBed() != null)
					{
						baby.Position = baby.Position + new IntVec3(0, 0, 1).RotatedBy(mother.CurrentBed().Rotation);
					}

					/*
					if (Visible && baby != null)
					{
						string key = "MessageGaveBirth";
						string text = TranslatorFormattedStringExtensions.Translate(key, mother.LabelIndefinite()).CapitalizeFirst();
						Messages.Message(text, baby, MessageTypeDefOf.NeutralEvent);
					}
					*/

				mother.records.AddTo(xxx.CountOfBirthEgg, 1);

				if (mother.records.GetAsInt(xxx.CountOfBirthEgg) > 100)
				{
					mother.Add(Quirk.Incubator);
					mother.Add(Quirk.ImpregnationFetish);
				}
			}
			else
			{
				if (PawnUtility.ShouldSendNotificationAbout(pawn) && (pawn.IsColonist || pawn.IsPrisonerOfColony))
				{
					string key = "EggDead";
					string text = TranslatorFormattedStringExtensions.Translate(key, pawn.LabelIndefinite()).CapitalizeFirst();
					Messages.Message(text, pawn, MessageTypeDefOf.SituationResolved);
				}
			}
			// Post birth
			if (mother.Spawned)
			{
				// Spawn guck
				if (mother.caller != null)
				{
					mother.caller.DoCall();
				}
				if (baby != null)
				{
					if (baby.caller != null)
					{
						baby.caller.DoCall();
					}
				}
				FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5);
				int howmuch = xxx.has_quirk(mother, "Incubator") ? Rand.Range(1, 3) * 2 : Rand.Range(1, 3);
				int i = 0;
				if (xxx.is_insect(baby) || xxx.is_insect(mother))
				{
					while (i++ < howmuch)
					{
						Thing jelly = ThingMaker.MakeThing(ThingDefOf.InsectJelly);
						jelly.SetForbidden(true, false);
						GenPlace.TryPlaceThing(jelly, mother.InteractionCell, mother.Map, ThingPlaceMode.Direct);
					}
				}
			}

			mother.health.RemoveHediff(this);
		}

		//set father/final egg type
		public void Fertilize(Pawn Pawn)
		{
			if (xxx.ImmortalsIsActive && (Pawn.health.hediffSet.HasHediff(xxx.IH_Immortal) || pawn.health.hediffSet.HasHediff(xxx.IH_Immortal)))
			{
				return;
			}
			if (!AndroidsCompatibility.IsAndroid(pawn))
				if (!fertilized && canbefertilized && ageTicks < abortTick)
				{
					if (RJWSettings.DevMode) Log.Message(xxx.get_pawnname(pawn) + " fertilize eggs:" + this.ToString());
					father = Pawn;
					ChangeEgg(father);
				}
		}

		//set implanter/base egg type
		public void Implanter(Pawn Pawn)
		{
			if (implanter == null)
			{
				if (RJWSettings.DevMode) Log.Message("Hediff_InsectEgg:: set implanter:" + xxx.get_pawnname(Pawn));
				implanter = Pawn;
				ChangeEgg(implanter);

				if (!implanter.health.hediffSet.HasHediff(xxx.sterilized))
				{
					if (((HediffDef_InsectEgg)def).selffertilized)
						Fertilize(implanter);
				}
				else
					canbefertilized = false;
			}
		}

		//Change egg type after implanting/fertilizing
		public void ChangeEgg(Pawn Pawn)
		{
			if (Pawn != null)
			{
				eggssize = Pawn.RaceProps.baseBodySize / 5;
				float gestationPeriod = Pawn.RaceProps?.gestationPeriodDays * 60000 ?? 450000;
				//float gestationPeriod = 10000 ;
				gestationPeriod = (xxx.has_quirk(pawn, "Incubator") || pawn.health.hediffSet.HasHediff(HediffDef.Named("FertilityEnhancer"))) ? gestationPeriod / 2 : gestationPeriod;
				bornTick = (int)(gestationPeriod * Pawn.RaceProps.baseBodySize + 0.5f);
				abortTick = (int)(bornTick / 3);
				if (!Genital_Helper.has_ovipositorF(pawn))
					Severity = eggssize;
				else if (eggssize > 0.1f)
					Severity = 0.1f;
				else
					Severity = eggssize;
			}
		}

		//for setting implanter/fertilize eggs
		public bool IsParent(Pawn parent)
		{
			//anyone can fertilize
			if (RJWPregnancySettings.egg_pregnancy_fertilize_anyone) return true;

			//only set egg parent or implanter can fertilize
			else return parentDef == parent.kindDef.defName
						|| parentDefs.Contains(parent.kindDef.defName)
						|| implanter.kindDef == parent.kindDef; // unknown eggs
		}

		public override string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.DebugString());
			stringBuilder.AppendLine(" Gestation progress: " + ((float)ageTicks / bornTick).ToStringPercent());
			if (RJWSettings.DevMode) stringBuilder.AppendLine(" Implanter: " + xxx.get_pawnname(implanter));
			if (RJWSettings.DevMode) stringBuilder.AppendLine(" Father: " + xxx.get_pawnname(father));
			if (RJWSettings.DevMode) stringBuilder.AppendLine(" bornTick: " + bornTick);
			//stringBuilder.AppendLine(" potential father: " + parentDef);
			return stringBuilder.ToString();
		}
	}
}
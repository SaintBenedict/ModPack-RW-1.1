using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Text;
using Multiplayer.API;
using System.Linq;
using RimWorld.Planet;

namespace rjw
{
	public abstract class Hediff_BasePregnancy : HediffWithComps
	{
		///<summary>
		///This hediff class simulates pregnancy.
		///</summary>	

		//Static fields
		private const int MiscarryCheckInterval = 1000;
		protected const int TicksPerDay = 60000;

		protected const string starvationMessage = "MessageMiscarriedStarvation";
		protected const string poorHealthMessage = "MessageMiscarriedPoorHealth";

		protected static readonly HashSet<string> non_genetic_traits = new HashSet<string>(DefDatabase<StringListDef>.GetNamed("NonInheritedTraits").strings);

		//Fields
		///All babies should be premade and stored here
		protected List<Pawn> babies;
		///Reference to daddy, goes null sometimes
		public Pawn father;
		///Is pregnancy visible?
		protected bool is_discovered;
		protected bool ShouldMiscarry = false;
		///Is pregnancy type checked?
		public bool is_checked = false;
		///Mechanoid pregnancy, false - spawn agressive mech
		public bool is_hacked = false;
		///Contractions duration, effectively additional hediff stage, a dirty hack to make birthing process notable
		protected int contractions;
		///Gestation progress per tick
		protected float progress_per_tick;
		//
		// Properties
		//
		public float GestationProgress
		{
			get => Severity;
			private set => Severity = value;
		}

		private bool IsSeverelyWounded
		{
			get
			{
				float num = 0;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				foreach (Hediff h in hediffs)
				{
					if (h is Hediff_Injury && (!h.IsPermanent() || !h.IsTended()))
					{
						num += h.Severity;
					}
				}
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				foreach (Hediff_MissingPart mp in missingPartsCommonAncestors)
				{
					if (mp.IsFresh)
					{
						num += mp.Part.def.GetMaxHealth(pawn);
					}
				}
				return num > 38 * pawn.RaceProps.baseHealthScale;
			}
		}

		/// <summary>
		/// Indicates pregnancy can be aborted using usual means.
		/// </summary>
		public virtual bool canBeAborted
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Indicates pregnancy can be miscarried.
		/// </summary>
		public virtual bool canMiscarry
		{
			get
			{
				return true;
			}
		}

		public override void PostMake()
		{
			// Ensure the hediff always applies to the torso, regardless of incorrect directive
			base.PostMake();
			BodyPartRecord torso = pawn.RaceProps.body.AllParts.Find(x => x.def.defName == "Torso");
			if (Part != torso)
				Part = torso;

			//(debug->add heddif)
			//init empty preg, instabirth, cause error during birthing
			//Initialize(pawn, father);
		}

		public override bool Visible => is_discovered;

		public virtual void DiscoverPregnancy()
		{
			is_discovered = true;
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				if (!is_checked)
				{
					string key1 = "RJW_PregnantTitle";
					string message_title = TranslatorFormattedStringExtensions.Translate(key1, pawn.LabelIndefinite());
					string key2 = "RJW_PregnantText";

					string message_text = TranslatorFormattedStringExtensions.Translate(key2, pawn.LabelIndefinite());
					Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.NeutralEvent, pawn, null);

				}
				else
				{
					PregnancyMessage();
				}
			}
		}

		public virtual void CheckPregnancy()
		{
			is_checked = true;
			if (!is_discovered)
				DiscoverPregnancy();
			else
				PregnancyMessage();
		}

		public virtual void PregnancyMessage()
		{
			string key1 = "RJW_PregnantTitle";
			string message_title = TranslatorFormattedStringExtensions.Translate(key1, pawn.LabelIndefinite());
			string key2 = "RJW_PregnantNormal";
			string message_text = TranslatorFormattedStringExtensions.Translate(key2, pawn.LabelIndefinite());
			Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.NeutralEvent, pawn, null);
		}

		// Quick method to simply return a body part instance by a given part name
		internal static BodyPartRecord GetPawnBodyPart(Pawn pawn, String bodyPart)
		{
			return pawn.RaceProps.body.AllParts.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed(bodyPart));
		}

		public virtual void Miscarry()
		{
			pawn.health?.RemoveHediff(this);
		}

		/// <summary>
		/// Called on abortion (noy implemented yet)
		/// </summary>
		public virtual void Abort() { }

		/// <summary>
		/// Mechanoids can remove pregnancy
		/// </summary>
		public virtual void Kill()
		{
			pawn.health?.RemoveHediff(this);
		}


		[SyncMethod]
		public Pawn partstospawn(Pawn baby, Pawn mother, Pawn dad)
		{
			//decide what parts to inherit
			//default use own parts
			Pawn partstospawn = baby;
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			//spawn with mother parts
			if (mother != null && Rand.Range(0, 100) <= 10)
				partstospawn = mother;
			//spawn with father parts
			if (dad != null && Rand.Range(0, 100) <= 10)
				partstospawn = dad;

			//Log.Message("[RJW] Pregnancy partstospawn " + partstospawn);
			return partstospawn;
		}

		[SyncMethod]
		public bool spawnfutachild(Pawn baby, Pawn mother, Pawn dad)
		{
			int futachance = 0;
			if (mother != null && Genital_Helper.is_futa(mother))
				futachance = futachance + 25;
			if (dad != null && Genital_Helper.is_futa(dad))
				futachance = futachance + 25;

			//Log.Message("[RJW] Pregnancy spawnfutachild " + futachance);
			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			//theres 1% change baby will be futa
			//bug ... or ... feature ... nature finds a way
			return (Rand.Range(0, 100) <= futachance);
		}

		[SyncMethod]
		public static Pawn Trytogetfather(ref Pawn mother)
		{
			//birthing with debug has no father
			//Postmake.initialize() has no father
			Log.Warning("Hediff_BasePregnancy::Trytogetfather() - debug? no father defined, trying to add one");

			Pawn pawn = mother;
			Pawn father = null;

			//possible fathers
			List<Pawn> partners = pawn.relations.RelatedPawns.Where(x =>
					Genital_Helper.has_penis_fertile(x) && !x.Dead &&
					(pawn.relations.DirectRelationExists(PawnRelationDefOf.Lover, x) ||
					pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, x) ||
					pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, x))
					).ToList();

			//add bonded animal
			if (xxx.is_zoophile(mother) && RJWSettings.bestiality_enabled)
				partners.AddRange(pawn.relations.RelatedPawns.Where(x =>
					Genital_Helper.has_penis_fertile(x) &&
					pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, x)).ToList());

			if (partners.Any())
			{
				father = partners.RandomElement();
				Log.Warning("Hediff_BasePregnancy::Trytogetfather() - father set to: " + xxx.get_pawnname(father));
			}

			return father;
		}

		[SyncMethod]
		public override void Tick()
		{
			ageTicks++;
			GestationProgress += progress_per_tick;

			if (pawn.IsHashIntervalTick(1000))
			{
				if (canMiscarry)
				{
					//Log.Message("[RJW] Pregnancy is ticking for " + pawn + " this is " + this.def.defName + " will end in " + 1/progress_per_tick/TicksPerDay + " days resulting in "+ babies[0].def.defName);
					//Rand.PopState();
					//Rand.PushState(RJW_Multiplayer.PredictableSeed());
					//miscarry after starving 0.5 day
					if (pawn.needs.food != null && pawn.needs.food.CurCategory == HungerCategory.Starving && Rand.MTBEventOccurs(0.5f, TicksPerDay, MiscarryCheckInterval))
					{
						var hed = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
						if (hed.Severity > 0.4)
						{
							if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
							{
								string text = "MessageMiscarriedStarvation".Translate(pawn.LabelIndefinite()).CapitalizeFirst();
								Messages.Message(text, pawn, MessageTypeDefOf.NegativeHealthEvent);
							}
							Miscarry();
							return;
						}
					}
					//let beatings only be important when pregnancy is developed somewhat
					//miscarry after SeverelyWounded 0.5 day
					if (Visible && ((IsSeverelyWounded && Rand.MTBEventOccurs(0.5f, TicksPerDay, MiscarryCheckInterval)) || ShouldMiscarry))
					{
						if (Visible && PawnUtility.ShouldSendNotificationAbout(pawn))
						{
							string text = "MessageMiscarriedPoorHealth".Translate(pawn.LabelIndefinite()).CapitalizeFirst();
							Messages.Message(text, pawn, MessageTypeDefOf.NegativeHealthEvent);
						}
						Miscarry();
						return;
					}
				}

				// Check if pregnancy is far enough along to "show" for the body type
				if (!is_discovered)
				{
					BodyTypeDef bodyT = pawn?.story?.bodyType;
					//float threshold = 0f;

					if ((bodyT == BodyTypeDefOf.Thin && GestationProgress > 0.25f) ||
						(bodyT == BodyTypeDefOf.Female && GestationProgress > 0.35f) ||
						(GestationProgress > 0.50f)) // todo: Modded bodies? (FemaleBB for, example)
						DiscoverPregnancy();

					//switch (bodyT)
					//{
					//case BodyType.Thin: threshold = 0.3f; break;
					//case BodyType.Female: threshold = 0.389f; break;
					//case BodyType.Male: threshold = 0.41f; break; 
					//default: threshold = 0.5f; break;
					//}
					//if (GestationProgress > threshold){ DiscoverPregnancy(); }
				}

				if (CurStageIndex == 3)
				{
					if (contractions == 0)
					{
						if (PawnUtility.ShouldSendNotificationAbout(pawn))
						{
							string text = "RJW_Contractions".Translate(pawn.LabelIndefinite());
							Messages.Message(text, pawn, MessageTypeDefOf.NeutralEvent);
						}
						contractions++;
					}
					if (GestationProgress >= 1 && (pawn.CarriedBy == null || pawn.CarriedByCaravan()))//birthing takes an hour
					{
						if (PawnUtility.ShouldSendNotificationAbout(pawn))
						{
							string message_title = "RJW_GaveBirthTitle".Translate(pawn.LabelIndefinite());
							string message_text = "RJW_GaveBirthText".Translate(pawn.LabelIndefinite());
							string baby_text = ((babies.Count == 1) ? "RJW_ABaby".Translate() : "RJW_NBabies".Translate(babies.Count));
							message_text = message_text + baby_text;
							Find.LetterStack.ReceiveLetter(message_title, message_text, LetterDefOf.PositiveEvent, pawn);
						}
						GiveBirth();
					}
				}
			}
		}

		//
		//These functions are different from CnP
		//
		public override void ExposeData()//If you didn't know, this one is used to save the object for later loading of the game... and to load the data into the object, <sigh>
		{
			base.ExposeData();
			Scribe_References.Look(ref father, "father");
			Scribe_Values.Look(ref is_checked, "is_checked");
			Scribe_Values.Look(ref is_hacked, "is_hacked");
			Scribe_Values.Look(ref is_discovered, "is_discovered");
			Scribe_Values.Look(ref ShouldMiscarry, "ShouldMiscarry");
			Scribe_Values.Look(ref contractions, "contractions");
			Scribe_Collections.Look(ref babies, saveDestroyedThings: true, label: "babies", lookMode: LookMode.Deep, ctorArgs: new object[0]);
			Scribe_Values.Look(ref progress_per_tick, "progress_per_tick", 1);
		}

		//This should generate pawns to be born in due time. Should take into account all settings and parent races
		[SyncMethod]
		protected virtual void GenerateBabies()
		{
			Pawn mother = pawn;
			//Log.Message("Generating babies for " + this.def.defName);
			if (mother == null)
			{
				Log.Error("Hediff_BasePregnancy::GenerateBabies() - no mother defined");
				return;
			}

			if (father == null)
			{
				father = Trytogetfather(ref mother);
			}

			//Babies will have average number of traits of their parents, this way it will hopefully be compatible with various mods that change number of allowed traits
			//int trait_count = 0;
			// not anymore. Using number of traits originally generated by game as a guide



			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			List<Trait> traitpool = new List<Trait>(); // if both parents has the same trait chanses to give it to child are doubled. 
													   // It was a bug, now it is a feature.
			
			float skin_whiteness = Rand.Range(0, 1);

			//Log.Message("[RJW]Generating babies " + traitpool.Count + " traits at first");
			if (xxx.has_traits(mother) && mother.RaceProps.Humanlike)
			{
				foreach (Trait momtrait in mother.story.traits.allTraits)
				{
					if (!RJWPregnancySettings.trait_filtering_enabled || !non_genetic_traits.Contains(momtrait.def.defName))
						traitpool.Add(momtrait);
				}
				skin_whiteness = mother.story.melanin;
				//trait_count = mother.story.traits.allTraits.Count;
			}
			if (father != null && xxx.has_traits(father) && father.RaceProps.Humanlike)
			{
				foreach (Trait poptrait in father.story.traits.allTraits)
				{
					if (!RJWPregnancySettings.trait_filtering_enabled || !non_genetic_traits.Contains(poptrait.def.defName))
						traitpool.Add(poptrait);
				}
				skin_whiteness = Rand.Range(skin_whiteness, father.story.melanin);
				//trait_count = Mathf.RoundToInt((trait_count + father.story.traits.allTraits.Count) / 2f);
			}
			//this probably doesnt matter in 1.0 remove it and wait for bug reports =)
			//trait_count = trait_count > 2 ? trait_count : 2;//Tynan has hardcoded 2-3 traits per pawn.  In the above, result may be less than that. Let us not have disfunctional babies.

			//Log.Message("[RJW]Generating babies " + traitpool.Count + " traits aFter parents");
			Pawn parent = mother; //race of child
			Pawn parent2 = father; //name for child

			//Decide on which parent is first to be inherited
			if (father != null && RJWPregnancySettings.use_parent_method)
			{
				//Log.Message("The baby race needs definition");
				//humanality
				if (xxx.is_human(mother) && xxx.is_human(father))
				{
					//Log.Message("It's of two humanlikes");
					if (!Rand.Chance(RJWPregnancySettings.humanlike_DNA_from_mother))
					{
						//Log.Message("Mother will birth father race");
						parent = father;
					}
					else
					{
						//Log.Message("Mother will birth own race");
					}
				}
				else
				{
					//bestiality
					if ((!xxx.is_human(mother) && xxx.is_human(father)) ||
						(xxx.is_human(mother) && !xxx.is_human(father)))
					{
						if (RJWPregnancySettings.bestiality_DNA_inheritance == 0.0f)
						{
							//Log.Message("mother will birth beast");
							if (xxx.is_human(mother))
								parent = father;
							else
								parent = mother;
						}
						else if (RJWPregnancySettings.bestiality_DNA_inheritance == 1.0f)
						{
							//Log.Message("mother will birth humanlike");
							if (xxx.is_human(mother))
								parent = mother;
							else
								parent = father;
						}
						else
						{
							if (!Rand.Chance(RJWPregnancySettings.bestial_DNA_from_mother))
							{
								//Log.Message("Mother will birth father race");
								parent = father;
							}
							else
							{
								//Log.Message("Mother will birth own race");
							}
						}

					}
					//animality
					else if (!Rand.Chance(RJWPregnancySettings.bestial_DNA_from_mother))
					{
						//Log.Message("Mother will birth father race");
						parent = father;
					}
					else
					{
						//Log.Message("Mother will birth own race");
					}
				}
			}
			//androids can only birth non androids
			if (AndroidsCompatibility.IsAndroid(mother) && !AndroidsCompatibility.IsAndroid(father))
			{
				parent = father;
			}
			else if (!AndroidsCompatibility.IsAndroid(mother) && AndroidsCompatibility.IsAndroid(father))
			{
				parent = mother;
			}
			else if (AndroidsCompatibility.IsAndroid(mother) && AndroidsCompatibility.IsAndroid(father))
			{
				Log.Warning("Both parents are andoids, what have you done monster!");
				//this should never happen but w/e
			}
			//slimes birth only other slimes
			if (xxx.is_slime(mother))
			{
				parent = mother;
			}
			if (father != null)
				if (xxx.is_slime(father))
				{
					parent = father;
				}
			//Log.Message("[RJW] The main parent is " + parent);
			//Log.Message("Mother: " + xxx.get_pawnname(mother) + " kind: " + mother.kindDef);
			//Log.Message("Father: " + xxx.get_pawnname(father) + " kind: " + father.kindDef);
			//Log.Message("Baby base: " + xxx.get_pawnname(parent) + " kind: " + parent.kindDef);

			// Surname passing
			string last_name = "";
			if (xxx.is_human(father))
				last_name = NameTriple.FromString(father.Name.ToStringFull).Last;
			if (xxx.is_human(mother) && last_name == "")
				last_name = NameTriple.FromString(mother.Name.ToStringFull).Last;
			//Log.Message("[RJW] Bady surname will be " + last_name);

			//Pawn generation request
			PawnKindDef spawn_kind_def = parent.kindDef;
			Faction spawn_faction = mother.IsPrisoner ? null : mother.Faction;
			PawnGenerationRequest request = new PawnGenerationRequest(
				kind: spawn_kind_def,
				faction: spawn_faction,
				forceGenerateNewPawn: true,
				newborn: true,
				canGeneratePawnRelations: false,
				colonistRelationChanceFactor: 0,
				allowFood: false,
				allowAddictions: false,
				relationWithExtraPawnChanceFactor: 0,
				fixedMelanin: skin_whiteness,
				fixedLastName: last_name
				);

			//Log.Message("[RJW] Generated request, making babies");
			//Litter size. Let's use the main parent litter size, increased by fertility.
			float litter_size = (parent.RaceProps.litterSizeCurve == null) ? 1 : Rand.ByCurve(parent.RaceProps.litterSizeCurve);
			//Log.Message("[RJW] base Litter size " + litter_size);
			litter_size *= Math.Min(mother.health.capacities.GetLevel(xxx.reproduction), 1);
			litter_size *= Math.Min(father == null ? 1 : father.health.capacities.GetLevel(xxx.reproduction), 1);
			litter_size = Math.Max(1, litter_size);
			//Log.Message("[RJW] Litter size (w fertility) " + litter_size);

			//Babies size vs mother body size
			//assuming mother belly is 1/3 of mother body size
			float baby_size = spawn_kind_def.RaceProps.lifeStageAges[0].def.bodySizeFactor * spawn_kind_def.RaceProps.baseBodySize; // adult size/5
																																	//Log.Message("[RJW] Baby size " + baby_size);
			float max_litter = 1f / 3f / baby_size;
			//Log.Message("[RJW] Max size " + max_litter);
			max_litter *= (mother.Has(Quirk.Breeder) || mother.Has(Quirk.Incubator)) ? 2 : 1;
			//Log.Message("[RJW] Max size (w quirks) " + max_litter);

			//Generate random amount of babies within litter/max size
			litter_size = (Rand.Range(litter_size, max_litter));
			//Log.Message("[RJW] Litter size roll 1:" + litter_size);
			litter_size = Mathf.RoundToInt(litter_size);
			//Log.Message("[RJW] Litter size roll 2:" + litter_size);
			litter_size = Math.Max(1, litter_size);
			//Log.Message("[RJW] final Litter size " + litter_size);

			for (int i = 0; i < litter_size; i++)
			{
				Pawn baby = PawnGenerator.GeneratePawn(request);
				//Choose traits to add to the child. Unlike CnP this will allow for some random traits
				if (xxx.is_human(baby) && traitpool.Count > 0)
				{
					updateTraits(baby, traitpool);
				}
				babies.Add(baby);
			}
		}

		/// <summary>
		/// Update pawns traits
		/// Uses original pawns trains and given list of traits as a source of traits to select.
		/// </summary>
		/// <param name="pawn">humanoid pawn</param>
		/// <param name="traitpool">list of parent traits</param>
		/// <param name="traitLimit">maximum allowed number of traits</param>
		void updateTraits(Pawn pawn, List<Trait> parenttraitpool, int traitLimit = -1)
		{
			if (pawn?.story?.traits == null)
			{
				return;
			}

			if (traitLimit == -1)
			{
				traitLimit = pawn.story.traits.allTraits.Count;
			}

			//Personal pool
			List<Trait> personalTraitPool = new List<Trait>(pawn.story.traits.allTraits);
			//Parents pool
			if (parenttraitpool != null)
			{
				personalTraitPool.AddRange(parenttraitpool);
			}

			//Game suggested traits.
			var forcedTraits = personalTraitPool
				.Where(x => x.ScenForced)
				.Distinct(new TraitComparer(ignoreDegree: true)); // result can be a mess, because game allows this mess to be created in scenario editor

			List<Trait> selectedTraits = new List<Trait>();
			selectedTraits.AddRange(forcedTraits); // enforcing scenario forced traits

			var comparer = new TraitComparer(); // trait comparision implementation, because without game compares traits *by reference*, makeing them all unique.

			while (selectedTraits.Count < traitLimit && personalTraitPool.Count > 0)
			{
				int index = Rand.Range(0, personalTraitPool.Count); // getting trait and removing from the pull
				var trait = personalTraitPool[index];
				personalTraitPool.RemoveAt(index);

				if (!selectedTraits.Any(x => comparer.Equals(x, trait) ||  // skipping traits conflicting with already added
				                             x.def.ConflictsWith(trait)))
				{
					selectedTraits.Add(new Trait(trait.def, trait.Degree, false));
				}
			}

			pawn.story.traits.allTraits = selectedTraits;
		}

		//Handles the spawning of pawns and adding relations
		//this is extended by other scripts
		public abstract void GiveBirth();

		public virtual void PostBirth(Pawn mother, Pawn father, Pawn baby)
		{
			//inject RJW_BabyState to the newborn if RimWorldChildren is not active
			//cnp patches its hediff right into pawn generator, so its already in if it can
			if (xxx.RimWorldChildrenIsActive)
			{
				if (xxx.is_human(mother))
				{
					//BnC compatibility
					if (DefDatabase<HediffDef>.GetNamedSilentFail("BnC_RJW_PostPregnancy") == null)
					{
						mother.health.AddHediff(HediffDef.Named("PostPregnancy"), null, null);
						mother.health.AddHediff(HediffDef.Named("Lactating"), mother.RaceProps.body.AllParts.Find(x => x.def.defName == "Chest"), null);
					}
					if (xxx.is_human(baby))
						if (mother.records.GetAsInt(xxx.CountOfBirthHuman) == 0 &&
							mother.records.GetAsInt(xxx.CountOfBirthAnimal) == 0 &&
							mother.records.GetAsInt(xxx.CountOfBirthEgg) == 0)
						{
							mother.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("IGaveBirthFirstTime"));
						}
						else
						{
							mother.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("IGaveBirth"));
						}
				}

				if (xxx.is_human(baby))
					if (xxx.is_human(father))
					{
						father.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PartnerGaveBirth"));
					}
			}

			BabyPostBirth(mother, father, baby);

			if (baby.playerSettings != null && mother.playerSettings != null)
			{
				baby.playerSettings.AreaRestriction = mother.playerSettings.AreaRestriction;
			}

			//spawn futa
			bool isfuta = spawnfutachild(baby, mother, father);

			if (isfuta)
			{
				SexPartAdder.add_genitals(baby, partstospawn(baby, mother, father), Gender.Male);
				SexPartAdder.add_genitals(baby, partstospawn(baby, mother, father), Gender.Female);
				SexPartAdder.add_breasts(baby, partstospawn(baby, mother, father), Gender.Female);
				baby.gender = Gender.Female;                //set gender to female for futas, should cause no errors since babies already generated with relations n stuff
			}
			else
			{
				SexPartAdder.add_genitals(baby, partstospawn(baby, mother, father));
				SexPartAdder.add_breasts(baby, partstospawn(baby, mother, father));
			}
			SexPartAdder.add_anus(baby, partstospawn(baby, mother, father));

			if (mother.Spawned)
			{
				// Move the baby in front of the mother, rather than on top
				if (mother.CurrentBed() != null)
				{
					baby.Position = baby.Position + new IntVec3(0, 0, 1).RotatedBy(mother.CurrentBed().Rotation);
				}
				// Spawn guck
				FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5);
				mother.caller?.DoCall();
				baby.caller?.DoCall();
				father?.caller?.DoCall();
			}


			if (xxx.is_human(baby))
				mother.records.AddTo(xxx.CountOfBirthHuman, 1);
			if (xxx.is_animal(baby))
				mother.records.AddTo(xxx.CountOfBirthAnimal, 1);

			if ((mother.records.GetAsInt(xxx.CountOfBirthHuman) > 10 || mother.records.GetAsInt(xxx.CountOfBirthAnimal) > 20))
			{
				mother.Add(Quirk.Breeder);
				mother.Add(Quirk.ImpregnationFetish);
			}
		}

		public static void BabyPostBirth(Pawn mother, Pawn father, Pawn baby)
		{
			if (!xxx.is_human(baby)) return;

			baby.story.childhood = null;
			baby.story.adulthood = null;

			if (!xxx.RimWorldChildrenIsActive)
			{
				if (baby.ageTracker.CurLifeStageIndex <= 1 && baby.ageTracker.AgeBiologicalYears < 1 && !baby.Dead)
				{
					// Clean out drugs, implants, and stuff randomly generated
					//no support for custom race stuff, if there is any
					//baby.health.hediffSet.Clear();
					baby.health.AddHediff(HediffDef.Named("RJW_BabyState"), null, null);//RJW_Babystate.tick_rare actually forces CnP switch to CnP one if it can, don't know wat do
					Hediff_SimpleBaby babystate = (Hediff_SimpleBaby)baby.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("RJW_BabyState"));
					if (babystate != null)
					{
						babystate.GrowUpTo(0, true);
					}
				}
			}
			else
			{
				Log.Message("[RJW]PostBirth:: Rewriting story of " + baby);
				//var disabledBaby = BackstoryDatabase.allBackstories["CustomBackstory_NA_Childhood_Disabled"]; // should be this, but bnc/cnp is broken and cant undisable work
				var BabyStory = BackstoryDatabase.allBackstories["CustomBackstory_NA_Childhood"];
				if (BabyStory != null)
				{
					baby.story.childhood = BabyStory;
				}
				else
				{
					Log.Error("Couldn't find the required Backstory: CustomBackstory_NA_Childhood!");
				}
				//BnC compatibility
				if (DefDatabase<HediffDef>.GetNamedSilentFail("BnC_RJW_PostPregnancy") != null)
				{
					baby.health.AddHediff(HediffDef.Named("BabyState"), null, null);
					baby.health.AddHediff(HediffDef.Named("NoManipulationFlag"), null, null);
				}
			}

			foreach (var skill in baby.skills?.skills)
				skill.Level = 0;

			if (baby.health.hediffSet.HasHediff(HediffDef.Named("DeathAcidifier")))
			{
				baby.health.RemoveHediff(baby.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("DeathAcidifier")));
			}
		}

		//This method is doing the work of the constructor since hediffs are created through HediffMaker instead of normal oop way
		//This can't be in PostMake() because there wouldn't be father.
		public virtual void Initialize(Pawn mother, Pawn dad)
		{
			BodyPartRecord torso = mother.RaceProps.body.AllParts.Find(x => x.def.defName == "Torso");
			mother.health.AddHediff(this, torso);
			//Log.Message("[RJW]" + this.GetType().ToString() + " pregnancy hediff generated: " + this.Label);
			//Log.Message("[RJW]" + this.GetType().ToString() + " mother: " + mother + " father: " + dad);
			father = dad;
			if (father != null)
			{
				babies = new List<Pawn>();
				contractions = 0;
				//Log.Message("[RJW]" + this.GetType().ToString() + " generating babies before: " + this.babies.Count);
				GenerateBabies();
			}
			//progress_per_tick = babies.NullOrEmpty() ? 1f : (1.0f) / (babies[0].RaceProps.gestationPeriodDays * TicksPerDay/50);
			progress_per_tick = babies.NullOrEmpty() ? 1f : (1.0f) / (babies[0].RaceProps.gestationPeriodDays * TicksPerDay);
			if (pawn.Has(Quirk.Breeder) || pawn.health.hediffSet.HasHediff(HediffDef.Named("FertilityEnhancer")))
				progress_per_tick *= 1.25f;
			if (xxx.ImmortalsIsActive && mother.health.hediffSet.HasHediff(xxx.IH_Immortal) || father.health.hediffSet.HasHediff(xxx.IH_Immortal))
			{
				ShouldMiscarry = true;
			}
			//Log.Message("[RJW]" + this.GetType().ToString() + " generating babies after: " + this.babies.Count);
		}

		private static Dictionary<Type, string> _hediffOfClass = null;
		protected static Dictionary<Type, string> hediffOfClass
		{
			get
			{
				if (_hediffOfClass == null)
				{
					_hediffOfClass = new Dictionary<Type, string>();
					var allRJWPregnancies = AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(
							a => a
							.GetTypes()
							.Where(t => t.IsSubclassOf(typeof(Hediff_BasePregnancy)))
						);

					foreach (var pregClass in allRJWPregnancies)
					{
						var attribute = (RJWAssociatedHediffAttribute)pregClass.GetCustomAttributes(typeof(RJWAssociatedHediffAttribute), false).FirstOrDefault();
						if (attribute != null)
						{
							_hediffOfClass[pregClass] = attribute.defName;
						}
					}
				}

				return _hediffOfClass;
			}
		}


		/// <summary>
		/// Creates pregnancy hediff and assigns it to mother 
		/// </summary>
		/// <typeparam name="T">type of pregnancy, should be subclass of Hediff_BasePregnancy</typeparam>
		/// <param name="mother"></param>
		/// <param name="father"></param>
		/// <returns>created hediff</returns>
		public static T Create<T>(Pawn mother, Pawn father) where T : Hediff_BasePregnancy
		{
			if (mother == null)
				return null;
			//if (mother.RaceHasOviPregnancy() && !(T is Hediff_MechanoidPregnancy))
			//{
			//	//return null;
			//}
			//else
			//{
			//}

			BodyPartRecord torso = mother.RaceProps.body.AllParts.Find(x => x.def.defName == "Torso");

			string defName = hediffOfClass.ContainsKey(typeof(T)) ? hediffOfClass[typeof(T)] : "RJW_pregnancy";
			if (RJWSettings.DevMode) Log.Message($"Hediff_BasePregnancy::create hediff:{defName} class:{typeof(T).FullName}");

			T hediff = HediffHelper.MakeHediff<T>(HediffDef.Named(defName), mother, torso);
			hediff.Initialize(mother, father);
			return hediff;
		}

		/// <summary>
		/// list of all known RJW pregnancy hediff names (new can be regicreted by mods)
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> KnownPregnancies()
		{
			return hediffOfClass.Values.Distinct(); // todo: performance
		}

		public override string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.DebugString());
			stringBuilder.AppendLine("Gestation progress: " + GestationProgress.ToStringPercent());
			//stringBuilder.AppendLine("Time left: " + ((int)((1f - GestationProgress) * babies[0].RaceProps.gestationPeriodDays * TicksPerDay/50)).ToStringTicksToPeriod());
			stringBuilder.AppendLine("Time left: " + ((int)((1f - GestationProgress) * babies[0].RaceProps.gestationPeriodDays * TicksPerDay)).ToStringTicksToPeriod());
			stringBuilder.AppendLine(" Father: " + xxx.get_pawnname(father));
			return stringBuilder.ToString();
		}
	}
}

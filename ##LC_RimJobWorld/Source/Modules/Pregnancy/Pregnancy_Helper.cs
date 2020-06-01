using RimWorld;
using Verse;
using System;
using System.Linq;
using System.Collections.Generic;
using Multiplayer.API;

///RimWorldChildren pregnancy:
//using RimWorldChildren;

namespace rjw
{
	/// <summary>
	/// This handles pregnancy chosing between different types of pregnancy awailable to it
	/// 1a:RimWorldChildren pregnancy for humanlikes
	/// 1b:RJW pregnancy for humanlikes
	/// 2:RJW pregnancy for bestiality
	/// 3:RJW pregnancy for insects
	/// 4:RJW pregnancy for mechanoids
	/// </summary>

	public static class PregnancyHelper
	{
		//called by aftersex (including rape, breed, etc)
		//called by mcevent

		//pawn - "father"; partner = mother
		public static void impregnate(Pawn pawn, Pawn partner, xxx.rjwSextype sextype = xxx.rjwSextype.None)
		{

			if (RJWSettings.DevMode) Log.Message("Rimjobworld::impregnate(" + sextype + "):: " + xxx.get_pawnname(pawn) + " + " + xxx.get_pawnname(partner) + ":");

			//"mech" pregnancy
			if (sextype == xxx.rjwSextype.MechImplant)
			{
				if (RJWPregnancySettings.mechanoid_pregnancy_enabled)
				{
					if (RJWSettings.DevMode) Log.Message(" mechanoid pregnancy");

					// removing old pregnancies
					if (RJWSettings.DevMode) Log.Message("[RJW] removing other pregnancies");
					var preg = GetPregnancy(partner);
					if (preg != null)
					{
						if (preg is Hediff_BasePregnancy)
						{
							(preg as Hediff_BasePregnancy).Kill();
						}
						else
						{
							partner.health.RemoveHediff(preg);
						}
					}

					// new pregnancy
					Hediff_MechanoidPregnancy hediff = Hediff_BasePregnancy.Create<Hediff_MechanoidPregnancy>(partner, pawn);

					return;
					/*
					// Not an actual pregnancy. This implants mechanoid tech into the target.
					//may lead to pregnancy
					//old "chip pregnancies", maybe integrate them somehow?
					//Rand.PopState();
					//Rand.PushState(RJW_Multiplayer.PredictableSeed());
					HediffDef_MechImplants egg = (from x in DefDatabase<HediffDef_MechImplants>.AllDefs	select x).RandomElement();
					if (egg != null)
					{
						if (RJWSettings.DevMode) Log.Message(" planting MechImplants:" + egg.ToString());
						PlantSomething(egg, partner, !Genital_Helper.has_vagina(partner), 1);
						return;
					}
					else
					{
						if (RJWSettings.DevMode) Log.Message(" no mech implant found");
					}*/
				}
				return;
			}

			// Sextype can result in pregnancy.
			if (!(sextype == xxx.rjwSextype.Vaginal || sextype == xxx.rjwSextype.DoublePenetration))
				return;

			//Log.Message("[RJW] RaceImplantEggs()" + pawn.RaceImplantEggs());
			//"insect" pregnancy
			//straight, female (partner) recives egg insertion from other/sex starter (pawn)
			var pawnpartBPR = Genital_Helper.get_genitalsBPR(pawn);
			var pawnparts = Genital_Helper.get_PartsHediffList(pawn, pawnpartBPR);
			var partnerpartBPR = Genital_Helper.get_genitalsBPR(partner);
			var partnerparts = Genital_Helper.get_PartsHediffList(partner, partnerpartBPR);

			if (Genital_Helper.has_vagina(partner, partnerparts) &&
					(Genital_Helper.has_ovipositorF(pawn, pawnparts) ||
						(Genital_Helper.has_ovipositorM(pawn, pawnparts) ||
							(Genital_Helper.has_penis_fertile(pawn, pawnparts) && pawn.RaceImplantEggs())))
				)
			{
				DoEgg(pawn, partner);
				return;
			}
			//reverse, female (pawn) starts sex/passive bestiality and fills herself with eggs - this is likely fucked up and needs fixing at jobdriver, processsex and aftersex levels
			else
			if (Genital_Helper.has_vagina(pawn, pawnparts) &&
					(Genital_Helper.has_ovipositorF(partner, partnerparts) ||
						(Genital_Helper.has_ovipositorM(partner, partnerparts) ||
							(Genital_Helper.has_penis_fertile(partner, partnerparts) && pawn.RaceImplantEggs())))
				)
			{
				DoEgg(partner, pawn);
				return;
			}

			//"normal" and "beastial" pregnancy
			if (RJWSettings.DevMode) Log.Message(" 'normal' pregnancy checks");

			//futa-futa docking?
			//if (CanImpregnate(partner, pawn, sextype) && CanImpregnate(pawn, partner, sextype))
			//{
			//Log.Message("[RJW] futa-futa docking...");
			//return;
			//Doimpregnate(pawn, partner);
			//Doimpregnate(partner, pawn);
			//}
			//normal, when female is passive/recives interaction
			if (Genital_Helper.has_penis_fertile(pawn, pawnparts) && Genital_Helper.has_vagina(partner, partnerparts) && CanImpregnate(pawn, partner, sextype))
			{
				if (RJWSettings.DevMode) Log.Message(" impregnate forward");
				Doimpregnate(pawn, partner);
			}
			//reverse, when female active/starts interaction
			else if (Genital_Helper.has_vagina(pawn, pawnparts) && Genital_Helper.has_penis_fertile(partner, partnerparts) && CanImpregnate(partner, pawn, sextype))
			{
				if (RJWSettings.DevMode) Log.Message(" impregnate reverse");
				Doimpregnate(partner, pawn);
			}
		}


		public static Hediff GetPregnancy(Pawn pawn)
		{
			var set = pawn.health.hediffSet;

			Hediff preg =
				Hediff_BasePregnancy.KnownPregnancies()
				.Select(x => set.GetFirstHediffOfDef(HediffDef.Named(x)))
				.FirstOrDefault(x => x != null) ??
				set.GetFirstHediffOfDef(HediffDef.Named("Pregnant"));

			return preg;
		}

		///<summary>Can get preg with above conditions, do impregnation.</summary>

		[SyncMethod]
		public static void DoEgg(Pawn pawn, Pawn partner)
		{
			if (RJWPregnancySettings.insect_pregnancy_enabled)
			{
				if (RJWSettings.DevMode) Log.Message(" insect pregnancy");

				//female "insect" plant eggs
				//futa "insect" 50% plant eggs
				if ((Genital_Helper.has_ovipositorF(pawn) && !Genital_Helper.has_ovipositorM(pawn)) ||
					(Genital_Helper.has_ovipositorF(pawn) && Genital_Helper.has_ovipositorM(pawn) && Rand.Value > 0.5f))
				{
					float maxeggssize = (partner.BodySize / 5) * (xxx.has_quirk(partner, "Incubator") ? 2f : 1f) * (Genital_Helper.has_ovipositorF(partner) ? 2f : 0.5f);
					float eggedsize = 0;
					foreach (Hediff_InsectEgg egg in partner.health.hediffSet.GetHediffs<Hediff_InsectEgg>())
					{
						if (egg.father != null)
							eggedsize += egg.father.RaceProps.baseBodySize / 5;
						else
							eggedsize += egg.implanter.RaceProps.baseBodySize / 5;
					}
					if (RJWSettings.DevMode) Log.Message(" determine " + xxx.get_pawnname(partner) + " size of eggs inside: " + eggedsize + ", max: " + maxeggssize);

					var eggs = pawn.health.hediffSet.GetHediffs<Hediff_InsectEgg>()
						.ToList();
					var partnerGenitals = Genital_Helper.get_genitalsBPR(partner);
					while (eggs.Any() && eggedsize < maxeggssize)
					{
						var egg = eggs.First();
						eggs.Remove(egg);
						
						pawn.health.RemoveHediff(egg);
						partner.health.AddHediff(egg, partnerGenitals);
						
						egg.Implanter(pawn);

						eggedsize += egg.eggssize;
					}
					
					//TODO: add widget toggle for bind all/neutral/hostile  pawns
					if (pawn.HostileTo(partner))
					//if (!pawn.IsColonist)
						if (!partner.health.hediffSet.HasHediff(HediffDef.Named("RJW_Cocoon")))
							//if (!partner.health.hediffSet.HasHediff(HediffDef.Named("RJW_Cocoon")) && pawn.Faction != partner.Faction)
							//if (!partner.health.hediffSet.HasHediff(HediffDef.Named("RJW_Cocoon")) && pawn.Faction != partner.Faction && pawn.HostileTo(partner))
							partner.health.AddHediff(HediffDef.Named("RJW_Cocoon"));
				}
				//male "insect" fertilize eggs
				else if (!pawn.health.hediffSet.HasHediff(xxx.sterilized))
				{
					foreach (var egg in (from x in partner.health.hediffSet.GetHediffs<Hediff_InsectEgg>() where x.IsParent(pawn) select x))
						egg.Fertilize(pawn);
				}
				return;
			}
		}

		[SyncMethod]
		public static void Doimpregnate(Pawn pawn, Pawn partner)
		{
			if (RJWSettings.DevMode) Log.Message("[RJW] Doimpregnate " + xxx.get_pawnname(pawn) + " is a father, " + xxx.get_pawnname(partner) + " is a mother");

			if (AndroidsCompatibility.IsAndroid(pawn) && !AndroidsCompatibility.AndroidPenisFertility(pawn))
			{
				if (RJWSettings.DevMode) Log.Message(" Father is android with no arcotech penis, abort");
				return;
			}
			if (AndroidsCompatibility.IsAndroid(partner) && !AndroidsCompatibility.AndroidVaginaFertility(partner))
			{
				if (RJWSettings.DevMode) Log.Message(" Mother is android with no arcotech vagina, abort");
				return;
			}

			// fertility check
			float fertility = RJWPregnancySettings.humanlike_impregnation_chance / 100f;
			if (xxx.is_animal(partner))
				fertility = RJWPregnancySettings.animal_impregnation_chance / 100f;

			// Interspecies modifier
			if (pawn.def.defName != partner.def.defName)
			{
				if (RJWPregnancySettings.complex_interspecies)
					fertility *= SexUtility.BodySimilarity(pawn, partner);
				else
					fertility *= RJWPregnancySettings.interspecies_impregnation_modifier;
			}

			//Rand.PopState();
			//Rand.PushState(RJW_Multiplayer.PredictableSeed());
			float ReproductionFactor = Math.Min(pawn.health.capacities.GetLevel(xxx.reproduction), partner.health.capacities.GetLevel(xxx.reproduction));
			float pregnancy_threshold = fertility * ReproductionFactor;
			float non_pregnancy_chance = Rand.Value;
			BodyPartRecord torso = partner.RaceProps.body.AllParts.Find(x => x.def == BodyPartDefOf.Torso);

			if (non_pregnancy_chance > pregnancy_threshold || pregnancy_threshold == 0)
			{
				if (RJWSettings.DevMode) Log.Message(" Impregnation failed. Chance: " + pregnancy_threshold.ToStringPercent() + " roll: " + non_pregnancy_chance.ToStringPercent());
				return;
			}
			if (RJWSettings.DevMode) Log.Message(" Impregnation succeeded. Chance: " + pregnancy_threshold.ToStringPercent() + " roll: " + non_pregnancy_chance.ToStringPercent());

			PregnancyDecider(partner, pawn);
		}


		///<summary>For checking normal pregnancy, should not for egg implantion or such.</summary>
		public static bool CanImpregnate(Pawn fucker, Pawn fucked, xxx.rjwSextype sextype = xxx.rjwSextype.Vaginal)
		{
			if (fucker == null || fucked == null) return false;

			if (RJWSettings.DevMode) Log.Message("Rimjobworld::CanImpregnate checks (" + sextype + "):: " + xxx.get_pawnname(fucker) + " + " + xxx.get_pawnname(fucked) + ":");

			if (sextype == xxx.rjwSextype.MechImplant && !RJWPregnancySettings.mechanoid_pregnancy_enabled)
			{
				if (RJWSettings.DevMode) Log.Message(" mechanoid 'pregnancy' disabled");
				return false;
			}
			if (!(sextype == xxx.rjwSextype.Vaginal || sextype == xxx.rjwSextype.DoublePenetration))
			{
				if (RJWSettings.DevMode) Log.Message(" sextype cannot result in pregnancy");
				return false;
			}

			if (AndroidsCompatibility.IsAndroid(fucker) && AndroidsCompatibility.IsAndroid(fucked))
			{
				if (RJWSettings.DevMode) Log.Message(xxx.get_pawnname(fucked) + " androids cant breed/reproduce androids");
				return false;
			}

			if ((fucker.IsUnsexyRobot() || fucked.IsUnsexyRobot()) && !(sextype == xxx.rjwSextype.MechImplant))
			{
				if (RJWSettings.DevMode) Log.Message(" unsexy robot cant be pregnant");
				return false;
			}

			if (!fucker.RaceHasPregnancy())
			{
				if (RJWSettings.DevMode) Log.Message(xxx.get_pawnname(fucked) + " filtered race that cant be pregnant");
				return false;
			}

			if (!fucked.RaceHasPregnancy())
			{
				if (RJWSettings.DevMode) Log.Message(xxx.get_pawnname(fucker) + " filtered race that cant impregnate");
				return false;
			}


			if (fucked.IsPregnant())
			{
				if (RJWSettings.DevMode) Log.Message(" already pregnant.");
				return false;
			}

			if ((from x in fucked.health.hediffSet.GetHediffs<Hediff_InsectEgg>() where x.def == DefDatabase<HediffDef_InsectEgg>.GetNamed(x.def.defName) select x).Any())
			{
				if (RJWSettings.DevMode) Log.Message(xxx.get_pawnname(fucked) + " cant get pregnant while eggs inside");
				return false;
			}

			var pawnpartBPR = Genital_Helper.get_genitalsBPR(fucker);
			var pawnparts = Genital_Helper.get_PartsHediffList(fucker, pawnpartBPR);
			var partnerpartBPR = Genital_Helper.get_genitalsBPR(fucked);
			var partnerparts = Genital_Helper.get_PartsHediffList(fucked, partnerpartBPR);
			if (!(Genital_Helper.has_penis_fertile(fucker, pawnparts) && Genital_Helper.has_vagina(fucked, partnerparts)) && !(Genital_Helper.has_penis_fertile(fucked, partnerparts) && Genital_Helper.has_vagina(fucker, pawnparts)))
			{
				if (RJWSettings.DevMode) Log.Message(" missing genitals for impregnation");
				return false;
			}

			if (fucker.health.capacities.GetLevel(xxx.reproduction) <= 0 || fucked.health.capacities.GetLevel(xxx.reproduction) <= 0)
			{
				if (RJWSettings.DevMode) Log.Message(" one (or both) pawn(s) infertile");
				return false;
			}

			if (xxx.is_human(fucked) && xxx.is_human(fucker) && (RJWPregnancySettings.humanlike_impregnation_chance == 0 || !RJWPregnancySettings.humanlike_pregnancy_enabled))
			{
				if (RJWSettings.DevMode) Log.Message(" human pregnancy chance set to 0% or pregnancy disabled.");
				return false;
			}
			else if (((xxx.is_animal(fucker) && xxx.is_human(fucked)) || (xxx.is_human(fucker) && xxx.is_animal(fucked))) && !RJWPregnancySettings.bestial_pregnancy_enabled)
			{
				if (RJWSettings.DevMode) Log.Message(" bestiality pregnancy chance set to 0% or pregnancy disabled.");
				return false;
			}
			else if (xxx.is_animal(fucked) && xxx.is_animal(fucker) && (RJWPregnancySettings.animal_impregnation_chance == 0 || !RJWPregnancySettings.animal_pregnancy_enabled))
			{
				if (RJWSettings.DevMode) Log.Message(" animal-animal pregnancy chance set to 0% or pregnancy disabled.");
				return false;
			}
			else if (fucker.def.defName != fucked.def.defName && (RJWPregnancySettings.interspecies_impregnation_modifier <= 0.0f && !RJWPregnancySettings.complex_interspecies))
			{
				if (RJWSettings.DevMode) Log.Message(" interspecies pregnancy disabled.");
				return false;
			}

			return true;
		}

		//Plant babies for human/bestiality pregnancy
		public static void PregnancyDecider(Pawn mother, Pawn father)
		{
			//human-human
			if (RJWPregnancySettings.humanlike_pregnancy_enabled && xxx.is_human(mother) && xxx.is_human(father))
			{
				CompEggLayer compEggLayer = mother.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					// fertilize eggs of humanlikes ?!
					if (!compEggLayer.FullyFertilized)
						compEggLayer.Fertilize(father);
				}
				else
					Hediff_BasePregnancy.Create<Hediff_HumanlikePregnancy>(mother, father);
			}
			//human-animal
			//maybe make separate option for human males vs female animals???
			else if (RJWPregnancySettings.bestial_pregnancy_enabled && ((xxx.is_human(mother) && xxx.is_animal(father)) || (xxx.is_animal(mother) && xxx.is_human(father))))
			{
				CompEggLayer compEggLayer = mother.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					// cant fertilize eggs
				}
				else
					Hediff_BasePregnancy.Create<Hediff_BestialPregnancy>(mother, father);
			}
			//animal-animal
			else if (xxx.is_animal(mother) && xxx.is_animal(father))
			{
				CompEggLayer compEggLayer = mother.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					// fertilize eggs of same species
					if (mother.kindDef == father.kindDef)
						compEggLayer.Fertilize(father);
				}
				else if (RJWPregnancySettings.animal_pregnancy_enabled)
				{
					Hediff_BasePregnancy.Create<Hediff_BestialPregnancy>(mother, father);
				}
			}
		}

		//Plant Insect eggs/mech chips/other preg mod hediff?
		public static bool PlantSomething(HediffDef def, Pawn target, bool isToAnal = false, int amount = 1)
		{
			if (def == null)
				return false;
			if (!isToAnal && !Genital_Helper.has_vagina(target))
				return false;
			if (isToAnal && !Genital_Helper.has_anus(target))
				return false;

			BodyPartRecord Part = (isToAnal) ? Genital_Helper.get_anusBPR(target) : Genital_Helper.get_genitalsBPR(target);
			if (Part != null || Part.parts.Count != 0)
			{
				//killoff normal preg
				if (!isToAnal)
				{
					if (RJWSettings.DevMode) Log.Message("[RJW] removing other pregnancies");

					var preg = GetPregnancy(target);
					if (preg != null)
					{
						if (preg is Hediff_BasePregnancy)
						{
							(preg as Hediff_BasePregnancy).Kill();
						}
						else
						{
							target.health.RemoveHediff(preg);
						}
					}
				}

				for (int i = 0; i < amount; i++)
				{

					if (RJWSettings.DevMode) Log.Message("[RJW] planting something weird");
					target.health.AddHediff(def, Part);
				}

				return true;
			}
			return false;
		}

		/// <summary>
		/// Remove CnP Pregnancy, that is added without passing rjw checks
		/// </summary>
		public static void cleanup_CnP(Pawn pawn)
		{
			//They do subpar probability checks and disrespect our settings, but I fail to just prevent their doloving override.
			//probably needs harmonypatch
			//So I remove the hediff if it is created and recreate it if needed in our handler later

			if (RJWSettings.DevMode) Log.Message("[RJW] cleanup_CnP after love check");

			var h = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("HumanPregnancy"));
			if (h != null && h.ageTicks < 10)
			{
				pawn.health.RemoveHediff(h);
				if (RJWSettings.DevMode) Log.Message("[RJW] removed hediff from " + xxx.get_pawnname(pawn));
			}
		}

		/// <summary>
		/// Remove Vanilla Pregnancy
		/// </summary>
		public static void cleanup_vanilla(Pawn pawn)
		{
			if (RJWSettings.DevMode) Log.Message("[RJW] cleanup_vanilla after love check");

			var h = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Pregnant"));
			if (h != null && h.ageTicks < 10)
			{
				pawn.health.RemoveHediff(h);
				if (RJWSettings.DevMode) Log.Message("[RJW] removed hediff from " + xxx.get_pawnname(pawn));
			}
		}

		/// <summary>
		/// Below is stuff for RimWorldChildren
		/// its not used, we rely only on our own pregnancies
		/// </summary>

		/// <summary>
		/// This function tries to call Children and pregnancy utilities to see if that mod could handle the pregnancy
		/// </summary>
		/// <returns>true if cnp pregnancy will work, false if rjw one should be used instead</returns>
		public static bool CnP_WillAccept(Pawn mother)
		{
			//if (!xxx.RimWorldChildrenIsActive)
				return false;

			//return RimWorldChildren.ChildrenUtility.RaceUsesChildren(mother);
		}

		/// <summary>
		/// This funtcion tries to call Children and Pregnancy to create humanlike pregnancy implemented by the said mod.
		/// </summary>
		public static void CnP_DoPreg(Pawn mother, Pawn father)
		{
			if (!xxx.RimWorldChildrenIsActive)
				return;

			//RimWorldChildren.Hediff_HumanPregnancy.Create(mother, father);
		}

	}
}
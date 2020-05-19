using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace VFEF
{
    public class CompScaresAnimals : ThingComp
    {
        public CompProperties_ScaresAnimals Props => (CompProperties_ScaresAnimals)base.props;
        private int hashOffset = 0;
        public int TickInterval => Props.ticksPerPulse;
        public float Radius => Props.effectRadius;
        public bool IsCheapIntervalTick => (int)(Find.TickManager.TicksGame + hashOffset) % TickInterval == 0;
        public static float HumanBodySize = 1f;
        public bool ShouldAffectColonyAnimal(Pawn animal)
        {
            return Props.AffectColonyAnimals || !(animal.Faction == Faction.OfPlayer);
        }
        public bool IsAffectedPredator(Pawn animal)
        {
            RaceProperties race = animal.def.race;
            if (race.predator && animal.Faction == Faction.OfPlayer) return false; // if colony predator, don't affect
            return !race.predator || race.maxPreyBodySize < HumanBodySize;         // otherwise, return whether the animal would prey upon humans
        }
        public bool SmallEnoughToScare(Pawn animal)
        {
            if (Props.maxBodySizeToScare < 0) return animal.BodySize < HumanBodySize;
            return animal.BodySize <= Props.maxBodySizeToScare;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            hashOffset = parent.thingIDNumber.HashOffset();
            if(respawningAfterLoad) HumanBodySize = ThingDefOf.Human.race.baseBodySize;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (IsCheapIntervalTick)
            {
                ScareAnimals();
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            ScareAnimals();
        }

        public void ScareAnimals()
        {
            //Select a from animals where a.bodySize <= human && a.intelligence < toolUser && !a.WouldHuntHumans
            IEnumerable<Pawn> pawns = base.parent.Map.mapPawns.AllPawns.Where(x => x.def.race.intelligence == Intelligence.Animal   // x has animal intelligence. I could use RaceProps but this has the potential for 
                                                                                                                                    // more flavor, e.g. Ferals mutants fleeing and intelligent animals not
                                                                                && SmallEnoughToScare(x)                            // x is small enough to scare
                                                                                && IsAffectedPredator(x)                            // x wouldn't prey upon colonists    
                                                                                && ShouldAffectColonyAnimal(x));                    // if set for colony animals to ignore it, they will
            if (pawns == null || pawns.Count() <= 0) return;
            //for all animals
            foreach (Pawn animal in pawns)
            {
                //if animal is nearby and not already fleeing
                if (animal != null && animal.jobs?.curJob?.def != JobDefOf.Flee && IntVec3Utility.DistanceTo(animal.Position, base.parent.Position) < Radius)
                {
                    //give the animal the flee job with parent as the thing they're fleeing
                    Job job = new Job(JobDefOf.Flee, 
                                      // Note that the below thing only treats things of the parent's def as dangerous. Done for performance reasons.
                                      // Not an issue with the mod as written, since VFE_Scarecrow is a single def, but could cause minor issues if the design changes.
                                      CellFinderLoose.GetFleeDest(animal, base.parent.Map.listerThings.ThingsOfDef(base.parent.def), Props.minFleeDistance), 
                                      base.parent.Position);
                    animal.jobs.StartJob(job, JobCondition.InterruptOptional);
                }
            }
        }
    }
}

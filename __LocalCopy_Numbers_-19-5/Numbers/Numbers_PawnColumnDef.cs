using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Numbers
{
    public class Numbers_PawnColumnDef : PawnColumnDef
    {
        public RecordDef record;
        public PawnCapacityDef capacity;
        public NeedDef need;
        public StatDef stat;
        public SkillDef skill;
        public PrisonerInteractionModeDef prisonerInteraction;
    }
}

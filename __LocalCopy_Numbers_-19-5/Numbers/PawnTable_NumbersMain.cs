namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class PawnTable_NumbersMain : PawnTable
    {
        public PawnTable_NumbersMain(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight)
        {
            PawnTableDef = def;
            SetMinMaxSize(def.minWidth, uiWidth, 0, (int)(uiHeight * Numbers_Settings.maxHeight));
        }

        public PawnTableDef PawnTableDef { get; protected set; }
    }
}

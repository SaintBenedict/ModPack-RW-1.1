namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_PrisonerRecruitmentDifficulty : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.RecruitDifficulty(Faction.OfPlayer).ToStringPercent();

        public override int Compare(Pawn a, Pawn b)
            => a.RecruitDifficulty(Faction.OfPlayer).CompareTo(b.RecruitDifficulty(Faction.OfPlayer));

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}

namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_ManhunterOnTameFailChance : PawnColumnWorker_Text
    {
        public override int Compare(Pawn a, Pawn b)
            => GetValue(a).CompareTo(GetValue(b));

        protected override string GetTextFor(Pawn pawn)
            => GetValue(pawn).ToStringPercent();

        protected override string GetTip(Pawn pawn)
            => "MessageAnimalManhuntsOnTameFailed".Translate(pawn.kindDef.GetLabelPlural().CapitalizeFirst(),
                                                             GetValue(pawn).ToStringPercent(), pawn.Named("ANIMAL"));

        private float GetValue(Pawn pawn)
            => pawn.RaceProps.manhunterOnTameFailChance;

        protected override string GetHeaderTip(PawnTable table)
            => "TameFailedRevengeChance".Translate() + "\n\n" + "Numbers_ColumnHeader_Tooltip".Translate();
        /*
        public override void DoHeader(Rect rect, PawnTable table)
        {
            float scale = 0.7f;
            base.DoHeader(rect, table);
            Vector2 headerIconSize = new Vector2(StaticConstructorOnGameStart.Tame.width * scale, StaticConstructorOnGameStart.Tame.height * scale);
            int     num            = (int)((rect.width - headerIconSize.x) / 2f);
            Rect    position       = new Rect(rect.x + num, rect.yMax - StaticConstructorOnGameStart.Tame.height, headerIconSize.x, headerIconSize.y);
            GUI.DrawTexture(position, StaticConstructorOnGameStart.Tame);
        }
        */
    }
}

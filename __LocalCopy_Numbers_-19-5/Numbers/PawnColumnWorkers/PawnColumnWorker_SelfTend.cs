namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_SelfTend : PawnColumnWorker_Checkbox
    {
        protected override bool GetValue(Pawn pawn) => pawn.playerSettings.selfTend;

        protected override bool HasCheckbox(Pawn pawn) => pawn.IsColonist && !pawn.Dead && !(pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor));

        protected override void SetValue(Pawn pawn, bool value)
        {
            if (value && pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) == 0)
                Messages.Message("MessageSelfTendUnsatisfied".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.CautionInput, false);

            pawn.playerSettings.selfTend = value;
        }

        protected override string GetHeaderTip(PawnTable table) => "SelfTend".Translate() + "\n\n" + "Numbers_ColumnHeader_Tooltip".Translate();

        protected override string GetTip(Pawn pawn) => "SelfTendTip".Translate(Faction.OfPlayer.def.pawnsPlural, TendUtility.SelfTendQualityFactor.ToStringPercent()).CapitalizeFirst();

        /*
        public override void DoHeader(Rect rect, PawnTable table)
        {
            float scale = 0.7f;
            base.DoHeader(rect, table);
            Vector2 headerIconSize = new Vector2(StaticConstructorOnGameStart.Tame.width * scale, StaticConstructorOnGameStart.Tame.height * scale);
            int num = (int)((rect.width - headerIconSize.x) / 2f);
            Rect position = new Rect(rect.x + num, rect.yMax - StaticConstructorOnGameStart.Tame.height, headerIconSize.x, headerIconSize.y);
            GUI.DrawTexture(position, StaticConstructorOnGameStart.Tame);
        }
        */
    }
}

namespace Numbers
{
    using RimWorld;
    using Verse;

    public class PawnColumnWorker_Forbidden : PawnColumnWorker_Checkbox
    {
        protected override bool GetValue(Pawn pawn) => ((Thing)pawn.ParentHolder).IsForbidden(Faction.OfPlayer);

        protected override void SetValue(Pawn pawn, bool value)
        {
            ((Thing)pawn.ParentHolder).SetForbidden(value);
        }
    }
}

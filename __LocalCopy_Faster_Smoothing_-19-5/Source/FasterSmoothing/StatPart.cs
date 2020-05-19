using Verse;
using RimWorld;

namespace FasterSmooth
{
  public class FasterSmoothing_StartPart : StatPart
  {
    public override void TransformValue(StatRequest req, ref float val)
    {
      val *= FSModSettings.amountSmoothingFactor;
    }

    public override string ExplanationPart(StatRequest req)
    {
      return "FSSPDescription".Translate() + ": x" + FSModSettings.amountSmoothingFactor.ToStringPercent();
    }
  }
}


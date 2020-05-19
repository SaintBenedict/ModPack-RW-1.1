using System;
using UnityEngine;
using Verse;

namespace FasterSmooth
{
  public class FSModSettings : ModSettings
  {
    public static float amountSmoothingFactor = 3f;


    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref amountSmoothingFactor, "amountSmoothingFactor");
    }
  }

  public class FSMod : Mod
  {
    FSModSettings settings;
    public FSMod(ModContentPack con) : base(con)
    {
      this.settings = GetSettings<FSModSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      Listing_Standard listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.Label("FSSmoothLabel".Translate() + ": " + FSModSettings.amountSmoothingFactor.ToStringPercent(), tooltip: "FSSmoothTooltip".Translate());
      FSModSettings.amountSmoothingFactor = listing.Slider(RoundToNearestHalf(FSModSettings.amountSmoothingFactor), 0.25f, 10f);
      listing.End();
      base.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
      base.WriteSettings();
    }

    public override string SettingsCategory()
    {
      return "FSTitle".Translate();
    }

    private float RoundToNearestHalf(float val)
    { 
      return (float)Math.Round(val * 2, MidpointRounding.AwayFromZero) / 2;
    }

    private float RoundToNearestTenth(float val)
    {
      return (float)Math.Round(val * 100, MidpointRounding.AwayFromZero) / 100;
    }
  }
}

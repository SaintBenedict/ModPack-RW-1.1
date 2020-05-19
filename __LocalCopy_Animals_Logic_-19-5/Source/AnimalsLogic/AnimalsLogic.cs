﻿using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;

namespace AnimalsLogic
{
    [StaticConstructorOnStartup]
    class AnimalsLogic : Mod
    {
#pragma warning disable 0649
        public static Settings Settings;
#pragma warning restore 0649

        public AnimalsLogic(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.quicksilverfox.rimworld.mod.animalslogic");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            base.GetSettings<Settings>();
        }

        public void Save()
        {
            LoadedModManager.GetMod<AnimalsLogic>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "AnimalsLogic";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}

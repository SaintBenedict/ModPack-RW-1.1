using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VAEShrubland
{

    public class VAEShrubland : Mod
    {
        public VAEShrubland(ModContentPack content) : base(content)
        {
            HarmonyInstance = new Harmony("OskarPotocki.VAEShrubland");
        }

        public static Harmony HarmonyInstance;

    }

}

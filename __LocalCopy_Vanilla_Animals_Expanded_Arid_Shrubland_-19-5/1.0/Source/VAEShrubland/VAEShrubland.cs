using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VAEShrubland
{

    public class VAEShrubland : Mod
    {
        public VAEShrubland(ModContentPack content) : base(content)
        {
            HarmonyInstance = HarmonyInstance.Create("OskarPotocki.VAEShrubland");
        }

        public static HarmonyInstance HarmonyInstance;

    }

}

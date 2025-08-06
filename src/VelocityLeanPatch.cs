using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(VelocityLean), "Awake")]
    public class VelocityLeanPatch
    {
        [HarmonyPostfix]
        public static void Postfix(VelocityLean __instance, ref float ___angularForceMultiplier)
        {
            ___angularForceMultiplier = PluginCore.config.AngularForceMultiplier;
        }
    }
}
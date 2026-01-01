using System;
using UnityEngine;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(SynchronizedObjectManager), "Awake")]
    public class SyncObjMngrPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SynchronizedObjectManager __instance, ref SnapshotInterpolationSettings ___snapshotInterpolationSettings, ref bool ___skipLateTicks)
        {
            ___skipLateTicks = false;
            ___snapshotInterpolationSettings.bufferLimit = 128;
            ___snapshotInterpolationSettings.bufferTimeMultiplier = 2.5f;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{

    [HarmonyPatch(typeof(PuckManager), "Server_SpawnPucksForPhase")]
    public class PuckManagerPatch
    {
        public static bool ModifyNextSpawn = false;

        [HarmonyPrefix]
        public static bool Prefix(PuckManager __instance)
        {
            if (!PluginCore.config.EnableJohnsFaceoff) return true;

            if (!ModifyNextSpawn)
            {
                PluginCore.Log("Spawn modification state faulty -- skipping faceoff tweak");
                return true;
            }

            FieldInfo fi = typeof(PuckManager).GetField("puckPositions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null)
            {
                PluginCore.Log("Failed to find puckPositions, skipping faceoff modification");
                return true;
            }

            var puckPositions = fi.GetValue(__instance) as List<PuckPosition>;

            foreach (PuckPosition pos in puckPositions)
            {
                if (pos == null) continue;
                if (pos.Phase != GamePhase.Play) continue;

                Vector3 spawnPos = new Vector3(0f, 0.5f, 0f);
                Quaternion spawnRot = pos.transform.rotation;
                Vector3 baseVel = new Vector3(0f, 6f, 0f);

                PluginCore.Log("Spawning puck for faceoff");
                Puck newPuck = __instance.Server_SpawnPuck(spawnPos, spawnRot);
                newPuck.Rigidbody.linearVelocity = baseVel;

                ModifyNextSpawn = false;

                return false;
            }

            PluginCore.Log("Could not find faceoff puck.");

            return true;

        }
    }
}
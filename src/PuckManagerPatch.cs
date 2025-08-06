using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(PuckManager), "Server_SpawnPucksForPhase")]
    public class PuckManagerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PuckManager __instance)
        {
            if (NetworkBehaviourSingleton<GameManager>.Instance.Phase == GamePhase.Playing)
            {
                if (!NetworkManager.Singleton.IsServer) return false;
                Debug.Log("[PuckManager] Spawning 1 puck for phase Playing");
                __instance.Server_SpawnPuck(new Vector3(0, UnityEngine.Random.Range(2f, 3f), 0), new Quaternion(UnityEngine.Random.Range(5f, 15f), UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(10f, 30f)), new Vector3(0, UnityEngine.Random.Range(-15, -1), 0));
                return false;
            }
            else return true;
        }
    }
}
using System;
using HarmonyLib;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(GoalController), "OnNetworkSpawn")]
    public class GoalControllerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GoalController __instance, ref Goal ___goal)
        {
            Transform postCollider = null;
            for (int i = 0; i < ___goal.transform.childCount; i++)
            {
                Transform child = ___goal.transform.GetChild(i);
                if (child.name.Contains("Goal Post Collider"))
                {
                    postCollider = child;
                }
            }

            if (postCollider == null)
            {
                PluginCore.Log("Post collider not found.");
                return;
            }

            foreach (CapsuleCollider col in postCollider.GetComponents<CapsuleCollider>()) col.material.bounciness = PluginCore.config.postBounciness;
            
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(Puck), "OnNetworkPostSpawn")]
    public class PuckPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Puck __instance, ref float ___maxSpeed, ref Vector3 ___stickTensor)
        {
            __instance.transform.localScale = new Vector3(PluginCore.config.PuckScale,
                PluginCore.config.PuckScale, PluginCore.config.PuckScale);
            ___maxSpeed = PluginCore.config.PuckMaxSpeed;
            ___stickTensor = new Vector3(PluginCore.config.PuckStickTensorX,
                PluginCore.config.PuckStickTensorY, PluginCore.config.PuckStickTensorZ);
            __instance.Rigidbody.linearDamping = PluginCore.config.PuckDrag;
            __instance.Rigidbody.mass = PluginCore.config.PuckMass;

            if (PluginCore.config.EnableMidStickCollider)
            {
                foreach (Stick stick in UnityEngine.Object.FindObjectsByType<Stick>(FindObjectsSortMode.None))
                {
                    Physics.IgnoreCollision(__instance.StickCollider, stick.GetComponent<BoxCollider>());
                    Physics.IgnoreCollision(__instance.IceCollider, stick.GetComponent<BoxCollider>());
                }
            }
        }
    }

    // [HarmonyPatch(typeof(Puck), "FixedUpdate")]
    // public class HeightDragTweak
    // {
    //     [HarmonyPrefix]
    //     public static void Prefix(Puck __instance)
    //     {
    //         Rigidbody puckRB = __instance.Rigidbody;
    //         if (puckRB.position.y > 4f) puckRB.linearDamping = 0.43f;
    //         else puckRB.linearDamping = 0.29f;
    //         puckRB.AddForce(new Vector3(0, -10.6f, 0), ForceMode.Acceleration);
    //     }
    // }
}
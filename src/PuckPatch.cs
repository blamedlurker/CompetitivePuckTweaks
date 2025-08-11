using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.InputSystem.Controls;

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

    // [HarmonyPatch(typeof(Puck), "OnCollisionExit")]
    // public class PuckColliderSkip
    // {
    //     [HarmonyPrefix]
    //     public static bool Prefix()
    //     {
    //         return false;
    //     }
    // }

    [HarmonyPatch(typeof(Puck), "FixedUpdate")]
    public class HeightDragTweak
    {
        [HarmonyPostfix]
        public static void Postfix(Puck __instance)
        {
            if (!PluginCore.config.PuckDragSpeedDependence) return;
            float delta = __instance.Rigidbody.linearVelocity.magnitude - PluginCore.config.PuckNominalSpeed;
            float newDrag = PluginCore.config.PuckDrag * (1 + PluginCore.config.PuckDragFactor * delta * delta * delta);
            __instance.Rigidbody.linearDamping = Mathf.Max(PluginCore.config.PuckDrag, newDrag);
        }
    }
}
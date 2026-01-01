using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.InputSystem.Controls;
using System.Numerics;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(Puck), "OnNetworkPostSpawn")]
    public class PuckPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Puck __instance, ref float ___maxSpeed, ref UnityEngine.Vector3 ___stickTensor)
        {
            __instance.transform.localScale = new UnityEngine.Vector3(PluginCore.config.PuckScale,
                PluginCore.config.PuckScale, PluginCore.config.PuckScale);
            PluginCore.Log($"Puck scaled to {PluginCore.config.PuckScale}");
            ___maxSpeed = PluginCore.config.PuckMaxSpeed;
            ___stickTensor = new UnityEngine.Vector3(PluginCore.config.PuckStickTensorX,
                PluginCore.config.PuckStickTensorY, PluginCore.config.PuckStickTensorZ);
            __instance.Rigidbody.linearDamping = PluginCore.config.PuckDrag;
            __instance.Rigidbody.mass = PluginCore.config.PuckMass;
            __instance.StickCollider.hasModifiableContacts = true;
            __instance.IceCollider.hasModifiableContacts = true;
            PluginCore.PuckIDs.Add(__instance.StickCollider.GetInstanceID());
            PluginCore.PuckIDs.Add(__instance.IceCollider.GetInstanceID());

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
            // experimental soft floor setup
            // UnityEngine.Vector3 centerOnIce = new UnityEngine.Vector3(__instance.IceCollider.bounds.center.x, -float.MaxValue, __instance.IceCollider.bounds.center.z);
            // UnityEngine.Vector3 closestPoint = __instance.IceCollider.ClosestPoint(centerOnIce);

            // PluginCore.Log($"closestPoint: {closestPoint}\npuckCenter: {__instance.transform.position}");

            // float depth = closestPoint.y - PluginCore.config.PuckFloor;

            // if (depth < 0)
            // {
            //     UnityEngine.Vector3 yVelocity = new UnityEngine.Vector3(0, __instance.Rigidbody.linearVelocity.y, 0);
            //     UnityEngine.Vector3 deltaV = new UnityEngine.Vector3(0, -depth * PluginCore.config.FloorFactor, 0) - yVelocity;
            //     __instance.Rigidbody.AddForceAtPosition(deltaV, closestPoint, ForceMode.VelocityChange);

            // }
            
            if (!PluginCore.config.PuckDragSpeedDependence) return;
            float delta = __instance.Rigidbody.linearVelocity.magnitude - PluginCore.config.PuckNominalSpeed;
            float newDrag = PluginCore.config.PuckDrag * (1 + PluginCore.config.PuckDragFactor * delta * delta * delta);
            __instance.Rigidbody.linearDamping = Mathf.Max(PluginCore.config.PuckDrag, newDrag);

            if (PluginCore.config.PuckHeightDependentDrag)
            {
                if (__instance.Rigidbody.position.y > PluginCore.config.PuckHeightLimit && __instance.Rigidbody.linearVelocity.y > 0)
                {
                    float overheight = __instance.Rigidbody.position.y - PluginCore.config.PuckHeightLimit;
                    float heightDrag = PluginCore.config.PuckHeightDragFactor * overheight;
                    __instance.Rigidbody.AddForce(new UnityEngine.Vector3(0f, -heightDrag, 0f), ForceMode.VelocityChange);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Puck), "OnNetworkDespawn")]
    public class PuckDespawnPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Puck __instance)
        {
            if (PluginCore.PuckIDs.Contains(__instance.StickCollider.GetInstanceID())) { PluginCore.PuckIDs.Remove(__instance.StickCollider.GetInstanceID()); }
            if (PluginCore.PuckIDs.Contains(__instance.IceCollider.GetInstanceID())) { PluginCore.PuckIDs.Remove(__instance.IceCollider.GetInstanceID()); }
        }
    }
}
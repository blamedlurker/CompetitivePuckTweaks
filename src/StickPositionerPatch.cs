using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{

    // old function: for goalie stick reach limit

    // [HarmonyPatch(typeof(StickPositioner), "PositionBladeTarget")]
    // public class StickControllerFixedUpdatePatch
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix(StickPositioner __instance, ref float ___maximumReach)
    //     {
    //         if (__instance.Player.Role.Value == PlayerRole.Goalie)
    //         {
    //             if (__instance.PlayerBody.IsSliding.Value)
    //             {
    //                 ___maximumReach = PluginCore.config.GoalieMaximumReach;
    //                 Vector3 playerProjection = Vector3.ProjectOnPlane(__instance.Stick.PlayerBody.transform.forward, Vector3.up);
    //                 Vector3 stickProjection = Vector3.ProjectOnPlane(__instance.Stick.transform.forward, Vector3.up);
    //                 float angle = Vector3.Angle(playerProjection, stickProjection);
    //                 ___maximumReach -= Mathf.Clamp((90 - angle) * 0.01f, 0f, PluginCore.config.GoalieReachDropoff);
    //             }
    //             else
    //             {
    //                 ___maximumReach = 2.5f; // Normal reach for non-sliding goalies
    //             }
    //         }
    //     }
    // }

    [HarmonyPatch(typeof(StickPositioner), "Awake")]
    public class StickPositionerAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(StickPositioner __instance, ref float ___softCollisionForce, ref Vector3 ___bladeTargetFocusPointInitialLocalPosition)
        {
            ___softCollisionForce = PluginCore.config.SoftCollisionForce;
            ___bladeTargetFocusPointInitialLocalPosition += new Vector3(0, PluginCore.config.BladeTargetFocusPointOffsetY, 0);
        }
            
    }
}
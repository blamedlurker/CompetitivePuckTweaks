using System;
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
        public static void Postfix(StickPositioner __instance, ref float ___softCollisionForce, ref Vector3 ___bladeTargetFocusPointInitialLocalPosition, ref float ___outputMin, ref float ___outputMax, ref float ___proportionalGain, ref float ___integralGain, ref float ___derivativeGain)
        {
            ___softCollisionForce = PluginCore.config.SoftCollisionForce;
            ___bladeTargetFocusPointInitialLocalPosition += new Vector3(0, PluginCore.config.BladeTargetFocusPointOffsetY, 0);
            // if (PluginCore.config.AlterStickPositionerOutput)
            // {
            //     float newOutput = PluginCore.config.StickPositionerOutputMax;
            //     if (__instance.PlayerBody.Player.Role.Value == PlayerRole.Goalie) newOutput = PluginCore.config.GoaliePositionerOutputMax;

            //     ___outputMin = -newOutput;
            //     ___outputMax = newOutput;
            // }
            
            if (PluginCore.config.EnableStickSpeedDecay) __instance.gameObject.AddComponent<FloatComponent>();
        }
    }

    [HarmonyPatch(typeof(StickPositioner), "FixedUpdate")]
    public class StickFixedUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(StickPositioner __instance, ref float ___outputMin, ref float ___outputMax)
        {
            float defaultValue = __instance.Player.Role.Value == PlayerRole.Goalie ? PluginCore.config.GoaliePositionerOutputMax : PluginCore.config.StickPositionerOutputMax;
            
            if (!PluginCore.config.AlterStickPositionerOutput) return;
            else if (!PluginCore.config.EnableStickSpeedDecay)
            {
                ___outputMin = -defaultValue;
                ___outputMax = defaultValue;
                return;
            }

            FloatComponent runningAvg = __instance.gameObject.GetComponent<FloatComponent>();

            runningAvg.value += ((__instance.Stick.Rigidbody.GetPointVelocity(__instance.Stick.BladeHandlePosition) - __instance.PlayerBody.Rigidbody.linearVelocity).magnitude - runningAvg.value) / PluginCore.config.StickSpeedDecaySpan;

            if (runningAvg.value > PluginCore.config.StickSpeedDecayLimit && ___outputMax > PluginCore.config.StickSpeedDecayMin)
            {
                ___outputMin += PluginCore.config.StickSpeedDecayRate * (runningAvg.value - PluginCore.config.StickSpeedDecayLimit);
                ___outputMax = -___outputMin;
            }
            else if (___outputMax < defaultValue)
            {
                ___outputMin = Mathf.Min(___outputMin - 20f, defaultValue);
                ___outputMax = -___outputMin;
            }
        }
    }
}
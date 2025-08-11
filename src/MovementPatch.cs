using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(Movement), "Start")]
    public class MovementPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Movement __instance, ref float ___turnAcceleration, ref float ___turnBrakeAcceleration, ref float ___turnMaxSpeed, ref float ___turnDrag,
                                    ref float ___maxBackwardsSpeed, ref float ___maxBackwardsSprintSpeed, ref float ___maxForwardsSpeed, ref float ___maxForwardsSprintSpeed)
        {
            ___turnDrag = PluginCore.config.TurnDrag;
            if (__instance.PlayerBody.Player.PlayerPosition.Role == PlayerRole.Attacker)
            {
                ___maxBackwardsSpeed = PluginCore.config.MaxBackwardsSpeed;
                ___maxBackwardsSprintSpeed = PluginCore.config.MaxBackwardsSprintSpeed;
                ___maxForwardsSpeed = PluginCore.config.MaxForwardsSpeed;
                ___maxForwardsSprintSpeed = PluginCore.config.MaxForwardsSprintSpeed;
            }
        }
    }

    [HarmonyPatch(typeof(Movement), "FixedUpdate")]
    public class ScalingTurnSpeedPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Movement __instance, ref float ___turnAcceleration, ref float ___turnBrakeAcceleration, ref float ___turnMaxSpeed,
            ref float ___forwardsAcceleration, ref float ___forwardsSprintAcceleration, ref float ___backwardsAcceleration, ref float ___backwardsSprintAcceleration)
        {
            float speed = __instance.Speed;
            ___turnAcceleration = PluginCore.config.TurnAccelerationBase - speed * PluginCore.config.TurnAccelerationScaling;
            ___turnBrakeAcceleration = PluginCore.config.TurnBrakeAccelerationBase - speed * PluginCore.config.TurnBrakeAccelerationScaling;
            ___turnMaxSpeed = PluginCore.config.TurnMaxSpeedBase - speed * PluginCore.config.TurnMaxSpeedScaling;
            ___forwardsAcceleration = Mathf.Max(PluginCore.config.ForwardsAccelerationBase -
                    speed * PluginCore.config.ForwardsAccelerationScaling, PluginCore.config.ForwardsAccelerationMin);
            ___forwardsSprintAcceleration = Mathf.Max(PluginCore.config.ForwardsSprintAccelerationBase -
                    speed * PluginCore.config.ForwardsSprintAccelerationScaling, PluginCore.config.ForwardsSprintAccelerationMin);
            ___backwardsAcceleration = Mathf.Max(PluginCore.config.BackwardsAccelerationBase -
                    speed * PluginCore.config.BackwardsAccelerationScaling);
            ___backwardsSprintAcceleration = Mathf.Max(PluginCore.config.BackwardsSprintAccelerationBase -
                    speed * PluginCore.config.BackwardsSprintAccelerationScaling, PluginCore.config.BackwardsSprintAccelerationMin);

        }
    }

}
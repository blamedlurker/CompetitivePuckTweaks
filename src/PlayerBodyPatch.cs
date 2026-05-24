using System;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(PlayerBody), "OnNetworkPostSpawn")]
    public class PlayerBodyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerBody __instance, ref float ___slideTurnMultiplier,
         ref float ___stopDrag, ref float ___balanceRecoveryTime, ref PlayerMesh ___playerMesh, ref float ___slideDrag, ref float ___tackleForceMultiplier,
         ref float ___tackleForceThreshold, ref float ___tackleSpeedThreshold)
        {
            if (__instance.Player.IsReplay.Value) return;

            if (GameManager.Instance.Phase == GamePhase.FaceOff)
            {
                if (__instance.Player.PlayerPosition.Name == "C")
                {
                    if (__instance.Player.Team == PlayerTeam.Blue) __instance.transform.position += new UnityEngine.Vector3(0, 0, PluginCore.config.CenterSpawnOffset);
                    else __instance.transform.position -= new UnityEngine.Vector3(0, 0, PluginCore.config.CenterSpawnOffset);
                }
            }

            ___slideTurnMultiplier = PluginCore.config.SlideTurnMultiplier;
            ___stopDrag = PluginCore.config.StopDrag;
            ___balanceRecoveryTime = PluginCore.config.BalanceRecoveryTime;
            ___tackleForceMultiplier = PluginCore.config.TackleForceMultiplier;
            ___tackleForceThreshold = PluginCore.config.TackleForceThreshold;
            ___tackleSpeedThreshold = PluginCore.config.TackleSpeedThreshold;
            __instance.GetComponent<CapsuleCollider>().radius *= PluginCore.config.TorsoColliderRadiusFactor;
            // cc.enabled = false;
            // BoxCollider newTorso = __instance.gameObject.AddComponent<BoxCollider>();
            __instance.GetComponent<SphereCollider>().radius *= PluginCore.config.HeadColliderRadiusFactor;
            // newTorso.center = new Vector3(0, 1.55f, 0);
            // newTorso.size = new Vector3(0.5f, 0.75f, 0.1f);
            // newTorso.excludeLayers = cc.excludeLayers;
            // newTorso.includeLayers = cc.includeLayers;
            // newTorso.material = cc.material;
            // newTorso.transform.forward = __instance.transform.forward;

            bool isGoalie = (__instance.name.Contains("Goalie"));

            if (PluginCore.config.EnableSmallerModels)
            {
                if (isGoalie)
                {
                    ___playerMesh.PlayerGroin.transform.localPosition += new Vector3(0, 0.1f, 0);
                }

                else
                {

                    ___playerMesh.PlayerTorso.GetComponentInChildren<MeshFilter>().mesh = PluginCore.torsoMesh;
                    ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().sharedMesh = PluginCore.torsoMesh;
                    ___playerMesh.PlayerTorso.transform.localPosition += new Vector3(0, 0.27f, 0);
                    ___playerMesh.PlayerGroin.GetComponentInChildren<MeshFilter>().mesh = PluginCore.groinMesh;
                    ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().sharedMesh = PluginCore.groinMesh;

                }
            }

            ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;
            ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;
            ___playerMesh.PlayerHead.GetComponentInChildren<SphereCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;

            if (isGoalie) return;

            if (PluginCore.config.EnablePuckThroughBodies && !__instance.Player.IsReplay.Value)
            {
                ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
                ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
                ___playerMesh.PlayerHead.GetComponentInChildren<SphereCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
            }

            if (PluginCore.config.EnablePuckThroughGroin && !__instance.Player.IsReplay.Value)
            {
                ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
            }

            ___slideDrag = PluginCore.config.SlideDrag;

            if (PluginCore.config.ThinSkaterBodies)
            {
                ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().transform.localScale = new Vector3(PluginCore.config.SkaterThinningFactor, 1, PluginCore.config.SkaterThinningFactor);
                ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().transform.localScale = new Vector3(PluginCore.config.SkaterThinningFactor, 1, PluginCore.config.SkaterThinningFactor);
            }


        }
    }

    [HarmonyPatch(typeof(PlayerBody), "FixedUpdate")]
    public class PlayerBodyFixedUpdatePatch
    {

        [HarmonyPrefix]
        public static void Prefix(PlayerBody __instance, ref float ___staminaRegenerationRate)
        {
            if (__instance.Player.Role == PlayerRole.Goalie)
            {
                if (__instance.IsSliding.Value)
                    ___staminaRegenerationRate = PluginCore.config.ButterflyStamRegenRate;
                else
                    ___staminaRegenerationRate = 10f;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(PlayerBody __instance)
        {
            if (!__instance.IsUpright && !__instance.IsSideways) __instance.OnFall();
        }
    }

    [HarmonyPatch(typeof(PlayerBody), "DashLeft")]
    public class PlayerBodyDashLeftPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerBody __instance, ref bool ___canDash, ref NetworkVariable<bool> ___IsSliding, ref float ___dashVelocity,
        ref NetworkVariable<bool> ___IsStopping, ref float ___dashStaminaDrain)
        {
            if (__instance.Player.IsReplay.Value) return true;
            if (!___canDash) return false;
            if (Mathf.Abs(__instance.transform.worldToLocalMatrix.MultiplyVector(__instance.Rigidbody.linearVelocity).x) > PluginCore.config.GoalieDashSpeedLimit) ___dashVelocity *= 0.5f;
            else ___dashVelocity = 6f;
            if (!PluginCore.config.EnableGoalieMicrodash) return true;
            if (___IsSliding.Value) return true;
            if (___IsStopping.Value) return true;
            if (__instance.IsJumping) return true;
            if (!(__instance.Rigidbody.linearVelocity.magnitude > 3.2f) && __instance.Stamina.Value > 0.5f * ___dashStaminaDrain)
            {

                __instance.Rigidbody.AddForce(-__instance.transform.right * ___dashVelocity * 0.55f, ForceMode.VelocityChange);
                __instance.Stamina.Value -= PluginCore.config.MicrodashStamCostFraction * ___dashStaminaDrain;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(PlayerBody __instance, ref bool ___HasDashExtended, ref NetworkVariable<bool> ___IsExtendedRight, ref Tween ___dashLegPadTween)
        {
            if (!PluginCore.config.ExtraLegPadTweening || __instance.Player.Role != PlayerRole.Goalie) return;
            if (___dashLegPadTween != null) ___dashLegPadTween.Kill(false);
            ___IsExtendedRight.Value = false;
            LegPadHelper helper = __instance.GetComponent<LegPadHelper>();
            __instance.StartCoroutine(helper.PostDashLeftStateChange());
        }
    }

    [HarmonyPatch(typeof(PlayerBody), "DashRight")]
    public class PlayerBodyDashRightPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerBody __instance, ref bool ___canDash, ref NetworkVariable<bool> ___IsSliding, ref float ___dashVelocity, ref NetworkVariable<bool> ___IsStopping, ref float ___dashStaminaDrain)
        {
            if (__instance.Player.IsReplay.Value) return true;
            if (!___canDash) return false;
            if (Mathf.Abs(__instance.transform.worldToLocalMatrix.MultiplyVector(__instance.Rigidbody.linearVelocity).x) > PluginCore.config.GoalieDashSpeedLimit) ___dashVelocity *= 0.5f;
            else ___dashVelocity = 6f;
            if (!PluginCore.config.EnableGoalieMicrodash) return true;
            if (___IsSliding.Value) return true;
            if (___IsStopping.Value) return true;
            if (__instance.IsJumping) return true;
            if (!(__instance.Rigidbody.linearVelocity.magnitude > 3.2f) && __instance.Stamina.Value > 0.5f * ___dashStaminaDrain)
            {
                __instance.Rigidbody.AddForce(__instance.transform.right * ___dashVelocity * 0.55f, ForceMode.VelocityChange);
                __instance.Stamina.Value -= PluginCore.config.MicrodashStamCostFraction * ___dashStaminaDrain;
                return false;
            }
            return true;

        }

        [HarmonyPostfix]
        public static void Postfix(PlayerBody __instance, ref bool ___HasDashExtended, ref NetworkVariable<bool> ___IsExtendedLeft, ref Tween ___dashLegPadTween)
        {
            if (!PluginCore.config.ExtraLegPadTweening || __instance.Player.Role != PlayerRole.Goalie) return;
            if (___dashLegPadTween != null) ___dashLegPadTween.Kill(false);
            ___IsExtendedLeft.Value = false;
            LegPadHelper helper = __instance.GetComponent<LegPadHelper>();
            __instance.StartCoroutine(helper.PostDashRightStateChange());
        }
    }

    // [HarmonyPatch(typeof(PlayerBody), "OnCollisionEnter")]
    // public class CollisionPatch
    // {
    //     [HarmonyTranspiler]
    //     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         bool foundGroundedCheck = false;
    //         int startIndex = -1;
    //         int endIndex = -1;

    //         var codes = new List<CodeInstruction>(instructions);
    //         // foreach (CodeInstruction codeInstruction in codes) PluginCore.Log(codeInstruction.ToString());
    //         for (int i = 0; i < codes.Count; i++)
    //         {
    //             if (codes[i].opcode == OpCodes.Ret)
    //             {
    //                 if (foundGroundedCheck)
    //                 {
    //                     endIndex = i;
    //                     break;
    //                 }
    //                 else
    //                 {
    //                     startIndex = i + 1;
    //                     for (int j = startIndex; j < codes.Count; j++)
    //                     {
    //                         if (codes[j].opcode == OpCodes.Ret) break;
    //                         string strOp = codes[j].ToString();
    //                         if (strOp == "call bool PlayerBody::get_IsGrounded()")
    //                         {
    //                             foundGroundedCheck = true;
    //                             break;
    //                         }
    //                     }
    //                 }
    //             }
    //         }

    //         if (startIndex > -1 && endIndex > -1)
    //         {
    //             codes[startIndex].opcode = OpCodes.Nop;
    //             codes.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
    //         }

    //         return codes.AsEnumerable();
    //     }
    // }


}
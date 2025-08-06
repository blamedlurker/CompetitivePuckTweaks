using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(PlayerBodyV2), "OnNetworkPostSpawn")]
    public class PlayerBodyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerBodyV2 __instance, ref float ___slideTurnMultiplier,
         ref float ___stopDrag, ref float ___balanceRecoveryTime, ref PlayerMesh ___playerMesh)
        {
            ___slideTurnMultiplier = PluginCore.config.SlideTurnMultiplier;
            ___stopDrag = PluginCore.config.StopDrag;
            ___balanceRecoveryTime = PluginCore.config.BalanceRecoveryTime;
            __instance.GetComponent<CapsuleCollider>().radius = PluginCore.config.PlayerColliderRadius;
            __instance.GetComponent<CapsuleCollider>().height = PluginCore.config.PlayerColliderHeight;

            if (PluginCore.config.EnableSmallerModels)
            {
                if (__instance.name.Contains("Goalie"))
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

            if (PluginCore.config.EnablePuckThroughBodies && !__instance.name.Contains("Goalie") && !__instance.Player.IsReplay.Value)
            {
                ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
                ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
                ___playerMesh.PlayerHead.GetComponentInChildren<SphereCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
            }

            if (PluginCore.config.EnablePuckThroughGroin && !__instance.name.Contains("Goalie") && !__instance.Player.IsReplay.Value)
            {
                ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().excludeLayers |= (1 << LayerMask.NameToLayer("Puck"));
            }

            ___playerMesh.PlayerTorso.GetComponentInChildren<MeshCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;
            ___playerMesh.PlayerGroin.GetComponentInChildren<MeshCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;
            ___playerMesh.PlayerHead.GetComponentInChildren<SphereCollider>().material.bounciness = PluginCore.config.PlayerColliderBounciness;
        }
    }

    [HarmonyPatch(typeof(PlayerBodyV2), "FixedUpdate")]
    public class PlayerBodyFixedUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerBodyV2 __instance)
        {
            if (!__instance.IsUpright && !__instance.IsSideways) __instance.OnFall();
        }
    }

    [HarmonyPatch(typeof(PlayerBodyV2), "DashLeft")]
    public class PlayerBodyDashLeftPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerBodyV2 __instance, ref bool ___canDash, ref NetworkVariable<bool> ___IsSliding, ref float ___dashVelocity,
        ref NetworkVariable<bool> ___IsStopping, ref float ___dashStaminaDrain)
        {
            if (!PluginCore.config.EnableGoalieMicrodash) return true;
            if (!___canDash) return false;
            if (___IsSliding.Value) return true;
            if (___IsStopping.Value) return true;
            if (__instance.IsJumping) return true;
            if (!(__instance.Rigidbody.linearVelocity.magnitude > 3.2f) && __instance.Stamina > 0.5f * ___dashStaminaDrain)
            {

                __instance.Rigidbody.AddForce(-__instance.transform.right * ___dashVelocity * 0.55f, ForceMode.VelocityChange);
                __instance.Stamina -= PluginCore.config.MicrodashStamCostFraction * ___dashStaminaDrain;
                return false; // Skip original method execution
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerBodyV2), "DashRight")]
    public class PlayerBodyDashRightPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerBodyV2 __instance, ref bool ___canDash, ref NetworkVariable<bool> ___IsSliding, ref float ___dashVelocity, ref NetworkVariable<bool> ___IsStopping, ref float ___dashStaminaDrain)
        {
            if (!PluginCore.config.EnableGoalieMicrodash) return true;
            if (!___canDash) return false;
            if (___IsSliding.Value) return true;
            if (___IsStopping.Value) return true;
            if (__instance.IsJumping) return true;
            if (!(__instance.Rigidbody.linearVelocity.magnitude > 3.2f) && __instance.Stamina > 0.5f * ___dashStaminaDrain)
            {

                __instance.Rigidbody.AddForce(__instance.transform.right * ___dashVelocity * 0.55f, ForceMode.VelocityChange);
                __instance.Stamina -= PluginCore.config.MicrodashStamCostFraction * ___dashStaminaDrain;
                return false;
            }
            return true;

        }
    }


}
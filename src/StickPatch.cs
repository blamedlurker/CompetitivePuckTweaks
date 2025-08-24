using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(Stick), "OnNetworkPostSpawn")]
    public class StickPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Stick __instance, ref GameObject ___shaftHandle, ref float ___shaftHandleProportionalGain)
        {
            ___shaftHandleProportionalGain = PluginCore.config.ShaftHandleProportionalGain;

            BoxCollider boxCollider = null;

            StickMesh newStickMesh = __instance.gameObject.GetComponentInChildren<StickMesh>();

            if (newStickMesh == null) { PluginCore.Log($"StickMesh is null!"); return; }

            if (__instance == null) { Debug.LogError($"[{Constants.MOD_NAME}] Stick null on network post spawn"); return; }

            // find meshcolliders
            MeshCollider[] newMeshColliders = newStickMesh.transform.GetComponentsInChildren<MeshCollider>();

            foreach (MeshCollider mC in newMeshColliders)
            {
                mC.hasModifiableContacts = true;
                PluginCore.StickMeshes.Add(mC.GetInstanceID(), __instance);
            }

            __instance.Rigidbody.mass = PluginCore.config.StickMass;

            if (PluginCore.config.DisableShaftCollision == false) return;

            if (PluginCore.config.EnableMidStickCollider)
            {

                boxCollider = __instance.gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new UnityEngine.Vector3(0.029f, 0.14f, 1.21f);
                boxCollider.center += new UnityEngine.Vector3(0, 0, 0.145f);
                boxCollider.hasModifiableContacts = true;
                PluginCore.StickMeshes.Add(boxCollider.GetInstanceID(), __instance);

                foreach (Puck puck in UnityEngine.Object.FindObjectsByType<Puck>(FindObjectsSortMode.None))
                {
                    Physics.IgnoreCollision(puck.IceCollider, boxCollider);
                    Physics.IgnoreCollision(puck.StickCollider, boxCollider);
                }
            }

            foreach (Stick stick in UnityEngine.Object.FindObjectsByType<Stick>(FindObjectsSortMode.None))
            {
                StickMesh oldStickMesh = stick.gameObject.GetComponentInChildren<StickMesh>();
                BoxCollider oldHandleCollider = stick.GetComponent<BoxCollider>();

                Physics.IgnoreCollision(newStickMesh.BladeCollider, oldStickMesh.ShaftCollider);
                Physics.IgnoreCollision(oldStickMesh.BladeCollider, newStickMesh.ShaftCollider);

                if (PluginCore.config.EnableMidStickCollider)
                {
                    Physics.IgnoreCollision(oldStickMesh.ShaftCollider, boxCollider);
                    Physics.IgnoreCollision(newStickMesh.ShaftCollider, boxCollider);
                    Physics.IgnoreCollision(newStickMesh.ShaftCollider, oldHandleCollider);
                    Physics.IgnoreCollision(oldStickMesh.ShaftCollider, oldHandleCollider);
                }

                foreach (MeshCollider collider in oldStickMesh.gameObject.GetComponentsInChildren<MeshCollider>())
                {
                    foreach (MeshCollider newCollider in newMeshColliders)
                    {
                        if ((collider.gameObject.tag.Contains("Stick Blade") && newCollider.gameObject.tag.Contains("Stick Shaft")) ||
                            (newCollider.gameObject.tag.Contains("Stick Blade") && collider.gameObject.tag.Contains("Stick Shaft")) ||
                            (collider.gameObject.tag.Contains("Stick Shaft") && newCollider.gameObject.tag.Contains("Stick Shaft")))
                        {
                            Physics.IgnoreCollision(collider, newCollider);
                        }
                        if (PluginCore.config.EnableMidStickCollider)
                        {
                            if (collider.gameObject.tag.Contains("Stick Shaft"))
                            {
                                Physics.IgnoreCollision(collider, boxCollider);
                                Physics.IgnoreCollision(collider, oldHandleCollider);
                            }
                            if (newCollider.gameObject.tag.Contains("Stick Shaft"))
                            {
                                Physics.IgnoreCollision(newCollider, boxCollider);
                                Physics.IgnoreCollision(newCollider, oldHandleCollider);
                            }
                        }
                    }
                }

                PluginCore.Log($"Collision ignorance updated.");
            }
        }
    }

    [HarmonyPatch(typeof(Stick), nameof(Stick.OnNetworkDespawn))]
    public class DespawnPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Stick __instance)
        {
            BoxCollider thisBoxCol;
            if (PluginCore.config.BananaMode) return false;
            if (!PluginCore.config.ConstrainStickOnStick) return true;
            StickMesh thisStickMesh = __instance.gameObject.GetComponentInChildren<StickMesh>();
            if (PluginCore.config.EnableMidStickCollider) { thisBoxCol = __instance.GetComponent<BoxCollider>(); }
            else { thisBoxCol = null; }

            MeshCollider[] colliders = thisStickMesh.GetComponentsInChildren<MeshCollider>();
            if (PluginCore.config.EnableMidStickCollider && PluginCore.StickMeshes.ContainsKey(thisBoxCol.GetInstanceID())) PluginCore.StickMeshes.Remove(thisBoxCol.GetInstanceID());
            foreach (MeshCollider col in colliders) if (PluginCore.StickMeshes.ContainsKey(col.GetInstanceID())) PluginCore.StickMeshes.Remove(col.GetInstanceID());
            return true;
        }
    }
}
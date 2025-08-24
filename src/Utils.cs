using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Win32;
using Unity.Collections;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    public class UtilObj
    {

        public void LoadListeners()
        {
            PluginCore.Log("Loading ModificationEvent...");
            Physics.ContactModifyEvent += this.ModificationEvent;
            PluginCore.Log("ModificationEvent loaded");
        }

        public void UnloadListeners()
        {
            Physics.ContactModifyEvent -= this.ModificationEvent;
            PluginCore.Log("ModificationEvent unloaded");
        }

        public void ModificationEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
        {
            for (int j = 0; j < pairs.Count(); j++)
            {
                ModifiableContactPair pair = pairs[j];
                if (PluginCore.StickMeshes.Keys.Contains(pair.colliderInstanceID) && PluginCore.StickMeshes.Keys.Contains(pair.otherColliderInstanceID)) for (int i = 0; i < pair.contactCount; i++)
                    {
                        // float maxImpulse = pair.GetMaxImpulse(i);
                        // bool flag = Mathf.Abs((PluginCore.StickMeshes[pair.colliderInstanceID].Rigidbody.linearVelocity - PluginCore.StickMeshes[pair.colliderInstanceID].PlayerBody.Rigidbody.linearVelocity).normalized.y) > PluginCore.config.StickConstraintThreshold;
                        // flag |= Mathf.Abs((PluginCore.StickMeshes[pair.otherColliderInstanceID].Rigidbody.linearVelocity - PluginCore.StickMeshes[pair.colliderInstanceID].PlayerBody.Rigidbody.linearVelocity).normalized.y) > PluginCore.config.StickConstraintThreshold;
                        // if (!flag)
                        // {
                        //     pair.SetTargetVelocity(i, UnityEngine.Vector3.zero);
                        // }
                        Stick stick = PluginCore.StickMeshes[pair.colliderInstanceID];
                        Stick otherStick = PluginCore.StickMeshes[pair.otherColliderInstanceID];

                        ModifiableMassProperties newMass = pair.massProperties;

                        float colZ = stick.transform.InverseTransformPoint(pair.GetPoint(i)).z;
                        float otherColZ = otherStick.transform.InverseTransformPoint(pair.GetPoint(i)).z;

                        if (colZ < 0f && otherColZ < 0f) return;

                        float stickMass = Mathf.Clamp((Mathf.Clamp(colZ, -1f, 0.5f) + 1f) * 1.25f, 0.001f, 1.7f);
                        float otherStickMass = Mathf.Clamp((Mathf.Clamp(otherColZ, -1f, 0.5f) + 1f) * 1.25f, 0.001f, 1.7f);

                        // -1.2 (butt) to 1.6

                        // UIChat.Instance.Server_SendSystemChatMessage($"colZ: {colZ}<br>otherColZ: {otherColZ}<br>default: {pair.GetPoint(i)}");

                        float newScale = stickMass;
                        float otherNewScale = otherStickMass;

                        newMass.inverseInertiaScale = newScale;
                        newMass.inverseMassScale = newScale;
                        newMass.otherInverseInertiaScale = otherNewScale;
                        newMass.otherInverseMassScale = otherNewScale;

                        // bool isHandle = false;

                        // if (UnityEngine.Vector3.Distance(pair.GetPoint(i), stick.BladeHandlePosition) > UnityEngine.Vector3.Distance(pair.GetPoint(i), stick.ShaftHandlePosition - 1.1f * stick.transform.localToWorldMatrix.MultiplyVector(-stick.transform.forward)))
                        // {
                        //     newMass.inverseInertiaScale = 0.15f;
                        //     newMass.inverseMassScale = 0.15f;
                        //     isHandle = true;
                        // }
                        // if (UnityEngine.Vector3.Distance(pair.GetPoint(i), otherStick.BladeHandlePosition) > UnityEngine.Vector3.Distance(pair.GetPoint(i), otherStick.ShaftHandlePosition - 1.1f * otherStick.transform.localToWorldMatrix.MultiplyVector(-otherStick.transform.forward)))
                        // {
                        //     newMass.otherInverseInertiaScale = 0.1f;
                        //     newMass.otherInverseMassScale = 0.1f;
                        // }
                        // else if (!isHandle)
                        // {
                        //     newMass.inverseInertiaScale = 2f;
                        //     newMass.inverseMassScale = 2f;
                        //     newMass.otherInverseInertiaScale = 2f;
                        //     newMass.otherInverseMassScale = 2f;
                        // }
                        pair.massProperties = newMass;

                    }
            }
        }
    }
}
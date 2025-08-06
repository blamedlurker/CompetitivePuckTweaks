using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    public class UtilObj
    {
        public UtilObj()
        {
            PluginCore.Log("Testing UtilObj loading...");
            this.ModificationEvent(new UnityEngine.PhysicsScene(), new NativeArray<ModifiableContactPair>());
        }

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

            foreach (ModifiableContactPair pair in pairs)
            {
                if (PluginCore.StickMeshes.Keys.Contains(pair.colliderInstanceID) && PluginCore.StickMeshes.Keys.Contains(pair.otherColliderInstanceID)) for (int i = 0; i < pair.contactCount; i++)
                    {
                        float maxImpulse = pair.GetMaxImpulse(i);
                        pair.SetMaxImpulse(i, (Mathf.Abs(PluginCore.StickMeshes[pair.colliderInstanceID].localToWorldMatrix.MultiplyVector(pair.GetNormal(i)).y) > 0.5f ? maxImpulse : 0f));
                    }
            }
        }
    }
}
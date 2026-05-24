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
            Physics.ContactModifyEventCCD += this.ModificationEvent;
            PluginCore.Log("ModificationEvent loaded");
        }

        public void UnloadListeners()
        {
            Physics.ContactModifyEvent -= this.ModificationEvent;
            Physics.ContactModifyEventCCD -= this.ModificationEvent;
            PluginCore.Log("ModificationEvent unloaded");
        }

        public void ModificationEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
        {
            try
            {
                bool ignoreCollision = false;
                for (int j = 0; j < pairs.Count(); j++)
                {

                    ModifiableContactPair pair = pairs[j];

                    bool colIsPuck = PluginCore.PuckIDs.Contains(pair.colliderInstanceID);
                    bool otherColIsPuck = PluginCore.PuckIDs.Contains(pair.otherColliderInstanceID);
                    bool colIsStick = PluginCore.StickMeshes.Keys.Contains(pair.colliderInstanceID);
                    bool otherColIsStick = PluginCore.StickMeshes.Keys.Contains(pair.otherColliderInstanceID);


                    if ((colIsPuck && otherColIsStick) || (colIsStick && otherColIsPuck))
                    {
                        // PluginCore.Log($"Stick-puck collision detected with {pairs.Count()} contact pairs and {pair.contactCount} contacts in pair {j}");
                        ModifiableMassProperties newMass = pair.massProperties;
                        float colZ = 0;
                        Stick stick = null;

                        if (colIsStick)
                        {
                            newMass.inverseMassScale = PluginCore.config.StickOnPuckInverseMass;
                            newMass.inverseInertiaScale = PluginCore.config.StickOnPuckInverseMass;

                            stick = PluginCore.StickMeshes[pair.colliderInstanceID];
                        }
                        if (otherColIsStick)
                        {
                            newMass.otherInverseMassScale = PluginCore.config.StickOnPuckInverseMass;
                            newMass.otherInverseInertiaScale = PluginCore.config.StickOnPuckInverseMass;

                            stick = PluginCore.StickMeshes[pair.otherColliderInstanceID];
                        }
                        pair.massProperties = newMass;
                        if (!PluginCore.config.ModifyPuckOnHandle) continue;
                        for (int i = 0; i < pair.contactCount; i++) if (stick.transform.InverseTransformPoint(pair.GetPoint(i)).z < colZ) colZ = stick.transform.InverseTransformPoint(pair.GetPoint(i)).z;
                        if (colZ < PluginCore.config.PuckOnHandleCollisionLimit && stick.Player.Role != PlayerRole.Goalie) ignoreCollision = true;
                    }
                    else if (colIsStick && otherColIsStick)
                        for (int i = 0; i < pair.contactCount; i++)
                        {
                            Stick stick = PluginCore.StickMeshes[pair.colliderInstanceID];
                            Stick otherStick = PluginCore.StickMeshes[pair.otherColliderInstanceID];

                            ModifiableMassProperties newMass = pair.massProperties;

                            float colZ = stick.transform.InverseTransformPoint(pair.GetPoint(i)).z;
                            float otherColZ = otherStick.transform.InverseTransformPoint(pair.GetPoint(i)).z;

                            if (colZ < 0f && otherColZ < 0f) return;

                            float stickMass = Mathf.Clamp((Mathf.Clamp(colZ, -1f, 0.5f) + 1f) * 1.25f, 0.001f, 1.7f);
                            float otherStickMass = Mathf.Clamp((Mathf.Clamp(otherColZ, -1f, 0.5f) + 1f) * 1.25f, 0.001f, 1.7f);

                            float newScale = stickMass;
                            float otherNewScale = otherStickMass;

                            newMass.inverseInertiaScale = newScale;
                            newMass.inverseMassScale = newScale;
                            newMass.otherInverseInertiaScale = otherNewScale;
                            newMass.otherInverseMassScale = otherNewScale;

                            pair.massProperties = newMass;

                        }
                }
                if (ignoreCollision)
                {
                    foreach (ModifiableContactPair pair in pairs)
                    {
                        for (int i = 0; i < pair.contactCount; i++) pair.IgnoreContact(i);
                    }
                }
            }
            catch (Exception e) { PluginCore.Log($"Exception in modification event: {e.Message}"); }
        }
    }
}
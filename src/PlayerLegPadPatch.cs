using UnityEngine;
using HarmonyLib;
using DG.Tweening;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins.Options;
using AYellowpaper.SerializedCollections;
using System.Reflection;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace CompetitivePuckTweaks.src
{

    [HarmonyPatch(typeof(PlayerLegPad), "OnStateChanged")]
    public class PlayerLegPadTweenPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerLegPad __instance, PlayerLegPadState oldState, PlayerLegPadState newState, ref Tween ___localPositionTween, ref Tween ___localRotationTween, ref SerializedDictionary<PlayerLegPadState, Transform> ___positions, ref float ___transitionDuration)
        {
            if (!PluginCore.config.ExtraLegPadTweening) return true;

            PlayerBody body = __instance.GetComponentInParent<PlayerBody>();

            if (body.Player.Role  != PlayerRole.Goalie) return true;

            Vector3 newPosition = ___positions[newState].localPosition;
            LegPadHelper helper = body.GetComponent<LegPadHelper>();

            FieldInfo localPositionField = typeof(PlayerLegPad).GetField("localPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo localRotationField = typeof(PlayerLegPad).GetField("localRotation", BindingFlags.NonPublic | BindingFlags.Instance);

            bool shouldExtend = newState == PlayerLegPadState.Butterfly;
            bool isLeft = __instance.transform.localPosition.x < 0;
            if (shouldExtend) newPosition += isLeft ? helper.GetCurrentLeftDiff(newState) : helper.GetCurrentRightDiff(newState);

            Tween tween = ___localPositionTween;
            if (tween != null)
            {
                tween.Kill(false);
            }
            Tween tween2 = ___localRotationTween;
            if (tween2 != null)
            {
                tween2.Kill(false);
            }

            ___localPositionTween = DOTween.To(() => (Vector3)localPositionField.GetValue(__instance), delegate (Vector3 value)
            {
                localPositionField.SetValue(__instance, value);
            }, newPosition, ___transitionDuration);
            ___localRotationTween = DOTween.To<Quaternion, Quaternion, NoOptions>(PureQuaternionPlugin.Plug(), () => (Quaternion)localRotationField.GetValue(__instance), delegate (Quaternion value)
            {
                localRotationField.SetValue(__instance, value);
            }, ___positions[newState].localRotation, ___transitionDuration);

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerLegPad), "FixedUpdate")]
    public class PlayerLegPadFixedUpdate
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerLegPad __instance, ref Tween ___localPositionTween, ref SerializedDictionary<PlayerLegPadState, Transform> ___positions, ref float ___transitionDuration)
        {
            if (!PluginCore.config.ExtraLegPadTweening) return true;
            PlayerBody body = __instance.GetComponentInParent<PlayerBody>();
            LegPadHelper helper = body.GetComponent<LegPadHelper>();
            bool isLeft = __instance.transform.localPosition.x < 0;

            if (__instance.State == PlayerLegPadState.Butterfly && !___localPositionTween.IsActive())
            {
                Vector3 diff = isLeft ? helper.GetCurrentLeftDiff(__instance.State) : helper.GetCurrentRightDiff(__instance.State);

                FieldInfo localPositionField = typeof(PlayerLegPad).GetField("localPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                localPositionField.SetValue(__instance, ___positions[__instance.State].localPosition + diff);
            }

            return false;
        }
    }

}
using UnityEngine;
using HarmonyLib;
using AYellowpaper.SerializedCollections;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(PlayerLegPad), "Awake")]
    public class PlayerLegPadPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerLegPad __instance, ref SerializedDictionary<PlayerLegPadState, Transform> ___positions)
        {
            if (___positions.ContainsKey(PlayerLegPadState.Butterfly))
            {
                Transform legPadPosition = ___positions[PlayerLegPadState.Butterfly];
                if (legPadPosition.localPosition.x > 0) legPadPosition.localPosition += new Vector3(PluginCore.config.ButterflyPadOffset, 0, 0);
                else legPadPosition.localPosition -= new Vector3(PluginCore.config.ButterflyPadOffset, 0, 0);
                ___positions[PlayerLegPadState.Butterfly] = legPadPosition;
            }
            else PluginCore.Log("Leg pad butterfly position NOT found");
        }
    }
}
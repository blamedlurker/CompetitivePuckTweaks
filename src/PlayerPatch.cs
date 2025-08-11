// using System;
// using UnityEngine;
// using HarmonyLib;

// namespace CompetitivePuckTweaks.src
// {
//     [HarmonyPatch(typeof(Player), nameof(Player.Server_SpawnCharacter))]
//     public class PlayerSpawnPatch
//     {
//         [HarmonyPostfix]
//         public static void Postfix(Player __instance, UnityEngine.Vector3 position, Quaternion rotation, PlayerRole role)
//         {
//             if (__instance.PlayerPosition.Name == "C")
//             {
//                 __instance.transform.position += __instance.transform.forward * (__instance.Team.Value == PlayerTeam.Blue ? 1.9f : -1.9f);
//             }
//         }
//     }

//     [HarmonyPatch(typeof(Player), nameof(Player.Server_RespawnCharacter))]
//     public class PlayerRespawnPatch
//     {
//         [HarmonyPostfix]
//         public static void Postfix(Player __instance, UnityEngine.Vector3 position, Quaternion rotation, PlayerRole role)
//         {
//             UIChat.Instance.Server_SendSystemChatMessage("PlayerRespawn Postfix called.");
//             if (__instance.PlayerPosition.Name == "C")
//             {
//                 UIChat.Instance.Server_SendSystemChatMessage("Player is C");
//                 __instance.transform.position += (__instance.Team.Value == PlayerTeam.Blue ? new UnityEngine.Vector3(0, 0, 1.5f) : new UnityEngine.Vector3(0, 0, -1.5f));
//             }
//         }
//     }
// }
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;

namespace CompetitivePuckTweaks.src
{
    // [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.LoadLevel1Scene))]
    // public class SceneManagerPatch
    // {
    //     [HarmonyPostfix]
    //     public static void Postfix()
    //     {
    //         try
    //         {
    //             PluginCore.Log("Loading listeners...");
    //             if (PluginCore.config.ConstrainStickOnStick) PluginCore.utilObj.LoadListeners();
    //             PluginCore.Log("Listeners loaded.");
    //         }
    //         catch (Exception e)
    //         {
    //             PluginCore.Log($"Failed to load listeners: {e}");
    //         }
    //     }
    // }
}
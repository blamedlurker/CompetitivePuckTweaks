using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.CodeDom;
using MonoMod.Utils;
using System.IO;

namespace CompetitivePuckTweaks.src
{
    [HarmonyPatch(typeof(UIChatController), "Event_Server_OnChatCommand")]
    public class UIChatControllerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(UIChatController __instance, Dictionary<string, object> message)
        {
            ulong clientId = (ulong)message["clientId"];
            string command = (string)message["command"];
            string[] args = (string[])message["args"];

            if (command == "/v" || command == "/version") { UIChat.Instance.Server_SendSystemChatMessage(Constants.MOD_VERSION, clientId); return; }
            else if (command == "/gc" || command == "/getconfig")
            {
                try
                {
                    PropertyInfo configField = typeof(ModConfig).GetProperty(args[0]);
                    UIChat.Instance.Server_SendSystemChatMessage($"<i>{configField.Name}</i> is currently set to <b>{configField.GetValue(PluginCore.config)}</b>", clientId);
                }
                catch (Exception e)
                {
                    UIChat.Instance.Server_SendSystemChatMessage($"Failed to get config value: {e.Message}", clientId);
                }
                return;
            }
            else if (command == "/gf" || command == "/getfields" || command == "/girlfriend")
            {
                UIChat.Instance.Server_SendSystemChatMessage("<b>Modifiable Fields</b>", clientId);
                foreach (PropertyInfo girlfriend in typeof(ModConfig).GetProperties()) UIChat.Instance.Server_SendSystemChatMessage($"{girlfriend}", clientId);
            }

            if (!PluginCore.config.OpenConfigChanges && !ServerManager.Instance.AdminSteamIds.Contains(PlayerManager.Instance.GetPlayerByClientId(clientId).SteamId.Value.ToString())) return;

            switch (command)
            {
                case "/resetserver":
                    GameManager.Instance.Server_SetPhase(GamePhase.FaceOff, 1);
                    GameManager.Instance.Server_SetPhase(GamePhase.Warmup, ServerManager.Instance.ServerConfigurationManager.ServerConfiguration.phaseDurationMap[GamePhase.Warmup]);
                    break;
                case "/killserver":
                    Application.Quit();
                    break;
                case "/setconfig":
                case "/sc":
                    try
                    {
                        SetConfig(args);
                        UIChat.Instance.Server_SendSystemChatMessage($"<i>{args[0]}</i> changed to <b>{args[1]}</b>");

                    }
                    catch (Exception e)
                    {
                        UIChat.Instance.Server_SendSystemChatMessage($"Unable to change config: {e.Message}", clientId);
                    }
                    break;
                case "/saveconfig":
                    try
                    {
                        string path;
                        if (args.Length > 0) path = args[0];
                        else path = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                        PluginCore.config.SaveToFile(path, true);
                        UIChat.Instance.Server_SendSystemChatMessage($"Config saved to <i>{args[0]}</i>", clientId);
                    }
                    catch (Exception e)
                    {
                        UIChat.Instance.Server_SendSystemChatMessage($"Failed to save config: {e.Message}", clientId);
                    }
                    break;
                case "/loadconfig":
                    try
                    {
                        string path;
                        if (args.Length > 0) path = args[0];
                        else path = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                        PluginCore.config = ModConfig.LoadFromFile(path);
                        UIChat.Instance.Server_SendSystemChatMessage($"Config file loaded.");
                        UIChat.Instance.Server_SendSystemChatMessage($"Load path: <i>{path}</i>", clientId);
                    }
                    catch (Exception e)
                    {
                        UIChat.Instance.Server_SendSystemChatMessage($"Failed to load config: {e.Message}", clientId);
                    }
                    break;

            }
        }

        /// <summary>
        /// Helper function for changing mod config values in-game
        /// </summary>
        /// <param name="args">array of args from command</param>
        public static void SetConfig(string[] args)
        {
            PropertyInfo targetInfo = typeof(ModConfig).GetProperty(args[0]);
            PluginCore.Log($"Property name: {targetInfo.Name}");
            PluginCore.Log($"property type: {targetInfo.PropertyType.Name} versus float: {typeof(float).Name}");
            if (targetInfo.PropertyType == typeof(float))
            {
                float.TryParse(args[1], out float newValue);
                targetInfo.SetValue(PluginCore.config, newValue);
            }
            else if (targetInfo.PropertyType.Equals(typeof(bool)))
            {
                bool.TryParse(args[1], out bool newValue);
                targetInfo.SetValue(PluginCore.config, newValue);
            }
            else if (targetInfo.PropertyType.Equals(typeof(int)))
            {
                int.TryParse(args[1], out int newValue);
                targetInfo.SetValue(PluginCore.config, newValue);
            }
        }
    }
}
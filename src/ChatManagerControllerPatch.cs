using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.CodeDom;
using MonoMod.Utils;
using System.IO;
using Unity.Netcode;

namespace CompetitivePuckTweaks.src
{
    public class ChatManagerControllerPatch
    {
        public static void ChatCommandListener(Dictionary<string, object> message)
        {
            ulong clientId = (ulong)message["clientId"];
            string command = (string)message["command"];
            string[] args = (string[])message["args"];

            if (command == "/cptv" || command == "/cptversion") { ChatManager.Instance.Server_SendChatMessage(Constants.MOD_VERSION, "white", new ulong[] { clientId }); return; }

            else if (command == "/gc" || command == "/getconfig")
            {
                try
                {
                    PropertyInfo configField = typeof(ModConfig).GetProperty(args[0]);
                    ChatManager.Instance.Server_SendChatMessage($"<i>{configField.Name}</i> is currently set to <b>{configField.GetValue(PluginCore.config)}</b>", "green", new ulong[] { clientId });
                }
                catch (Exception e)
                {
                    ChatManager.Instance.Server_SendChatMessage($"Failed to get config value: {e.Message}", "red", new ulong[] { clientId });
                }
                return;
            }
            else if (command == "/gf" || command == "/getfields" || command == "/girlfriend")
            {
                int i = 1;
                if (args.Length > 0) int.TryParse(args[0], out i);
                ChatManager.Instance.Server_SendChatMessage($"<b>Modifiable Fields</b> <i>Page {i}</i>", "white", new ulong[] { clientId });
                PropertyInfo[] properties = typeof(ModConfig).GetProperties();
                for (int j = 20 * (i - 1); j < properties.Length && j < 20 * i; j++)
                {
                    ChatManager.Instance.Server_SendChatMessage($"{properties[j]} = {properties[j].GetValue(PluginCore.config)}", "white", new ulong[] { clientId });
                }
            }

            else switch (command)
                {
                    case "/resetserver":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        GameManager.Instance.Server_SetGameState(GamePhase.FaceOff);
                        GameManager.Instance.Server_SetGameState(GamePhase.Warmup);
                        break;
                    case "/killserver":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        Application.Quit();
                        break;
                    case "/setconfig":
                    case "/sc":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        try
                        {
                            SetConfig(args);
                            ChatManager.Instance.Server_BroadcastChatMessage($"<i>{args[0]}</i> changed to <b>{args[1]}</b> by {PlayerManager.Instance.GetPlayerByClientId(clientId).Username.Value}", "green");

                        }
                        catch (Exception e)
                        {
                            ChatManager.Instance.Server_SendChatMessage($"Unable to change config: {e.Message}", "red", new ulong[] { clientId });
                        }
                        break;
                    case "/saveconfig":
                    case "/save":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        try
                        {
                            string path;
                            if (args.Length > 0) path = args[0];
                            else path = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                            PluginCore.config.SaveToFile(path, true);
                            ChatManager.Instance.Server_SendChatMessage($"Config saved to <i>{path}</i>", "green", new ulong[] { clientId });
                        }
                        catch (Exception e)
                        {
                            ChatManager.Instance.Server_SendChatMessage($"Failed to save config: {e.Message}", "red", new ulong[] { clientId });
                        }
                        break;
                    case "/loadconfig":
                    case "/load":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        try
                        {
                            string path;
                            if (args.Length > 0) path = args[0];
                            else path = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                            PluginCore.config = ModConfig.LoadFromFile(path);
                            ChatManager.Instance.Server_BroadcastChatMessage($"Config file loaded.", "green");
                            ChatManager.Instance.Server_SendChatMessage($"Load path: <i>{path}</i>", "white", new ulong[] { clientId });
                        }
                        catch (Exception e)
                        {
                            ChatManager.Instance.Server_SendChatMessage($"Failed to load config: {e.Message}", "red", new ulong[] { clientId });
                        }
                        break;
                    case "/forcesync":
                    case "/fs":
                        if (!CheckAdminStateForCommand(command, clientId)) return;
                        foreach (Player player in PlayerManager.Instance.GetPlayers())
                        {
                            ulong id = player.OwnerClientId;
                            PluginCore.ManualSync(id);
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
            if (targetInfo.Name == "PuckScale")
            {
                foreach (Puck puck in PuckManager.Instance.GetPucks()) puck.transform.localScale = Vector3.one * PluginCore.config.PuckScale;
                foreach (Player player in PlayerManager.Instance.GetPlayers()) PluginCore.ManualSync(player.OwnerClientId);
            }
            if (targetInfo.Name == "ExtraLegPadTweening" || targetInfo.Name == "LegPadReturnTime" || targetInfo.Name == "LegPadSpreadVelocityFactor") foreach (Player player in PlayerManager.Instance.GetPlayers()) PluginCore.ManualSync(player.OwnerClientId);
        }

        public static bool CheckAdminStateForCommand(string command, ulong clientId)
        {
            if (!PluginCore.config.OpenConfigChanges && !ServerManager.Instance.AdminManager.IsSteamIdAdmin(PlayerManager.Instance.GetPlayerByClientId(clientId).SteamId.Value.ToString()))
            {
                ChatManager.Instance.Server_SendChatMessage($"Command {command} is restricted to server admins.", "red", new ulong[] { clientId });
                return false;
            }
            return true;
        }
    }
}
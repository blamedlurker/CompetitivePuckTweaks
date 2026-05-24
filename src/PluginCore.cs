using UnityEngine;
using UnityEngine.Rendering;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Collections.Generic;
using Unity.Netcode;
using GLTFast;


namespace CompetitivePuckTweaks.src
{

    public class PluginCore : IPuckPlugin
    {
        public Harmony _harmony = new Harmony("_harmony");
        public static Mesh torsoMesh;
        public static Mesh groinMesh;
        public static ModConfig config = new ModConfig();
        public static Dictionary<int, Stick> StickMeshes = new Dictionary<int, Stick>();
        public static List<int> PuckIDs = new List<int>();
        public static UtilObj utilObj = new UtilObj();
        private bool EventListenersPresent = false;

        /// <summary>
        /// Core plugin enable function
        /// </summary>
        /// <returns>bool status of enable success</returns>
        public bool OnEnable()
        {
            PluginCore.Log($"CPT version {Constants.MOD_VERSION} is installed.");

            if (!(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null))
            {
                PluginCore.Log($"[WARNING] This mod is only intended for servers. Continue at your own risk.");
            }
            PluginCore.Log($"Enabling...");

            try
            {
                PluginCore.Log($"Loading configuration...");
                if (Directory.Exists(Path.Combine(".", "config")))
                {
                    string configPath = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                    if (File.Exists(configPath))
                    {
                        config = ModConfig.LoadFromFile(configPath);
                        if (config.DisableShaftCollision == false) config.EnableMidStickCollider = false;
                        PluginCore.Log($"Configuration loaded from {configPath}");
                        config.SaveToFile(configPath);
                    }
                    else
                    {
                        config = new ModConfig();
                        PluginCore.Log($"Configuration file not found at {configPath}, creating file with defaults.");
                        config.SaveToFile(configPath);
                    }
                }
                else
                {
                    PluginCore.Log($"Config directory not found, creating new directory and config file with defaults.");
                    Directory.CreateDirectory(Path.Combine(".", "config"));
                    string configPath = Path.Combine(".", "config", "CompetitivePuckTweaks.json");
                    config = new ModConfig();
                    config.SaveToFile(configPath);
                }


                PluginCore.Log($"Current configuration: {JsonSerializer.Serialize(config)}");

                if (NetworkManager.Singleton != null)
                    InjectComponents();
                else
                {
                    PluginCore.Log($"Waiting for network manager...");
                    var coroutineHost = new GameObject("CPTCoroutineHost");
                    UnityEngine.Object.DontDestroyOnLoad(coroutineHost);
                    var runner = coroutineHost.AddComponent<MonoBehaviour>();
                    runner.StartCoroutine(WaitForNetworkManager());
                }

                if (config.UsePhysicsModificationEvents) utilObj = new UtilObj();

                _harmony.PatchAll();

                if (config.UsePhysicsModificationEvents) utilObj.LoadListeners();

                foreach (MethodBase method in _harmony.GetPatchedMethods()) PluginCore.Log($"Patched method: {method.ReflectedType}.{method.Name}");

                if (config.EnableSmallerModels)
                {
                    DefinePlayerMeshes();
                }

                if (config.DisableStickCollision) Physics.IgnoreLayerCollision(6, 6, true);

                Time.fixedDeltaTime = config.FixedDeltaTime;
                Physics.defaultSolverIterations = config.SolverIterations;

                EventManager.AddEventListener("Event_Server_OnApprovedClientConnected", SendSyncMessage);
                EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", CheckGamePhase);
                EventManager.AddEventListener("Event_Server_OnChatCommand", ChatManagerControllerPatch.ChatCommandListener);
                Log("Sync message listener added.");

                return true;
            }
            catch (Exception e)
            {
                PluginCore.Log($"Failed to enable: {e}");
                return false;
            }
        }

        /// <summary>
        /// Core plugin disable function.
        /// </summary>
        /// <returns>bool corresponding to success or failure of disable</returns>
        public bool OnDisable()
        {
            if (!(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null))
            {
                PluginCore.Log($"[WARNING] This mod is only intended for servers. Continue at your own risk.");
            }
            PluginCore.Log($"Disabling...");
            try
            {
                if (NetworkManager.Singleton != null)
                    RemoveInjectedComponents();
                else
                {
                    PluginCore.Log($"Waiting for network manager...");
                    var coroutineHost = new GameObject("CPTCoroutineHost");
                    UnityEngine.Object.DontDestroyOnLoad(coroutineHost);
                    var runner = coroutineHost.AddComponent<MonoBehaviour>();
                    runner.StartCoroutine(WaitForNetworkManagerForRemoval());
                }

                _harmony.UnpatchSelf();
                if (config.UsePhysicsModificationEvents) utilObj.UnloadListeners();
                if (EventListenersPresent) EventManager.RemoveEventListener("Event_Server_OnApprovedClientConnected", SendSyncMessage);
                Log($"Mod disabled.");
                return true;
            }
            catch (Exception e)
            {
                PluginCore.Log($"Failed to disable: {e}");
                return false;
            }
        }

        private System.Collections.IEnumerator WaitForNetworkManager()
        {
            while (NetworkManager.Singleton == null)
                yield return null;
            InjectComponents();
        }

        private System.Collections.IEnumerator WaitForNetworkManagerForRemoval()
        {
            while (NetworkManager.Singleton == null)
                yield return null;
            RemoveInjectedComponents();
        }

        private void InjectComponents()
        {
            NetworkManager nm = NetworkManager.Singleton;
            Log($"Checking prefabs for injection...");
            foreach (var entry in nm.NetworkConfig.Prefabs.Prefabs)
            {
                var prefab = entry.Prefab;
                if (prefab == null) continue;

                PlayerBody body = prefab.GetComponent<PlayerBody>();

                if (body != null && prefab.name.Contains("Goalie"))
                {
                    if (prefab.GetComponent<LegPadHelper>() == null)
                    {
                        var helper = prefab.AddComponent<LegPadHelper>();
                        helper.legPadLeft = body.PlayerMesh.PlayerLegPadLeft;
                        helper.legPadRight = body.PlayerMesh.PlayerLegPadRight;
                        helper.playerRigidbody = body.Rigidbody;
                        var field = typeof(NetworkObject).GetField("m_ChildNetworkBehaviours", BindingFlags.NonPublic | BindingFlags.Instance);
                        field.SetValue(prefab.GetComponent<NetworkObject>(), null);
                        PluginCore.Log($"Injected LegPadHelper into {prefab.name}");
                    }
                }
            }


        }

        private void RemoveInjectedComponents()
        {
            NetworkManager nm = NetworkManager.Singleton;
            foreach (var entry in nm.NetworkConfig.Prefabs.Prefabs)
            {
                var prefab = entry.Prefab;
                if (prefab == null) continue;

                PlayerBody body = prefab.GetComponent<PlayerBody>();

                if (body != null)
                {
                    if (prefab.GetComponent<LegPadHelper>() != null)
                    {
                        UnityEngine.Object.Destroy(prefab.GetComponent<LegPadHelper>());
                        PluginCore.Log($"LegPadHelper removed from {prefab.name}");
                    }
                }
            }
        }

        /// <summary>
        /// Loads custom meshes (CURRENTLY DEPRECATED)
        /// </summary>
        public static void DefinePlayerMeshes()
        {

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(path, "shrunk_torso"));
            AssetBundle groinBundle = AssetBundle.LoadFromFile(Path.Combine(path, "groin"));

            Mesh importTorsoMesh = assetBundle.LoadAsset("assets/meshes/shrunk_torso.fbx", typeof(Mesh)) as Mesh;
            Mesh importGroinMesh = groinBundle.LoadAsset("assets/shrunk_groin.blend", typeof(Mesh)) as Mesh;

            PluginCore.Log($"Assets in groinBundle: {string.Join(", ", groinBundle.GetAllAssetNames())}");

            torsoMesh = UnityEngine.Object.Instantiate(importTorsoMesh);
            groinMesh = UnityEngine.Object.Instantiate(importGroinMesh);

            torsoMesh.Optimize();
            groinMesh.Optimize();
            torsoMesh.RecalculateNormals();
            groinMesh.RecalculateNormals();
            torsoMesh.RecalculateTangents();
            groinMesh.RecalculateTangents();
            torsoMesh.RecalculateBounds();
            groinMesh.RecalculateBounds();

            PluginCore.Log($"PlayerTorso mesh defined with {torsoMesh.vertexCount} vertices and {torsoMesh.triangles.Length / 3} triangles.");
            PluginCore.Log($"PlayerGroin mesh defined with {groinMesh.vertexCount} vertices and {groinMesh.triangles.Length / 3} triangles.");
        }

        /// <summary>
        /// Logs a message formatted with mod name
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public static void Log(string message)
        {
            Debug.Log($"[{Constants.MOD_NAME}] " + message);
        }

        /// <summary>
        /// Sends named custom message for syncing client config with server
        /// </summary>
        /// <param name="message">Input dictionary with connection information</param>
        public void SendSyncMessage(Dictionary<string, object> message)
        {
            ulong targetId = (ulong)message["clientId"];
            Log($"Sending config sync message to client {targetId}...");

            ConfigSyncPackage messageContent = new ConfigSyncPackage(config);
            var writer = new FastBufferWriter(1024, Unity.Collections.Allocator.Temp);
            var customMessagingManager = NetworkManager.Singleton.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValueSafe(messageContent);
                customMessagingManager.SendNamedMessage("CPT_sync_config", targetId, writer);
                Log($"Config sync sent to client {targetId}");
            }
        }

        public void CheckGamePhase(Dictionary<string, object> message)
        {
            if (!PluginCore.config.EnableJohnsFaceoff || message == null) return;

            GameState oldState = (GameState)message["oldGameState"];
            GameState newState = (GameState)message["newGameState"];

            GamePhase oldPhase = oldState.Phase;
            GamePhase newPhase = newState.Phase;

            if (newPhase == GamePhase.FaceOff)
            {
                PuckManagerPatch.ModifyNextSpawn = true;
                return;
            }
            else if (oldPhase == GamePhase.FaceOff && newPhase == GamePhase.Play) return;
            else PuckManagerPatch.ModifyNextSpawn = false;
        }

        public static void ManualSync(ulong targetId)
        {
            Log($"Sending config sync message to client {targetId}...");

            ConfigSyncPackage messageContent = new ConfigSyncPackage(config);
            var writer = new FastBufferWriter(1024, Unity.Collections.Allocator.Temp);
            var customMessagingManager = NetworkManager.Singleton.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValueSafe(messageContent);
                customMessagingManager.SendNamedMessage("CPT_sync_config", targetId, writer);
                Log($"Config sync sent to client {targetId}");
            }
        }
    }
}
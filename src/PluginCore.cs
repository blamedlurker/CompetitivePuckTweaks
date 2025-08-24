using UnityEngine;
using UnityEngine.Rendering;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using SingularityGroup.HotReload;
using System.Collections.Generic;

namespace CompetitivePuckTweaks.src
{

    public class PluginCore : IPuckMod
    {
        public Harmony _harmony = new Harmony("_harmony");
        public static Mesh torsoMesh;
        public static Mesh groinMesh;
        public static ModConfig config = new ModConfig();
        public static Dictionary<int, Stick> StickMeshes = new Dictionary<int, Stick>();
        public static UtilObj utilObj = new UtilObj();

        /// <summary>
        /// Core plugin enable function
        /// </summary>
        /// <returns>bool status of enable success</returns>
        public bool OnEnable()
        {
            PluginCore.Log($"CPT version {Constants.MOD_VERSION} is installed.");

            if (!(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null))
            {
                PluginCore.Log($"This mod is only intended for servers.");
                return false;
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
                if (config.ConstrainStickOnStick) utilObj = new UtilObj();

                _harmony.PatchAll();

                if (config.ConstrainStickOnStick) utilObj.LoadListeners();

                foreach (MethodBase method in _harmony.GetPatchedMethods()) PluginCore.Log($"Patched method: {method.ReflectedType}.{method.Name}");

                if (config.EnableSmallerModels)
                {
                    DefinePlayerMeshes();
                }

                if (config.DisableStickCollision) Physics.IgnoreLayerCollision(6, 6, true);

                Time.fixedDeltaTime = config.FixedDeltaTime;
                Physics.defaultSolverIterations = config.SolverIterations;


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
                PluginCore.Log($"This mod is only intended for servers.");
                return false;
            }
            PluginCore.Log($"Disabling...");
            try
            {
                _harmony.UnpatchSelf();
                // if (config.ConstrainStickOnStickDim) Utils.UnloadListeners();
                return true;
            }
            catch (Exception e)
            {
                PluginCore.Log($"Failed to disable: {e}");
                return false;
            }
        }

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

        public static void Log(string message)
        {
            Debug.Log($"[{Constants.MOD_NAME}] " + message);
        }
    }
}
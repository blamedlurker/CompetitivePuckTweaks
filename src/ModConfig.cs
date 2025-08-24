using System;
using System.Text.Json;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace CompetitivePuckTweaks.src
{
    public class ModConfig
    {
        // fields for movement configuration

        public float TurnAccelerationBase { get; set; } = 1.825f;
        public float TurnBrakeAccelerationBase { get; set; } = 5.8f;
        public float TurnMaxSpeedBase { get; set; } = 1.48f;
        public float TurnAccelerationScaling { get; set; } = 0f;
        public float TurnBrakeAccelerationScaling { get; set; } = 0f;
        public float TurnMaxSpeedScaling { get; set; } = 0f;
        public float TurnDrag { get; set; } = 3f;
        public float MaxBackwardsSpeed { get; set; } = 7.5f;
        public float MaxBackwardsSprintSpeed { get; set; } = 8.75f;
        public float AngularForceMultiplier { get; set; } = 5.22f;
        public float ForwardsAccelerationBase { get; set; } = 2f;
        public float ForwardsSprintAccelerationBase { get; set; } = 4.75f;
        public float ForwardsAccelerationMin { get; set; } = 2f;
        public float ForwardsSprintAccelerationMin { get; set; } = 4f;
        public float BackwardsAccelerationBase { get; set; } = 1.8f;
        public float BackwardsAccelerationMin { get; set; } = 1.8f;
        public float BackwardsSprintAccelerationBase { get; set; } = 2f;
        public float BackwardsSprintAccelerationMin { get; set; } = 2f;
        public float ForwardsAccelerationScaling { get; set; } = 0;
        public float ForwardsSprintAccelerationScaling { get; set; } = 0;
        public float BackwardsAccelerationScaling { get; set; } = 0;
        public float BackwardsSprintAccelerationScaling { get; set; } = 0;
        public float MaxForwardsSpeed { get; set; } = 7.5f;
        public float MaxForwardsSprintSpeed { get; set; } = 8.75f;

        // fields for player body configuration

        public float SlideTurnMultiplier { get; set; } = 2f;
        public float StopDrag { get; set; } = 2.5f;
        public float BalanceRecoveryTime { get; set; } = 2f;
        public bool EnableGoalieMicrodash { get; set; } = false;
        public float MicrodashStamCostFraction { get; set; } = 0.75f;
        public bool EnableSmallerModels { get; set; } = false;
        public float TorsoColliderRadiusFactor { get; set; } = 1f;
        public float HeadColliderRadiusFactor { get; set; } = 1f;
        public float PlayerColliderHeight { get; set; } = 1.5f;
        public float PlayerColliderBounciness { get; set; } = 0.05f;
        public float SlideDrag { get; set; } = 0.2f;
        public float CenterSpawnOffset { get; set; } = 0f;

        // fields for puck configuration
        public float PuckMaxSpeed { get; set; } = 30f;
        public float PuckStickTensorX { get; set; } = 0.003f;
        public float PuckStickTensorY { get; set; } = 0.001f;
        public float PuckStickTensorZ { get; set; } = 0.003f;
        public float PuckScale { get; set; } = 1f;
        public float PuckDrag { get; set; } = 0.3f;
        public float PuckMass { get; set; } = 0.375f;
        public bool RandomPuckDrop { get; set; } = true;
        public bool EnablePuckThroughBodies { get; set; } = false;
        public bool EnablePuckThroughGroin { get; set; } = true;
        public bool PuckDragSpeedDependence { get; set; } = false;
        public float PuckNominalSpeed { get; set; } = 30f;
        public float PuckDragFactor { get; set; } = 0.0025f;

        // fields for stick configuration
        public bool DisableStickCollision { get; set; } = false;
        public bool DisableShaftCollision { get; set; } = false;
        public bool EnableMidStickCollider { get; set; } = false;
        public int MidStickColliderSize { get; set; } = 0;
        public float StickMass { get; set; } = 1.1f;
        public bool AlterStickPositionerOutput { get; set; } = false;
        public float ShaftHandleProportionalGain { get; set; } = 500f;
        public float StickPositionerOutputMax { get; set; } = 750f;
        public float StickConstraintThreshold { get; set; } = 0.3f;

        // fields for stick positioner configuration
        public float SoftCollisionForce { get; set; } = 20f;
        public float BladeTargetFocusPointOffsetY { get; set; } = 0f;
        // public bool LimitGoalieReach { get; set; } = false;
        // public float GoalieMaximumReach { get; set; } = 2.5f;
        // public float GoalieReachDropoff { get; set; } = 0.8f;

        // fields for arena configuration
        public float postBounciness { get; set; } = 0f;

        // fields for physics configuration
        public float FixedDeltaTime { get; set; } = 0.01f;
        public int SolverIterations { get; set; } = 6;
        public bool ConstrainStickOnStick { get; set; } = true;

        // fields for mod configuration
        public bool OpenConfigChanges { get; set; } = false;

        // jokes
        public bool BananaMode { get; set; } = false;

        public ModConfig()
        {
            // Default constructor initializes with default values
        }

        public static ModConfig LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ModConfig(); // Return default config if file does not exist
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<ModConfig>(json) ?? new ModConfig();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load config from {filePath}: {ex.Message}");
                return new ModConfig(); // Return default config on error
            }
        }

        public void SaveToFile(string filePath, bool newfile = false)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                if (!File.Exists(filePath) && newfile) File.Create(filePath);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save config to {filePath}: {ex.Message}");
            }
        }


    }
}
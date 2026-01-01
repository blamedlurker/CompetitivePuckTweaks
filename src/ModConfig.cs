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
        public float GoalieTurnAcceleration { get; set; } = 1.825f;
        public float GoalieTurnBrakeAcceleration { get; set; } = 5.8f;
        public float GoalieTurnMaxSpeed { get; set; } = 1.48f;
        public float GoalieTurnDrag { get; set; } = 3f;
        public float MaxBackwardsSpeed { get; set; } = 7.5f;
        public float MaxBackwardsSprintSpeed { get; set; } = 8.75f;
        public float GoalieMaxForwardsSpeed { get; set; } = 5f;
        public float GoalieMaxForwardsSprintSpeed { get; set; } = 6f;
        public float GoalieMaxBackwardsSpeed { get; set; } = 5f;
        public float GoalieMaxBackwardsSprintSpeed { get; set; } = 6f;
        public float AngularForceMultiplier { get; set; } = 5.1f;
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
        public float PostSlideTurnTime { get; set; } = 0.35f;
        public float PostSlideTurnMax { get; set; } = 1.75f;
        public float PostSlideTurnAcceleration { get; set; } = 2.5f;
        public float PostSlideBrakeAcceleration { get; set; } = 8f;

        // fields for player body configuration

        public float SlideTurnMultiplier { get; set; } = 2f;
        public float StopDrag { get; set; } = 2.5f;
        public float BalanceRecoveryTime { get; set; } = 2f;
        public float GoalieDashSpeedLimit { get; set; } = 2.5f;
        public bool EnableGoalieMicrodash { get; set; } = false;
        public float MicrodashStamCostFraction { get; set; } = 0.75f;
        public bool EnableSmallerModels { get; set; } = false;
        public float TorsoColliderRadiusFactor { get; set; } = 1f;
        public float HeadColliderRadiusFactor { get; set; } = 1f;
        public float PlayerColliderBounciness { get; set; } = 0.05f;
        public float SlideDrag { get; set; } = 0.2f;
        public float CenterSpawnOffset { get; set; } = 1.8f;
        public float TackleSpeedThreshold { get; set; } = 7.6f;
        public float TackleForceThreshold { get; set; } = 7f;
        public float TackleForceMultiplier { get; set; } = 0.3f;
        public bool ThinSkaterBodies { get; set; } = true;
        public float SkaterThinningFactor { get; set; } = 0.5f;
        public float ButterflyPadOffset { get; set; } = 0f;

        // fields for puck configuration
        public float PuckMaxSpeed { get; set; } = 50f;
        public float PuckStickTensorX { get; set; } = 0.006f;
        public float PuckStickTensorY { get; set; } = 0.002f;
        public float PuckStickTensorZ { get; set; } = 0.006f;
        public float PuckScale { get; set; } = 0.92f;
        public float PuckDrag { get; set; } = 0.3f;
        public float PuckMass { get; set; } = 0.375f;
        public bool RandomPuckDrop { get; set; } = true;
        public bool EnablePuckThroughBodies { get; set; } = true;
        public bool EnablePuckThroughGroin { get; set; } = true;
        public bool PuckDragSpeedDependence { get; set; } = true;
        public float PuckNominalSpeed { get; set; } = 20f;
        public float PuckDragFactor { get; set; } = 0.0014f;
        public bool PuckHeightDependentDrag { get; set; } = false;
        public float PuckHeightLimit { get; set; } = 2f;
        public float PuckHeightDragFactor { get; set; } = 0f;

        // fields for stick configuration
        public bool DisableStickCollision { get; set; } = false;
        public bool DisableShaftCollision { get; set; } = false;
        public bool EnableMidStickCollider { get; set; } = false;
        public float StickMass { get; set; } = 1f;
        public bool AlterStickPositionerOutput { get; set; } = true;
        public float ShaftHandleProportionalGain { get; set; } = 500f;
        public float StickPositionerOutputMax { get; set; } = 1250f;
        public float GoaliePositionerOutputMax { get; set; } = 1200f;
        public float StickOnPuckInverseMass { get; set; } = 1f;
        public bool EnableStickSpeedDecay { get; set; } = true;
        public int StickSpeedDecaySpan { get; set; } = 75;
        public float StickSpeedDecayLimit { get; set; } = 22f;
        public float StickSpeedDecayRate { get; set; } = 6f;
        public float StickSpeedDecayMin { get; set; } = 500;

        // fields for stick positioner configuration
        public float SoftCollisionForce { get; set; } = 20f;
        public float BladeTargetFocusPointOffsetY { get; set; } = 0f;
        // public bool LimitGoalieReach { get; set; } = false;
        // public float GoalieMaximumReach { get; set; } = 2.5f;
        // public float GoalieReachDropoff { get; set; } = 0.8f;

        // fields for arena configuration
        public float postBounciness { get; set; } = 0f;
        public bool EnableSoftBoards { get; set; } = true; // Enable to make boards soft and bouncy
        public float BoardBounciness { get; set; } = 0.19f; // How bouncy the boards are (0.0 = no bounce, 1.0 = perfect bounce)
        public float BoardFriction { get; set; } = 0.1f; // How much friction the boards have
        public float BoardSoftness { get; set; } = 0.5f; // How soft the boards feel (affects collision response)

        // fields for physics configuration
        public float FixedDeltaTime { get; set; } = 0.01f;
        public int SolverIterations { get; set; } = 6;
        public bool UsePhysicsModificationEvents { get; set; } = true;

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
using UnityEngine;
using Unity.Netcode;
using System;
using System.Reflection;
using System.Collections;
using AYellowpaper.SerializedCollections;

namespace CompetitivePuckTweaks.src
{
    public class LegPadHelper : NetworkBehaviour
    {
        public PlayerLegPad legPadLeft;
        public PlayerLegPad legPadRight;
        public NetworkVector3 leftPosition = new NetworkVector3(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVector3 rightPosition = new NetworkVector3(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private FieldInfo localPositionField = typeof(PlayerLegPad).GetField("localPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        public Rigidbody playerRigidbody;
        public Vector3[] leftPositionList = new Vector3[5];
        public Vector3[] rightPositionList = new Vector3[5];
        private SerializedDictionary<PlayerLegPadState, Transform> positionsLeft = new SerializedDictionary<PlayerLegPadState, Transform>();
        private SerializedDictionary<PlayerLegPadState, Transform> positionsRight = new SerializedDictionary<PlayerLegPadState, Transform>();

        protected override void __initializeVariables()
        {
            base.__initializeVariables();
            if (leftPosition == null) throw new System.Exception($"{nameof(leftPosition)} cannot be null on {nameof(LegPadHelper)}");
            if (rightPosition == null) throw new System.Exception($"{nameof(rightPosition)} cannot be null on {nameof(LegPadHelper)}");
            leftPosition.Initialize(this);
            rightPosition.Initialize(this);
            NetworkVariableFields.Add(leftPosition);
            NetworkVariableFields.Add(rightPosition);
        }

        protected override void __initializeRpcs()
        {
            base.__initializeRpcs();
        }

        protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
        {
            playerRigidbody = GetComponentInParent<Rigidbody>();
            base.OnNetworkPreSpawn(ref networkManager);
        }

        public override void OnNetworkSpawn()
        {
            // velocity.OnValueChanged += this.OnVelocityChanged;
            base.OnNetworkSpawn();
        }

        protected override void OnNetworkPostSpawn()
        {
            // s_InitializeMethod.Invoke(velocity, new object[] { this });
            // velocity.Initialize(this);
            if (IsServer)
            {
                positionsLeft = (SerializedDictionary<PlayerLegPadState, Transform>)typeof(PlayerLegPad).GetField("positions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legPadLeft);
                positionsRight = (SerializedDictionary<PlayerLegPadState, Transform>)typeof(PlayerLegPad).GetField("positions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(legPadRight);
            }
            else if (!PluginCore.config.ExtraLegPadTweening)
            {
                enabled = false;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    leftPositionList[i] = new Vector3(0, 0, 0);
                    rightPositionList[i] = new Vector3(0, 0, 0);
                }
            }
            base.OnNetworkPostSpawn();
        }

        private void Awake()
        {

        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {
                Vector3 newPosition = new Vector3();
                newPosition = InterpList(ref leftPositionList, leftPosition.Value);
                localPositionField.SetValue(legPadLeft, newPosition);
                newPosition = InterpList(ref rightPositionList, rightPosition.Value);
                localPositionField.SetValue(legPadRight, newPosition);
                return;
            }
            leftPosition.Value = (Vector3)localPositionField.GetValue(legPadLeft);
            rightPosition.Value = (Vector3)localPositionField.GetValue(legPadRight);
        }

        private void OnVelocityChanged(Vector3 prev, Vector3 current)
        {
            // if needed should be implemented in custom network var first
        }

        public Vector3 GetCurrentLeftDiff(PlayerLegPadState refState)
        {
            PlayerBody body = legPadLeft.GetComponentInParent<PlayerBody>();
            Vector3 localVelocity = body.transform.InverseTransformVector(playerRigidbody.linearVelocity);
            if (localVelocity.x > PluginCore.config.LegPadSpreadMinVelocity)
            {
                //right dash
                float offset = Mathf.Clamp(-localVelocity.x * PluginCore.config.LegPadSpreadVelocityFactor, float.NegativeInfinity, 0);
                float defaultX = positionsLeft[refState].localPosition.x;
                if (Mathf.Abs(offset + defaultX) > PluginCore.config.LegPadExtensionLimit) offset = -PluginCore.config.LegPadExtensionLimit - defaultX;
                return new Vector3(offset, 0, 0);
            }
            return Vector3.zero;
        }

        public Vector3 GetCurrentRightDiff(PlayerLegPadState refState)
        {
            PlayerBody body = legPadRight.GetComponentInParent<PlayerBody>();
            Vector3 localVelocity = body.transform.InverseTransformVector(playerRigidbody.linearVelocity);
            if (localVelocity.x < -PluginCore.config.LegPadSpreadMinVelocity)
            {
                //left dash
                float offset = Mathf.Clamp(-localVelocity.x * PluginCore.config.LegPadSpreadVelocityFactor, 0, float.PositiveInfinity);
                float defaultX = positionsRight[refState].localPosition.x;
                if (Mathf.Abs(offset + defaultX) > PluginCore.config.LegPadExtensionLimit) offset = PluginCore.config.LegPadExtensionLimit - defaultX;
                return new Vector3(offset, 0, 0);
            }
            return Vector3.zero;
            
        }

        public IEnumerator PostDashLeftStateChange()
        {
            yield return new WaitForFixedUpdate();
            typeof(PlayerLegPad).GetMethod("OnStateChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(legPadRight, new object[] { legPadRight.State, legPadRight.State });
        }

        public IEnumerator PostDashRightStateChange()
        {
            yield return new WaitForFixedUpdate();
            typeof(PlayerLegPad).GetMethod("OnStateChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(legPadLeft, new object[] { legPadLeft.State, legPadLeft.State });
        }

        private Vector3 InterpList(ref Vector3[] list, Vector3 newItem)
        {
            Vector3 newVal = new Vector3();
            Vector3 a = list[0];
            Vector3 b = list[4];
            Vector3 diff = b - a;
            for (int i = 1; 0 < i && i < 4; i++)
            {
                list[i] = a + diff * ((float)i / 4f);
            }
            newVal = list[0];
            list[0] = list[1];
            list[1] = list[2];
            list[2] = list[3];
            list[3] = list[4];
            list[4] = newItem;
            return newVal;
        }

    }

    public class NetworkVector3 : NetworkVariableBase
    {
        private Vector3 m_Value;
        private bool m_IsDirty;

        public Vector3 Value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;
                m_Value = value;
                SetDirty(true);
            }
        }

        public NetworkVector3(Vector3 v, NetworkVariableReadPermission readPerms, NetworkVariableWritePermission writePerms) : base(readPerms, writePerms)
        {
            Value = v;
        }

        public override void WriteField(FastBufferWriter writer)
        {
            byte[] bytes = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(Value.x), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Value.y), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Value.z), 0, bytes, 8, 4);
            writer.WriteBytesSafe(bytes);
            // PluginCore.Log($"[SERVER] Write Field {m_Value}");
            // writer.WriteValue(m_Value);
        }

        public override void ReadField(FastBufferReader reader)
        {
            Vector3 newValue = new Vector3();
            byte[] bytes = new byte[4];
            reader.ReadBytesSafe(ref bytes, 4, 0);
            newValue.x = BitConverter.ToSingle(bytes);
            reader.ReadBytesSafe(ref bytes, 4, 4);
            newValue.y = BitConverter.ToSingle(bytes);
            reader.ReadBytesSafe(ref bytes, 4, 8);
            newValue.z = BitConverter.ToSingle(bytes);
            m_Value = newValue;
            // PluginCore.Log($"[CLIENT] ReadDelta fired");
            // reader.ReadValue(out m_Value);
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            // PluginCore.Log($"[SERVER] WriteDelta {m_Value}");
            writer.WriteValue(m_Value);
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            // PluginCore.Log($"[CLIENT] ReadDelta fired");
            reader.ReadValue(out m_Value);
            if (keepDirtyDelta) SetDirty(true);
        }

        public override bool IsDirty()
        {
            return base.IsDirty();
        }
        public override void ResetDirty()
        {
            base.ResetDirty();
        }
    }

    public class NetworkBool : NetworkVariableBase
    {
        private bool m_Value;
        private bool m_IsDirty;

        public bool Value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;
                m_Value = value;
                SetDirty(true);
            }
        }

        public NetworkBool(bool v, NetworkVariableReadPermission readPerms, NetworkVariableWritePermission writePerms) : base(readPerms, writePerms)
        {
            Value = v;
        }

        public override void WriteField(FastBufferWriter writer)
        {
            // byte[] bytes = new byte[12];
            // Buffer.BlockCopy(BitConverter.GetBytes(Value.x), 0, bytes, 0, 4);
            // Buffer.BlockCopy(BitConverter.GetBytes(Value.y), 0, bytes, 4, 4);
            // Buffer.BlockCopy(BitConverter.GetBytes(Value.z), 0, bytes, 8, 4);
            // writer.WriteBytesSafe(bytes);
            // PluginCore.Log($"[SERVER] Write Field {m_Value}");
            writer.WriteValueSafe(m_Value);
        }

        public override void ReadField(FastBufferReader reader)
        {
            // Vector3 newValue = new Vector3();
            // byte[] bytes = new byte[4];
            // reader.ReadBytesSafe(ref bytes, 4, 0);
            // newValue.x = BitConverter.ToSingle(bytes);
            // reader.ReadBytesSafe(ref bytes, 4, 4);
            // newValue.y = BitConverter.ToSingle(bytes);
            // reader.ReadBytesSafe(ref bytes, 4, 8);
            // newValue.z = BitConverter.ToSingle(bytes);
            // Value = newValue;
            // PluginCore.Log($"[CLIENT] ReadDelta fired");
            reader.ReadValueSafe(out m_Value);
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            // PluginCore.Log($"[SERVER] WriteDelta {m_Value}");
            writer.WriteValueSafe(m_Value);
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            // PluginCore.Log($"[CLIENT] ReadDelta fired");
            reader.ReadValueSafe(out m_Value);
            if (keepDirtyDelta) SetDirty(true);
        }

        public override bool IsDirty()
        {
            return base.IsDirty();
        }
        public override void ResetDirty()
        {
            base.ResetDirty();
        }
    }
}
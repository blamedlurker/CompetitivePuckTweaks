using Unity.Netcode;

namespace CompetitivePuckTweaks.src
{
    public struct ConfigSyncPackage : INetworkSerializable
    {
        public float PuckScale;
        public float LegPadOffset;

        public ConfigSyncPackage(ModConfig modConfig)
        {
            PuckScale = modConfig.PuckScale;
            LegPadOffset = modConfig.ButterflyPadOffset;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PuckScale);
            serializer.SerializeValue(ref LegPadOffset);
        }
    }
}
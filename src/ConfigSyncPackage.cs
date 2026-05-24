using Unity.Netcode;

namespace CompetitivePuckTweaks.src
{
    public struct ConfigSyncPackage : INetworkSerializable
    {
        public float PuckScale;
        public bool ExtraLegPadTweening;
        public float LegPadReturnTime;
        public float LegPadSpreadVelocityFactor;
        public float LegPadExtensionLimit;

        public ConfigSyncPackage(ModConfig modConfig)
        {
            PuckScale = modConfig.PuckScale;
            ExtraLegPadTweening = modConfig.ExtraLegPadTweening;
            LegPadReturnTime = modConfig.LegPadReturnTime;
            LegPadSpreadVelocityFactor = modConfig.LegPadSpreadVelocityFactor;
            LegPadExtensionLimit = modConfig.LegPadExtensionLimit;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PuckScale);
            serializer.SerializeValue(ref ExtraLegPadTweening);
            serializer.SerializeValue(ref LegPadReturnTime);
            serializer.SerializeValue(ref LegPadSpreadVelocityFactor);
            serializer.SerializeValue(ref LegPadExtensionLimit);
        }
    }
}
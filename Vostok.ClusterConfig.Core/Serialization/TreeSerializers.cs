namespace Vostok.ClusterConfig.Core.Serialization
{
    internal static class TreeSerializers
    {
        public static readonly ITreeSerializer V1 = new TreeSerializerV1();
    }
}

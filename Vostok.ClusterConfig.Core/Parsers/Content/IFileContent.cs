using System.IO;
using JetBrains.Annotations;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal interface IFileContent
    {
        [NotNull]
        string AsString { get; }

        [NotNull]
        byte[] AsBytes { get; }

        [NotNull]
        Stream AsStream { get; }
    }
}
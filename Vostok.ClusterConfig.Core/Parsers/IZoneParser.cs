using System.IO;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal interface IZoneParser
    {
        [CanBeNull]
        ObjectNode Parse([NotNull] DirectoryInfo directory);
    }
}

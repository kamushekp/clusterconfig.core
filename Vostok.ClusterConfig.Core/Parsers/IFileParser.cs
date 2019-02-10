using System.IO;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal interface IFileParser
    {
        [CanBeNull]
        ObjectNode Parse([NotNull] FileInfo file);
    }
}
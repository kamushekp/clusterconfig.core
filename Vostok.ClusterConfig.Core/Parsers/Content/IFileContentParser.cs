using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal interface IFileContentParser
    {
        [CanBeNull]
        ObjectNode Parse([NotNull] string name, [NotNull] IFileContent content);
    }
}
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal interface IFileContentParser
    {
        [CanBeNull]
        ISettingsNode Parse([NotNull] string name, [NotNull] IFileContent content);
    }
}
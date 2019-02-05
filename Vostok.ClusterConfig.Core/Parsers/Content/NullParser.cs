using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class NullParser : IFileContentParser
    {
        public ISettingsNode Parse(string name, IFileContent content) => null;
    }
}
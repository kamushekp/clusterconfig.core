using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class WholeFileParser : IFileContentParser
    {
        public ISettingsNode Parse(string name, IFileContent content) 
            => new ValueNode(name, content.AsString);
    }
}
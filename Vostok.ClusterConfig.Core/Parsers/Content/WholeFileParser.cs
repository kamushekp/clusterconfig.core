using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class WholeFileParser : IFileContentParser
    {
        public static readonly WholeFileParser Instance = new WholeFileParser();

        public ObjectNode Parse(string name, IFileContent content) 
            => new ObjectNode(name, new [] { new ValueNode(string.Empty, content.AsString) });
    }
}
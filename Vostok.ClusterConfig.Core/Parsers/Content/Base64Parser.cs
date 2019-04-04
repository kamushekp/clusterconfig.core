using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class Base64Parser : IFileContentParser
    {
        public static readonly Base64Parser Instance = new Base64Parser();

        public ObjectNode Parse(string name, IFileContent content)
            => new ObjectNode(name, new[] {new ValueNode(string.Empty, Convert.ToBase64String(content.AsBytes))});
    }
}
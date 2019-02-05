using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class Base64Parser : IFileContentParser
    {
        public ISettingsNode Parse(string name, IFileContent content)
            => new ValueNode(name, Convert.ToBase64String(content.AsBytes));
    }
}
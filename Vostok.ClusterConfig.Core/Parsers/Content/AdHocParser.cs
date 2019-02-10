using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class AdHocParser : IFileContentParser
    {
        private readonly Func<string, IFileContent, ObjectNode> parser;

        public AdHocParser(Func<string, IFileContent, ObjectNode> parser)
        {
            this.parser = parser;
        }

        public ObjectNode Parse(string name, IFileContent content) 
            => parser(name, content);
    }
}
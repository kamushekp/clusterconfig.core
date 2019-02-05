using System;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class AdHocParser : IFileContentParser
    {
        private readonly Func<string, IFileContent, ISettingsNode> parser;

        public AdHocParser(Func<string, IFileContent, ISettingsNode> parser)
        {
            this.parser = parser;
        }

        public ISettingsNode Parse(string name, IFileContent content) 
            => parser(name, content);
    }
}
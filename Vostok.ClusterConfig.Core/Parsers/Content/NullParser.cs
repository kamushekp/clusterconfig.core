using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class NullParser : IFileContentParser
    {
        public static readonly NullParser Instance = new NullParser();

        public ObjectNode Parse(string name, IFileContent content) => null;
    }
}
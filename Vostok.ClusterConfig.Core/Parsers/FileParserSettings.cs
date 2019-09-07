using System.Collections.Generic;
using Vostok.ClusterConfig.Core.Parsers.Content;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal class FileParserSettings
    {
        public IFileContentParser DefaultParser { get; set; } = new KeyValueParser();

        public Dictionary<string, IFileContentParser> CustomParsers { get; set; } = new Dictionary<string, IFileContentParser>
        {
            [".example"] = NullParser.Instance,
            [".xml"] = WholeFileParser.Instance,
            [".xslt"] = WholeFileParser.Instance,
            [".json"] = WholeFileParser.Instance,
            [".cs"] = WholeFileParser.Instance,
            [".cer"] = Base64Parser.Instance,
            [".yml"] = WholeFileParser.Instance,
            [".yaml"] = WholeFileParser.Instance,
            [".toml"] = WholeFileParser.Instance
        };

        public int MaximumFileSize { get; set; } = 1024 * 1024;
    }
}

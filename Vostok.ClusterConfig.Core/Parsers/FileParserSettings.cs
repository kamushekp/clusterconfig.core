using System.Collections.Generic;
using Vostok.ClusterConfig.Core.Parsers.Content;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal class FileParserSettings
    {
        public IFileContentParser DefaultParser { get; set; } = new KeyValueParser();

        public Dictionary<string, IFileContentParser> CustomParsers { get; set; } = new Dictionary<string, IFileContentParser>
        {
            [".example"] = new NullParser(),
            [".xml"] = new WholeFileParser(),
            [".xslt"] = new WholeFileParser(),
            [".json"] = new WholeFileParser(),
            [".cs"] = new WholeFileParser(),
            [".cer"] = new Base64Parser()
        };

        public int MaximumFileSize { get; set; } = 1024 * 1024;
    }
}

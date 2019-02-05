using System.IO;
using Vostok.ClusterConfig.Core.Parsers.Content;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal class FileParser : IFileParser
    {
        private readonly FileParserSettings settings;

        public FileParser(FileParserSettings settings)
        {
            this.settings = settings;
        }

        public ISettingsNode Parse(FileInfo file)
        {
            try
            {
                var fileSize = file.Length;
                if (fileSize > settings.MaximumFileSize)
                    return null;

                var parser = settings.CustomParsers.TryGetValue(file.Extension.ToLower(), out var customParser)
                    ? customParser
                    : settings.DefaultParser;

                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    return parser.Parse(file.Name.ToLower(), new FileContent(stream, (int)fileSize));
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}

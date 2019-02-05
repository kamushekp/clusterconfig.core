using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    internal class ZoneParser : IZoneParser
    {
        private readonly IFileParser fileParser;

        public ZoneParser(IFileParser fileParser)
        {
            this.fileParser = fileParser;
        }

        public ISettingsNode Parse(DirectoryInfo directory)
        {
            return new ObjectNode(null, ParseDirectory(directory));
        }

        private static IEnumerable<FileInfo> GetFilesSafe(DirectoryInfo directory)
        {
            try
            {
                return directory.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<FileInfo>();
            }
        }

        private static IEnumerable<DirectoryInfo> GetDirectoriesSafe(DirectoryInfo directory)
        {
            try
            {
                return directory.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<DirectoryInfo>();
            }
        }

        private static bool ShouldBeProcessed(FileInfo fileInfo)
        {
            if (fileInfo.Name.StartsWith("."))
                return false;

            if (fileInfo.Extension.ToLower() == ".example")
                return false;

            return true;
        }

        private static bool ShouldBeProcessed(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Name.StartsWith("."))
                return false;

            return true;
        }

        private IEnumerable<ISettingsNode> ParseDirectory(DirectoryInfo directory)
        {
            if (!directory.Exists)
                yield break;

            foreach (var file in GetFilesSafe(directory).Where(ShouldBeProcessed))
            {
                var fileTree = fileParser.Parse(file);
                if (fileTree != null)
                    yield return fileTree;
            }

            foreach (var subDirectory in GetDirectoriesSafe(directory).Where(ShouldBeProcessed))
            {
                var subDirectoryNode = new ObjectNode(subDirectory.Name.ToLower(), ParseDirectory(subDirectory));
                if (subDirectoryNode.ChildrenCount > 0)
                    yield return subDirectoryNode;
            }
        }
    }
}

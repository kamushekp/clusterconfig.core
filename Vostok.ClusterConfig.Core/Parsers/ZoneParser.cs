using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Parsers
{
    // (iloktionov): Zone mapping to ISettingsNodes.
    // 1. The zone itself corresponds to a root ObjectNode with null name.
    // 2. Every subdirectory one the way to a file is an ObjectNode with corresponding name.
    // 3. Every file is an ObjectNode with corresponding name. It can have two type of children:
    //  3.1. Named: these are ValueNodes or ArrayNodes with names parsed from the file itself.
    //  3.2. Unnamed: this can be either a ValueNode or ArrayNode stored under an empty key to represent file content.

    internal class ZoneParser : IZoneParser
    {
        private readonly IFileParser fileParser;

        public ZoneParser(IFileParser fileParser)
        {
            this.fileParser = fileParser;
        }

        public ObjectNode Parse(DirectoryInfo directory)
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

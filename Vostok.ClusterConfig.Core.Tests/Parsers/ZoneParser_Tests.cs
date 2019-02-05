using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Core.Parsers;
using Vostok.ClusterConfig.Core.Tests.Helpers;
using Vostok.Configuration.Abstractions.SettingsTree;

// ReSharper disable PossibleNullReferenceException

namespace Vostok.ClusterConfig.Core.Tests.Parsers
{
    [TestFixture]
    internal class ZoneParser_Tests
    {
        private TemporaryDirectory directory;
        private IFileParser fileParser;
        private Func<FileInfo, ISettingsNode> fileParserImpl;
        private ZoneParser zoneParser;

        [SetUp]
        public void TestSetup()
        {
            directory = new TemporaryDirectory();

            fileParserImpl = info => new ObjectNode(info.Name);
            fileParser = Substitute.For<IFileParser>();
            fileParser.Parse(Arg.Any<FileInfo>()).Returns(info => fileParserImpl(info.Arg<FileInfo>()));

            zoneParser = new ZoneParser(fileParser);
        }

        [Test]
        public void Should_return_an_empty_node_with_null_name_for_empty_zone()
        {
            var tree = zoneParser.Parse(directory.Info);

            tree.Should().BeOfType<ObjectNode>();
            tree.Name.Should().BeNull();
            tree.Children.Should().BeEmpty();
        }

        [Test]
        public void Should_add_a_nested_node_for_each_file()
        {
            File.Create(Path.Combine(directory.Path, "foo.txt"));
            File.Create(Path.Combine(directory.Path, "foo-bar.txt"));

            var tree = zoneParser.Parse(directory.Info);

            var children = tree.Children.ToArray();

            children.Should().HaveCount(2);

            children.Select(c => c.Name).Should().BeEquivalentTo("foo.txt", "foo-bar.txt");
        }

        [Test]
        public void Should_skip_example_files()
        {
            File.Create(Path.Combine(directory.Path, "foo.example"));

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().BeEmpty();
        }

        [Test]
        public void Should_skip_files_with_names_starting_with_a_dot()
        {
            File.Create(Path.Combine(directory.Path, ".ssh-config"));

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().BeEmpty();
        }

        [Test]
        public void Should_skip_files_not_parsed_by_file_parser()
        {
            File.Create(Path.Combine(directory.Path, "file1"));
            File.Create(Path.Combine(directory.Path, "file2"));

            fileParserImpl = _ => null;

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().BeEmpty();
        }

        [Test]
        public void Should_add_a_nested_node_for_each_subdirectory_with_lowercased_name()
        {
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir1"));
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir2"));

            File.Create(Path.Combine(directory.Path, "SubDir1", "foo.txt"));
            File.Create(Path.Combine(directory.Path, "SubDir2", "foo.txt"));

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().HaveCount(2);

            tree.Children.Select(c => c.Name).Should().BeEquivalentTo("subdir1", "subdir2");

            tree["SubDir1"]["foo.txt"].Should().NotBeNull();
            tree["SubDir2"]["foo.txt"].Should().NotBeNull();
        }

        [Test]
        public void Should_skip_subdirectories_without_any_parsed_files()
        {
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir1"));
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir1", "SubDir2"));
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir3"));
            Directory.CreateDirectory(Path.Combine(directory.Path, "SubDir3", "SubDir4"));

            File.Create(Path.Combine(directory.Path, "SubDir1", "SubDir2", "foo.txt"));
            File.Create(Path.Combine(directory.Path, "SubDir3", "SubDir4", "foo.example"));

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().HaveCount(1);

            tree["SubDir1"]["SubDir2"]["foo.txt"].Should().NotBeNull();
        }

        [Test]
        public void Should_skip_subdirectories_with_names_starting_with_a_dot()
        {
            Directory.CreateDirectory(Path.Combine(directory.Path, ".git"));

            File.Create(Path.Combine(directory.Path, ".git", "index"));

            var tree = zoneParser.Parse(directory.Info);

            tree.Children.Should().BeEmpty();
        }
    }
}
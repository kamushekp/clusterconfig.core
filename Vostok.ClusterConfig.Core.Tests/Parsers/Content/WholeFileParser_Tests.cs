using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Core.Parsers.Content;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Tests.Parsers.Content
{
    [TestFixture]
    internal class WholeFileParser_Tests
    {
        [Test]
        public void Should_return_whole_file_content_as_an_object_node_with_unnamed_value_node()
        {
            var parser = new WholeFileParser();

            var text = "Hello!" + Environment.NewLine + "Goodbye!";

            var content = Substitute.For<IFileContent>();

            content.AsString.Returns(text);

            var node = parser.Parse("foo", content);

            node.Should().BeOfType<ObjectNode>();
            node.Name.Should().Be("foo");
            node.Children.Should().HaveCount(1);

            node[string.Empty].Should().BeOfType<ValueNode>().Which.Value.Should().Be(text);
        }
    }
}
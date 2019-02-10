using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Core.Parsers.Content;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Tests.Parsers.Content
{
    [TestFixture]
    internal class KeyValueParser_Tests
    {
        private KeyValueParser parser;
        private IFileContent content;
        private StringBuilder contentBuilder;
        private string fileName;

        [SetUp]
        public void TestSetup()
        {
            parser = new KeyValueParser();

            contentBuilder = new StringBuilder();

            content = Substitute.For<IFileContent>();
            content.AsStream.Returns(_ => new MemoryStream(Encoding.UTF8.GetBytes(contentBuilder.ToString())));

            fileName = Guid.NewGuid().ToString();
        }

        [Test]
        public void Should_return_an_object_node_with_empty_value_for_empty_text()
        {
            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);
            node.Children.Should().ContainSingle().Which.Should().Be(new ValueNode(string.Empty, string.Empty));
        }

        [Test]
        public void Should_return_an_object_node_with_empty_value_for_text_consisting_of_blank_lines()
        {
            contentBuilder.AppendLine("  ");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("\t");

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);
            node.Children.Should().ContainSingle().Which.Should().Be(new ValueNode(string.Empty, string.Empty));
        }

        [Test]
        public void Should_return_an_object_node_with_empty_value_for_text_consisting_of_comments()
        {
            contentBuilder.AppendLine("# comment 1");
            contentBuilder.AppendLine("  # comment 2");
            contentBuilder.AppendLine("\t# comment 3");

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);
            node.Children.Should().ContainSingle().Which.Should().Be(new ValueNode(string.Empty, string.Empty));
        }

        [Test]
        public void Should_return_an_object_node_with_unnamed_value_node_if_text_consists_of_a_single_unkeyed_value()
        {
            contentBuilder.AppendLine("some-value");

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);

            var value = node.Children.Should().ContainSingle()
                .Which.Should().BeOfType<ValueNode>()
                .Which;

            value.Name.Should().Be(string.Empty);
            value.Value.Should().Be("some-value");
        }

        [Test]
        public void Should_return_an_object_node_with_unnamed_array_node_with_correct_order_if_text_consists_of_multiple_unkeyed_values()
        {
            var values = Enumerable.Range(0, 1000).Select(_ => Guid.NewGuid().ToString()).ToArray();

            foreach (var value in values)
                contentBuilder.AppendLine(value);

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);

            var array = node.Children.Should().ContainSingle()
                .Which.Should().BeOfType<ArrayNode>()
                .Which;

            array.Name.Should().Be(string.Empty);
            array.Children.Should().OnlyContain(child => child is ValueNode);
            array.Children.Select(child => child.Value).Should().Equal(values);
        }

        [Test]
        public void Should_return_an_object_node_if_text_has_at_least_one_keyed_line()
        {
            contentBuilder.AppendLine("a = b");

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);
            node.Children.Should().ContainSingle().Which.Should().Be(new ValueNode("a", "b"));
        }

        [Test]
        public void Should_trim_leading_and_trailing_whitespaces_from_lines()
        {
            contentBuilder.AppendLine(" a = b  ");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("a", "b"));
        }

        [Test]
        public void Should_convert_keys_to_lowercase()
        {
            contentBuilder.AppendLine("KEY = value");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("key", "value"));
        }

        [Test]
        public void Should_not_mess_with_values_case()
        {
            contentBuilder.AppendLine("KEY = VALUE");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("key", "VALUE"));
        }

        [Test]
        public void Should_support_no_spacing_between_key_value_and_separator()
        {
            contentBuilder.AppendLine("key=value");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("key", "value"));
        }

        [Test]
        public void Should_support_empty_keys()
        {
            contentBuilder.AppendLine("key1 = value1");
            contentBuilder.AppendLine("= value2");

            Parse().Children.Should().Contain(new ValueNode("", "value2"));
        }

        [Test]
        public void Should_support_empty_values()
        {
            contentBuilder.AppendLine("key=");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("key", ""));
        }

        [TestCase("=")]
        [TestCase(":=")]
        public void Should_support_multiple_separators(string separator)
        {
            contentBuilder.AppendLine($"key {separator} value");

            Parse().Children.Should().ContainSingle().Which.Should().Be(new ValueNode("key", "value"));
        }

        [Test]
        public void Should_fold_keys_with_multiple_values_into_array_nodes()
        {
            contentBuilder.AppendLine("key = value1");
            contentBuilder.AppendLine("key = value2");
            contentBuilder.AppendLine("key = value3");

            Parse()["key"].Should().BeOfType<ArrayNode>().Which.Children.Should().Equal(
                new ValueNode("0", "value1"),
                new ValueNode("1", "value2"),
                new ValueNode("2", "value3"));
        }

        [Test]
        public void Should_correctly_parse_a_complex_text_with_a_mix_of_all_cases()
        {
            contentBuilder.AppendLine();
            contentBuilder.AppendLine("# key1 = Value1");
            contentBuilder.AppendLine("key2 := Value2");
            contentBuilder.AppendLine("Plain1");
            contentBuilder.AppendLine("key3 = Value3");
            contentBuilder.AppendLine("Plain3");
            contentBuilder.AppendLine("   ");
            contentBuilder.AppendLine("key3 := Value4");
            contentBuilder.AppendLine("key4 = Value5");
            contentBuilder.AppendLine("key5 =    ");
            contentBuilder.AppendLine(":=plain2");
            contentBuilder.AppendLine();

            var node = Parse().Should().BeOfType<ObjectNode>().Which;

            node.Name.Should().Be(fileName);

            node["key1"].Should().BeNull();

            node["key2"].Should().Be(new ValueNode("key2", "Value2"));

            node["key3"].Should().Be(new ArrayNode("key3", new []
            {
                new ValueNode("0", "Value3"),
                new ValueNode("1", "Value4")
            }));

            node["key4"].Should().Be(new ValueNode("key4", "Value5"));

            node["key5"].Should().Be(new ValueNode("key5", ""));

            node[""].Should().Be(new ArrayNode("", new []
            {
                new ValueNode("0", "Plain1"),
                new ValueNode("1", "Plain3"),
                new ValueNode("2", "plain2"),
            }));
        }

        private ISettingsNode Parse()
            => parser.Parse(fileName, content);
    }
}
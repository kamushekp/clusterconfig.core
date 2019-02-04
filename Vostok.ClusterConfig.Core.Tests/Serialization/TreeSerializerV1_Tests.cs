using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterConfig.Core.Serialization;
using Vostok.Commons.Binary;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.ClusterConfig.Core.Tests.Serialization
{
    [TestFixture]
    internal class TreeSerializerV1_Tests
    {
        private TreeSerializerV1 serializer;
        private ISettingsNode tree;

        [SetUp]
        public void TestSetup()
        {
            serializer = new TreeSerializerV1();
        }

        [Test]
        public void Test()
        {
            AddToTree("foo/bar/baz/key1", "value1");
            AddToTree("foo/bar/baz/key2", "value2");

            AddToTree("foo/ban/baz/key1", "value1");
            AddToTree("foo/ban/baz/key2", "value2");

            Serialize(out var reader);

            var node = serializer.Deserialize(reader, new[] {"foo", "ban"});

            node.Should().Be(tree.ScopeTo("foo", "ban"));
        }

        private void Serialize(out IBinaryReader reader)
        {
            var writer = new BinaryBufferWriter(64);

            writer.Write(Guid.NewGuid()); // garbage

            serializer.Serialize(tree, writer);

            reader = new BinaryBufferReader(writer.Buffer, 16);
        }

        private void AddToTree(string path, string value)
            => AddLeafNode(path, name => new ValueNode(name, value));

        private void AddToTree(string path, string[] values)
            => AddLeafNode(path, name => new ArrayNode(name, values.Select(v => new ValueNode(null, v)).ToArray()));

        private void AddLeafNode(string path, Func<string, ISettingsNode> nodeFactory)
        {
            var segments = path.Split('/');
            if (segments.Length == 0)
                return;

            Array.Reverse(segments);

            var node = nodeFactory(segments.First());

            foreach (var segment in segments.Skip(1))
            {
                node = new ObjectNode(segment, new[] { node });
            }

            node = new ObjectNode(null, new[] { node });

            AddToTree(node);
        }

        private void AddToTree(ISettingsNode node)
            => tree = SettingsNodeMerger.Merge(tree, node, SettingsMergeOptions.Default);
    }
}
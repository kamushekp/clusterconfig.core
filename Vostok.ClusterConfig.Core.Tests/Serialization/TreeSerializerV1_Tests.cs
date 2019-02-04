using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.ClusterConfig.Core.Serialization;
using Vostok.ClusterConfig.Core.Tests.Helpers;
using Vostok.Commons.Binary;
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

            tree = new TreeBuilder()
                .Add("plain-value", "value1")
                .Add("plain-array", new [] {"1", "2"})

                .Add("foo/bar/baz", "3")
                .Add("foo/bar/baz2", "4")
                .Add("foo/bar/baz3", "5")

                .Add("foo/bar", new [] {"bar1", "bar2"})
                .Add("foo", Guid.NewGuid().ToString())

                .Add("one/two", "three")
                .Add("one/three", "five")
                .Add("one/three/four", string.Empty)

                .Build();
        }

        [Test]
        public void Should_correctly_serialize_and_deserialize_a_complex_tree()
        {
            TestSerialization();
        }

        [Test]
        public void Should_correctly_serialize_and_deserialize_a_tree_consisting_from_empty_object_nodes()
        {
            tree = new ObjectNode(null, new [] {new ObjectNode("foo") as ISettingsNode });

            TestSerialization();
        }

        [TestCase("plain-value")]
        [TestCase("plain-array")]
        [TestCase("foo")]
        [TestCase("foo/bar")]
        [TestCase("foo/bar/baz")]
        [TestCase("foo/bar/baz2")]
        [TestCase("foo/bar/baz3")]
        [TestCase("one")]
        [TestCase("one/two")]
        [TestCase("one/three")]
        [TestCase("one/three/four")]
        public void Navigation_should_deserialize_a_subtree_located_under_existing_path(string path)
        {
            TestNavigation(path);
        }

        [TestCase("PLAIN-VALUE")]
        [TestCase("Plain-Array")]
        [TestCase("Foo")]
        [TestCase("FOO/bar")]
        [TestCase("foo/Bar/BAZ")]
        public void Navigation_should_be_case_insensitive(string path)
        {
            TestNavigation(path);
        }

        [TestCase("non/existing/bullshit")]
        [TestCase("123/plain-value")]
        [TestCase("123/plain-array")]
        [TestCase("foo/bar/baz/whatever")]
        [TestCase("foo/bar/baz4")]
        [TestCase("")]
        public void Navigation_should_fail_to_find_a_subtree_located_under_non_existing_path(string path)
        {
            TestNavigation(path);
        }

        [TestCase("plain-value/value1")]
        [TestCase("foo/bar/baz/3")]
        public void Navigation_should_fail_to_find_a_subtree_located_under_a_path_ending_in_a_value_node(string path)
        {
            TestNavigation(path);
        }

        [TestCase("plain-array/value1")]
        [TestCase("foo/bar/array")]
        public void Navigation_should_fail_to_find_a_subtree_located_under_a_path_ending_in_an_array_node(string path)
        {
            TestNavigation(path);
        }

        [Test]
        public void Should_fail_on_a_null_argument()
        {
            tree = null;

            TestFailure<ArgumentNullException>();
        }

        [Test]
        public void Should_fail_on_nested_arrays()
        {
            tree = new ArrayNode("a1", new [] {new ArrayNode("a2", new List<ISettingsNode>()) });

            TestFailure<InvalidOperationException>();
        }

        [Test]
        public void Should_fail_on_arrays_with_nested_objects()
        {
            tree = new ArrayNode("a1", new[] { new ObjectNode("obj") });

            TestFailure<InvalidOperationException>();
        }

        [Test]
        public void Should_fail_on_unknown_node_types()
        {
            tree = Substitute.For<ISettingsNode>();

            TestFailure<InvalidOperationException>();
        }

        private void TestSerialization()
        {
            var writer = new BinaryBufferWriter(64);

            writer.Write(Guid.NewGuid());

            serializer.Serialize(tree, writer);

            var reader = new BinaryBufferReader(writer.Buffer, 16);

            var deserializedTree = serializer.Deserialize(reader);

            deserializedTree.Should().Be(tree);
        }

        private void TestNavigation(ClusterConfigPath path)
        {
            var writer = new BinaryBufferWriter(64);

            writer.Write(Guid.NewGuid());

            serializer.Serialize(tree, writer);

            var reader = new BinaryBufferReader(writer.Buffer, 16);

            var deserializedTree = serializer.Deserialize(reader, path.Segments);

            deserializedTree.Should().Be(tree.ScopeTo(path.Segments));
        }

        private void TestFailure<TException>()
            where TException : Exception
        {
            try
            {
                TestSerialization();
            }
            catch (Exception error)
            {
                error.Should().BeOfType<TException>();

                Console.Out.WriteLine(error);

                return;
            }

            Assert.Fail($"Expected an error of type '{typeof(TException).Name}'.");
        }
    }
}
using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Configuration.Abstractions.SettingsTree;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.ClusterConfig.Core.Serialization
{
    // ValueNode format:
    //  + node type (byte)
    //  + value length (int)
    //  + value (UTF-8)

    // ArrayNode format:
    //  + node type (byte)
    //  + values count (int)
    //  + ValueNode[]

    // ObjectNode format:
    //  + node type (byte)
    //  + ChildIndex (detailed below)
    //  + Node[]

    // ChildIndex format:
    //  + Index length (int) — does not include itself
    //  + ChildCoordinate[]

    // ChildCoordinate format:
    //  + Path segment (string in UTF-8 with length)
    //  + Offset of child content (int, counted from the end of the index)

    internal class TreeSerializerV1 : ITreeSerializer
    {
        private const byte ObjectNodeType = 1;
        private const byte ArrayNodeType = 2;
        private const byte ValueNodeType = 3;

        public void Serialize(ISettingsNode tree, IBinaryWriter writer)
            => SerializeAny(tree ?? throw new ArgumentNullException(nameof(tree)), writer);

        public ISettingsNode Deserialize(IBinaryReader reader)
            => DeserializeAny(null, reader);

        public ISettingsNode Deserialize(IBinaryReader reader, IEnumerable<string> path)
        {
            return TryNavigate(reader, path, out var last) ? DeserializeAny(last, reader) : null;
        }

        #region Arbitrary nodes

        private static void SerializeAny(ISettingsNode tree, IBinaryWriter writer)
        {
            switch (tree)
            {
                case ObjectNode objectNode:
                    Serialize(objectNode, writer);
                    return;

                case ArrayNode arrayNode:
                    Serialize(arrayNode, writer);
                    return;

                case ValueNode valueNode:
                    Serialize(valueNode, writer);
                    return;
            }

            throw new InvalidOperationException($"Serialized tree contains a node of unknown type '{tree.GetType().Name}'.");
        }

        private static ISettingsNode DeserializeAny(string name, IBinaryReader reader)
        {
            var nodeType = reader.ReadByte();

            switch (nodeType)
            {
                case ObjectNodeType:
                    return DeserializeObjectNode(name, reader);

                case ArrayNodeType:
                    return DeserializeArrayNode(name, reader);

                case ValueNodeType:
                    return DeserializeValueNode(name, reader);
            }

            throw new InvalidOperationException($"Node type value '{nodeType}' does not correspond to any known nodes.");
        }

        #endregion

        #region ObjectNode

        private static void Serialize(ObjectNode node, IBinaryWriter writer)
        {
            writer.Write(ObjectNodeType);

            var indexLengthPosition = writer.Position;

            writer.Write(int.MinValue);

            var indexStartingPosition = writer.Position;

            writer.Write(node.ChildrenCount);

            var indexOffsetPositions = new Dictionary<string, long>(node.ChildrenCount);

            foreach (var child in node.Children)
            {
                if (child.Name == null)
                    throw new InvalidOperationException("Serialized tree contains an object node with unnamed child node.");

                writer.WriteWithLength(child.Name);

                indexOffsetPositions[child.Name] = writer.Position;

                writer.Write(int.MinValue);
            }

            var indexLength = (int)(writer.Position - indexStartingPosition);

            using (writer.JumpTo(indexLengthPosition))
                writer.Write(indexLength);

            var childrenStartingPosition = writer.Position;

            foreach (var child in node.Children)
            {
                var childOffset = (int)(writer.Position - childrenStartingPosition);

                using (writer.JumpTo(indexOffsetPositions[child.Name]))
                    writer.Write(childOffset);

                SerializeAny(child, writer);
            }
        }

        private static ObjectNode DeserializeObjectNode(string name, IBinaryReader reader)
        {
            var indexLength = reader.ReadInt32();
            var childrenStartingPosition = reader.Position + indexLength;
            var childrenCount = reader.ReadInt32();
            var children = new List<ISettingsNode>(childrenCount);

            for (var i = 0; i < childrenCount; i++)
            {
                var childName = reader.ReadString();
                var childOffset = reader.ReadInt32();

                using (reader.JumpTo(childrenStartingPosition + childOffset))
                {
                    children.Add(DeserializeAny(childName, reader));
                }
            }

            return new ObjectNode(name, children);
        }

        private static bool TryNavigate(IBinaryReader reader, IEnumerable<string> path, out string last)
        {
            last = null;

            foreach (var segment in path)
            {
                var nodeType = reader.ReadByte();
                if (nodeType != ObjectNodeType)
                    return false;

                var indexLength = reader.ReadInt32();
                var childrenStartingPosition = reader.Position + indexLength;
                var childrenCount = reader.ReadInt32();
                var childPosition = null as long?;

                for (var i = 0; i < childrenCount; i++)
                {
                    var childName = reader.ReadString();
                    var childOffset = reader.ReadInt32();

                    if (!StringComparer.OrdinalIgnoreCase.Equals(childName, segment))
                        continue;

                    childPosition = childrenStartingPosition + childOffset;
                    break;
                }

                if (childPosition == null)
                    return false;

                last = segment;

                reader.Position = childPosition.Value;
            }

            return true;
        }

        #endregion

        #region ArrayNode

        private static void Serialize(ArrayNode node, IBinaryWriter writer)
        {
            writer.Write(ArrayNodeType);
            writer.Write(node.ChildrenCount);

            foreach (var child in node.Children)
            {
                if (child is ValueNode valueNode)
                {
                    Serialize(valueNode, writer);
                }
                else throw new InvalidOperationException("Serialized tree contains an ArrayNode with children not being ValueNodes.");
            }
        }

        private static ArrayNode DeserializeArrayNode(string name, IBinaryReader reader)
        {
            var childrenCount = reader.ReadInt32();
            var children = new List<ISettingsNode>(childrenCount);

            for (var i = 0; i < childrenCount; i++)
            {
                if (reader.ReadByte() != ValueNodeType)
                    throw new InvalidOperationException("Serialized tree contains an ArrayNode with children not being ValueNodes.");

                children.Add(DeserializeValueNode(i.ToString(), reader));
            }

            return new ArrayNode(name, children);
        }

        #endregion

        #region ValueNode

        private static void Serialize(ValueNode node, IBinaryWriter writer)
        {
            writer.Write(ValueNodeType);
            writer.WriteWithLength(node.Value ?? string.Empty);
        }

        private static ValueNode DeserializeValueNode(string name, IBinaryReader reader)
            => new ValueNode(name, reader.ReadString());

        #endregion
    }
}
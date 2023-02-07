using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    /// <summary>
    /// The BufferedNodeDeserializer acts as a psuedo <see cref="INodeDeserializer" />.
    /// If any of it's <see cref="ITypeDiscriminator" /> has a matching BaseType, the BufferedNodeDeserializer will
    /// begin buffering the yaml stream. It will then use the matching <see cref="ITypeDiscriminator" />s to determine
    /// a dotnet output type for the yaml node. As the node is buffered, the <see cref="ITypeDiscriminator" />s are
    /// able to examine the actual values within, and use these when discriminating a type.
    /// Once a matching type is found, the BufferedNodeDeserializer uses it's inner deserializers to perform
    /// the final deserialization for that type & object.
    /// Usually you will want all default <see cref="INodeDeserializer" />s that exist in the outer
    /// <see cref="Deserializer" /> to also be used as inner deserializers.
    /// </summary>
    public class BufferedNodeDeserializer : INodeDeserializer
    {
        private readonly IList<INodeDeserializer> innerDeserializers;
        private readonly IList<ITypeDiscriminator> typeDiscriminators;
        private readonly int maxDepthToBuffer;
        private readonly int maxLengthToBuffer;

        public BufferedNodeDeserializer(IList<INodeDeserializer> innerDeserializers, IList<ITypeDiscriminator> typeDiscriminators, int maxDepthToBuffer, int maxLengthToBuffer)
        {
            this.innerDeserializers = innerDeserializers;
            this.typeDiscriminators = typeDiscriminators;
            this.maxDepthToBuffer = maxDepthToBuffer;
            this.maxLengthToBuffer = maxLengthToBuffer;
        }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (!reader.Accept<MappingStart>(out var mapping))
            {
                value = null;
                return false;
            }

            // Can any of the registered discriminators deal with the expected type?
            var possibleDiscriminators = typeDiscriminators.Where(t => t.BaseType.IsAssignableFrom(expectedType));
            if (!possibleDiscriminators.Any())
            {
                value = null;
                return false;
            }

            // Now buffer all the nodes in this mapping
            var start = reader.Current!.Start;
            Type actualType = expectedType;
            ParserBuffer buffer;
            try
            {
                buffer = new ParserBuffer(reader, maxDepth: maxDepthToBuffer, maxLength: maxLengthToBuffer);
            }
            catch (Exception exception)
            {
                throw new YamlException(start, reader.Current.End, "Failed to buffer yaml node", exception);
            }

            try
            {
                // use the discriminator to tell us what type it is really expecting by letting it inspect the parsing events
                foreach (var discriminator in possibleDiscriminators)
                {
                    buffer.Reset();
                    if (discriminator.TryDiscriminate(buffer, out var descriminatedType))
                    {
                        actualType = descriminatedType!;
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new YamlException(start, reader.Current.End, "Failed to discriminate type", exception);
            }

            // now continue by re-emitting parsing events and using the inner deserializers to handle
            buffer.Reset();
            foreach (var deserializer in innerDeserializers)
            {
                if (deserializer.Deserialize(buffer, actualType, nestedObjectDeserializer, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }
    }   
}
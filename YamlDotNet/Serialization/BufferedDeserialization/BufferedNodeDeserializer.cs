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
            var possibleDiscriminators = typeDiscriminators.Where(t => expectedType.IsAssignableTo(t.BaseType));
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
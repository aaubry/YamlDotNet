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
        private readonly INodeDeserializer originalDeserializer;
        private readonly int maxDepthToBuffer;
        private readonly int maxLengthToBuffer;
        private readonly List<ITypeDiscriminator> typeDiscriminators;

        public BufferedNodeDeserializer(INodeDeserializer originalDeserializer, int maxDepthToBuffer, int maxLengthToBuffer, List<ITypeDiscriminator> typeDiscriminators)
        {
            if (!(originalDeserializer is ObjectNodeDeserializer))
            {
                throw new ArgumentException($"{nameof(BufferedNodeDeserializer)} requires the original resolver to be a {nameof(ObjectNodeDeserializer)}");
            }

            this.typeDiscriminators = typeDiscriminators;
            this.originalDeserializer = originalDeserializer;
            this.maxDepthToBuffer = maxDepthToBuffer;
            this.maxLengthToBuffer = maxLengthToBuffer;
        }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            // we're essentially "in front of" the normal ObjectNodeDeserializer.
            // We could let it check if the current event is a mapping, but we also need to know.
            if (!reader.Accept<MappingStart>(out var mapping))
            {
                value = null;
                return false;
            }

            // Can any of the registered discriminators deal with the expected type?
            // We check this first so we can avoid buffering the parser unless we know we need to
            var possibleDiscriminators = typeDiscriminators.Where(t => expectedType.IsAssignableTo(t.BaseType));
            if (!possibleDiscriminators.Any())
            {
                return originalDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }

            // Now buffer all the nodes in this mapping
            var start = reader.Current!.Start;
            Type actualType = expectedType;
            ParserBuffer buffer;
            try
            {
                buffer = new ParserBuffer(reader, maxDepth: maxDepthToBuffer, maxLength: maxLengthToBuffer);

                // use the discriminator to tell us what type it is really expecting by letting it inspect the parsing events
                foreach (var discriminator in possibleDiscriminators)
                {
                    buffer.Reset();
                    if (discriminator.TryDiscriminate(buffer, out var descriminatedType))
                    {
                        actualType = descriminatedType!;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new YamlException(start, reader.Current.End, "Failed when resolving abstract type", exception);
            }

            // now continue by re-emitting parsing events
            buffer.Reset();
            return originalDeserializer.Deserialize(buffer, actualType, nestedObjectDeserializer, out value);
        }
    }   
}
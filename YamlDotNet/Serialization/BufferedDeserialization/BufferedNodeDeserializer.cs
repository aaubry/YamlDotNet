using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class BufferedNodeDeserializer : INodeDeserializer
    {
        private readonly INodeDeserializer originalDeserializer;
        private readonly int maxDepthToBuffer;
        private readonly int maxLengthToBuffer;
        private readonly List<IValueTypeDiscriminator> typeDiscriminators;

        public BufferedNodeDeserializer(INodeDeserializer originalDeserializer, int maxDepthToBuffer, int maxLengthToBuffer, List<IValueTypeDiscriminator> typeDiscriminators)
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

            // can any of the registered discrimaintors deal with the abstract type?
            var possibleDiscriminators = typeDiscriminators.Where(t => t.BaseType == expectedType);
            if (!possibleDiscriminators.Any())
            {
                // no? then not a node/type we want to deal with
                return originalDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
            }

            // now buffer all the nodes in this mapping
            var start = reader.Current!.Start;
            Type? actualType = null;
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

                if (actualType is null)
                {
                    throw new Exception($"None of the registered type discriminators could map to {expectedType}");
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
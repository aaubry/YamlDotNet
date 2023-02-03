using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class BufferedNodeDeserializerOptions : IBufferedNodeDeserializerOptions
    {
        internal readonly List<ITypeDiscriminator> discriminators = new List<ITypeDiscriminator>();

        public void AddValueTypeDiscriminator(ITypeDiscriminator discriminator)
        {
            this.discriminators.Add(discriminator);
        }

        public void AddKeyValueDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping)
        {
            this.discriminators.Add(new KeyValueTypeDiscriminator(typeof(T), discriminatorKey, valueTypeMapping));
        }

        public void AddUniqueKeyDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping)
        {
            this.discriminators.Add(new UniqueKeyTypeDiscriminator(typeof(T), uniqueKeyTypeMapping));
        }
    }
}
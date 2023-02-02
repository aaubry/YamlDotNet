using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class BufferedNodeDeserializerOptions : IBufferedNodeDeserializerOptions
    {
        internal readonly List<IValueTypeDiscriminator> discriminators = new List<IValueTypeDiscriminator>();

        public void AddKeyValueDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping)
        {
            this.discriminators.Add(new KeyValueTypeDiscriminator(typeof(T), discriminatorKey, valueTypeMapping));
        }
    }
}
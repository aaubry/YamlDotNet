using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public interface IBufferedNodeDeserializerOptions
    {
        public void AddValueTypeDiscriminator(ITypeDiscriminator discriminator);
        public void AddKeyValueDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping);
        public void AddKeyValueDiscriminator<T>(string discriminatorKey, params (string, Type)[] valueTypeMapping) => AddKeyValueDiscriminator<T>(discriminatorKey, valueTypeMapping.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2));
        public void AddUniqueKeyDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping);
        public void AddUniqueKeyDiscriminator<T>(params (string, Type)[] uniqueKeyTypeMapping) => AddUniqueKeyDiscriminator<T>(uniqueKeyTypeMapping.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2));
    }
}
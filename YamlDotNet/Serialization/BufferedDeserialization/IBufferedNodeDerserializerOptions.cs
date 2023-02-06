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
        public void AddTypeDiscriminator(ITypeDiscriminator discriminator);
        public void AddKeyValueTypeDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping);

#if NET7_0_OR_GREATER
        public void AddKeyValueTypeDiscriminator<T>(string discriminatorKey, params (string, Type)[] valueTypeMapping) => AddKeyValueTypeDiscriminator<T>(discriminatorKey, valueTypeMapping.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2));
#endif

        public void AddUniqueKeyTypeDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping);

#if NET7_0_OR_GREATER
        public void AddUniqueKeyTypeDiscriminator<T>(params (string, Type)[] uniqueKeyTypeMapping) => AddUniqueKeyTypeDiscriminator<T>(uniqueKeyTypeMapping.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2));
#endif
    }
}
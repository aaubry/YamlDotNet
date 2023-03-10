using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class TypeDiscriminatingNodeDeserializerOptions : ITypeDiscriminatingNodeDeserializerOptions
    {
        internal readonly List<ITypeDiscriminator> discriminators = new List<ITypeDiscriminator>();

        /// <summary>
        /// Adds an <see cref="ITypeDiscriminator" /> to be checked by the TypeDiscriminatingNodeDeserializer.
        /// </summary>
        /// <param name="discriminator">The <see cref="ITypeDiscriminator" /> to add.</param>
        public void AddTypeDiscriminator(ITypeDiscriminator discriminator)
        {
            this.discriminators.Add(discriminator);
        }

        /// <summary>
        /// Adds a <see cref="KeyValueTypeDiscriminator" /> to be checked by the TypeDiscriminatingNodeDeserializer.
        /// <see cref="KeyValueTypeDiscriminator" />s use the value of a specified key on the yaml object to map
        /// to a target type.
        /// </summary>
        /// <param name="discriminatorKey">The yaml key to discriminate on.</param>
        /// <param name="valueTypeMapping">A dictionary of values for the yaml key mapping to their respective types.</param>
        public void AddKeyValueTypeDiscriminator<T>(string discriminatorKey, IDictionary<string, Type> valueTypeMapping)
        {
            this.discriminators.Add(new KeyValueTypeDiscriminator(typeof(T), discriminatorKey, valueTypeMapping));
        }

        /// <summary>
        /// Adds a <see cref="UniqueKeyTypeDiscriminator" /> to be checked by the TypeDiscriminatingNodeDeserializer.
        /// <see cref="UniqueKeyTypeDiscriminator" />s use the presence of unique keys on the yaml object to map
        /// to different target types.
        /// </summary>
        /// <param name="uniqueKeyTypeMapping">A dictionary of unique yaml keys mapping to their respective types.</param>
        public void AddUniqueKeyTypeDiscriminator<T>(IDictionary<string, Type> uniqueKeyTypeMapping)
        {
            this.discriminators.Add(new UniqueKeyTypeDiscriminator(typeof(T), uniqueKeyTypeMapping));
        }
    }
}
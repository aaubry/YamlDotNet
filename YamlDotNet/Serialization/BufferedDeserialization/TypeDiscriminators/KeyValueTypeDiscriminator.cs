using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    /// <summary>
    /// A TypeDiscriminator that discriminates which type to deserialize a yaml stream into by checking the value
    /// of a known key.
    /// </summary>
    public class KeyValueTypeDiscriminator : ITypeDiscriminator
    {
        public Type BaseType { get; private set; }
        private readonly string targetKey;
        private readonly IDictionary<string, Type> typeMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueTypeDiscriminator"/> class.
        /// The KeyValueTypeDiscriminator will check the target key specified, and if it's value is contained within the
        /// type mapping dictionary, the coresponding type will be discriminated.
        /// </summary>
        /// <param name="baseType">The base type which all discriminated types will implement. Use object if you're discriminating
        /// unrelated types. Note the less specific you are with the base type the more yaml will need to be buffered.</param>
        /// <param name="targetKey">The known key to check the value of when discriminating.</param>
        /// <param name="typeMapping">A mapping dictionary of string to types.</param>
        /// <exception cref="ArgumentOutOfRangeException">If any of the target types do not implement the base type.</exception>
        public KeyValueTypeDiscriminator(Type baseType, string targetKey, IDictionary<string, Type> typeMapping)
        {
            foreach (var keyValuePair in typeMapping)
            {
                if (!baseType.IsAssignableFrom(keyValuePair.Value))
                {
                    throw new ArgumentOutOfRangeException($"{nameof(typeMapping)} dictionary contains type {keyValuePair.Value} which is not a assignable to {baseType}");
                }
            }
            this.BaseType = baseType;
            this.targetKey = targetKey;
            this.typeMapping = typeMapping;
        }

        /// <summary>
        /// Checks if the current parser contains the target key, and that it's value matches one of the type mappings.
        /// If so, return true, and the matching type.
        /// Otherwise, return false.
        /// This will consume the parser, so you will usually need the parser to be a buffer so an instance 
        /// of the discriminated type can be deserialized later.
        /// </summary>
        /// <param name="parser">The IParser to consume and discriminate a type from.</param>
        /// <param name="suggestedType">The output type discriminated. Null if there target key was not present of if the value
        /// of the target key was not within the type mapping.</param>
        /// <returns>Returns true if the discriminator matched the yaml stream.</returns>
        public bool TryDiscriminate(IParser parser, out Type? suggestedType)
        {
            if (parser.TryFindMappingEntry(
                scalar => targetKey == scalar.Value,
                out Scalar? key,
                out ParsingEvent? value))
            {
                // read the value of the discriminator key
                if (value is Scalar valueScalar && typeMapping.TryGetValue(valueScalar.Value, out var childType))
                {
                    suggestedType = childType;
                    return true;
                }
            }

            // we could not find our key, thus we could not determine correct child type
            suggestedType = null;
            return false;
        }
    }
}
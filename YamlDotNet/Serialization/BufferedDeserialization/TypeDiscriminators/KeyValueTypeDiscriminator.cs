using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    public class KeyValueTypeDiscriminator : ITypeDiscriminator
    {
        public Type BaseType { get; private set; }
        private readonly string targetKey;
        private readonly IDictionary<string, Type> typeMapping;

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

        public bool TryDiscriminate(IParser parser, out Type? suggestedType)
        {
            if (parser.TryFindMappingEntry(
                scalar => targetKey == scalar.Value,
                out Scalar? key,
                out ParsingEvent? value))
            {
                // read the value of the discriminator key
                if (value is Scalar valueScalar)
                {
                    suggestedType = CheckName(valueScalar.Value);
                    return true;
                }
                else
                {
                    throw new Exception($"Could not determine {BaseType} to deserialize to, {targetKey} has an empty value");
                }
            }

            // we could not find our key, thus we could not determine correct child type
            suggestedType = null;
            return false;
        }

        private Type CheckName(string value)
        {
            if (typeMapping.TryGetValue(value, out var childType))
            {
                return childType;
            }

            var known = string.Join(",", typeMapping.Keys.ToArray());
            
            throw new Exception($"Could not determine {BaseType} to deserialize to, expecting '{targetKey}' to be one of: {known}, but got '{value}'");
        }
    }
}
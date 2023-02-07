using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    public class UniqueKeyTypeDiscriminator : ITypeDiscriminator
    {
        public Type BaseType { get; private set; }

        private readonly IDictionary<string, Type> typeMapping;

        public UniqueKeyTypeDiscriminator(Type baseType, IDictionary<string, Type> typeMapping)
        {
            foreach (var keyValuePair in typeMapping)
            {
                if (!baseType.IsAssignableFrom(keyValuePair.Value))
                {
                    throw new ArgumentOutOfRangeException($"{nameof(typeMapping)} dictionary contains type {keyValuePair.Value} which is not a assignable to {baseType}");
                }
            }
            this.BaseType = baseType;
            this.typeMapping = typeMapping;
        }

        public bool TryDiscriminate(IParser parser, out Type? suggestedType)
        {
            if (parser.TryFindMappingEntry(
                scalar => this.typeMapping.ContainsKey(scalar.Value),
                out Scalar key,
                out ParsingEvent _))
            {
                suggestedType = this.typeMapping[key.Value];
                return true;
            }

            suggestedType = null;
            return false;
        }
    }
}
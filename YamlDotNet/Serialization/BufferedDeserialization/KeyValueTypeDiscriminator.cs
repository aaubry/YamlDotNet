using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class KeyValueTypeDiscriminator : IValueTypeDiscriminator
    {
        public Type BaseType { get; private set; }
        private readonly string targetKey;
        private readonly IDictionary<string, Type> typeMapping;

        public KeyValueTypeDiscriminator(Type baseType, string targetKey, IDictionary<string, Type> typeMapping)
        {
            foreach (var (_, type) in typeMapping)
            {
                if (type == null)
                {
                    throw new ArgumentNullException($"{nameof(typeMapping)} dictionary contains null value. All types must map to valid sub-types of {baseType}");
                }
                if (type == baseType)
                {
                    throw new ArgumentNullException($"{nameof(typeMapping)} dictionary contains base type {baseType} directly. All types must map to valid sub-types of {baseType}");
                }
                if (!baseType.IsAssignableFrom(type))
                {
                    throw new ArgumentOutOfRangeException($"{nameof(typeMapping)} dictionary contains type {type} which is not a valid sub-type of {baseType}. All types must map to valid sub-types of {baseType}");
                }
            }
            this.BaseType = baseType;
            this.targetKey = targetKey;
            this.typeMapping = typeMapping;
        }

        public bool TryDiscriminate(IParser parser, out Type? suggestedType)
        {
            if (TryFindMappingEntry(
                parser,
                scalar => targetKey == scalar.Value,
                out Scalar? key,
                out ParsingEvent? value))
            {
                // read the value of the kind key
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

            var known = string.Join(",", typeMapping.Keys);
            
            throw new Exception($"Could not determine {BaseType} to deserialize to, expecting '{targetKey}' to be one of: {known}, but got '{value}'");
        }

        private bool TryFindMappingEntry(IParser parser, Func<Scalar, bool> selector, out Scalar? key, out ParsingEvent? value)
        {
            parser.Consume<MappingStart>();
            do
            {
                // so we only want to check keys in this mapping, don't descend
                switch (parser.Current)
                {
                    case Scalar scalar:
                        // we've found a scalar, check if it's value matches one
                        // of our  predicate
                        var keyMatched = selector(scalar);

                        // move head so we can read or skip value
                        parser.MoveNext();

                        // read the value of the mapping key
                        if (keyMatched)
                        {
                            // success
                            value = parser.Current;
                            key = scalar;
                            return true;
                        }

                        // skip the value
                        parser.SkipThisAndNestedEvents();

                        break;
                    case MappingStart _:
                    case SequenceStart _:
                        parser.SkipThisAndNestedEvents();
                        break;
                    default:
                        // do nothing, skip to next node
                        parser.MoveNext();
                        break;
                }
            } while (parser.Current != null);

            key = null;
            value = null;
            return false;
        }
    }
}
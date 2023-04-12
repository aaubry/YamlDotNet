// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
        /// <param name="typeMapping">A mapping dictionary of yaml values to types.</param>
        /// <exception cref="ArgumentOutOfRangeException">If any of the target types do not implement the base type.</exception>
        public KeyValueTypeDiscriminator(Type baseType, string targetKey, IDictionary<string, Type> typeMapping)
        {
            foreach (var keyValuePair in typeMapping)
            {
                if (!baseType.IsAssignableFrom(keyValuePair.Value))
                {
                    throw new ArgumentOutOfRangeException(nameof(typeMapping), $"{keyValuePair.Value} is not a assignable to {baseType}");
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
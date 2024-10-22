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
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public class TypeDiscriminatingNodeDeserializerOptions : ITypeDiscriminatingNodeDeserializerOptions
    {
        internal readonly List<ITypeDiscriminator> discriminators = new ();

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

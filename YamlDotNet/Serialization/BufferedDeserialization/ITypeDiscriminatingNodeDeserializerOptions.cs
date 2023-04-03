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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public interface ITypeDiscriminatingNodeDeserializerOptions
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
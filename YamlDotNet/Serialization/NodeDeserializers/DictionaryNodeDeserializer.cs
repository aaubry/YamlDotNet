// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class DictionaryNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory objectFactory;

        public DictionaryNodeDeserializer(IObjectFactory objectFactory)
        {
            this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        }

        bool INodeDeserializer.Deserialize(Node node, Type expectedType, IValueDeserializer deserializer, out object? value)
        {
            IDictionary? dictionary;
            Type keyType, valueType;
            var genericDictionaryType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();
                keyType = genericArguments[0];
                valueType = genericArguments[1];

                value = objectFactory.Create(expectedType);

                dictionary = value as IDictionary;
                if (dictionary == null)
                {
                    // Uncommon case where a type implements IDictionary<TKey, TValue> but not IDictionary
                    dictionary = (IDictionary?)Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<,>).MakeGenericType(keyType, valueType), value);
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(expectedType))
            {
                keyType = typeof(object);
                valueType = typeof(object);

                value = objectFactory.Create(expectedType);
                dictionary = (IDictionary)value;
            }
            else
            {
                value = null;
                return false;
            }

            var mapping = node.Expect<Mapping>();
            DeserializeHelper(keyType, valueType, mapping, deserializer, dictionary!);

            return true;
        }

        private static void DeserializeHelper(Type tKey, Type tValue, Mapping mapping, IValueDeserializer deserializer, IDictionary result)
        {
            foreach (var tuple in mapping)
            {
                var key = deserializer.DeserializeValue(tuple.Key, tKey);
                var value = deserializer.DeserializeValue(tuple.Value, tValue);
                result[key!] = value;
            }
        }
    }
}
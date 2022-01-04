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
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class DictionaryNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory objectFactory;
        private readonly PreexistingDictionaryPopulationStrategy populationStrategy;

        public DictionaryNodeDeserializer(IObjectFactory objectFactory, PreexistingDictionaryPopulationStrategy populationStrategy = PreexistingDictionaryPopulationStrategy.CreateNew)
        {
            this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            this.populationStrategy = populationStrategy;
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?, object?> nestedObjectDeserializer, out object? value, object? currentValue)
        {
            IDictionary? dictionary;
            Type keyType, valueType;
            var genericDictionaryType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();
                keyType = genericArguments[0];
                valueType = genericArguments[1];

                value = (currentValue == null || populationStrategy == PreexistingDictionaryPopulationStrategy.CreateNew) ? objectFactory.Create(expectedType) : currentValue;
                dictionary = value as IDictionary;
                if (dictionary == null)
                {
                    // Uncommon case where a type implements IDictionary<TKey, TValue> but not IDictionary
                    dictionary = (IDictionary?)Activator.CreateInstance(typeof(GenericDictionaryToNonGenericAdapter<,>).MakeGenericType(keyType, valueType), value);

                    // TODO: How to handle pre-existing instance in this case?
                    if (populationStrategy != PreexistingDictionaryPopulationStrategy.CreateNew)
                    {
                        throw new NotSupportedException($"Types implementing generic interface {typeof(IDictionary<,>).Name} but not non-generic interface {typeof(IDictionary).Name} are not yet supported when using {nameof(Deserializer.PopulateObject)}() in combination with {populationStrategy}.");
                    }
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(expectedType))
            {
                keyType = typeof(object);
                valueType = typeof(object);

                value = (currentValue == null || populationStrategy == PreexistingDictionaryPopulationStrategy.CreateNew) ? objectFactory.Create(expectedType) : currentValue;
                dictionary = (IDictionary)value;
            }
            else
            {
                value = null;
                return false;
            }

            DeserializeHelper(keyType, valueType, parser, nestedObjectDeserializer, dictionary!);

            return true;
        }

        private void DeserializeHelper(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?, object?> nestedObjectDeserializer, IDictionary result)
        {
            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                var key = nestedObjectDeserializer(parser, tKey, null);
                var value = nestedObjectDeserializer(parser, tValue, null);
                var valuePromise = value as IValuePromise;

                if (key is IValuePromise keyPromise)
                {
                    if (valuePromise == null)
                    {
                        // Key is pending, value is known
                        keyPromise.ValueAvailable += v => AddKeyValuePair(result, v!, value!); ;
                    }
                    else
                    {
                        // Both key and value are pending. We need to wait until both of them become available.
                        var hasFirstPart = false;

                        keyPromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                AddKeyValuePair(result, v!, value!);
                            }
                            else
                            {
                                key = v!;
                                hasFirstPart = true;
                            }
                        };

                        valuePromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                AddKeyValuePair(result, key!, v!);
                            }
                            else
                            {
                                value = v;
                                hasFirstPart = true;
                            }
                        };
                    }
                }
                else
                {
                    if (valuePromise == null)
                    {
                        // Happy path: both key and value are known
                        AddKeyValuePair(result, key!, value!);
                    }
                    else
                    {
                        // Key is known, value is pending
                        valuePromise.ValueAvailable += v => AddKeyValuePair(result, key!, v!);
                    }
                }
            }
        }

        private void AddKeyValuePair(IDictionary result, object key, object value)
        {
            switch (populationStrategy)
            {
                case PreexistingDictionaryPopulationStrategy.AddItemsThrowOnExistingKeys:
                    result.Add(key!, value!);
                    break;

                case PreexistingDictionaryPopulationStrategy.CreateNew:
                case PreexistingDictionaryPopulationStrategy.AddItemsReplaceExistingKeys:
                    result[key!] = value!;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}

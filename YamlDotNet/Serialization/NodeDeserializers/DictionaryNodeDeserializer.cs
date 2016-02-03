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
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class DictionaryNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory _objectFactory;

        public DictionaryNodeDeserializer(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
        {
            IDictionary dictionary;
            Type keyType, valueType;
            var genericDictionaryType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<,>));
            if (genericDictionaryType != null)
            {
                var genericArguments = genericDictionaryType.GetGenericArguments();
                keyType = genericArguments[0];
                valueType = genericArguments[1];

                value = _objectFactory.Create(expectedType);

                dictionary = value as IDictionary;
                if (dictionary == null)
                {
                    dictionary = new GenericDictionaryToNonGenericAdapter(value, genericDictionaryType);
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(expectedType))
            {
                keyType = typeof(object);
                valueType = typeof(object);

                value = _objectFactory.Create(expectedType);
                dictionary = (IDictionary)value;
            }
            else
            {
                value = null;
                return false;
            }

            DeserializeHelper(keyType, valueType, reader, expectedType, nestedObjectDeserializer, dictionary);

            return true;
        }

        private static void DeserializeHelper(Type tKey, Type tValue, EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, IDictionary result)
        {
            reader.Expect<MappingStart>();
            while (!reader.Accept<MappingEnd>())
            {
                var key = nestedObjectDeserializer(reader, tKey);
                var keyPromise = key as IValuePromise;

                var value = nestedObjectDeserializer(reader, tValue);
                var valuePromise = value as IValuePromise;

                if (keyPromise == null)
                {
                    if (valuePromise == null)
                    {
                        // Happy path: both key and value are known
                        result[key] = value;
                    }
                    else
                    {
                        // Key is known, value is pending
                        valuePromise.ValueAvailable += v => result[key] = v;
                    }
                }
                else
                {
                    if (valuePromise == null)
                    {
                        // Key is pending, value is known
                        keyPromise.ValueAvailable += v => result[v] = value;
                    }
                    else
                    {
                        // Both key and value are pending. We need to wait until both of them becom available.
                        var hasFirstPart = false;

                        keyPromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                result[v] = value;
                            }
                            else
                            {
                                key = v;
                                hasFirstPart = true;
                            }
                        };

                        valuePromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                result[key] = v;
                            }
                            else
                            {
                                value = v;
                                hasFirstPart = true;
                            }
                        };
                    }
                }
            }
            reader.Expect<MappingEnd>();
        }

        /// <summary>
        /// Adapts an <see cref="IDictionary{TKey, TValue}" /> to <see cref="IDictionary" />
        /// because not all generic dictionaries implement <see cref="IDictionary" />.
        /// </summary>
        private sealed class GenericDictionaryToNonGenericAdapter : IDictionary
        {
            private readonly object genericDictionary;
            private readonly MethodInfo indexerSetter;

            public GenericDictionaryToNonGenericAdapter(object genericDictionary, Type genericDictionaryType)
            {
                this.genericDictionary = genericDictionary;

                indexerSetter = genericDictionaryType.GetProperty("Item").GetSetMethod();
            }

            public void Add(object key, object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object key)
            {
                throw new NotImplementedException();
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public bool IsFixedSize
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public ICollection Keys
            {
                get { throw new NotImplementedException(); }
            }

            public void Remove(object key)
            {
                throw new NotImplementedException();
            }

            public ICollection Values
            {
                get { throw new NotImplementedException(); }
            }

            public object this[object key]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    indexerSetter.Invoke(genericDictionary, new object[] { key, value });
                }
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

            public object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
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
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class CollectionNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory _objectFactory;

        public CollectionNodeDeserializer(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
        {
            IList list;
            bool canUpdate = true;
            Type itemType;
            var genericCollectionType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(ICollection<>));
            if (genericCollectionType != null)
            {
                var genericArguments = genericCollectionType.GetGenericArguments();
                itemType = genericArguments[0];

                value = _objectFactory.Create(expectedType);
                list = value as IList;
                if (list == null)
                {
                    var genericListType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IList<>));
                    canUpdate = genericListType != null;
                    list = new GenericCollectionToNonGenericAdapter(value, genericCollectionType, genericListType);
                }
            }
            else if (typeof(IList).IsAssignableFrom(expectedType))
            {
                itemType = typeof(object);

                value = _objectFactory.Create(expectedType);
                list = (IList)value;
            }
            else
            {
                value = false;
                return false;
            }

            DeserializeHelper(itemType, reader, expectedType, nestedObjectDeserializer, list, canUpdate);

            return true;
        }

        internal static void DeserializeHelper(Type tItem, EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, IList result, bool canUpdate)
        {
            reader.Expect<SequenceStart>();
            while (!reader.Accept<SequenceEnd>())
            {
                var current = reader.Parser.Current;

                var value = nestedObjectDeserializer(reader, tItem);
                var promise = value as IValuePromise;
                if (promise == null)
                {
                    result.Add(TypeConverter.ChangeType(value, tItem));
                }
                else if (canUpdate)
                {
                    var index = result.Add(tItem.IsValueType ? Activator.CreateInstance(tItem) : null);
                    promise.ValueAvailable += v => result[index] = TypeConverter.ChangeType(v, tItem);
                }
                else
                {
                    throw new ForwardAnchorNotSupportedException(
                        current.Start,
                        current.End,
                        "Forward alias references are not allowed because this type does not implement IList<>"
                    );
                }
            }
            reader.Expect<SequenceEnd>();
        }

        /// <summary>
        /// Adapts an <see cref="ICollection{T}" /> to <see cref="IList" />
        /// because not all generic collections implement <see cref="IList" />.
        /// </summary>
        private sealed class GenericCollectionToNonGenericAdapter : IList
        {
            private readonly object genericCollection;
            private readonly MethodInfo addMethod;
            private readonly MethodInfo indexerSetter;
            private readonly MethodInfo countGetter;

            public GenericCollectionToNonGenericAdapter(object genericCollection, Type genericCollectionType, Type genericListType)
            {
                this.genericCollection = genericCollection;

                addMethod = genericCollectionType.GetMethod("Add");
                countGetter = genericCollectionType.GetProperty("Count").GetGetMethod();

                if (genericListType != null)
                {
                    indexerSetter = genericListType.GetProperty("Item").GetSetMethod();
                }
            }

            public int Add(object value)
            {
                var index = (int)countGetter.Invoke(genericCollection, null);
                addMethod.Invoke(genericCollection, new object[] { value });
                return index;
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object value)
            {
                throw new NotImplementedException();
            }

            public int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, object value)
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

            public void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public object this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    indexerSetter.Invoke(genericCollection, new object[] { index, value });
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

            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}

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
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class StaticArrayNodeDeserializer : INodeDeserializer
    {
        private readonly StaticObjectFactory factory;

        public StaticArrayNodeDeserializer(StaticObjectFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
        {
            if (!factory.IsArray(expectedType))
            {
                value = false;
                return false;
            }
            var itemType = factory.GetValueType(expectedType);
            var items = new ArrayList();
            StaticCollectionNodeDeserializer.DeserializeHelper(itemType, parser, nestedObjectDeserializer, items, factory);
            var array = factory.CreateArray(expectedType, items.Count);
            items.CopyTo(array, 0);

            value = array;
            return true;
        }

        private sealed class ArrayList : IList
        {
            private object?[] data;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Initialized inside Clear()
            public ArrayList()
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
            {
                Clear();
            }

            public int Add(object? value)
            {
                if (Count == data.Length)
                {
                    Array.Resize(ref data, data.Length * 2);
                }
                data[Count] = value;
                return Count++;
            }

            public void Clear()
            {
                data = new object[10];
                Count = 0;
            }

            bool IList.Contains(object? value) => throw new NotSupportedException();
            int IList.IndexOf(object? value) => throw new NotSupportedException();
            void IList.Insert(int index, object? value) => throw new NotSupportedException();
            void IList.Remove(object? value) => throw new NotSupportedException();
            void IList.RemoveAt(int index) => throw new NotSupportedException();

            public bool IsFixedSize => false;

            public bool IsReadOnly => false;

            public object? this[int index]
            {
                get
                {
                    return data[index];
                }
                set
                {
                    data[index] = value;
                }
            }

            public void CopyTo(Array array, int index)
            {
                Array.Copy(data, 0, array, index, Count);
            }

            public int Count { get; private set; }

            public bool IsSynchronized => false;
            public object SyncRoot => data;

            public IEnumerator GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return data[i];
                }
            }
        }
    }
}


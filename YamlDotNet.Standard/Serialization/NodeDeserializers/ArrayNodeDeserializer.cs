//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class ArrayNodeDeserializer : INodeDeserializer
    {
        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            if (!expectedType.IsArray)
            {
                value = false;
                return false;
            }

            var itemType = expectedType.GetElementType();

            var items = new ArrayList();
            CollectionNodeDeserializer.DeserializeHelper(itemType, parser, nestedObjectDeserializer, items, true);

            var array = Array.CreateInstance(itemType, items.Count);
            items.CopyTo(array, 0);

            value = array;
            return true;
        }

        private sealed class ArrayList : IList
        {
            private object[] data;
            private int count;

            public ArrayList()
            {
                Clear();
            }

            public int Add(object value)
            {
                if (count == data.Length)
                {
                    Array.Resize(ref data, data.Length * 2);
                }
                data[count] = value;
                return count++;
            }

            public void Clear()
            {
                data = new object[10];
                count = 0;
            }

            public bool Contains(object value)
            {
                throw new NotSupportedException();
            }

            public int IndexOf(object value)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public void Remove(object value)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public object this[int index]
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
                Array.Copy(data, 0, array, index, count);
            }

            public int Count
            {
                get { return count; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return data; }
            }

            public IEnumerator GetEnumerator()
            {
                for (int i = 0; i < count; ++i)
                {
                    yield return data[i];
                }
            }
        }
    }
}


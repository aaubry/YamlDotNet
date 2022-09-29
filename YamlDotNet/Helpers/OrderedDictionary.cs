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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace YamlDotNet.Helpers
{
    [Serializable]
    internal sealed class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
        where TKey : notnull
    {
        [NonSerialized]
        private Dictionary<TKey, TValue> dictionary;
        private readonly List<KeyValuePair<TKey, TValue>> list;
        private readonly IEqualityComparer<TKey> comparer;

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set
            {
                if (dictionary.ContainsKey(key))
                {
                    var index = list.FindIndex(kvp => comparer.Equals(kvp.Key, key));
                    dictionary[key] = value;
                    list[index] = new KeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys => new KeyCollection(this);

        public ICollection<TValue> Values => new ValueCollection(this);

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public OrderedDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            list = new List<KeyValuePair<TKey, TValue>>();
            dictionary = new Dictionary<TKey, TValue>(comparer);
            this.comparer = comparer;
        }

        public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item.Key, item.Value);
            list.Add(item);
        }

        public void Clear()
        {
            dictionary.Clear();
            list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => dictionary.Contains(item);

        public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            list.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => list.GetEnumerator();

        public void Insert(int index, TKey key, TValue value)
        {
            dictionary.Add(key, value);
            list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            if (dictionary.ContainsKey(key))
            {
                var index = list.FindIndex(kvp => comparer.Equals(kvp.Key, key));
                list.RemoveAt(index);
                if (!dictionary.Remove(key))
                {
                    throw new InvalidOperationException();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public void RemoveAt(int index)
        {
            var key = list[index].Key;
            dictionary.Remove(key);
            list.RemoveAt(index);
        }

#if !(NETCOREAPP3_1)
#pragma warning disable 8767 // Nullability of reference types in type of parameter ... doesn't match implicitly implemented member
#endif

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
            dictionary.TryGetValue(key, out value);

#if !(NETCOREAPP3_1)
#pragma warning restore 8767
#endif

        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Reconstruct the dictionary from the serialized list
            dictionary = new Dictionary<TKey, TValue>();
            foreach (var kvp in list)
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }

        private class KeyCollection : ICollection<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> orderedDictionary;

            public int Count => orderedDictionary.list.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TKey item) => orderedDictionary.dictionary.Keys.Contains(item);

            public KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                this.orderedDictionary = orderedDictionary;
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                for (var i = 0; i < orderedDictionary.list.Count; i++)
                {
                    array[i] = orderedDictionary.list[i + arrayIndex].Key;
                }
            }

            public IEnumerator<TKey> GetEnumerator() =>
                orderedDictionary.list.Select(kvp => kvp.Key).GetEnumerator();

            public bool Remove(TKey item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ValueCollection : ICollection<TValue>
        {
            private readonly OrderedDictionary<TKey, TValue> orderedDictionary;

            public int Count => orderedDictionary.list.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(TValue item) => orderedDictionary.dictionary.Values.Contains(item);

            public ValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                this.orderedDictionary = orderedDictionary;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                for (var i = 0; i < orderedDictionary.list.Count; i++)
                {
                    array[i] = orderedDictionary.list[i + arrayIndex].Value;
                }
            }

            public IEnumerator<TValue> GetEnumerator() =>
                orderedDictionary.list.Select(kvp => kvp.Value).GetEnumerator();

            public bool Remove(TValue item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

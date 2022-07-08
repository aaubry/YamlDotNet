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

namespace YamlDotNet.Helpers
{
    internal static class ReadOnlyCollectionExtensions
    {
        private sealed class ReadOnlyListAdapter<T> : IReadOnlyList<T>
        {
            private readonly List<T> list;

            public ReadOnlyListAdapter(List<T> list)
            {
                this.list = list ?? throw new ArgumentNullException(nameof(list));
            }

            public T this[int index] => list[index];
            public int Count => list.Count;
            public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
        }

        public static IReadOnlyList<T> AsReadonlyList<T>(this List<T> list)
        {
            return new ReadOnlyListAdapter<T>(list);
        }

        private sealed class ReadOnlyDictionaryAdapter<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
        {
            private readonly Dictionary<TKey, TValue> dictionary;

            public ReadOnlyDictionaryAdapter(Dictionary<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            }

            public TValue this[TKey key] => dictionary[key];

            public IEnumerable<TKey> Keys => dictionary.Keys;

            public IEnumerable<TValue> Values => dictionary.Values;

            public int Count => dictionary.Count;

            public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => dictionary.TryGetValue(key, out value);

            IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadonlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            return new ReadOnlyDictionaryAdapter<TKey, TValue>(dictionary);
        }
    }
}

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

using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Test.Serialization
{
    /// <summary>
    /// Test Dictionary that implements <see cref="IReadOnlyDictionary{TKey, TValue}" />, but not
    /// <see cref="IDictionary{TKey, TValue}"/> nor <see cref="IDictionary"/>.
    /// </summary>
    public class GenericTestReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> dictionary;

        public GenericTestReadOnlyDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
        }

        public TValue this[TKey key]
        {
            get { return dictionary[key]; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return dictionary.Values; }
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }
    }
}

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

namespace YamlDotNet.Helpers
{
    /// <summary>
    /// Adapts an <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" /> to <see cref="IDictionary" />
    /// because not all generic dictionaries implement <see cref="IDictionary" />.
    /// </summary>
    internal sealed class GenericDictionaryToNonGenericAdapter<TKey, TValue> : IDictionary
        where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> genericDictionary;

        public GenericDictionaryToNonGenericAdapter(IDictionary<TKey, TValue> genericDictionary)
        {
            this.genericDictionary = genericDictionary ?? throw new ArgumentNullException(nameof(genericDictionary));
        }

        public void Add(object key, object? value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object key)
        {
            throw new NotSupportedException();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new DictionaryEnumerator(genericDictionary.GetEnumerator());
        }

        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        public ICollection Keys
        {
            get { throw new NotSupportedException(); }
        }

        public void Remove(object key)
        {
            throw new NotSupportedException();
        }

        public ICollection Values
        {
            get { throw new NotSupportedException(); }
        }

        public object? this[object key]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                genericDictionary[(TKey)key] = (TValue)value!;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(Key, Value);
                }
            }

            public object Key
            {
                get { return enumerator.Current.Key!; }
            }

            public object? Value
            {
                get { return enumerator.Current.Value; }
            }

            public object Current
            {
                get { return Entry; }
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }
    }
}

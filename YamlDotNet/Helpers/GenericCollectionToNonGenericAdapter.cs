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
    /// Adapts an <see cref="System.Collections.Generic.ICollection{T}" /> to <see cref="IList" />
    /// because not all generic collections implement <see cref="IList" />.
    /// </summary>
    internal sealed class GenericCollectionToNonGenericAdapter<T> : IList
    {
        private readonly ICollection<T> genericCollection;

        public GenericCollectionToNonGenericAdapter(ICollection<T> genericCollection)
        {
            this.genericCollection = genericCollection ?? throw new ArgumentNullException(nameof(genericCollection));
        }

        public int Add(object? value)
        {
            var index = genericCollection.Count;
            genericCollection.Add((T)value!);
            return index;
        }

        public void Clear()
        {
            genericCollection.Clear();
        }

        public bool Contains(object? value)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(object? value)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        public void Remove(object? value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public object? this[int index]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                ((IList<T>)genericCollection)[index] = (T)value!;
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

        public IEnumerator GetEnumerator()
        {
            return genericCollection.GetEnumerator();
        }
    }
}

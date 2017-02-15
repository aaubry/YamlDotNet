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
using System.Reflection;

namespace YamlDotNet.Helpers
{
    /// <summary>
    /// Adapts an <see cref="System.Collections.Generic.ICollection{T}" /> to <see cref="IList" />
    /// because not all generic collections implement <see cref="IList" />.
    /// </summary>
    internal sealed class GenericCollectionToNonGenericAdapter : IList
    {
        private readonly object genericCollection;
        private readonly MethodInfo addMethod;
        private readonly MethodInfo indexerSetter;
        private readonly MethodInfo countGetter;

        public GenericCollectionToNonGenericAdapter(object genericCollection, Type genericCollectionType, Type genericListType)
        {
            this.genericCollection = genericCollection;

            addMethod = genericCollectionType.GetPublicInstanceMethod("Add");
            countGetter = genericCollectionType.GetPublicProperty("Count").GetGetMethod();

            if (genericListType != null)
            {
                indexerSetter = genericListType.GetPublicProperty("Item").GetSetMethod();
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
            throw new NotSupportedException();
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
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
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
                throw new NotSupportedException();
            }
            set
            {
                indexerSetter.Invoke(genericCollection, new object[] { index, value });
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
            return ((IEnumerable)genericCollection).GetEnumerator();
        }
    }
}
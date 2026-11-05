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

namespace YamlDotNet.Serialization.Utilities
{
    internal sealed class ArrayDimension : IEnumerable
    {
        private readonly Array array;
        private readonly int dimension;
        private readonly int[] indices;

        public ArrayDimension(Array array)
            : this(array, 0, new int[array.Rank])
        {
            for (var i = 0; i < array.Rank; ++i)
            {
                if (array.GetLowerBound(i) != 0)
                {
                    throw new NotSupportedException("Only zero-based multidimensional arrays can be serialized as YAML sequences.");
                }
            }
        }

        private ArrayDimension(Array array, int dimension, int[] indices)
        {
            this.array = array;
            this.dimension = dimension;
            this.indices = indices;
        }

        public Type ItemType => dimension == array.Rank - 1 ? array.GetType().GetElementType()! : typeof(ArrayDimension);

        public IEnumerator GetEnumerator()
        {
            var position = (int[])indices.Clone();
            for (var i = 0; i < array.GetLength(dimension); ++i)
            {
                position[dimension] = i;
                yield return dimension == array.Rank - 1
                    ? array.GetValue(position)
                    : new ArrayDimension(array, dimension + 1, (int[])position.Clone());
            }
        }
    }
}

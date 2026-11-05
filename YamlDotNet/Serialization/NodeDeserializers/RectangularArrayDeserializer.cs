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
using System.Collections.ObjectModel;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    internal static class RectangularArrayDeserializer
    {
        public static Array Deserialize(IParser parser, Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            INamingConvention enumNamingConvention, ITypeInspector typeInspector)
        {
            var rank = expectedType.GetArrayRank();
            var elementType = expectedType.GetElementType()!;
            var rowType = elementType;
            for (var i = 1; i < rank; ++i)
            {
                rowType = typeof(ArrayRow<>).MakeGenericType(rowType);
            }

            parser.Accept<SequenceStart>(out var start);
            var rows = new List<object?>();
            // Keep the root anchor owned by the outer value deserializer. Only
            // nested rows go through nestedObjectDeserializer again.
            CollectionNodeDeserializer.DeserializeHelper(rowType, parser, nestedObjectDeserializer,
                rows, true, enumNamingConvention, typeInspector);

            var lengths = new int[rank];
            for (var i = 0; i < rank; ++i)
            {
                lengths[i] = -1;
            }
            Measure(rows, 0);
            for (var i = 0; i < rank; ++i)
            {
                // A dimension hidden after an empty sequence has no YAML data.
                if (lengths[i] < 0)
                {
                    lengths[i] = 0;
                }
            }

            var array = Array.CreateInstance(elementType, lengths);
            Copy(rows, 0, new int[rank]);
            return array;

            void Measure(IList row, int dimension)
            {
                if (lengths[dimension] < 0)
                {
                    lengths[dimension] = row.Count;
                }
                else if (lengths[dimension] != row.Count)
                {
                    throw InvalidShape($"Dimension {dimension + 1} has inconsistent lengths in a rectangular array.");
                }

                if (dimension < rank - 1)
                {
                    foreach (var item in row)
                    {
                        if (item is not IArrayRow child)
                        {
                            throw InvalidShape("A rectangular array cannot contain null rows.");
                        }
                        Measure(child, dimension + 1);
                    }
                }
            }

            void Copy(IList row, int dimension, int[] indices)
            {
                for (var i = 0; i < row.Count; ++i)
                {
                    indices[dimension] = i;
                    if (dimension == rank - 1)
                    {
                        array.SetValue(row[i], indices);
                    }
                    else
                    {
                        Copy((IArrayRow)row[i]!, dimension + 1, indices);
                    }
                }

                if (dimension == rank - 1)
                {
                    var prefix = (int[])indices.Clone();
                    // Self-references may resolve only after this array is
                    // returned. Forward their updates to every aliased row.
                    ((IArrayRow)row).ValueChanged += (index, item) =>
                    {
                        var position = (int[])prefix.Clone();
                        position[dimension] = index;
                        array.SetValue(item, position);
                    };
                }
            }

            YamlException InvalidShape(string message) => new YamlException(
                start?.Start ?? Mark.Empty, start?.End ?? Mark.Empty, message);
        }

        private interface IArrayRow : IList
        {
            event Action<int, object?>? ValueChanged;
        }

        private sealed class ArrayRow<T> : Collection<T>, IArrayRow
        {
            public ArrayRow()
            {
            }

            public event Action<int, object?>? ValueChanged;

            protected override void SetItem(int index, T item)
            {
                base.SetItem(index, item);
                ValueChanged?.Invoke(index, item);
            }
        }
    }
}

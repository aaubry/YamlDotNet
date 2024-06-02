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
using YamlDotNet.Helpers;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Represents a location inside a file
    /// </summary>
    public readonly struct Mark : IEquatable<Mark>, IComparable<Mark>, IComparable
    {
        /// <summary>
        /// Gets a <see cref="Mark"/> with empty values.
        /// </summary>
        public static readonly Mark Empty = new Mark(0, 1, 1);

        /// <summary>
        /// Gets / sets the absolute offset in the file
        /// </summary>
        public long Index { get; }

        /// <summary>
        /// Gets / sets the number of the line
        /// </summary>
        public long Line { get; }

        /// <summary>
        /// Gets / sets the index of the column
        /// </summary>
        public long Column { get; }

        public Mark(long index, long line, long column)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to zero.");
            }
            if (line < 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(line), "Line must be greater than or equal to 1.");
            }
            if (column < 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(column), "Column must be greater than or equal to 1.");
            }

            Index = index;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Line: {Line}, Col: {Column}, Idx: {Index}";
        }

        /// <summary />
        public override bool Equals(object? obj)
        {
            return Equals((Mark)(obj ?? Empty));
        }

        /// <summary />
        public bool Equals(Mark other)
        {
            return Index == other.Index
                && Line == other.Line
                && Column == other.Column;
        }

        /// <summary />
        public override int GetHashCode()
        {
            return HashCode.CombineHashCodes(
                Index.GetHashCode(),
                HashCode.CombineHashCodes(
                    Line.GetHashCode(),
                    Column.GetHashCode()
                )
            );
        }

        /// <summary />
        public int CompareTo(object? obj)
        {
            return CompareTo((Mark)(obj ?? Empty));
        }

        /// <summary />
        public int CompareTo(Mark other)
        {
            var cmp = Line.CompareTo(other.Line);
            if (cmp == 0)
            {
                cmp = Column.CompareTo(other.Column);
            }
            return cmp;
        }

        public static bool operator ==(Mark left, Mark right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Mark left, Mark right)
        {
            return !(left == right);
        }

        public static bool operator <(Mark left, Mark right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Mark left, Mark right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Mark left, Mark right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Mark left, Mark right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}

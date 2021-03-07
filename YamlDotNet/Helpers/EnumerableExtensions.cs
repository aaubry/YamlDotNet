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
using System.Collections.Generic;

namespace YamlDotNet.Helpers
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Wraps an <see cref="IEnumerable{T}" /> to ensure that it will only be enumerated once,
        /// even if the returned <see cref="IEnumerable{T}" /> is enumerated multiple times.
        /// This is achieved by caching the results as they are consumed from the inner sequence.
        /// </summary>
        /// <remarks>
        /// The inner enumerator is disposed only when its end is reached.
        /// If it is never enumerated to the end, it will never be disposed.
        /// </remarks>
        public static IEnumerable<T> Buffer<T>(this IEnumerable<T> sequence)
        {
            return new BufferedEnumerable<T>(sequence);
        }

        private sealed class BufferedEnumerable<T> : IEnumerable<T>
        {
            private readonly List<T> buffer = new List<T>();
            private bool isComplete;
            private IEnumerable<T>? sequence;
            private IEnumerator<T>? enumerator;
            private Exception? exception;

            public BufferedEnumerable(IEnumerable<T> sequence)
            {
                this.sequence = sequence;
            }

            public IEnumerator<T> GetEnumerator()
            {
                lock (buffer)
                {
                    if (isComplete)
                    {
                        return buffer.GetEnumerator();
                    }

                    if (enumerator == null && exception == null)
                    {
                        enumerator = sequence!.GetEnumerator();
                        sequence = null;
                    }
                    return new BufferedEnumerator(this);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private bool EnumeratorMoveNext(ref int currentIndex)
            {
                lock (buffer)
                {
                    if (currentIndex + 1 < buffer.Count)
                    {
                        ++currentIndex;
                        return true;
                    }

                    if (!isComplete)
                    {
                        if (exception != null)
                        {
                            throw exception;
                        }

                        bool hasNext;
                        try
                        {
                            hasNext = enumerator!.MoveNext();
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                            enumerator!.Dispose();
                            enumerator = null;
                            throw;
                        }

                        if (hasNext)
                        {
                            buffer.Add(enumerator.Current);
                            ++currentIndex;
                            return true;
                        }

                        isComplete = true;
                        enumerator.Dispose();
                        enumerator = null;
                    }

                    return false;
                }
            }

            private sealed class BufferedEnumerator : IEnumerator<T>
            {
                private readonly BufferedEnumerable<T> owner;
                private int currentIndex;

                public BufferedEnumerator(BufferedEnumerable<T> owner)
                {
                    this.owner = owner;
                    currentIndex = -1;
                }

                public T Current => owner.buffer[currentIndex];
                object? IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    return owner.EnumeratorMoveNext(ref currentIndex);
                }

                public void Reset()
                {
                    currentIndex = 0;
                }
            }
        }

        /// <summary>
        /// Wraps an <see cref="IEnumerable{T}" /> to ensure that it can only be enumerated once.
        /// </summary>
        public static IEnumerable<T> SingleUse<T>(this IEnumerable<T> sequence)
        {
            return new SingleUseEnumerable<T>(sequence);
        }

        private sealed class SingleUseEnumerable<T> : IEnumerable<T>
        {
            private IEnumerable<T>? sequence;

            public SingleUseEnumerable(IEnumerable<T> sequence)
            {
                this.sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (sequence == null)
                {
                    throw new InvalidOperationException("This sequence cannot be enumerated more than once");
                }
                var enumerator = this.sequence.GetEnumerator();
                this.sequence = null;
                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}

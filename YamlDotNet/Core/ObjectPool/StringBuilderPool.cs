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
using System.Diagnostics;
using System.Text;

namespace YamlDotNet.Core.ObjectPool
{
    /// <summary>
    /// Pooling of StringBuilder instances.
    /// </summary>
    /// <remarks>
    /// The builders are cached per thread. The scanner rents and returns up to four builders for
    /// every scalar it reads, and a cache that is only ever used by one thread serves that without
    /// the interlocked operations that a pool shared between threads needs on every rent and return.
    /// </remarks>
    [DebuggerStepThrough]
    internal static class StringBuilderPool
    {
        // Scanning a scalar keeps up to four builders rented at the same time.
        private const int MaximumRetainedPerThread = 4;

        private static readonly StringBuilderPooledObjectPolicy Policy = new StringBuilderPooledObjectPolicy
        {
            InitialCapacity = 16,
            MaximumRetainedCapacity = 1024
        };

        [ThreadStatic]
        private static BuilderCache? ThreadCache;

        public static BuilderWrapper Rent()
        {
            var builder = ThreadCache?.Pop() ?? Policy.Create();
            Debug.Assert(builder.Length == 0);
            return new BuilderWrapper(builder);
        }

        private static void Return(StringBuilder builder)
        {
            // The policy clears the builder, or rejects it when it has grown too large to keep.
            if (Policy.Return(builder))
            {
                (ThreadCache ??= new BuilderCache()).Push(builder);
            }
        }

        internal readonly struct BuilderWrapper : IDisposable
        {
            public readonly StringBuilder Builder;

            public BuilderWrapper(StringBuilder builder)
            {
                Builder = builder;
            }

            public override string ToString()
            {
                return Builder.ToString();
            }

            public void Dispose()
            {
                Return(Builder);
            }
        }

        private sealed class BuilderCache
        {
            private readonly StringBuilder?[] builders = new StringBuilder?[MaximumRetainedPerThread];
            private int count;

            public StringBuilder? Pop()
            {
                if (count == 0)
                {
                    return null;
                }

                var builder = builders[--count];
                builders[count] = null;
                return builder;
            }

            public void Push(StringBuilder builder)
            {
                if (count < builders.Length)
                {
                    builders[count++] = builder;
                }
            }
        }
    }
}

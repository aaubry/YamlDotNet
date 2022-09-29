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

namespace YamlDotNet.Helpers
{
    /// <summary>
    /// Pooling of StringBuilder instances.
    /// </summary>
    [DebuggerStepThrough]
    internal static class StringBuilderPool
    {
        private static readonly ConcurrentObjectPool<StringBuilder> Pool;

        static StringBuilderPool()
        {
            Pool = new ConcurrentObjectPool<StringBuilder>(() => new StringBuilder());
        }

        public static BuilderWrapper Rent()
        {
            var builder = Pool.Allocate();
            Debug.Assert(builder.Length == 0);
            return new BuilderWrapper(builder, Pool);
        }

        internal readonly struct BuilderWrapper : IDisposable
        {
            public readonly StringBuilder Builder;
            private readonly ConcurrentObjectPool<StringBuilder> _pool;

            public BuilderWrapper(StringBuilder builder, ConcurrentObjectPool<StringBuilder> pool)
            {
                Builder = builder;
                _pool = pool;
            }

            public override string ToString()
            {
                return Builder.ToString();
            }

            public void Dispose()
            {
                var builder = Builder;

                // do not store builders that are too large.
                if (builder.Capacity <= 1024)
                {
                    builder.Length = 0;
                    _pool.Free(builder);
                }
            }
        }
    }
}

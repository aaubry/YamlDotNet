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
using YamlDotNet.Core;

namespace YamlDotNet.Core.ObjectPool
{
    /// <summary>
    /// Pooling of <see cref="StringLookAheadBuffer"/> instances.
    /// </summary>
    internal static class StringLookAheadBufferPool
    {
        private static readonly ObjectPool<StringLookAheadBuffer> Pool = ObjectPool.Create(new DefaultPooledObjectPolicy<StringLookAheadBuffer>());

        public static BufferWrapper Rent(string value)
        {
            var buffer = Pool.Get();
            Debug.Assert(buffer.Length == 0);

            buffer.Value = value;
            return new BufferWrapper(buffer, Pool);
        }

        internal readonly struct BufferWrapper : IDisposable
        {
            public readonly StringLookAheadBuffer Buffer;
            private readonly ObjectPool<StringLookAheadBuffer> pool;

            public BufferWrapper(StringLookAheadBuffer buffer, ObjectPool<StringLookAheadBuffer> pool)
            {
                Buffer = buffer;
                this.pool = pool;
            }

            public override string ToString()
            {
                return Buffer.ToString()!;
            }

            public void Dispose()
            {
                pool.Return(Buffer);
            }
        }
    }
}

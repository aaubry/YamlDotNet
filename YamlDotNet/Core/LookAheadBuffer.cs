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
using System.IO;
using YamlDotNet.Helpers;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Provides access to a stream and allows to peek at the next characters,
    /// up to the buffer's capacity.
    /// </summary>
    /// <remarks>
    /// This class implements a circular buffer with a fixed capacity.
    /// </remarks>
    [DebuggerStepThrough]
    public sealed class LookAheadBuffer : ILookAheadBuffer
    {
        private readonly TextReader input;
        private readonly char[] buffer;
        private readonly int blockSize;
        private readonly int mask;
        private int firstIndex;
        private int writeOffset;
        private int count;
        private bool endOfInput;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookAheadBuffer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="capacity">The capacity.</param>
        public LookAheadBuffer(TextReader input, int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity must be positive.");
            }

            if (!capacity.IsPowerOfTwo())
            {
                throw new ArgumentException("The capacity must be a power of 2.", nameof(capacity));
            }

            this.input = input ?? throw new ArgumentNullException(nameof(input));

            blockSize = capacity;

            // Allocate twice the required capacity to ensure that 
            buffer = new char[capacity * 2];
            mask = capacity * 2 - 1;
        }

        /// <summary>
        /// Gets a value indicating whether the end of the input reader has been reached.
        /// </summary>
        public bool EndOfInput => endOfInput && count == 0;

        /// <summary>
        /// Gets the index of the character for the specified offset.
        /// </summary>
        private int GetIndexForOffset(int offset)
        {
            return (firstIndex + offset) & mask;
        }

        /// <summary>
        /// Gets the character at the specified offset.
        /// </summary>
        public char Peek(int offset)
        {
#if DEBUG
            if (offset < 0 || offset >= blockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "The offset must be between zero and the capacity of the buffer.");
            }
#endif
            if (offset >= count)
            {
                FillBuffer();
            }

            if (offset < count)
            {
                return buffer[(firstIndex + offset) & mask];
            }
            else
            {
                return '\0';
            }
        }

        /// <summary>
        /// Reads characters until at least <paramref name="length"/> characters are in the buffer.
        /// </summary>
        /// <param name="length">
        /// Number of characters to cache.
        /// </param>
        public void Cache(int length)
        {
            if (length >= count)
            {
                FillBuffer();
            }
        }

        private void FillBuffer()
        {
            if (endOfInput)
            {
                return;
            }

            var remainingSize = blockSize;
            do
            {
                var readCount = input.Read(buffer, writeOffset, remainingSize);
                if (readCount == 0)
                {
                    endOfInput = true;
                    return;
                }

                remainingSize -= readCount;
                writeOffset += readCount;
                count += readCount;
            } while (remainingSize > 0);

            if (writeOffset == buffer.Length)
            {
                writeOffset = 0;
            }

            Debug.Assert(writeOffset == 0 || writeOffset == blockSize);
        }

        /// <summary>
        /// Skips the next <paramref name="length"/> characters. Those characters must have been
        /// obtained first by calling the <see cref="Peek"/> or <see cref="Cache"/> methods.
        /// </summary>
        public void Skip(int length)
        {
            if (length < 1 || length > blockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length must be between 1 and the number of characters in the buffer. Use the Peek() and / or Cache() methods to fill the buffer.");
            }
            firstIndex = GetIndexForOffset(length);
            count -= length;
        }
    }
}

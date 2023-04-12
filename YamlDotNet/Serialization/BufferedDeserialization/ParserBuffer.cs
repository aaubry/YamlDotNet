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
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    /// <summary>
    /// Wraps a <see cref="IParser"/> instance and allows it to be buffered as a LinkedList in memory and replayed.
    /// </summary>
    public class ParserBuffer : IParser
    {
        private readonly LinkedList<ParsingEvent> buffer;

        private LinkedListNode<ParsingEvent>? current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserBuffer"/> class.
        /// </summary>
        /// <param name="parserToBuffer">The Parser to buffer.</param>
        /// <param name="maxDepth">The maximum depth of the parser to buffer before raising an ArgumentOutOfRangeException.</param>
        /// <param name="maxLength">The maximum length of the LinkedList can buffer before raising an ArgumentOutOfRangeException.</param>
        /// <exception cref="ArgumentOutOfRangeException">If parser does not fit within the max depth and length specified.</exception>
        public ParserBuffer(IParser parserToBuffer, int maxDepth, int maxLength)
        {
            buffer = new LinkedList<ParsingEvent>();
            buffer.AddLast(parserToBuffer.Consume<MappingStart>());
            var depth = 0;
            do
            {
                var next = parserToBuffer.Consume<ParsingEvent>();
                depth += next.NestingIncrease;
                buffer.AddLast(next);

                if (maxDepth > -1 && depth > maxDepth)
                {
                    throw new ArgumentOutOfRangeException(nameof(parserToBuffer), "Parser buffer exceeded max depth");
                }
                if (maxLength > -1 && buffer.Count > maxLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(parserToBuffer), "Parser buffer exceeded max length");
                }
            } while (depth >= 0);

            current = buffer.First;
        }

        /// <summary>
        /// Gets the current event. Returns null after <see cref="MoveNext" /> returns false.
        /// </summary>
        public ParsingEvent? Current => current?.Value;

        /// <summary>
        /// Moves to the next event.
        /// </summary>
        /// <returns>Returns true if there are more events available, otherwise returns false.</returns>
        public bool MoveNext()
        {
            current = current?.Next;
            return current != null;
        }

        /// <summary>
        /// Resets the buffer back to it's first event.
        /// </summary>
        public void Reset()
        {
            current = buffer.First;
        }
    }
}
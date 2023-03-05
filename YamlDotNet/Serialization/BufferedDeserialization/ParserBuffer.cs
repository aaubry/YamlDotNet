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
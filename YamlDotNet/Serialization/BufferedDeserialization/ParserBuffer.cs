using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
        public class ParserBuffer : IParser
    {
        private readonly LinkedList<ParsingEvent> buffer;

        private LinkedListNode<ParsingEvent>? current;

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
                    throw new ArgumentOutOfRangeException("Attempted to buffer a parser beyond specified max depth");
                }
                if (maxLength > -1 && buffer.Count > maxLength)
                {
                    throw new ArgumentOutOfRangeException("Attempted to buffer a parser beyond specified max length");
                }
            } while (depth >= 0);

            current = buffer.First;
        }

        public ParsingEvent? Current => current?.Value;

        public bool MoveNext()
        {
            current = current?.Next;
            return current != null;
        }

        public void Reset()
        {
            current = buffer.First;
        }
    }
}
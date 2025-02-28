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

using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.BufferedDeserialization;

public class ParserAnchorBuffer : IParser
{
    private readonly LinkedList<ParsingEvent> buffer;

    private LinkedListNode<ParsingEvent>? current;

    public AnchorName Anchor { get; }

    public bool IsCycling { get; private set; }

    public ParserAnchorBuffer(AnchorName anchor, IParser parserToBuffer)
    {
        Anchor = anchor;
        buffer = new LinkedList<ParsingEvent>();
        var depth = -1;
        do
        {
            var next = parserToBuffer.Consume<ParsingEvent>();
            depth += next.NestingIncrease;
            buffer.AddLast(next);

            if (next is AnchorAlias alias && alias.Value == Anchor)
            {
                IsCycling = true;
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

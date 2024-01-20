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
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Core.ParsingComments
{
    public sealed class NodeCommentValueDeserializer : IValueDeserializer
    {
        private readonly NodeValueDeserializer inner;

        public NodeCommentValueDeserializer(IList<INodeDeserializer> deserializers, IList<INodeTypeResolver> typeResolvers, ITypeConverter typeConverter)
        {
            inner = new NodeValueDeserializer(deserializers, typeResolvers, typeConverter)
            {
                GetNodeEvent = this.GetNodeEvent
            };
        }

        public object? DeserializeValue(IParser parser, Type expectedType, SerializerState state, IValueDeserializer nestedObjectDeserializer)
        {
            if (!(parser is ParserWithComments))
            {
                throw new ArgumentException($"In {this.GetType().Name} the parser must be of type {typeof(ParserWithComments).Name} ParserWithComments");
            }
            return inner.DeserializeValue(parser, expectedType, state, nestedObjectDeserializer);
        }

        public NodeEvent? GetNodeEvent(IParser parser, Type expectedType)
        {
            parser.Accept<NodeEvent>(out var nodeEvent);

            if (nodeEvent == null
                && !((IParserWithComments)parser).SkipComments
                && !typeof(IYamlConvertible).IsAssignableFrom(expectedType))
            {
                if (parser.Current is YamlDotNet.Core.Events.Comment cmt && cmt.IsInline)
                {
                    return nodeEvent;
                }
                ((IParserWithComments)parser).SkipFollowingComments();
                parser.Accept<NodeEvent>(out nodeEvent);
            }

            return nodeEvent;
        }
    }
}

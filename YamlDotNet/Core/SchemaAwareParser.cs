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
using System.Collections.Generic;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Schemas;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Decorates an <see cref="IParser" /> to apply a schema to it.
    /// </summary>
    public sealed class SchemaAwareParser : IParser
    {
        private readonly IParser parser;
        private readonly NonSpecificTagResolver tagResolver;

        public SchemaAwareParser(IParser parser, ISchema schema)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.tagResolver = new NonSpecificTagResolver(schema);
        }

        public ParsingEvent? Current { get; private set; }

        private sealed class NonSpecificTagResolver : IParsingEventVisitor<ParsingEvent>
        {
            private readonly List<NodeEvent> currentPath = new List<NodeEvent>();
            private readonly ISchema schema;

            public NonSpecificTagResolver(ISchema schema)
            {
                this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            }

            private void PushPathSegment(NodeEvent segment)
            {
                this.currentPath.Add(segment);
            }

            private void PopPathSegment()
            {
                this.currentPath.RemoveAt(this.currentPath.Count - 1);
            }

            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(Scalar scalar)
            {
                var wasResolved = scalar.Tag.Name.IsNonSpecific
                    ? schema.ResolveNonSpecificTag(scalar, currentPath, out var resolvedTag)
                    : schema.ResolveSpecificTag(scalar.Tag.Name, out resolvedTag);

                if (wasResolved)
                {
                    scalar = new Scalar(scalar.Anchor, resolvedTag!, scalar.Value, scalar.Style, scalar.Start, scalar.End);
                }
                return scalar;
            }

            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(SequenceStart sequenceStart)
            {
                var wasResolved = sequenceStart.Tag.Name.IsNonSpecific
                    ? schema.ResolveNonSpecificTag(sequenceStart, currentPath, out var resolvedTag)
                    : schema.ResolveSpecificTag(sequenceStart.Tag.Name, out resolvedTag);

                if (wasResolved)
                {
                    sequenceStart = new SequenceStart(sequenceStart.Anchor, resolvedTag!, sequenceStart.Style, sequenceStart.Start, sequenceStart.End);
                }
                PushPathSegment(sequenceStart);
                return sequenceStart;
            }

            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(MappingStart mappingStart)
            {
                var wasResolved = mappingStart.Tag.Name.IsNonSpecific
                    ? schema.ResolveNonSpecificTag(mappingStart, currentPath, out var resolvedTag)
                    : schema.ResolveSpecificTag(mappingStart.Tag.Name, out resolvedTag);

                if (wasResolved)
                {
                    mappingStart = new MappingStart(mappingStart.Anchor, resolvedTag!, mappingStart.Style, mappingStart.Start, mappingStart.End);
                }
                PushPathSegment(mappingStart);
                return mappingStart;
            }
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(SequenceEnd sequenceEnd)
            {
                PopPathSegment();
                return sequenceEnd;
            }

            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(MappingEnd mappingEnd)
            {
                PopPathSegment();
                return mappingEnd;
            }

            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(AnchorAlias anchorAlias) => anchorAlias;
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(StreamStart streamStart) => streamStart;
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(StreamEnd streamEnd) => streamEnd;
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(DocumentStart documentStart) => documentStart;
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(DocumentEnd documentEnd) => documentEnd;
            ParsingEvent IParsingEventVisitor<ParsingEvent>.Visit(Comment comment) => comment;
        }

        public bool MoveNext()
        {
            if (this.parser.MoveNext())
            {
                this.Current = this.parser.Current!.Accept(this.tagResolver);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
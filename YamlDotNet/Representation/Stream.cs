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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Representation.Schemas;
using ScalarEvent = YamlDotNet.Core.Events.Scalar;

namespace YamlDotNet.Representation
{
    public sealed class Stream
    {
        public static IEnumerable<Document> Load(string yaml, Func<DocumentStart, ISchema> schemaSelector)
        {
            return Load(new Parser(new StringReader(yaml)), schemaSelector);
        }

        private static bool TryLoadDocument(IParser parser, Func<DocumentStart, ISchema> schemaSelector, [NotNullWhen(true)] out Document? document)
        {
            if (parser.TryConsume<DocumentStart>(out var documentStart))
            {
                var schema = schemaSelector(documentStart);

                var loader = new NodeLoader(parser);
                var node = loader.LoadNode(schema.Root, out _);

                parser.Consume<DocumentEnd>();
                document = new Document(node, schema);
                return true;
            }
            else
            {
                document = null;
                return false;
            }
        }

        private static IEnumerable<Document> LoadDocuments(IParser parser, Func<DocumentStart, ISchema> schemaSelector)
        {
            while (TryLoadDocument(parser, schemaSelector, out var document))
            {
                yield return document;
            }
            parser.Consume<StreamEnd>();
        }

        public static IEnumerable<Document> Load(IParser parser, Func<DocumentStart, ISchema> schemaSelector, bool stream = false)
        {
            var loadMultipleDocuments = parser.TryConsume<StreamStart>(out _);
            if (loadMultipleDocuments)
            {
                var documents = LoadDocuments(parser, schemaSelector);
                return stream ? documents : documents.ToList();
            }
            else
            {
                return TryLoadDocument(parser, schemaSelector, out var document)
                    ? new[] { document }
                    : Enumerable.Empty<Document>();
            }
        }

        public static string Dump(IEnumerable<Document> stream, bool explicitSeparators = false)
        {
            using var buffer = new StringWriter();
            Dump(new Emitter(buffer), stream, explicitSeparators);
            return buffer.ToString();
        }

        public static string Dump(Document document, bool explicitSeparators = false)
        {
            using var buffer = new StringWriter();
            Dump(new Emitter(buffer), document, explicitSeparators);
            return buffer.ToString();
        }

        public static void Dump(IEmitter emitter, params Document[] stream) => Dump(emitter, stream.AsEnumerable());

        public static void Dump(IEmitter emitter, IEnumerable<Document> stream, bool explicitSeparators = false)
        {
            emitter.Emit(new StreamStart());
            foreach (var document in stream)
            {
                EmitDocument(emitter, explicitSeparators, document);
            }
            emitter.Emit(new StreamEnd());
        }

        public static void Dump(IEmitter emitter, Document document, bool explicitSeparators = false)
        {
            emitter.Emit(new StreamStart());
            EmitDocument(emitter, explicitSeparators, document);
            emitter.Emit(new StreamEnd());
        }

        private static void EmitDocument(IEmitter emitter, bool explicitSeparators, Document document)
        {
            emitter.Emit(new DocumentStart(null, null, isImplicit: !explicitSeparators));

            var count = 0;
            var anchors = AssignAnchors(document.Content, _ => $"n{count++}");
            var dumper = new NodeDumper(emitter, anchors);
            dumper.Dump(document.Content, document.Schema.Root, out _);

            emitter.Emit(new DocumentEnd(isImplicit: !explicitSeparators));
        }

        private sealed class NodeLoader
        {
            private readonly Dictionary<AnchorName, Node> anchoredNodes = new Dictionary<AnchorName, Node>();
            private readonly IParser parser;

            public NodeLoader(IParser parser)
            {
                this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            }

            public Node LoadNode(ISchemaIterator iterator, out ISchemaIterator? childIterator)
            {
                if (parser.TryConsume<AnchorAlias>(out var anchorAlias))
                {
                    childIterator = null;
                    return LoadAlias(anchorAlias);
                }
                else if (parser.TryConsume<ScalarEvent>(out var scalar))
                {
                    childIterator = iterator.EnterNode(scalar, out var mapper);
                    return LoadScalar(scalar, mapper);
                }
                else if (parser.TryConsume<SequenceStart>(out var sequenceStart))
                {
                    childIterator = iterator.EnterNode(sequenceStart, out var mapper);
                    return LoadSequence(sequenceStart, childIterator, mapper);
                }
                else if (parser.TryConsume<MappingStart>(out var mappingStart))
                {
                    childIterator = iterator.EnterNode(mappingStart, out var mapper);
                    return LoadMapping(mappingStart, childIterator, mapper);
                }
                else
                {
                    throw new SemanticErrorException(parser.Current!.Start, parser.Current!.End, $"Found an unexpected event '{parser.Current!.Type}'.");
                }
            }

            private Node LoadAlias(AnchorAlias anchorAlias)
            {
                if (anchoredNodes.TryGetValue(anchorAlias.Value, out var node))
                {
                    return node;
                }
                throw new AnchorNotFoundException(anchorAlias.Start, anchorAlias.End, $"Anchor '{anchorAlias.Value}' not found.");
            }

            private Node LoadScalar(ScalarEvent scalar, INodeMapper mapper)
            {
                var node = new Scalar(mapper, scalar.Value, scalar.Start, scalar.End);
                AddAnchoredNode(scalar.Anchor, node);
                return node;
            }

            private Node LoadSequence(SequenceStart sequenceStart, ISchemaIterator iterator, INodeMapper mapper)
            {
                var items = new List<Node>();

                // Notice that the items collection will still be mutated after constructing the Sequence object.
                // We need to create it now in order to support recursive anchors.
                var sequence = new Sequence(mapper, items.AsReadonlyList(), sequenceStart.Start, sequenceStart.End);
                AddAnchoredNode(sequenceStart.Anchor, sequence);

                while (!parser.TryConsume<SequenceEnd>(out _))
                {
                    var child = LoadNode(iterator, out _);
                    items.Add(child);
                }

                return sequence;
            }

            private Node LoadMapping(MappingStart mappingStart, ISchemaIterator iterator, INodeMapper mapper)
            {
                var items = new Dictionary<Node, Node>();

                // Notice that the items collection will still be mutated after constructing the Sequence object.
                // We need to create it now in order to support recursive anchors.
                var mapping = new Mapping(mapper, items.AsReadonlyDictionary(), mappingStart.Start, mappingStart.End);
                AddAnchoredNode(mappingStart.Anchor, mapping);

                while (!parser.TryConsume<MappingEnd>(out _))
                {
                    var key = LoadNode(iterator, out var keyIterator);

                    // keyIterator is null when the node came from an alias.
                    // In that case, we need to enter it. This not done at LoadNode level because we don't always want that.
                    if (keyIterator == null)
                    {
                        keyIterator = iterator.EnterNode(key, out var _);
                    }

                    var value = LoadNode(keyIterator.EnterMappingValue(), out _);

                    items.Add(key, value);
                }

                return mapping;
            }

            private void AddAnchoredNode(AnchorName anchor, Node node)
            {
                if (!anchor.IsEmpty)
                {
                    // The anchor might already exist. In that case we want to replace it.
                    anchoredNodes[anchor] = node;
                }
            }
        }

        private static Dictionary<Node, AnchorName> AssignAnchors(Node root, Func<Node, AnchorName> assignAnchor)
        {
            var encounteredNodes = new HashSet<Node>(ReferenceEqualityComparer<Node>.Default);
            var assignedAnchors = new Dictionary<Node, AnchorName>(ReferenceEqualityComparer<Node>.Default);

            AssignAnchors(root);
            return assignedAnchors;

            void AssignAnchors(Node node)
            {
                if (node is Scalar)
                {
                    return;
                }

                if (encounteredNodes.Add(node))
                {
                    foreach (var child in node.Children)
                    {
                        AssignAnchors(child);
                    }
                }
                else if (!assignedAnchors.ContainsKey(node))
                {
                    assignedAnchors.Add(node, assignAnchor(node));
                }
            }
        }

        private sealed class NodeDumper
        {
            private readonly IEmitter emitter;
            private readonly Dictionary<Node, AnchorName> anchors;
            private readonly HashSet<Node> emittedAnchoredNodes = new HashSet<Node>(ReferenceEqualityComparer<Node>.Default);

            public NodeDumper(IEmitter emitter, Dictionary<Node, AnchorName> anchors)
            {
                this.emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
                this.anchors = anchors ?? throw new ArgumentNullException(nameof(anchors));
            }

            public void Dump(Node node, ISchemaIterator iterator, out ISchemaIterator childIterator)
            {
                switch (node)
                {
                    case Scalar scalar:
                        childIterator = iterator.EnterNode(scalar, out _);
                        Dump(scalar, childIterator);
                        break;

                    case Sequence sequence:
                        childIterator = iterator.EnterNode(sequence, out _);
                        Dump(sequence, childIterator);
                        break;

                    case Mapping mapping:
                        childIterator = iterator.EnterNode(mapping, out _);
                        Dump(mapping, childIterator);
                        break;

                    default:
                        throw Invariants.InvalidCase(node);
                }
            }

            private void Dump(Scalar scalar, ISchemaIterator iterator)
            {
                if (!TryEmitAlias(scalar, out var anchor))
                {
                    var tag = iterator.IsTagImplicit(scalar, out var style) ? TagName.Empty : scalar.Tag;

                    emitter.Emit(new ScalarEvent(anchor, tag, scalar.Value, style));
                }
            }

            private void Dump(Sequence sequence, ISchemaIterator iterator)
            {
                if (!TryEmitAlias(sequence, out var anchor))
                {
                    var tag = iterator.IsTagImplicit(sequence, out var style) ? TagName.Empty : sequence.Tag;

                    emitter.Emit(new SequenceStart(anchor, tag, style));
                    foreach (var item in sequence)
                    {
                        Dump(item, iterator, out _);
                    }
                    emitter.Emit(new SequenceEnd());
                }
            }

            private void Dump(Mapping mapping, ISchemaIterator iterator)
            {
                if (!TryEmitAlias(mapping, out var anchor))
                {
                    var tag = iterator.IsTagImplicit(mapping, out var style) ? TagName.Empty : mapping.Tag;

                    emitter.Emit(new MappingStart(anchor, tag, style));
                    foreach (var (key, value) in mapping)
                    {
                        Dump(key, iterator, out var keyIterator);
                        Dump(value, keyIterator.EnterMappingValue(), out _);
                    }
                    emitter.Emit(new MappingEnd());
                }
            }

            private bool TryEmitAlias(Node node, out AnchorName anchor)
            {
                if (anchors.TryGetValue(node, out anchor) && !emittedAnchoredNodes.Add(node))
                {
                    emitter.Emit(new AnchorAlias(anchor));
                    return true;
                }
                return false;
            }
        }
    }
}
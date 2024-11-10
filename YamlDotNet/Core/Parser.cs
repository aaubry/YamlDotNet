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
using System.Diagnostics;
using System.IO;
using YamlDotNet.Core.Tokens;
using MappingStyle = YamlDotNet.Core.Events.MappingStyle;
using ParsingEvent = YamlDotNet.Core.Events.ParsingEvent;
using SequenceStyle = YamlDotNet.Core.Events.SequenceStyle;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Parses YAML streams.
    /// </summary>
    public class Parser : IParser
    {
        private readonly Stack<ParserState> states = new Stack<ParserState>();
        private readonly TagDirectiveCollection tagDirectives = new ();
        private ParserState state;

        private readonly IScanner scanner;
        private Token? currentToken;
        private VersionDirective? version;

        private Token? GetCurrentToken()
        {
            if (currentToken == null)
            {
                while (scanner.MoveNextWithoutConsuming())
                {
                    currentToken = scanner.Current;

                    if (currentToken is Comment commentToken)
                    {
                        pendingEvents.Enqueue(new Events.Comment(commentToken.Value, commentToken.IsInline, commentToken.Start, commentToken.End));
                        scanner.ConsumeCurrent();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return currentToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="input">The input where the YAML stream is to be read.</param>
        public Parser(TextReader input)
            : this(new Scanner(input))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
        }

        /// <summary>
        /// Gets the current event.
        /// </summary>
        public ParsingEvent? Current { get; private set; }

        private readonly EventQueue pendingEvents = new EventQueue();

        /// <summary>
        /// Moves to the next event.
        /// </summary>
        /// <returns>Returns true if there are more events available, otherwise returns false.</returns>
        public bool MoveNext()
        {
            // No events after the end of the stream or error.
            if (state == ParserState.StreamEnd)
            {
                Current = null;
                return false;
            }
            else if (pendingEvents.Count == 0)
            {
                // Generate the next event.
                pendingEvents.Enqueue(StateMachine());
            }

            Current = pendingEvents.Dequeue();
            return true;
        }

        private ParsingEvent StateMachine()
        {
            switch (state)
            {
                case ParserState.StreamStart:
                    return ParseStreamStart();

                case ParserState.ImplicitDocumentStart:
                    return ParseDocumentStart(true);

                case ParserState.DocumentStart:
                    return ParseDocumentStart(false);

                case ParserState.DocumentContent:
                    return ParseDocumentContent();

                case ParserState.DocumentEnd:
                    return ParseDocumentEnd();

                case ParserState.BlockNode:
                    return ParseNode(true, false);

                case ParserState.BlockNodeOrIndentlessSequence:
                    return ParseNode(true, true);

                case ParserState.FlowNode:
                    return ParseNode(false, false);

                case ParserState.BlockSequenceFirstEntry:
                    return ParseBlockSequenceEntry(true);

                case ParserState.BlockSequenceEntry:
                    return ParseBlockSequenceEntry(false);

                case ParserState.IndentlessSequenceEntry:
                    return ParseIndentlessSequenceEntry();

                case ParserState.BlockMappingFirstKey:
                    return ParseBlockMappingKey(true);

                case ParserState.BlockMappingKey:
                    return ParseBlockMappingKey(false);

                case ParserState.BlockMappingValue:
                    return ParseBlockMappingValue();

                case ParserState.FlowSequenceFirstEntry:
                    return ParseFlowSequenceEntry(true);

                case ParserState.FlowSequenceEntry:
                    return ParseFlowSequenceEntry(false);

                case ParserState.FlowSequenceEntryMappingKey:
                    return ParseFlowSequenceEntryMappingKey();

                case ParserState.FlowSequenceEntryMappingValue:
                    return ParseFlowSequenceEntryMappingValue();

                case ParserState.FlowSequenceEntryMappingEnd:
                    return ParseFlowSequenceEntryMappingEnd();

                case ParserState.FlowMappingFirstKey:
                    return ParseFlowMappingKey(true);

                case ParserState.FlowMappingKey:
                    return ParseFlowMappingKey(false);

                case ParserState.FlowMappingValue:
                    return ParseFlowMappingValue(false);

                case ParserState.FlowMappingEmptyValue:
                    return ParseFlowMappingValue(true);

                default:
                    Debug.Assert(false, "Invalid state");      // Invalid state.
                    throw new InvalidOperationException();
            }
        }

        private void Skip()
        {
            if (currentToken != null)
            {
                currentToken = null;
                scanner.ConsumeCurrent();
            }
        }

        /// <summary>
        /// Parse the production:
        /// stream   ::= STREAM-START implicit_document? explicit_document* STREAM-END
        ///              ************
        /// </summary>
        private Events.StreamStart ParseStreamStart()
        {
            var current = GetCurrentToken();

            if (!(current is StreamStart streamStart))
            {
                throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "Did not find expected <stream-start>.");
            }
            Skip();

            state = ParserState.ImplicitDocumentStart;
            return new Events.StreamStart(streamStart.Start, streamStart.End);
        }

        /// <summary>
        /// Parse the productions:
        /// implicit_document    ::= block_node DOCUMENT-END*
        ///                          *
        /// explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
        ///                          *************************
        /// </summary>
        private ParsingEvent ParseDocumentStart(bool isImplicit)
        {
            if (currentToken is VersionDirective)
            {
                // EB22
                throw new SyntaxErrorException("While parsing a document start node, could not find document end marker before version directive.");
            }

            // Parse extra document end indicators.

            var current = GetCurrentToken();
            if (!isImplicit)
            {
                while (current is DocumentEnd)
                {
                    Skip();
                    current = GetCurrentToken();
                }
            }

            if (current == null)
            {
                throw new SyntaxErrorException("Reached the end of the stream while parsing a document start.");
            }

            if (current is Scalar && (state == ParserState.ImplicitDocumentStart || state == ParserState.DocumentStart))
            {
                isImplicit = true;
            }

            // Parse an isImplicit document.

            if (isImplicit && !(current is VersionDirective || current is TagDirective || current is DocumentStart || current is StreamEnd || current is DocumentEnd) || current is BlockMappingStart)
            {
                var directives = new TagDirectiveCollection();
                ProcessDirectives(directives);

                states.Push(ParserState.DocumentEnd);

                state = ParserState.BlockNode;

                return new Events.DocumentStart(null, directives, true, current.Start, current.End);
            }

            // Parse an explicit document.

            else if (!(current is StreamEnd || current is DocumentEnd))
            {
                var start = current.Start;
                var directives = new TagDirectiveCollection();
                var versionDirective = ProcessDirectives(directives);

                current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document start");

                if (!(current is DocumentStart))
                {
                    throw new SemanticErrorException(current.Start, current.End, "Did not find expected <document start>.");
                }

                states.Push(ParserState.DocumentEnd);

                state = ParserState.DocumentContent;

                var end = current.End;
                Skip();
                return new Events.DocumentStart(versionDirective, directives, false, start, end);
            }

            // Parse the stream end.

            else
            {
                if (current is DocumentEnd)
                {
                    Skip();
                }
                state = ParserState.StreamEnd;

                current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document start");

                var evt = new Events.StreamEnd(current.Start, current.End);
                // Do not call skip here because that would throw an exception
                if (scanner.MoveNextWithoutConsuming())
                {
                    throw new InvalidOperationException("The scanner should contain no more tokens.");
                }
                return evt;
            }
        }

        /// <summary>
        /// Parse directives.
        /// </summary>
        private VersionDirective? ProcessDirectives(TagDirectiveCollection tags)
        {
            var hasOwnDirectives = false;
            VersionDirective? localVersion = null;

            while (true)
            {
                if (GetCurrentToken() is VersionDirective currentVersion)
                {
                    if (version != null)
                    {
                        throw new SemanticErrorException(currentVersion.Start, currentVersion.End, "Found duplicate %YAML directive.");
                    }

                    if (currentVersion.Version.Major != Constants.MajorVersion || currentVersion.Version.Minor > Constants.MinorVersion)
                    {
                        throw new SemanticErrorException(currentVersion.Start, currentVersion.End, "Found incompatible YAML document.");
                    }

                    localVersion = version = currentVersion;
                    hasOwnDirectives = true;
                }
                else if (GetCurrentToken() is TagDirective tag)
                {
                    if (tags.Contains(tag.Handle))
                    {
                        throw new SemanticErrorException(tag.Start, tag.End, "Found duplicate %TAG directive.");
                    }
                    tags.Add(tag);
                    hasOwnDirectives = true;
                }

                // Starting from v1.2, it is not permitted to use tag shorthands for multiple documents in a stream.
                else if (GetCurrentToken() is DocumentStart && (version == null || (version.Version.Major == 1 && version.Version.Minor > 1)))
                {
                    if (GetCurrentToken() is DocumentStart && (version == null))
                    {
                        version = new VersionDirective(new Version(1, 2));
                    }

                    hasOwnDirectives = true;
                    break;
                }
                else
                {
                    break;
                }

                Skip();
            }

            AddTagDirectives(tags, Constants.DefaultTagDirectives);

            if (hasOwnDirectives)
            {
                tagDirectives.Clear();
            }

            AddTagDirectives(tagDirectives, tags);

            return localVersion;
        }

        private static void AddTagDirectives(TagDirectiveCollection directives, IEnumerable<TagDirective> source)
        {
            foreach (var directive in source)
            {
                if (!directives.Contains(directive))
                {
                    directives.Add(directive);
                }
            }
        }

        /// <summary>
        /// Parse the productions:
        /// explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
        ///                                                    ***********
        /// </summary>
        private ParsingEvent ParseDocumentContent()
        {
            if (
                GetCurrentToken() is VersionDirective ||
                GetCurrentToken() is TagDirective ||
                GetCurrentToken() is DocumentStart ||
                GetCurrentToken() is DocumentEnd ||
                GetCurrentToken() is StreamEnd
            )
            {
                state = states.Pop();
                return ProcessEmptyScalar(scanner.CurrentPosition);
            }
            else
            {
                return ParseNode(true, false);
            }
        }

        /// <summary>
        /// Generate an empty scalar event.
        /// </summary>
        private static Events.Scalar ProcessEmptyScalar(in Mark position)
        {
            return new Events.Scalar(AnchorName.Empty, TagName.Empty, string.Empty, ScalarStyle.Plain, true, false, position, position);
        }

        /// <summary>
        /// Parse the productions:
        /// block_node_or_indentless_sequence    ::=
        ///                          ALIAS
        ///                          *****
        ///                          | properties (block_content | indentless_block_sequence)?
        ///                            **********  *
        ///                          | block_content | indentless_block_sequence
        ///                            *
        /// block_node           ::= ALIAS
        ///                          *****
        ///                          | properties block_content?
        ///                            ********** *
        ///                          | block_content
        ///                            *
        /// flow_node            ::= ALIAS
        ///                          *****
        ///                          | properties flow_content?
        ///                            ********** *
        ///                          | flow_content
        ///                            *
        /// properties           ::= TAG ANCHOR? | ANCHOR TAG?
        ///                          *************************
        /// block_content        ::= block_collection | flow_collection | SCALAR
        ///                                                               ******
        /// flow_content         ::= flow_collection | SCALAR
        ///                                            ******
        /// </summary>
        private ParsingEvent ParseNode(bool isBlock, bool isIndentlessSequence)
        {
            if (GetCurrentToken() is Error errorToken)
            {
                throw new SemanticErrorException(errorToken.Start, errorToken.End, errorToken.Value);
            }

            var current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
            if (current is AnchorAlias alias)
            {
                state = states.Pop();
                ParsingEvent evt = new Events.AnchorAlias(alias.Value, alias.Start, alias.End);
                Skip();
                return evt;
            }

            var start = current.Start;

            var anchorName = AnchorName.Empty;
            var tagName = TagName.Empty;
            Anchor? lastAnchor = null;
            Tag? lastTag = null;

            // The anchor and the tag can be in any order. This loop repeats at most twice.
            while (true)
            {
                if (anchorName.IsEmpty && current is Anchor anchor)
                {
                    lastAnchor = anchor;
                    anchorName = anchor.Value;
                    Skip();
                }
                else if (tagName.IsEmpty && current is Tag tag)
                {
                    lastTag = tag;
                    if (string.IsNullOrEmpty(tag.Handle))
                    {
                        tagName = new TagName(tag.Suffix);
                    }
                    else if (tagDirectives.Contains(tag.Handle))
                    {
                        tagName = new TagName(string.Concat(tagDirectives[tag.Handle].Prefix, tag.Suffix));
                    }
                    else
                    {
                        throw new SemanticErrorException(tag.Start, tag.End, "While parsing a node, found undefined tag handle.");
                    }

                    Skip();
                }
                else if (current is Anchor secondAnchor)
                {
                    throw new SemanticErrorException(secondAnchor.Start, secondAnchor.End, "While parsing a node, found more than one anchor.");
                }
                else if (current is AnchorAlias anchorAlias)
                {
                    throw new SemanticErrorException(anchorAlias.Start, anchorAlias.End, "While parsing a node, did not find expected token.");
                }
                else if (current is Error error)
                {
                    if (lastTag != null && lastAnchor != null && !anchorName.IsEmpty)
                    {
                        return new Events.Scalar(anchorName, default, string.Empty, default, false, false, lastAnchor.Start, lastAnchor.End);
                    }
                    throw new SemanticErrorException(error.Start, error.End, error.Value);
                }
                else
                {
                    break;
                }

                current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
            }

            var isImplicit = tagName.IsEmpty;

            if (isIndentlessSequence && GetCurrentToken() is BlockEntry)
            {
                state = ParserState.IndentlessSequenceEntry;

                return new Events.SequenceStart(
                    anchorName,
                    tagName,
                    isImplicit,
                    SequenceStyle.Block,
                    start,
                    current.End
                );
            }
            else
            {
                if (current is Scalar scalar)
                {
                    var isPlainImplicit = false;
                    var isQuotedImplicit = false;
                    if ((scalar.Style == ScalarStyle.Plain && tagName.IsEmpty) || tagName.IsNonSpecific)
                    {
                        isPlainImplicit = true;
                    }
                    else if (tagName.IsEmpty)
                    {
                        isQuotedImplicit = true;
                    }

                    state = states.Pop();
                    Skip();

                    ParsingEvent evt = new Events.Scalar(anchorName, tagName, scalar.Value, scalar.Style, isPlainImplicit, isQuotedImplicit, start, scalar.End, scalar.IsKey);

                    // Read next token to ensure the error case spec test 'CXX2':
                    // "Mapping with anchor on document start line".

                    if (!anchorName.IsEmpty && scanner.MoveNextWithoutConsuming())
                    {
                        currentToken = scanner.Current;
                        if (currentToken is Error)
                        {
                            errorToken = (currentToken as Error)!;
                            throw new SemanticErrorException(errorToken.Start, errorToken.End, errorToken.Value);
                        }
                    }

                    // Read next token to ensure the error case spec test 'T833':

                    if (state == ParserState.FlowMappingKey && !(scanner.Current is FlowMappingEnd) && scanner.MoveNextWithoutConsuming())
                    {
                        currentToken = scanner.Current;
                        if (currentToken != null && !(currentToken is FlowEntry) && !(currentToken is FlowMappingEnd))
                        {
                            throw new SemanticErrorException(currentToken.Start, currentToken.End, "While parsing a flow mapping, did not find expected ',' or '}'.");
                        }
                    }

                    return evt;
                }

                if (current is FlowSequenceStart flowSequenceStart)
                {
                    state = ParserState.FlowSequenceFirstEntry;
                    return new Events.SequenceStart(anchorName, tagName, isImplicit, SequenceStyle.Flow, start, flowSequenceStart.End);
                }

                if (current is FlowMappingStart flowMappingStart)
                {
                    state = ParserState.FlowMappingFirstKey;
                    return new Events.MappingStart(anchorName, tagName, isImplicit, MappingStyle.Flow, start, flowMappingStart.End);
                }

                if (isBlock)
                {
                    if (current is BlockSequenceStart blockSequenceStart)
                    {
                        state = ParserState.BlockSequenceFirstEntry;
                        return new Events.SequenceStart(anchorName, tagName, isImplicit, SequenceStyle.Block, start, blockSequenceStart.End);
                    }

                    if (current is BlockMappingStart blockMappingStart)
                    {
                        state = ParserState.BlockMappingFirstKey;
                        return new Events.MappingStart(anchorName, tagName, isImplicit, MappingStyle.Block, start, blockMappingStart.End);
                    }
                }

                if (!anchorName.IsEmpty || !tagName.IsEmpty)
                {
                    state = states.Pop();
                    return new Events.Scalar(anchorName, tagName, string.Empty, ScalarStyle.Plain, isImplicit, false, start, current.End);
                }

                throw new SemanticErrorException(current.Start, current.End, "While parsing a node, did not find expected node content.");
            }
        }

        /// <summary>
        /// Parse the productions:
        /// implicit_document    ::= block_node DOCUMENT-END*
        ///                                     *************
        /// explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
        ///                                                                *************
        /// </summary>

        private Events.DocumentEnd ParseDocumentEnd()
        {
            var current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a document end");

            var isImplicit = true;
            var start = current.Start;
            var end = start;

            if (current is DocumentEnd)
            {
                end = current.End;
                Skip();
                isImplicit = false;
            }
            else if (!(currentToken is StreamEnd || currentToken is DocumentStart || currentToken is FlowSequenceEnd || currentToken is VersionDirective ||
                (Current is Events.Scalar && currentToken is Error)))
            {
                throw new SemanticErrorException(start, end, "Did not find expected <document end>.");
            }

            if (version != null && version.Version.Major == 1 && version.Version.Minor > 1)
            {
                version = null;
            }
            state = ParserState.DocumentStart;
            return new Events.DocumentEnd(isImplicit, start, end);
        }

        /// <summary>
        /// Parse the productions:
        /// block_sequence ::= BLOCK-SEQUENCE-START (BLOCK-ENTRY block_node?)* BLOCK-END
        ///                    ********************  *********** *             *********
        /// </summary>

        private ParsingEvent ParseBlockSequenceEntry(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            var current = GetCurrentToken();
            if (current is BlockEntry blockEntry)
            {
                var mark = blockEntry.End;

                Skip();
                current = GetCurrentToken();
                if (!(current is BlockEntry || current is BlockEnd))
                {
                    states.Push(ParserState.BlockSequenceEntry);
                    return ParseNode(true, false);
                }
                else
                {
                    state = ParserState.BlockSequenceEntry;
                    return ProcessEmptyScalar(mark);
                }
            }
            else if (current is BlockEnd blockEnd)
            {
                state = states.Pop();
                ParsingEvent evt = new Events.SequenceEnd(blockEnd.Start, blockEnd.End);
                Skip();
                return evt;
            }
            else
            {
                throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "While parsing a block collection, did not find expected '-' indicator.");
            }
        }

        /// <summary>
        /// Parse the productions:
        /// indentless_sequence  ::= (BLOCK-ENTRY block_node?)+
        ///                           *********** *
        /// </summary>
        private ParsingEvent ParseIndentlessSequenceEntry()
        {
            var current = GetCurrentToken();
            if (current is BlockEntry blockEntry)
            {
                var mark = blockEntry.End;
                Skip();

                current = GetCurrentToken();
                if (!(current is BlockEntry || current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.IndentlessSequenceEntry);
                    return ParseNode(true, false);
                }
                else
                {
                    state = ParserState.IndentlessSequenceEntry;
                    return ProcessEmptyScalar(mark);
                }
            }
            else
            {
                state = states.Pop();
                return new Events.SequenceEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
            }
        }

        /// <summary>
        /// Parse the productions:
        /// block_mapping        ::= BLOCK-MAPPING_START
        ///                          *******************
        ///                          ((KEY block_node_or_indentless_sequence?)?
        ///                            *** *
        ///                          (VALUE block_node_or_indentless_sequence?)?)*
        ///
        ///                          BLOCK-END
        ///                          *********
        /// </summary>
        private ParsingEvent ParseBlockMappingKey(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            var current = GetCurrentToken();
            if (current is Key key)
            {
                var mark = key.End;
                Skip();
                current = GetCurrentToken();
                if (!(current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.BlockMappingValue);
                    return ParseNode(true, true);
                }
                else
                {
                    state = ParserState.BlockMappingValue;
                    return ProcessEmptyScalar(mark);
                }
            }

            else if (current is Value value)
            {
                Skip();
                return ProcessEmptyScalar(value.End);
            }

            else if (current is AnchorAlias anchorAlias)
            {
                Skip();
                return new Events.AnchorAlias(anchorAlias.Value, anchorAlias.Start, anchorAlias.End);
            }

            else if (current is BlockEnd blockEnd)
            {
                state = states.Pop();
                ParsingEvent evt = new Events.MappingEnd(blockEnd.Start, blockEnd.End);
                Skip();
                return evt;
            }

            else if (GetCurrentToken() is Error error)
            {
                throw new SyntaxErrorException(error.Start, error.End, error.Value);
            }

            else
            {
                throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "While parsing a block mapping, did not find expected key.");
            }
        }

        /// <summary>
        /// Parse the productions:
        /// block_mapping        ::= BLOCK-MAPPING_START
        ///
        ///                          ((KEY block_node_or_indentless_sequence?)?
        ///
        ///                          (VALUE block_node_or_indentless_sequence?)?)*
        ///                           ***** *
        ///                          BLOCK-END
        ///
        /// </summary>
        private ParsingEvent ParseBlockMappingValue()
        {
            var current = GetCurrentToken();
            if (current is Value value)
            {
                var mark = value.End;
                Skip();

                current = GetCurrentToken();
                if (!(current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.BlockMappingKey);
                    return ParseNode(true, true);
                }
                else
                {
                    state = ParserState.BlockMappingKey;
                    return ProcessEmptyScalar(mark);
                }
            }
            else if (current is Error error)
            {
                throw new SemanticErrorException(error.Start, error.End, error.Value);
            }
            else
            {
                state = ParserState.BlockMappingKey;
                return ProcessEmptyScalar(current?.Start ?? Mark.Empty);
            }
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence        ::= FLOW-SEQUENCE-START
        ///                          *******************
        ///                          (flow_sequence_entry FLOW-ENTRY)*
        ///                           *                   **********
        ///                          flow_sequence_entry?
        ///                          *
        ///                          FLOW-SEQUENCE-END
        ///                          *****************
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                          *
        /// </summary>
        private ParsingEvent ParseFlowSequenceEntry(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            ParsingEvent evt;
            var current = GetCurrentToken();
            if (!(current is FlowSequenceEnd))
            {
                if (!isFirst)
                {
                    if (current is FlowEntry)
                    {
                        Skip();
                        current = GetCurrentToken();
                    }
                    else
                    {
                        throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "While parsing a flow sequence, did not find expected ',' or ']'.");
                    }
                }

                if (current is Key)
                {
                    state = ParserState.FlowSequenceEntryMappingKey;
                    evt = new Events.MappingStart(AnchorName.Empty, TagName.Empty, true, MappingStyle.Flow);
                    Skip();
                    return evt;
                }
                else if (!(current is FlowSequenceEnd))
                {
                    states.Push(ParserState.FlowSequenceEntry);
                    return ParseNode(false, false);
                }
            }

            state = states.Pop();
            evt = new Events.SequenceEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
            Skip();
            return evt;
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                      *** *
        /// </summary>
        private ParsingEvent ParseFlowSequenceEntryMappingKey()
        {
            var current = GetCurrentToken();
            if (!(current is Value || current is FlowEntry || current is FlowSequenceEnd))
            {
                states.Push(ParserState.FlowSequenceEntryMappingValue);
                return ParseNode(false, false);
            }
            else
            {
                var mark = current?.End ?? Mark.Empty;
                Skip();
                state = ParserState.FlowSequenceEntryMappingValue;
                return ProcessEmptyScalar(mark);
            }
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                                      ***** *
        /// </summary>
        private ParsingEvent ParseFlowSequenceEntryMappingValue()
        {
            var current = GetCurrentToken();
            if (current is Value)
            {
                Skip();
                current = GetCurrentToken();
                if (!(current is FlowEntry || current is FlowSequenceEnd))
                {
                    states.Push(ParserState.FlowSequenceEntryMappingEnd);
                    return ParseNode(false, false);
                }
            }
            state = ParserState.FlowSequenceEntryMappingEnd;
            return ProcessEmptyScalar(current?.Start ?? Mark.Empty);
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                                                      *
        /// </summary>
        private Events.MappingEnd ParseFlowSequenceEntryMappingEnd()
        {
            state = ParserState.FlowSequenceEntry;
            var current = GetCurrentToken();
            return new Events.MappingEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
        }

        /// <summary>
        /// Parse the productions:
        /// flow_mapping         ::= FLOW-MAPPING-START
        ///                          ******************
        ///                          (flow_mapping_entry FLOW-ENTRY)*
        ///                           *                  **********
        ///                          flow_mapping_entry?
        ///                          ******************
        ///                          FLOW-MAPPING-END
        ///                          ****************
        /// flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                          *           *** *
        /// </summary>
        private ParsingEvent ParseFlowMappingKey(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            var current = GetCurrentToken();
            if (!(current is FlowMappingEnd))
            {
                if (!isFirst)
                {
                    if (current is FlowEntry)
                    {
                        Skip();
                        current = GetCurrentToken();
                    }
                    else if (!(current is Scalar))
                    {
                        throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "While parsing a flow mapping,  did not find expected ',' or '}'.");
                    }
                }

                if (current is Key)
                {
                    Skip();

                    current = GetCurrentToken();
                    if (!(current is Value || current is FlowEntry || current is FlowMappingEnd))
                    {
                        states.Push(ParserState.FlowMappingValue);
                        return ParseNode(false, false);
                    }
                    else
                    {
                        state = ParserState.FlowMappingValue;
                        return ProcessEmptyScalar(current?.Start ?? Mark.Empty);
                    }
                }
                else if (current is Scalar)
                {
                    states.Push(ParserState.FlowMappingValue);
                    return ParseNode(false, false);
                }
                else if (!(current is FlowMappingEnd))
                {
                    states.Push(ParserState.FlowMappingEmptyValue);
                    return ParseNode(false, false);
                }
            }

            state = states.Pop();
            Skip();
            return new Events.MappingEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
        }

        /// <summary>
        /// Parse the productions:
        /// flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                   *                  ***** *
        /// </summary>
        private ParsingEvent ParseFlowMappingValue(bool isEmpty)
        {
            var current = GetCurrentToken();
            if (!isEmpty && current is Value)
            {
                Skip();
                current = GetCurrentToken();
                if (!(current is FlowEntry || current is FlowMappingEnd))
                {
                    states.Push(ParserState.FlowMappingKey);
                    return ParseNode(false, false);
                }
            }

            state = ParserState.FlowMappingKey;

            if (!isEmpty && current is Scalar scalar)
            {
                Skip();
                return new Events.Scalar(AnchorName.Empty, TagName.Empty, scalar.Value, scalar.Style, false, false, current.Start, scalar.End);
            }

            return ProcessEmptyScalar(current?.Start ?? Mark.Empty);
        }

        private class EventQueue
        {
            // This class is specialized for our specific use case where there are exactly two priority levels.
            // If more levels are required, a more generic implementation should be used instead.
            private readonly Queue<ParsingEvent> highPriorityEvents = new Queue<ParsingEvent>();
            private readonly Queue<ParsingEvent> normalPriorityEvents = new Queue<ParsingEvent>();

            public void Enqueue(ParsingEvent @event)
            {
                switch (@event.Type)
                {
                    case Events.EventType.StreamStart:
                    case Events.EventType.DocumentStart:
                        highPriorityEvents.Enqueue(@event);
                        break;

                    default:
                        normalPriorityEvents.Enqueue(@event);
                        break;
                }
            }

            public ParsingEvent Dequeue()
            {
                return highPriorityEvents.Count > 0
                    ? highPriorityEvents.Dequeue()
                    : normalPriorityEvents.Dequeue();
            }

            public int Count
            {
                get
                {
                    return highPriorityEvents.Count + normalPriorityEvents.Count;
                }
            }
        }
    }
}

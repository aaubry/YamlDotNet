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
using System.Diagnostics;
using System.IO;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core
{
    public interface IYamlLoader
    {
        void OnStreamStart(Mark start, Mark end);
        void OnStreamEnd(Mark start, Mark end);

        void OnDocumentStart(VersionDirective? version, TagDirectiveCollection? tags, bool isImplicit, Mark start, Mark end);
        void OnDocumentEnd(bool isImplicit, Mark start, Mark end);

        void OnSequenceStart(AnchorName anchor, TagName tag, SequenceStyle style, Mark start, Mark end);
        void OnSequenceEnd(Mark start, Mark end);

        void OnMappingStart(AnchorName anchor, TagName tag, MappingStyle style, Mark start, Mark end);
        void OnMappingEnd(Mark start, Mark end);

        void OnScalar(AnchorName anchor, TagName tag, string value, ScalarStyle style, Mark start, Mark end);

        void OnAlias(AnchorName value, Mark start, Mark end);

        void OnComment(string value, bool isInline, Mark start, Mark end);
    }

    /// <summary>
    /// Parses YAML streams.
    /// </summary>
    public sealed class Parser2
    {
        private readonly Stack<ParserState> states = new Stack<ParserState>();
        private readonly TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
        private ParserState state;

        private readonly IScanner scanner;
        private readonly IYamlLoader loader;
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
                        loader.OnComment(commentToken.Value, commentToken.IsInline, commentToken.Start, commentToken.End);
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
        public Parser2(TextReader input, IYamlLoader loader)
            : this(new Scanner(input), loader)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        public Parser2(IScanner scanner, IYamlLoader loader)
        {
            this.scanner = scanner;
            this.loader = loader;
        }

        public void Load()
        {
            while (state != ParserState.StreamEnd)
            {
                StateMachine();
            }
        }

        private void StateMachine()
        {
            switch (state)
            {
                case ParserState.StreamStart:
                    ParseStreamStart();
                    break;

                case ParserState.ImplicitDocumentStart:
                    ParseDocumentStart(true);
                    break;

                case ParserState.DocumentStart:
                    ParseDocumentStart(false);
                    break;

                case ParserState.DocumentContent:
                    ParseDocumentContent();
                    break;

                case ParserState.DocumentEnd:
                    ParseDocumentEnd();
                    break;

                case ParserState.BlockNode:
                    ParseNode(true, false);
                    break;

                case ParserState.BlockNodeOrIndentlessSequence:
                    ParseNode(true, true);
                    break;

                case ParserState.FlowNode:
                    ParseNode(false, false);
                    break;

                case ParserState.BlockSequenceFirstEntry:
                    ParseBlockSequenceEntry(true);
                    break;

                case ParserState.BlockSequenceEntry:
                    ParseBlockSequenceEntry(false);
                    break;

                case ParserState.IndentlessSequenceEntry:
                    ParseIndentlessSequenceEntry();
                    break;

                case ParserState.BlockMappingFirstKey:
                    ParseBlockMappingKey(true);
                    break;

                case ParserState.BlockMappingKey:
                    ParseBlockMappingKey(false);
                    break;

                case ParserState.BlockMappingValue:
                    ParseBlockMappingValue();
                    break;

                case ParserState.FlowSequenceFirstEntry:
                    ParseFlowSequenceEntry(true);
                    break;

                case ParserState.FlowSequenceEntry:
                    ParseFlowSequenceEntry(false);
                    break;

                case ParserState.FlowSequenceEntryMappingKey:
                    ParseFlowSequenceEntryMappingKey();
                    break;

                case ParserState.FlowSequenceEntryMappingValue:
                    ParseFlowSequenceEntryMappingValue();
                    break;

                case ParserState.FlowSequenceEntryMappingEnd:
                    ParseFlowSequenceEntryMappingEnd();
                    break;

                case ParserState.FlowMappingFirstKey:
                    ParseFlowMappingKey(true);
                    break;

                case ParserState.FlowMappingKey:
                    ParseFlowMappingKey(false);
                    break;

                case ParserState.FlowMappingValue:
                    ParseFlowMappingValue(false);
                    break;

                case ParserState.FlowMappingEmptyValue:
                    ParseFlowMappingValue(true);
                    break;

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
        private void ParseStreamStart()
        {
            var current = GetCurrentToken();

            if (!(current is StreamStart streamStart))
            {
                throw new SemanticErrorException(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty, "Did not find expected <stream-start>.");
            }
            Skip();

            state = ParserState.ImplicitDocumentStart;
            loader.OnStreamStart(streamStart.Start, streamStart.End);
        }

        /// <summary>
        /// Parse the productions:
        /// implicit_document    ::= block_node DOCUMENT-END*
        ///                          *
        /// explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
        ///                          *************************
        /// </summary>
        private void ParseDocumentStart(bool isImplicit)
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

                loader.OnDocumentStart(null, directives, true, current.Start, current.End);
                return;
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
                loader.OnDocumentStart(versionDirective, directives, false, start, end);
                return;
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

                loader.OnStreamEnd(current.Start, current.End);
                // Do not call skip here because that would throw an exception
                if (scanner.MoveNextWithoutConsuming())
                {
                    throw new InvalidOperationException("The scanner should contain no more tokens.");
                }
            }
        }

        /// <summary>
        /// Parse directives.
        /// </summary>
        private VersionDirective? ProcessDirectives(TagDirectiveCollection tags)
        {
            bool hasOwnDirectives = false;
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
        private void ParseDocumentContent()
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
                ProcessEmptyScalar(scanner.CurrentPosition);
            }
            else
            {
                ParseNode(true, false);
            }
        }

        /// <summary>
        /// Generate an empty scalar event.
        /// </summary>
        private void ProcessEmptyScalar(Mark position)
        {
            loader.OnScalar(AnchorName.Empty, TagName.Empty, string.Empty, ScalarStyle.Plain, position, position);
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
        private void ParseNode(bool isBlock, bool isIndentlessSequence)
        {
            if (GetCurrentToken() is Error errorToken)
            {
                throw new SemanticErrorException(errorToken.Start, errorToken.End, errorToken.Value);
            }

            var current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
            if (current is AnchorAlias alias)
            {
                state = states.Pop();
                loader.OnAlias(alias.Value, alias.Start, alias.End);
                Skip();
                return;
            }

            var start = current.Start;

            var anchorName = AnchorName.Empty;
            TagName tag = TagName.Empty;
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
                else if (tag.IsEmpty && current is Tag tagToken)
                {
                    lastTag = tagToken;
                    if (string.IsNullOrEmpty(tagToken.Handle))
                    {
                        tag = new TagName(tagToken.Suffix);
                    }
                    else if (tagDirectives.Contains(tagToken.Handle))
                    {
                        tag = new TagName(string.Concat(tagDirectives[tagToken.Handle].Prefix, tagToken.Suffix));
                    }
                    else
                    {
                        throw new SemanticErrorException(tagToken.Start, tagToken.End, "While parsing a node, found undefined tag handle.");
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
                        loader.OnScalar(anchorName, TagName.Empty, string.Empty, ScalarStyle.Plain, lastAnchor.Start, lastAnchor.End);
                        return;
                    }
                    throw new SemanticErrorException(error.Start, error.End, error.Value);
                }
                else
                {
                    break;
                }

                current = GetCurrentToken() ?? throw new SemanticErrorException("Reached the end of the stream while parsing a node");
            }

            if (isIndentlessSequence && GetCurrentToken() is BlockEntry)
            {
                state = ParserState.IndentlessSequenceEntry;

                loader.OnSequenceStart(
                    anchorName,
                    tag,
                    SequenceStyle.Block,
                    start,
                    current.End
                );
            }
            else
            {
                if (current is Scalar scalar)
                {
                    state = states.Pop();
                    Skip();

                    if (tag.IsEmpty && scalar.Style != ScalarStyle.Plain)
                    {
                        tag = TagName.NonSpecific;
                    }

                    loader.OnScalar(anchorName, tag, scalar.Value, scalar.Style, start, scalar.End);

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
                    // "Flow mapping missing a separating comma".

                    if (state == ParserState.FlowMappingKey && scanner.MoveNextWithoutConsuming())
                    {
                        currentToken = scanner.Current;
                        if (currentToken != null && !(currentToken is FlowEntry) && !(currentToken is FlowMappingEnd))
                        {
                            throw new SemanticErrorException(currentToken.Start, currentToken.End, "While parsing a flow mapping, did not find expected ',' or '}'.");
                        }
                    }

                    return;
                }

                if (current is FlowSequenceStart flowSequenceStart)
                {
                    state = ParserState.FlowSequenceFirstEntry;
                    loader.OnSequenceStart(anchorName, tag, SequenceStyle.Flow, start, flowSequenceStart.End);
                    return;
                }

                if (current is FlowMappingStart flowMappingStart)
                {
                    state = ParserState.FlowMappingFirstKey;
                    loader.OnMappingStart(anchorName, tag, MappingStyle.Flow, start, flowMappingStart.End);
                    return;
                }

                if (isBlock)
                {
                    if (current is BlockSequenceStart blockSequenceStart)
                    {
                        state = ParserState.BlockSequenceFirstEntry;
                        loader.OnSequenceStart(anchorName, tag, SequenceStyle.Block, start, blockSequenceStart.End);
                        return;
                    }

                    if (current is BlockMappingStart blockMappingStart)
                    {
                        state = ParserState.BlockMappingFirstKey;
                        loader.OnMappingStart(anchorName, tag, MappingStyle.Block, start, blockMappingStart.End);
                        return;
                    }
                }

                if (!anchorName.IsEmpty || !tag.IsEmpty)
                {
                    state = states.Pop();
                    loader.OnScalar(anchorName, tag, string.Empty, ScalarStyle.Plain, start, current.End);
                    return;
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

        private void ParseDocumentEnd()
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
                (/*Current is Events.Scalar &&*/ currentToken is Error)))
            {
                throw new SemanticErrorException(start, end, "Did not find expected <document end>.");
            }

            if (version != null && version.Version.Major == 1 && version.Version.Minor > 1)
            {
                version = null;
            }
            state = ParserState.DocumentStart;
            loader.OnDocumentEnd(isImplicit, start, end);
        }

        /// <summary>
        /// Parse the productions:
        /// block_sequence ::= BLOCK-SEQUENCE-START (BLOCK-ENTRY block_node?)* BLOCK-END
        ///                    ********************  *********** *             *********
        /// </summary>

        private void ParseBlockSequenceEntry(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            var current = GetCurrentToken();
            if (current is BlockEntry blockEntry)
            {
                Mark mark = blockEntry.End;

                Skip();
                current = GetCurrentToken();
                if (!(current is BlockEntry || current is BlockEnd))
                {
                    states.Push(ParserState.BlockSequenceEntry);
                    ParseNode(true, false);
                }
                else
                {
                    state = ParserState.BlockSequenceEntry;
                    ProcessEmptyScalar(mark);
                }
            }
            else if (current is BlockEnd blockEnd)
            {
                state = states.Pop();
                loader.OnSequenceEnd(blockEnd.Start, blockEnd.End);
                Skip();
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
        private void ParseIndentlessSequenceEntry()
        {
            var current = GetCurrentToken();
            if (current is BlockEntry blockEntry)
            {
                Mark mark = blockEntry.End;
                Skip();

                current = GetCurrentToken();
                if (!(current is BlockEntry || current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.IndentlessSequenceEntry);
                    ParseNode(true, false);
                }
                else
                {
                    state = ParserState.IndentlessSequenceEntry;
                    ProcessEmptyScalar(mark);
                }
            }
            else
            {
                state = states.Pop();
                loader.OnSequenceEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
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
        private void ParseBlockMappingKey(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

            var current = GetCurrentToken();
            if (current is Key key)
            {
                Mark mark = key.End;
                Skip();
                current = GetCurrentToken();
                if (!(current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.BlockMappingValue);
                    ParseNode(true, true);
                }
                else
                {
                    state = ParserState.BlockMappingValue;
                    ProcessEmptyScalar(mark);
                }
            }

            else if (current is Value value)
            {
                Skip();
                ProcessEmptyScalar(value.End);
            }

            else if (current is AnchorAlias anchorAlias)
            {
                Skip();
                loader.OnAlias(anchorAlias.Value, anchorAlias.Start, anchorAlias.End);
            }

            else if (current is BlockEnd blockEnd)
            {
                state = states.Pop();
                loader.OnMappingEnd(blockEnd.Start, blockEnd.End);
                Skip();
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
        private void ParseBlockMappingValue()
        {
            var current = GetCurrentToken();
            if (current is Value value)
            {
                Mark mark = value.End;
                Skip();

                current = GetCurrentToken();
                if (!(current is Key || current is Value || current is BlockEnd))
                {
                    states.Push(ParserState.BlockMappingKey);
                    ParseNode(true, true);
                }
                else
                {
                    state = ParserState.BlockMappingKey;
                    ProcessEmptyScalar(mark);
                }
            }

            else if (current is Error error)
            {
                throw new SemanticErrorException(error.Start, error.End, error.Value);
            }

            else
            {
                state = ParserState.BlockMappingKey;
                ProcessEmptyScalar(current?.Start ?? Mark.Empty);
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
        private void ParseFlowSequenceEntry(bool isFirst)
        {
            if (isFirst)
            {
                GetCurrentToken();
                Skip();
            }

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

                if (current is Key key)
                {
                    state = ParserState.FlowSequenceEntryMappingKey;
                    loader.OnMappingStart(AnchorName.Empty, TagName.Empty, MappingStyle.Flow, key.Start ?? Mark.Empty, key.End ?? Mark.Empty);
                    Skip();
                    return;
                }
                else if (!(current is FlowSequenceEnd))
                {
                    states.Push(ParserState.FlowSequenceEntry);
                    ParseNode(false, false);
                    return;
                }
            }

            state = states.Pop();
            loader.OnSequenceEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
            Skip();
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                      *** *
        /// </summary>
        private void ParseFlowSequenceEntryMappingKey()
        {
            var current = GetCurrentToken();
            if (!(current is Value || current is FlowEntry || current is FlowSequenceEnd))
            {
                states.Push(ParserState.FlowSequenceEntryMappingValue);
                ParseNode(false, false);
            }
            else
            {
                Mark mark = current?.End ?? Mark.Empty;
                Skip();
                state = ParserState.FlowSequenceEntryMappingValue;
                ProcessEmptyScalar(mark);
            }
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                                      ***** *
        /// </summary>
        private void ParseFlowSequenceEntryMappingValue()
        {
            var current = GetCurrentToken();
            if (current is Value)
            {
                Skip();
                current = GetCurrentToken();
                if (!(current is FlowEntry || current is FlowSequenceEnd))
                {
                    states.Push(ParserState.FlowSequenceEntryMappingEnd);
                    ParseNode(false, false);
                    return;
                }
            }
            state = ParserState.FlowSequenceEntryMappingEnd;
            ProcessEmptyScalar(current?.Start ?? Mark.Empty);
        }

        /// <summary>
        /// Parse the productions:
        /// flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                                                      *
        /// </summary>
        private void ParseFlowSequenceEntryMappingEnd()
        {
            state = ParserState.FlowSequenceEntry;
            var current = GetCurrentToken();
            loader.OnMappingEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
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
        private void ParseFlowMappingKey(bool isFirst)
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
                    else
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
                        ParseNode(false, false);
                        return;
                    }
                    else
                    {
                        state = ParserState.FlowMappingValue;
                        ProcessEmptyScalar(current?.Start ?? Mark.Empty);
                        return;
                    }
                }
                else if (current is Scalar)
                {
                    states.Push(ParserState.FlowMappingValue);
                    ParseNode(false, false);
                    return;
                }
                else if (!(current is FlowMappingEnd))
                {
                    states.Push(ParserState.FlowMappingEmptyValue);
                    ParseNode(false, false);
                    return;
                }
            }

            state = states.Pop();
            Skip();
            loader.OnMappingEnd(current?.Start ?? Mark.Empty, current?.End ?? Mark.Empty);
        }

        /// <summary>
        /// Parse the productions:
        /// flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
        ///                                   *                  ***** *
        /// </summary>
        private void ParseFlowMappingValue(bool isEmpty)
        {
            var current = GetCurrentToken();
            if (isEmpty)
            {
                state = ParserState.FlowMappingKey;
                ProcessEmptyScalar(current?.Start ?? Mark.Empty);
                return;
            }

            if (current is Value)
            {
                Skip();
                current = GetCurrentToken();
                if (!(current is FlowEntry || current is FlowMappingEnd))
                {
                    states.Push(ParserState.FlowMappingKey);
                    ParseNode(false, false);
                    return;
                }
            }

            state = ParserState.FlowMappingKey;
            ProcessEmptyScalar(current?.Start ?? Mark.Empty);
        }
    }
}

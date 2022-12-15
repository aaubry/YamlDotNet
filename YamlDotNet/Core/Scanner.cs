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
using System.Text;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Helpers;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Converts a sequence of characters into a sequence of YAML tokens.
    /// </summary>
    public class Scanner : IScanner
    {
        private const int MaxVersionNumberLength = 9;

        private static readonly SortedDictionary<char, char> SimpleEscapeCodes = new SortedDictionary<char, char>
        {
            { '0', '\0' },
            { 'a', '\x07' },
            { 'b', '\x08' },
            { 't', '\x09' },
            { '\t', '\x09' },
            { 'n', '\x0A' },
            { 'v', '\x0B' },
            { 'f', '\x0C' },
            { 'r', '\x0D' },
            { 'e', '\x1B' },
            { ' ', '\x20' },
            { '"', '"' },
            { '\\', '\\' },
            { '/', '/' },
            { 'N', '\x85' },
            { '_', '\xA0' },
            { 'L', '\x2028' },
            { 'P', '\x2029' }
        };

        private readonly Stack<int> indents = new Stack<int>();
        private readonly InsertionQueue<Token> tokens = new InsertionQueue<Token>();
        private readonly Stack<SimpleKey> simpleKeys = new Stack<SimpleKey>();
        private readonly CharacterAnalyzer<LookAheadBuffer> analyzer;

        private readonly Cursor cursor;
        private bool streamStartProduced;
        private bool streamEndProduced;
        private bool plainScalarFollowedByComment;
        private int flowSequenceStartLine;
        private bool flowCollectionFetched = false;
        private bool startFlowCollectionFetched = false;
        private int indent = -1;
        private bool flowScalarFetched;
        private bool simpleKeyAllowed;
        private int flowLevel;
        private int tokensParsed;
        private bool tokenAvailable;
        private Token? previous;
        private Anchor? previousAnchor;
        private Scalar? lastScalar = null;

        private bool IsDocumentStart() =>
            !analyzer.EndOfInput &&
            cursor.LineOffset == 0 &&
            analyzer.Check('-', 0) &&
            analyzer.Check('-', 1) &&
            analyzer.Check('-', 2) &&
            analyzer.IsWhiteBreakOrZero(3);

        private bool IsDocumentEnd() =>
            !analyzer.EndOfInput &&
            cursor.LineOffset == 0 &&
            analyzer.Check('.', 0) &&
            analyzer.Check('.', 1) &&
            analyzer.Check('.', 2) &&
            analyzer.IsWhiteBreakOrZero(3);

        private bool IsDocumentIndicator() => IsDocumentStart() || IsDocumentEnd();

        public bool SkipComments
        {
            get; private set;
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public Token? Current
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scanner"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="skipComments">Indicates whether comments should be ignored</param>
        public Scanner(TextReader input, bool skipComments = true)
        {
            analyzer = new CharacterAnalyzer<LookAheadBuffer>(new LookAheadBuffer(input, 1024));
            cursor = new Cursor();
            SkipComments = skipComments;
        }

        /// <summary>
        /// Gets the current position inside the input stream.
        /// </summary>
        /// <value>The current position.</value>
        public Mark CurrentPosition
        {
            get
            {
                return cursor.Mark();
            }
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (Current != null)
            {
                ConsumeCurrent();
            }

            return MoveNextWithoutConsuming();
        }

        public bool MoveNextWithoutConsuming()
        {
            if (!tokenAvailable && !streamEndProduced)
            {
                FetchMoreTokens();
            }
            if (tokens.Count > 0)
            {
                Current = tokens.Dequeue();
                tokenAvailable = false;
                return true;
            }
            else
            {
                Current = null;
                return false;
            }
        }

        /// <summary>
        /// Consumes the current token and increments the parsed token count
        /// </summary>
        public void ConsumeCurrent()
        {
            ++tokensParsed;
            tokenAvailable = false;
            previous = Current;
            Current = null;
        }

        private char ReadCurrentCharacter()
        {
            var currentCharacter = analyzer.Peek(0);
            Skip();
            return currentCharacter;
        }

        private char ReadLine()
        {
            if (analyzer.Check("\r\n\x85")) // CR LF -> LF  --- CR|LF|NEL -> LF
            {
                SkipLine();
                return '\n';
            }

            var nextChar = analyzer.Peek(0); // LS|PS -> LS|PS
            SkipLine();
            return nextChar;
        }

        private void FetchMoreTokens()
        {
            // While we need more tokens to fetch, do it.

            while (true)
            {
                // Check if we really need to fetch more tokens.

                var needsMoreTokens = false;

                if (tokens.Count == 0)
                {
                    // Queue is empty.

                    needsMoreTokens = true;
                }
                else
                {
                    // Check if any potential simple key may occupy the head position.

                    foreach (var simpleKey in simpleKeys)
                    {
                        if (simpleKey.IsPossible && simpleKey.TokenNumber == tokensParsed)
                        {
                            needsMoreTokens = true;
                            break;
                        }
                    }
                }

                // We are finished.
                if (!needsMoreTokens)
                {
                    break;
                }

                // Fetch the next token.

                FetchNextToken();
            }
            tokenAvailable = true;
        }

        private static bool StartsWith(StringBuilder what, char start)
        {
            return what.Length > 0 && what[0] == start;
        }

        /// <summary>
        /// Check the list of potential simple keys and remove the positions that
        /// cannot contain simple keys anymore.
        /// </summary>

        private void StaleSimpleKeys()
        {
            // Check for a potential simple key for each flow level.

            foreach (var key in simpleKeys)
            {

                // The specification requires that a simple key

                //  - is limited to a single line,
                //  - is shorter than 1024 characters.


                if (key.IsPossible && (key.Line < cursor.Line || key.Index + 1024 < cursor.Index))
                {

                    // Check if the potential simple key to be removed is required.

                    if (key.IsRequired)
                    {
                        var mark = cursor.Mark();
                        tokens.Enqueue(new Error("While scanning a simple key, could not find expected ':'.", mark, mark));
                    }

                    key.MarkAsImpossible();
                }
            }
        }

        private void FetchNextToken()
        {
            // Check if we just started scanning.  Fetch STREAM-START then.
            if (!streamStartProduced)
            {
                FetchStreamStart();
                return;
            }

            // Eat whitespaces and comments until we reach the next token.

            ScanToNextToken();

            // Remove obsolete potential simple keys.

            StaleSimpleKeys();

            // Check the indentation level against the current column.

            UnrollIndent(cursor.LineOffset);


            // Ensure that the buffer contains at least 4 characters.  4 is the length
            // of the longest indicators ('--- ' and '... ').


            analyzer.Buffer.Cache(4);

            // Is it the end of the stream?

            if (analyzer.Buffer.EndOfInput)
            {
                lastScalar = null;
                FetchStreamEnd();
            }

            // Is it a directive?

            if (cursor.LineOffset == 0 && analyzer.Check('%'))
            {
                lastScalar = null;
                FetchDirective();
                return;
            }

            // Is it the document start indicator?

            if (IsDocumentStart())
            {
                lastScalar = null;
                FetchDocumentIndicator(true);
                return;
            }

            // Is it the document end indicator?

            if (IsDocumentEnd())
            {
                lastScalar = null;
                FetchDocumentIndicator(false);
                return;
            }

            // Is it the flow sequence start indicator?

            if (analyzer.Check('['))
            {
                lastScalar = null;
                FetchFlowCollectionStart(true);
                return;
            }

            // Is it the flow mapping start indicator?

            if (analyzer.Check('{'))
            {
                lastScalar = null;
                FetchFlowCollectionStart(false);
                return;
            }

            // Is it the flow sequence end indicator?

            if (analyzer.Check(']'))
            {
                lastScalar = null;
                FetchFlowCollectionEnd(true);
                return;
            }

            // Is it the flow mapping end indicator?

            if (analyzer.Check('}'))
            {
                lastScalar = null;
                FetchFlowCollectionEnd(false);
                return;
            }

            // Is it the flow entry indicator?

            if (analyzer.Check(','))
            {
                lastScalar = null;
                FetchFlowEntry();
                return;
            }

            // Is it the block entry indicator?

            if (analyzer.Check('-'))
            {
                if (analyzer.IsWhiteBreakOrZero(1))
                {
                    FetchBlockEntry();
                    return;
                }
                else if (flowLevel > 0 && analyzer.Check(",[]{}", 1))
                {
                    tokens.Enqueue(new Error("Invalid key indicator format.", cursor.Mark(), cursor.Mark()));
                }
            }

            // Is it the key indicator?

            if (analyzer.Check('?') &&
                (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)))
            {
                if (analyzer.IsWhiteBreakOrZero(1))
                {
                    FetchKey();
                    return;
                }
            }

            // Is it the value indicator?
            if (analyzer.Check(':') &&
                (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)) &&
                !(simpleKeyAllowed && flowLevel > 0) &&
                !(flowScalarFetched && analyzer.Check(':', 1)))
            {
                if (analyzer.IsWhiteBreakOrZero(1) || analyzer.Check(',', 1) || flowScalarFetched || flowCollectionFetched || startFlowCollectionFetched)
                {
                    if (lastScalar != null)
                    {
                        lastScalar.IsKey = true;
                        lastScalar = null;
                    }

                    FetchValue();
                    return;
                }
            }

            // Is it an alias?

            if (analyzer.Check('*'))
            {
                FetchAnchor(true);
                return;
            }

            // Is it an anchor?

            if (analyzer.Check('&'))
            {
                FetchAnchor(false);
                return;
            }

            // Is it a tag?

            if (analyzer.Check('!'))
            {
                FetchTag();
                return;
            }

            // Is it a literal scalar?

            if (analyzer.Check('|') && flowLevel == 0)
            {
                FetchBlockScalar(true);
                return;
            }

            // Is it a folded scalar?

            if (analyzer.Check('>') && flowLevel == 0)
            {
                FetchBlockScalar(false);
                return;
            }

            // Is it a single-quoted scalar?

            if (analyzer.Check('\''))
            {
                FetchFlowScalar(true);
                return;
            }

            // Is it a double-quoted scalar?

            if (analyzer.Check('"'))
            {
                FetchFlowScalar(false);
                return;
            }


            // Is it a plain scalar?

            // A plain scalar may start with any non-blank characters except

            //      '-', '?', ':', ',', '[', ']', '{', '}',
            //      '#', '&', '*', '!', '|', '>', '\'', '\"',
            //      '%', '@', '`'.

            // In the block context (and, for the '-' indicator, in the flow context
            // too), it may also start with the characters

            //      '-', '?', ':'

            // if it is followed by a non-space character.

            // The last rule is more restrictive than the specification requires.


            var isInvalidPlainScalarCharacter = analyzer.IsWhiteBreakOrZero() || analyzer.Check("-?:,[]{}#&*!|>'\"%@`");

            var isPlainScalar =
                !isInvalidPlainScalarCharacter ||
                (analyzer.Check('-') && !analyzer.IsWhite(1)) ||
                (analyzer.Check("?:") && !analyzer.IsWhiteBreakOrZero(1)) ||
                (simpleKeyAllowed && flowLevel > 0);

            if (isPlainScalar)
            {
                if (plainScalarFollowedByComment)
                {
                    var startMark = cursor.Mark();
                    tokens.Enqueue(new Error("While scanning plain scalar, found a comment between adjacent scalars.", startMark, startMark));
                }

                if (flowScalarFetched || flowCollectionFetched && !startFlowCollectionFetched)
                {
                    if (analyzer.Check(':'))
                    {
                        Skip();
                    }
                }

                flowScalarFetched = false;
                flowCollectionFetched = false;
                startFlowCollectionFetched = false;
                plainScalarFollowedByComment = false;

                FetchPlainScalar();
                return;
            }

            if (simpleKeyAllowed && indent >= cursor.LineOffset && analyzer.IsTab())
            {
                throw new SyntaxErrorException("While scanning a mapping, found invalid tab as indentation.");
            }

            if (analyzer.IsWhiteBreakOrZero())
            {
                Skip();
                return;
            }

            // If we don't determine the token type so far, it is an error.
            var start = cursor.Mark();
            Skip();
            var end = cursor.Mark();

            throw new SyntaxErrorException(start, end, "While scanning for the next token, found character that cannot start any token.");
        }

        private bool CheckWhiteSpace()
        {
            return analyzer.Check(' ') || ((flowLevel > 0 || !simpleKeyAllowed) && analyzer.Check('\t'));
        }

        private void Skip()
        {
            cursor.Skip();
            analyzer.Buffer.Skip(1);
        }

        private void SkipLine()
        {
            if (analyzer.IsCrLf())
            {
                cursor.SkipLineByOffset(2);
                analyzer.Buffer.Skip(2);
            }
            else if (analyzer.IsBreak())
            {
                cursor.SkipLineByOffset(1);
                analyzer.Buffer.Skip(1);
            }
            else if (!analyzer.IsZero())
            {
                throw new InvalidOperationException("Not at a break.");
            }
        }

        private void ScanToNextToken()
        {
            // Until the next token is not find.

            while (true)
            {

                // Eat whitespaces.

                // Tabs are allowed:

                //  - in the flow context;
                //  - in the block context, but not at the beginning of the line or
                //  after '-', '?', or ':' (complex value).


                while (CheckWhiteSpace())
                {
                    Skip();
                }

                ProcessComment();

                // If it is a line break, eat it.

                if (analyzer.IsBreak())
                {
                    SkipLine();

                    // In the block context, a new line may start a simple key.

                    if (flowLevel == 0)
                    {
                        simpleKeyAllowed = true;
                    }
                }
                else
                {
                    // We have find a token.

                    break;
                }
            }
        }

        private void ProcessComment()
        {
            if (analyzer.Check('#'))
            {
                var start = cursor.Mark();

                // Eat '#'
                Skip();

                // Eat leading whitespace
                while (analyzer.IsSpace())
                {
                    Skip();
                }

                using var textBuilder = StringBuilderPool.Rent();
                var text = textBuilder.Builder;
                while (!analyzer.IsBreakOrZero())
                {
                    text.Append(ReadCurrentCharacter());
                }

                if (!SkipComments)
                {
                    var isInline = previous != null
                        && previous.End.Line == start.Line
                        && previous.End.Column != 1
                        && !(previous is StreamStart);

                    tokens.Enqueue(new Comment(text.ToString(), isInline, start, cursor.Mark()));
                }
            }
        }

        private void FetchStreamStart()
        {
            // Initialize the simple key stack.

            simpleKeys.Push(new SimpleKey());

            // A simple key is allowed at the beginning of the stream.

            simpleKeyAllowed = true;

            // We have started.

            streamStartProduced = true;

            // Create the STREAM-START token and append it to the queue.

            var mark = cursor.Mark();
            tokens.Enqueue(new StreamStart(mark, mark));
        }

        /// <summary>
        /// Pop indentation levels from the indents stack until the current level
        /// becomes less or equal to the column.  For each indentation level, append
        /// the BLOCK-END token.
        /// </summary>

        private void UnrollIndent(int column)
        {
            // In the flow context, do nothing.

            if (flowLevel != 0)
            {
                return;
            }

            // Loop through the indentation levels in the stack.

            while (indent > column)
            {
                // Create a token and append it to the queue.

                var mark = cursor.Mark();
                tokens.Enqueue(new BlockEnd(mark, mark));

                // Pop the indentation level.

                indent = indents.Pop();
            }
        }

        /// <summary>
        /// Produce the STREAM-END token and shut down the scanner.
        /// </summary>
        private void FetchStreamEnd()
        {
            cursor.ForceSkipLineAfterNonBreak();

            // Reset the indentation level.

            UnrollIndent(-1);

            // Reset simple keys.

            RemoveSimpleKey();

            simpleKeyAllowed = false;

            // Create the STREAM-END token and append it to the queue.

            streamEndProduced = true;
            var mark = cursor.Mark();
            tokens.Enqueue(new StreamEnd(mark, mark));
        }

        private void FetchDirective()
        {
            // Reset the indentation level.

            UnrollIndent(-1);

            // Reset simple keys.

            RemoveSimpleKey();

            simpleKeyAllowed = false;

            // Create the YAML-DIRECTIVE or TAG-DIRECTIVE token.

            var token = ScanDirective();

            // Append the token to the queue.
            if (token != null)
            {
                tokens.Enqueue(token);
            }
        }

        /// <summary>
        /// Scan a YAML-DIRECTIVE or TAG-DIRECTIVE token.
        ///
        /// Scope:
        ///      %YAML    1.1    # a comment \n
        ///      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        ///      %TAG    !yaml!  tag:yaml.org,2002:  \n
        ///      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        /// </summary>
        private Token? ScanDirective()
        {
            // Eat '%'.

            var start = cursor.Mark();

            Skip();

            // Scan the directive name.

            var name = ScanDirectiveName(start);

            // Is it a YAML directive?

            Token directive;
            switch (name)
            {
                case "YAML":
                    if (previous is DocumentStart || previous is StreamStart || previous is DocumentEnd)
                    {
                        directive = ScanVersionDirectiveValue(start);
                    }
                    else
                    {
                        throw new SemanticErrorException(start, cursor.Mark(), "While scanning a version directive, did not find preceding <document end>.");
                    }
                    break;

                case "TAG":
                    directive = ScanTagDirectiveValue(start);
                    break;

                default:
                    // warning: skipping reserved directive line
                    while (!analyzer.EndOfInput && !analyzer.Check('#') && !analyzer.IsBreak())
                    {
                        Skip();
                    }
                    return null;
            }

            // Eat the rest of the line including any comments.

            while (analyzer.IsWhite())
            {
                Skip();
            }

            ProcessComment();

            // Check if we are at the end of the line.

            if (!analyzer.IsBreakOrZero())
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, did not find expected comment or line break.");
            }

            // Eat a line break.

            if (analyzer.IsBreak())
            {
                SkipLine();
            }

            return directive;
        }

        /// <summary>
        /// Produce the DOCUMENT-START or DOCUMENT-END token.
        /// </summary>

        private void FetchDocumentIndicator(bool isStartToken)
        {
            // Reset the indentation level.

            UnrollIndent(-1);

            // Reset simple keys.

            RemoveSimpleKey();

            simpleKeyAllowed = false;

            // Consume the token.

            var start = cursor.Mark();

            Skip();
            Skip();
            Skip();

            if (isStartToken)
            {
                tokens.Enqueue(new DocumentStart(start, cursor.Mark()));
            }
            else
            {
                Token? errorToken = null;
                while (!analyzer.EndOfInput && !analyzer.IsBreak() && !analyzer.Check('#'))
                {
                    if (!analyzer.IsWhite())
                    {
                        errorToken = new Error("While scanning a document end, found invalid content after '...' marker.", start, cursor.Mark());
                        break;
                    }
                    Skip();
                }
                tokens.Enqueue(new DocumentEnd(start, start));
                if (errorToken != null)
                {
                    tokens.Enqueue(errorToken);
                }
            }
        }

        /// <summary>
        /// Produce the FLOW-SEQUENCE-START or FLOW-MAPPING-START token.
        /// </summary>

        private void FetchFlowCollectionStart(bool isSequenceToken)
        {
            // The indicators '[' and '{' may start a simple key.

            SaveSimpleKey();

            // Increase the flow level.

            IncreaseFlowLevel();

            // A simple key may follow the indicators '[' and '{'.

            simpleKeyAllowed = true;

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            // Create the FLOW-SEQUENCE-START of FLOW-MAPPING-START token.

            Token token;
            if (isSequenceToken)
            {
                token = new FlowSequenceStart(start, start);
                flowSequenceStartLine = token.Start.Line;
            }
            else
            {
                token = new FlowMappingStart(start, start);
            }

            tokens.Enqueue(token);
            startFlowCollectionFetched = true;
        }

        /// <summary>
        /// Increase the flow level and resize the simple key list if needed.
        /// </summary>

        private void IncreaseFlowLevel()
        {
            // Reset the simple key on the next level.

            simpleKeys.Push(new SimpleKey());

            // Increase the flow level.

            ++flowLevel;
        }

        /// <summary>
        /// Produce the FLOW-SEQUENCE-END or FLOW-MAPPING-END token.
        /// </summary>

        private void FetchFlowCollectionEnd(bool isSequenceToken)
        {
            // Reset any potential simple key on the current flow level.

            RemoveSimpleKey();

            // Decrease the flow level.

            DecreaseFlowLevel();

            // No simple keys after the indicators ']' and '}'.

            simpleKeyAllowed = false;

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            Token? token, errorToken = null;
            if (isSequenceToken)
            {
                if (analyzer.Check('#'))
                {
                    errorToken = new Error("While scanning a flow sequence end, found invalid comment after ']'.", start, start);
                }

                token = new FlowSequenceEnd(start, start);
            }
            else
            {
                token = new FlowMappingEnd(start, start);
            }

            tokens.Enqueue(token);
            if (errorToken != null)
            {
                tokens.Enqueue(errorToken);
            }

            flowCollectionFetched = true;
        }

        /// <summary>
        /// Decrease the flow level.
        /// </summary>

        private void DecreaseFlowLevel()
        {
            // flowLevel could be zero in case of malformed YAML.
            // Since this is handled elsewhere, just ignore it.
            if (flowLevel > 0)
            {
                --flowLevel;
                simpleKeys.Pop();
            }
        }

        /// <summary>
        /// Produce the FLOW-ENTRY token.
        /// </summary>

        private void FetchFlowEntry()
        {
            // Reset any potential simple keys on the current flow level.

            RemoveSimpleKey();

            // Simple keys are allowed after ','.

            simpleKeyAllowed = true;

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            var end = cursor.Mark();
            if (analyzer.Check('#'))
            {
                tokens.Enqueue(new Error("While scanning a flow entry, found invalid comment after comma.", start, end));
                return;
            }

            // Create the FLOW-ENTRY token and append it to the queue.

            tokens.Enqueue(new FlowEntry(start, end));
        }

        /// <summary>
        /// Produce the BLOCK-ENTRY token.
        /// </summary>

        private void FetchBlockEntry()
        {
            // Check if the scanner is in the block context.

            if (flowLevel == 0)
            {
                // Check if we are allowed to start a new entry.

                if (!simpleKeyAllowed)
                {
                    if (previousAnchor != null)
                    {
                        if (previousAnchor.End.Line == cursor.Line)
                        {
                            throw new SemanticErrorException(previousAnchor.Start, previousAnchor.End, "Anchor before sequence entry on same line is not allowed.");
                        }
                    }
                    var mark = cursor.Mark();
                    tokens.Enqueue(new Error("Block sequence entries are not allowed in this context.", mark, mark));
                }

                // Add the BLOCK-SEQUENCE-START token if needed.
                RollIndent(cursor.LineOffset, -1, true, cursor.Mark());
            }
            else
            {

                // It is an error for the '-' indicator to occur in the flow context,
                // but we let the Parser detect and report about it because the Parser
                // is able to point to the context.

            }

            // Reset any potential simple keys on the current flow level.

            RemoveSimpleKey();

            // Simple keys are allowed after '-'.

            simpleKeyAllowed = true;

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            // Create the BLOCK-ENTRY token and append it to the queue.

            tokens.Enqueue(new BlockEntry(start, cursor.Mark()));
        }

        /// <summary>
        /// Produce the KEY token.
        /// </summary>

        private void FetchKey()
        {
            // In the block context, additional checks are required.

            if (flowLevel == 0)
            {
                // Check if we are allowed to start a new key (not necessary simple).

                if (!simpleKeyAllowed)
                {
                    var mark = cursor.Mark();
                    throw new SyntaxErrorException(mark, mark, "Mapping keys are not allowed in this context.");
                }

                // Add the BLOCK-MAPPING-START token if needed.

                RollIndent(cursor.LineOffset, -1, false, cursor.Mark());
            }

            // Reset any potential simple keys on the current flow level.

            RemoveSimpleKey();

            // Simple keys are allowed after '?' in the block context.

            simpleKeyAllowed = flowLevel == 0;

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            // Create the KEY token and append it to the queue.

            tokens.Enqueue(new Key(start, cursor.Mark()));
        }

        /// <summary>
        /// Produce the VALUE token.
        /// </summary>

        private void FetchValue()
        {
            var simpleKey = simpleKeys.Peek();

            // Have we find a simple key?

            if (simpleKey.IsPossible)
            {
                // Create the KEY token and insert it into the queue.

                tokens.Insert(simpleKey.TokenNumber - tokensParsed, new Key(simpleKey.Mark, simpleKey.Mark));

                // In the block context, we may need to add the BLOCK-MAPPING-START token.

                RollIndent(simpleKey.LineOffset, simpleKey.TokenNumber, false, simpleKey.Mark);

                // Remove the simple key.

                simpleKey.MarkAsImpossible();

                // A simple key cannot follow another simple key.

                simpleKeyAllowed = false;
            }
            else
            {
                // The ':' indicator follows a complex key.

                // Simple keys after ':' are allowed in the block context.

                var localSimpleKeyAllowed = flowLevel == 0;

                // In the block context, extra checks are required.

                if (localSimpleKeyAllowed)
                {
                    // Check if we are allowed to start a complex value.

                    if (!simpleKeyAllowed)
                    {
                        var mark = cursor.Mark();
                        tokens.Enqueue(new Error("Mapping values are not allowed in this context.", mark, mark));
                        return;
                    }

                    // Add the BLOCK-MAPPING-START token if needed.

                    RollIndent(cursor.LineOffset, -1, false, cursor.Mark());

                    // Check if we are dealing with empty key.

                    if (cursor.LineOffset == 0 && simpleKey.LineOffset == 0)
                    {
                        // Create the KEY token and insert it into the queue.

                        tokens.Insert(tokens.Count, new Key(simpleKey.Mark, simpleKey.Mark));

                        // A simple key cannot follow another simple key.

                        localSimpleKeyAllowed = false;
                    }
                }

                simpleKeyAllowed = localSimpleKeyAllowed;
            }

            // Consume the token.

            var start = cursor.Mark();
            Skip();

            // Create the VALUE token and append it to the queue.

            tokens.Enqueue(new Value(start, cursor.Mark()));
        }

        /// <summary>
        /// Push the current indentation level to the stack and set the new level
        /// the current column is greater than the indentation level.  In this case,
        /// append or insert the specified token into the token queue.
        /// </summary>
        private void RollIndent(int column, int number, bool isSequence, Mark position)
        {
            // In the flow context, do nothing.

            if (flowLevel > 0)
            {
                return;
            }

            if (indent < column)
            {

                // Push the current indentation level to the stack and set the new
                // indentation level.


                indents.Push(indent);

                indent = column;

                // Create a token and insert it into the queue.

                Token token;
                if (isSequence)
                {
                    token = new BlockSequenceStart(position, position);
                }
                else
                {
                    token = new BlockMappingStart(position, position);
                }

                if (number == -1)
                {
                    tokens.Enqueue(token);
                }
                else
                {
                    tokens.Insert(number - tokensParsed, token);
                }
            }
        }

        /// <summary>
        /// Produce the ALIAS or ANCHOR token.
        /// </summary>

        private void FetchAnchor(bool isAlias)
        {
            // An anchor or an alias could be a simple key.

            SaveSimpleKey();

            // A simple key cannot follow an anchor or an alias.

            simpleKeyAllowed = false;

            // Create the ALIAS or ANCHOR token and append it to the queue.

            tokens.Enqueue(ScanAnchor(isAlias));
        }

        private Token ScanAnchor(bool isAlias)
        {
            // Eat the indicator character.

            var start = cursor.Mark();

            Skip();

            var isAliasKey = false;
            if (isAlias)
            {
                var key = simpleKeys.Peek();
                isAliasKey = key.IsRequired && key.IsPossible;
            }

            // Consume the value.
            // YAML 1.2 - section 6.9.2."Node Anchors" specifies disallowed characters
            // in the anchor name as follows:
            //     '[', ']', '{', '}' and ','
            // ref: https://yaml.org/spec/1.2/spec.html#id2785586

            using var valueBuilder = StringBuilderPool.Rent();
            var value = valueBuilder.Builder;
            while (!analyzer.IsWhiteBreakOrZero())
            {
                // Anchor: read all allowed characters

                // Alias: read all allowed characters except colon (':'); read colon when token is:
                //    * not used in key OR
                //    * used in key and colon is not last character

                if (!analyzer.Check("[]{},") &&
                    !(isAliasKey && analyzer.Check(':') && analyzer.IsWhiteBreakOrZero(1)))
                {
                    value.Append(ReadCurrentCharacter());
                }
                else
                {
                    break;
                }
            }

            // Check if length of the anchor is greater than 0 and it is followed by
            // a whitespace character or one of the indicators:

            //      '?', ':', ',', ']', '}', '%', '@', '`'.


            if (value.Length == 0 || !(analyzer.IsWhiteBreakOrZero() || analyzer.Check("?:,]}%@`")))
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning an anchor or alias, found value containing disallowed: []{},");
            }

            // Create a token.
            var name = new AnchorName(value.ToString());
            if (isAlias)
            {
                return new AnchorAlias(name, start, cursor.Mark());
            }
            else
            {
                return previousAnchor = new Anchor(name, start, cursor.Mark());
            }
        }

        /// <summary>
        /// Produce the TAG token.
        /// </summary>

        private void FetchTag()
        {
            // A tag could be a simple key.

            SaveSimpleKey();

            // A simple key cannot follow a tag.

            simpleKeyAllowed = false;

            // Create the TAG token and append it to the queue.

            tokens.Enqueue(ScanTag());
        }

        /// <summary>
        /// Scan a TAG token.
        /// </summary>

        Token ScanTag()
        {
            var start = cursor.Mark();

            // Check if the tag is in the canonical form.

            string handle;
            string suffix;

            if (analyzer.Check('<', 1))
            {
                // Set the handle to ''

                handle = string.Empty;

                // Eat '!<'

                Skip();
                Skip();

                // Consume the tag value.

                suffix = ScanTagUri(null, start);

                // Check for '>' and eat it.

                if (!analyzer.Check('>'))
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find the expected '>'.");
                }

                Skip();
            }
            else
            {
                // The tag has either the '!suffix' or the '!handle!suffix' form.

                // First, try to scan a handle.

                var firstPart = ScanTagHandle(false, start);

                // Check if it is, indeed, handle.

                if (firstPart.Length > 1 && firstPart[0] == '!' && firstPart[firstPart.Length - 1] == '!')
                {
                    handle = firstPart;

                    // Scan the suffix now.

                    suffix = ScanTagUri(null, start);
                }
                else
                {
                    // It wasn't a handle after all.  Scan the rest of the tag.

                    suffix = ScanTagUri(firstPart, start);

                    // Set the handle to '!'.

                    handle = "!";


                    // A special case: the '!' tag.  Set the handle to '' and the
                    // suffix to '!'.


                    if (suffix.Length == 0)
                    {
                        suffix = handle;
                        handle = string.Empty;
                    }
                }
            }

            // Check the character which ends the tag.

            if (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check(','))
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find expected whitespace, comma or line break.");
            }

            // Create a token.

            return new Tag(handle, suffix, start, cursor.Mark());
        }

        /// <summary>
        /// Produce the SCALAR(...,literal) or SCALAR(...,folded) tokens.
        /// </summary>

        private void FetchBlockScalar(bool isLiteral)
        {
            // A block scalar can be a simple key

            SaveSimpleKey();

            // A simple key may follow a block scalar.

            simpleKeyAllowed = true;

            // Create the SCALAR token and append it to the queue.

            tokens.Enqueue(ScanBlockScalar(isLiteral));
        }

        /// <summary>
        /// Scan a block scalar.
        /// </summary>

        Token ScanBlockScalar(bool isLiteral)
        {
            using var valueBuilder = StringBuilderPool.Rent();
            var value = valueBuilder.Builder;

            using var leadingBreakBuilder = StringBuilderPool.Rent();
            var leadingBreak = leadingBreakBuilder.Builder;

            using var trailingBreaksBuilder = StringBuilderPool.Rent();
            var trailingBreaks = trailingBreaksBuilder.Builder;

            var chomping = 0;
            var increment = 0;
            var currentIndent = 0;
            var leadingBlank = false;
            bool? isFirstLine = null;

            // Eat the indicator '|' or '>'.

            var start = cursor.Mark();

            Skip();

            // Check for a chomping indicator.

            if (analyzer.Check("+-"))
            {
                // Set the chomping method and eat the indicator.

                chomping = analyzer.Check('+') ? +1 : -1;

                Skip();

                // Check for an indentation indicator.

                if (analyzer.IsDigit())
                {
                    // Check that the indentation is greater than 0.

                    if (analyzer.Check('0'))
                    {
                        throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found an indentation indicator equal to 0.");
                    }

                    // Get the indentation level and eat the indicator.

                    increment = analyzer.AsDigit();

                    Skip();
                }
            }

            // Do the same as above, but in the opposite order.

            else if (analyzer.IsDigit())
            {
                if (analyzer.Check('0'))
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found an indentation indicator equal to 0.");
                }

                increment = analyzer.AsDigit();

                Skip();

                if (analyzer.Check("+-"))
                {
                    chomping = analyzer.Check('+') ? +1 : -1;

                    Skip();
                }
            }

            // Check if there is a comment without whitespace after block scalar indicator (yaml-test-suite: X4QW).

            if (analyzer.Check('#'))
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found a comment without whtespace after '>' indicator.");
            }

            // Eat whitespaces and comments to the end of the line.

            while (analyzer.IsWhite())
            {
                Skip();
            }

            ProcessComment();

            // Check if we are at the end of the line.

            if (!analyzer.IsBreakOrZero())
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, did not find expected comment or line break.");
            }

            // Eat a line break.

            if (analyzer.IsBreak())
            {
                SkipLine();
                if (!isFirstLine.HasValue)
                {
                    isFirstLine = true;
                }
                else if (isFirstLine == true)
                {
                    isFirstLine = false;
                }
            }

            var end = cursor.Mark();

            // Set the indentation level if it was specified.

            if (increment != 0)
            {
                currentIndent = indent >= 0 ? indent + increment : increment;
            }

            // Scan the leading line breaks and determine the indentation level if needed.

            currentIndent = ScanBlockScalarBreaks(currentIndent, trailingBreaks, isLiteral, ref end, ref isFirstLine);
            isFirstLine = false;

            // Scan the block scalar content.

            while (cursor.LineOffset == currentIndent && !analyzer.IsZero() && !IsDocumentEnd())
            {
                // We are at the beginning of a non-empty line.

                // Is it a trailing whitespace?

                var trailingBlank = analyzer.IsWhite();

                // Check if we need to fold the leading line break.

                if (!isLiteral && StartsWith(leadingBreak, '\n') && !leadingBlank && !trailingBlank)
                {
                    // Do we need to join the lines by space?

                    if (trailingBreaks.Length == 0)
                    {
                        value.Append(' ');
                    }

                    leadingBreak.Length = 0;
                }
                else
                {
                    value.Append(leadingBreak.ToString());
                    leadingBreak.Length = 0;
                }

                // Append the remaining line breaks.

                value.Append(trailingBreaks.ToString());
                trailingBreaks.Length = 0;

                // Is it a leading whitespace?

                leadingBlank = analyzer.IsWhite();

                // Consume the current line.

                while (!analyzer.IsBreakOrZero())
                {
                    value.Append(ReadCurrentCharacter());
                }

                // Consume the line break.
                var lineBreak = ReadLine();
                if (lineBreak != '\0')
                {
                    leadingBreak.Append(lineBreak);
                }

                // Eat the following indentation spaces and line breaks.

                currentIndent = ScanBlockScalarBreaks(currentIndent, trailingBreaks, isLiteral, ref end, ref isFirstLine);
            }

            // Chomp the tail.

            if (chomping != -1)
            {
                value.Append(leadingBreak);
            }
            if (chomping == 1)
            {
                value.Append(trailingBreaks);
            }

            // Create a token.

            var style = isLiteral ? ScalarStyle.Literal : ScalarStyle.Folded;
            return new Scalar(value.ToString(), style, start, end);
        }

        /// <summary>
        /// Scan indentation spaces and line breaks for a block scalar.  Determine the
        /// indentation level if needed.
        /// </summary>

        private int ScanBlockScalarBreaks(int currentIndent, StringBuilder breaks, bool isLiteral, ref Mark end, ref bool? isFirstLine)
        {
            var maxIndent = 0;
            var indentOfFirstLine = -1;

            end = cursor.Mark();

            // Eat the indentation spaces and line breaks.

            while (true)
            {
                // Eat the indentation spaces.

                while ((currentIndent == 0 || cursor.LineOffset < currentIndent) && analyzer.IsSpace())
                {
                    Skip();
                }

                if (cursor.LineOffset > maxIndent)
                {
                    maxIndent = cursor.LineOffset;
                }

                // Have we find a non-empty line?

                if (!analyzer.IsBreak())
                {
                    if (isLiteral && isFirstLine == true)
                    {
                        var localIndent = cursor.LineOffset;
                        var i = 0;
                        while (!analyzer.IsBreak(i) && analyzer.IsSpace(i))
                        {
                            ++i;
                            ++localIndent;
                        }

                        if (analyzer.IsBreak(i) && localIndent > cursor.LineOffset)
                        {
                            isFirstLine = false;
                            indentOfFirstLine = localIndent;
                        }
                    }
                    break;
                }

                if (isFirstLine == true)
                {
                    isFirstLine = false;
                    indentOfFirstLine = cursor.LineOffset;
                }

                // Consume the line break.

                breaks.Append(ReadLine());

                end = cursor.Mark();
            }

            // Check if first line after literal is all spaces and count of spaces is more than "1 + currentIndent".

            if (isLiteral && indentOfFirstLine > 1 && currentIndent < indentOfFirstLine - 1)
            {
                // W9L4
                throw new SemanticErrorException(end, cursor.Mark(), "While scanning a literal block scalar, found extra spaces in first line.");
            }

            if (!isLiteral && maxIndent > cursor.LineOffset && indentOfFirstLine > -1)
            {
                // S98Z
                throw new SemanticErrorException(end, cursor.Mark(), "While scanning a literal block scalar, found more spaces in lines above first content line.");
            }

            // Determine the indentation level if needed.

            if (currentIndent == 0 && (cursor.LineOffset > 0 || indent > -1))
            {
                currentIndent = Math.Max(maxIndent, Math.Max(indent + 1, 1));
            }

            return currentIndent;
        }

        /// <summary>
        /// Produce the SCALAR(...,single-quoted) or SCALAR(...,double-quoted) tokens.
        /// </summary>

        private void FetchFlowScalar(bool isSingleQuoted)
        {
            // A plain scalar could be a simple key.

            SaveSimpleKey();

            // A simple key cannot follow a flow scalar.

            simpleKeyAllowed = false;

            // Indicates the adjacent flow scalar that a prior flow scalar has been fetched.

            flowScalarFetched = true;

            // Create the SCALAR token and append it to the queue.

            tokens.Enqueue(ScanFlowScalar(isSingleQuoted));

            // Check if there is a comment subsequently after double-quoted scalar without space.

            if (!isSingleQuoted && analyzer.Check('#'))
            {
                var start = cursor.Mark();
                tokens.Enqueue(new Error("While scanning a flow sequence end, found invalid comment after double-quoted scalar.", start, start));
            }
        }

        /// <summary>
        /// Scan a quoted scalar.
        /// </summary>

        private Token ScanFlowScalar(bool isSingleQuoted)
        {
            // Eat the left quote.

            var start = cursor.Mark();

            Skip();

            // Consume the content of the quoted scalar.

            using var valueBuilder = StringBuilderPool.Rent();
            var value = valueBuilder.Builder;

            using var whitespacesBuilder = StringBuilderPool.Rent();
            var whitespaces = whitespacesBuilder.Builder;

            using var leadingBreakBuilder = StringBuilderPool.Rent();
            var leadingBreak = leadingBreakBuilder.Builder;

            using var trailingBreaksBuilder = StringBuilderPool.Rent();
            var trailingBreaks = trailingBreaksBuilder.Builder;

            var hasLeadingBlanks = false;

            while (true)
            {
                // Check that there are no document indicators at the beginning of the line.

                if (IsDocumentIndicator())
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unexpected document indicator.");
                }

                // Check for EOF.

                if (analyzer.IsZero())
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unexpected end of stream.");
                }

                if (hasLeadingBlanks && !isSingleQuoted && indent >= cursor.LineOffset)
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a multi-line double-quoted scalar, found wrong indentation.");
                }

                hasLeadingBlanks = false;

                // Consume non-blank characters.

                while (!analyzer.IsWhiteBreakOrZero())
                {
                    // Check for an escaped single quote.

                    if (isSingleQuoted && analyzer.Check('\'', 0) && analyzer.Check('\'', 1))
                    {
                        value.Append('\'');
                        Skip();
                        Skip();
                    }

                    // Check for the right quote.

                    else if (analyzer.Check(isSingleQuoted ? '\'' : '"'))
                    {
                        break;
                    }

                    // Check for an escaped line break.

                    else if (!isSingleQuoted && analyzer.Check('\\') && analyzer.IsBreak(1))
                    {
                        Skip();
                        SkipLine();
                        hasLeadingBlanks = true;
                        break;
                    }

                    // Check for an escape sequence.

                    else if (!isSingleQuoted && analyzer.Check('\\'))
                    {
                        var codeLength = 0;

                        // Check the escape character.

                        var escapeCharacter = analyzer.Peek(1);
                        switch (escapeCharacter)
                        {
                            case 'x':
                                codeLength = 2;
                                break;

                            case 'u':
                                codeLength = 4;
                                break;

                            case 'U':
                                codeLength = 8;
                                break;

                            default:
                                char unescapedCharacter;
                                if (SimpleEscapeCodes.TryGetValue(escapeCharacter, out unescapedCharacter))
                                {
                                    value.Append(unescapedCharacter);
                                }
                                else
                                {
                                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unknown escape character.");
                                }
                                break;
                        }

                        Skip();
                        Skip();

                        // Consume an arbitrary escape code.

                        if (codeLength > 0)
                        {
                            var character = 0;

                            // Scan the character value.

                            for (var k = 0; k < codeLength; ++k)
                            {
                                if (!analyzer.IsHex(k))
                                {
                                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, did not find expected hexadecimal number.");
                                }
                                character = ((character << 4) + analyzer.AsHex(k));
                            }

                            // Check the value and write the character.

                            if ((character >= 0xD800 && character <= 0xDFFF) || character > 0x10FFFF)
                            {
                                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found invalid Unicode character escape code.");
                            }

                            value.Append(char.ConvertFromUtf32(character));

                            // Advance the pointer.

                            for (var k = 0; k < codeLength; ++k)
                            {
                                Skip();
                            }
                        }
                    }
                    else
                    {
                        // It is a non-escaped non-blank character.

                        value.Append(ReadCurrentCharacter());
                    }
                }

                // Check if we are at the end of the scalar.

                if (analyzer.Check(isSingleQuoted ? '\'' : '"'))
                {
                    break;
                }

                // Consume blank characters.

                while (analyzer.IsWhite() || analyzer.IsBreak())
                {
                    if (analyzer.IsWhite())
                    {
                        // Consume a space or a tab character.

                        if (!hasLeadingBlanks)
                        {
                            whitespaces.Append(ReadCurrentCharacter());
                        }
                        else
                        {
                            Skip();
                        }
                    }
                    else
                    {
                        // Check if it is a first line break.

                        if (!hasLeadingBlanks)
                        {
                            whitespaces.Length = 0;
                            leadingBreak.Append(ReadLine());
                            hasLeadingBlanks = true;
                        }
                        else
                        {
                            trailingBreaks.Append(ReadLine());
                        }
                    }
                }

                // Join the whitespaces or fold line breaks.

                if (hasLeadingBlanks)
                {
                    // Do we need to fold line breaks?

                    if (StartsWith(leadingBreak, '\n'))
                    {
                        if (trailingBreaks.Length == 0)
                        {
                            value.Append(' ');
                        }
                        else
                        {
                            value.Append(trailingBreaks.ToString());
                        }
                    }
                    else
                    {
                        value.Append(leadingBreak.ToString());
                        value.Append(trailingBreaks.ToString());
                    }
                    leadingBreak.Length = 0;
                    trailingBreaks.Length = 0;
                }
                else
                {
                    value.Append(whitespaces.ToString());
                    whitespaces.Length = 0;
                }
            }

            // Eat the right quote.

            Skip();

            return new Scalar(value.ToString(), isSingleQuoted ? ScalarStyle.SingleQuoted : ScalarStyle.DoubleQuoted, start, cursor.Mark());
        }

        /// <summary>
        /// Produce the SCALAR(...,plain) token.
        /// </summary>

        private void FetchPlainScalar()
        {
            // A plain scalar could be a simple key.

            SaveSimpleKey();

            // A simple key cannot follow a flow scalar.

            simpleKeyAllowed = false;

            // Create the SCALAR token and append it to the queue.
            var isMultiline = false;
            var scalar = ScanPlainScalar(ref isMultiline);
            lastScalar = scalar;
            if (isMultiline && analyzer.Check(':') && flowLevel == 0 && indent < cursor.LineOffset)
            {
                tokens.Enqueue(new Error("While scanning a multiline plain scalar, found invalid mapping.", cursor.Mark(), cursor.Mark()));
            }
            tokens.Enqueue(scalar);
        }

        /// <summary>
        /// Scan a plain scalar.
        /// </summary>

        private Scalar ScanPlainScalar(ref bool isMultiline)
        {
            using var valueBuilder = StringBuilderPool.Rent();
            var value = valueBuilder.Builder;

            using var whitespacesBuilder = StringBuilderPool.Rent();
            var whitespaces = whitespacesBuilder.Builder;

            using var leadingBreakBuilder = StringBuilderPool.Rent();
            var leadingBreak = leadingBreakBuilder.Builder;

            using var trailingBreaksBuilder = StringBuilderPool.Rent();
            var trailingBreaks = trailingBreaksBuilder.Builder;

            var hasLeadingBlanks = false;
            var currentIndent = indent + 1;

            var start = cursor.Mark();
            var end = start;

            var key = simpleKeys.Peek();

            // Consume the content of the plain scalar.

            while (true)
            {
                // Check for a document indicator.

                if (IsDocumentIndicator())
                {
                    break;
                }

                // Check for a comment.

                if (analyzer.Check('#'))
                {
                    if (indent < 0 && flowLevel == 0)
                    {
                        plainScalarFollowedByComment = true;
                    }
                    break;
                }

                var isAliasValue = analyzer.Check('*') && !(key.IsPossible && key.IsRequired);

                // Consume non-blank characters.
                while (!analyzer.IsWhiteBreakOrZero())
                {
                    // Check for indicators that may end a plain scalar.

                    if (analyzer.Check(':') &&
                        !isAliasValue &&
                        (analyzer.IsWhiteBreakOrZero(1) ||
                         (flowLevel > 0 && analyzer.Check(',', 1))) ||
                        (flowLevel > 0 && analyzer.Check(",[]{}")))
                    {
                        if (flowLevel == 0 && !key.IsPossible)
                        {
                            tokens.Enqueue(new Error("While scanning a plain scalar value, found invalid mapping.", cursor.Mark(), cursor.Mark()));
                        }
                        break;
                    }

                    // Check if we need to join whitespaces and breaks.

                    if (hasLeadingBlanks || whitespaces.Length > 0)
                    {
                        if (hasLeadingBlanks)
                        {
                            // Do we need to fold line breaks?

                            if (StartsWith(leadingBreak, '\n'))
                            {
                                if (trailingBreaks.Length == 0)
                                {
                                    value.Append(' ');
                                }
                                else
                                {
                                    value.Append(trailingBreaks);
                                }
                            }
                            else
                            {
                                value.Append(leadingBreak);
                                value.Append(trailingBreaks);
                            }

                            leadingBreak.Length = 0;
                            trailingBreaks.Length = 0;

                            hasLeadingBlanks = false;
                        }
                        else
                        {
                            value.Append(whitespaces);
                            whitespaces.Length = 0;
                        }
                    }
                    if (flowLevel > 0 && cursor.LineOffset < currentIndent)
                    {
                        throw new Exception();
                    }
                    // Copy the character.

                    value.Append(ReadCurrentCharacter());

                    end = cursor.Mark();
                }

                // Is it the end?

                if (!(analyzer.IsWhite() || analyzer.IsBreak()))
                {
                    break;
                }

                // Consume blank characters.

                while (analyzer.IsWhite() || analyzer.IsBreak())
                {
                    if (analyzer.IsWhite())
                    {
                        // Check for tab character that abuse indentation.

                        if (hasLeadingBlanks && cursor.LineOffset < currentIndent && analyzer.IsTab())
                        {
                            throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a plain scalar, found a tab character that violate indentation.");
                        }

                        // Consume a space or a tab character.

                        if (!hasLeadingBlanks)
                        {
                            whitespaces.Append(ReadCurrentCharacter());
                        }
                        else
                        {
                            Skip();
                        }
                    }
                    else
                    {
                        isMultiline = true;

                        // Check if it is a first line break.

                        if (!hasLeadingBlanks)
                        {
                            whitespaces.Length = 0;
                            leadingBreak.Append(ReadLine());
                            hasLeadingBlanks = true;
                        }
                        else
                        {
                            trailingBreaks.Append(ReadLine());
                        }
                    }
                }

                // Check indentation level.

                if (flowLevel == 0 && cursor.LineOffset < currentIndent)
                {
                    break;
                }
            }

            // Note that we change the 'simple_key_allowed' flag.

            if (hasLeadingBlanks)
            {
                simpleKeyAllowed = true;
            }

            // Create a token.

            return new Scalar(value.ToString(), ScalarStyle.Plain, start, end);
        }


        /// <summary>
        /// Remove a potential simple key at the current flow level.
        /// </summary>

        private void RemoveSimpleKey()
        {
            var key = simpleKeys.Peek();

            if (key.IsPossible && key.IsRequired)
            {
                // If the key is required, it is an error.

                throw new SyntaxErrorException(key.Mark, key.Mark, "While scanning a simple key, could not find expected ':'.");
            }

            // Remove the key from the stack.

            key.MarkAsImpossible();
        }

        /// <summary>
        /// Scan the directive name.
        ///
        /// Scope:
        ///      %YAML   1.1     # a comment \n
        ///       ^^^^
        ///      %TAG    !yaml!  tag:yaml.org,2002:  \n
        ///       ^^^
        /// </summary>
        private string ScanDirectiveName(in Mark start)
        {
            using var nameBuilder = StringBuilderPool.Rent();
            var name = nameBuilder.Builder;

            // Consume the directive name.

            while (analyzer.IsAlphaNumericDashOrUnderscore())
            {
                name.Append(ReadCurrentCharacter());
            }

            // Check if the name is empty.

            if (name.Length == 0)
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, could not find expected directive name.");
            }

            // Check for end of stream

            if (analyzer.EndOfInput)
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, found unexpected end of stream.");
            }

            // Check for an blank character after the name.

            if (!analyzer.IsWhiteBreakOrZero())
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, found unexpected non-alphabetical character.");
            }

            return name.ToString();
        }

        private void SkipWhitespaces()
        {
            // Eat whitespaces.

            while (analyzer.IsWhite())
            {
                Skip();
            }
        }

        /// <summary>
        /// Scan the value of VERSION-DIRECTIVE.
        ///
        /// Scope:
        ///      %YAML   1.1     # a comment \n
        ///           ^^^^^^
        /// </summary>
        private Token ScanVersionDirectiveValue(in Mark start)
        {
            SkipWhitespaces();

            // Consume the major version number.

            var major = ScanVersionDirectiveNumber(start);

            // Eat '.'.

            if (!analyzer.Check('.'))
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, did not find expected digit or '.' character.");
            }

            Skip();

            // Consume the minor version number.

            var minor = ScanVersionDirectiveNumber(start);

            return new VersionDirective(new Version(major, minor), start, start);
        }

        /// <summary>
        /// Scan the value of a TAG-DIRECTIVE token.
        ///
        /// Scope:
        ///      %TAG    !yaml!  tag:yaml.org,2002:  \n
        ///          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        /// </summary>
        private Token ScanTagDirectiveValue(in Mark start)
        {
            SkipWhitespaces();

            // Scan a handle.

            var handle = ScanTagHandle(true, start);

            // Expect a whitespace.

            if (!analyzer.IsWhite())
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace.");
            }

            SkipWhitespaces();

            // Scan a prefix.

            var prefix = ScanTagUri(null, start);

            // Expect a whitespace or line break.

            if (!analyzer.IsWhiteBreakOrZero())
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace or line break.");
            }

            return new TagDirective(handle, prefix, start, start);
        }

        /// <summary>
        /// Scan a tag.
        /// </summary>

        private string ScanTagUri(string? head, Mark start)
        {
            using var tagBuilder = StringBuilderPool.Rent();
            var tag = tagBuilder.Builder;

            if (head != null && head.Length > 1)
            {
                tag.Append(head.Substring(1));
            }

            // Scan the tag.

            // The set of characters that may appear in URI is as follows:

            //      '0'-'9', 'A'-'Z', 'a'-'z', '_', '-', ';', '/', '?', ':', '@', '&',
            //      '=', '+', '$', ',', '.', '!', '~', '*', '\'', '(', ')', '[', ']',
            //      '%'.


            while (analyzer.IsAlphaNumericDashOrUnderscore() || analyzer.Check(";/?:@&=+$.!~*'()[]%") ||
                   (analyzer.Check(',') && !analyzer.IsBreak(1)))
            {
                // Check if it is a URI-escape sequence.

                if (analyzer.Check('%'))
                {
                    tag.Append(ScanUriEscapes(start));
                }
                else if (analyzer.Check('+'))
                {
                    tag.Append(' ');
                    Skip();
                }
                else
                {
                    tag.Append(ReadCurrentCharacter());
                }
            }

            // Check if the tag is non-empty.

            if (tag.Length == 0)
            {
                return string.Empty;
            }

            var result = tag.ToString();
            if (result.EndsWith(","))
            {
                throw new SyntaxErrorException(cursor.Mark(), cursor.Mark(), "Unexpected comma at end of tag");
            }

            return result;
        }

        private static readonly byte[] EmptyBytes = new byte[0];

        /// <summary>
        /// Decode an URI-escape sequence corresponding to a single UTF-8 character.
        /// </summary>

        private string ScanUriEscapes(in Mark start)
        {
            // Decode the required number of characters.

            var charBytes = EmptyBytes;
            var nextInsertionIndex = 0;
            var width = 0;
            do
            {
                // Check for a URI-escaped octet.

                if (!(analyzer.Check('%') && analyzer.IsHex(1) && analyzer.IsHex(2)))
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find URI escaped octet.");
                }

                // Get the octet.

                var octet = (analyzer.AsHex(1) << 4) + analyzer.AsHex(2);

                // If it is the leading octet, determine the length of the UTF-8 sequence.

                if (width == 0)
                {
                    width = (octet & 0x80) == 0x00 ? 1 :
                            (octet & 0xE0) == 0xC0 ? 2 :
                            (octet & 0xF0) == 0xE0 ? 3 :
                            (octet & 0xF8) == 0xF0 ? 4 : 0;

                    if (width == 0)
                    {
                        throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect leading UTF-8 octet.");
                    }

                    charBytes = new byte[width];
                }
                else
                {
                    // Check if the trailing octet is correct.

                    if ((octet & 0xC0) != 0x80)
                    {
                        throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect trailing UTF-8 octet.");
                    }
                }

                // Copy the octet and move the pointers.

                charBytes[nextInsertionIndex++] = (byte)octet;

                Skip();
                Skip();
                Skip();
            }
            while (--width > 0);

            var result = Encoding.UTF8.GetString(charBytes, 0, nextInsertionIndex);

            if (result.Length == 0 || result.Length > 2)
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect UTF-8 sequence.");
            }

            return result;
        }

        /// <summary>
        /// Scan a tag handle.
        /// </summary>

        private string ScanTagHandle(bool isDirective, Mark start)
        {

            // Check the initial '!' character.

            if (!analyzer.Check('!'))
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find expected '!'.");
            }

            // Copy the '!' character.

            using var tagHandleBuilder = StringBuilderPool.Rent();
            var tagHandle = tagHandleBuilder.Builder;
            tagHandle.Append(ReadCurrentCharacter());

            // Copy all subsequent alphabetical and numerical characters.

            while (analyzer.IsAlphaNumericDashOrUnderscore())
            {
                tagHandle.Append(ReadCurrentCharacter());
            }

            // Check if the trailing character is '!' and copy it.

            if (analyzer.Check('!'))
            {
                tagHandle.Append(ReadCurrentCharacter());
            }
            else
            {

                // It's either the '!' tag or not really a tag handle.  If it's a %TAG
                // directive, it's an error.  If it's a tag token, it must be a part of
                // URI.


                if (isDirective && (tagHandle.Length != 1 || tagHandle[0] != '!'))
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag directive, did not find expected '!'.");
                }
            }

            return tagHandle.ToString();
        }

        /// <summary>
        /// Scan the version number of VERSION-DIRECTIVE.
        ///
        /// Scope:
        ///      %YAML   1.1     # a comment \n
        ///              ^
        ///      %YAML   1.1     # a comment \n
        ///                ^
        /// </summary>
        private int ScanVersionDirectiveNumber(in Mark start)
        {
            var value = 0;
            var length = 0;

            // Repeat while the next character is digit.

            while (analyzer.IsDigit())
            {
                // Check if the number is too long.

                if (++length > MaxVersionNumberLength)
                {
                    throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, found extremely long version number.");
                }

                value = value * 10 + analyzer.AsDigit();

                Skip();
            }

            // Check if the number was present.

            if (length == 0)
            {
                throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, did not find expected version number.");
            }

            return value;
        }

        /// <summary>
        /// Check if a simple key may start at the current position and add it if
        /// needed.
        /// </summary>

        private void SaveSimpleKey()
        {

            // A simple key is required at the current position if the scanner is in
            // the block context and the current column coincides with the indentation
            // level.


            var isRequired = (flowLevel == 0 && indent == cursor.LineOffset);


            // A simple key is required only when it is the first token in the current
            // line.  Therefore it is always allowed.  But we add a check anyway.


            Debug.Assert(simpleKeyAllowed || !isRequired, "Can't require a simple key and disallow it at the same time.");    // Impossible.


            // If the current position may start a simple key, save it.


            if (simpleKeyAllowed)
            {
                var key = new SimpleKey(isRequired, tokensParsed + tokens.Count, cursor);

                RemoveSimpleKey();

                simpleKeys.Pop();
                simpleKeys.Push(key);
            }
        }
    }
}

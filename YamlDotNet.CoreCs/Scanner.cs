using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using YamlDotNet.CoreCs.Tokens;

namespace YamlDotNet.CoreCs
{
	public class Scanner
	{
		private const int MaxVersionNumberLength = 9;
		
		private Stack<int> indents = new Stack<int>();
		private Queue<Token> tokens = new Queue<Token>();
		private Stack<SimpleKey> simpleKeys = new Stack<SimpleKey>();
		private bool streamStartProduced = false;
		private bool streamEndProduced = false;
		private int indent = -1;
		private bool simpleKeyAllowed;
		private Mark mark;
		private int flowLevel = 0;
		private int tokensParsed;
		
		private const int MaxBufferLength = 8;
		private readonly LookAheadBuffer buffer;
		
		private static readonly IDictionary<char, char> simpleEscapeCodes = InitializeSimpleEscapeCodes();
		
		private static IDictionary<char, char> InitializeSimpleEscapeCodes() {
			IDictionary<char, char> codes = new SortedDictionary<char, char>();
			codes.Add('0', '\0');
			codes.Add('a', '\x07');
			codes.Add('b', '\x08');
			codes.Add('t', '\x09');
			codes.Add('\t', '\x09');
			codes.Add('n', '\x0A');
			codes.Add('v', '\x0B');
			codes.Add('f', '\x0C');
			codes.Add('r', '\x0D');
			codes.Add('e', '\x1B');
			codes.Add(' ', '\x20');
			codes.Add('"', '"');
			codes.Add('\'', '\'');
			codes.Add('\\', '\\');
			codes.Add('N', '\x85');
			codes.Add('_', '\xA0');
			codes.Add('L', '\x2028');
			codes.Add('P', '\x2029');		
			return codes;
		}
			
		private char ReadCurrentCharacter() {
			char currentCharacter = buffer.Peek(0);
			Skip();
			return currentCharacter;
		}
		
		private char ReadLine() {
			if(Check("\r\n\x85")) // CR LF -> LF  --- CR|LF|NEL -> LF
			{
				SkipLine();
				return '\n';
			}
			
			char nextChar = buffer.Peek(0); // LS|PS -> LS|PS
			SkipLine();
			return nextChar;
		}
		
		public Scanner(TextReader input) {
			buffer = new LookAheadBuffer(input, MaxBufferLength);
			mark.Column = 1;
			mark.Line = 1;
		}
		
		private Token current;
		
		public Token Current
		{
			get {
				return current;
			}
		}
		
		public bool MoveNext() {
			if(tokens.Count == 0 && !streamEndProduced) {
				FetchMoreTokens();
			}
			if(tokens.Count > 0) {
				current = tokens.Dequeue();
				return true;
			} else {
				current = null;
				return false;
			}
		}
		
		private void FetchMoreTokens()
		{
			/* While we need more tokens to fetch, do it. */
			
			for(;;)
			{
				/*
				 * Check if we really need to fetch more tokens.
				 */
				
				bool needsMoreTokens = false;
				
				if (tokens.Count == 0)
				{
					/* Queue is empty. */
					
					needsMoreTokens = true;
				}
				else
				{
					/* Check if any potential simple key may occupy the head position. */
					
					yaml_parser_stale_simple_keys();
					
					foreach (SimpleKey simpleKey in simpleKeys) {
						if (simpleKey.IsPossible && simpleKey.TokenNumber == tokensParsed) {
							needsMoreTokens = true;
							break;
						}
					}
				}
				
				/* We are finished. */
				
				if (!needsMoreTokens) {
					break;
				}
				
				/* Fetch the next token. */
				
				yaml_parser_fetch_next_token();
			}
		}
		
		private void yaml_parser_stale_simple_keys() {
			// TODO: To be implemented
		}
		
		private void yaml_parser_fetch_next_token()
		{
			/* Ensure that the buffer is initialized. */

			buffer.Cache(1);

			/* Check if we just started scanning.  Fetch STREAM-START then. */

			if (!streamStartProduced) {
				 yaml_parser_fetch_stream_start();
			}

			/* Eat whitespaces and comments until we reach the next token. */

			yaml_parser_scan_to_next_token();

			/* Remove obsolete potential simple keys. */

			yaml_parser_stale_simple_keys();

			/* Check the indentation level against the current column. */

			yaml_parser_unroll_indent(mark.Column);

			/*
			 * Ensure that the buffer contains at least 4 characters.  4 is the length
			 * of the longest indicators ('--- ' and '... ').
			 */

			buffer.Cache(4);

			/* Is it the end of the stream? */

			if(buffer.EndOfInput) {
				yaml_parser_fetch_stream_end();
				return;
			}

			/* Is it a directive? */

			if (mark.Column == 1 && Check('%')) {
				yaml_parser_fetch_directive();
				return;
			}

			/* Is it the document start indicator? */

			bool isDocumentStart =
				mark.Column == 1 &&
				Check('-', 0) &&
				Check('-', 1) &&
				Check('-', 2) &&
				IsBlankOrBreakOrZero(3);
				
			if (IsDocumentIndicator()) {
				yaml_parser_fetch_document_indicator(true);
				return;
			}

			/* Is it the document end indicator? */

			bool isDocumentEnd =
				mark.Column == 1 &&
				Check('.', 0) &&
				Check('.', 1) &&
				Check('.', 2) &&
				IsBlankOrBreakOrZero(3);

			if (isDocumentEnd) {
				yaml_parser_fetch_document_indicator(false);
				return;
			}

			/* Is it the flow sequence start indicator? */

			if (Check('[')) {
				yaml_parser_fetch_flow_collection_start(true);
				return;
			}

			/* Is it the flow mapping start indicator? */

			if (Check('{')) {
				yaml_parser_fetch_flow_collection_start(false);
				return;
			}

			/* Is it the flow sequence end indicator? */

			if (Check(']')) {
				yaml_parser_fetch_flow_collection_end(true);
				return;
			}

			/* Is it the flow mapping end indicator? */

			if (Check('}')) {
				yaml_parser_fetch_flow_collection_end(false);
				return;
			}

			/* Is it the flow entry indicator? */

			if (Check(',')) {
				yaml_parser_fetch_flow_entry();
				return;
			}

			/* Is it the block entry indicator? */

			if (Check('-') && IsBlankOrBreakOrZero(1)) {
				yaml_parser_fetch_block_entry();
				return;
			}

			/* Is it the key indicator? */

			if (Check('?') && (flowLevel > 0 || IsBlankOrBreakOrZero(1))) {
				yaml_parser_fetch_key();
				return;
			}

			/* Is it the value indicator? */

			if (Check(':') && (flowLevel > 0 || IsBlankOrBreakOrZero(1))) {
				yaml_parser_fetch_value();
				return;
			}

			/* Is it an alias? */

			if (Check('*')) {
				yaml_parser_fetch_anchor(true);
				return;
			}

			/* Is it an anchor? */

			if (Check('&')) {
				yaml_parser_fetch_anchor(false);
				return;
			}

			/* Is it a tag? */

			if (Check('!')) {
				yaml_parser_fetch_tag();
				return;
			}

			/* Is it a literal scalar? */

			if (Check('|') && flowLevel == 0) {
				yaml_parser_fetch_block_scalar(true);
				return;
			}

			/* Is it a folded scalar? */

			if (Check('>') && flowLevel == 0) {
				yaml_parser_fetch_block_scalar(false);
				return;
			}

			/* Is it a single-quoted scalar? */

			if (Check('\'')) {
				yaml_parser_fetch_flow_scalar(true);
				return;
			}

			/* Is it a double-quoted scalar? */

			if (Check('"')) {
				yaml_parser_fetch_flow_scalar(false);
				return;
			}

			/*
			 * Is it a plain scalar?
			 *
			 * A plain scalar may start with any non-blank characters except
			 *
			 *      '-', '?', ':', ',', '[', ']', '{', '}',
			 *      '#', '&', '*', '!', '|', '>', '\'', '\"',
			 *      '%', '@', '`'.
			 *
			 * In the block context (and, for the '-' indicator, in the flow context
			 * too), it may also start with the characters
			 *
			 *      '-', '?', ':'
			 *
			 * if it is followed by a non-space character.
			 *
			 * The last rule is more restrictive than the specification requires.
			 */

			bool isInvalidPlainScalarCharacter = IsBlankOrBreakOrZero() || Check("-?:,[]{}#&*!|>'\"%@`");
			
			bool isPlainScalar =
				!isInvalidPlainScalarCharacter ||
				(Check('-') && !IsBlank(1)) ||
				(flowLevel == 0 && (Check("?:")) && !IsBlankOrBreakOrZero(1));
			
			if (isPlainScalar) {
				yaml_parser_fetch_plain_scalar();
				return;
			}

			/*
			 * If we don't determine the token type so far, it is an error.
			 */

			throw new SyntaxErrorException("While scanning for the next token, found character that cannot start any token.", mark);
		}
		
		private bool Check(char expected) {
			return Check(expected, 0);
		}
		
		private bool Check(char expected, int offset) {
			return buffer.Peek(offset) == expected;
		}
		
		private bool Check(string expectedCharacters) {
			return Check(expectedCharacters, 0);
		}
		
		private bool Check(string expectedCharacters, int offset) {
			Debug.Assert(expectedCharacters.Length > 1, "Use Check(char, int) instead.");
			
			char character = buffer.Peek(offset);
			
			foreach (char expected in expectedCharacters) {
				if(expected == character) {
					return true;
				}
			}
			return false;
		}
		
		private bool CheckWhiteSpace() {
			return Check(' ') || ((flowLevel > 0 || !simpleKeyAllowed) && Check('\t')); 
		}
			
		private bool IsDocumentIndicator() {
			return IsDocumentIndicator(0);
		}

		private bool IsDocumentIndicator(int offset) {
			if (mark.Column == 1 && IsBlankOrBreakOrZero(3)) {
				bool isDocumentStart = Check('-', 0) && Check('-', 1) && Check('-', 2);
				bool isDocumentEnd = Check('.', 0) && Check('.', 1) && Check('.', 2);

				return isDocumentStart || isDocumentEnd;
			} else {
				return false;
			}
		}

		/*
		 * Check if the character at the specified position is an alphabetical
		 * character, a digit, '_', or '-'.
		 */

		private bool IsAlpha(int offset) {
			char character = buffer.Peek(offset);

			return
				(character >= '0' && character <= '9') ||
				(character >= 'A' && character <= 'Z') ||
				(character >= 'a' && character <= 'z') ||
				character == '_' ||
				character == '-';
				
		}
		
		private bool IsAlpha() {
			return IsAlpha(0);
		}

		/*
		 * Check if the character at the specified position is a digit.
		 */
		
		private bool IsDigit(int offset) {
			char character = buffer.Peek(offset);
			return character >= '0' && character <= '9';
		}
		
		private bool IsDigit() {
			return IsDigit(0);
		}

		/*
		 * Get the value of a digit.
		 */

		private int AsDigit(int offset) {
			return buffer.Peek(offset) - '0';
		}
		
		private int AsDigit() {
			return AsDigit(0);
		}

		/*
		 * Check if the character at the specified position is a hex-digit.
		 */

		private bool IsHex(int offset) {
			char character = buffer.Peek(offset);
			return
				(character >= '0' && character <= '9') ||
				(character >= 'A' && character <= 'F') ||
				(character >= 'a' && character <= 'f');
		}
		
		private bool IsHex() {
			return IsHex(0);
		}

		/*
		 * Get the value of a hex-digit.
		 */

		private int AsHex(int offset) {
			char character = buffer.Peek(offset);
			
			if(character <= '9') {
				return character - '0';
			} else if(character <= 'F') {
				return character - 'A' + 10;
			} else {
				return character - 'a' + 10;
			}
		}
		
		private int AsHex() {
			return AsHex(0);
		}
		 
		/*
		 * Check if the character is ASCII.
		 */

		private bool IsAscii(int offset) {
			return buffer.Peek(offset) <= '\x7F';
		}
		
		private bool IsAscii() {
			return IsAscii(0);
		}

		/*
		 * Check if the character can be printed unescaped.
		 */

		private bool IsPrintable(int offset) {
			char character = buffer.Peek(offset);

			return
				character == '\x0A' ||
				(character >= '\x20' && character <= '\x7E') || 
				(character >= '\xA0' && character <= '\xD7FF') || 
				(character >= '\xE000' && character <= '\xFFFD' && character != '\xFEFF'); 				
		}
		
		private bool IsPrintable() {
			return IsPrintable(0);
		}
		
		/*
		 * Check if the character at the specified position is NUL.
		 */

		private bool IsZero(int offset) {
			return Check('\0', offset);
		}
		
		private bool IsZero() {
			return IsZero(0);
		}

		/*
		 * Check if the character at the specified position is space.
		 */

		private bool IsSpace(int offset) {
			return Check(' ', offset);
		}
		
		private bool IsSpace() {
			return IsSpace(0);
		}

		/*
		 * Check if the character at the specified position is tab.
		 */

		private bool IsTab(int offset) {
			return Check('\t', offset);
		}
		
		private bool IsTab() {
			return IsTab(0);
		}

		/*
		 * Check if the character at the specified position is blank (space or tab).
		 */

		private bool IsBlank(int offset) {
			return IsSpace(offset) || IsTab(offset);
		}
		
		private bool IsBlank() {
			return IsBlank(0);
		}

		/*
		 * Check if the character at the specified position is a line break.
		 */

		private bool IsBreak(int offset) {
			return Check("\r\n\x85\x2028\x2029", offset);
		}
		
		private bool IsBreak() {
			return IsBreak(0);
		}
		
		private bool IsCrLf(int offset) {
			return Check('\r', offset) && Check('\n', offset + 1);
		}
		
		private bool IsCrLf() {
			return IsCrLf(0);
		}

		/*
		 * Check if the character is a line break or NUL.
		 */
		
		private bool IsBreakOrZero(int offset) {
			return IsBreak(offset) || IsZero(offset);
		}
		
		private bool IsBreakOrZero() {
			return IsBreakOrZero(0);
		}

		/*
		 * Check if the character is a line break, space, or NUL.
		 */
		
		private bool IsSpaceOrZero(int offset) {
			return IsSpace(offset) || IsZero(offset);
		}
		
		private bool IsSpaceOrZero() {
			return IsSpaceOrZero(0);
		}

		/*
		 * Check if the character is a line break, space, tab, or NUL.
		 */
		
		private bool IsBlankOrBreakOrZero(int offset) {
			return IsBlank(offset) || IsBreakOrZero(offset);
		}
		
		private bool IsBlankOrBreakOrZero() {
			return IsBlankOrBreakOrZero(0);
		}
		
		private void Skip() {
			++mark.Index;
			++mark.Column;
			buffer.Skip(1);
		}
		
		private void SkipLine() {
			if(IsCrLf()) {
				mark.Index += 2;
				mark.Column = 1;
				++mark.Line;
				buffer.Skip(2);
			} else if(IsBreak()) {
				++mark.Index;
				mark.Column = 1;
				++mark.Line;
				buffer.Skip(1);
			} else {
				throw new InvalidOperationException("Not at a break.");
			}
		}
		
		private void yaml_parser_scan_to_next_token()
		{
			/* Until the next token is not found. */

			for(;;)
			{
				/*
				 * Eat whitespaces.
				 *
				 * Tabs are allowed:
				 *
				 *  - in the flow context;
				 *  - in the block context, but not at the beginning of the line or
				 *  after '-', '?', or ':' (complex value).  
				 */

				buffer.Cache(1);

				while (CheckWhiteSpace()) {
					Skip();
					buffer.Cache(1);
				}

				/* Eat a comment until a line break. */

				if (Check('#')) {
					while (!IsBreakOrZero()) {
						Skip();
						buffer.Cache(1);
					}
				}

				/* If it is a line break, eat it. */

				if (IsBreak())
				{
					buffer.Cache(2);
					SkipLine();

					/* In the block context, a new line may start a simple key. */

					if (flowLevel == 0) {
						simpleKeyAllowed = true;
					}
				}
				else
				{
					/* We have found a token. */

					break;
				}
			}
		}

		private void yaml_parser_fetch_stream_start()
		{
			/* Initialize the simple key stack. */

			simpleKeys.Push(new SimpleKey());

			/* A simple key is allowed at the beginning of the stream. */

			simpleKeyAllowed = true;

			/* We have started. */

			streamStartProduced = true;

			/* Create the STREAM-START token and append it to the queue. */

			tokens.Enqueue(new StreamStart(mark, mark));
		}

		/*
		 * Pop indentation levels from the indents stack until the current level
		 * becomes less or equal to the column.  For each intendation level, append
		 * the BLOCK-END token.
		 */

		private void yaml_parser_unroll_indent(int column)
		{
			/* In the flow context, do nothing. */

			if (flowLevel != 0) {
				return;
			}

			/* Loop through the intendation levels in the stack. */

			while (indent > column)
			{
				/* Create a token and append it to the queue. */

				tokens.Enqueue(new BlockEnd(mark, mark));

				/* Pop the indentation level. */

				indent = indents.Pop();
			}
		}
		
		/*
		 * Produce the STREAM-END token and shut down the scanner.
		 */
		private void yaml_parser_fetch_stream_end() {
			/* Force new line. */

			if (mark.Column != 1) {
				mark.Column = 1;
				++mark.Line;
			}

			/* Reset the indentation level. */

			yaml_parser_unroll_indent(-1);

			/* Reset simple keys. */

			yaml_parser_remove_simple_key();

			simpleKeyAllowed = false;

			/* Create the STREAM-END token and append it to the queue. */

			streamEndProduced = true;
			tokens.Enqueue(new StreamEnd(mark, mark));
		}
		
		private void yaml_parser_fetch_directive() {
			/* Reset the indentation level. */

			yaml_parser_unroll_indent(-1);

			/* Reset simple keys. */

			yaml_parser_remove_simple_key();

			simpleKeyAllowed = false;

			/* Create the YAML-DIRECTIVE or TAG-DIRECTIVE token. */

			Token token = yaml_parser_scan_directive();

			/* Append the token to the queue. */

			tokens.Enqueue(token);
		}
		
		/*
		 * Scan a YAML-DIRECTIVE or TAG-DIRECTIVE token.
		 *
		 * Scope:
		 *      %YAML    1.1    # a comment \n
		 *      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		 *      %TAG    !yaml!  tag:yaml.org,2002:  \n
		 *      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		 */

		private Token yaml_parser_scan_directive()
		{
			/* Eat '%'. */

			Mark start = mark;

			Skip();

			/* Scan the directive name. */

			string name = yaml_parser_scan_directive_name(start);

			/* Is it a YAML directive? */

			Token directive;
			switch (name) {
				case "YAML":
					directive = yaml_parser_scan_version_directive_value(start);
					break;

				case "TAG":
					directive = yaml_parser_scan_tag_directive_value(start);
					break;
				
				default:
					throw new SyntaxErrorException("While scanning a directive, found uknown directive name.", start);
			}

			/* Eat the rest of the line including any comments. */

			buffer.Cache(1);

			while (IsBlank()) {
				Skip();
				buffer.Cache(1);
			}

			if (Check('#')) {
				while (!IsBreakOrZero()) {
					Skip();
					buffer.Cache(1);
				}
			}

			/* Check if we are at the end of the line. */

			if (!IsBreakOrZero()) {
				throw new SyntaxErrorException("While scanning a directive, did not found expected comment or line break.", start);
			}

			/* Eat a line break. */

			if (IsBreak()) {
				buffer.Cache(2);
				SkipLine();
			}
			
			return directive;
		}

		/*
		 * Produce the DOCUMENT-START or DOCUMENT-END token.
		 */

		private void yaml_parser_fetch_document_indicator(bool isStartToken)
		{
			/* Reset the indentation level. */

			yaml_parser_unroll_indent(-1);

			/* Reset simple keys. */

			yaml_parser_remove_simple_key();

			simpleKeyAllowed = false;

			/* Consume the token. */

			Mark start = mark;

			Skip();
			Skip();
			Skip();

			Token token = isStartToken ? (Token)new DocumentStart(start, mark) : new DocumentEnd(start, start);
			tokens.Enqueue(token);
		}
		
		/*
		 * Produce the FLOW-SEQUENCE-START or FLOW-MAPPING-START token.
		 */

		private void yaml_parser_fetch_flow_collection_start(bool isSequenceToken) {
			/* The indicators '[' and '{' may start a simple key. */

			yaml_parser_save_simple_key();

			/* Increase the flow level. */

			yaml_parser_increase_flow_level();

			/* A simple key may follow the indicators '[' and '{'. */

			simpleKeyAllowed = true;

			/* Consume the token. */

			Mark start = mark;
			Skip();

			/* Create the FLOW-SEQUENCE-START of FLOW-MAPPING-START token. */

			Token token;
			if(isSequenceToken) {
				token = new FlowSequenceStart(start, start);
			} else {
				token = new FlowMappingStart(start, start);
			}

			tokens.Enqueue(token);
		}

		/*
		 * Increase the flow level and resize the simple key list if needed.
		 */

		private void yaml_parser_increase_flow_level()
		{
			/* Reset the simple key on the next level. */

			simpleKeys.Push(new SimpleKey());
			
			/* Increase the flow level. */

			++flowLevel;
		}		
		
		/*
		 * Produce the FLOW-SEQUENCE-END or FLOW-MAPPING-END token.
		 */

		private void yaml_parser_fetch_flow_collection_end(bool isSequenceToken) {
			/* Reset any potential simple key on the current flow level. */

			yaml_parser_remove_simple_key();

			/* Decrease the flow level. */

			yaml_parser_decrease_flow_level();

			/* No simple keys after the indicators ']' and '}'. */

			simpleKeyAllowed = false;

			/* Consume the token. */

			Mark start = mark;
			Skip();

			Token token;
			if(isSequenceToken) {
				token = new FlowSequenceEnd(start, start);
			} else {
				token = new FlowMappingEnd(start, start);
			}

			tokens.Enqueue(token);
		}
		
		/*
		 * Decrease the flow level.
		 */

		private void yaml_parser_decrease_flow_level()
		{
			Debug.Assert(flowLevel > 0, "Could flowLevel be zero when this method is called?");
			if (flowLevel > 0) {
				--flowLevel;
				simpleKeys.Pop();
			}
		}
		
		private void yaml_parser_fetch_flow_entry() {
			throw new NotImplementedException();
		}

		private void yaml_parser_fetch_block_entry() {
			throw new NotImplementedException();
		}

		private void yaml_parser_fetch_key() {
			throw new NotImplementedException();
		}

		private void yaml_parser_fetch_value() {
			throw new NotImplementedException();
		}

		/*
		 * Produce the ALIAS or ANCHOR token.
		 */

		private void yaml_parser_fetch_anchor(bool isAlias)
		{
			/* An anchor or an alias could be a simple key. */

			yaml_parser_save_simple_key();

			/* A simple key cannot follow an anchor or an alias. */

			simpleKeyAllowed = false;

			/* Create the ALIAS or ANCHOR token and append it to the queue. */

			tokens.Enqueue(yaml_parser_scan_anchor(isAlias));
		}

		private Token yaml_parser_scan_anchor(bool isAlias)
		{
			/* Eat the indicator character. */

			Mark start = mark;

			Skip();

			/* Consume the value. */

			StringBuilder value = new StringBuilder();
			while (IsAlpha()) {
				value.Append(ReadCurrentCharacter());
			}

			/*
			 * Check if length of the anchor is greater than 0 and it is followed by
			 * a whitespace character or one of the indicators:
			 *
			 *      '?', ':', ',', ']', '}', '%', '@', '`'.
			 */

			if(value.Length == 0 || !(IsBlankOrBreakOrZero() || Check("?:,]}%@`"))) {
				throw new SyntaxErrorException("While scanning an anchor or alias, did not find expected alphabetic or numeric character.", start);
			}

			/* Create a token. */
			
			if(isAlias) {
				return new Alias(value.ToString());
			}
			else {
				return new Anchor(value.ToString());
			}
		}

		/*
		 * Produce the TAG token.
		 */

		private void yaml_parser_fetch_tag()
		{
			/* A tag could be a simple key. */

			yaml_parser_save_simple_key();

			/* A simple key cannot follow a tag. */

			simpleKeyAllowed = false;

			/* Create the TAG token and append it to the queue. */

			tokens.Enqueue(yaml_parser_scan_tag());
		}

		/*
		 * Scan a TAG token.
		 */

		Token yaml_parser_scan_tag()
		{
			Mark start = mark;

			/* Check if the tag is in the canonical form. */

			string handle;
			string suffix;
			
			if (Check('<', 1))
			{
				/* Set the handle to '' */

				handle = string.Empty;

				/* Eat '!<' */

				Skip();
				Skip();

				/* Consume the tag value. */

				suffix = yaml_parser_scan_tag_uri(false, null, start);

				/* Check for '>' and eat it. */

				if (!Check('>')) {
					throw new SyntaxErrorException("While scanning a tag, did not find the expected '>'.", start);
				}

				Skip();
			}
			else
			{
				/* The tag has either the '!suffix' or the '!handle!suffix' form. */

				/* First, try to scan a handle. */

				string firstPart = yaml_parser_scan_tag_handle(false, start);

				/* Check if it is, indeed, handle. */

				if (firstPart.Length > 1 && firstPart[0] == '!' && firstPart[firstPart.Length - 1] == '!')
				{
					handle = firstPart;
					
					/* Scan the suffix now. */

					suffix = yaml_parser_scan_tag_uri(false, null, start);
				}
				else
				{
					/* It wasn't a handle after all.  Scan the rest of the tag. */

					suffix = yaml_parser_scan_tag_uri(false, null, start);

					yaml_parser_scan_tag_uri(false, firstPart, start);

					/* Set the handle to '!'. */

					handle = "!";

					/*
					 * A special case: the '!' tag.  Set the handle to '' and the
					 * suffix to '!'.
					 */

					if (suffix.Length == 0) {
						suffix = handle;
						handle = string.Empty;
					}
				}
			}

			/* Check the character which ends the tag. */

			if (!IsBlankOrBreakOrZero()) {
				throw new SyntaxErrorException("While scanning a tag, did not found expected whitespace or line break.", start);
			}

			/* Create a token. */

			return new Tag(handle, suffix, start, mark);
		}

			
		private void yaml_parser_fetch_block_scalar(bool isLiteral) {
			throw new NotImplementedException();
		}

		/*
		 * Produce the SCALAR(...,single-quoted) or SCALAR(...,double-quoted) tokens.
		 */

		private void yaml_parser_fetch_flow_scalar(bool isSingleQuoted) {
			/* A plain scalar could be a simple key. */

			yaml_parser_save_simple_key();

			/* A simple key cannot follow a flow scalar. */

			simpleKeyAllowed = false;

			/* Create the SCALAR token and append it to the queue. */

			tokens.Enqueue(yaml_parser_scan_flow_scalar(isSingleQuoted));
		}

		/*
		 * Scan a quoted scalar.
		 */

		private Token yaml_parser_scan_flow_scalar(bool isSingleQuoted)
		{
			/* Eat the left quote. */

			Mark start = mark;

			Skip();

			/* Consume the content of the quoted scalar. */

			StringBuilder value = new StringBuilder();
			StringBuilder whitespaces = new StringBuilder();
			StringBuilder leadingBreak = new StringBuilder();
			StringBuilder trailingBreaks = new StringBuilder();
			for(;;)
			{
				/* Check that there are no document indicators at the beginning of the line. */

				buffer.Cache(4);

				if (IsDocumentIndicator()) {
					throw new SyntaxErrorException("While scanning a quoted scalar, found unexpected document indicator.", start);
				}

				/* Check for EOF. */

				if(IsZero()) {
					throw new SyntaxErrorException("While scanning a quoted scalar, found unexpected end of stream.", start);
				}

				/* Consume non-blank characters. */

				bool hasLeadingBlanks = false;

				while (!IsBlankOrBreakOrZero())
				{
					/* Check for an escaped single quote. */

					if (isSingleQuoted && Check('\'', 0) && Check('\'', 1))
					{
						value.Append('\'');
						Skip();
						Skip();
					}

					/* Check for the right quote. */

					else if (Check(isSingleQuoted ? '\'' : '"'))
					{
						break;
					}

					/* Check for an escaped line break. */

					else if (!isSingleQuoted && Check('\\') && IsBreak(1))
					{
						Skip();
						SkipLine();
						hasLeadingBlanks = true;
						break;
					}

					/* Check for an escape sequence. */

					else if (!isSingleQuoted && Check('\\'))
					{
						int codeLength = 0;

						/* Check the escape character. */
				
						char escapeCharacter = buffer.Peek(1); 
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
								if(simpleEscapeCodes.TryGetValue(escapeCharacter, out unescapedCharacter)) {
									value.Append(unescapedCharacter);
								} else {
									throw new SyntaxErrorException("While parsing a quoted scalar, found unknown escape character.", start);
								}
								break;
						}

						Skip();
						Skip();

						/* Consume an arbitrary escape code. */

						if (codeLength > 0)
						{
							uint character = 0;

							/* Scan the character value. */

							for (int k = 0; k < codeLength; ++k) {
								if (!IsHex(k)) {
									throw new SyntaxErrorException("While parsing a quoted scalar, did not find expected hexdecimal number.", start);
								}
								character = (uint)((character << 4) + AsHex(k));
							}

							/* Check the value and write the character. */

							if ((character >= 0xD800 && character <= 0xDFFF) || character > 0x10FFFF) {
								throw new SyntaxErrorException("While parsing a quoted scalar, found invalid Unicode character escape code.", start);
							}

							value.Append((char)character);

							/* Advance the pointer. */

							for (int k = 0; k < codeLength; ++k) {
								Skip();
							}
						}
					}
					else
					{
						/* It is a non-escaped non-blank character. */

						value.Append(ReadCurrentCharacter());
					}
				}

				/* Check if we are at the end of the scalar. */

				if (Check(isSingleQuoted ? '\'' : '"'))
					break;

				/* Consume blank characters. */

				while (IsBlank() || IsBreak())
				{
					if (IsBlank())
					{
						/* Consume a space or a tab character. */

						if (!hasLeadingBlanks) {
							whitespaces.Append(ReadCurrentCharacter());
						}
						else {
							Skip();
						}
					}
					else
					{
						/* Check if it is a first line break. */

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

				/* Join the whitespaces or fold line breaks. */

				if (hasLeadingBlanks)
				{
					/* Do we need to fold line breaks? */

					if (leadingBreak.Length > 0 && leadingBreak[0] == '\n') {
						if (trailingBreaks.Length == 0) {
							value.Append(' ');
						}
						else {
							value.Append(trailingBreaks.ToString());
						}
					}
					else {
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

			/* Eat the right quote. */

			Skip();

			return new Scalar(value.ToString(), isSingleQuoted ? ScalarStyle.SingleQuoted : ScalarStyle.DoubleQuoted);
		}

		/*
		 * Produce the SCALAR(...,plain) token.
		 */

		private void yaml_parser_fetch_plain_scalar()
		{
			/* A plain scalar could be a simple key. */

			yaml_parser_save_simple_key();

			/* A simple key cannot follow a flow scalar. */

			simpleKeyAllowed = false;

			/* Create the SCALAR token and append it to the queue. */

				tokens.Enqueue(yaml_parser_scan_plain_scalar());
		}
		
		/*
		 * Scan a plain scalar.
		 */

			private Token yaml_parser_scan_plain_scalar()
		{
			StringBuilder value = new StringBuilder();
			StringBuilder whitespaces = new StringBuilder();
			StringBuilder leadingBreak = new StringBuilder();
			StringBuilder trailingBreaks = new StringBuilder();
			
			bool hasLeadingBlanks = false;
			int currentIndent = indent + 1;

			Mark start = mark;
			Mark end = mark;

			/* Consume the content of the plain scalar. */

			for(;;)
			{
				/* Check for a document indicator. */

				if (IsDocumentIndicator()) {
					break;
				}

				/* Check for a comment. */

				if (Check('#')) {
					break;
				}

				/* Consume non-blank characters. */
				while (!IsBlankOrBreakOrZero())
				{
					/* Check for 'x:x' in the flow context. TODO: Fix the test "spec-08-13". */

					if (flowLevel > 0 && Check(':') && !IsBlankOrBreakOrZero(1)) {
						throw new SyntaxErrorException("While scanning a plain scalar, found unexpected ':'.", start);
					}

					/* Check for indicators that may end a plain scalar. */

					if ((Check(':') && IsBlankOrBreakOrZero(1)) || (flowLevel > 0 && Check(",:?[]{}"))) {
						break;
					}

					/* Check if we need to join whitespaces and breaks. */

					if (hasLeadingBlanks || whitespaces.Length > 0)
					{
						if (hasLeadingBlanks)
						{
							/* Do we need to fold line breaks? */

							if (leadingBreak.Length > 0 && leadingBreak[0] == '\n') {
								if (trailingBreaks.Length == 0) {
									value.Append(' ');
								}
								else {
									value.Append(trailingBreaks);
								}
							}
							else {
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

					/* Copy the character. */

					value.Append(ReadCurrentCharacter());

					end = mark;
				}

				/* Is it the end? */

				if (!(IsBlank() || IsBreak())) {
					break;
				}

				/* Consume blank characters. */

				while (IsBlank() || IsBreak())
				{
					if (IsBlank())
					{
						/* Check for tab character that abuse intendation. */

						if (hasLeadingBlanks && mark.Column < indent && IsTab()) {
							throw new SyntaxErrorException("While scanning a plain scalar, found a tab character that violate intendation.", start);
						}

						/* Consume a space or a tab character. */

						if (!hasLeadingBlanks) {
							value.Append(ReadCurrentCharacter());
						}
						else {
							Skip();
						}
					}
					else
					{
						/* Check if it is a first line break. */

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

				/* Check intendation level. */

				if (flowLevel == 0 && mark.Column < indent) {
					break;
				}
			}

			/* Note that we change the 'simple_key_allowed' flag. */

			if (hasLeadingBlanks) {
				simpleKeyAllowed = true;
			}

			/* Create a token. */

			return new Scalar(value.ToString(), ScalarStyle.Plain, start, end);
		}

		
		/*
		 * Remove a potential simple key at the current flow level.
		 */

		private void yaml_parser_remove_simple_key()
		{
			SimpleKey key = simpleKeys.Peek();

			if (key.IsPossible && key.IsRequired)
			{
				/* If the key is required, it is an error. */

				throw new SyntaxErrorException("While scanning a simple key, could not found expected ':'.", key.Mark);
			}

			/* Remove the key from the stack. */

			key.IsPossible = false;
		}

		/*
		 * Scan the directive name.
		 *
		 * Scope:
		 *      %YAML   1.1     # a comment \n
		 *       ^^^^
		 *      %TAG    !yaml!  tag:yaml.org,2002:  \n
		 *       ^^^
		 */

		private string yaml_parser_scan_directive_name(Mark start) {
			StringBuilder name = new StringBuilder();

			/* Consume the directive name. */

			buffer.Cache(1);
			
			while (IsAlpha())
			{
				name.Append(ReadCurrentCharacter());
				buffer.Cache(1);
			}

			/* Check if the name is empty. */

			if(name.Length == 0) {
				throw new SyntaxErrorException("While scanning a directive, could not find expected directive name.", start);
			}

			/* Check for an blank character after the name. */

			if (!IsBlankOrBreakOrZero()) {
				throw new SyntaxErrorException("While scanning a directive, found unexpected non-alphabetical character.", start);
			}

			return name.ToString();
		}
		
		private void SkipWhitespaces() {
			/* Eat whitespaces. */

			buffer.Cache(1);

			while (IsBlank()) {
				Skip();
				buffer.Cache(1);
			}
		}
		
		/*
		 * Scan the value of VERSION-DIRECTIVE.
		 *
		 * Scope:
		 *      %YAML   1.1     # a comment \n
		 *           ^^^^^^
		 */

		private Token yaml_parser_scan_version_directive_value(Mark start)
		{
			SkipWhitespaces();

			/* Consume the major version number. */

			int major = yaml_parser_scan_version_directive_number(start);

			/* Eat '.'. */

			if (!Check('.')) {
				throw new SyntaxErrorException("While scanning a %YAML directive, did not find expected digit or '.' character.", start);
			}

			Skip();

			/* Consume the minor version number. */

			int minor = yaml_parser_scan_version_directive_number(start);

			return new VersionDirective(new Version(major, minor), start, start);
		}

		/*
		 * Scan the value of a TAG-DIRECTIVE token.
		 *
		 * Scope:
		 *      %TAG    !yaml!  tag:yaml.org,2002:  \n
		 *          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
		 */

		private Token yaml_parser_scan_tag_directive_value(Mark start)
		{
			SkipWhitespaces();

			/* Scan a handle. */

			string handle = yaml_parser_scan_tag_handle(true, start);

			/* Expect a whitespace. */

			buffer.Cache(1);
			
			if (!IsBlank()) {
				throw new SyntaxErrorException("While scanning a %TAG directive, did not find expected whitespace.", start);
			}

			SkipWhitespaces();

			/* Scan a prefix. */

			string prefix = yaml_parser_scan_tag_uri(true, null, start);
			
			/* Expect a whitespace or line break. */

			buffer.Cache(1);

			if (!IsBlankOrBreakOrZero()) {
				throw new SyntaxErrorException("While scanning a %TAG directive, did not find expected whitespace or line break.", start);
			}

			return new TagDirective(handle, prefix, start, start);
		}
		
		/*
		 * Scan a tag.
		 */

		private string yaml_parser_scan_tag_uri(bool isDirective, string head, Mark start) {
			StringBuilder tag = new StringBuilder();
			if(head != null && head.Length > 1) {
				tag.Append(head.Substring(1));
			}
			
			/* Scan the tag. */

			buffer.Cache(1);

			/*
			 * The set of characters that may appear in URI is as follows:
			 *
			 *      '0'-'9', 'A'-'Z', 'a'-'z', '_', '-', ';', '/', '?', ':', '@', '&',
			 *      '=', '+', '$', ',', '.', '!', '~', '*', '\'', '(', ')', '[', ']',
			 *      '%'.
			 */

			while (IsAlpha() || Check(";/?:@&=+$,.!~*'()[]%"))
			{
				/* Check if it is a URI-escape sequence. */

				if (Check('%')) {
					tag.Append(yaml_parser_scan_uri_escapes(start));
				}
				else {
					tag.Append(ReadCurrentCharacter());
				}

				buffer.Cache(1);
			}

			/* Check if the tag is non-empty. */

			if (tag.Length == 0) {
				throw new SyntaxErrorException("While parsing a tag, did not find expected tag URI.", start);
			}

			return tag.ToString();
		}

		/*
		 * Decode an URI-escape sequence corresponding to a single UTF-8 character.
		 */

		private char yaml_parser_scan_uri_escapes(Mark start)
		{
			/* Decode the required number of characters. */

			List<byte> charBytes = new List<byte>();
			int width = 0;
			do {
				/* Check for a URI-escaped octet. */

				buffer.Cache(3);

				if (!(Check('%') && IsHex(1) && IsHex(2))) {
					throw new SyntaxErrorException("While parsing a tag, did not find URI escaped octet.", start);
				}

				/* Get the octet. */

				int octet = (AsHex(1) << 4) + AsHex(2);

				/* If it is the leading octet, determine the length of the UTF-8 sequence. */

				if (width == 0)
				{
					width = (octet & 0x80) == 0x00 ? 1 :
							(octet & 0xE0) == 0xC0 ? 2 :
							(octet & 0xF0) == 0xE0 ? 3 :
							(octet & 0xF8) == 0xF0 ? 4 : 0;
					
					if (width == 0) {
						throw new SyntaxErrorException("While parsing a tag, found an incorrect leading UTF-8 octet.", start);
					}
				}
				else
				{
					/* Check if the trailing octet is correct. */

					if ((octet & 0xC0) != 0x80) {
						throw new SyntaxErrorException("While parsing a tag, found an incorrect trailing UTF-8 octet.", start);
					}
				}

				/* Copy the octet and move the pointers. */

				charBytes.Add((byte)octet);
				
				Skip();
				Skip();
				Skip();
			} while (--width > 0);

			char[] characters = Encoding.UTF8.GetChars(charBytes.ToArray());
			
			if(characters.Length != 1) {
				throw new SyntaxErrorException("While parsing a tag, found an incorrect UTF-8 sequence.", start);
			}
			
			return characters[0];
		}

		/*
		 * Scan a tag handle.
		 */

		private string yaml_parser_scan_tag_handle(bool isDirective, Mark start) {

			/* Check the initial '!' character. */

			buffer.Cache(1);
			
			if (!Check('!')) {
				throw new SyntaxErrorException("While scanning a tag, did not find expected '!'.", start);
			}

			/* Copy the '!' character. */

			StringBuilder tagHandle = new StringBuilder();
			tagHandle.Append(ReadCurrentCharacter());

			/* Copy all subsequent alphabetical and numerical characters. */

			buffer.Cache(1);
			while (IsAlpha())
			{
				tagHandle.Append(ReadCurrentCharacter());
				buffer.Cache(1);
			}

			/* Check if the trailing character is '!' and copy it. */

			if (Check('!'))
			{
				tagHandle.Append(ReadCurrentCharacter());
			}
			else
			{
				/*
				 * It's either the '!' tag or not really a tag handle.  If it's a %TAG
				 * directive, it's an error.  If it's a tag token, it must be a part of
				 * URI.
				 */

				if (isDirective && (tagHandle.Length != 1  || tagHandle[0] != '!')) {
					throw new SyntaxErrorException("While parsing a tag directive, did not find expected '!'.", start);
				}
			}

			return tagHandle.ToString();
		}
		
		/*
		 * Scan the version number of VERSION-DIRECTIVE.
		 *
		 * Scope:
		 *      %YAML   1.1     # a comment \n
		 *              ^
		 *      %YAML   1.1     # a comment \n
		 *                ^
		 */

		private int yaml_parser_scan_version_directive_number(Mark start)
		{
			int value = 0;
			int length = 0;

			/* Repeat while the next character is digit. */

			buffer.Cache(1);

			while (IsDigit())
			{
				/* Check if the number is too long. */

				if (++length > MaxVersionNumberLength) {
					throw new SyntaxErrorException("While scanning a %YAML directive, found extremely long version number.", start);
				}

				value = value * 10 + AsDigit();

				Skip();

				buffer.Cache(1);
			}

			/* Check if the number was present. */

			if (length == 0) {
				throw new SyntaxErrorException("While scanning a %YAML directive, did not find expected version number.", start);
			}
		
			return value;
		}

		/*
		 * Check if a simple key may start at the current position and add it if
		 * needed.
		 */

		private void yaml_parser_save_simple_key()
		{
			/*
			 * A simple key is required at the current position if the scanner is in
			 * the block context and the current column coincides with the indentation
			 * level.
			 */

			bool isRequired = (flowLevel == 0 && indent == mark.Column);

			/*
			 * A simple key is required only when it is the first token in the current
			 * line.  Therefore it is always allowed.  But we add a check anyway.
			 */

			Debug.Assert(simpleKeyAllowed || !isRequired, "Can't require a simple key and disallow it at the same time.");    /* Impossible. */

			/*
			 * If the current position may start a simple key, save it.
			 */

			if (simpleKeyAllowed)
			{
				SimpleKey key = new SimpleKey(true, isRequired, tokensParsed + tokens.Count, mark);

				yaml_parser_remove_simple_key();

				simpleKeys.Pop();
				simpleKeys.Push(key);
			}
		}
	}
}
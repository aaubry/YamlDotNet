using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.Core.Tokens;
using Event = YamlDotNet.Core.Events.ParsingEvent;
using SequenceStyle = YamlDotNet.Core.Events.SequenceStyle;
using MappingStyle = YamlDotNet.Core.Events.MappingStyle;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Parses YAML streams.
	/// </summary>
	public class Parser
	{
		private readonly Stack<ParserState> states = new Stack<ParserState>();
		private TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
		private ParserState state;

		private readonly Scanner scanner;
		private Event current;

		private Token currentToken;
		
		public static void DisplayCaller(int count) {
			StackFrame callee = new StackFrame(1);
			Console.Write(callee.GetMethod().Name);
			
			for(int i = 0; i < count; ++i) {
				StackFrame caller = new StackFrame(2 + i);
				Console.Write(" <= {0}", caller.GetMethod().Name);
			}
			Console.WriteLine();
		}
		
		
		private Token GetCurrentToken()
		{
			if(currentToken == null) {
				if(scanner.InternalMoveNext()) {
					currentToken = scanner.Current;
				}
			}
			return currentToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Parser"/> class.
		/// </summary>
		/// <param name="input">The input where the YAML stream is to be read.</param>
		public Parser(TextReader input)
		{
			scanner = new Scanner(input);
		}

		/// <summary>
		/// Gets the current event.
		/// </summary>
		public Event Current
		{
			get
			{
				return current;
			}
		}

		/// <summary>
		/// Moves to the next event.
		/// </summary>
		/// <returns>Returns true if there are more events available, otherwise returns false.</returns>
		public bool MoveNext()
		{
			/* No events after the end of the stream or error. */
			if (state == ParserState.YAML_PARSE_END_STATE)
			{
				current = null;
				return false;
			}
			else
			{
				/* Generate the next event. */
				current = yaml_parser_state_machine();
				return true;
			}
		}

		Event yaml_parser_state_machine()
		{
			Console.WriteLine("STATE({0}) {1}", (int)state, state);
			
			switch (state)
			{
				case ParserState.YAML_PARSE_STREAM_START_STATE:
					return yaml_parser_parse_stream_start();

				case ParserState.YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE:
					return yaml_parser_parse_document_start(true);

				case ParserState.YAML_PARSE_DOCUMENT_START_STATE:
					return yaml_parser_parse_document_start(false);

				case ParserState.YAML_PARSE_DOCUMENT_CONTENT_STATE:
					return yaml_parser_parse_document_content();

				case ParserState.YAML_PARSE_DOCUMENT_END_STATE:
					return yaml_parser_parse_document_end();

				case ParserState.YAML_PARSE_BLOCK_NODE_STATE:
					return yaml_parser_parse_node(true, false);

				case ParserState.YAML_PARSE_BLOCK_NODE_OR_INDENTLESS_SEQUENCE_STATE:
					return yaml_parser_parse_node(true, true);

				case ParserState.YAML_PARSE_FLOW_NODE_STATE:
					return yaml_parser_parse_node(false, false);

				case ParserState.YAML_PARSE_BLOCK_SEQUENCE_FIRST_ENTRY_STATE:
					return yaml_parser_parse_block_sequence_entry(true);

				case ParserState.YAML_PARSE_BLOCK_SEQUENCE_ENTRY_STATE:
					return yaml_parser_parse_block_sequence_entry(false);

				case ParserState.YAML_PARSE_INDENTLESS_SEQUENCE_ENTRY_STATE:
					return yaml_parser_parse_indentless_sequence_entry();

				case ParserState.YAML_PARSE_BLOCK_MAPPING_FIRST_KEY_STATE:
					return yaml_parser_parse_block_mapping_key(true);

				case ParserState.YAML_PARSE_BLOCK_MAPPING_KEY_STATE:
					return yaml_parser_parse_block_mapping_key(false);

				case ParserState.YAML_PARSE_BLOCK_MAPPING_VALUE_STATE:
					return yaml_parser_parse_block_mapping_value();

				case ParserState.YAML_PARSE_FLOW_SEQUENCE_FIRST_ENTRY_STATE:
					return yaml_parser_parse_flow_sequence_entry(true);

				case ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_STATE:
					return yaml_parser_parse_flow_sequence_entry(false);

				case ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_KEY_STATE:
					return yaml_parser_parse_flow_sequence_entry_mapping_key();

				case ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_VALUE_STATE:
					return yaml_parser_parse_flow_sequence_entry_mapping_value();

				case ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_END_STATE:
					return yaml_parser_parse_flow_sequence_entry_mapping_end();

				case ParserState.YAML_PARSE_FLOW_MAPPING_FIRST_KEY_STATE:
					return yaml_parser_parse_flow_mapping_key(true);

				case ParserState.YAML_PARSE_FLOW_MAPPING_KEY_STATE:
					return yaml_parser_parse_flow_mapping_key(false);

				case ParserState.YAML_PARSE_FLOW_MAPPING_VALUE_STATE:
					return yaml_parser_parse_flow_mapping_value(false);

				case ParserState.YAML_PARSE_FLOW_MAPPING_EMPTY_VALUE_STATE:
					return yaml_parser_parse_flow_mapping_value(true);

				default:
					Debug.Assert(false, "Invalid state");      /* Invalid state. */
					throw new NotImplementedException();
			}
		}

		private void Skip()
		{
			if(currentToken != null) {
				currentToken = null;
				scanner.ConsumeCurrent();
			}
		}

		/*
		 * Parse the production:
		 * stream   ::= STREAM-START implicit_document? explicit_document* STREAM-END
		 *              ************
		 */
		private Event yaml_parser_parse_stream_start()
		{
			Tokens.StreamStart streamStart = GetCurrentToken() as Tokens.StreamStart;
			if(streamStart == null) {
				throw new ParserException("Did not found expected <stream-start>.", GetCurrentToken().Start);
			}
			Skip();

			state = ParserState.YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE;
			return new Events.StreamStart(streamStart.Start, streamStart.End);
		}

		/*
		 * Parse the productions:
		 * implicit_document    ::= block_node DOCUMENT-END*
		 *                          *
		 * explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
		 *                          *************************
		 */
		private Event yaml_parser_parse_document_start(bool isImplicit)
		{
			/* Parse extra document end indicators. */

			if (!isImplicit)
			{
				while (GetCurrentToken() is DocumentEnd)
				{
					Skip();
				}
			}

			/* Parse an isImplicit document. */

			if (isImplicit && !(GetCurrentToken() is VersionDirective || GetCurrentToken() is TagDirective || GetCurrentToken() is DocumentStart || GetCurrentToken() is StreamEnd))
			{
				TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
				yaml_parser_process_directives(tagDirectives);

				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);

				state = ParserState.YAML_PARSE_BLOCK_NODE_STATE;
				
				return new Events.DocumentStart(null, tagDirectives, true, GetCurrentToken().Start, GetCurrentToken().End);
			}

			/* Parse an explicit document. */

			else if (!(GetCurrentToken() is StreamEnd))
			{
				Mark start = GetCurrentToken().Start;
				TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
				VersionDirective versionDirective = yaml_parser_process_directives(tagDirectives);

				if (!(GetCurrentToken() is DocumentStart))
				{
					throw new ParserException("Did not found expected <document start>.", GetCurrentToken().Start);
				}

				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);

				state = ParserState.YAML_PARSE_DOCUMENT_CONTENT_STATE;

				Event evt = new Events.DocumentStart(versionDirective, tagDirectives, false, start, GetCurrentToken().End);
				Skip();
				return evt;
			}

			/* Parse the stream end. */

			else
			{
				state = ParserState.YAML_PARSE_END_STATE;

				Event evt = new Events.StreamEnd(GetCurrentToken().Start, GetCurrentToken().End);
				// Do not call skip here because that would throw an exception
				if (scanner.InternalMoveNext())
				{
					throw new InvalidOperationException("The scanner should contain no more tokens.");
				}
				return evt;
			}
		}

		/*
		 * Parse directives.
		 */
		private VersionDirective yaml_parser_process_directives(TagDirectiveCollection tags)
		{
			VersionDirective version = null;

			while (true)
			{
				VersionDirective currentVersion;
				TagDirective tag;

				if ((currentVersion = GetCurrentToken() as VersionDirective) != null)
				{
					if (version != null)
					{
						throw new ParserException("Found duplicate %YAML directive.", currentVersion.Start);
					}

					if (currentVersion.Version.Major != 1 || currentVersion.Version.Minor != 1)
					{
						throw new ParserException("Found incompatible YAML document.", currentVersion.Start);
					}

					version = currentVersion;
				}
				else if ((tag = GetCurrentToken() as TagDirective) != null)
				{
					if (tagDirectives.Contains(tag.Handle))
					{
						throw new ParserException("Found duplicate %TAG directive.", tag.Start);
					}
					tagDirectives.Add(tag);
					if (tags != null)
					{
						tags.Add(tag);
					}
				}
				else
				{
					break;
				}

				Skip();
			}

			if(tags != null) {
				AddDefaultTagDirectives(tags);
			}
			AddDefaultTagDirectives(tagDirectives);

			return version;
		}

		private static void AddDefaultTagDirectives(TagDirectiveCollection directives) {
			foreach (TagDirective directive in defaultTagDirectives) {
				if(!directives.Contains(directive.Handle)) {
					directives.Add(directive);
				}
			}
		}
		
		private static readonly TagDirective[] defaultTagDirectives = new TagDirective[] {
			new TagDirective("!", "!"),
			new TagDirective("!!", "tag:yaml.org,2002:"),
		};
		
		/*
		 * Parse the productions:
		 * explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
		 *                                                    ***********
		 */
		private Event yaml_parser_parse_document_content()
		{
			if (
				GetCurrentToken() is VersionDirective ||
				GetCurrentToken() is TagDirective ||
				GetCurrentToken() is DocumentStart ||
				GetCurrentToken() is DocumentEnd ||
				GetCurrentToken() is StreamEnd
			) {
				state = states.Pop();
				return yaml_parser_process_empty_scalar(scanner.CurrentPosition);
			}
			else {
				return yaml_parser_parse_node(true, false);
			}
		}

		/*
		 * Generate an empty scalar event.
		 */
		private Event yaml_parser_process_empty_scalar(Mark position)
		{
			return new Events.Scalar(null, null, string.Empty, ScalarStyle.Plain, true, false, position, position);
		}

		/*
		 * Parse the productions:
		 * block_node_or_indentless_sequence    ::=
		 *                          ALIAS
		 *                          *****
		 *                          | properties (block_content | indentless_block_sequence)?
		 *                            **********  *
		 *                          | block_content | indentless_block_sequence
		 *                            *
		 * block_node           ::= ALIAS
		 *                          *****
		 *                          | properties block_content?
		 *                            ********** *
		 *                          | block_content
		 *                            *
		 * flow_node            ::= ALIAS
		 *                          *****
		 *                          | properties flow_content?
		 *                            ********** *
		 *                          | flow_content
		 *                            *
		 * properties           ::= TAG ANCHOR? | ANCHOR TAG?
		 *                          *************************
		 * block_content        ::= block_collection | flow_collection | SCALAR
		 *                                                               ******
		 * flow_content         ::= flow_collection | SCALAR
		 *                                            ******
		 */
		private Event yaml_parser_parse_node(bool isBlock, bool isIndentlessSequence)
		{
			AnchorAlias alias = GetCurrentToken() as AnchorAlias;
			if (alias != null)
			{
				state = states.Pop();
				Event evt = new Events.AnchorAlias(alias.Value, alias.Start, alias.End);
				Skip();
				return evt;
			}

			Mark start = GetCurrentToken().Start;

			Anchor anchor = null;
			Tag tag = null;

			// The anchor and the tag can be in any order. This loop repeats at most twice.
			while(true)
			{
				if(anchor == null && (anchor = GetCurrentToken() as Anchor) != null)
				{
					Skip();
				}
				else if(tag == null && (tag = GetCurrentToken() as Tag) != null)
				{
					Skip();
				}
				else
				{
					break;
				}
			}

			string tagName = null;
			if(tag != null) {
				if(string.IsNullOrEmpty(tag.Handle)) {
					tagName = tag.Suffix;
				} else if(tagDirectives.Contains(tag.Handle)) {
					tagName = string.Concat(tagDirectives[tag.Handle].Prefix, tag.Suffix);
				} else {
					throw new ParserException("While parsing a node, found undefined tag handle.", tag.Start);
				}
			}
			if(string.IsNullOrEmpty(tagName)) {
				tagName = null;
			}

			string anchorName = anchor != null ? string.IsNullOrEmpty(anchor.Value) ? null : anchor.Value : null;

			bool isImplicit = string.IsNullOrEmpty(tagName);

			if (isIndentlessSequence && GetCurrentToken() is BlockEntry) {
				state = ParserState.YAML_PARSE_INDENTLESS_SEQUENCE_ENTRY_STATE;

				return new Events.SequenceStart(
					anchorName,
					tagName,
					isImplicit,
					SequenceStyle.Block,
					start,
					GetCurrentToken().End
				);
			}
			else
			{
				Scalar scalar = GetCurrentToken() as Scalar;
				if(scalar != null) {
					bool isPlainImplicit = false;
					bool isQuotedImplicit = false;
					if((scalar.Style == ScalarStyle.Plain && tagName == null) || tagName == "!") {
						isPlainImplicit = true;
					} else if(tagName == null) {
						isQuotedImplicit = true;
					}

					state = states.Pop();
					Event evt = new Events.Scalar(anchorName, tagName, scalar.Value, scalar.Style, isPlainImplicit, isQuotedImplicit, start, scalar.End);

					Skip();
					return evt;
				}

				FlowSequenceStart flowSequenceStart = GetCurrentToken() as FlowSequenceStart;
				if(flowSequenceStart != null) {
					state = ParserState.YAML_PARSE_FLOW_SEQUENCE_FIRST_ENTRY_STATE;
					return new Events.SequenceStart(anchorName, tagName, isImplicit, SequenceStyle.Flow, start, flowSequenceStart.End);
				}

				FlowMappingStart flowMappingStart = GetCurrentToken() as FlowMappingStart;
				if(flowMappingStart != null) {
					state = ParserState.YAML_PARSE_FLOW_MAPPING_FIRST_KEY_STATE;
					return new Events.MappingStart(anchorName, tagName, isImplicit, MappingStyle.Flow, start, flowMappingStart.End);
				}

				if(isBlock) {
					BlockSequenceStart blockSequenceStart = GetCurrentToken() as BlockSequenceStart;
					if(blockSequenceStart != null) {
						state = ParserState.YAML_PARSE_BLOCK_SEQUENCE_FIRST_ENTRY_STATE;
						return new Events.SequenceStart(anchorName, tagName, isImplicit, SequenceStyle.Block, start, blockSequenceStart.End);
					}
					
					BlockMappingStart blockMappingStart = GetCurrentToken() as BlockMappingStart;
					if (blockMappingStart != null) {
						state = ParserState.YAML_PARSE_BLOCK_MAPPING_FIRST_KEY_STATE;
						return new Events.MappingStart(anchorName, tagName, isImplicit, MappingStyle.Block, start, GetCurrentToken().End);
					}
				}

				if(anchorName != null || tag != null) {
					state = states.Pop();
					return new Events.Scalar(anchorName, tagName, string.Empty, ScalarStyle.Plain, isImplicit, false, start, GetCurrentToken().End);
				}
				
				throw new ParserException("While parsing a node, did not found expected node content.", GetCurrentToken().Start);
			}
		}

		/*
		 * Parse the productions:
		 * implicit_document    ::= block_node DOCUMENT-END*
		 *                                     *************
		 * explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
		 *                                                                *************
		 */

		private Event yaml_parser_parse_document_end()
		{
			bool isImplicit = true;
			Mark start = GetCurrentToken().Start;
			Mark end = start;

			if (GetCurrentToken() is DocumentEnd) {
				end = GetCurrentToken().End;
				Skip();
				isImplicit = false;
			}

			tagDirectives.Clear();

			state = ParserState.YAML_PARSE_DOCUMENT_START_STATE;
			return new Events.DocumentEnd(isImplicit, start, end);
		}

		/*
		 * Parse the productions:
		 * block_sequence ::= BLOCK-SEQUENCE-START (BLOCK-ENTRY block_node?)* BLOCK-END
		 *                    ********************  *********** *             *********
		 */

		private Event yaml_parser_parse_block_sequence_entry(bool isFirst)
		{
			if (isFirst) {
				GetCurrentToken();
				Skip();
			}

			if (GetCurrentToken() is BlockEntry)
			{
				Mark mark = GetCurrentToken().End;
				
				Skip();
				if(!(GetCurrentToken() is BlockEntry || GetCurrentToken() is BlockEnd)) {
					states.Push(ParserState.YAML_PARSE_BLOCK_SEQUENCE_ENTRY_STATE);
					return yaml_parser_parse_node(true, false);
				}
				else {
					state = ParserState.YAML_PARSE_BLOCK_SEQUENCE_ENTRY_STATE;
					return yaml_parser_process_empty_scalar(mark);
				}
			}

			else if (GetCurrentToken() is BlockEnd)
			{
				state = states.Pop();
				Event evt = new Events.SequenceEnd(GetCurrentToken().Start, GetCurrentToken().End);
				Skip();
				return evt;
			}

			else
			{
				throw new ParserException("While parsing a block collection, did not found expected '-' indicator.", GetCurrentToken().Start);
			}
		}

		private Event yaml_parser_parse_indentless_sequence_entry()
		{
			throw new NotImplementedException();
		}

		/*
		 * Parse the productions:
		 * block_mapping        ::= BLOCK-MAPPING_START
		 *                          *******************
		 *                          ((KEY block_node_or_indentless_sequence?)?
		 *                            *** *
		 *                          (VALUE block_node_or_indentless_sequence?)?)*
		 *
		 *                          BLOCK-END
		 *                          *********
		 */
		private Event yaml_parser_parse_block_mapping_key(bool isFirst)
		{
			if (isFirst) {
				GetCurrentToken();
				Skip();
			}

			if (GetCurrentToken() is Key)
			{
				Mark mark = GetCurrentToken().End;
				Skip();
				if(!(GetCurrentToken() is Key || GetCurrentToken() is Value || GetCurrentToken() is BlockEnd)) {
					states.Push(ParserState.YAML_PARSE_BLOCK_MAPPING_VALUE_STATE);
					return yaml_parser_parse_node(true, true);
				}
				else {
					state = ParserState.YAML_PARSE_BLOCK_MAPPING_VALUE_STATE;
					return yaml_parser_process_empty_scalar(mark);
				}
			}

			else if (GetCurrentToken() is BlockEnd)
			{
				state = states.Pop();
				Event evt = new Events.MappingEnd(GetCurrentToken().Start, GetCurrentToken().End);
				Skip();
				return evt;
			}

			else
			{
				throw new ParserException("While parsing a block mapping, did not found expected key.", GetCurrentToken().Start);
			}
		}

		/*
		 * Parse the productions:
		 * block_mapping        ::= BLOCK-MAPPING_START
		 *
		 *                          ((KEY block_node_or_indentless_sequence?)?
		 *
		 *                          (VALUE block_node_or_indentless_sequence?)?)*
		 *                           ***** *
		 *                          BLOCK-END
		 *
		 */
		private Event yaml_parser_parse_block_mapping_value()
		{
			if (GetCurrentToken() is Value)
			{
				Mark mark = GetCurrentToken().End;
				Skip();
				
				if(!(GetCurrentToken() is Key || GetCurrentToken() is Value || GetCurrentToken() is BlockEnd)) {
					states.Push(ParserState.YAML_PARSE_BLOCK_MAPPING_KEY_STATE);
					return yaml_parser_parse_node(true, true);
				}
				else {
					state = ParserState.YAML_PARSE_BLOCK_MAPPING_KEY_STATE;
					return yaml_parser_process_empty_scalar(mark);
				}
			}

			else
			{
				state = ParserState.YAML_PARSE_BLOCK_MAPPING_KEY_STATE;
				return yaml_parser_process_empty_scalar(GetCurrentToken().Start);
			}
		}

		/*
		 * Parse the productions:
		 * flow_sequence        ::= FLOW-SEQUENCE-START
		 *                          *******************
		 *                          (flow_sequence_entry FLOW-ENTRY)*
		 *                           *                   **********
		 *                          flow_sequence_entry?
		 *                          *
		 *                          FLOW-SEQUENCE-END
		 *                          *****************
		 * flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
		 *                          *
		 */
		private Event yaml_parser_parse_flow_sequence_entry(bool isFirst)
		{
			if (isFirst) {
				GetCurrentToken();
				Skip();
			}

			Event evt;
			if (!(GetCurrentToken() is FlowSequenceEnd))
			{
				if (!isFirst) {
					if (GetCurrentToken() is FlowEntry) {
						Skip();
					}
					else {
						throw new ParserException("While parsing a flow sequence, did not found expected ',' or ']'.", GetCurrentToken().Start);
					}
				}

				if(GetCurrentToken() is Key) {
					state = ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_KEY_STATE;
					evt = new Events.MappingStart(null, null, true, MappingStyle.Flow);
					Skip();
					return evt;
				} else if (!(GetCurrentToken() is FlowSequenceEnd)) {
					states.Push(ParserState.YAML_PARSE_FLOW_SEQUENCE_ENTRY_STATE);
					return yaml_parser_parse_node(false, false);
				}
			}
			
			state = states.Pop();
			evt = new Events.SequenceEnd(GetCurrentToken().Start, GetCurrentToken().End);
			Skip();
			return evt;
		}

		private Event yaml_parser_parse_flow_sequence_entry_mapping_key()
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_flow_sequence_entry_mapping_value()
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_flow_sequence_entry_mapping_end()
		{
			throw new NotImplementedException();
		}

		/*
		 * Parse the productions:
		 * flow_mapping         ::= FLOW-MAPPING-START
		 *                          ******************
		 *                          (flow_mapping_entry FLOW-ENTRY)*
		 *                           *                  **********
		 *                          flow_mapping_entry?
		 *                          ******************
		 *                          FLOW-MAPPING-END
		 *                          ****************
		 * flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
		 *                          *           *** *
		 */
		private Event yaml_parser_parse_flow_mapping_key(bool isFirst)
		{
			if (isFirst) {
				GetCurrentToken();
				Skip();
			}

			if (!(GetCurrentToken() is FlowMappingEnd))
			{
				if (!isFirst) {
					if (GetCurrentToken() is FlowEntry) {
						Skip();
					}
					else {
						throw new ParserException("While parsing a flow mapping,  did not found expected ',' or '}'.", GetCurrentToken().Start);
					}
				}

				if (GetCurrentToken() is Key) {
					Skip();
					
					if(!(GetCurrentToken() is Value || GetCurrentToken() is FlowEntry || GetCurrentToken() is FlowMappingEnd)) {
						states.Push(ParserState.YAML_PARSE_FLOW_MAPPING_VALUE_STATE);
						return yaml_parser_parse_node(false, false);
					}
					else {
						state = ParserState.YAML_PARSE_FLOW_MAPPING_VALUE_STATE;
						return yaml_parser_process_empty_scalar(GetCurrentToken().Start);
					}
				}
				else if (!(GetCurrentToken() is FlowMappingEnd)) {
					states.Push(ParserState.YAML_PARSE_FLOW_MAPPING_EMPTY_VALUE_STATE);
					return yaml_parser_parse_node(false, false);
				}
			}

			state = states.Pop();
			Event evt = new Events.MappingEnd(GetCurrentToken().Start, GetCurrentToken().End);
			Skip();
			return evt;
		}

		/*
		 * Parse the productions:
		 * flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
		 *                                   *                  ***** *
		 */
		private Event yaml_parser_parse_flow_mapping_value(bool isEmpty)
		{
			if (isEmpty) {
				state = ParserState.YAML_PARSE_FLOW_MAPPING_KEY_STATE;
				return yaml_parser_process_empty_scalar(GetCurrentToken().Start);
			}

			if (GetCurrentToken() is Value) {
				Skip();
				if(!(GetCurrentToken() is FlowEntry || GetCurrentToken() is FlowMappingEnd)) {
					states.Push(ParserState.YAML_PARSE_FLOW_MAPPING_KEY_STATE);
					return yaml_parser_parse_node(false, false);
				}
			}

			state = ParserState.YAML_PARSE_FLOW_MAPPING_KEY_STATE;
			return yaml_parser_process_empty_scalar(GetCurrentToken().Start);
		}
	}
}
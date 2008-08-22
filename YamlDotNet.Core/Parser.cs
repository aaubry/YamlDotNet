using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using YamlDotNet.Core.Tokens;
using Event = YamlDotNet.Core.Events.ParsingEvent;
using SequenceStyle = YamlDotNet.Core.Events.SequenceStyle;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Parses YAML streams.
	/// </summary>
	public class Parser
	{
		private class TagDirectiveCollection : KeyedCollection<string, TagDirective>
		{
			protected override string GetKeyForItem(TagDirective item)
			{
				return item.Handle;
			}
		}

		private Stack<ParserState> states = new Stack<ParserState>();
		private Stack<Mark> marks = new Stack<Mark>();
		private TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
		private ParserState state;

		private readonly Scanner scanner;
		private Event current;

		private Token CurrentToken
		{
			get
			{
				return scanner.Current;
			}
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
			if (!scanner.MoveNext())
			{
				throw new InvalidOperationException("The scanner should contain more token.");
			}
		}

		/*
		 * Parse the production:
		 * stream   ::= STREAM-START implicit_document? explicit_document* STREAM-END
		 *              ************
		 */
		private Event yaml_parser_parse_stream_start()
		{
			Skip();

			Tokens.StreamStart token = Expect<Tokens.StreamStart>();

			state = ParserState.YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE;
			return new Events.StreamStart(token.Start, token.End);
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
				while (Token is DocumentEnd)
				{
					Skip();
				}
			}

			/* Parse an isImplicit document. */

			if (isImplicit && !(CurrentToken is VersionDirective || CurrentToken is TagDirective || CurrentToken is DocumentStart || CurrentToken is StreamEnd))
			{
				yaml_parser_process_directives(null);

				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);

				state = ParserState.YAML_PARSE_BLOCK_NODE_STATE;

				return new Events.DocumentStart(CurrentToken.Start, CurrentToken.End);
			}

			/* Parse an explicit document. */

			else if (!(CurrentToken is StreamEnd))
			{
				Mark start = CurrentToken.Start;
				List<TagDirective> tagDirectives = new List<TagDirective>();
				VersionDirective versionDirective = yaml_parser_process_directives(tagDirectives);

				if (!(CurrentToken is DocumentStart))
				{
					throw new ParserException("Did not found expected <document start>.", CurrentToken.Start);
				}

				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);

				state = ParserState.YAML_PARSE_DOCUMENT_CONTENT_STATE;

				Event evt = new Events.DocumentStart(versionDirective, tagDirectives, start, CurrentToken.End);
				Skip();
				return evt;
			}

			/* Parse the stream end. */

			else
			{
				state = ParserState.YAML_PARSE_END_STATE;

				Event evt = new Events.StreamEnd(CurrentToken.Start, CurrentToken.End);
				// Do not call skip here because that would throw an exception
				if (scanner.MoveNext())
				{
					throw new InvalidOperationException("The scanner should contain no more tokens.");
				}
				return evt;
			}
		}

		/*
		 * Parse directives.
		 */
		private VersionDirective yaml_parser_process_directives(IList<TagDirective> tags)
		{
			VersionDirective version = null;

			while (true)
			{
				VersionDirective currentVersion;
				TagDirective tag;

				if ((currentVersion = CurrentToken as VersionDirective) != null)
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
				else if ((tag = CurrentToken as TagDirective) != null)
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

			if (!tagDirectives.Contains("!"))
			{
				tagDirectives.Add(new TagDirective("!", "!"));
			}

			if (!tagDirectives.Contains("!!"))
			{
				tagDirectives.Add(new TagDirective("!!", "tag:yaml.org,2002:"));
			}

			return version;
		}

		/*
		 * Parse the productions:
		 * explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
		 *                                                    ***********
		 */
		private Event yaml_parser_parse_document_content()
		{
			if (
				CurrentToken is VersionDirective ||
				CurrentToken is TagDirective ||
				CurrentToken is DocumentStart ||
				CurrentToken is DocumentEnd ||
				CurrentToken is StreamEnd
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
			return new Events.Scalar(string.Empty, string.Empty, ScalarStyle.Plain, position, position);
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
			AnchorAlias alias = CurrentToken as AnchorAlias;
			if (alias != null)
			{
				state = states.Pop();
				Event evt = new Events.AnchorAlias(alias.Value, alias.Start, alias.End);
				Skip();
				return evt;
			}

			Mark start = CurrentToken.Start;
			Mark end = start;

			Anchor anchor = null;
			Tag tag = null;

			// The anchor and the tag can be in any order. This loop repeats at most twice.
			while(true)
			{
				if(anchor == null && (anchor = CurrentToken as Anchor) != null)
				{
					Skip();
					end = anchor.End;
				}
				else if(tag == null && (tag = CurrentToken as Tag) != null)
				{
					Skip();
					end = anchor.End;
				}
				else
				{
					break;
				}
			}

			string tagName = null;
			if(tag != null) {
				if(string.IsNullOrEmpty(tag.Handle)) {
					tagName = stag.Suffix;
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

			if (isIndentlessSequence && CurrentToken is BlockEntry) {
				state = ParserState.YAML_PARSE_INDENTLESS_SEQUENCE_ENTRY_STATE;

				return new Events.SequenceStart(
					anchorName,
					tagName,
					isImplicit,
					SequenceStyle.Block,
					start,
					CurrentToken.End
				);
			}
			else
			{
				Scalar scalar = CurrentToken as Scalar;
				if(scalar != null) {
					bool isPlainImplicit = false;
					bool isQuotedImplicit = false;
					if((scalar.Style == ScalarStyle.Plain && tagName == null) || tag == "!") {
						isPlainImplicit = true;
					} else if(tagName == null) {
						isQuotedImplicit = true;
					}

					state = states.Pop();
					Event evt = new Events.Scalar(anchorName, tagName, scalar.Value, scalar.Style, start, scalar.End);
					Skip();
					return evt;
				}

				FlowSequenceStart flowSequenceStart = CurrentToken as FlowSequenceStart;
				if(


				else if (token->type == YAML_FLOW_SEQUENCE_START_TOKEN) {
					end_mark = token->end_mark;
					parser->state = YAML_PARSE_FLOW_SEQUENCE_FIRST_ENTRY_STATE;
					SEQUENCE_START_EVENT_INIT(*event, anchor, tag, isImplicit,
							YAML_FLOW_SEQUENCE_STYLE, start_mark, end_mark);
					return 1;
				}
				else if (token->type == YAML_FLOW_MAPPING_START_TOKEN) {
					end_mark = token->end_mark;
					parser->state = YAML_PARSE_FLOW_MAPPING_FIRST_KEY_STATE;
					MAPPING_START_EVENT_INIT(*event, anchor, tag, isImplicit,
							YAML_FLOW_MAPPING_STYLE, start_mark, end_mark);
					return 1;
				}
				else if (isBlock && token->type == YAML_BLOCK_SEQUENCE_START_TOKEN) {
					end_mark = token->end_mark;
					parser->state = YAML_PARSE_BLOCK_SEQUENCE_FIRST_ENTRY_STATE;
					SEQUENCE_START_EVENT_INIT(*event, anchor, tag, isImplicit,
							YAML_BLOCK_SEQUENCE_STYLE, start_mark, end_mark);
					return 1;
				}
				else if (isBlock && token->type == YAML_BLOCK_MAPPING_START_TOKEN) {
					end_mark = token->end_mark;
					parser->state = YAML_PARSE_BLOCK_MAPPING_FIRST_KEY_STATE;
					MAPPING_START_EVENT_INIT(*event, anchor, tag, isImplicit,
							YAML_BLOCK_MAPPING_STYLE, start_mark, end_mark);
					return 1;
				}
				else if (anchor || tag) {
					yaml_char_t *value = (yaml_char_t*)yaml_malloc(1);
					if (!value) {
						parser->error = YAML_MEMORY_ERROR;
						goto error;
					}
					value[0] = '\0';
					parser->state = POP(parser, parser->states);
					SCALAR_EVENT_INIT(*event, anchor, tag, value, 0,
							isImplicit, 0, YAML_PLAIN_SCALAR_STYLE,
							start_mark, end_mark);
					return 1;
				}
				else {
					yaml_parser_set_parser_error_context(parser,
							(isBlock ? "while parsing a block node"
							 : "while parsing a flow node"), start_mark,
							"did not found expected node content", token->start_mark);
					goto error;
				}
			}

		error:
			yaml_free(anchor);
			yaml_free(tag_handle);
			yaml_free(tag_suffix);
			yaml_free(tag);

			return 0;
		}


		private Event yaml_parser_parse_document_end()
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_node(bool isBlock, bool isIndentlessSequence)
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_block_sequence_entry(bool isFirst)
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_indentless_sequence_entry()
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_block_mapping_key(bool isFirst)
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_block_mapping_value()
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_flow_sequence_entry(bool isFirst)
		{
			throw new NotImplementedException();
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

		private Event yaml_parser_parse_flow_mapping_key(bool isFirst)
		{
			throw new NotImplementedException();
		}

		private Event yaml_parser_parse_flow_mapping_value(bool isEmpty)
		{
			throw new NotImplementedException();
		}
	}
}
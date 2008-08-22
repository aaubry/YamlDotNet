using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using YamlDotNet.Core.Tokens;
using Event = YamlDotNet.Core.Events.Event;

namespace YamlDotNet.Core
{
	public class Parser
	{
		private class TagDirectiveCollection : KeyedCollection<string, TagDirective> {
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
		private Event current = null;
		
		public Parser(TextReader input) {
			scanner = new Scanner(input);
		}
		
		public Event Current {
			get {
				return current;
			}
		}
		
		public bool MoveNext() {
			/* No events after the end of the stream or error. */
			if (state == ParserState.YAML_PARSE_END_STATE) {
				current = null;
				return false;
			} else {
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
		
		private T Expect<T>() where T : Token {
			Token token = scanner.Current;
			T t = token as T;
			if(t == null) {
				throw new ParserException(string.Format(CultureInfo.InvariantCulture, "Did not found expected {0}.", typeof(T).Name), token.Start);
			} else {
				scanner.MoveNext();
				return t;
			}
		}
		
		private void Skip() {
			if(!scanner.MoveNext()) {
				throw new InvalidOperationException("The scanner should contain more token.");
			}
		}
		
		/*
		 * Parse the production:
		 * stream   ::= STREAM-START implicit_document? explicit_document* STREAM-END
		 *              ************
		 */

		private Event yaml_parser_parse_stream_start() {
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
				while (scanner.Current is Tokens.DocumentEnd) {
					Skip();
				}
			}

			/* Parse an isImplicit document. */

			if (isImplicit && !(scanner.Current is VersionDirective || scanner.Current is TagDirective || scanner.Current is DocumentStart || scanner.Current is StreamEnd))
			{
				yaml_parser_process_directives(null);

				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);
				
				state = ParserState.YAML_PARSE_BLOCK_NODE_STATE;

				return new Events.DocumentStart(scanner.Current.Start, scanner.Current.End);
			}

			/* Parse an explicit document. */

			else if (!(scanner.Current is StreamEnd))
			{
				Mark start = scanner.Current.Start;
				List<TagDirective> tagDirectives = new List<TagDirective>();
				VersionDirective versionDirective = yaml_parser_process_directives(tagDirectives);
				
				if(!(scanner.Current is DocumentStart)) {
					throw new ParserException("Did not found expected <document start>.", scanner.Current.Start);
				}
				
				states.Push(ParserState.YAML_PARSE_DOCUMENT_END_STATE);

				state = ParserState.YAML_PARSE_DOCUMENT_CONTENT_STATE;
				
				Event evt = new Events.DocumentStart(versionDirective, tagDirectives, start, scanner.Current.End);
				Skip();
				return evt;
			}

			/* Parse the stream end. */

			else
			{
				state = ParserState.YAML_PARSE_END_STATE;

				Event evt = new Events.StreamEnd(scanner.Current.Start, scanner.Current.End);
				// Do not call skip here because that would throw an exception
				if(scanner.MoveNext()) {
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

			while(true)
			{
				VersionDirective currentVersion;
				TagDirective tag;
				
				if((currentVersion = scanner.Current as VersionDirective) != null) {
					if(version != null) {
						throw new ParserException("Found duplicate %YAML directive.", currentVersion.Start);
					}
					
					if(currentVersion.Version.Major != 1 || currentVersion.Version.Minor != 1) {
						throw new ParserException("Found incompatible YAML document.", currentVersion.Start);
					}

					version = currentVersion;
				} else if((tag = scanner.Current as TagDirective) != null) {
					if (tagDirectives.Contains(tag.Handle)) {
						throw new ParserException("Found duplicate %TAG directive.", tag.Start);
					}
					tagDirectives.Add(tag);
					if(tags != null) {
						tags.Add(tag);
					}
				} else {
					break;
				}
				
				Skip();
			}
			
			if(!tagDirectives.Contains("!")) {
				tagDirectives.Add(new TagDirective("!", "!"));
			}
			
			if(!tagDirectives.Contains("!!")) {
				tagDirectives.Add(new TagDirective("!!", "tag:yaml.org,2002:"));
			}
		    
			return version;
		}

		private Event yaml_parser_parse_document_content()
		{
			throw new NotImplementedException();
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
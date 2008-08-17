using System;
using System.Diagnostics;
using System.Collections.Generic;
using YamlDotNet.CoreCs.Tokens;
using YamlDotNet.CoreCs.Events;

namespace YamlDotNet.CoreCs
{
	public class Parser
	{
		private Stack<ParserState> states = new Stack<ParserState>();
		private Stack<Mark> marks = new Stack<Mark>();
		private Stack<TagDirective> tagDirectives = new Stack<TagDirective>();

		private bool streamStartProduced;
		private bool streamEndProduced;
		private bool error;
		private ParserState state;
		
		private Scanner scanner;
		
		public Event Parse() {
			/* No events after the end of the stream or error. */
			if (streamEndProduced || error || state == ParserState.YAML_PARSE_END_STATE) {
				return null;
			}

			/* Generate the next event. */
			return yaml_parser_state_machine();
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
					return null;
			}
		}
		
		private T Expect<T>() where T : Token {
			Token token = scanner.Current;
			T t = token as T;
			if(t == null) {
				throw new ParserException("did not found expected <stream-start>", token.Start);
			} else {
				scanner.MoveNext();
				return t;
			}
		}
		
		public Event yaml_parser_parse_stream_start() {
			Tokens.StreamStart token = Expect<Tokens.StreamStart>();

			state = ParserState.YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE;
			return new Events.StreamStart(token.Start, token.End);
		}
		
		public Event yaml_parser_parse_document_start(bool isImplicit) {
			return null;
		}

		public Event yaml_parser_parse_document_content() {
			return null;
		}

		public Event yaml_parser_parse_document_end() {
			return null;
		}

		public Event yaml_parser_parse_node(bool isBlock, bool isIndentlessSequence) {
			return null;
		}

		public Event yaml_parser_parse_block_sequence_entry(bool isFirst) {
			return null;
		}

		public Event yaml_parser_parse_indentless_sequence_entry() {
			return null;
		}

		public Event yaml_parser_parse_block_mapping_key(bool isFirst) {
			return null;
		}

		public Event yaml_parser_parse_block_mapping_value() {
			return null;
		}

		public Event yaml_parser_parse_flow_sequence_entry(bool isFirst) {
			return null;
		}

		public Event yaml_parser_parse_flow_sequence_entry_mapping_key() {
			return null;
		}

		public Event yaml_parser_parse_flow_sequence_entry_mapping_value() {
			return null;
		}

		public Event yaml_parser_parse_flow_sequence_entry_mapping_end() {
			return null;
		}

		public Event yaml_parser_parse_flow_mapping_key(bool isFirst) {
			return null;
		}

		public Event yaml_parser_parse_flow_mapping_value(bool isEmpty) {
			return null;
		}
	}
}
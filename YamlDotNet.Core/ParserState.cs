
using System;

namespace YamlDotNet.CoreCs
{
	public enum ParserState
	{
		/** Expect STREAM-START. */
		YAML_PARSE_STREAM_START_STATE,
		/** Expect the beginning of an implicit document. */
		YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE,
		/** Expect DOCUMENT-START. */
		YAML_PARSE_DOCUMENT_START_STATE,
		/** Expect the content of a document. */
		YAML_PARSE_DOCUMENT_CONTENT_STATE,
		/** Expect DOCUMENT-END. */
		YAML_PARSE_DOCUMENT_END_STATE,
		/** Expect a block node. */
		YAML_PARSE_BLOCK_NODE_STATE,
		/** Expect a block node or indentless sequence. */
		YAML_PARSE_BLOCK_NODE_OR_INDENTLESS_SEQUENCE_STATE,
		/** Expect a flow node. */
		YAML_PARSE_FLOW_NODE_STATE,
		/** Expect the first entry of a block sequence. */
		YAML_PARSE_BLOCK_SEQUENCE_FIRST_ENTRY_STATE,
		/** Expect an entry of a block sequence. */
		YAML_PARSE_BLOCK_SEQUENCE_ENTRY_STATE,
		/** Expect an entry of an indentless sequence. */
		YAML_PARSE_INDENTLESS_SEQUENCE_ENTRY_STATE,
		/** Expect the first key of a block mapping. */
		YAML_PARSE_BLOCK_MAPPING_FIRST_KEY_STATE,
		/** Expect a block mapping key. */
		YAML_PARSE_BLOCK_MAPPING_KEY_STATE,
		/** Expect a block mapping value. */
		YAML_PARSE_BLOCK_MAPPING_VALUE_STATE,
		/** Expect the first entry of a flow sequence. */
		YAML_PARSE_FLOW_SEQUENCE_FIRST_ENTRY_STATE,
		/** Expect an entry of a flow sequence. */
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_STATE,
		/** Expect a key of an ordered mapping. */
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_KEY_STATE,
		/** Expect a value of an ordered mapping. */
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_VALUE_STATE,
		/** Expect the and of an ordered mapping entry. */
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_END_STATE,
		/** Expect the first key of a flow mapping. */
		YAML_PARSE_FLOW_MAPPING_FIRST_KEY_STATE,
		/** Expect a key of a flow mapping. */
		YAML_PARSE_FLOW_MAPPING_KEY_STATE,
		/** Expect a value of a flow mapping. */
		YAML_PARSE_FLOW_MAPPING_VALUE_STATE,
		/** Expect an empty value of a flow mapping. */
		YAML_PARSE_FLOW_MAPPING_EMPTY_VALUE_STATE,
		/** Expect nothing. */
		YAML_PARSE_END_STATE
	}
}

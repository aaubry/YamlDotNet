
using System;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Defines the YAML parser's state.
	/// </summary>
	internal enum ParserState
	{
		/// <summary>
		/// Expect STREAM-START. 
		/// </summary>
		YAML_PARSE_STREAM_START_STATE,

		/// <summary>
		/// Expect the beginning of an implicit document. 
		/// </summary>
		YAML_PARSE_IMPLICIT_DOCUMENT_START_STATE,

		/// <summary>
		/// Expect DOCUMENT-START. 
		/// </summary>
		YAML_PARSE_DOCUMENT_START_STATE,

		/// <summary>
		/// Expect the content of a document. 
		/// </summary>
		YAML_PARSE_DOCUMENT_CONTENT_STATE,

		/// <summary>
		/// Expect DOCUMENT-END. 
		/// </summary>
		YAML_PARSE_DOCUMENT_END_STATE,

		/// <summary>
		/// Expect a block node. 
		/// </summary>
		YAML_PARSE_BLOCK_NODE_STATE,

		/// <summary>
		/// Expect a block node or indentless sequence. 
		/// </summary>
		YAML_PARSE_BLOCK_NODE_OR_INDENTLESS_SEQUENCE_STATE,

		/// <summary>
		/// Expect a flow node. 
		/// </summary>
		YAML_PARSE_FLOW_NODE_STATE,

		/// <summary>
		/// Expect the first entry of a block sequence. 
		/// </summary>
		YAML_PARSE_BLOCK_SEQUENCE_FIRST_ENTRY_STATE,

		/// <summary>
		/// Expect an entry of a block sequence. 
		/// </summary>
		YAML_PARSE_BLOCK_SEQUENCE_ENTRY_STATE,

		/// <summary>
		/// Expect an entry of an indentless sequence. 
		/// </summary>
		YAML_PARSE_INDENTLESS_SEQUENCE_ENTRY_STATE,

		/// <summary>
		/// Expect the first key of a block mapping. 
		/// </summary>
		YAML_PARSE_BLOCK_MAPPING_FIRST_KEY_STATE,

		/// <summary>
		/// Expect a block mapping key. 
		/// </summary>
		YAML_PARSE_BLOCK_MAPPING_KEY_STATE,

		/// <summary>
		/// Expect a block mapping value. 
		/// </summary>
		YAML_PARSE_BLOCK_MAPPING_VALUE_STATE,

		/// <summary>
		/// Expect the first entry of a flow sequence. 
		/// </summary>
		YAML_PARSE_FLOW_SEQUENCE_FIRST_ENTRY_STATE,

		/// <summary>
		/// Expect an entry of a flow sequence. 
		/// </summary>
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_STATE,

		/// <summary>
		/// Expect a key of an ordered mapping. 
		/// </summary>
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_KEY_STATE,

		/// <summary>
		/// Expect a value of an ordered mapping. 
		/// </summary>
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_VALUE_STATE,

		/// <summary>
		/// Expect the and of an ordered mapping entry. 
		/// </summary>
		YAML_PARSE_FLOW_SEQUENCE_ENTRY_MAPPING_END_STATE,

		/// <summary>
		/// Expect the first key of a flow mapping. 
		/// </summary>
		YAML_PARSE_FLOW_MAPPING_FIRST_KEY_STATE,

		/// <summary>
		/// Expect a key of a flow mapping. 
		/// </summary>
		YAML_PARSE_FLOW_MAPPING_KEY_STATE,

		/// <summary>
		/// Expect a value of a flow mapping. 
		/// </summary>
		YAML_PARSE_FLOW_MAPPING_VALUE_STATE,

		/// <summary>
		/// Expect an empty value of a flow mapping. 
		/// </summary>
		YAML_PARSE_FLOW_MAPPING_EMPTY_VALUE_STATE,

		/// <summary>
		/// Expect nothing. 
		/// </summary>
		YAML_PARSE_END_STATE
	}
}

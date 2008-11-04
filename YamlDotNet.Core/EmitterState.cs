using System;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Defines the YAML emitter's state.
	/// </summary>
	internal enum EmitterState
	{
		/// <summary>
		/// Expect STREAM-START.
		/// </summary>
		YAML_EMIT_STREAM_START_STATE,

		/// <summary>
		/// Expect the first DOCUMENT-START or STREAM-END.
		/// </summary>
		YAML_EMIT_FIRST_DOCUMENT_START_STATE,

		/// <summary>
		/// Expect DOCUMENT-START or STREAM-END.
		/// </summary>
		YAML_EMIT_DOCUMENT_START_STATE,

		/// <summary>
		/// Expect the content of a document.
		/// </summary>
		YAML_EMIT_DOCUMENT_CONTENT_STATE,

		/// <summary>
		/// Expect DOCUMENT-END.
		/// </summary>
		YAML_EMIT_DOCUMENT_END_STATE,

		/// <summary>
		/// Expect the first item of a flow sequence.
		/// </summary>
		YAML_EMIT_FLOW_SEQUENCE_FIRST_ITEM_STATE,

		/// <summary>
		/// Expect an item of a flow sequence.
		/// </summary>
		YAML_EMIT_FLOW_SEQUENCE_ITEM_STATE,

		/// <summary>
		/// Expect the first key of a flow mapping.
		/// </summary>
		YAML_EMIT_FLOW_MAPPING_FIRST_KEY_STATE,

		/// <summary>
		/// Expect a key of a flow mapping.
		/// </summary>
		YAML_EMIT_FLOW_MAPPING_KEY_STATE,

		/// <summary>
		/// Expect a value for a simple key of a flow mapping.
		/// </summary>
		YAML_EMIT_FLOW_MAPPING_SIMPLE_VALUE_STATE,

		/// <summary>
		/// Expect a value of a flow mapping.
		/// </summary>
		YAML_EMIT_FLOW_MAPPING_VALUE_STATE,

		/// <summary>
		/// Expect the first item of a block sequence.
		/// </summary>
		YAML_EMIT_BLOCK_SEQUENCE_FIRST_ITEM_STATE,

		/// <summary>
		/// Expect an item of a block sequence.
		/// </summary>
		YAML_EMIT_BLOCK_SEQUENCE_ITEM_STATE,

		/// <summary>
		/// Expect the first key of a block mapping.
		/// </summary>
		YAML_EMIT_BLOCK_MAPPING_FIRST_KEY_STATE,

		/// <summary>
		/// Expect the key of a block mapping.
		/// </summary>
		YAML_EMIT_BLOCK_MAPPING_KEY_STATE,

		/// <summary>
		/// Expect a value for a simple key of a block mapping.
		/// </summary>
		YAML_EMIT_BLOCK_MAPPING_SIMPLE_VALUE_STATE,

		/// <summary>
		/// Expect a value of a block mapping.
		/// </summary>
		YAML_EMIT_BLOCK_MAPPING_VALUE_STATE,

		/// <summary>
		/// Expect nothing.
		/// </summary>
		YAML_EMIT_END_STATE
	}
}
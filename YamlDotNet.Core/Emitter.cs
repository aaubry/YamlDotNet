using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;
using Event = YamlDotNet.Core.Events.ParsingEvent;
using TagDirective = YamlDotNet.Core.Tokens.TagDirective;
using VersionDirective = YamlDotNet.Core.Tokens.VersionDirective;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Emits YAML streams.
	/// </summary>
	public class Emitter
	{
		private readonly TextWriter output;
		
		private readonly bool isCanonical;
		private readonly int bestIndent;
		private readonly int bestWidth;
		private EmitterState state;
		
		private readonly Stack<EmitterState> states = new Stack<EmitterState>();
		private readonly Queue<Event> events = new Queue<Event>();
		private readonly Stack<int> indents = new Stack<int>();
		private readonly TagDirectiveCollection tagDirectives = new TagDirectiveCollection();
		private int indent;
		private int flowLevel;
		//private bool isRootContext;
		//private bool isSequenceContext;
		private bool isMappingContext;
		private bool isSimpleKeyContext;
		
		private int line;
		private int column;
		private bool isWhitespace;
		private bool isIndentation;
		
		//private const int MaxBufferLength = 8;
		
		private struct AnchorData {
			public string anchor;

			// Field is never assigned
#pragma warning disable 0649
			public bool isAlias;
#pragma warning restore 0649
		}
		
		private AnchorData anchorData; 
		
		private struct TagData {
			public string handle;
			public string suffix;
		}
		
		private TagData tagData;
		
		private struct ScalarData {
			public string value;
			public bool isMultiline;
			public bool isFlowPlainAllowed;
			public bool isBlockPlainAllowed;
			public bool isSingleQuotedAllowed;
			public bool isBlockAllowed;
			public ScalarStyle style;
		}
		
		private bool IsUnicode {
			get {
				return
					output.Encoding == Encoding.UTF8 ||
					output.Encoding == Encoding.Unicode ||
					output.Encoding == Encoding.BigEndianUnicode ||
					output.Encoding == Encoding.UTF7 ||
					output.Encoding == Encoding.UTF32;
			}
		}
		
		private ScalarData scalarData;

		private const int MinBestIndent = 2;
		private const int MaxBestIndent = 9;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Emitter"/> class.
		/// </summary>
		/// <param name="output">The <see cref="TextWriter"/> where the emitter will write.</param>
		public Emitter(TextWriter output)
			: this(output, MinBestIndent)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Emitter"/> class.
		/// </summary>
		/// <param name="output">The <see cref="TextWriter"/> where the emitter will write.</param>
		/// <param name="bestIndent">The preferred indentation.</param>
		public Emitter(TextWriter output, int bestIndent)
			: this(output, bestIndent, int.MaxValue)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Emitter"/> class.
		/// </summary>
		/// <param name="output">The <see cref="TextWriter"/> where the emitter will write.</param>
		/// <param name="bestIndent">The preferred indentation.</param>
		/// <param name="bestWidth">The preferred text width.</param>
		public Emitter(TextWriter output, int bestIndent, int bestWidth)
			: this(output, bestIndent, bestWidth, false)
		{
		}
		
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Emitter"/> class.
		/// </summary>
		/// <param name="output">The <see cref="TextWriter"/> where the emitter will write.</param>
		/// <param name="bestIndent">The preferred indentation.</param>
		/// <param name="bestWidth">The preferred text width.</param>
		/// <param name="isCanonical">If true, write the output in canonical form.</param>
		public Emitter(TextWriter output, int bestIndent, int bestWidth, bool isCanonical)
		{
			if(bestIndent < MinBestIndent || bestIndent > MaxBestIndent) {
				throw new ArgumentOutOfRangeException("bestIndent", string.Format(CultureInfo.InvariantCulture, "The bestIndent parameter must be between {0} and {1}.", MinBestIndent, MaxBestIndent));
			}
			
			this.bestIndent = bestIndent;
			
			if(bestWidth <= bestIndent * 2) {
				throw new ArgumentOutOfRangeException("bestWidth", "The bestWidth parameter must be greater than bestIndent * 2.");
			}
			
			this.bestWidth = bestWidth;
			
			this.isCanonical = isCanonical;
			
			this.output = output;
		}
		
		private void Write(char value) {
			output.Write(value);
			++column;
		}
		
		private void Write(string value) {
			output.Write(value);
			column += value.Length;
		}
		
		private void WriteBreak() {
			output.WriteLine();
			column = 0;
			++line;
		}
		
		/// <summary>
		/// Emit an evt.
		/// </summary>
		public void Emit(Event @event) {
			events.Enqueue(@event);

			while (!yaml_emitter_need_more_events()) {
				Event current = events.Peek();
				yaml_emitter_analyze_event(current);
				yaml_emitter_state_machine(current);

				// Only dequeue after calling yaml_emitter_state_machine because it checks how many events are in the queue.
				events.Dequeue();
			}
		}
		
		/// <summary>
		/// Check if we need to accumulate more events before emitting.
		/// 
		/// We accumulate extra
		///  - 1 event for DOCUMENT-START
		///  - 2 events for SEQUENCE-START
		///  - 3 events for MAPPING-START
		/// </summary>
		private bool yaml_emitter_need_more_events()
		{
			if(events.Count == 0) {
				return true;
			}

			int accumulate;
			switch (events.Peek().Type) {
				case EventType.YAML_DOCUMENT_START_EVENT:
					accumulate = 1;
					break;
					
				case EventType.YAML_SEQUENCE_START_EVENT:
					accumulate = 2;
					break;

				case EventType.YAML_MAPPING_START_EVENT:
					accumulate = 3;
					break;

				default:
					return false;
			}

			if(events.Count > accumulate) {
				return false;
			}

			int level = 0;
			foreach (var evt in events) {
				switch(evt.Type) {
					case EventType.YAML_DOCUMENT_START_EVENT:
					case EventType.YAML_SEQUENCE_START_EVENT:
					case EventType.YAML_MAPPING_START_EVENT:
						++level;
						break;

					case EventType.YAML_DOCUMENT_END_EVENT:
					case EventType.YAML_SEQUENCE_END_EVENT:
					case EventType.YAML_MAPPING_END_EVENT:
						--level;
						break;
				}
				if(level == 0) {
					return false;
				}
			}
			
			return true;
		}
		
		private void yaml_emitter_analyze_anchor(string anchor, bool isAlias)
		{
			anchorData.anchor = anchor;
			anchorData.isAlias = isAlias;
		}

		/// <summary>
		/// Check if the evt data is valid.
		/// </summary>
		private void yaml_emitter_analyze_event(Event evt)
		{
			anchorData.anchor = null;
			tagData.handle = null;
			tagData.suffix = null;
			
			AnchorAlias alias = evt as AnchorAlias;
			if(alias != null) {
				yaml_emitter_analyze_anchor(alias.Value, true);
				return;
			}
			
			NodeEvent nodeEvent = evt as NodeEvent;
			if(nodeEvent != null) {
				Scalar scalar = evt as Scalar;
				if(scalar != null) {
					yaml_emitter_analyze_scalar(scalar.Value);
				}

				yaml_emitter_analyze_anchor(nodeEvent.Anchor, false);
			
				if(!string.IsNullOrEmpty(nodeEvent.Tag) && (isCanonical || nodeEvent.IsCanonical)) {
					yaml_emitter_analyze_tag(nodeEvent.Tag);
				}
				return;
			}
		}

		/// <summary>
		/// Check if a scalar is valid.
		/// </summary>
		private void yaml_emitter_analyze_scalar(string value)
		{
			bool block_indicators = false;
			bool flow_indicators = false;
			bool line_breaks = false;
			bool special_characters = false;

			bool leading_spaces = false;
			bool leading_breaks = false;
			bool trailing_spaces = false;
			bool trailing_breaks = false;
			bool inline_breaks_spaces = false;
			bool mixed_breaks_spaces = false;

			bool spaces = false;
			bool breaks = false;
			bool mixed = false;
			bool leading = false;

			scalarData.value = value;

			if(value.Length == 0)
			{
				scalarData.isMultiline = false;
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = true;
				scalarData.isSingleQuotedAllowed = true;
				scalarData.isBlockAllowed = false;
				return;
			}

			if (value.StartsWith("---", StringComparison.Ordinal) || value.StartsWith("...", StringComparison.Ordinal))
			{
				block_indicators = true;
				flow_indicators = true;
			}

			bool preceeded_by_space = true;
			
			CharacterAnalyzer<StringLookAheadBuffer> buffer = new CharacterAnalyzer<StringLookAheadBuffer>(new StringLookAheadBuffer(value));
			bool followed_by_space = buffer.IsBlankOrBreakOrZero(1);

			bool isFirst = true;
			while(!buffer.EndOfInput)
			{
				if (isFirst)
				{
					if(buffer.Check(@"#,[]{}&*!|>\""%@`")) {
						flow_indicators = true;
						block_indicators = true;
					}

					if (buffer.Check("?:")) {
						flow_indicators = true;
						if (followed_by_space) {
							block_indicators = true;
						}
					}

					if (buffer.Check('-') && followed_by_space) {
						flow_indicators = true;
						block_indicators = true;
					}
				}
				else
				{
					if(buffer.Check(",?[]{}")) {
						flow_indicators = true;
					}

					if (buffer.Check(':')) {
						flow_indicators = true;
						if (followed_by_space) {
							block_indicators = true;
						}
					}

					if (buffer.Check('#') && preceeded_by_space) {
						flow_indicators = true;
						block_indicators = true;
					}
				}

				
				if (!buffer.IsPrintable() || (!buffer.IsAscii() && !IsUnicode)) {
					special_characters = true;
				}

				if (buffer.IsBreak()) {
					line_breaks = true;
				}

				if (buffer.IsSpace())
				{
					spaces = true;
					if (isFirst) {
						leading = true;
					}
				}

				else if (buffer.IsBreak())
				{
					if (spaces) {
						mixed = true;
					}
					breaks = true;
					if (isFirst) {
						leading = true;
					}
				}

				else if (spaces || breaks)
				{
					if (leading) {
						if (spaces && breaks) {
							mixed_breaks_spaces = true;
						}
						else if (spaces) {
							leading_spaces = true;
						}
						else if (breaks) {
							leading_breaks = true;
						}
					}
					else {
						if (mixed) {
							mixed_breaks_spaces = true;
						}
						else if (spaces && breaks) {
							inline_breaks_spaces = true;
						}
						else if (spaces) {
							//inline_spaces = true;
						}
						else if (breaks) {
							//inline_breaks = true;
						}
					}
					spaces = breaks = mixed = leading = false;
				}

				if ((spaces || breaks) && buffer.Buffer.Position == buffer.Buffer.Length - 1)
				{
					if (spaces && breaks) {
						mixed_breaks_spaces = true;
					}
					else if (spaces) {
						if (leading) {
							leading_spaces = true;
						}
						trailing_spaces = true;
					}
					else if (breaks) {
						if (leading) {
							leading_breaks = true;
						}
						trailing_breaks = true;
					}
				}

				preceeded_by_space = buffer.IsBlankOrBreakOrZero();
				buffer.Skip(1);
				if (!buffer.EndOfInput) {
					followed_by_space = buffer.IsBlankOrBreakOrZero(1);
				}
				isFirst = false;
			}

			scalarData.isMultiline = line_breaks;

			scalarData.isFlowPlainAllowed = true;
			scalarData.isBlockPlainAllowed = true;
			scalarData.isSingleQuotedAllowed = true;
			scalarData.isBlockAllowed = true;

			if (leading_spaces || leading_breaks || trailing_spaces) {
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = false;
				scalarData.isBlockAllowed = false;
			}

			if (trailing_breaks) {
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = false;
			}

			if (inline_breaks_spaces) {
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = false;
				scalarData.isSingleQuotedAllowed = false;
			}

			if (mixed_breaks_spaces || special_characters) {
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = false;
				scalarData.isSingleQuotedAllowed = false;
				scalarData.isBlockAllowed = false;
			}

			if (line_breaks) {
				scalarData.isFlowPlainAllowed = false;
				scalarData.isBlockPlainAllowed = false;
			}

			if (flow_indicators) {
				scalarData.isFlowPlainAllowed = false;
			}

			if (block_indicators) {
				scalarData.isBlockPlainAllowed = false;
			}
		}
		
		/// <summary>
		/// Check if a tag is valid.
		/// </summary>
		private void yaml_emitter_analyze_tag(string tag)
		{
			tagData.handle = tag;
			foreach (var tagDirective in tagDirectives) {
				if(tag.StartsWith(tagDirective.Prefix, StringComparison.Ordinal)) {
					tagData.handle = tagDirective.Handle;
					tagData.suffix = tag.Substring(tagDirective.Prefix.Length);
					break;
				}
			}
		}

		/// <summary>
		/// State dispatcher.
		/// </summary>
		private void yaml_emitter_state_machine(Event evt)
		{
			switch (state)
			{
				case EmitterState.YAML_EMIT_STREAM_START_STATE:
					yaml_emitter_emit_stream_start(evt);
					break;

				case EmitterState.YAML_EMIT_FIRST_DOCUMENT_START_STATE:
					yaml_emitter_emit_document_start(evt, true);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_START_STATE:
					yaml_emitter_emit_document_start(evt, false);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_CONTENT_STATE:
					yaml_emitter_emit_document_content(evt);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_END_STATE:
					yaml_emitter_emit_document_end(evt);
					break;

				case EmitterState.YAML_EMIT_FLOW_SEQUENCE_FIRST_ITEM_STATE:
					yaml_emitter_emit_flow_sequence_item(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_SEQUENCE_ITEM_STATE:
					yaml_emitter_emit_flow_sequence_item(evt, false);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_FIRST_KEY_STATE:
					yaml_emitter_emit_flow_mapping_key(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_KEY_STATE:
					yaml_emitter_emit_flow_mapping_key(evt, false);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_SIMPLE_VALUE_STATE:
					yaml_emitter_emit_flow_mapping_value(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_VALUE_STATE:
					yaml_emitter_emit_flow_mapping_value(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_SEQUENCE_FIRST_ITEM_STATE:
					yaml_emitter_emit_block_sequence_item(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_SEQUENCE_ITEM_STATE:
					yaml_emitter_emit_block_sequence_item(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_FIRST_KEY_STATE:
					yaml_emitter_emit_block_mapping_key(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_KEY_STATE:
					yaml_emitter_emit_block_mapping_key(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_SIMPLE_VALUE_STATE:
					yaml_emitter_emit_block_mapping_value(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_VALUE_STATE:
					yaml_emitter_emit_block_mapping_value(evt, false);
					break;

				case EmitterState.YAML_EMIT_END_STATE:
					throw new YamlException("Expected nothing after STREAM-END");

				default:
					Debug.Assert(false, "Invalid state.");
					throw new InvalidOperationException("Invalid state");
			}
		}
		
		/// <summary>
		/// Expect STREAM-START.
		/// </summary>
		private void yaml_emitter_emit_stream_start(Event evt)
		{
			if(evt.Type != EventType.YAML_STREAM_START_EVENT) {
				throw new ArgumentException("Expected STREAM-START.", "evt");
			}
			
			indent = -1;
			line = 0;
			column = 0;
			isWhitespace = true;
			isIndentation = true;

			state = EmitterState.YAML_EMIT_FIRST_DOCUMENT_START_STATE;
		}

		/// <summary>
		/// Expect DOCUMENT-START or STREAM-END.
		/// </summary>
		private void yaml_emitter_emit_document_start(Event evt, bool isFirst)
		{
			DocumentStart documentStart = evt as DocumentStart;
			if (documentStart != null)
			{
				bool isImplicit = documentStart.IsImplicit && isFirst && !isCanonical;

				if(documentStart.Version != null) {
					yaml_emitter_analyze_version_directive(documentStart.Version);

					isImplicit = false;
					yaml_emitter_write_indicator("%YAML", true, false, false);
					yaml_emitter_write_indicator(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Constants.MajorVersion, Constants.MinorVersion), true, false, false);
					yaml_emitter_write_indent();
				}

				if (documentStart.Tags != null)
				{
					foreach (var tagDirective in documentStart.Tags)
					{
						yaml_emitter_append_tag_directive(tagDirective, false);
					}
				}

				foreach (var tagDirective in Constants.DefaultTagDirectives) {
					yaml_emitter_append_tag_directive(tagDirective, true);
				}

				if(documentStart.Tags != null && documentStart.Tags.Count != 0) {
					isImplicit = false;
					foreach (var tagDirective in documentStart.Tags) {
						yaml_emitter_write_indicator("%TAG", true, false, false);
						yaml_emitter_write_tag_handle(tagDirective.Handle);
						yaml_emitter_write_tag_content(tagDirective.Prefix, true);
						yaml_emitter_write_indent();
					}
				}

				if (yaml_emitter_check_empty_document()) {
					isImplicit = false;
				}

				if (!isImplicit) {
					yaml_emitter_write_indent();
					yaml_emitter_write_indicator("---", true, false, false);
					if (isCanonical) {
						yaml_emitter_write_indent();
					}
				}

				state = EmitterState.YAML_EMIT_DOCUMENT_CONTENT_STATE;
			}

			else if (evt.Type == EventType.YAML_STREAM_END_EVENT)
			{
				state = EmitterState.YAML_EMIT_END_STATE;
			} else {
				throw new YamlException("Expected DOCUMENT-START or STREAM-END");
			}
		}
		
		/// <summary>
		/// Check if the document content is an empty scalar.
		/// </summary>
		private static bool yaml_emitter_check_empty_document()
		{
			// TODO: This method should be implemented
			return false;
		}
		
		private void yaml_emitter_write_tag_handle(string value)
		{
			if (!isWhitespace) {
				Write(' ');
			}

			Write(value);

			isWhitespace = false;
			isIndentation = false;
		}

		private static readonly Regex uriReplacer = new Regex(@"[^0-9A-Za-z_\-;?@=$~\\\)\]/:&+,\.\*\(\[]", RegexOptions.Compiled | RegexOptions.Singleline);
		
		private static string UrlEncode(string text) {
			return uriReplacer.Replace(text, delegate(Match match) {
				StringBuilder buffer = new StringBuilder();
				foreach (var toEncode in Encoding.UTF8.GetBytes(match.Value)) {
					buffer.AppendFormat("%{0:X02}", toEncode);
				}
				return buffer.ToString();
			});
		}
		
		private void yaml_emitter_write_tag_content(string value, bool needsWhitespace)
		{
			if (needsWhitespace && !isWhitespace) {
				Write(' ');
			}

			Write(UrlEncode(value));

			isWhitespace = false;
			isIndentation = false;
		}
	
		/// <summary>
		/// Append a directive to the directives stack.
		/// </summary>
		private void yaml_emitter_append_tag_directive(TagDirective value, bool allowDuplicates)
		{
			if(tagDirectives.Contains(value)) {
				if(allowDuplicates) {
					return;
				} else {
					throw new YamlException("Duplicate %TAG directive.");
				}
			} else {
				tagDirectives.Add(value);
			}
		}

		/// <summary>
		/// Check if a %YAML directive is valid.
		/// </summary>
		private static void yaml_emitter_analyze_version_directive(VersionDirective versionDirective)
		{
			if(versionDirective.Version.Major != Constants.MajorVersion || versionDirective.Version.Minor != Constants.MinorVersion) {
				throw new YamlException("Incompatible %YAML directive");
			}
		}

		private void yaml_emitter_write_indicator(string indicator, bool needWhitespace, bool whitespace, bool indentation)
		{
			if (needWhitespace && !isWhitespace) {
				Write(' ');
			}

			Write(indicator);

			isWhitespace = whitespace;
			isIndentation &= indentation;
		}
		
		private void yaml_emitter_write_indent()
		{
			int currentIndent = Math.Max(indent, 0);

			if (!isIndentation || column > currentIndent || (column == currentIndent && !isWhitespace)) {
				WriteBreak();
			}

			while (column < currentIndent) {
				Write(' ');
			}

			isWhitespace = true;
			isIndentation = true;
		}

		/// <summary>
		/// Expect the root node.
		/// </summary>
		private void yaml_emitter_emit_document_content(Event evt)
		{
			states.Push(EmitterState.YAML_EMIT_DOCUMENT_END_STATE);
			yaml_emitter_emit_node(evt, false, false);
		}

		/// <summary>
		/// Expect a node.
		/// </summary>
		private void yaml_emitter_emit_node(ParsingEvent evt, bool isMapping, bool isSimpleKey)
		{
			isMappingContext = isMapping;
			isSimpleKeyContext = isSimpleKey;

			switch (evt.Type)
			{
				case EventType.YAML_ALIAS_EVENT:
					yaml_emitter_emit_alias();
					break;

				case EventType.YAML_SCALAR_EVENT:
					yaml_emitter_emit_scalar(evt);
					break;

				case EventType.YAML_SEQUENCE_START_EVENT:
					yaml_emitter_emit_sequence_start(evt);
					break;

				case EventType.YAML_MAPPING_START_EVENT:
					yaml_emitter_emit_mapping_start(evt);
					break;

				default:
					throw new YamlException("Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS.");
			}
		}

		/// <summary>
		/// Expect SEQUENCE-START.
		/// </summary>
		private void yaml_emitter_emit_sequence_start(Event evt)
		{
			yaml_emitter_process_anchor();
			yaml_emitter_process_tag();

			SequenceStart sequenceStart = (SequenceStart)evt;
			
			if (flowLevel != 0 || isCanonical || sequenceStart.Style == SequenceStyle.Flow || yaml_emitter_check_empty_sequence()) {
				state = EmitterState.YAML_EMIT_FLOW_SEQUENCE_FIRST_ITEM_STATE;
			}
			else {
				state = EmitterState.YAML_EMIT_BLOCK_SEQUENCE_FIRST_ITEM_STATE;
			}
		}
		
		/// <summary>
		/// Check if the next events represent an empty sequence.
		/// </summary>
		private bool yaml_emitter_check_empty_sequence()
		{
			if (events.Count < 2) {
				return false;
			}

			FakeList<Event> eventList = new FakeList<Event>(events);
			return eventList[0].Type == EventType.YAML_SEQUENCE_START_EVENT && eventList[1].Type == EventType.YAML_SEQUENCE_END_EVENT;
		}

		/// <summary>
		/// Check if the next events represent an empty mapping.
		/// </summary>
		private bool yaml_emitter_check_empty_mapping()
		{
			if (events.Count < 2) {
				return false;
			}

			FakeList<Event> eventList = new FakeList<Event>(events);
			return eventList[0].Type == EventType.YAML_MAPPING_START_EVENT && eventList[1].Type == EventType.YAML_MAPPING_END_EVENT;
		}
		
		/// <summary>
		/// Write a tag.
		/// </summary>
		private void yaml_emitter_process_tag()
		{
			if(tagData.handle == null && tagData.suffix == null) {
				return;
			}

			if (tagData.handle != null)
			{
				yaml_emitter_write_tag_handle(tagData.handle);
				if(tagData.suffix != null) {
					yaml_emitter_write_tag_content(tagData.suffix, false);
				}
			}
			else
			{
				yaml_emitter_write_indicator("!<", true, false, false);
				yaml_emitter_write_tag_content(tagData.suffix, false);
				yaml_emitter_write_indicator(">", false, false, false);
			}
		}

		/// <summary>
		/// Expect MAPPING-START.
		/// </summary>
		private void yaml_emitter_emit_mapping_start(Event evt)
		{
			yaml_emitter_process_anchor();
			yaml_emitter_process_tag();

			MappingStart mappingStart = (MappingStart)evt;

			if (flowLevel != 0 || isCanonical || mappingStart.Style == MappingStyle.Flow || yaml_emitter_check_empty_mapping()) {
				state = EmitterState.YAML_EMIT_FLOW_MAPPING_FIRST_KEY_STATE;
			}
			else {
				state = EmitterState.YAML_EMIT_BLOCK_MAPPING_FIRST_KEY_STATE;
			}
		}

		/// <summary>
		/// Expect SCALAR.
		/// </summary>
		private void yaml_emitter_emit_scalar(Event evt)
		{
			yaml_emitter_select_scalar_style(evt);
			yaml_emitter_process_anchor();
			yaml_emitter_process_tag();
			yaml_emitter_increase_indent(true, false);
			yaml_emitter_process_scalar();
			
			indent = indents.Pop();
			state = states.Pop();
		}
		
		/// <summary>
		/// Write a scalar.
		/// </summary>
		private void yaml_emitter_process_scalar()
		{
			switch (scalarData.style)
			{
				case ScalarStyle.Plain:
					yaml_emitter_write_plain_scalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.SingleQuoted:
					yaml_emitter_write_single_quoted_scalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.DoubleQuoted:
					yaml_emitter_write_double_quoted_scalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.Literal:
					yaml_emitter_write_literal_scalar(scalarData.value);
					break;

				case ScalarStyle.Folded:
					yaml_emitter_write_folded_scalar(scalarData.value);
					break;

				default:
					// Impossible.
					throw new InvalidOperationException();
			}
		}
		
		private static bool IsBreak(char character) {
			return character == '\r' || character == '\n' || character == '\x85' || character == '\x2028' || character == '\x2029';
		}
		
		/// <summary>
		/// Check if the specified character is a space.
		/// </summary>
		private static bool IsSpace(char character)
		{
			return character == ' ';
		}

		private static bool IsPrintable(char character) {
			return
				character == '\x9' ||
				character == '\xA' ||
				character == '\xD' ||
				(character >= '\x20' && character <= '\x7E') ||
				character == '\x85' ||
				(character >= '\xA0' && character <= '\xD7FF') ||
				(character >= '\xE000' && character <= '\xFFFD');
		}

		private void yaml_emitter_write_folded_scalar(string value)
		{
			int chomp = yaml_emitter_determine_chomping(value);
			bool breaks = true;
			bool leadingSpaces = false;

			yaml_emitter_write_indicator(chomp == -1 ? ">-" : chomp == +1 ? ">+" : ">", true, false, false);
			yaml_emitter_write_indent();

			for(int i = 0; i < value.Length; ++i) {
				char character = value[i];

				if(IsBreak(character)) {
					if (!breaks && !leadingSpaces && character == '\n') {
						do {
							++i;
						} while (i < value.Length && IsBreak(value[i]));
						
						if(i < value.Length && value[i] != ' ') {
							WriteBreak();
						}
					}
					WriteBreak();
					isIndentation = true;
					breaks = true;
				}
				else
				{
					if (breaks) {
						yaml_emitter_write_indent();
						leadingSpaces = character == ' ';
					}
					if (!breaks && character == ' ' && i + 1 < value.Length && value[i + 1] != ' ' && column > bestWidth) {
						yaml_emitter_write_indent();
					}
					else {
						Write(character);
					}
					isIndentation = false;
					breaks = false;
				}
			}
		}
	
		private static int yaml_emitter_determine_chomping(string value)
		{
			// TODO: Understand the reason for this method
			if(value.Length == 0) {
				return -1;
			}

			/*
			do {
				string.pointer --;
			} while ((*string.pointer & 0xC0) == 0x80);
			if (!IS_BREAK(string))
				return -1;
			if (string.start == string.pointer)
				return 0;
			do {
				string.pointer --;
			} while ((*string.pointer & 0xC0) == 0x80);
			if (!IS_BREAK(string))
				return 0;
			return +1;
			*/
			return 1;
		}

		private void yaml_emitter_write_literal_scalar(string value)
		{
			int chomp = yaml_emitter_determine_chomping(value);
			bool breaks = false;

			yaml_emitter_write_indicator(chomp == -1 ? "|-" : chomp == +1 ? "|+" : "|", true, false, false);
			yaml_emitter_write_indent();

			foreach (var character in value) {
				if(IsBreak(character)) {
					WriteBreak();
					isIndentation = true;
					breaks = true;
				}
				else {
					if(breaks) {
						yaml_emitter_write_indent();
					}
					Write(character);
					isIndentation = false;
					breaks = false;
				}
			}
		}

		private void yaml_emitter_write_double_quoted_scalar(string value, bool allowBreaks)
		{
			yaml_emitter_write_indicator("\"", true, false, false);

			bool spaces = false;
			for (int index = 0; index < value.Length; ++index) {
				char character = value[index];

				
				if (!IsPrintable(character) || IsBreak(character) || character == '"' || character == '\\')
				{
					Write('\\');

					switch (character)
					{
						case '\0':
							Write('0');
							break;

						case '\x7':
							Write('a');
							break;

						case '\x8':
							Write('b');
							break;

						case '\x9':
							Write('t');
							break;

						case '\xA':
							Write('n');
							break;

						case '\xB':
							Write('v');
							break;

						case '\xC':
							Write('f');
							break;

						case '\xD':
							Write('r');
							break;

						case '\x1B':
							Write('e');
							break;

						case '\x22':
							Write('"');
							break;

						case '\x5C':
							Write('\\');
							break;

						case '\x85':
							Write('N');
							break;

						case '\xA0':
							Write('_');
							break;

						case '\x2028':
							Write('L');
							break;

						case '\x2029':
							Write('P');
							break;

						default:
							short code = (short)character;
							if (code <= 0xFF) {
								Write('x');
								Write(code.ToString("X02", CultureInfo.InvariantCulture));
							}
							else { //if (code <= 0xFFFF) {
								Write('u');
								Write(code.ToString("X04", CultureInfo.InvariantCulture));
							}
							//else {
							//	Write('U');
							//	Write(code.ToString("X08"));
							//}
							break;
					}
					spaces = false;
				}
				else if (character == ' ')
				{
					if (allowBreaks && !spaces && column > bestWidth && index > 0 && index + 1 < value.Length) {
						yaml_emitter_write_indent();
						if(value[index + 1] == ' ') {
							Write('\\');
						}
					}
					else {
						Write(character);
					}
					spaces = true;
				}
				else
				{
					Write(character);
					spaces = false;
				}
			}

			yaml_emitter_write_indicator("\"", false, false, false);

			isWhitespace = false;
			isIndentation = false;
		}

		private void yaml_emitter_write_single_quoted_scalar(string value, bool allowBreaks)
		{
			yaml_emitter_write_indicator("'", true, false, false);

			bool spaces = false;
			bool breaks = false;

			for (int index = 0; index < value.Length; ++index) {
				char character = value[index];

				if (character == ' ')
				{
					if (allowBreaks && !spaces && column > bestWidth && index != 0 && index + 1 < value.Length && value[index + 1] != ' ') {
						yaml_emitter_write_indent();
					}
					else {
						Write(character);
					}
					spaces = true;
				}
				else if (IsBreak(character))
				{
					if (!breaks && character == '\n') {
						WriteBreak();
					}
					WriteBreak();
					isIndentation = true;
					breaks = true;
				}
				else
				{
					if (breaks) {
						yaml_emitter_write_indent();
					}
					if (character == '\'') {
						Write(character);
					}
					Write(character);
					isIndentation = false;
					spaces = false;
					breaks = false;
				}
			}

			yaml_emitter_write_indicator("'", false, false, false);

			isWhitespace = false;
			isIndentation = false;
		}
		
		private void yaml_emitter_write_plain_scalar(string value, bool allowBreaks)
		{
			if (!isWhitespace) {
				Write(' ');
			}

			bool spaces = false;
			bool breaks = false;
			for (int index = 0; index < value.Length; ++index) {
				char character = value[index];

				if(IsSpace(character)) {
					if(allowBreaks && !spaces && column > bestWidth && index + 1 < value.Length && value[index + 1] != ' ') {
						yaml_emitter_write_indent();
					}
					else {
						Write(character);
					}
					spaces = true;
				}
				else if (IsBreak(character))
				{
					if (!breaks && character == '\n') {
						WriteBreak();
					}
					WriteBreak();
					isIndentation = true;
					breaks = true;
				}
				else
				{
					if (breaks) {
						yaml_emitter_write_indent();
					}
					Write(character);
					isIndentation = false;
					spaces = false;
					breaks = false;
				}
			}

			isWhitespace = false;
			isIndentation = false;
		}

		/// <summary>
		/// Increase the indentation level.
		/// </summary>
		private void yaml_emitter_increase_indent(bool isFlow, bool isIndentless)
		{
			indents.Push(indent);

			if (indent < 0) {
				indent = isFlow ? bestIndent : 0;
			}
			else if (!isIndentless) {
				indent += bestIndent;
			}
		}
		
		/// <summary>
		/// Determine an acceptable scalar style.
		/// </summary>
		private void yaml_emitter_select_scalar_style(Event evt)
		{
			Scalar scalar = (Scalar)evt;
			
			ScalarStyle style = scalar.Style;
			bool noTag = tagData.handle == null && tagData.suffix == null;

			if (noTag && !scalar.IsPlainImplicit && !scalar.IsQuotedImplicit) {
				throw new YamlException("Neither tag nor isImplicit flags are specified.");
			}

			if (style == ScalarStyle.Any) {
				style = ScalarStyle.Plain;
			}
			
			if (isCanonical) {
				style = ScalarStyle.DoubleQuoted;
			}

			
			if (isSimpleKeyContext && scalarData.isMultiline) {
				style = ScalarStyle.DoubleQuoted;
			}

			if (style == ScalarStyle.Plain)
			{
				if ((flowLevel != 0 && !scalarData.isFlowPlainAllowed) || (flowLevel == 0 && !scalarData.isBlockPlainAllowed)) {
					style = ScalarStyle.SingleQuoted;
				}
				if (string.IsNullOrEmpty(scalarData.value) && (flowLevel != 0 || isSimpleKeyContext)) {
					style = ScalarStyle.SingleQuoted;
				}
				if (noTag && !scalar.IsPlainImplicit) {
					style = ScalarStyle.SingleQuoted;
				}
			}

			if (style == ScalarStyle.SingleQuoted)
			{
				if (!scalarData.isSingleQuotedAllowed) {
					style = ScalarStyle.DoubleQuoted;
				}
			}

			if (style == ScalarStyle.Literal || style == ScalarStyle.Folded)
			{
				if (!scalarData.isBlockAllowed || flowLevel != 0 || isSimpleKeyContext) {
					style = ScalarStyle.DoubleQuoted;
				}
			}

			if (noTag && !scalar.IsQuotedImplicit && style != ScalarStyle.Plain)
			{
				tagData.handle = "!";
			}

			scalarData.style = style;
		}

		/// <summary>
		/// Expect ALIAS.
		/// </summary>
		private void yaml_emitter_emit_alias()
		{
			yaml_emitter_process_anchor();
			state = states.Pop();
		}
		
		/// <summary>
		/// Write an achor.
		/// </summary>
		private void yaml_emitter_process_anchor()
		{
			if (anchorData.anchor != null)
			{
				yaml_emitter_write_indicator(anchorData.isAlias ? "*" : "&", true, false, false);
				yaml_emitter_write_anchor(anchorData.anchor);
			}
		}

		private void yaml_emitter_write_anchor(string value)
		{
			Write(value);

			isWhitespace = false;
			isIndentation = false;
		}

		/// <summary>
		/// Expect DOCUMENT-END.
		/// </summary>
		private void yaml_emitter_emit_document_end(Event evt)
		{
			DocumentEnd documentEnd = evt as DocumentEnd;
			if (documentEnd != null)
			{
				yaml_emitter_write_indent();
				if(!documentEnd.IsImplicit) {
					yaml_emitter_write_indicator("...", true, false, false);
					yaml_emitter_write_indent();
				}

				state = EmitterState.YAML_EMIT_DOCUMENT_START_STATE;

				tagDirectives.Clear();
			} else {
				throw new YamlException("Expected DOCUMENT-END.");
			}
		}

		/// <summary>
		/// 
		/// Expect a flow item node.
		/// </summary>

		private void yaml_emitter_emit_flow_sequence_item(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				yaml_emitter_write_indicator("[", true, true, false);
				yaml_emitter_increase_indent(true, false);
				++flowLevel;
			}

			if (evt.Type == EventType.YAML_SEQUENCE_END_EVENT)
			{
				--flowLevel;
				indent = indents.Pop();
				if (isCanonical && !isFirst) {
					yaml_emitter_write_indicator(",", false, false, false);
					yaml_emitter_write_indent();
				}
				yaml_emitter_write_indicator("]", false, false, false);
				state = states.Pop();
				return;
			}

			if (!isFirst) {
				yaml_emitter_write_indicator(",", false, false, false);
			}

			if (isCanonical ||  column > bestWidth) {
				yaml_emitter_write_indent();
			}
			
			states.Push(EmitterState.YAML_EMIT_FLOW_SEQUENCE_ITEM_STATE);

			yaml_emitter_emit_node(evt, false, false);
		}

		/// <summary>
		/// Expect a flow key node.
		/// </summary>
		private void yaml_emitter_emit_flow_mapping_key(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				yaml_emitter_write_indicator("{", true, true, false);
				yaml_emitter_increase_indent(true, false);
				++flowLevel;
			}

			if (evt.Type == EventType.YAML_MAPPING_END_EVENT)
			{
				--flowLevel;
				indent = indents.Pop();
				if (isCanonical && !isFirst) {
					yaml_emitter_write_indicator(",", false, false, false);
					yaml_emitter_write_indent();
				}
				yaml_emitter_write_indicator("}", false, false, false);
				state = states.Pop();
				return;
			}

			if (!isFirst) {
				yaml_emitter_write_indicator(",", false, false, false);
			}
			if (isCanonical || column > bestWidth) {
				yaml_emitter_write_indent();
			}

			if (!isCanonical && yaml_emitter_check_simple_key())
			{
				states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_SIMPLE_VALUE_STATE);
				yaml_emitter_emit_node(evt, true, true);
			}
			else
			{
				yaml_emitter_write_indicator("?", true, false, false);
				states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_VALUE_STATE);
				yaml_emitter_emit_node(evt, true, false);
			}
		}

		private const int MaxAliasLength = 128;

		private static int SafeStringLength(string value) {
			return value != null ? value.Length : 0;
		}
		
		/// <summary>
		/// Check if the next node can be expressed as a simple key.
		/// </summary>
		private bool yaml_emitter_check_simple_key()
		{
			if(events.Count < 1) {
				return false;
			}
			
			int length;
			switch (events.Peek().Type)
			{
				case EventType.YAML_ALIAS_EVENT:
					length = SafeStringLength(anchorData.anchor);
					break;

				case EventType.YAML_SCALAR_EVENT:
					if (scalarData.isMultiline) {
						return false;
					}
					
					length =
						SafeStringLength(anchorData.anchor) +
						SafeStringLength(tagData.handle) +
						SafeStringLength(tagData.suffix) +
						SafeStringLength(scalarData.value);
					break;

				case EventType.YAML_SEQUENCE_START_EVENT:
					if (!yaml_emitter_check_empty_sequence()) {
						return false;
					}
					length =
						SafeStringLength(anchorData.anchor) +
						SafeStringLength(tagData.handle) +
						SafeStringLength(tagData.suffix);
					break;

				case EventType.YAML_MAPPING_START_EVENT:
					if (!yaml_emitter_check_empty_sequence()) {
						return false;
					}
					length =
						SafeStringLength(anchorData.anchor) +
						SafeStringLength(tagData.handle) +
						SafeStringLength(tagData.suffix);
					break;

				default:
					return false;
			}
			
			return length <= MaxAliasLength;
		}

		/// <summary>
		/// Expect a flow value node.
		/// </summary>
		private void yaml_emitter_emit_flow_mapping_value(Event evt, bool isSimple)
		{
			if (isSimple) {
				yaml_emitter_write_indicator(":", false, false, false);
			}
			else {
				if (isCanonical || column > bestWidth) {
					yaml_emitter_write_indent();
				}
				yaml_emitter_write_indicator(":", true, false, false);
			}
			states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_KEY_STATE);
			yaml_emitter_emit_node(evt, true, false);
		}

		/// <summary>
		/// Expect a block item node.
		/// </summary>
		private void yaml_emitter_emit_block_sequence_item(Event evt, bool isFirst)
		{
			if (isFirst) {
				yaml_emitter_increase_indent(false, (isMappingContext && !isIndentation));
			}

			if (evt.Type == EventType.YAML_SEQUENCE_END_EVENT)
			{
				indent = indents.Pop();
				state = states.Pop();
				return;
			}

			yaml_emitter_write_indent();
			yaml_emitter_write_indicator("-", true, false, true);
			states.Push(EmitterState.YAML_EMIT_BLOCK_SEQUENCE_ITEM_STATE);

			yaml_emitter_emit_node(evt, false, false);
		}
		
		/// <summary>
		/// Expect a block key node.
		/// </summary>
		private void yaml_emitter_emit_block_mapping_key(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				yaml_emitter_increase_indent(false, false);
			}

			if (evt.Type == EventType.YAML_MAPPING_END_EVENT)
			{
				indent = indents.Pop();
				state = states.Pop();
				return;
			}

			yaml_emitter_write_indent();

			if (yaml_emitter_check_simple_key())
			{
				states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_SIMPLE_VALUE_STATE);
				yaml_emitter_emit_node(evt, true, true);
			}
			else
			{
				yaml_emitter_write_indicator("?", true, false, true);
				states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_VALUE_STATE);
				yaml_emitter_emit_node(evt, true, false);
			}
		}

		/// <summary>
		/// Expect a block value node.
		/// </summary>
		private void yaml_emitter_emit_block_mapping_value(Event evt, bool isSimple)
		{
			if (isSimple) {
				yaml_emitter_write_indicator(":", false, false, false);
			}
			else {
				yaml_emitter_write_indent();
				yaml_emitter_write_indicator(":", true, false, true);
			}
			states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_KEY_STATE);
			yaml_emitter_emit_node(evt, true, false);
		}
	}
}

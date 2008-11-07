using System;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;
using Event = YamlDotNet.Core.Events.IParsingEvent;
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
		private bool isMappingContext;
		private bool isSimpleKeyContext;
		
		private int line;
		private int column;
		private bool isWhitespace;
		private bool isIndentation;
		
		private struct AnchorData {
			public string anchor;
			public bool isAlias;
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

			while (!NeedMoreEvents()) {
				Event current = events.Peek();
				AnalyzeEvent(current);
				StateMachine(current);

				// Only dequeue after calling state_machine because it checks how many events are in the queue.
				events.Dequeue();
			}
		}

		private static EventType GetEventType(IParsingEvent @event)
		{
			if(@event is IAnchorAlias)
			{
				return EventType.YAML_ALIAS_EVENT;
			}

			if (@event is IDocumentEnd)
			{
				return EventType.YAML_DOCUMENT_END_EVENT;
			}

			if (@event is IDocumentStart)
			{
				return EventType.YAML_DOCUMENT_START_EVENT;
			}

			if (@event is IMappingEnd)
			{
				return EventType.YAML_MAPPING_END_EVENT;
			}

			if (@event is IMappingStart)
			{
				return EventType.YAML_MAPPING_START_EVENT;
			}

			if (@event is IScalar)
			{
				return EventType.YAML_SCALAR_EVENT;
			}

			if (@event is ISequenceEnd)
			{
				return EventType.YAML_SEQUENCE_END_EVENT;
			}

			if (@event is ISequenceStart)
			{
				return EventType.YAML_SEQUENCE_START_EVENT;
			}

			if(@event is IStreamEnd)
			{
				return EventType.YAML_STREAM_END_EVENT;
			}

			if(@event is IStreamStart)
			{
				return EventType.YAML_STREAM_START_EVENT;
			}

			throw new ArgumentException("The specified event is of the wrong type.");
		}

		/// <summary>
		/// Check if we need to accumulate more events before emitting.
		/// 
		/// We accumulate extra
		///  - 1 event for DOCUMENT-START
		///  - 2 events for SEQUENCE-START
		///  - 3 events for MAPPING-START
		/// </summary>
		private bool NeedMoreEvents()
		{
			if(events.Count == 0) {
				return true;
			}

			int accumulate;
			switch (GetEventType(events.Peek())) {
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
				switch(GetEventType(evt)) {
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
		
		private void AnalyzeAnchor(string anchor, bool isAlias)
		{
			anchorData.anchor = anchor;
			anchorData.isAlias = isAlias;
		}

		/// <summary>
		/// Check if the evt data is valid.
		/// </summary>
		private void AnalyzeEvent(Event evt)
		{
			anchorData.anchor = null;
			tagData.handle = null;
			tagData.suffix = null;
			
			AnchorAlias alias = evt as AnchorAlias;
			if(alias != null) {
				AnalyzeAnchor(alias.Value, true);
				return;
			}
			
			NodeEvent nodeEvent = evt as NodeEvent;
			if(nodeEvent != null) {
				Scalar scalar = evt as Scalar;
				if(scalar != null) {
					AnalyzeScalar(scalar.Value);
				}

				AnalyzeAnchor(nodeEvent.Anchor, false);
			
				if(!string.IsNullOrEmpty(nodeEvent.Tag) && (isCanonical || nodeEvent.IsCanonical)) {
					AnalyzeTag(nodeEvent.Tag);
				}
				return;
			}
		}

		/// <summary>
		/// Check if a scalar is valid.
		/// </summary>
		private void AnalyzeScalar(string value)
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
		private void AnalyzeTag(string tag)
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
		private void StateMachine(Event evt)
		{
			switch (state)
			{
				case EmitterState.YAML_EMIT_STREAM_START_STATE:
					EmitStreamStart(evt);
					break;

				case EmitterState.YAML_EMIT_FIRST_DOCUMENT_START_STATE:
					EmitDocumentStart(evt, true);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_START_STATE:
					EmitDocumentStart(evt, false);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_CONTENT_STATE:
					EmitDocumentContent(evt);
					break;

				case EmitterState.YAML_EMIT_DOCUMENT_END_STATE:
					EmitDocumentEnd(evt);
					break;

				case EmitterState.YAML_EMIT_FLOW_SEQUENCE_FIRST_ITEM_STATE:
					EmitFlowSequenceItem(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_SEQUENCE_ITEM_STATE:
					EmitFlowSequenceItem(evt, false);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_FIRST_KEY_STATE:
					EmitFlowMappingKey(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_KEY_STATE:
					EmitFlowMappingKey(evt, false);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_SIMPLE_VALUE_STATE:
					EmitFlowMappingValue(evt, true);
					break;

				case EmitterState.YAML_EMIT_FLOW_MAPPING_VALUE_STATE:
					EmitFlowMappingValue(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_SEQUENCE_FIRST_ITEM_STATE:
					EmitBlockSequenceItem(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_SEQUENCE_ITEM_STATE:
					EmitBlockSequenceItem(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_FIRST_KEY_STATE:
					EmitBlockMappingKey(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_KEY_STATE:
					EmitBlockMappingKey(evt, false);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_SIMPLE_VALUE_STATE:
					EmitBlockMappingValue(evt, true);
					break;

				case EmitterState.YAML_EMIT_BLOCK_MAPPING_VALUE_STATE:
					EmitBlockMappingValue(evt, false);
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
		private void EmitStreamStart(Event evt)
		{
			if(!(evt is IStreamStart)) {
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
		private void EmitDocumentStart(Event evt, bool isFirst)
		{
			DocumentStart documentStart = evt as DocumentStart;
			if (documentStart != null)
			{
				bool isImplicit = documentStart.IsImplicit && isFirst && !isCanonical;

				if(documentStart.Version != null) {
					AnalyzeVersionDirective(documentStart.Version);

					isImplicit = false;
					WriteIndicator("%YAML", true, false, false);
					WriteIndicator(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", Constants.MajorVersion, Constants.MinorVersion), true, false, false);
					WriteIndent();
				}

				if (documentStart.Tags != null)
				{
					foreach (var tagDirective in documentStart.Tags)
					{
						AppendTagDirective(tagDirective, false);
					}
				}

				foreach (var tagDirective in Constants.DefaultTagDirectives) {
					AppendTagDirective(tagDirective, true);
				}

				if(documentStart.Tags != null && documentStart.Tags.Count != 0) {
					isImplicit = false;
					foreach (var tagDirective in documentStart.Tags) {
						WriteIndicator("%TAG", true, false, false);
						WriteTagHandle(tagDirective.Handle);
						WriteTagContent(tagDirective.Prefix, true);
						WriteIndent();
					}
				}

				if (CheckEmptyDocument()) {
					isImplicit = false;
				}

				if (!isImplicit) {
					WriteIndent();
					WriteIndicator("---", true, false, false);
					if (isCanonical) {
						WriteIndent();
					}
				}

				state = EmitterState.YAML_EMIT_DOCUMENT_CONTENT_STATE;
			}

			else if (!(evt is IStreamEnd))
			{
				state = EmitterState.YAML_EMIT_END_STATE;
			} else {
				throw new YamlException("Expected DOCUMENT-START or STREAM-END");
			}
		}
		
		/// <summary>
		/// Check if the document content is an empty scalar.
		/// </summary>
		private bool CheckEmptyDocument()
		{
			int index = 0;
			foreach (var parsingEvent in events)
			{
				if(++index == 2)
				{
					Scalar scalar = parsingEvent as Scalar;
					if(scalar != null)
					{
						return string.IsNullOrEmpty(scalar.Value);
					}
					break;
				}
			}

			return false;
		}
		
		private void WriteTagHandle(string value)
		{
			if (!isWhitespace) {
				Write(' ');
			}

			Write(value);

			isWhitespace = false;
			isIndentation = false;
		}

		private static readonly Regex uriReplacer = new Regex(@"[^0-9A-Za-z_\-;?@=$~\\\)\]/:&+,\.\*\(\[!]", RegexOptions.Compiled | RegexOptions.Singleline);
		
		private static string UrlEncode(string text) {
			return uriReplacer.Replace(text, delegate(Match match) {
				StringBuilder buffer = new StringBuilder();
				foreach (var toEncode in Encoding.UTF8.GetBytes(match.Value)) {
					buffer.AppendFormat("%{0:X02}", toEncode);
				}
				return buffer.ToString();
			});
		}
		
		private void WriteTagContent(string value, bool needsWhitespace)
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
		private void AppendTagDirective(TagDirective value, bool allowDuplicates)
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
		private static void AnalyzeVersionDirective(VersionDirective versionDirective)
		{
			if(versionDirective.Version.Major != Constants.MajorVersion || versionDirective.Version.Minor != Constants.MinorVersion) {
				throw new YamlException("Incompatible %YAML directive");
			}
		}

		private void WriteIndicator(string indicator, bool needWhitespace, bool whitespace, bool indentation)
		{
			if (needWhitespace && !isWhitespace) {
				Write(' ');
			}

			Write(indicator);

			isWhitespace = whitespace;
			isIndentation &= indentation;
		}
		
		private void WriteIndent()
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
		private void EmitDocumentContent(Event evt)
		{
			states.Push(EmitterState.YAML_EMIT_DOCUMENT_END_STATE);
			EmitNode(evt, false, false);
		}

		/// <summary>
		/// Expect a node.
		/// </summary>
		private void EmitNode(IParsingEvent evt, bool isMapping, bool isSimpleKey)
		{
			isMappingContext = isMapping;
			isSimpleKeyContext = isSimpleKey;

			switch (GetEventType(evt))
			{
				case EventType.YAML_ALIAS_EVENT:
					EmitAlias();
					break;

				case EventType.YAML_SCALAR_EVENT:
					EmitScalar(evt);
					break;

				case EventType.YAML_SEQUENCE_START_EVENT:
					EmitSequenceStart(evt);
					break;

				case EventType.YAML_MAPPING_START_EVENT:
					EmitMappingStart(evt);
					break;

				default:
					throw new YamlException("Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS.");
			}
		}

		/// <summary>
		/// Expect SEQUENCE-START.
		/// </summary>
		private void EmitSequenceStart(Event evt)
		{
			ProcessAnchor();
			ProcessTag();

			SequenceStart sequenceStart = (SequenceStart)evt;
			
			if (flowLevel != 0 || isCanonical || sequenceStart.Style == SequenceStyle.Flow || CheckEmptySequence()) {
				state = EmitterState.YAML_EMIT_FLOW_SEQUENCE_FIRST_ITEM_STATE;
			}
			else {
				state = EmitterState.YAML_EMIT_BLOCK_SEQUENCE_FIRST_ITEM_STATE;
			}
		}
		
		/// <summary>
		/// Check if the next events represent an empty sequence.
		/// </summary>
		private bool CheckEmptySequence()
		{
			if (events.Count < 2) {
				return false;
			}

			FakeList<Event> eventList = new FakeList<Event>(events);
			return eventList[0] is ISequenceStart && eventList[1] is ISequenceEnd;
		}

		/// <summary>
		/// Check if the next events represent an empty mapping.
		/// </summary>
		private bool CheckEmptyMapping()
		{
			if (events.Count < 2) {
				return false;
			}

			FakeList<Event> eventList = new FakeList<Event>(events);
			return eventList[0] is IMappingStart && eventList[1] is IMappingEnd;
		}
		
		/// <summary>
		/// Write a tag.
		/// </summary>
		private void ProcessTag()
		{
			if(tagData.handle == null && tagData.suffix == null) {
				return;
			}

			if (tagData.handle != null)
			{
				WriteTagHandle(tagData.handle);
				if(tagData.suffix != null) {
					WriteTagContent(tagData.suffix, false);
				}
			}
			else
			{
				WriteIndicator("!<", true, false, false);
				WriteTagContent(tagData.suffix, false);
				WriteIndicator(">", false, false, false);
			}
		}

		/// <summary>
		/// Expect MAPPING-START.
		/// </summary>
		private void EmitMappingStart(Event evt)
		{
			ProcessAnchor();
			ProcessTag();

			MappingStart mappingStart = (MappingStart)evt;

			if (flowLevel != 0 || isCanonical || mappingStart.Style == MappingStyle.Flow || CheckEmptyMapping()) {
				state = EmitterState.YAML_EMIT_FLOW_MAPPING_FIRST_KEY_STATE;
			}
			else {
				state = EmitterState.YAML_EMIT_BLOCK_MAPPING_FIRST_KEY_STATE;
			}
		}

		/// <summary>
		/// Expect SCALAR.
		/// </summary>
		private void EmitScalar(Event evt)
		{
			SelectScalarStyle(evt);
			ProcessAnchor();
			ProcessTag();
			IncreaseIndent(true, false);
			ProcessScalar();
			
			indent = indents.Pop();
			state = states.Pop();
		}
		
		/// <summary>
		/// Write a scalar.
		/// </summary>
		private void ProcessScalar()
		{
			switch (scalarData.style)
			{
				case ScalarStyle.Plain:
					WritePlainScalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.SingleQuoted:
					WriteSingleQuotedScalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.DoubleQuoted:
					WriteDoubleQuotedScalar(scalarData.value, !isSimpleKeyContext);
					break;

				case ScalarStyle.Literal:
					WriteLiteralScalar(scalarData.value);
					break;

				case ScalarStyle.Folded:
					WriteFoldedScalar(scalarData.value);
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

		private void WriteFoldedScalar(string value)
		{
			int chomp = DetermineChomping(value);
			bool breaks = true;
			bool leadingSpaces = false;

			WriteIndicator(chomp == -1 ? ">-" : chomp == +1 ? ">+" : ">", true, false, false);
			WriteIndent();

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
						WriteIndent();
						leadingSpaces = character == ' ';
					}
					if (!breaks && character == ' ' && i + 1 < value.Length && value[i + 1] != ' ' && column > bestWidth) {
						WriteIndent();
					}
					else {
						Write(character);
					}
					isIndentation = false;
					breaks = false;
				}
			}
		}
	
		private static int DetermineChomping(string value)
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

		private void WriteLiteralScalar(string value)
		{
			int chomp = DetermineChomping(value);
			bool breaks = false;

			WriteIndicator(chomp == -1 ? "|-" : chomp == +1 ? "|+" : "|", true, false, false);
			WriteIndent();

			foreach (var character in value) {
				if(IsBreak(character)) {
					WriteBreak();
					isIndentation = true;
					breaks = true;
				}
				else {
					if(breaks) {
						WriteIndent();
					}
					Write(character);
					isIndentation = false;
					breaks = false;
				}
			}
		}

		private void WriteDoubleQuotedScalar(string value, bool allowBreaks)
		{
			WriteIndicator("\"", true, false, false);

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
						WriteIndent();
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

			WriteIndicator("\"", false, false, false);

			isWhitespace = false;
			isIndentation = false;
		}

		private void WriteSingleQuotedScalar(string value, bool allowBreaks)
		{
			WriteIndicator("'", true, false, false);

			bool spaces = false;
			bool breaks = false;

			for (int index = 0; index < value.Length; ++index) {
				char character = value[index];

				if (character == ' ')
				{
					if (allowBreaks && !spaces && column > bestWidth && index != 0 && index + 1 < value.Length && value[index + 1] != ' ') {
						WriteIndent();
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
						WriteIndent();
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

			WriteIndicator("'", false, false, false);

			isWhitespace = false;
			isIndentation = false;
		}
		
		private void WritePlainScalar(string value, bool allowBreaks)
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
						WriteIndent();
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
						WriteIndent();
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
		private void IncreaseIndent(bool isFlow, bool isIndentless)
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
		private void SelectScalarStyle(Event evt)
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
		private void EmitAlias()
		{
			ProcessAnchor();
			state = states.Pop();
		}
		
		/// <summary>
		/// Write an achor.
		/// </summary>
		private void ProcessAnchor()
		{
			if (anchorData.anchor != null)
			{
				WriteIndicator(anchorData.isAlias ? "*" : "&", true, false, false);
				WriteAnchor(anchorData.anchor);
			}
		}

		private void WriteAnchor(string value)
		{
			Write(value);

			isWhitespace = false;
			isIndentation = false;
		}

		/// <summary>
		/// Expect DOCUMENT-END.
		/// </summary>
		private void EmitDocumentEnd(Event evt)
		{
			DocumentEnd documentEnd = evt as DocumentEnd;
			if (documentEnd != null)
			{
				WriteIndent();
				if(!documentEnd.IsImplicit) {
					WriteIndicator("...", true, false, false);
					WriteIndent();
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

		private void EmitFlowSequenceItem(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				WriteIndicator("[", true, true, false);
				IncreaseIndent(true, false);
				++flowLevel;
			}

			if (evt is ISequenceEnd)
			{
				--flowLevel;
				indent = indents.Pop();
				if (isCanonical && !isFirst) {
					WriteIndicator(",", false, false, false);
					WriteIndent();
				}
				WriteIndicator("]", false, false, false);
				state = states.Pop();
				return;
			}

			if (!isFirst) {
				WriteIndicator(",", false, false, false);
			}

			if (isCanonical ||  column > bestWidth) {
				WriteIndent();
			}
			
			states.Push(EmitterState.YAML_EMIT_FLOW_SEQUENCE_ITEM_STATE);

			EmitNode(evt, false, false);
		}

		/// <summary>
		/// Expect a flow key node.
		/// </summary>
		private void EmitFlowMappingKey(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				WriteIndicator("{", true, true, false);
				IncreaseIndent(true, false);
				++flowLevel;
			}

			if (evt is IMappingEnd)
			{
				--flowLevel;
				indent = indents.Pop();
				if (isCanonical && !isFirst) {
					WriteIndicator(",", false, false, false);
					WriteIndent();
				}
				WriteIndicator("}", false, false, false);
				state = states.Pop();
				return;
			}

			if (!isFirst) {
				WriteIndicator(",", false, false, false);
			}
			if (isCanonical || column > bestWidth) {
				WriteIndent();
			}

			if (!isCanonical && CheckSimpleKey())
			{
				states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_SIMPLE_VALUE_STATE);
				EmitNode(evt, true, true);
			}
			else
			{
				WriteIndicator("?", true, false, false);
				states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_VALUE_STATE);
				EmitNode(evt, true, false);
			}
		}

		private const int MaxAliasLength = 128;

		private static int SafeStringLength(string value) {
			return value != null ? value.Length : 0;
		}
		
		/// <summary>
		/// Check if the next node can be expressed as a simple key.
		/// </summary>
		private bool CheckSimpleKey()
		{
			if(events.Count < 1) {
				return false;
			}
			
			int length;
			switch (GetEventType(events.Peek()))
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
					if (!CheckEmptySequence()) {
						return false;
					}
					length =
						SafeStringLength(anchorData.anchor) +
						SafeStringLength(tagData.handle) +
						SafeStringLength(tagData.suffix);
					break;

				case EventType.YAML_MAPPING_START_EVENT:
					if (!CheckEmptySequence()) {
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
		private void EmitFlowMappingValue(Event evt, bool isSimple)
		{
			if (isSimple) {
				WriteIndicator(":", false, false, false);
			}
			else {
				if (isCanonical || column > bestWidth) {
					WriteIndent();
				}
				WriteIndicator(":", true, false, false);
			}
			states.Push(EmitterState.YAML_EMIT_FLOW_MAPPING_KEY_STATE);
			EmitNode(evt, true, false);
		}

		/// <summary>
		/// Expect a block item node.
		/// </summary>
		private void EmitBlockSequenceItem(Event evt, bool isFirst)
		{
			if (isFirst) {
				IncreaseIndent(false, (isMappingContext && !isIndentation));
			}

			if (evt is ISequenceEnd)
			{
				indent = indents.Pop();
				state = states.Pop();
				return;
			}

			WriteIndent();
			WriteIndicator("-", true, false, true);
			states.Push(EmitterState.YAML_EMIT_BLOCK_SEQUENCE_ITEM_STATE);

			EmitNode(evt, false, false);
		}
		
		/// <summary>
		/// Expect a block key node.
		/// </summary>
		private void EmitBlockMappingKey(Event evt, bool isFirst)
		{
			if (isFirst)
			{
				IncreaseIndent(false, false);
			}

			if (evt is IMappingEnd)
			{
				indent = indents.Pop();
				state = states.Pop();
				return;
			}

			WriteIndent();

			if (CheckSimpleKey())
			{
				states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_SIMPLE_VALUE_STATE);
				EmitNode(evt, true, true);
			}
			else
			{
				WriteIndicator("?", true, false, true);
				states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_VALUE_STATE);
				EmitNode(evt, true, false);
			}
		}

		/// <summary>
		/// Expect a block value node.
		/// </summary>
		private void EmitBlockMappingValue(Event evt, bool isSimple)
		{
			if (isSimple) {
				WriteIndicator(":", false, false, false);
			}
			else {
				WriteIndent();
				WriteIndicator(":", true, false, true);
			}
			states.Push(EmitterState.YAML_EMIT_BLOCK_MAPPING_KEY_STATE);
			EmitNode(evt, true, false);
		}
	}
}

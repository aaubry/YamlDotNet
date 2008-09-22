using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a single node in the YAML document.
	/// </summary>
	public abstract class YamlNode : IEnumerable<YamlNode>
	{
		private string anchor;
		private string tag;

		/// <summary>
		/// Gets or sets the anchor of the node.
		/// </summary>
		/// <value>The anchor.</value>
		public string Anchor
		{
			get
			{
				return anchor;
			}
			set
			{
				anchor = value;
			}
		}

		/// <summary>
		/// Gets or sets the tag of the node.
		/// </summary>
		/// <value>The tag.</value>
		public string Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		/// <summary>
		/// Loads the specified event.
		/// </summary>
		/// <param name="yamlEvent">The event.</param>
		/// <param name="state">The state of the document.</param>
		internal void Load(NodeEvent yamlEvent, DocumentLoadingState state)
		{
			tag = yamlEvent.Tag;
			if (yamlEvent.Anchor != null)
			{
				anchor = yamlEvent.Anchor;
				state.AddAnchor(this);
			}
		}

		/// <summary>
		/// Parses the node represented by the next event in <paramref name="events" />.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="state">The state.</param>
		/// <returns>Returns the node that has been parsed.</returns>
		internal static YamlNode ParseNode(EventReader events, DocumentLoadingState state)
		{
			if (events.Accept<Scalar>())
			{
				return new YamlScalarNode(events, state);
			}

			if (events.Accept<SequenceStart>())
			{
				return new YamlSequenceNode(events, state);
			}

			if (events.Accept<MappingStart>())
			{
				return new YamlMappingNode(events, state);
			}

			if (events.Accept<AnchorAlias>())
			{
				AnchorAlias alias = events.Expect<AnchorAlias>();
				return state.GetNode(alias.Value, false) ?? new YamlAliasNode(alias.Value);
			}

			throw new ArgumentException("The current event is of an unsupported type.", "events");
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal abstract void ResolveAliases(DocumentLoadingState state);
		
		internal abstract void Save(Emitter emitter);

		IEnumerator<YamlNode> IEnumerable<YamlNode>.GetEnumerator() {
			return GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		
		private class EmptyEnumerator : IEnumerator<YamlNode> {
			public void Reset() {
			}
			
			public YamlNode Current {
				get {
					return null;
				}
			}
			
			public bool MoveNext() {
				return false;
			}
			
			public void Dispose() {
			}
			
			private EmptyEnumerator() {
			}
			
			public static EmptyEnumerator Instance = new EmptyEnumerator();
		}
		
		internal virtual IEnumerator<YamlNode> GetEnumerator() {
			return EmptyEnumerator.Instance;
		}
	}
}
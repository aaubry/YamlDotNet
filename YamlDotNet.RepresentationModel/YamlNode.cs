using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a single node in the YAML document.
	/// </summary>
	public abstract class YamlNode
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

		/// <summary>
		/// Saves the current node to the specified emitter.
		/// </summary>
		/// <param name="emitter">The emitter where the node is to be saved.</param>
		/// <param name="state">The state.</param>
		internal void Save(Emitter emitter, EmitterState state)
		{
			if (!string.IsNullOrEmpty(anchor) && !state.EmittedAnchors.Add(anchor))
			{
				emitter.Emit(new AnchorAlias(anchor));
			}
			else
			{
				Emit(emitter, state);
			}
		}

		/// <summary>
		/// Saves the current node to the specified emitter.
		/// </summary>
		/// <param name="emitter">The emitter where the node is to be saved.</param>
		/// <param name="state">The state.</param>
		internal abstract void Emit(Emitter emitter, EmitterState state);

		/// <summary>
		/// Accepts the specified visitor by calling the appropriate Visit method on it.
		/// </summary>
		/// <param name="visitor">
		/// A <see cref="IYamlVisitor"/>.
		/// </param>
		public abstract void Accept(IYamlVisitor visitor);
	}
}
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
		/// <summary>
		/// Gets or sets the anchor of the node.
		/// </summary>
		/// <value>The anchor.</value>
		public string Anchor { get; set; }

		/// <summary>
		/// Gets or sets the tag of the node.
		/// </summary>
		/// <value>The tag.</value>
		public string Tag { get; set; }

		/// <summary>
		/// Loads the specified event.
		/// </summary>
		/// <param name="yamlEvent">The event.</param>
		/// <param name="state">The state of the document.</param>
		internal void Load(NodeEvent yamlEvent, DocumentLoadingState state)
		{
			Tag = yamlEvent.Tag;
			if (yamlEvent.Anchor != null)
			{
				Anchor = yamlEvent.Anchor;
				state.AddAnchor(this);
			}
		}

		/// <summary>
		/// Parses the node represented by the next event in <paramref name="events" />.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="state">The state.</param>
		/// <returns>Returns the node that has been parsed.</returns>
		static internal YamlNode ParseNode(EventReader events, DocumentLoadingState state)
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
			if (!string.IsNullOrEmpty(Anchor) && !state.EmittedAnchors.Add(Anchor))
			{
				emitter.Emit(new AnchorAlias(Anchor));
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

		/// <summary>
		/// Provides a basic implementation of Object.Equals 
		/// </summary>
		protected bool Equals(YamlNode other)
		{
			// Do not use the anchor in the equality comparison because that would prevent anchored nodes from being found in dictionaries.
			return SafeEquals(Tag, other.Tag);
		}

		/// <summary>
		/// Gets a value indicating whether two objects are equal.
		/// </summary>
		protected static bool SafeEquals(object first, object second)
		{
			if (first != null)
			{
				return first.Equals(second);
			}
			else if (second != null)
			{
				return second.Equals(first);
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			// Do not use the anchor in the hash code because that would prevent anchored nodes from being found in dictionaries.
			return GetHashCode(Tag);
		}

		/// <summary>
		/// Gets the hash code of the specified object, or zero if the object is null. 
		/// </summary>
		protected static int GetHashCode(object value)
		{
			return value == null ? 0 : value.GetHashCode();
		}

		/// <summary>
		/// Combines two hash codes into one. 
		/// </summary>
		protected static int CombineHashCodes(int h1, int h2)
		{
			return unchecked(((h1 << 5) + h1) ^ h2);
		}
	}
}
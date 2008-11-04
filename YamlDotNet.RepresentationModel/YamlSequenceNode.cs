using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a sequence node in the YAML document.
	/// </summary>
	[DebuggerDisplay("Count = {children.Count}")]
	public class YamlSequenceNode : YamlNode
	{
		private readonly IList<YamlNode> children = new List<YamlNode>();

		/// <summary>
		/// Gets the collection of child nodes.
		/// </summary>
		/// <value>The children.</value>
		public IList<YamlNode> Children
		{
			get
			{
				return children;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="state">The state.</param>
		internal YamlSequenceNode(EventReader events, DocumentLoadingState state)
		{
			SequenceStart sequence = events.Expect<SequenceStart>();
			Load(sequence, state);

			bool hasUnresolvedAliases = false;
			while (!events.Accept<SequenceEnd>())
			{
				YamlNode child = ParseNode(events, state);
				children.Add(child);
				hasUnresolvedAliases |= child is YamlAliasNode;
			}

			if (hasUnresolvedAliases)
			{
				state.AddNodeWithUnresolvedAliases(this);
			}

			events.Expect<SequenceEnd>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
		/// </summary>
		public YamlSequenceNode()
		{
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal override void ResolveAliases(DocumentLoadingState state)
		{
			for (int i = 0; i < children.Count; ++i)
			{
				if (children[i] is YamlAliasNode)
				{
					children[i] = state.GetNode(children[i].Anchor, true);
				}
			}
		}
		
		internal override void Save(Emitter emitter)
		{
			emitter.Emit(new SequenceStart(Anchor, Tag, true, SequenceStyle.Any));
			foreach (var node in children) {
				node.Save(emitter);
			}
			emitter.Emit(new SequenceEnd());
		}
		
		/// <summary>
		/// Accepts the specified visitor by calling the appropriate Visit method on it.
		/// </summary>
		/// <param name="visitor">
		/// A <see cref="IYamlVisitor"/>.
		/// </param>
		public override void Accept(IYamlVisitor visitor) {
			visitor.Visit(this);
		}
	}
}
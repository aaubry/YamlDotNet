using System;
using System.Collections.Generic;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a mapping node in the YAML document.
	/// </summary>
	public class YamlMappingNode : YamlNode
	{
		private readonly IDictionary<YamlNode, YamlNode> children = new Dictionary<YamlNode, YamlNode>();

		/// <summary>
		/// Gets the children of the current node.
		/// </summary>
		/// <value>The children.</value>
		public IDictionary<YamlNode, YamlNode> Children
		{
			get
			{
				return children;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="state">The state.</param>
		internal YamlMappingNode(EventReader events, DocumentState state)
		{
			MappingStart mapping = events.Expect<MappingStart>();
			Load(mapping, state);

			bool hasUnresolvedAliases = false;
			while (!events.Accept<MappingEnd>())
			{
				YamlNode key = ParseNode(events, state);
				YamlNode value = ParseNode(events, state);

				children.Add(key, value);
				
				hasUnresolvedAliases |= key is YamlAliasNode || value is YamlAliasNode;
			}
			
			if(hasUnresolvedAliases) {
				state.AddNodeWithUnresolvedAliases(this);
			}

			events.Expect<MappingEnd>();
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal override void ResolveAliases(DocumentState state)
		{
			Dictionary<YamlNode, YamlNode> keysToUpdate = null;
			foreach(KeyValuePair<YamlNode, YamlNode> entry in children)
			{
				if (entry.Key is YamlAliasNode)
				{
					if (keysToUpdate == null)
					{
						keysToUpdate = new Dictionary<YamlNode, YamlNode>();
					}
					keysToUpdate.Add(entry.Key, state.GetNode(entry.Key.Anchor, true));
				}
				if (entry.Value is YamlAliasNode)
				{
					children[entry.Key] = state.GetNode(entry.Value.Anchor, true);
				}
			}
			if (keysToUpdate != null)
			{
				foreach(KeyValuePair<YamlNode, YamlNode> entry in keysToUpdate)
				{
					YamlNode value = children[entry.Key];
					children.Remove(entry.Key);
					children.Add(entry.Value, value);
				}
			}
		}
	}
}
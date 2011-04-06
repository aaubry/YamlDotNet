using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

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
		internal YamlMappingNode(EventReader events, DocumentLoadingState state)
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

			if (hasUnresolvedAliases)
			{
				state.AddNodeWithUnresolvedAliases(this);
			}

			events.Expect<MappingEnd>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlMappingNode"/> class.
		/// </summary>
		public YamlMappingNode()
		{
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal override void ResolveAliases(DocumentLoadingState state)
		{
			Dictionary<YamlNode, YamlNode> keysToUpdate = null;
			foreach(var entry in children)
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
				foreach(var entry in keysToUpdate)
				{
					YamlNode value = children[entry.Key];
					children.Remove(entry.Key);
					children.Add(entry.Value, value);
				}
			}
		}

		/// <summary>
		/// Saves the current node to the specified emitter.
		/// </summary>
		/// <param name="emitter">The emitter where the node is to be saved.</param>
		/// <param name="state">The state.</param>
		internal override void Emit(Emitter emitter, EmitterState state)
		{
			emitter.Emit(new MappingStart(Anchor, Tag, true, MappingStyle.Any));
			foreach (var entry in children) {
				entry.Key.Save(emitter, state);
				entry.Value.Save(emitter, state);
			}
			emitter.Emit(new MappingEnd());
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
		
		/// <summary />
		public override bool Equals(object other)
		{
			var obj = other as YamlMappingNode;
			if(obj == null || !Equals(obj) || children.Count != obj.children.Count)
			{
				return false;
			}

			foreach (var entry in children)
			{
				YamlNode otherNode;
				if(!obj.children.TryGetValue(entry.Key, out otherNode) || !SafeEquals(entry.Value, otherNode))
				{
					return false;
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			var hashCode = base.GetHashCode();
			
			foreach (var entry in children)
			{
				hashCode = CombineHashCodes(hashCode, GetHashCode(entry.Key));
				hashCode = CombineHashCodes(hashCode, GetHashCode(entry.Value));
			}
			return hashCode;
		}

	}
}
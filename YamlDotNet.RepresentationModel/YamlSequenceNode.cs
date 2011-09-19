using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a sequence node in the YAML document.
	/// </summary>
	[DebuggerDisplay("Count = {children.Count}")]
	public class YamlSequenceNode : YamlNode, IEnumerable<YamlNode>
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
		/// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
		/// </summary>
		public YamlSequenceNode(params YamlNode[] children)
			: this((IEnumerable<YamlNode>)children)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSequenceNode"/> class.
		/// </summary>
		public YamlSequenceNode(IEnumerable<YamlNode> children)
		{
			foreach (var child in children)
			{
				this.children.Add(child);
			}
		}

		/// <summary>
		/// Adds the specified child to the <see cref="Children"/> collection.
		/// </summary>
		/// <param name="child">The child.</param>
		public void Add(YamlNode child)
		{
			children.Add(child);
		}

		/// <summary>
		/// Adds a scalar node to the <see cref="Children"/> collection.
		/// </summary>
		/// <param name="child">The child.</param>
		public void Add(string child)
		{
			children.Add(new YamlScalarNode(child));
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

		/// <summary>
		/// Saves the current node to the specified emitter.
		/// </summary>
		/// <param name="emitter">The emitter where the node is to be saved.</param>
		/// <param name="state">The state.</param>
		internal override void Emit(Emitter emitter, EmitterState state)
		{
			emitter.Emit(new SequenceStart(Anchor, Tag, true, SequenceStyle.Any));
			foreach (var node in children) {
				node.Save(emitter, state);
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
			
		/// <summary />
		public override bool Equals(object other)
		{
			var obj = other as YamlSequenceNode;
			if(obj == null || !Equals(obj) || children.Count != obj.children.Count)
			{
				return false;
			}
			
			for(int i = 0; i < children.Count; ++i)
			{
				if(!SafeEquals(children[i], obj.children[i]))
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
			
			foreach (var item in children)
			{
				hashCode = CombineHashCodes(hashCode, GetHashCode(item));
			}
			return hashCode;
		}


		#region IEnumerable<YamlNode> Members

		/// <summary />
		public IEnumerator<YamlNode> GetEnumerator()
		{
			return Children.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
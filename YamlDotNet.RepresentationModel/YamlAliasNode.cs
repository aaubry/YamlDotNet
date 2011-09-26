using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents an alias node in the YAML document.
	/// </summary>
	internal class YamlAliasNode : YamlNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="YamlAliasNode"/> class.
		/// </summary>
		/// <param name="anchor">The anchor.</param>
		internal YamlAliasNode(string anchor)
		{
			Anchor = anchor;
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal override void ResolveAliases(DocumentLoadingState state)
		{
			throw new NotSupportedException("Resolving an alias on an alias node does not make sense");
		}

		/// <summary>
		/// Saves the current node to the specified emitter.
		/// </summary>
		/// <param name="emitter">The emitter where the node is to be saved.</param>
		/// <param name="state">The state.</param>
		internal override void Emit(Emitter emitter, EmitterState state)
		{
			throw new NotSupportedException("A YamlAliasNode is an implementation detail and should never be saved.");
		}
		
		/// <summary>
		/// Accepts the specified visitor by calling the appropriate Visit method on it.
		/// </summary>
		/// <param name="visitor">
		/// A <see cref="IYamlVisitor"/>.
		/// </param>
		public override void Accept(IYamlVisitor visitor)
		{
			throw new NotSupportedException("A YamlAliasNode is an implementation detail and should never be visited.");
		}
		
		/// <summary />
		public override bool Equals(object other)
		{
			var obj = other as YamlAliasNode;
			return obj != null && Equals(obj) && SafeEquals(Anchor, obj.Anchor);
		}
		
		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "*" + Anchor;
		}

		/// <summary>
		/// Gets all nodes from the document, starting on the current node.
		/// </summary>
		public override IEnumerable<YamlNode> AllNodes
		{
			get { yield return this; }
		}
	}
}
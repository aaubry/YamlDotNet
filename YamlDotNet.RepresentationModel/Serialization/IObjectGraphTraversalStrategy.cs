using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Defines a strategy that walks through an object graph.
	/// </summary>
	public interface IObjectGraphTraversalStrategy
	{
		/// <summary>
		/// Traverses the specified object graph.
		/// </summary>
		/// <param name="graph">The graph.</param>
		/// <param name="visitor">An <see cref="IObjectGraphVisitor"/> that is to be notified during the traversal.</param>
		void Traverse(IObjectDescriptor graph, IObjectGraphVisitor visitor);
	}
}
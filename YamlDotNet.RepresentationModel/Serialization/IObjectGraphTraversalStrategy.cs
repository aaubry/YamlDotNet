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
		/// <param name="type">The static type of the graph.</param>
		/// <param name="visitor">An <see cref="IObjectGraphVisitor"/> that is to be notified during the traversal.</param>
		void Traverse(object graph, Type type, IObjectGraphVisitor visitor);

	    /// <summary>
	    /// Defines whether the traversal should include elements which contain the default value (e.g. null)
	    /// </summary>
	    bool EmitDefaults { get; set; }
	}
}
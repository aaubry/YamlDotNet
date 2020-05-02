using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    public interface ISchema
    {
        /// <summary>
        /// Attempts to resolve the non-specific tag of the specified scalar.
        /// </summary>
        /// <param name="node">The scalar node for which the tag sould be resolved.</param>
        /// <param name="path">An ordered sequence of the nodes that lead to this scalar (not including this one).</param>
        /// <param name="resolvedTag">The resolved tag, if any.</param>
        /// <returns>Returns true if the tag coudld be resolved; otherwise returns false.</returns>
        bool ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag);

        /// <summary>
        /// Attempts to resolve the non-specific tag of the specified mapping.
        /// </summary>
        /// <param name="node">The scalar node for which the tag sould be resolved.</param>
        /// <param name="path">An ordered sequence of the nodes that lead to this mapping (not including this one).</param>
        /// <param name="resolvedTag">The resolved tag, if any.</param>
        /// <returns>Returns true if the tag coudld be resolved; otherwise returns false.</returns>
        bool ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag);

        /// <summary>
        /// Attempts to resolve the non-specific tag of the specified sequence.
        /// </summary>
        /// <param name="node">The scalar node for which the tag sould be resolved.</param>
        /// <param name="path">An ordered sequence of the nodes that lead to this sequence (not including this one).</param>
        /// <param name="resolvedTag">The resolved tag, if any.</param>
        /// <returns>Returns true if the tag coudld be resolved; otherwise returns false.</returns>
        bool ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag);

        /// <summary>
        /// Attempts to resolve a specific tag to enrich it with schema information.
        /// </summary>
        /// <param name="tag">The tag as specified in the original YAMl document.</param>
        /// <param name="resolvedTag">The resolved tag, if any.</param>
        /// <returns>Returns true if the tag coudld be resolved; otherwise returns false.</returns>
        bool ResolveSpecificTag(TagName tag, [NotNullWhen(true)] out ITag? resolvedTag);
    }
}

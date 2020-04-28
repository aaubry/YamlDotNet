using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    public interface ISchema
    {
        /// <summary>
        /// Attempts to resolve the non-specific tag of the specified node.
        /// </summary>
        /// <param name="node">The scalar node for which the tag sould be resolved.</param>
        /// <param name="path">An ordered sequence of the nodes that lead to this scalar (not including this one).</param>
        /// <returns>A resolved tag or <see cref="TagName.Empty"/> if the tag could not be resolved.</returns>
        TagName ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path);
        TagName ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path);
        TagName ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path);
    }
}

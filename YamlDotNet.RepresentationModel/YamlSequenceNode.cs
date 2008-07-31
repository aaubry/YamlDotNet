using System;
using YamlDotNet.Core;
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
        internal YamlSequenceNode(EventReader events, DocumentState state)
        {
            using (SequenceStartEvent sequence = events.Expect<SequenceStartEvent>())
            {
                Load(sequence, state);

                while (!events.Accept<SequenceEndEvent>())
                {
                    children.Add(ParseNode(events, state));
                }
            }

            events.Expect<SequenceEndEvent>().Dispose();
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal override void ResolveAliases(DocumentState state)
        {
            for (int i = 0; i < children.Count; ++i)
            {
                if (children[i] is YamlAliasNode)
                {
                    children[i] = state.GetNode(children[i].Anchor, true);
                }
            }
        }
    }
}
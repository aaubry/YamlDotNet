using System;
using YamlDotNet.Core;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a single node in the YAML document.
    /// </summary>
    public abstract class YamlNode
    {
        private string anchor;
        private string tag;

        /// <summary>
        /// Gets or sets the anchor of the node.
        /// </summary>
        /// <value>The anchor.</value>
        public string Anchor
        {
            get
            {
                return anchor;
            }
            set
            {
                anchor = value;
            }
        }

        /// <summary>
        /// Gets or sets the tag of the node.
        /// </summary>
        /// <value>The tag.</value>
        public string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }

        /// <summary>
        /// Loads the specified event.
        /// </summary>
        /// <param name="yamlEvent">The event.</param>
        /// <param name="state">The state of the document.</param>
        internal void Load(INodeEvent yamlEvent, DocumentState state)
        {
            tag = yamlEvent.Tag;
            if (yamlEvent.Anchor != null)
            {
                anchor = yamlEvent.Anchor;
                state.AddAnchor(this);
            }
        }

        /// <summary>
        /// Parses the node represented by the next event in <paramref name="events" />.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="state">The state.</param>
        /// <returns>Returns the node that has been parsed.</returns>
        internal static YamlNode ParseNode(EventReader events, DocumentState state)
        {
            if (events.Accept<ScalarEvent>())
            {
                return new YamlScalarNode(events, state);
            }

            if (events.Accept<SequenceStartEvent>())
            {
                return new YamlSequenceNode(events, state);
            }

            if (events.Accept<MappingStartEvent>())
            {
                return new YamlMappingNode(events, state);
            }

            if (events.Accept<AliasEvent>())
            {
                using (AliasEvent alias = events.Expect<AliasEvent>())
                {
                    YamlNode target = state.GetNode(alias.Anchor, false);
                    if (target != null)
                    {
                        return target;
                    }
                    else
                    {
                        state.HasUnresolvedAliases = true;
                        return new YamlAliasNode(alias.Anchor);
                    }
                }
            }

            throw new ArgumentException("The current event is of an unsupported type.", "events");
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal abstract void ResolveAliases(DocumentState state);
    }
}
using System;
using YamlDotNet.Core;
using System.Diagnostics;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Represents a scalar node in the YAML document.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class YamlScalarNode : YamlNode
    {
        private string value;
        private ScalarStyle style;

        /// <summary>
        /// Gets or sets the value of the node.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Gets or sets the style of the node.
        /// </summary>
        /// <value>The style.</value>
        public ScalarStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="state">The state.</param>
        internal YamlScalarNode(EventReader events, DocumentState state)
        {
            using (ScalarEvent scalar = events.Expect<ScalarEvent>())
            {
                Load(scalar, state);
                value = scalar.Value;
                style = scalar.Style;
            }
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved when the node was created.
        /// </summary>
        /// <param name="state">The state of the document.</param>
        internal override void ResolveAliases(DocumentState state)
        {
            throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
        }
    }
}
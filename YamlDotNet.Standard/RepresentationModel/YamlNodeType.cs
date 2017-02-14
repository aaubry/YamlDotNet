using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Specifies the type of node in the representation model.
    /// </summary>
    public enum YamlNodeType
    {
        /// <summary>
        /// The node is a <see cref="YamlAliasNode"/>.
        /// </summary>
        Alias,

        /// <summary>
        /// The node is a <see cref="YamlMappingNode"/>.
        /// </summary>
        Mapping,

        /// <summary>
        /// The node is a <see cref="YamlScalarNode"/>.
        /// </summary>
        Scalar,

        /// <summary>
        /// The node is a <see cref="YamlSequenceNode"/>.
        /// </summary>
        Sequence
    }
}

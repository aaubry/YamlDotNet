using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    /// <summary>
    /// Implements the Failsafe schema: <see href="https://yaml.org/spec/1.2/spec.html#id2802346"/>.
    /// </summary>
    public sealed class FailsafeSchema : ISchema
    {
        private readonly TagName fallbackTag;

        private FailsafeSchema(TagName fallbackTag)
        {
            this.fallbackTag = fallbackTag;
        }

        /// <summary>
        /// A version of the <see cref="FailsafeSchema"/> that conforms strictly to the specification
        /// by not resolving any unrecognized scalars.
        /// </summary>
        public static readonly FailsafeSchema Strict = new FailsafeSchema(TagName.Empty);

        /// <summary>
        /// A version of the <see cref="FailsafeSchema"/> that treats unrecognized scalars as strings.
        /// </summary>
        public static readonly FailsafeSchema Lenient = new FailsafeSchema(YamlTagRepository.String);

        public TagName ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path)
        {
            return node.Tag.IsEmpty ? fallbackTag : YamlTagRepository.String;
        }

        public TagName ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path)
        {
            return node.Tag.IsEmpty ? node.Tag : YamlTagRepository.Mapping;
        }

        public TagName ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path)
        {
            return node.Tag.IsEmpty ? node.Tag : YamlTagRepository.Sequence;
        }
    }
}

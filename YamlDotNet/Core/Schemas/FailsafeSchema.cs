using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    /// <summary>
    /// Implements the Failsafe schema: <see href="https://yaml.org/spec/1.2/spec.html#id2802346"/>.
    /// </summary>
    public sealed class FailsafeSchema : ISchema
    {
        private readonly ITag fallbackTag;

        private FailsafeSchema(ITag fallbackTag)
        {
            this.fallbackTag = fallbackTag;
        }

        /// <summary>
        /// A version of the <see cref="FailsafeSchema"/> that conforms strictly to the specification
        /// by not resolving any unrecognized scalars.
        /// </summary>
        public static readonly FailsafeSchema Strict = new FailsafeSchema(new SimpleTag(TagName.Empty));

        /// <summary>
        /// A version of the <see cref="FailsafeSchema"/> that treats unrecognized scalars as strings.
        /// </summary>
        public static readonly FailsafeSchema Lenient = new FailsafeSchema(String);

        public static readonly ITag String = new SimpleTag(YamlTagRepository.String, s => s.Value);
        public static readonly ITag Mapping = new SimpleTag(YamlTagRepository.Mapping);
        public static readonly ITag Sequence = new SimpleTag(YamlTagRepository.Sequence);

        public bool ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (node.Tag.Name.IsEmpty)
            {
                resolvedTag = fallbackTag;
                return !fallbackTag.Name.IsEmpty;
            }

            resolvedTag = String;
            return true;
        }

        public bool ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (node.Tag.Name.IsEmpty)
            {
                resolvedTag = fallbackTag;
                return !fallbackTag.Name.IsEmpty;
            }

            resolvedTag = Mapping;
            return true;
        }

        public bool ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (node.Tag.Name.IsEmpty)
            {
                resolvedTag = fallbackTag;
                return !fallbackTag.Name.IsEmpty;
            }

            resolvedTag = Sequence;
            return true;
        }

        public bool ResolveSpecificTag(TagName tag, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (tag.Equals(String.Name))
            {
                resolvedTag = String;
                return true;
            }
            else if (tag.Equals(Sequence.Name))
            {
                resolvedTag = Sequence;
                return true;
            }
            else if (tag.Equals(Mapping.Name))
            {
                resolvedTag = Mapping;
                return true;
            }
            else
            {
                resolvedTag = null;
                return false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    public abstract class RegexBasedSchema : ISchema
    {
        protected sealed class RegexTagMappingTable : IEnumerable
        {
            private List<KeyValuePair<Regex, TagName>> entries = new List<KeyValuePair<Regex, TagName>>();

            public void Add(string pattern, TagName tag)
            {
                Add(new Regex(pattern, StandardRegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture), tag);
            }

            public void Add(Regex pattern, TagName tag)
            {
                entries.Add(new KeyValuePair<Regex, TagName>(pattern, tag));
            }

            public KeyValuePair<Regex, TagName>[] ToArray() => entries.ToArray();

            IEnumerator IEnumerable.GetEnumerator() => entries.GetEnumerator();
        }

        private readonly KeyValuePair<Regex, TagName>[] tagMappingTable;
        private readonly TagName fallbackTag;

        protected RegexBasedSchema(RegexTagMappingTable tagMappingTable)
            : this(tagMappingTable, TagName.Empty)
        {
        }

        protected RegexBasedSchema(RegexTagMappingTable tagMappingTable, TagName fallbackTag)
        {
            this.tagMappingTable = tagMappingTable.ToArray();
            this.fallbackTag = fallbackTag;
        }

        public TagName ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path)
        {
            if (!node.Tag.IsEmpty)
            {
                return YamlTagRepository.String;
            }

            var value = node.Value;
            foreach (var entry in tagMappingTable)
            {
                if (entry.Key.IsMatch(value))
                {
                    return entry.Value;
                }
            }

            return fallbackTag;
        }

        public TagName ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path)
        {
            return YamlTagRepository.Mapping;
        }

        public TagName ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path)
        {
            return YamlTagRepository.Sequence;
        }
    }
}

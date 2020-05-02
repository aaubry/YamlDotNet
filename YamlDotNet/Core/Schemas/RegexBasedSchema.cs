using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
    public abstract class RegexBasedSchema : ISchema
    {
        protected interface IRegexBasedTag : ITag
        {
            bool Matches(string value, [NotNullWhen(true)] out ITag? resultingTag);
        }

        private sealed class CompositeRegexBasedTag : IRegexBasedTag
        {
            private readonly IRegexBasedTag[] subTags;

            public TagName Name { get; }
            public ScalarParser? ScalarParser { get; }

            public CompositeRegexBasedTag(TagName name, IEnumerable<IRegexBasedTag> subTags)
            {
                Name = name;
                this.subTags = subTags.ToArray();

                ScalarParser = ParseScalar;
            }

            private object? ParseScalar(Scalar scalar)
            {
                var value = scalar.Value;
                foreach (var subTag in subTags)
                {
                    if (subTag.Matches(value, out var resultingTag))
                    {
                        return resultingTag.ScalarParser!(scalar);
                    }
                }

                throw new SemanticErrorException($"The value '{value}' could not be parsed as '{Name}'.");
            }

            public bool Matches(string value, [NotNullWhen(true)] out ITag? resultingTag)
            {
                foreach (var subTag in subTags)
                {
                    if (subTag.Matches(value, out resultingTag))
                    {
                        return true;
                    }
                }

                resultingTag = null;
                return false;
            }
        }

        private sealed class RegexBasedTag : IRegexBasedTag
        {
            private readonly Regex pattern;
            public TagName Name { get; }
            public ScalarParser? ScalarParser { get; }

            public RegexBasedTag(TagName name, Regex pattern, ScalarParser scalarParser)
            {
                Name = name;
                this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
                ScalarParser = scalarParser ?? throw new ArgumentNullException(nameof(scalarParser));
            }

            public bool Matches(string value, [NotNullWhen(true)] out ITag? resultingTag)
            {
                resultingTag = this;
                return pattern.IsMatch(value);
            }
        }

        protected sealed class RegexTagMappingTable : IEnumerable<IRegexBasedTag>
        {
            private readonly List<IRegexBasedTag> entries = new List<IRegexBasedTag>();

            public void Add(string pattern, TagName tag, ScalarParser parser)
            {
                Add(new Regex(pattern, StandardRegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture), tag, parser);
            }

            public void Add(Regex pattern, TagName tag, ScalarParser parser)
            {
                entries.Add(new RegexBasedTag(tag, pattern, parser));
            }

            public IEnumerator<IRegexBasedTag> GetEnumerator() => entries.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly IDictionary<TagName, IRegexBasedTag> tags;
        private readonly ITag fallbackTag;

        protected RegexBasedSchema(RegexTagMappingTable tagMappingTable)
            : this(tagMappingTable, new SimpleTag(TagName.Empty))
        {
        }

        protected RegexBasedSchema(RegexTagMappingTable tagMappingTable, ITag fallbackTag)
        {
            this.tags = tagMappingTable
                .GroupBy(e => e.Name)
                .Select(g => g.Count() switch
                {
                    1 => g.First(),
                    _ => new CompositeRegexBasedTag(g.Key, g)
                })
                .ToDictionary(e => e.Name);

            this.fallbackTag = fallbackTag;
        }

        public bool ResolveNonSpecificTag(Scalar node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (!node.Tag.Name.IsEmpty)
            {
                resolvedTag = FailsafeSchema.String;
                return true;
            }

            var value = node.Value;
            foreach (var tag in tags.Values)
            {
                if (tag.Matches(value, out resolvedTag))
                {
                    return true;
                }
            }

            resolvedTag = fallbackTag;
            return !fallbackTag.Name.IsEmpty;
        }

        public bool ResolveNonSpecificTag(MappingStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            resolvedTag = FailsafeSchema.Mapping;
            return true;
        }

        public bool ResolveNonSpecificTag(SequenceStart node, IEnumerable<NodeEvent> path, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            resolvedTag = FailsafeSchema.Sequence;
            return true;
        }

        public bool ResolveSpecificTag(TagName tag, [NotNullWhen(true)] out ITag? resolvedTag)
        {
            if (tags.TryGetValue(tag, out var result))
            {
                resolvedTag = result;
                return true;
            }
            else if (tag.Equals(fallbackTag.Name))
            {
                resolvedTag = fallbackTag;
                return true;
            }

            resolvedTag = null;
            return false;
        }
    }
}
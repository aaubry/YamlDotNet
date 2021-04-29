//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class TypeSchema : ISchema
    {
        private List<NodeMatcher> rootMatchers;

        public TypeSchema(TypeMatcherTable typeMatchers, Type root, params Type[] wellKnownTypes)
            : this(typeMatchers, root, (IEnumerable<Type>)wellKnownTypes)
        {
        }

        public TypeSchema(TypeMatcherTable typeMatchers, Type root, IEnumerable<Type> wellKnownTypes)
        {
            rootMatchers = new List<NodeMatcher>();
            if (root != typeof(object))
            {
                rootMatchers.Add(typeMatchers.GetNodeMatcher(root));
            }

            foreach (var wellKnownType in wellKnownTypes)
            {
                rootMatchers.Add(typeMatchers.GetNodeMatcher(wellKnownType));
            }

            Root = new RootNodeMatchersIterator(rootMatchers);
        }

        public override string ToString() => Root.ToString()!;
        public string DebugView => ToString();

        public ISchemaIterator Root { get; }

        public IEnumerable<NodeMatcher> RootMatchers => rootMatchers;

        private sealed class SingleNodeMatcherIterator : ISchemaIterator
        {
            private readonly NodeMatcher matcher;
            private readonly ISchemaIterator root;
            private readonly IEnumerable<NodeMatcher>? valueMatchers;

            public SingleNodeMatcherIterator(NodeMatcher matcher, ISchemaIterator root)
            {
                this.matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
                this.root = root ?? throw new ArgumentNullException(nameof(root));
            }

            public SingleNodeMatcherIterator(NodeMatcher matcher, ISchemaIterator root, IEnumerable<NodeMatcher> valueMatchers)
                : this(matcher, root)
            {
                this.valueMatchers = valueMatchers ?? throw new ArgumentNullException(nameof(valueMatchers));
            }

            public bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                switch (matcher)
                {
                    case SequenceMatcher sequenceMatcher:
                        foreach (var itemMatcher in sequenceMatcher.ItemMatchers)
                        {
                            if (itemMatcher.Matches(node))
                            {
                                mapper = itemMatcher.Mapper;
                                childIterator = new SingleNodeMatcherIterator(itemMatcher, root);
                                return true;
                            }
                        }
                        break;

                    case MappingMatcher mappingMatcher:
                        foreach (var (keyMatcher, valueMatchers) in mappingMatcher.ItemMatchers)
                        {
                            if (keyMatcher.Matches(node))
                            {
                                mapper = keyMatcher.Mapper;
                                childIterator = new SingleNodeMatcherIterator(keyMatcher, root, valueMatchers);
                                return true;
                            }
                        }
                        break;

                    case ScalarMatcher _:
                        // Should not happen
                        break;
                }

                return root.TryEnterNode(node, out childIterator, out mapper);
            }

            public bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                if (value != null)
                {
                    var typeOfValue = value.GetType();
                    switch (matcher)
                    {
                        case SequenceMatcher sequenceMatcher:
                            foreach (var itemMatcher in sequenceMatcher.ItemMatchers)
                            {
                                if (itemMatcher.Matches(typeOfValue))
                                {
                                    mapper = itemMatcher.Mapper;
                                    childIterator = new SingleNodeMatcherIterator(itemMatcher, root);
                                    return true;
                                }
                            }
                            break;

                        case MappingMatcher mappingMatcher:
                            foreach (var (keyMatcher, valueMatchers) in mappingMatcher.ItemMatchers)
                            {
                                if (keyMatcher.Matches(typeOfValue))
                                {
                                    mapper = keyMatcher.Mapper;
                                    childIterator = new SingleNodeMatcherIterator(keyMatcher, root, valueMatchers);
                                    return true;
                                }
                            }
                            break;

                        case ScalarMatcher _:
                            // Should not happen
                            break;
                    }
                }

                return root.TryEnterValue(value, out childIterator, out mapper);
            }

            public bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator)
            {
                if (valueMatchers != null)
                {
                    childIterator = new MultipleNodeMatchersIterator(valueMatchers, root);
                    return true;
                }
                else
                {
                    // There's no need to fall back to the root because it will never recognize mapping values
                    childIterator = null;
                    return false;
                }
            }

            public bool IsTagImplicit(IScalar scalar, out ScalarStyle style)
            {
                if (matcher is ScalarMatcher scalarMatcher && scalarMatcher.MatchesContent(scalar))
                {
                    var plainAllowed = scalar.Value.Length > 0;
                    style = plainAllowed
                        ? ScalarStyle.Plain
                        : scalarMatcher.SuggestedStyle;

                    return true;
                }

                style = default;
                return false;
            }

            public bool IsTagImplicit(ISequence sequence, out SequenceStyle style)
            {
                if (matcher is SequenceMatcher sequenceMatcher && sequenceMatcher.Matches(sequence))
                {
                    style = sequenceMatcher.SuggestedStyle;
                    return true;
                }

                style = default;
                return false;
            }

            public bool IsTagImplicit(IMapping mapping, out MappingStyle style)
            {
                if (matcher is MappingMatcher mappingMatcher && mappingMatcher.Matches(mapping))
                {
                    style = mappingMatcher.SuggestedStyle;
                    return true;
                }

                style = default;
                return false;
            }

            public override string ToString() => matcher.ToString();
        }

        private sealed class MultipleNodeMatchersIterator : ISchemaIterator
        {
            private readonly IEnumerable<NodeMatcher> nodeMatchers;
            private readonly ISchemaIterator root;

            public MultipleNodeMatchersIterator(IEnumerable<NodeMatcher> nodeMatchers, ISchemaIterator root)
            {
                this.nodeMatchers = nodeMatchers ?? throw new ArgumentNullException(nameof(nodeMatchers));
                this.root = root ?? throw new ArgumentNullException(nameof(root));
            }

            public bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                var matcher = nodeMatchers.FirstOrDefault(m => m.Matches(node));
                if (matcher != null)
                {
                    mapper = matcher.Mapper;
                    childIterator = new SingleNodeMatcherIterator(matcher, root);
                    return true;
                }
                else
                {
                    return root.TryEnterNode(node, out childIterator, out mapper);
                }
            }

            public bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                if (value != null)
                {
                    var typeOfValue = value.GetType();
                    var matcher = nodeMatchers.FirstOrDefault(m => m.Matches(typeOfValue));
                    if (matcher != null)
                    {
                        mapper = matcher.Mapper;
                        childIterator = new SingleNodeMatcherIterator(matcher, root);
                        return true;
                    }
                }

                return root.TryEnterValue(value, out childIterator, out mapper);
            }

            public bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator)
            {
                // There's no need to fall back to the root because it will never recognize mapping values
                childIterator = null;
                return false;
            }

            public bool IsTagImplicit(IScalar scalar, out ScalarStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public bool IsTagImplicit(ISequence sequence, out SequenceStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public bool IsTagImplicit(IMapping mapping, out MappingStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public override string ToString() => string.Join(", ", nodeMatchers.Select(m => m.ToString()).ToArray());
        }

        private sealed class RootNodeMatchersIterator : ISchemaIterator
        {
            private readonly IEnumerable<NodeMatcher> nodeMatchers;

            public RootNodeMatchersIterator(IEnumerable<NodeMatcher> nodeMatchers)
            {
                this.nodeMatchers = nodeMatchers ?? throw new ArgumentNullException(nameof(nodeMatchers));
            }

            public bool TryEnterNode(INode node, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                var matcher = nodeMatchers.FirstOrDefault(m => m.Matches(node));
                if (matcher != null)
                {
                    mapper = matcher.Mapper;
                    childIterator = new SingleNodeMatcherIterator(matcher, this);
                    return true;
                }
                else
                {
                    mapper = null;
                    childIterator = null;
                    return false;
                }
            }

            public bool TryEnterValue(object? value, [NotNullWhen(true)] out ISchemaIterator? childIterator, [NotNullWhen(true)] out INodeMapper? mapper)
            {
                if (value != null)
                {
                    var typeOfValue = value.GetType();
                    var matcher = nodeMatchers.FirstOrDefault(m => m.Matches(typeOfValue));
                    if (matcher != null)
                    {
                        mapper = matcher.Mapper;
                        childIterator = new SingleNodeMatcherIterator(matcher, this);
                        return true;
                    }
                }

                mapper = null;
                childIterator = null;
                return false;
            }

            public bool TryEnterMappingValue([NotNullWhen(true)] out ISchemaIterator? childIterator)
            {
                childIterator = null;
                return false;
            }

            public bool IsTagImplicit(IScalar scalar, out ScalarStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public bool IsTagImplicit(ISequence sequence, out SequenceStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public bool IsTagImplicit(IMapping mapping, out MappingStyle style)
            {
                throw new NotImplementedException("TODO");
            }

            public override string ToString() => string.Join(", ", nodeMatchers.Select(m => m.ToString()).ToArray());
        }
    }
}

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
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class TypeSchema : ISchema
    {
        private readonly List<NodeMatcher> nodeMatchers;
        private readonly ICache<Type, NodeMatcher> nodeMatchersCache;
        private readonly ITagNameResolver tagNameResolver;
        private readonly ITypeInspector typeInspector;
        private readonly INamingConvention namingConvention;
        private readonly bool ignoreUnmatched;

        public TypeSchema(
            ISchema baseSchema,
            ITagNameResolver tagNameResolver,
            ITypeInspector typeInspector,
            INamingConvention namingConvention,
            bool ignoreUnmatched,
            Type root,
            IEnumerable<Type> wellKnownTypes,
            bool requireThreadSafety
        )
        {
            this.tagNameResolver = tagNameResolver;
            this.typeInspector = typeInspector;
            this.namingConvention = namingConvention;
            this.ignoreUnmatched = ignoreUnmatched;

            if (requireThreadSafety)
            {
                nodeMatchersCache = new ThreadSafeCache<Type, NodeMatcher>();
            }
            else
            {
                nodeMatchersCache = new SingleThreadCache<Type, NodeMatcher>();
            }

            this.nodeMatchers = baseSchema.RootMatchers.ToList();
            if (root != typeof(object))
            {
                this.nodeMatchers.Insert(0, GetCachedNodeMatcher(root));
            }

            foreach (var wellKnownType in wellKnownTypes)
            {
                this.nodeMatchers.Add(GetCachedNodeMatcher(wellKnownType));
            }

            Root = new RootNodeMatchersIterator(this.nodeMatchers);
        }

        public override string ToString() => Root.ToString()!;
        public string DebugView => ToString();

        public ISchemaIterator Root { get; }

        public IEnumerable<NodeMatcher> RootMatchers => nodeMatchers;

        private NodeMatcher GetCachedNodeMatcher(Type type)
        {
            if (type.IsGenericParameter)
            {
                throw new ArgumentException("Cannot get a node matcher for a generic parameter.", nameof(type));
            }

            return nodeMatchersCache.GetOrAdd(type, ResolveNodeMatcher);
        }

        private (NodeMatcher, Action?) ResolveNodeMatcher(Type type)
        {
            foreach (var candidate in GetSuperTypes(type))
            {
                foreach (var nodeMatcher in nodeMatchers)
                {
                    if (nodeMatcher.Matches(candidate))
                    {
                        return (nodeMatcher, null);
                    }
                }

                if (candidate == typeof(object))
                {
                    return CreateObjectMatcher(type);
                }

                if (candidate.IsGenericType())
                {
                    var genericCandidateType = candidate.GetGenericTypeDefinition();
                    if (genericCandidateType == typeof(IEnumerable<>))
                    {
                        return CreateEnumerableMatcher(type, candidate);
                    }
                    if (genericCandidateType == typeof(IDictionary<,>))
                    {
                        return CreateDictionartyMatcher(type, candidate);
                    }
                }
            }

            throw new ArgumentException($"Could not resolve a tag for type '{type.FullName}'.");
        }

        private (NodeMatcher nodeMatcher, Action? afterCreation) CreateEnumerableMatcher(Type concrete, Type iEnumerable)
        {
            var tag = tagNameResolver.Resolve(concrete);

            var genericArguments = iEnumerable.GetGenericArguments();
            var itemType = genericArguments[0];

            var implementation = concrete;
            if (concrete.IsInterface())
            {
                implementation = typeof(List<>).MakeGenericType(genericArguments);
            }

            var matcher = NodeMatcher
                .ForSequences(SequenceMapper.Create(tag, implementation, itemType), concrete)
                .Either(
                    s => s.MatchEmptyTags(),
                    s => s.MatchTag(tag)
                )
                .Create();

            return (
                matcher,
                () => matcher.AddItemMatcher(GetCachedNodeMatcher(itemType))
            );
        }

        private (NodeMatcher nodeMatcher, Action? afterCreation) CreateDictionartyMatcher(Type concrete, Type iDictionary)
        {
            var tag = tagNameResolver.Resolve(concrete);

            var genericArguments = iDictionary.GetGenericArguments();
            var keyType = genericArguments[0];
            var valueType = genericArguments[1];

            var implementation = concrete;
            if (concrete.IsInterface())
            {
                implementation = typeof(Dictionary<,>).MakeGenericType(genericArguments);
            }

            var matcher = NodeMatcher
                .ForMappings(MappingMapper.Create(tag, implementation, keyType, valueType), concrete)
                .Either(
                    s => s.MatchEmptyTags(),
                    s => s.MatchTag(tag)
                )
                .Create();

            return (
                matcher,
                () =>
                {
                    matcher.AddItemMatcher(
                        keyMatcher: GetCachedNodeMatcher(keyType),
                        valueMatchers: GetCachedNodeMatcher(valueType)
                    );
                }
            );
        }

        private (NodeMatcher nodeMatcher, Action? afterCreation) CreateObjectMatcher(Type concrete)
        {
            if (concrete == typeof(object))
            {
                return (NodeMatcher.NoMatch, null);
            }

            var tag = tagNameResolver.Resolve(concrete);

            var properties = typeInspector.GetProperties(concrete, null).OrderBy(p => p.Order);
            //var mapper = new ObjectMapper2(concrete, properties, tag, ignoreUnmatched);
            var mapper = new ObjectMapper(concrete, properties, tag, ignoreUnmatched);

            var matcher = NodeMatcher
                .ForMappings(mapper, concrete)
                .Either(
                    s => s.MatchEmptyTags(),
                    s => s.MatchTag(tag)
                )
                .Create();

            return (
                matcher,
                () =>
                {
                    // TODO: Update the object mapper with the specific properties that exist (or create it complete from the start)

                    {
                        foreach (var property in properties)
                        {
                            var keyName = namingConvention.Apply(property.Name);

                            // TODO: Use the following:
                            //        - property.CanWrite
                            //        - property.ScalarStyle
                            //        - property.TypeOverride

                            matcher.AddItemMatcher(
                                keyMatcher: NodeMatcher
                                    .ForScalars(new TranslateStringMapper(keyName, property.Name))
                                    .MatchValue(keyName)
                                    .Create(),
                                valueMatchers: GetCachedNodeMatcher(property.Type)
                            );
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Returns type and all its parent classes and implemented interfaces in this order:
        /// 1. the type itself;
        /// 2. its superclasses, starting on the base class, except object;
        /// 3. all interfaces implemented by the type;
        /// 4. typeof(object).
        /// </summary>
        private static IEnumerable<Type> GetSuperTypes(Type type)
        {
            if (type.IsInterface())
            {
                yield return type;
            }
            else
            {
                Type? ancestor = type;
                // Object will be returned last
                while (ancestor != null && ancestor != typeof(object))
                {
                    yield return ancestor;
                    ancestor = ancestor.BaseType();
                }
            }
            foreach (var itf in type.GetInterfaces())
            {
                yield return itf;
            }
            yield return typeof(object);
        }


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
                    childIterator = root;
                    return true;
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
                childIterator = root;
                return true;
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

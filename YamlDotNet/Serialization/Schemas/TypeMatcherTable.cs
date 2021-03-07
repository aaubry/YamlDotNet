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
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Helpers;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Serialization.Schemas
{
    public delegate NodeMatcher NodeMatcherFactory(Type sourceType, Type matchedType, Func<Type, NodeMatcher> nodeMapperLookup);
    public delegate (NodeMatcher nodeMatcher, Action? afterCreation) TwoStepNodeMatcherFactory(Type sourceType, Type matchedType, Func<Type, NodeMatcher> nodeMapperLookup);

    public sealed class TypeMatcherTable : IEnumerable
    {
        private readonly List<NodeMatcher> nodeMatchers = new List<NodeMatcher>();
        private readonly Dictionary<Type, TwoStepNodeMatcherFactory> nodeMatcherFactories = new Dictionary<Type, TwoStepNodeMatcherFactory>();
        private readonly ICache<Type, NodeMatcher> nodeMatchersByType;

        public TypeMatcherTable(bool requireThreadSafety)
        {
            if (requireThreadSafety)
            {
                nodeMatchersByType = new ThreadSafeCache<Type, NodeMatcher>();
            }
            else
            {
                nodeMatchersByType = new SingleThreadCache<Type, NodeMatcher>();
            }
        }

        public void Add(NodeMatcher nodeMatcher)
        {
            nodeMatchers.Add(nodeMatcher);
        }

        public void Add(IEnumerable<NodeMatcher> nodeMatchers)
        {
            this.nodeMatchers.AddRange(nodeMatchers);
        }

        public void Add(Type type, NodeMatcherFactory nodeMatcherFactory)
        {
            nodeMatcherFactories.Add(type, (s, m, l) => (nodeMatcherFactory(s, m, l), null));
        }

        public void Add(Type type, TwoStepNodeMatcherFactory nodeMatcherFactory)
        {
            nodeMatcherFactories.Add(type, nodeMatcherFactory);
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("This class implements IEnumerable only to allow collection initialization.");

        public NodeMatcher GetNodeMatcher(Type type)
        {
            if (type.IsGenericParameter)
            {
                throw new ArgumentException("Cannot get a node matcher for a generic parameter.", nameof(type));
            }

            return nodeMatchersByType.GetOrAdd(type, ResolveNodeMatcher);
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

                if (nodeMatcherFactories.TryGetValue(candidate, out var concreteMatcherFactory))
                {
                    return concreteMatcherFactory(type, candidate, GetNodeMatcher);
                }

                if (candidate.IsGenericType() && nodeMatcherFactories.TryGetValue(candidate.GetGenericTypeDefinition(), out var genericMatcherFactory))
                {
                    return genericMatcherFactory(type, candidate, GetNodeMatcher);
                }
            }

            throw new ArgumentException($"Could not resolve a tag for type '{type.FullName}'.");
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
    }
}

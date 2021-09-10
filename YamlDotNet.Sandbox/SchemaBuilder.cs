using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public class SchemaBuilder : ISchemaBuilder, IEnumerable
    {
        private readonly Dictionary<Type, List<ISchemaNodeFactory<Scalar /*, TValue*/>>> scalarNodes = new();
        private readonly Dictionary<Type, List<ISchemaNodeFactory<Sequence /*, TValue*/>>> sequenceNodes = new();
        private readonly Dictionary<Type, List<ISchemaNodeFactory<Mapping /*, TValue*/>>> mappingNodes = new();

        public void Add<TValue>(Func<ISchemaBuilder, ISchemaNode<Scalar, TValue>> nodeFactory, Expression<Func<Scalar, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate)
        {
            Add(new SchemaNodeFactory<Scalar, TValue>(nodePredicate, valuePredicate, nodeFactory));
        }

        public void Add<TValue>(Func<ISchemaBuilder, ISchemaNode<Sequence, TValue>> nodeFactory, Expression<Func<Sequence, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate)
        {
            Add(new SchemaNodeFactory<Sequence, TValue>(nodePredicate, valuePredicate, nodeFactory));
        }

        public void Add<TValue>(Func<ISchemaBuilder, ISchemaNode<Mapping, TValue>> nodeFactory, Expression<Func<Mapping, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate)
        {
            Add(new SchemaNodeFactory<Mapping, TValue>(nodePredicate, valuePredicate, nodeFactory));
        }

        public void Add<TValue>(ISchemaNodeFactory<Scalar, TValue> schemaNodeFactory) => AddToLookup(scalarNodes, schemaNodeFactory);
        public void Add<TValue>(ISchemaNodeFactory<Sequence, TValue> schemaNodeFactory) => AddToLookup(sequenceNodes, schemaNodeFactory);
        public void Add<TValue>(ISchemaNodeFactory<Mapping, TValue> schemaNodeFactory) => AddToLookup(mappingNodes, schemaNodeFactory);

        private static void AddToLookup<TNode, TValue>(Dictionary<Type, List<ISchemaNodeFactory<TNode>>> lookup, ISchemaNodeFactory<TNode, TValue> schemaNodeFactory)
            where TNode : Node
        {
            if (!lookup.TryGetValue(typeof(TValue), out var nodesForType))
            {
                nodesForType = new();
                lookup.Add(typeof(TValue), nodesForType);
            }

            nodesForType.Add(schemaNodeFactory);
        }

        private sealed class SchemaBuilderState : ISchemaBuilder
        {
            // Marker
            private interface ISchemaBuilderCacheEntry { }

            private sealed class SchemaBuilderCacheEntry<TValue> : ISchemaBuilderCacheEntry
            {
                public bool IsConstructing;
                public bool IsRecursive;
                public ISchemaNode<Node, TValue> SchemaNode;

                public SchemaBuilderCacheEntry(ISchemaNode<Node, TValue> schemaNode)
                {
                    IsConstructing = true;
                    SchemaNode = schemaNode;
                }
            }

            private readonly Dictionary<Type, ISchemaBuilderCacheEntry> schemaNodeCache = new();
            private readonly SchemaBuilder schemaBuilder;

            public SchemaBuilderState(SchemaBuilder schemaBuilder)
            {
                this.schemaBuilder = schemaBuilder;
            }

            public ISchemaNode<Node, TValue> BuildSchema<TValue>()
            {
                SchemaBuilderCacheEntry<TValue> cacheEntry;
                if (schemaNodeCache.TryGetValue(typeof(TValue), out var cacheEntryAsObject))
                {
                    cacheEntry = (SchemaBuilderCacheEntry<TValue>)cacheEntryAsObject;
                    if (cacheEntry.IsConstructing && !cacheEntry.IsRecursive)
                    {
                        // Recursion was detected. We need to switch to a recursive ISchemaNode<Node, TValue>
                        cacheEntry.SchemaNode = new RecursiveSchemaNode<TValue>(cacheEntry.SchemaNode);
                        cacheEntry.IsRecursive = true;
                    }

                    return cacheEntry.SchemaNode;
                }

                var choice = new ChoiceSchemaNode<TValue>();
                cacheEntry = new SchemaBuilderCacheEntry<TValue>(choice);
                schemaNodeCache.Add(typeof(TValue), cacheEntry);

                AddNodes(schemaBuilder.scalarNodes, choice.Add);
                AddNodes(schemaBuilder.sequenceNodes, choice.Add);
                AddNodes(schemaBuilder.mappingNodes, choice.Add);

                cacheEntry.IsConstructing = false;

                // The cache entry may have been modified due to recursion, so we can't return choice directly.
                return cacheEntry.SchemaNode;

                void AddNodes<TNode>(Dictionary<Type, List<ISchemaNodeFactory<TNode>>> lookup, Action<ISchemaNode<TNode, TValue>, Expression<Func<TNode, bool>>, Expression<Func<TValue, bool>>> addToChoice)
                    where TNode : Node
                {
                    if (lookup.TryGetValue(typeof(TValue), out var scalarNodesForType))
                    {
                        foreach (ISchemaNodeFactory<TNode, TValue> nodeFactory in scalarNodesForType)
                        {
                            addToChoice(nodeFactory.BuildNode(this), nodeFactory.BuildNodePredicate(), nodeFactory.BuildValuePredicate());
                        }
                    }
                }
            }
        }

        public ISchemaNode<Node, TValue> BuildSchema<TValue>()
        {
            var state = new SchemaBuilderState(this);
            return state.BuildSchema<TValue>();
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }
}

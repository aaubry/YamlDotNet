using System;
using System.Linq.Expressions;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public sealed class SchemaNodeFactory<TNode, TValue> : ISchemaNodeFactory<TNode, TValue>
        where TNode : Node
    {
        private readonly Expression<Func<TNode, bool>> nodePredicate;
        private readonly Expression<Func<TValue, bool>> valuePredicate;
        private readonly Func<ISchemaBuilder, ISchemaNode<TNode, TValue>> nodeFactory;

        public SchemaNodeFactory(Expression<Func<TNode, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, Func<ISchemaBuilder, ISchemaNode<TNode, TValue>> nodeFactory)
        {
            this.nodePredicate = nodePredicate;
            this.valuePredicate = valuePredicate;
            this.nodeFactory = nodeFactory;
        }

        public ISchemaNode<TNode, TValue> BuildNode(ISchemaBuilder schemaBuilder) => nodeFactory(schemaBuilder);
        public Expression<Func<TNode, bool>> BuildNodePredicate() => nodePredicate;
        public Expression<Func<TValue, bool>> BuildValuePredicate() => valuePredicate;
    }
}

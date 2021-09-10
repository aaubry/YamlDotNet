using System;
using System.Linq.Expressions;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    // Marker
    public interface ISchemaNodeFactory<TNode>
        where TNode : Node
    { }

    public interface ISchemaNodeFactory<TNode, TValue> : ISchemaNodeFactory<TNode>
        where TNode : Node
    {
        Expression<Func<TNode, bool>> BuildNodePredicate();
        Expression<Func<TValue, bool>> BuildValuePredicate();
        ISchemaNode<TNode, TValue> BuildNode(ISchemaBuilder schemaBuilder);
    }
}

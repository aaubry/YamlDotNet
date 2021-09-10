using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public interface ISchemaNode
    {
        Expression GenerateConstructor(Expression node, IVariableAllocator variableAllocator);
        Expression GenerateRepresenter(Expression value, IVariableAllocator variableAllocator);

        void RenderGraph(SchemaNodeRenderer renderer, string id);

        ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style);
        ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style);
        ISchemaNode EnterMapping(ref TagName tag, MappingStyle style);
        ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode);

        object Identity { get; }

        //TagName ResolveScalarTag(string value, ScalarStyle style) => throw new NotSupportedException();
        //TagName ResolveSequenceTag(SequenceStyle style) => throw new NotSupportedException();
        //TagName ResolveMappingTag(MappingStyle style) => throw new NotSupportedException();
    }

    public interface ISchemaNode<TNode, TValue> : ISchemaNode
        where TNode : INode
    {
        Expr<TValue> GenerateConstructor(Expr<TNode> node, IVariableAllocator variableAllocator);
        Expr<TNode> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator);
    }
}

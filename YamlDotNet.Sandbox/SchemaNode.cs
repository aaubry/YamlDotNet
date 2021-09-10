using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public abstract class SchemaNode<TNode, TValue> : ISchemaNode<TNode, TValue>
        where TNode : Node
    {
        public virtual object Identity => this;

        public abstract Expr<TValue> GenerateConstructor(Expr<TNode> node, IVariableAllocator variableAllocator);
        public abstract Expr<TNode> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator);

        public Expression GenerateConstructor(Expression node, IVariableAllocator variableAllocator) => GenerateConstructor(node.As<TNode>(), variableAllocator);
        public Expression GenerateRepresenter(Expression value, IVariableAllocator variableAllocator) => GenerateRepresenter(value.As<TValue>(), variableAllocator);

        public abstract void RenderGraph(SchemaNodeRenderer renderer, string id);

        // TODO: Make abstract
        public virtual ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style) => throw NotSupported(GetType());

        // TODO: Make abstract
        public virtual ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style) => throw NotSupported(GetType());

        // TODO: Make abstract
        public virtual ISchemaNode EnterMapping(ref TagName tag, MappingStyle style) => throw NotSupported(GetType());
        
        // TODO: Make abstract
        public virtual ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode) => throw NotSupported(GetType());

        private static Exception NotSupported(Type implementer, [CallerMemberName] string methodName = null!) => new NotSupportedException($"Method {methodName} is not supported by {implementer.FullName}");
    }
}

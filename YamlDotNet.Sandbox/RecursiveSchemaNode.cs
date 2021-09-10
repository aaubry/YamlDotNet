using System;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;

namespace YamlDotNet.Sandbox
{
    public class RecursiveSchemaNode<TValue> : SchemaNode<Node, TValue>
    {
        private readonly ISchemaNode<Node, TValue> baseSchemaNode;

        public RecursiveSchemaNode(ISchemaNode<Node, TValue> baseSchemaNode)
        {
            this.baseSchemaNode = baseSchemaNode;
        }

        public override ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style) => baseSchemaNode.EnterScalar(ref tag, value, style);
        public override ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style) => baseSchemaNode.EnterSequence(ref tag, style);
        public override ISchemaNode EnterMapping(ref TagName tag, MappingStyle style) => baseSchemaNode.EnterMapping(ref tag, style);
        public override ISchemaNode EnterMappingKey(Node key, ISchemaNode schemaNode) => baseSchemaNode.EnterMappingKey(key, schemaNode);

        public override Expr<TValue> GenerateConstructor(Expr<Node> node, IVariableAllocator variableAllocator)
        {
            // Store the constructor as a lambda-expression that can be called recursively.
            var param = variableAllocator.Allocate(this, () =>
            {
                var nodeParam = Expr.Parameter<Node>("node");
                return Expression.Lambda<Func<Node, TValue>>(
                    baseSchemaNode.GenerateConstructor(nodeParam, variableAllocator),
                    nodeParam
                );
            });
            return Expression.Invoke(param, node).As<TValue>();
        }

        public override Expr<Node> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
        {
            // Store the representer as a lambda-expression that can be called recursively.
            var param = variableAllocator.Allocate(this, () =>
            {
                var valueParam = Expr.Parameter<TValue>("value");
                return Expression.Lambda<Func<TValue, Node>>(
                    baseSchemaNode.GenerateRepresenter(valueParam, variableAllocator),
                    valueParam
                );
            });
            return Expression.Invoke(param, value).As<Node>();
        }

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            renderer
                .WriteLine($"{id} [shape=octagon, label=\"recursion\"];")
                .WriteLine($"{id} -> {renderer.GetNodeId(baseSchemaNode)};");
        }
    }
}

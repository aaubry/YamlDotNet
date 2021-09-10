using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using static YamlDotNet.Sandbox.Program;

namespace YamlDotNet.Sandbox
{
    public class ChoiceSchemaNode<TValue> : SchemaNode<Node, TValue>, IEnumerable
    {
        // TODO: Lazy initialization of the lists since they will be empty most of the time.
        private readonly List<(Expression<Func<Scalar, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Scalar, TValue> node)> scalarNodes = new();
        private readonly List<(Expression<Func<Sequence, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Sequence, TValue> node)> sequenceNodes = new();
        private readonly List<(Expression<Func<Mapping, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Mapping, TValue> node)> mappingNodes = new();

        public override ISchemaNode EnterScalar(ref TagName tag, string value, ScalarStyle style)
        {
            // TODO:
            var node = scalarNodes
                .Where(s => s.nodePredicate.Compile()(new Scalar(_n, value)))
                .Select(s => s.node)
                .FirstOrDefault()
                ?? throw new Exception("TODO");

            return node;
        }

        public override ISchemaNode EnterSequence(ref TagName tag, SequenceStyle style)
        {
            // TODO:
            var node = sequenceNodes
                .Where(s => s.nodePredicate.Compile()(new Sequence(_n)))
                .Select(s => s.node)
                .FirstOrDefault()
                ?? throw new Exception("TODO");

            return node;
        }

        public override ISchemaNode EnterMapping(ref TagName tag, MappingStyle style)
        {
            // TODO:
            var node = mappingNodes
                .Where(s => s.nodePredicate.Compile()(new Mapping(_n)))
                .Select(s => s.node)
                .FirstOrDefault()
                ?? throw new Exception("TODO");

            return node;
        }

        public void Add(ISchemaNode<Scalar, TValue> node, Expression<Func<Scalar, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => scalarNodes.Add((nodePredicate, valuePredicate, node));
        public void Add(ISchemaNode<Sequence, TValue> node, Expression<Func<Sequence, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => sequenceNodes.Add((nodePredicate, valuePredicate, node));
        public void Add(ISchemaNode<Mapping, TValue> node, Expression<Func<Mapping, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => mappingNodes.Add((nodePredicate, valuePredicate, node));

        public override Expr<TValue> GenerateConstructor(Expr<Node> node, IVariableAllocator variableAllocator)
        {
            Expression constructor = Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)); // TODO: Throw a good and detailed exception.
            constructor = AddNodeTypeChoices(node, mappingNodes, constructor);
            constructor = AddNodeTypeChoices(node, sequenceNodes, constructor);
            constructor = AddNodeTypeChoices(node, scalarNodes, constructor);

            return constructor.As<TValue>();

            Expression AddNodeTypeChoices<TNode>(Expression node, List<(Expression<Func<TNode, bool>> nodePredicate, Expression<Func<TValue, bool>>, ISchemaNode<TNode, TValue> node)> schemaNodes, Expression otherwise) where TNode : Node
            {
                if (schemaNodes.Count == 0)
                {
                    return otherwise;
                }
                else
                {
                    var nodeAsTNode = Expr.Variable<TNode>();
                    var nodeTypeConstructor = GenerateNodeTypeConstructor(nodeAsTNode, schemaNodes);
                    return Expression.Block(
                        typeof(TValue),
                        new ParameterExpression[]
                        {
                            nodeAsTNode
                        },
                        Expression.Assign(nodeAsTNode, Expression.TypeAs(node, typeof(TNode))),
                        Expression.Condition(
                            Expression.ReferenceNotEqual(nodeAsTNode, Expression.Constant(null, typeof(object))),
                            nodeTypeConstructor,
                            otherwise
                        )
                    );
                }
            }

            Expression GenerateNodeTypeConstructor<TNode>(ParamExpr<TNode> nodeAsTNode, List<(Expression<Func<TNode, bool>> nodePredicate, Expression<Func<TValue, bool>>, ISchemaNode<TNode, TValue> node)> schemaNodes)
                where TNode : INode
            {
                return schemaNodes
                    .AsEnumerable()
                    .Reverse() // TODO: This is inefficient and can be improved.
                    .Aggregate(
                        (Expression)Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)), // TODO: Throw a good and detailed exception.
                        (otherwise, n) => OptimizedIf(
                            n.nodePredicate.Apply(nodeAsTNode),
                            n.node.GenerateConstructor(nodeAsTNode, variableAllocator).Expression.UpCast(typeof(TValue)),
                            otherwise
                        )
                    );
            }
        }

        public override Expr<Node> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
        {
            var representer = scalarNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value, variableAllocator).Expression.UpCast(typeof(Node))))
                .Concat(
                    sequenceNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value, variableAllocator).Expression.UpCast(typeof(Node))))
                )
                .Concat(
                    mappingNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value, variableAllocator).Expression.UpCast(typeof(Node))))
                )
                .Reverse() // TODO: This is inefficient and can be improved.
                .Aggregate(
                    (Expression)Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(Node)), // TODO: Throw a good and detailed exception.
                    (otherwise, n) => OptimizedIf(
                        n.predicate,
                        n.representer,
                        otherwise
                    )
                );

            return representer.As<Node>();
        }

        private static Expression OptimizedIf(Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (ExpressionIsTrue(test))
            {
                return ifTrue;
            }

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        private static bool ExpressionIsTrue(Expression test)
        {
            return test is ConstantExpression constant && constant.Value.Equals(true);
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();

        public override void RenderGraph(SchemaNodeRenderer renderer, string id)
        {
            renderer
                .WriteLine($"{id} [shape=diamond, label=\"choice\"];");

            foreach (var node in scalarNodes)
            {
                renderer.WriteLine($"{id} -> {renderer.GetNodeId(node.node)} [label=\"{node.nodePredicate.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")}\"];");
            }
            foreach (var node in sequenceNodes)
            {
                renderer.WriteLine($"{id} -> {renderer.GetNodeId(node.node)} [label=\"{node.nodePredicate.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")}\"];");
            }
            foreach (var node in mappingNodes)
            {
                renderer.WriteLine($"{id} -> {renderer.GetNodeId(node.node)} [label=\"{node.nodePredicate.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")}\"];");
            }
        }
    }
}

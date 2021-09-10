using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;

namespace YamlDotNet.Sandbox
{
    public static class SchemaNodeExtensions
    {
        private sealed class VariableAllocator : IVariableAllocator
        {
            private readonly Dictionary<object, ParameterExpression> variables = new();
            private readonly List<Expression> initializers = new();

            public ParamExpr<TValue> Allocate<TValue>(object key, Func<Expression<TValue>> initializerFactory)
            {
                if (!variables.TryGetValue(key, out var variable))
                {
                    variable = Expression.Variable(typeof(TValue));
                    variables.Add(key, variable);

                    var initializer = initializerFactory();
                    initializers.Add(Expression.Assign(variable, initializer));
                }
                return new ParamExpr<TValue>(variable);
            }

            public IEnumerable<ParameterExpression> Variables => variables.Values;
            public IEnumerable<Expression> Initializers => initializers;
        }

        public static Expression<Func<TNode, TValue>> GenerateConstructor<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var variableAllocator = new VariableAllocator();
            var nodeParam = Expr.Parameter<TNode>("node");

            var constructor = schemaNode.GenerateConstructor(nodeParam, variableAllocator);

            var decoratedConstructor = Expression.Lambda<Func<TNode, TValue>>(
                Expression.Block(
                    variableAllocator.Variables,
                    variableAllocator.Initializers.Concat(new[] { constructor.Expression })
                ),
                nodeParam
            );

            return decoratedConstructor;
        }

        public static Expression<Func<TValue, TNode>> GenerateRepresenter<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var variableAllocator = new VariableAllocator();
            var valueParam = Expr.Parameter<TValue>("value");

            var representer = schemaNode.GenerateRepresenter(valueParam, variableAllocator);

            var decoratedRepresenter = Expression.Lambda<Func<TValue, TNode>>(
                Expression.Block(
                    variableAllocator.Variables,
                    variableAllocator.Initializers.Concat(new[] { representer.Expression })
                ),
                valueParam
            );

            return decoratedRepresenter;
        }
    }
}

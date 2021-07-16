using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Sandbox
{
    class Program
    {
        private static object ConstructDictionary(Mapping mapping)
        {
            return new Dictionary<object, object>();
        }

        private static object ConstructSequence(Sequence sequence, IEnumerable<object> items)
        {
            return new List<object>(items);
        }

        private static object ConstructScalar(Scalar scalar)
        {
            return scalar.Value;
        }

        public static void Main()
        {
            var _n = new UnresolvedValueMapper(null);

            var document1 = new Document(
                new Sequence(_n,
                    new Scalar(_n, "hello"),
                    new Scalar(_n, "world")
                ),
                NullSchema.Instance
            );

            var document2 = new Document(
                new Sequence(_n,
                    new Scalar(_n, "hello"),
                    new Scalar(_n, "world"),
                    new Scalar(_n, "42")
                ),
                NullSchema.Instance
            );

            //var document1 = new Document(
            //    new Mapping(_n,
            //        (
            //            new Scalar(_n, "hello"),
            //            new Scalar(_n, "world")
            //        )
            //    ),
            //    NullSchema.Instance
            //);

            //var document2 = new Document(
            //    new Mapping(_n,
            //        (
            //            new Scalar(_n, "hello"),
            //            new Sequence(_n,
            //                new Scalar(_n, "cruel"),
            //                new Scalar(_n, "world")
            //            )
            //        )
            //    ),
            //    NullSchema.Instance
            //);

            Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, document1, document2);

            var schema = new SequenceSchemaNode<List<object>, object>(
                (i, s) => new List<object>(i),
                l => l,
                (_, n) => new Sequence(_n, n.ToList()),
                new ChoiceSchemaNode<object>
                {
                    {
                        new ScalarSchemaNode<object>(
                            s => int.Parse(s.Value),
                            val => new Scalar(_n, val!.ToString()!)
                        ),
                        s => Regex.IsMatch(s.Value, @"\d+")
                    },
                    {
                        new ScalarSchemaNode<object>(
                            s => s.Value,
                            val => new Scalar(_n, val!.ToString()!)
                        ),
                        s => true
                    }
                }
            );

            var constructor = schema.GenerateConstructor().Compile();
            foreach (var document in new[] { document1, document2 })
            {
                Console.WriteLine("----");
                Console.WriteLine(constructor);

                var value = constructor((Sequence)document.Content);
                Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));
            }
        }
    }

    public interface ISchemaNode<TNode, TValue>
        where TNode : INode
    {
        // Expression<Func<TNode, TValue>>
        Expression GenerateConstructor(Expression node);

        // Expression<Func<TValue, TNode>>
        Expression GenerateRepresenter(Expression value);
    }

    public static class SchemaNodeExtensions
    {
        public static Expression<Func<TNode, TValue>> GenerateConstructor<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var nodeParam = Expression.Parameter(typeof(TNode), "node");
            return Expression.Lambda<Func<TNode, TValue>>(
                schemaNode.GenerateConstructor(nodeParam),
                nodeParam
            );
        }

        public static Expression<Func<TValue, TNode>> GenerateRepresenter<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var valueParam = Expression.Parameter(typeof(TValue), "value");
            return Expression.Lambda<Func<TValue, TNode>>(
                schemaNode.GenerateRepresenter(valueParam),
                valueParam
            );
        }
    }

    public class ChoiceSchemaNode<TValue> : ISchemaNode<Node, TValue>, IEnumerable
    {
        // TODO: Lazy initialization of the lists since they will be empty most of the time.
        private readonly List<(Expression<Func<Scalar, bool>> predicate, ISchemaNode<Scalar, TValue> node)> scalarNodes = new();
        private readonly List<(Expression<Func<Sequence, bool>> predicate, ISchemaNode<Sequence, TValue> node)> sequenceNodes = new();
        private readonly List<(Expression<Func<Mapping, bool>> predicate, ISchemaNode<Mapping, TValue> node)> mappingNodes = new();

        public void Add(ISchemaNode<Scalar, TValue> node, Expression<Func<Scalar, bool>> predicate) => scalarNodes.Add((predicate, node));
        public void Add(ISchemaNode<Sequence, TValue> node, Expression<Func<Sequence, bool>> predicate) => sequenceNodes.Add((predicate, node));
        public void Add(ISchemaNode<Mapping, TValue> node, Expression<Func<Mapping, bool>> predicate) => mappingNodes.Add((predicate, node));

        public Expression GenerateConstructor(Expression node)
        {
            Expression constructor = Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)); // TODO: Throw a good and detailed exception.
            constructor = AddNodeTypeChoices(node, mappingNodes, constructor);
            constructor = AddNodeTypeChoices(node, sequenceNodes, constructor);
            constructor = AddNodeTypeChoices(node, scalarNodes, constructor);

            return constructor;

            static Expression AddNodeTypeChoices<TNode>(Expression node, List<(Expression<Func<TNode, bool>> predicate, ISchemaNode<TNode, TValue> node)> schemaNodes, Expression otherwise) where TNode : Node
            {
                if (schemaNodes.Count == 0)
                {
                    return otherwise;
                }
                else
                {
                    var nodeAsTNode = Expression.Variable(typeof(TNode));
                    var nodeTypeConstructor = GenerateNodeTypeConstructor(nodeAsTNode, schemaNodes);
                    return Expression.Block(
                        typeof(TValue),
                        new[]
                        {
                                nodeAsTNode
                        },
                        Expression.Assign(nodeAsTNode, Expression.TypeAs(node, typeof(TNode))),
                        Expression.Condition(
                            Expression.ReferenceNotEqual(nodeAsTNode, Expression.Default(typeof(TNode))),
                            nodeTypeConstructor,
                            otherwise
                        )
                    );
                }
            }

            static Expression GenerateNodeTypeConstructor<TNode>(ParameterExpression nodeAsTNode, List<(Expression<Func<TNode, bool>> predicate, ISchemaNode<TNode, TValue> node)> schemaNodes)
                where TNode : INode
            {
                return schemaNodes
                    .AsEnumerable()
                    .Reverse() // TODO: This is inefficient and can be improved.
                    .Aggregate(
                        (Expression)Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)), // TODO: Throw a good and detailed exception.
                        (otherwise, n) => Expression.Condition(
                            n.predicate.Apply(nodeAsTNode),
                            Expression.Convert(
                                n.node.GenerateConstructor(nodeAsTNode),
                                typeof(TValue)
                            ),
                            otherwise
                        )
                    );
            }
        }

        public Expression GenerateRepresenter(Expression value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();
    }

    public class ScalarSchemaNode<TValue> : ISchemaNode<Scalar, TValue>
    {
        private readonly Expression<Func<Scalar, TValue>> constructor;
        private readonly Expression<Func<TValue, Scalar>> representer;

        public ScalarSchemaNode(// TODO: Func<IScalar, bool> predicate,
            Expression<Func<Scalar, TValue>> constructor,
            Expression<Func<TValue, Scalar>> representer)
        {
            this.constructor = constructor;
            this.representer = representer;
        }

        public Expression GenerateConstructor(Expression node) => constructor.Apply(node);
        public Expression GenerateRepresenter(Expression value) => representer.Apply(value);
    }

    public class SequenceSchemaNode<TSequence, TItem> : ISchemaNode<Sequence, TSequence>
        where TSequence : IEnumerable<TItem>
    {
        private readonly Expression<Func<IEnumerable<TItem>, Sequence, TSequence>> constructor;
        private readonly Expression<Func<TSequence, IEnumerable<TItem>>> deconstructor;
        private readonly Expression<Func<TSequence, IEnumerable<Node>, Sequence>> representer;
        private readonly ISchemaNode<Node, TItem> itemSchema;

        public SequenceSchemaNode(// TODO: Func<IScalar, bool> predicate,
            Expression<Func<IEnumerable<TItem>, Sequence, TSequence>> constructor,
            Expression<Func<TSequence, IEnumerable<TItem>>> deconstructor,
            Expression<Func<TSequence, IEnumerable<Node>, Sequence>> representer,
            ISchemaNode<Node, TItem> itemSchema
        )
        {
            this.constructor = constructor;
            this.deconstructor = deconstructor;
            this.representer = representer;
            this.itemSchema = itemSchema;
        }

        public Expression GenerateConstructor(Expression node)
        {
            var itemConstructor = itemSchema.GenerateConstructor();

            Expression<Func<Func<Node, TItem>, Sequence, IEnumerable<TItem>>> itemsConstructorTemplate = (c, s) => s.Select(c);

            var itemsConstructorBody = itemsConstructorTemplate.Apply(itemConstructor, node);
            return constructor.Apply(itemsConstructorBody, node);
        }

        public Expression GenerateRepresenter(Expression value)
        {
            var itemRepresenter = itemSchema.GenerateRepresenter();

            Expression<Func<Func<TItem, Node>, IEnumerable<TItem>, IEnumerable<Node>>> itemsRepresenterTemplate = (c, s) => s.Select(c);

            var items = deconstructor.Apply(value);

            var itemsRepresenterBody = itemsRepresenterTemplate.Apply(itemRepresenter, items);
            return representer.Apply(value, items);
        }
    }
}

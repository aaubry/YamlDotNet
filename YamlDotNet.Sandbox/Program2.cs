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
using static YamlDotNet.Sandbox.Program;
using E = YamlDotNet.Helpers.ExpressionBuilder;

namespace YamlDotNet.Sandbox
{
    public class Model
    {
        public Model(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public int Age { get; set; }
    }

    class Program
    {
        public static readonly UnresolvedValueMapper _n = new UnresolvedValueMapper(null);

        public static void Main()
        {
            Main2();
        }

        //public static ISchemaNode<Scalar, TValue> BuildSchema<TValue>()
        //{

        //}

        public static void Main2()
        {
            var sequenceSchema = new SequenceSchemaNode<List<object>, object>(
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
                        s => Regex.IsMatch(s.Value, @"\d+"),
                        v => v is int
                    },
                    {
                        new ScalarSchemaNode<object>(
                            s => s.Value,
                            val => new Scalar(_n, val!.ToString()!)
                        ),
                        s => true,
                        v => true
                    }
                }
            );

            Roundtrip(
                new Sequence(_n,
                    new Scalar(_n, "hello"),
                    new Scalar(_n, "world")
                ),
                sequenceSchema
            );

            Roundtrip(
                new Sequence(_n,
                    new Scalar(_n, "hello"),
                    new Scalar(_n, "world"),
                    new Scalar(_n, "42")
                ),
                sequenceSchema
            );

            var mappingSchema = new MappingSchemaNode<Dictionary<string, object>, string, object>(
                (i, m) => new Dictionary<string, object>(i),
                d => d,
                (_, n) => new Mapping(_n, new Dictionary<Node, Node>(n)),
                new ChoiceSchemaNode<string>
                {
                    {
                        new ScalarSchemaNode<string>(
                            s => s.Value,
                            val => new Scalar(_n, val!.ToString()!)
                        ),
                        s => true,
                        v => true
                    }
                },
                new ChoiceSchemaNode<object>
                {
                    {
                        new ScalarSchemaNode<object>(
                            s => s.Value,
                            val => new Scalar(_n, val!.ToString()!)
                        ),
                        s => true,
                        v => true
                    }
                }
            );

            Roundtrip(
                new Mapping(_n,
                    (
                        new Scalar(_n, "hello"),
                        new Scalar(_n, "world")
                    ),
                    (
                        new Scalar(_n, "goodbye"),
                        new Scalar(_n, "moon")
                    )
                ),
                mappingSchema
            );

            Roundtrip(
                new Mapping(_n,
                    (
                        new Scalar(_n, "name"),
                        new Scalar(_n, "John Smith")
                    ),
                    (
                        new Scalar(_n, "age"),
                        new Scalar(_n, "34")
                    )
                ),
                new ModelSchemaNode()
            );

            static void Roundtrip<TNode, TItem>(TNode node, ISchemaNode<TNode, TItem> schema) where TNode : Node
            {
                Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, new Document(node, NullSchema.Instance));

                var constructor = schema.GenerateConstructor();
                Console.WriteLine("----");
                Console.WriteLine(constructor);

                Console.WriteLine("----");

                var value = constructor.Compile()(node);
                Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));

                var representer = schema.GenerateRepresenter();
                Console.WriteLine("----");
                Console.WriteLine(representer);

                Console.WriteLine("----");

                var yaml = representer.Compile()(value);
                Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, new Document(yaml, NullSchema.Instance));
                Console.WriteLine("....");
            }
        }
    }

    public interface ISchemaNode<TNode, TValue>
        where TNode : INode
    {
        // Expression<Func<TNode, TValue>>
        Expr<TValue> GenerateConstructor(Expr<TNode> node);

        // Expression<Func<TValue, TNode>>
        Expr<TNode> GenerateRepresenter(Expr<TValue> value);
    }

    public static class SchemaNodeExtensions
    {
        public static Expression<Func<TNode, TValue>> GenerateConstructor<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var nodeParam = Expr.Parameter<TNode>("node");
            return Expression.Lambda<Func<TNode, TValue>>(
                schemaNode.GenerateConstructor(nodeParam),
                nodeParam
            );
        }

        public static Expression<Func<TValue, TNode>> GenerateRepresenter<TNode, TValue>(this ISchemaNode<TNode, TValue> schemaNode)
            where TNode : INode
        {
            var valueParam = Expr.Parameter<TValue>("value");
            return Expression.Lambda<Func<TValue, TNode>>(
                schemaNode.GenerateRepresenter(valueParam),
                valueParam
            );
        }
    }

    public class ChoiceSchemaNode<TValue> : ISchemaNode<Node, TValue>, IEnumerable
    {
        // TODO: Lazy initialization of the lists since they will be empty most of the time.
        private readonly List<(Expression<Func<Scalar, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Scalar, TValue> node)> scalarNodes = new();
        private readonly List<(Expression<Func<Sequence, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Sequence, TValue> node)> sequenceNodes = new();
        private readonly List<(Expression<Func<Mapping, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate, ISchemaNode<Mapping, TValue> node)> mappingNodes = new();

        public void Add(ISchemaNode<Scalar, TValue> node, Expression<Func<Scalar, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => scalarNodes.Add((nodePredicate, valuePredicate, node));
        public void Add(ISchemaNode<Sequence, TValue> node, Expression<Func<Sequence, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => sequenceNodes.Add((nodePredicate, valuePredicate, node));
        public void Add(ISchemaNode<Mapping, TValue> node, Expression<Func<Mapping, bool>> nodePredicate, Expression<Func<TValue, bool>> valuePredicate) => mappingNodes.Add((nodePredicate, valuePredicate, node));

        public Expr<TValue> GenerateConstructor(Expr<Node> node)
        {
            Expression constructor = Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)); // TODO: Throw a good and detailed exception.
            constructor = AddNodeTypeChoices(node, mappingNodes, constructor);
            constructor = AddNodeTypeChoices(node, sequenceNodes, constructor);
            constructor = AddNodeTypeChoices(node, scalarNodes, constructor);

            return constructor.As<TValue>();

            static Expression AddNodeTypeChoices<TNode>(Expression node, List<(Expression<Func<TNode, bool>> nodePredicate, Expression<Func<TValue, bool>>, ISchemaNode<TNode, TValue> node)> schemaNodes, Expression otherwise) where TNode : Node
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
                            Expression.ReferenceNotEqual(nodeAsTNode, Expression.Default(typeof(TNode))),
                            nodeTypeConstructor,
                            otherwise
                        )
                    );
                }
            }

            static Expression GenerateNodeTypeConstructor<TNode>(ParamExpr<TNode> nodeAsTNode, List<(Expression<Func<TNode, bool>> nodePredicate, Expression<Func<TValue, bool>>, ISchemaNode<TNode, TValue> node)> schemaNodes)
                where TNode : INode
            {
                return schemaNodes
                    .AsEnumerable()
                    .Reverse() // TODO: This is inefficient and can be improved.
                    .Aggregate(
                        (Expression)Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(TValue)), // TODO: Throw a good and detailed exception.
                        (otherwise, n) => OptimizedIf(
                            n.nodePredicate.Apply(nodeAsTNode),
                            n.node.GenerateConstructor(nodeAsTNode).Expression.UpCast(typeof(TValue)),
                            otherwise
                        )
                    );
            }
        }

        public Expr<Node> GenerateRepresenter(Expr<TValue> value)
        {
            var representer = scalarNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value).Expression.UpCast(typeof(Node))))
                .Concat(
                    sequenceNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value).Expression.UpCast(typeof(Node))))
                )
                .Concat(
                    mappingNodes.Select(n => (predicate: n.valuePredicate.Apply(value), representer: n.node.GenerateRepresenter(value).Expression.UpCast(typeof(Node))))
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
            if (test is ConstantExpression constant && constant.Value.Equals(true))
            {
                return ifTrue;
            }

            return Expression.Condition(test, ifTrue, ifFalse);
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

        public Expr<TValue> GenerateConstructor(Expr<Scalar> node) => constructor.Apply(node);
        public Expr<Scalar> GenerateRepresenter(Expr<TValue> value) => representer.Apply(value);
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

        public Expr<TSequence> GenerateConstructor(Expr<Sequence> node)
        {
            Expression<Func<IEnumerable<TItem>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        s => E.Inject(itemSchema.GenerateConstructor(E.Wrap(s)))
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public Expr<Sequence> GenerateRepresenter(Expr<TSequence> value)
        {
            var items = deconstructor.Apply(value);

            Expression<Func<IEnumerable<Node>>> itemsRepresenterTemplate =
                () => E.Inject(items)
                    .Select(
                        v => E.Inject(itemSchema.GenerateRepresenter(E.Wrap(v)))
                    );

            var itemsRepresenterBody = itemsRepresenterTemplate.Inject();
            return representer.Apply(value, itemsRepresenterBody);
        }
    }

    public class MappingSchemaNode<TMapping, TKey, TValue> : ISchemaNode<Mapping, TMapping>
        where TMapping : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>, Mapping, TMapping>> constructor;
        private readonly Expression<Func<TMapping, IEnumerable<KeyValuePair<TKey, TValue>>>> deconstructor;
        private readonly Expression<Func<TMapping, IEnumerable<KeyValuePair<Node, Node>>, Mapping>> representer;
        private readonly ISchemaNode<Node, TKey> keySchema;
        private readonly ISchemaNode<Node, TValue> valueSchema;

        public MappingSchemaNode(
            Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>, Mapping, TMapping>> constructor,
            Expression<Func<TMapping, IEnumerable<KeyValuePair<TKey, TValue>>>> deconstructor,
            Expression<Func<TMapping, IEnumerable<KeyValuePair<Node, Node>>, Mapping>> representer,
            ISchemaNode<Node, TKey> keySchema,
            ISchemaNode<Node, TValue> valueSchema
        )
        {
            this.constructor = constructor;
            this.deconstructor = deconstructor;
            this.representer = representer;
            this.keySchema = keySchema;
            this.valueSchema = valueSchema;
        }

        public Expr<TMapping> GenerateConstructor(Expr<Mapping> node)
        {
            Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        p => new KeyValuePair<TKey, TValue>(
                            E.Inject(keySchema.GenerateConstructor(E.Wrap(p.Key))),
                            E.Inject(valueSchema.GenerateConstructor(E.Wrap(p.Value)))
                        )
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public Expr<Mapping> GenerateRepresenter(Expr<TMapping> value)
        {
            var pairs = deconstructor.Apply(value);

            Expression<Func<IEnumerable<KeyValuePair<Node, Node>>>> pairsRepresenterTemplate =
                () => E.Inject(pairs)
                    .Select(
                        p => new KeyValuePair<Node, Node>(
                            E.Inject(keySchema.GenerateRepresenter(E.Wrap(p.Key))),
                            E.Inject(valueSchema.GenerateRepresenter(E.Wrap(p.Value)))
                        )
                    );

            var pairsRepresenterBody = pairsRepresenterTemplate.Inject();
            return representer.Apply(value, pairsRepresenterBody);
        }
    }

    public class ModelSchemaNode : ISchemaNode<Mapping, Model>
    {
        private readonly ScalarSchemaNode<string> nameSchema = new ScalarSchemaNode<string>(s => s.Value, v => new Scalar(_n, v));
        private readonly ScalarSchemaNode<int> ageSchema = new ScalarSchemaNode<int>(s => int.Parse(s.Value), v => new Scalar(_n, v.ToString()));

        public Expr<Model> GenerateConstructor(Expr<Mapping> node)
        {
            Expression<Func<Model>> constructor =
                () => new Model(E.Inject(nameSchema.GenerateConstructor(E.Wrap((Scalar)E.Inject(node)["name"]))))
                {
                    Age = E.Inject(ageSchema.GenerateConstructor(E.Wrap((Scalar)E.Inject(node)["age"])))
                };

            return constructor.Inject();
        }

        public Expr<Mapping> GenerateRepresenter(Expr<Model> value)
        {
            Expression<Func<Mapping>> representer =
                () => new Mapping(_n, new Dictionary<Node, Node>
                {
                    {
                        new Scalar(_n, "name"),
                        E.Inject(nameSchema.GenerateRepresenter(E.Wrap(E.Inject(value).Name)))
                    },
                    {
                        new Scalar(_n, "age"),
                        E.Inject(ageSchema.GenerateRepresenter(E.Wrap(E.Inject(value).Age)))
                    },
                });

            return representer.Inject();
        }
    }
}

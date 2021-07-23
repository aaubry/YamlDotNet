using FastExpressionCompiler;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using static YamlDotNet.Sandbox.Program;
using E = YamlDotNet.Helpers.ExpressionBuilder;

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

    public interface ISchemaBuilder
    {
        ISchemaNode<Node, TValue> BuildSchema<TValue>();
    }

    public static class SchemaBuilderExtensions
    {
        public static ISchemaNode BuildSchema(this ISchemaBuilder schemaBuilder, Type valueType)
        {
            return (ISchemaNode)buildSchemaHelperMethod
                .MakeGenericMethod(valueType)
                .Invoke(null, new[] { schemaBuilder })!;
        }

        private static readonly MethodInfo buildSchemaHelperMethod = typeof(SchemaBuilderExtensions).GetMethod(nameof(BuildSchemaHelper), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method '{nameof(BuildSchemaHelper)}' not found in class '{nameof(SchemaBuilderExtensions)}'.");

        private static ISchemaNode BuildSchemaHelper<TValue>(ISchemaBuilder schemaBuilder)
        {
            var genericSchemaNode = schemaBuilder.BuildSchema<TValue>();
            return new SchemaBuilderAdapter<TValue>(genericSchemaNode);
        }

        private sealed class SchemaBuilderAdapter<TValue> : ISchemaNode
        {
            private readonly ISchemaNode<Node, TValue> schemaNode;

            public SchemaBuilderAdapter(ISchemaNode<Node, TValue> schemaNode)
            {
                this.schemaNode = schemaNode;
            }

            public Expression GenerateConstructor(Expression node, IVariableAllocator variableAllocator)
            {
                return schemaNode.GenerateConstructor(node.As<Node>(), variableAllocator);
            }

            public Expression GenerateRepresenter(Expression value, IVariableAllocator variableAllocator)
            {
                return schemaNode.GenerateRepresenter(value.As<TValue>(), variableAllocator);
            }
        }
    }

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

                return choice;

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

    public class ModelWithConstructor
    {
        public ModelWithConstructor(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public int Age { get; set; }
    }

    public class ModelWithProperties
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class RecursiveModel
    {
        public List<RecursiveModel>? Children { get; set; }
    }

    class Program
    {
        public static readonly UnresolvedValueMapper _n = new UnresolvedValueMapper(null);

        public static void Main()
        {
            //Main2();

            var schemaBuilder = new SchemaBuilder
            {
                {
                    _ => new ScalarSchemaNode<int>(
                        s => int.Parse(s.Value),
                        val => new Scalar(_n, val!.ToString()!)
                    ),
                    s => Regex.IsMatch(s.Value, @"\d+"),
                    v => true
                },
                {
                    _ => new ScalarSchemaNode<string>(
                        s => s.Value,
                        val => new Scalar(_n, val!.ToString()!)
                    ),
                    s => true,
                    v => true
                },
                {
                    b => new SequenceSchemaNode<List<int>, int>(
                        (i, _) => new List<int>(i),
                        l => l,
                        (_, n) => new Sequence(_n, n.ToList()),
                        b.BuildSchema<int>()
                    ),
                    s => true,
                    v => true
                },
                {
                    b => new SequenceSchemaNode<List<RecursiveModel>, RecursiveModel>(
                        (i, _) => new List<RecursiveModel>(i),
                        l => l,
                        (_, n) => new Sequence(_n, n.ToList()),
                        b.BuildSchema<RecursiveModel>()
                    ),
                    s => true,
                    v => true
                },
                //{
                //    b => new RecursiveModelSchemaNode(b),
                //    m => true,
                //    v => true
                //}
                {
                    b => new ObjectSchemaNode<ModelWithProperties>(b),
                    m => true,
                    v => true
                },
                {
                    b => new ObjectSchemaNode<RecursiveModel>(b),
                    m => true,
                    v => true
                },
            };

            //Roundtrip<int>(new Scalar(_n, "42"));
            //Roundtrip<string>(new Scalar(_n, "hello"));
            //Roundtrip<List<int>>(new Sequence(_n, new Scalar(_n, "42"), new Scalar(_n, "23"), new Scalar(_n, "11")));
            //Roundtrip<RecursiveModel>(
            //    new Mapping(_n, new Dictionary<Node, Node>
            //    {
            //        {
            //            new Scalar(_n, "Children"),
            //            new Sequence(_n,
            //                new Mapping(_n, new Dictionary<Node, Node>
            //                {
            //                    {
            //                        new Scalar(_n, "Children"),
            //                        new Sequence(_n)
            //                    }
            //                }),
            //                new Mapping(_n, new Dictionary<Node, Node>
            //                {
            //                    {
            //                        new Scalar(_n, "Children"),
            //                        new Sequence(_n)
            //                    }
            //                })
            //            )
            //        }
            //    })
            //);

            Roundtrip<ModelWithProperties>(
                new Mapping(_n,
                    (
                        new Scalar(_n, "Name"),
                        new Scalar(_n, "John Smith")
                    ),
                    (
                        new Scalar(_n, "Age"),
                        new Scalar(_n, "34")
                    )
                )
            );

            void Roundtrip<TItem>(Node node)
            {
                var schema = schemaBuilder!.BuildSchema<TItem>();

                Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, new Document(node, NullSchema.Instance));

                var constructor = schema.GenerateConstructor();
                Console.WriteLine("----");
                Console.WriteLine(constructor);

                Console.WriteLine("----");

                var func = constructor.Compile();
                var value = func(node);
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

    public interface IVariableAllocator
    {
        public ParamExpr<TValue> Allocate<TValue>(object key, Func<Expression<TValue>> initializerFactory);
    }

    public interface ISchemaNode<TNode, TValue>
        where TNode : INode
    {
        Expr<TValue> GenerateConstructor(Expr<TNode> node, IVariableAllocator variableAllocator);
        Expr<TNode> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator);
    }

    public interface ISchemaNode
    {
        Expression GenerateConstructor(Expression node, IVariableAllocator variableAllocator);
        Expression GenerateRepresenter(Expression value, IVariableAllocator variableAllocator);
    }

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

    public class RecursiveSchemaNode<TValue> : ISchemaNode<Node, TValue>
    {
        private readonly ISchemaNode<Node, TValue> baseSchemaNode;

        public RecursiveSchemaNode(ISchemaNode<Node, TValue> baseSchemaNode)
        {
            this.baseSchemaNode = baseSchemaNode;
        }

        public Expr<TValue> GenerateConstructor(Expr<Node> node, IVariableAllocator variableAllocator)
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

        public Expr<Node> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
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

        public Expr<TValue> GenerateConstructor(Expr<Node> node, IVariableAllocator variableAllocator)
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
                            Expression.ReferenceNotEqual(nodeAsTNode, Expression.Default(typeof(TNode))),
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

        public Expr<Node> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
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

        public Expr<TValue> GenerateConstructor(Expr<Scalar> node, IVariableAllocator variableAllocator) => constructor.Apply(node);
        public Expr<Scalar> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator) => representer.Apply(value);
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

        public Expr<TSequence> GenerateConstructor(Expr<Sequence> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<IEnumerable<TItem>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        s => E.Inject(itemSchema.GenerateConstructor(E.Wrap(s), variableAllocator))
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public Expr<Sequence> GenerateRepresenter(Expr<TSequence> value, IVariableAllocator variableAllocator)
        {
            var items = deconstructor.Apply(value);

            Expression<Func<IEnumerable<Node>>> itemsRepresenterTemplate =
                () => E.Inject(items)
                    .Select(
                        v => E.Inject(itemSchema.GenerateRepresenter(E.Wrap(v), variableAllocator))
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

        public Expr<TMapping> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<IEnumerable<KeyValuePair<TKey, TValue>>>> itemsConstructorTemplate =
                () => E.Inject(node)
                    .Select(
                        p => new KeyValuePair<TKey, TValue>(
                            E.Inject(keySchema.GenerateConstructor(E.Wrap(p.Key), variableAllocator)),
                            E.Inject(valueSchema.GenerateConstructor(E.Wrap(p.Value), variableAllocator))
                        )
                    );

            var itemsConstructorBody = itemsConstructorTemplate.Inject();
            return constructor.Apply(itemsConstructorBody, node);
        }

        public Expr<Mapping> GenerateRepresenter(Expr<TMapping> value, IVariableAllocator variableAllocator)
        {
            var pairs = deconstructor.Apply(value);

            Expression<Func<IEnumerable<KeyValuePair<Node, Node>>>> pairsRepresenterTemplate =
                () => E.Inject(pairs)
                    .Select(
                        p => new KeyValuePair<Node, Node>(
                            E.Inject(keySchema.GenerateRepresenter(E.Wrap(p.Key), variableAllocator)),
                            E.Inject(valueSchema.GenerateRepresenter(E.Wrap(p.Value), variableAllocator))
                        )
                    );

            var pairsRepresenterBody = pairsRepresenterTemplate.Inject();
            return representer.Apply(value, pairsRepresenterBody);
        }
    }

    public class ModelSchemaNode : ISchemaNode<Mapping, ModelWithConstructor>
    {
        private readonly ScalarSchemaNode<string> nameSchema = new ScalarSchemaNode<string>(s => s.Value, v => new Scalar(_n, v));
        private readonly ScalarSchemaNode<int> ageSchema = new ScalarSchemaNode<int>(s => int.Parse(s.Value), v => new Scalar(_n, v.ToString()));

        public Expr<ModelWithConstructor> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<ModelWithConstructor>> constructor =
                () => new ModelWithConstructor(E.Inject(nameSchema.GenerateConstructor(E.Wrap((Scalar)E.Inject(node)["name"]), variableAllocator)))
                {
                    Age = E.Inject(ageSchema.GenerateConstructor(E.Wrap((Scalar)E.Inject(node)["age"]), variableAllocator))
                };

            return constructor.Inject();
        }

        public Expr<Mapping> GenerateRepresenter(Expr<ModelWithConstructor> value, IVariableAllocator variableAllocator)
        {
            Expression<Func<Mapping>> representer =
                () => new Mapping(_n, new Dictionary<Node, Node>
                {
                    {
                        new Scalar(_n, "name"),
                        E.Inject(nameSchema.GenerateRepresenter(E.Wrap(E.Inject(value).Name), variableAllocator))
                    },
                    {
                        new Scalar(_n, "age"),
                        E.Inject(ageSchema.GenerateRepresenter(E.Wrap(E.Inject(value).Age), variableAllocator))
                    },
                });

            return representer.Inject();
        }
    }

    public class RecursiveModelSchemaNode : ISchemaNode<Mapping, RecursiveModel>
    {
        private readonly ISchemaNode<Node, List<RecursiveModel>?> childrenSchema;

        public RecursiveModelSchemaNode(ISchemaBuilder schemaBuidler)
        {
            this.childrenSchema = schemaBuidler.BuildSchema<List<RecursiveModel>?>();
        }

        public Expr<RecursiveModel> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            Expression<Func<RecursiveModel>> constructor =
                () => new RecursiveModel
                {
                    Children = E.Inject(childrenSchema.GenerateConstructor(E.Wrap(E.Inject(node)[nameof(RecursiveModel.Children)]), variableAllocator))
                };

            return constructor.Inject();
        }

        public Expr<Mapping> GenerateRepresenter(Expr<RecursiveModel> value, IVariableAllocator variableAllocator)
        {
            Expression<Func<Mapping>> representer =
                () => new Mapping(_n, new Dictionary<Node, Node>
                {
                    {
                        new Scalar(_n, nameof(RecursiveModel.Children)),
                        E.Inject(childrenSchema.GenerateRepresenter(E.Wrap(E.Inject(value).Children), variableAllocator))
                    },
                });

            return representer.Inject();
        }
    }

    public class ObjectSchemaNode<TValue> : ISchemaNode<Mapping, TValue>
    {
        private readonly ISchemaNode<Node, string> stringSchemaNode;
        private readonly Dictionary<string, (PropertyInfo property, ISchemaNode schemaNode)> properties;

        public ObjectSchemaNode(ISchemaBuilder schemaBuidler)
        {
            this.stringSchemaNode = schemaBuidler.BuildSchema<string>();

            this.properties = typeof(TValue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(
                    p => p.Name,
                    p => (p, schemaBuidler.BuildSchema(p.PropertyType))
                );
        }

        private static readonly PropertyInfo mappingIndexer = typeof(Mapping)
            .GetProperty("Item", typeof(Node), new[] { typeof(string) })
            ?? throw new InvalidOperationException($"Indexer not found in class '{nameof(Mapping)}'.");

        private static readonly MethodInfo mappingGetEnumerator = typeof(IEnumerable<KeyValuePair<Node, Node>>).GetMethod(nameof(IEnumerable<KeyValuePair<Node, Node>>.GetEnumerator))
            ?? throw new InvalidOperationException($"Method '{nameof(IEnumerable<KeyValuePair<Node, Node>>.GetEnumerator)}' not found in class '{nameof(IEnumerable<KeyValuePair<Node, Node>>)}'.");

        private static readonly MethodInfo enumeratorMoveNext = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))
            ?? throw new InvalidOperationException($"Method '{nameof(IEnumerator.MoveNext)}' not found in class '{nameof(IEnumerator)}'.");

        private static readonly PropertyInfo mappingEnumeratorCurrent = typeof(IEnumerator<KeyValuePair<Node, Node>>).GetProperty(nameof(IEnumerator<KeyValuePair<Node, Node>>.Current))
            ?? throw new InvalidOperationException($"Property '{nameof(IEnumerator<KeyValuePair<Node, Node>>.Current)}' not found in class '{nameof(IEnumerator<KeyValuePair<Node, Node>>)}'.");

        private static readonly PropertyInfo keyValuePairKey = typeof(KeyValuePair<Node, Node>).GetProperty(nameof(KeyValuePair<Node, Node>.Key))
            ?? throw new InvalidOperationException($"Property '{nameof(KeyValuePair<Node, Node>.Key)}' not found in class '{nameof(KeyValuePair<Node, Node>)}'.");

        private static readonly PropertyInfo keyValuePairValue = typeof(KeyValuePair<Node, Node>).GetProperty(nameof(KeyValuePair<Node, Node>.Value))
            ?? throw new InvalidOperationException($"Property '{nameof(KeyValuePair<Node, Node>.Value)}' not found in class '{nameof(KeyValuePair<Node, Node>)}'.");

        private static readonly MethodInfo stringComparison = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(string) })
            ?? throw new InvalidOperationException($"Method '{nameof(string.Equals)}(string, string)' not found in class '{nameof(String)}'.");

        public Expr<TValue> GenerateConstructor(Expr<Mapping> node, IVariableAllocator variableAllocator)
        {
            var enumerator = Expression.Variable(typeof(IEnumerator<KeyValuePair<Node, Node>>), "en");
            var result = Expression.Variable(typeof(TValue), "result");
            var loopBreak = Expression.Label();
            var currentVariable = Expression.Variable(typeof(KeyValuePair<Node, Node>), "current");
            var keyVariable = Expression.Variable(typeof(string), "key");
            var valueVariable = Expression.Variable(typeof(Node), "value");

            var constructor = Expression.Block(
                typeof(TValue),
                new[] { enumerator, result },
                Expression.Assign(
                    result,
                    Expression.New(typeof(TValue)) // TODO: Select constructor
                ),
                Expression.Assign(
                    enumerator,
                    Expression.Call(node, mappingGetEnumerator)
                ),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(enumerator, enumeratorMoveNext),
                        Expression.Block(
                            new[] { currentVariable, keyVariable, valueVariable },
                            //Debug<IEnumerator<KeyValuePair<Node, Node>>>(n => Console.WriteLine(n), enumerator),
                            Expression.Assign(
                                currentVariable,
                                Expression.Property(enumerator, mappingEnumeratorCurrent)
                            ),
                            Expression.Assign(
                                keyVariable,
                                stringSchemaNode.GenerateConstructor(
                                    Expression.Property(currentVariable, keyValuePairKey).As<Node>(),
                                    variableAllocator
                                )
                            ),
                            Expression.Assign(
                                valueVariable,
                                Expression.Property(currentVariable, keyValuePairValue)
                            ),
                            Expression.Switch(
                                typeof(void),
                                keyVariable,
                                Expression.Throw(Expression.New(typeof(KeyNotFoundException))), // TODO: Throw a good exception
                                stringComparison,
                                properties.Select(p => Expression.SwitchCase(
                                     Expression.Assign(
                                         Expression.Property(result, p.Value.property),
                                         p.Value.schemaNode.GenerateConstructor(valueVariable, variableAllocator)
                                     ),
                                     Expression.Constant(p.Key) // TODO: Naming convention
                                 ))
                            )
                        ),
                        Expression.Break(loopBreak)
                    ),
                    loopBreak
                ),
                result
            );

            return constructor.As<TValue>();
        }

        public Expr<Mapping> GenerateRepresenter(Expr<TValue> value, IVariableAllocator variableAllocator)
        {
            throw new NotImplementedException();
        }

        private Expression Debug<T>(Action<T> callback, Expression value)
        {
            return Expression.Invoke(Expression.Constant(callback), value);
        }
    }
}

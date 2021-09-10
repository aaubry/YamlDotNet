using FastExpressionCompiler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using StringReader = System.IO.StringReader;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;

namespace YamlDotNet.Sandbox
{
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
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class RecursiveModel
    {
        public List<RecursiveModel>? Children { get; set; }
        public RecursiveModel? SpecialChild { get; set; }
        public ModelWithProperties? MyProperty { get; set; }
    }

    public class Loader : IYamlLoader
    {
        private class TempNodeMapper : INodeMapper
        {
            public TagName Tag { get; }

            public INodeMapper Canonical => this;

            public TempNodeMapper(TagName tag)
            {
                Tag = tag;
            }

            public object? Construct(Node node)
            {
                throw new NotImplementedException();
            }

            public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
            {
                throw new NotImplementedException();
            }
        }


        private readonly Stack<(ISchemaNode schema, Action<Node, ISchemaNode> nodeHandler)> previousStates = new();

        private ISchemaNode currentSchema;
        private Action<Node, ISchemaNode> currentNodeHandler = InvalidState;

        private static void InvalidState(Node _, ISchemaNode __) => throw new InvalidOperationException();

        public List<Document> Documents { get; } = new();

        public Loader(ISchemaNode schema)
        {
            this.currentSchema = schema;
        }

        public void OnStreamStart(Mark start, Mark end)
        {
            if (previousStates.Count != 0)
            {
                throw new InvalidOperationException();
            }

            PushNodeHandler(currentSchema, InvalidState);
        }

        public void OnStreamEnd(Mark start, Mark end)
        {
            PopNodeHandler();

            if (previousStates.Count != 0)
            {
                throw new InvalidOperationException();
            }
        }

        public void OnDocumentStart(Core.Tokens.VersionDirective? version, TagDirectiveCollection? tags, bool isImplicit, Mark start, Mark end)
        {
            PushNodeHandler(currentSchema, (n, _) => Documents.Add(new Document(n, NullSchema.Instance)));
        }

        public void OnDocumentEnd(bool isImplicit, Mark start, Mark end)
        {
            PopNodeHandler();
        }

        public void OnScalar(AnchorName anchor, TagName tag, string value, ScalarStyle style, Mark start, Mark end)
        {
            var scalarSchema = currentSchema.EnterScalar(ref tag, value, style);

            var scalar = new Scalar(new TempNodeMapper(tag), value, start, end);
            currentNodeHandler(scalar, scalarSchema);
        }

        public void OnSequenceStart(AnchorName anchor, TagName tag, SequenceStyle style, Mark start, Mark end)
        {
            var sequenceSchema = currentSchema.EnterSequence(ref tag, style);

            var items = new List<Node>();
            currentNodeHandler(new Sequence(new TempNodeMapper(tag), items), sequenceSchema);
            PushNodeHandler(sequenceSchema, (n, _) => items.Add(n));
        }

        public void OnSequenceEnd(Mark start, Mark end)
        {
            PopNodeHandler();
        }

        public void OnMappingStart(AnchorName anchor, TagName tag, MappingStyle style, Mark start, Mark end)
        {
            var mappingSchema = currentSchema.EnterMapping(ref tag, style);

            var items = new Dictionary<Node, Node>();
            currentNodeHandler(new Mapping(new TempNodeMapper(tag), items), mappingSchema);
            PushNodeHandler(mappingSchema, (key, keySchema) =>
            {
                var keyInnerSchema = keySchema.EnterMappingKey(key, keySchema);
                //var keyInnerSchema = currentSchema.EnterMappingKey(key, keySchema);

                PushNodeHandler(keyInnerSchema, (value, valueSchema) =>
                {
                    items.Add(key, value);
                    PopNodeHandler();
                });
            });
        }

        public void OnMappingEnd(Mark start, Mark end)
        {
            PopNodeHandler();
        }

        public void OnAlias(AnchorName value, Mark start, Mark end)
        {
            throw new NotImplementedException();
        }

        public void OnComment(string value, bool isInline, Mark start, Mark end)
        {
        }

        private void PushNodeHandler(ISchemaNode schema, Action<Node, ISchemaNode> handler)
        {
            previousStates.Push((currentSchema, currentNodeHandler));
            currentSchema = schema;
            currentNodeHandler = handler;
        }

        private void PopNodeHandler()
        {
            (currentSchema, currentNodeHandler) = previousStates.Pop();
        }
    }

    class Program
    {
        public static readonly UnresolvedValueMapper _n = new UnresolvedValueMapper(null);

        public static void Main()
        {
            var schemaBuilder = new SchemaBuilder
            {
                {
                    _ => new ScalarSchemaNode<int>(
                        YamlTagRepository.Integer,
                        s => int.Parse(s.Value),
                        val => new Scalar(_n, val!.ToString()!)
                    ),
                    s => Regex.IsMatch(s.Value, @"\d+"),
                    v => true
                },
                {
                    _ => new ScalarSchemaNode<string>(
                        YamlTagRepository.String,
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
                {
                    b => new MappingSchemaNode<Dictionary<int, string>, int, string>(
                        (p, _) => new Dictionary<int, string>(p),
                        d => d,
                        (_, p) => new Mapping(_n, p),
                        b.BuildSchema<int>(),
                        b.BuildSchema<string>()
                    ),
                    m => true,
                    v => true
                },
                {
                    b => new MappingSchemaNode<Dictionary<int, Dictionary<int, string>>, int, Dictionary<int, string>>(
                        (p, _) => new Dictionary<int, Dictionary<int, string>>(p),
                        d => d,
                        (_, p) => new Mapping(_n, p),
                        b.BuildSchema<int>(),
                        b.BuildSchema<Dictionary<int, string>>()
                    ),
                    m => true,
                    v => true
                }
            };

            var schema = schemaBuilder.BuildSchema<RecursiveModel>();

            Console.WriteLine(SchemaNodeRenderer.Render(schema));

            var loader = new Loader(schema);
            var parser = new Parser2(new StringReader(@"{ Children: [{ Children: [], MyProperty: { Name: John, Age: 31 } }, { Children: [] }] }"), loader);

            //var schema = schemaBuilder.BuildSchema<Dictionary<int, Dictionary<int, string>>>();
            //var loader = new Loader(schema);
            //var parser = new Parser2(new StringReader(@"{ 1: { 5: one }, 2: { 6: two } }"), loader);

            parser.Load();

            Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, loader.Documents);
        }

        public static void Main2()
        {
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
            Roundtrip<RecursiveModel>(
                new Mapping(_n, new Dictionary<Node, Node>
                {
                    {
                        new Scalar(_n, "Children"),
                        new Sequence(_n,
                            new Mapping(_n, new Dictionary<Node, Node>
                            {
                                {
                                    new Scalar(_n, "Children"),
                                    new Sequence(_n)
                                }
                            }),
                            new Mapping(_n, new Dictionary<Node, Node>
                            {
                                {
                                    new Scalar(_n, "Children"),
                                    new Sequence(_n)
                                }
                            })
                        )
                    }
                })
            );

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

                Console.WriteLine(SchemaNodeRenderer.Render(schema));

                Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, new Document(node, NullSchema.Instance));

                var constructor = schema.GenerateConstructor();
                Console.WriteLine("----");
                Console.WriteLine(constructor);

                var cs = constructor.ToCSharpString();

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
    }


}

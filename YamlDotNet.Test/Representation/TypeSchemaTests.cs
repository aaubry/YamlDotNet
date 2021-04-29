//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Test.Representation
{
    public class TypeSchemaTests
    {
        private readonly ITestOutputHelper output;

        public TypeSchemaTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void ExplicitTag()
        {
            var sut = new TypeSchemaBuilder()
                .WithTagMapping("!Dictionary", typeof(Dictionary<string, int>))
                .Build(typeof(SimpleModel));

            var stream = Stream.Load(Yaml.ParserForText(@"
                !Dictionary {
                    key1: 1,
                    key2: 2
                }
            "), _ => sut);

            var content = stream.First().Content;

            var yaml = Stream.Dump(new[] { new Document(content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);

            var value = content.Mapper.Construct(content);
        }

        [Fact]
        public void ExplicitChildTag()
        {
            var sut = new TypeSchemaBuilder()
                .WithTagMapping("!List", typeof(List<IDictionary<string, int>>))
                .WithTagMapping("!Dictionary", typeof(Dictionary<string, int>))
                .Build(typeof(List<IDictionary<string, int>>));

            var stream = Stream.Load(Yaml.ParserForText(@"
                !List
                - !Dictionary { key1: 1, key2: 2 }
            "), _ => sut);

            var content = stream.First().Content;

            var yaml = Stream.Dump(new[] { new Document(content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);

            var value = content.Mapper.Construct(content);
        }

        [Fact]
        public void TypesFromSchemaAreUsed()
        {
            var sut = new TypeSchemaBuilder()
                .Build(typeof(Dictionary<int, string>));

            var iterator = sut.Root;
            iterator = iterator.EnterNode(new FakeEvents.Mapping(TagName.Empty), out var mappingMapper);
            Assert.IsNotType<UnresolvedTagMapper>(mappingMapper);

            iterator = iterator.EnterNode(new FakeEvents.Scalar(TagName.Empty, "123"), out var keyMapper);
            Assert.IsNotType<UnresolvedTagMapper>(keyMapper);

            iterator = iterator.EnterMappingValue();
            iterator = iterator.EnterNode(new FakeEvents.Scalar(TagName.Empty, "World"), out var valueMapper);
            Assert.IsNotType<UnresolvedTagMapper>(valueMapper);

            Assert.IsNotType<NullSchemaIterator>(iterator);
        }

        [Fact]
        public void X()
        {
            var sut = new TypeSchemaBuilder()
                .Build(typeof(SimpleModel));

            output.WriteLine(sut.ToString());

            var stream = Stream.Load(Yaml.ParserForText(@"
                Value: 123
                List: [ 1, 2 ]
                Dict:
                    1: one
                    2: two
                    3: three
                RecursiveChild: { Value: 456 }
                Child: { Name: abc }
            "), _ => sut);

            var content = stream.First().Content;

            var yaml = Stream.Dump(new[] { new Document(content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);


            var value = content.Mapper.Construct(content);

            var model = Assert.IsType<SimpleModel>(value);
            Assert.Equal(123, model.Value);
            Assert.Equal(456, model.RecursiveChild.Value);
            Assert.Equal("abc", model.Child.Name);
            Assert.Equal(new[] { 1, 2 }, model.List);

            Assert.Equal(3, model.Dict.Count);
            Assert.True(model.Dict.TryGetValue(1, out var one) && one == "one");
            Assert.True(model.Dict.TryGetValue(2, out var two) && two == "two");
            Assert.True(model.Dict.TryGetValue(3, out var three) && three == "three");

            var representation = sut.Represent(model);

            yaml = Stream.Dump(new[] { new Document(representation.Content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);

            Assert.Equal(content, representation.Content);

            //var yaml = Stream.Dump(new[] { representation });
            //output.WriteLine("=== Dumped YAML ===");
            //output.WriteLine(yaml);

            //var yaml = Stream.Dump(new[] { new Document(content, FailsafeSchema.Strict) });
            //var yaml = Stream.Dump(new[] { new Document(content, NullSchema.Instance) });
            //output.WriteLine("=== Dumped YAML ===");
            //output.WriteLine(yaml);
        }

        [Fact]
        public void SequenceMapperTest()
        {
            var sut = new TypeSchemaBuilder()
                .Build(typeof(IList<int>));

            output.WriteLine(sut.ToString());

            var doc = sut.Represent(new[] { 1, 2 });

            var yaml = Stream.Dump(new[] { new Document(doc.Content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);
        }

        [Fact]
        public void SequenceMapperTest2()
        {
            var sut = new TypeSchemaBuilder()
                .Build(typeof(IList<IList<int>>));

            output.WriteLine(sut.ToString());

            //var stream = Stream.Load(Yaml.ParserForText(@"
            //    - 1
            //    - 2
            //"), _ => sut);

            //var content = stream.First().Content;

            //var value = content.Mapper.Construct(content);
            //var model = Assert.IsType<List<int>>(value);
            //Assert.Equal(new[] { 1, 2 }, model);

            var doc = sut.Represent(new[] { new[] { 1, 2 }, new[] { 3 } });

            var yaml = Stream.Dump(new[] { new Document(doc.Content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);

            //var yaml = Stream.Dump(new[] { doc });
            //output.WriteLine(yaml);
        }

        [Fact]
        public void MappingMapperTest()
        {
            var sut = new TypeSchemaBuilder()
                .Build(typeof(IDictionary<int, string>));

            output.WriteLine(sut.ToString());

            var doc = sut.Represent(new Dictionary<int, string> { { 1, "one" }, { 2, "two" } });

            var yaml = Stream.Dump(new[] { new Document(doc.Content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);
        }

        [Fact]
        public void ObjectMapperTest()
        {
            var model = new { one = 1, two = "abc" };

            var sut = new TypeSchemaBuilder()
                .Build(model.GetType());

            output.WriteLine(sut.ToString());

            var doc = sut.Represent(model);

            var yaml = Stream.Dump(new[] { new Document(doc.Content, NullSchema.Instance) });
            output.WriteLine("=== Dumped YAML ===");
            output.WriteLine(yaml);
        }

        class TypeSchemaBuilder : BuilderSkeleton<TypeSchemaBuilder>
        {
            private readonly Dictionary<TagName, Type> tagMappings;

            public TypeSchemaBuilder() : base(DynamicTypeResolver.Instance)
            {
                tagMappings = new Dictionary<TagName, Type>();
            }

            protected override TypeSchemaBuilder Self => this;

            public override TypeSchemaBuilder WithTagMapping(TagName tag, Type type)
            {
                tagMappings.Add(tag, type);
                return this;
            }

            public ISchema Build(Type type)
            {
                return BuildSchemaFactory(tagMappings.ToDictionary(p => p.Value, p => p.Key).AsReadonlyDictionary(), false)(type);
            }
        }

        private static class FakeEvents
        {
            public record Scalar(TagName Tag, string Value) : IScalar
            {
                public NodeKind Kind => NodeKind.Scalar;
            }

            public record Mapping(TagName Tag) : IMapping
            {
                public NodeKind Kind => NodeKind.Mapping;
            }

            public record Sequence(TagName Tag) : ISequence
            {
                public NodeKind Kind => NodeKind.Sequence;
            }
        }

#nullable disable

        public class SimpleModel
        {
            public int Value { get; set; }

            public List<int> List { get; set; }
            public Dictionary<int, string> Dict { get; set; }

            //// TODO: List<SimpleModelChild>

            public SimpleModel RecursiveChild { get; set; }
            public SimpleModelChild Child { get; set; }
        }

        public class SimpleModelChild
        {
            public string Name { get; set; }
        }
    }
}

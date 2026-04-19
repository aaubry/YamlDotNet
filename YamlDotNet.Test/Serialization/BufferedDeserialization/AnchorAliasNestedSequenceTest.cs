// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization.BufferedDeserialization;

public class AnchorAliasNestedSequenceTest
{
    [Fact]
    public void AnchorAliasNestedSequence()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var executors = deserializer.Deserialize<List<Executor>>(Document);
        executors[0].Filters.Count.Should().Be(2);
        executors[1].Filters.Count.Should().Be(3);
        foreach (var (x, y) in executors[0].Filters.Zip(executors[1].Filters, (x, y) => (x, y)))
        {
            ReferenceEquals(x, y).Should().BeFalse();
        }
    }

    public class FilterCollection : List<Filter>, IYamlConvertible
    {
        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<SequenceStart>();
            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                if (parser.Accept<AnchorAlias>(out _))
                {
                    AddRange((List<Filter>)nestedObjectDeserializer.Invoke(typeof(List<Filter>))!);
                }

                Add((Filter)nestedObjectDeserializer.Invoke(typeof(Filter))!);
            }
        }

        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotImplementedException();
        }
    }

    public const string Document = @"
- name: a
  filters: &shared
    - type: foo
      value: 1
    - type: bar
      value: 2
- name: b
  filters: 
    - *shared
    - type: extra
      value: 3
";

    public class Executor
    {
        public string Name { get; set; }
        public FilterCollection Filters { get; set; }
    }

    public class Filter
    {
        public string Type { get; set; }
        public int Value { get; set; }
    }
}

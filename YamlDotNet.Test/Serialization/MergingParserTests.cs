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

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Test.Core;

namespace YamlDotNet.Test.Serialization
{
    public class MergingParserTests: EmitterTestsHelper
    {
        [Fact]
        public void MergingParserWithMergeObjectWithSequence_EachLevelsShouldEquals()
        {
            var yaml = @"base_level: &base 
  tenant: 
  - a1
  - a2
Level1: &Level1
    <<: [*base]
Level2: &Level2
    <<: *Level1
";
            
            var etalonLevel = @"tenant:
- a1
- a2
".NormalizeNewLines();
            
            var mergingParser = new MergingParser(new Parser(new StringReader(yaml)));
            var yamlObject = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object>>(mergingParser);
            yamlObject.Should().NotBeNull();

            var serializer = new SerializerBuilder().Build();
            serializer.Serialize(yamlObject["base_level"]).NormalizeNewLines().Should().Be(etalonLevel);
            serializer.Serialize(yamlObject["Level1"]).NormalizeNewLines().Should().Be(etalonLevel);
            serializer.Serialize(yamlObject["Level2"]).NormalizeNewLines().Should().Be(etalonLevel);
        } 
        
        [Fact]
        public void MergingParserWithMergeObjectWithSequence_EmittedTextShouldNotContainsDeletedEvents()
        {
            var yaml = @"base_level: &base 
  tenant:
  - a1
  - a2
Level1: &Level1
    <<: [*base]
Level2:
    <<: *Level1
";

            var etalonEmittedText = @"base_level: &base
  tenant:
  - a1
  - a2
Level1: &Level1
  tenant:
  - a1
  - a2
Level2:
  tenant:
  - a1
  - a2
".NormalizeNewLines();
            
            var mergingParser = new MergingParser(new Parser(new StringReader(yaml)));
            var events = EnumerationOf(mergingParser);
            EmittedTextFrom(events).NormalizeNewLines().Should().Be(etalonEmittedText);
        }  
        
        [Fact]
        public void MergingParserWithMergeObjectWithSequenceAndScalarItems_EmittedTextShouldNotContainsDeletedEvents()
        {
            var yaml = @"base_level: &base 
  tenant:
  - a1
  - a2
Level1: &Level1
    <<: [*base]
    item1:
Level2:
    <<: *Level1
    item2:
";

            var etalonEmittedText = @"base_level: &base
  tenant:
  - a1
  - a2
Level1: &Level1
  tenant:
  - a1
  - a2
  item1: ''
Level2:
  tenant:
  - a1
  - a2
  item1: ''
  item2: ''
".NormalizeNewLines();
            
            var mergingParser = new MergingParser(new Parser(new StringReader(yaml)));
            var events = EnumerationOf(mergingParser);
            EmittedTextFrom(events).NormalizeNewLines().Should().Be(etalonEmittedText);
        }

        [Fact]
        public void MergingParserWithNestedSequence_ShouldNotThrowException()
        {
            var yaml = @"
base_level: &base {}
Level1: &Level1
    <<: [*base]
Level2: &Level2
    <<: [*Level1]
Level3:
    <<: *Level2
";
            var etalonMergedYaml = @"base_level: {}
Level1: {}
Level2: {}
Level3: {}
".NormalizeNewLines();

            var mergingParserFailed = new MergingParser(new Parser(new StringReader(yaml)));
            var yamlObject = new DeserializerBuilder().Build().Deserialize(mergingParserFailed);

            new SerializerBuilder().Build().Serialize(yamlObject!).NormalizeNewLines().Should().Be(etalonMergedYaml);
        }
    }
}

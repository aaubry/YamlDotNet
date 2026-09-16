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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Test.Core;

namespace YamlDotNet.Test.Serialization
{
    public class MergingParserTests : EmitterTestsHelper
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

        [Fact]
        public async Task MergingParserWithMergeKeyBomb_ShouldThrowExceptionWhenTooManyEvents()
        {
            // Timebox this test to avoid infinite loops in case of bugs.
            // 30 seconds should be more than enough for this test to run even on a slow machine, and if it takes longer than that,
            // it's likely that the merging parser is not correctly counting events and enforcing the limit.
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            cancellationTokenSource.Token.Register(() =>
            {
                throw new TimeoutException("The test took too long, likely due to an infinite loop in the merging parser.");
            });

            try
            {
                await Task.Run(() =>
                {
                    var sb = new StringBuilder();

                    // Base anchor
                    sb.AppendLine("a0: &a0");
                    sb.AppendLine("  x: 1");
                    sb.AppendLine();

                    // Each level merges the previous anchor TWICE (fanout=2), doubling event count
                    for (int i = 1; i <= 25; i++)
                    {
                        sb.AppendLine($"a{i}: &a{i}");
                        sb.AppendLine($"  <<: *a{i - 1}");  // first merge
                        sb.AppendLine($"  <<: *a{i - 1}");  // second merge
                        sb.AppendLine();
                    }

                    sb.AppendLine("final:");
                    sb.AppendLine("  <<: *a25");

                    var yaml = sb.ToString();
                    var parser = new Parser(new StringReader(yaml));
                    var mergingParser = new MergingParser(parser, 1000);
                    try
                    {
                        while (mergingParser.MoveNext())
                        {
                            //move through everything, we're in a timebox so if this takes too long, the cancellation token will trigger and fail the test
                        }
                    }
                    catch (YamlException ex) when (ex.Message.Contains("Too many parsing events"))
                    {
                        // Expected exception, test passes
                        return;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unexpected exception", ex);
                    }
                }, cancellationTokenSource.Token);
            }
            catch (TimeoutException ex)
            {
                Assert.Fail($"Test failed due to timeout: {ex.Message}");
            }
        }

        [Fact]
        public async Task MergingParserWithManySmallMerges_ShouldThrowExceptionWhenCumulativeEventsExceedLimit()
        {
            // Timebox this test to avoid infinite loops in case of bugs.
            // 30 seconds should be more than enough for this test to run even on a slow machine, and if it takes longer than that,
            // it's likely that the merging parser is not correctly counting events and enforcing the limit.
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            cancellationTokenSource.Token.Register(() =>
            {
                throw new TimeoutException("The test took too long, likely due to an infinite loop in the merging parser.");
            });

            try
            {
                await Task.Run(() =>
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("base: &base");
                    for (var i = 0; i < 25; i++)
                    {
                        sb.AppendLine($"  k{i}: v{i}");
                    }

                    sb.AppendLine();
                    for (var i = 0; i < 35; i++)
                    {
                        sb.AppendLine($"entry{i}:");
                        sb.AppendLine("  <<: *base");
                        sb.AppendLine();
                    }

                    var parser = new Parser(new StringReader(sb.ToString()));
                    var mergingParser = new MergingParser(parser, 1000);
                    var totalParsedEvents = 0;
                    Action parse = () =>
                    {
                        while (mergingParser.MoveNext())
                        {
                            totalParsedEvents++;
                        }
                    };

                    parse.Should().Throw<YamlException>()
                        .Where(ex => ex.Message.Contains("Too many parsing events"));
                    Console.WriteLine($"Total parsed events before exception: {totalParsedEvents}");
                }, cancellationTokenSource.Token);
            }
            catch (TimeoutException ex)
            {
                Assert.Fail($"Test failed due to timeout: {ex.Message}");
            }
        }

        [Fact]
        public void MergingParserWithDeepSingleChain_ShouldParseWithinLimit()
        {
            const int depth = 200;
            var sb = new StringBuilder();

            sb.AppendLine("a0: &a0");
            sb.AppendLine("  root: value");
            sb.AppendLine();

            for (var i = 1; i <= depth; i++)
            {
                sb.AppendLine($"a{i}: &a{i}");
                sb.AppendLine($"  <<: *a{i - 1}");
                sb.AppendLine($"  level{i}: {i}");
                sb.AppendLine();
            }

            sb.AppendLine("final:");
            sb.AppendLine($"  <<: *a{depth}");

            var parser = new Parser(new StringReader(sb.ToString()));
            var mergingParser = new MergingParser(parser, 50000);
            var deserializer = new DeserializerBuilder().Build();

            var yamlObject = deserializer.Deserialize<Dictionary<string, object>>(mergingParser);

            yamlObject.Should().ContainKey("final");
        }
    }
}

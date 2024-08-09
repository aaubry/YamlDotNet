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

using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.Test.Serialization;

public class ForcedPlainStyleTests
{
    /// <summary>
    /// This class tests the regular 'Plain' scalar style
    /// </summary>
    private class PlainStyleModel
    {
        public string MightBeQuotedText { get; set; }
    }

    /// <summary>
    /// This class tests the new 'ForcePlain' scalar style
    /// </summary>
    private class ForcedPlainStyleModel
    {
        [YamlMember(ScalarStyle = ScalarStyle.ForcePlain)]
        public string AlwaysUnquotedText { get; set; }
    }

    // Test the regular 'Plain' scalar type
    [Fact]
    public void Plain_Values_With_Control_Flow_Chars_Should_Be_Quoted()
    {
        var sut = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = sut.Serialize(new PlainStyleModel { MightBeQuotedText = "{{ a.b.c }}" });
        Assert.StartsWith("MightBeQuotedText: '{{ a.b.c }}'", yaml);
    }

    [Fact]
    public void Values_With_No_Control_Flow_Chars_Should_Not_Be_Quoted()
    {
        var sut = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = sut.Serialize(new PlainStyleModel { MightBeQuotedText = "Some regular text" });
        Assert.StartsWith("MightBeQuotedText: Some regular text", yaml);
    }

    // Test the new regular 'ForcedPlain' scalar type
    [Fact]
    public void Forced_Plain_Values_With_Control_Flow_Chars_Should_Not_Be_Quoted()
    {
        var sut = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = sut.Serialize(new ForcedPlainStyleModel { AlwaysUnquotedText = "{{ a.b.c }}" });
        Assert.StartsWith("AlwaysUnquotedText: {{ a.b.c }}", yaml);
        Assert.DoesNotContain("'", yaml);
    }

    [Fact]
    public void Forced_Plain_Values_With_No_Control_Flow_Chars_Should_Not_Be_Quoted()
    {
        var sut = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = sut.Serialize(new ForcedPlainStyleModel { AlwaysUnquotedText = "Regular text" });
        Assert.StartsWith("AlwaysUnquotedText: Regular text", yaml);
        Assert.DoesNotContain("'", yaml);
    }
}

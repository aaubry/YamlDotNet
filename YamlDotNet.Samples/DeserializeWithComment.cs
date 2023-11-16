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
using System.IO;
using Xunit.Abstractions;
using YamlDotNet.Core.ParsingComments;
using YamlDotNet.Samples.Helpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


namespace YamlDotNet.Samples
{
    public class DeserializeWithComment
    {
        private readonly ITestOutputHelper output;

        public DeserializeWithComment(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            DisplayName = "Deserializing with comments",
            Description = "Shows how to process comments"
        )]
        public void Main()
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .BuildWithCommentsDeserializer();
            var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .JsonCompatible()
                    .Build();

            var content = WithInlineCommentsAndList;

            var parser = new ParserWithComments(new ScannerWithComments(new StringReader(content)));
            var yamlObject = deserializer.Deserialize<object>(parser);

            var json = serializer.Serialize(yamlObject);

            output.WriteLine(json);
            Console.WriteLine(json);

        }

        private const string WithInlineCommentsAndList =
@"valuelist:
   - string1  #{1st comment}
   - string2
# block comment
   - string3  #{2nd comment}
simplevalue: 12
objectlist:
  - att1: 12
    att2: v1
  - att1: 13
    att2: v2
  - att1: 14 #3rd comment
    att2: v3    #4th comment
";
    }
}

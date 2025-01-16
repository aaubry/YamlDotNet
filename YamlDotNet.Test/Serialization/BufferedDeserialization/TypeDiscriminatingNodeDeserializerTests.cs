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
using FluentAssertions;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization.BufferedDeserialization
{
    public class TypeDiscriminatingNodeDeserializerTests
    {
        [Fact]
        public void TypeDiscriminatingNodeDeserializer_ThrowsWhen_MaxDepthExceeded()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<object>("kind", new Dictionary<string, Type>());
                },
                    maxDepth: 2,
                    maxLength: 40)
                .Build();

            Action act = () => bufferedDeserializer.Deserialize<object>(KubernetesServiceYaml);
            act
              .Should().Throw<YamlException>()
              .WithMessage("*Failed to buffer yaml node")
              .WithInnerException<ArgumentOutOfRangeException>()
              .WithMessage("Parser buffer exceeded max depth*");
        }

        [Fact]
        public void TypeDiscriminatingNodeDeserializer_ThrowsWhen_MaxLengthExceeded()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<object>("kind", new Dictionary<string, Type>());
                },
                    maxDepth: 3,
                    maxLength: 20)
                .Build();

            Action act = () => bufferedDeserializer.Deserialize<object>(KubernetesServiceYaml);
            act
              .Should().Throw<YamlException>()
              .WithMessage("*Failed to buffer yaml node")
              .WithInnerException<ArgumentOutOfRangeException>()
              .WithMessage("Parser buffer exceeded max length*");
        }

        public const string KubernetesServiceYaml = @"
apiVersion: v1
kind: Service
metadata:
  name: my-service
spec:
  selector:
    app.kubernetes.io/name: MyApp
  ports:
    - protocol: TCP
      port: 80
      targetPort: 9376
";
    }
}

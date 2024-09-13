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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Test.Serialization.BufferedDeserialization
{
    public class KeyValueTypeDiscriminatorTests
    {
        [Fact]
        public void KeyValueTypeDiscriminator_WithParentBaseType_Single()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 40)
                .Build();

            var service = bufferedDeserializer.Deserialize<KubernetesResource>(KubernetesServiceYaml);
            service.Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_WithParentBaseType_List()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 40)
                .Build();

            var resources = bufferedDeserializer.Deserialize<List<KubernetesResource>>(ListOfKubernetesYaml);
            resources[0].Should().BeOfType<KubernetesNamespace>();
            resources[1].Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_WithObjectBaseType_Single()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<object>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 40)
                .Build();

            var service = bufferedDeserializer.Deserialize<object>(KubernetesServiceYaml);
            service.Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_WithObjectBaseType_List()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<object>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 30)
                .Build();

            var resources = bufferedDeserializer.Deserialize<List<object>>(ListOfKubernetesYaml);
            resources[0].Should().BeOfType<KubernetesNamespace>();
            resources[1].Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_WithInterfaceBaseType_Single()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<IKubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 40)
                .Build();

            var service = bufferedDeserializer.Deserialize<IKubernetesResource>(KubernetesServiceYaml);
            service.Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_WithInterfaceBaseType_List()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<IKubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 30)
                .Build();

            var resources = bufferedDeserializer.Deserialize<List<IKubernetesResource>>(ListOfKubernetesYaml);
            resources[0].Should().BeOfType<KubernetesNamespace>();
            resources[1].Should().BeOfType<KubernetesService>();
        }

        [Fact]
        public void KeyValueTypeDiscriminator_MultipleWithSameKey()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeDiscriminatingNodeDeserializer(options =>
                {
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Namespace", typeof(KubernetesNamespace) },
                        });
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        new Dictionary<string, Type>()
                        {
                            { "Service", typeof(KubernetesService) }
                        });
                },
                    maxDepth: 3,
                    maxLength: 40)
                .Build();

            var resources = bufferedDeserializer.Deserialize<List<KubernetesResource>>(ListOfKubernetesYaml);
            resources[0].Should().BeOfType<KubernetesNamespace>();
            resources[1].Should().BeOfType<KubernetesService>();
        }

        public const string ListOfKubernetesYaml = @"
- apiVersion: v1
  kind: Namespace
  metadata:
    name: test-namespace
- apiVersion: v1
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

        public interface IKubernetesResource { }

        public class KubernetesResource : IKubernetesResource
        {
            public string ApiVersion { get; set; }
            public string Kind { get; set; }
            public KubernetesMetadata Metadata { get; set; }

            public class KubernetesMetadata
            {
                public string Name { get; set; }
            }
        }

        public class KubernetesService : KubernetesResource
        {
            public KubernetesServiceSpec Spec { get; set; }
            public class KubernetesServiceSpec
            {
                public Dictionary<string, string> Selector { get; set; }
                public List<KubernetesServicePort> Ports { get; set; }
                public class KubernetesServicePort
                {
                    public string Protocol { get; set; }
                    public int Port { get; set; }
                    public int TargetPort { get; set; }
                }
            }
        }

        public class KubernetesNamespace : KubernetesResource
        {

        }
    }
}

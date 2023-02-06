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
    public class BufferedDeserializerTest
    {
        [Fact]
        public void KeyValueTypeDiscriminator_WithParentBaseType_Single()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
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
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<KubernetesResource>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
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
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<object>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
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
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<object>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
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
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<IKubernetesResource>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
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
                .WithBufferedNodeDeserializer(options => {
                    options.AddKeyValueTypeDiscriminator<IKubernetesResource>(
                        "kind",
                        ("Namespace", typeof(KubernetesNamespace)),
                        ("Service", typeof(KubernetesService)));
                    },
                    maxDepth: 3,
                    maxLength: 30)
                .Build();

            var resources = bufferedDeserializer.Deserialize<List<IKubernetesResource>>(ListOfKubernetesYaml);
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

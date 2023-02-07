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
    public class BufferedNodeDeserializerTests
    {
        [Fact]
        public void BufferedNodeDeserializer_ThrowsWhen_MaxDepthExceeded()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithBufferedNodeDeserializer(options => {
                        options.AddKeyValueTypeDiscriminator<object>("kind", new Dictionary<string, Type>());
                    },
                    maxDepth: 2,
                    maxLength: 40)
                .Build();

            Action act = () => bufferedDeserializer.Deserialize<object>(KubernetesServiceYaml);
            act
              .ShouldThrow<YamlException>()
              .WithMessage("Failed to buffer yaml node")
              .WithInnerException<ArgumentOutOfRangeException>()
              .Where(e => e.InnerException.Message.Contains("Parser buffer exceeded max depth"));
        }
        
        [Fact]
        public void BufferedNodeDeserializer_ThrowsWhen_MaxLengthExceeded()
        {
            var bufferedDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithBufferedNodeDeserializer(options => {
                        options.AddKeyValueTypeDiscriminator<object>("kind", new Dictionary<string, Type>());
                    },
                    maxDepth: 3,
                    maxLength: 20)
                .Build();

            Action act = () => bufferedDeserializer.Deserialize<object>(KubernetesServiceYaml);
            act
              .ShouldThrow<YamlException>()
              .WithMessage("Failed to buffer yaml node")
              .WithInnerException<ArgumentOutOfRangeException>()
              .Where(e => e.InnerException.Message.Contains("Parser buffer exceeded max length"));
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

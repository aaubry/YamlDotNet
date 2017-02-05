using System.IO;
using Xunit.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Samples.Helpers;

namespace YamlDotNet.Samples
{
    public class ConvertYamlToJson
    {
        private readonly ITestOutputHelper output;

        public ConvertYamlToJson(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            DisplayName = "Convert YAML to JSON",
            Description = "Shows how to convert a YAML document to JSON."
        )]
        public void Main()
        {
            // convert string/file to YAML object
            var r = new StringReader(@"
scalar: a scalar
sequence:
  - one
  - two
");
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(r);

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);

            output.WriteLine(json);
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Samples.Helpers;

namespace YamlDotNet.Samples
{
    public class ValidatingDuringDeserialization
    {
        private readonly ITestOutputHelper output;

        public ValidatingDuringDeserialization(ITestOutputHelper output)
        {
            this.output = output;
        }

        // First, we'll implement a new INodeDeserializer
        // that will decorate another INodeDeserializer with validation:
        public class ValidatingNodeDeserializer : INodeDeserializer
        {
            private readonly INodeDeserializer _nodeDeserializer;

            public ValidatingNodeDeserializer(INodeDeserializer nodeDeserializer)
            {
                _nodeDeserializer = nodeDeserializer;
            }

            public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
            {
                if (_nodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value))
                {
                    var context = new ValidationContext(value, null, null);
                    Validator.ValidateObject(value, context, true);
                    return true;
                }
                return false;
            }
        }

        [Sample(
            DisplayName = "Validating during deserialization",
            Description = @"
                By manipulating the list of node deserializers,
                it is easy to add behavior to the deserializer.
                This example shows how to validate the objects as they are deserialized.
            "
        )]
        public void Main()
        {
            // Then we wrap the existing ObjectNodeDeserializer
            // with our ValidatingNodeDeserializer:
            var deserializer = new DeserializerBuilder()
                .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
                .Build();

            // This will fail with a validation exception
            var ex = Assert.Throws<YamlException>(() =>
                deserializer.Deserialize<Data>(new StringReader(@"Name: ~"))
            );

            Assert.IsType<ValidationException>(ex.InnerException);
        }
    }

    public class Data
    {
        [Required]
        public string Name { get; set; }
    }
}
//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

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
using YamlDotNet.Representation;

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

            public bool Deserialize(Node node, Type expectedType, IValueDeserializer deserializer, out object value)
            {
                if (_nodeDeserializer.Deserialize(node, expectedType, deserializer, out value))
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
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Representation;
using YamlDotNet.Representation.Schemas;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.Schemas
{
    public sealed class ObjectMapper2 : INodeMapper
    {
        private readonly Type type;
        private readonly bool ignoreUnmatchedProperties;
        private readonly Func<Node, object?> constructor;

        public ObjectMapper2(Type type, IEnumerable<IPropertyDescriptor> properties, TagName tag, bool ignoreUnmatchedProperties)
        {
            this.type = type;
            Tag = tag;
            this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;

            this.constructor = GenerateConstructor(type, properties, ignoreUnmatchedProperties);
        }

        public TagName Tag { get; }
        public INodeMapper Canonical => this;

        private static Func<Node, object?> GenerateConstructor(Type type, IEnumerable<IPropertyDescriptor> properties, bool ignoreUnmatchedProperties)
        {
            // TODO: >>>>>> Continue here

            //var candidateConstructors = type.GetConstructors(BindingFlags.Public);
            // TODO: Allow to filter the list of candidate constructors

            // TODO: Support multiple constructors
            //Expression.IfThen


            //var constructor = candidateConstructors.Single();

            //var newExpression = 

            throw new NotImplementedException();
        }

        public object? Construct(Node node) => constructor(node);
        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class ObjectMapper : INodeMapper
    {
        private readonly Type type;
        private readonly bool ignoreUnmatchedProperties;

        public ObjectMapper(Type type, TagName tag, bool ignoreUnmatchedProperties)
        {
            this.type = type;
            Tag = tag;
            this.ignoreUnmatchedProperties = ignoreUnmatchedProperties;
        }

        public TagName Tag { get; }
        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            var mapping = node.Expect<Mapping>();

            // TODO: Pre-calculate the constructor(s) using expression trees
            // TODO: Object factory ?
            var native = Activator.CreateInstance(type)!;
            foreach (var (keyNode, valueNode) in mapping)
            {
                var key = keyNode.Mapper.Construct(keyNode);
                var keyAsString = TypeConverter.ChangeType<string>(key);
                // TODO: Naming convention
                // TODO: Type inspector
                var property = type.GetPublicProperty(keyAsString);
                if (property == null)
                {
                    if (ignoreUnmatchedProperties)
                    {
                        continue;
                    }
                    throw new YamlException(keyNode.Start, keyNode.End, $"The property '{keyAsString}' was not found on type '{type.FullName}'."); // TODO: Exception type
                }

                var value = valueNode.Mapper.Construct(valueNode);
                var convertedValue = TypeConverter.ChangeType(value, property.PropertyType);
                property.SetValue(native, convertedValue, null);
            }
            return native;
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            if (native is null) // TODO: Do we need this ?
            {
                return NullMapper.Default.NullScalar;
            }

            state.RecursionLevel.Increment();

            var children = new Dictionary<Node, Node>();

            // Notice that the children collection will still be mutated after constructing the Sequence object.
            // We need to create it now in order to memorize it.
            var mapping = new Mapping(this, children.AsReadonlyDictionary());
            state.MemorizeRepresentation(native, mapping);

            // TODO: Type inspector
            // TODO: Get the properties from the iterator ?
            var properties = native.GetType().GetPublicProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(native, null);
                // TODO: Proper null handling
                if (value != null)
                {
                    var key = property.Name; // TODO: Naming convention

                    // Here we use EnterNode instead of EnterValue because we'll need to match the value
                    // TODO: If we iterated the children from the iterator, we wouldn't need to do this!
                    var keyIterator = iterator.EnterNode(new PropertyName(key), out var keyMapper);
                    var keyNode = keyMapper.RepresentMemorized(key, keyIterator, state);

                    var valueIterator = keyIterator.EnterMappingValue().EnterValue(value, out var valueMapper);
                    var valueNode = valueMapper.RepresentMemorized(value, valueIterator, state);

                    children.Add(keyNode, valueNode);
                }
            }

            state.RecursionLevel.Decrement();

            return mapping;
        }

        private sealed class PropertyName : IScalar
        {
            public PropertyName(string value)
            {
                Value = value;
            }

            public string Value { get; }
            public NodeKind Kind => NodeKind.Scalar;
            public TagName Tag => TagName.Empty;
        }

        public override string ToString()
        {
            return Tag.ToString();
        }
    }
}

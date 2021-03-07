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
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Representation.Schemas
{
    /// <summary>
    /// The tag:yaml.org,2002:map tag, as specified in the Failsafe schema.
    /// </summary>
    /// <typeparam name="TMapping">The type of the mappings that this will handle.</typeparam>
    /// <typeparam name="TKey">The type of the keys of the mapping.</typeparam>
    /// <typeparam name="TValue">The type of the values of the mapping.</typeparam>
    /// <remarks>
    /// Uses <typeparamref name="TMapping"/> as native representation.
    /// </remarks>
    public sealed class MappingMapper<TMapping, TKey, TValue> : INodeMapper
        where TMapping : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly CollectionFactory<TMapping> factory;

        public MappingMapper(TagName tag, CollectionFactory<TMapping> factory)
        {
            Tag = tag;
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TagName Tag { get; }
        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            var mapping = node.Expect<Mapping>();
            var dictionary = factory(mapping.Count);
            foreach (var (keyNode, valueNode) in mapping)
            {
                var key = keyNode.Mapper.Construct(keyNode);
                var convertedKey = TypeConverter.ChangeType<TKey>(key);

                var value = valueNode.Mapper.Construct(valueNode);
                var convertedValue = TypeConverter.ChangeType<TValue>(value);

                dictionary.Add(convertedKey, convertedValue);
            }
            return dictionary;
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            state.RecursionLevel.Increment();

            var children = new Dictionary<Node, Node>();

            // Notice that the children collection will still be mutated after constructing the Sequence object.
            // We need to create it now in order to memorize it.
            var mapping = new Mapping(this, children.AsReadonlyDictionary());
            state.MemorizeRepresentation(native!, mapping);

            foreach (var (key, value) in (IDictionary<TKey, TValue>)native!)
            {

                var keyIterator = iterator.EnterValue(key, out var keyMapper);
                var keyNode = keyMapper.RepresentMemorized(key, keyIterator, state);

                var valueIterator = keyIterator.EnterMappingValue().EnterValue(value, out var valueMapper);
                var valueNode = valueMapper.RepresentMemorized(value, valueIterator, state);
                children.Add(keyNode, valueNode);
            }

            state.RecursionLevel.Decrement();
            return mapping;
        }
    }

    public static class MappingMapper<TKey, TValue> where TKey : notnull
    {
        public static readonly MappingMapper<IDictionary<TKey, TValue>, TKey, TValue> Default = new MappingMapper<IDictionary<TKey, TValue>, TKey, TValue>(YamlTagRepository.Mapping, n => new Dictionary<TKey, TValue>(n));
    }

    public static class MappingMapper
    {
        public static INodeMapper Default(Type keyType, Type valueType)
        {
            var mapperType = typeof(MappingMapper<,>).MakeGenericType(keyType, valueType);
            var defaultField = mapperType.GetPublicStaticField(nameof(MappingMapper<object, object>.Default))
                ?? throw new MissingMemberException($"Expected to find a property named '{nameof(MappingMapper<object, object>.Default)}' in class '{mapperType.FullName}'.");

            return (INodeMapper)defaultField.GetValue(null)!;
        }

        public static INodeMapper Create(Type mappingType, Type keyType, Type valueType)
        {
            return Create(YamlTagRepository.Mapping, mappingType, keyType, valueType);
        }

        public static INodeMapper Create(TagName tag, Type mappingType, Type keyType, Type valueType)
        {
            return (INodeMapper)createHelperGenericMethod
                .MakeGenericMethod(mappingType, keyType, valueType)
                .Invoke(null, new object[] { tag })!;
        }

        private static readonly MethodInfo createHelperGenericMethod = typeof(MappingMapper).GetPrivateStaticMethod(nameof(CreateHelper))
            ?? throw new MissingMethodException($"Expected to find a method named '{nameof(CreateHelper)}' in class '{typeof(MappingMapper).FullName}'.");

        private static INodeMapper CreateHelper<TMapping, TKey, TValue>(TagName tag)
            where TMapping : IDictionary<TKey, TValue>
            where TKey : notnull
        {
            var factory = CollectionFactoryHelper.CreateFactory<TMapping>();
            return new MappingMapper<TMapping, TKey, TValue>(tag, factory);
        }
    }
}

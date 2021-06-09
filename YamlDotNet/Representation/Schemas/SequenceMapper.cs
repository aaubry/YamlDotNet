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
    /// A mapper of sequences to collections.
    /// </summary>
    /// <typeparam name="TSequence">The type of the sequences that this will handle.</typeparam>
    /// <typeparam name="TElement">The type of the elements of the sequence.</typeparam>
    /// <remarks>
    /// Uses <typeparamref name="TSequence"/> as native representation.
    /// </remarks>
    public sealed class SequenceMapper<TSequence, TElement> : INodeMapper
        where TSequence : ICollection<TElement>
    {
        private readonly CollectionFactory<TSequence> factory;

        public SequenceMapper(TagName tag, CollectionFactory<TSequence> factory)
        {
            Tag = tag;
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TagName Tag { get; }
        public INodeMapper Canonical => this;

        public object? Construct(Node node)
        {
            var sequence = node.Expect<Sequence>();
            var collection = factory(sequence.Count);

            // Handle pre-allocated and fixed-size collections, such as arrays
            if (collection.Count != 0 && collection is IList<TElement> list)
            {
                for (var i = 0; i < sequence.Count; ++i)
                {
                    var child = sequence[i];
                    var item = child.Mapper.Construct(child);
                    var convertedItem = TypeConverter.ChangeType<TElement>(item);
                    list[i] = convertedItem;
                }
            }
            else
            {
                foreach (var child in sequence)
                {
                    var item = child.Mapper.Construct(child);
                    var convertedItem = TypeConverter.ChangeType<TElement>(item);
                    collection.Add(convertedItem);
                }
            }
            return collection;
        }

        public Node Represent(object? native, ISchemaIterator iterator, IRepresentationState state)
        {
            state.RecursionLevel.Increment();

            var children = new List<Node>();

            // Notice that the children collection will still be mutated after constructing the Sequence object.
            // We need to create it now in order to memorize it.
            var sequence = new Sequence(this, children.AsReadonlyList());
            state.MemorizeRepresentation(native!, sequence);

            foreach (var item in (ICollection<TElement>)native!)
            {
                var itemIterator = iterator.EnterValue(item, out var itemMapper);
                var childNode = itemMapper.RepresentMemorized(item, itemIterator, state);
                children.Add(childNode);
            }

            state.RecursionLevel.Decrement();

            return sequence;
        }
    }

    public static class SequenceMapper<T>
    {
        public static readonly SequenceMapper<ICollection<T>, T> Default = new SequenceMapper<ICollection<T>, T>(YamlTagRepository.Sequence, n => new List<T>(n));
    }

    public static class SequenceMapper
    {
        public static INodeMapper Default(Type itemType)
        {
            var mapperType = typeof(SequenceMapper<>).MakeGenericType(itemType);
            var defaultField = mapperType.GetPublicStaticField(nameof(SequenceMapper<object>.Default))
                ?? throw new MissingMemberException($"Expected to find a property named '{nameof(SequenceMapper<object>.Default)}' in class '{mapperType.FullName}'.");

            return (INodeMapper)defaultField.GetValue(null)!;
        }

        public static INodeMapper Create(Type sequenceType, Type itemType)
        {
            return Create(YamlTagRepository.Sequence, sequenceType, itemType);
        }

        public static INodeMapper Create(TagName tag, Type sequenceConcreteType, Type itemType)
        {
            return (INodeMapper)createHelperGenericMethod
                .MakeGenericMethod(sequenceConcreteType, itemType)
                .Invoke(null, new object[] { tag })!;
        }

        private static readonly MethodInfo createHelperGenericMethod = typeof(SequenceMapper).GetPrivateStaticMethod(nameof(CreateHelper))
            ?? throw new MissingMethodException($"Expected to find a method named '{nameof(CreateHelper)}' in class '{typeof(SequenceMapper).FullName}'.");

        private static INodeMapper CreateHelper<TSequence, TItem>(TagName tag)
            where TSequence : ICollection<TItem>
        {
            var factory = CollectionFactoryHelper.CreateFactory<TSequence>();
            return new SequenceMapper<TSequence, TItem>(tag, factory);
        }
    }
}

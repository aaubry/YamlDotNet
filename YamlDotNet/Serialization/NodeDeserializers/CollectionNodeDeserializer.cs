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
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class CollectionNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory objectFactory;
        private readonly CollectionPopulatingStrategy populatingStrategy;

        public CollectionNodeDeserializer(IObjectFactory objectFactory, CollectionPopulatingStrategy populatingStrategy = CollectionPopulatingStrategy.CreateNew)
        {
            this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            this.populatingStrategy = populatingStrategy;
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?, object?> nestedObjectDeserializer, out object? value, object? currentValue)
        {
            IList? list;
            var canUpdate = true;
            Type itemType;
            var genericCollectionType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(ICollection<>));
            if (genericCollectionType != null)
            {
                var genericArguments = genericCollectionType.GetGenericArguments();
                itemType = genericArguments[0];

                value = (currentValue == null || populatingStrategy == CollectionPopulatingStrategy.CreateNew) ? objectFactory.Create(expectedType) : currentValue;

                list = value as IList;
                if (list == null)
                {
                    // Uncommon case where a type implements IList<T> but not IList
                    var genericListType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IList<>));
                    canUpdate = genericListType != null;
                    list = (IList?)Activator.CreateInstance(typeof(GenericCollectionToNonGenericAdapter<>).MakeGenericType(itemType), value);

                    // TODO: How to handle pre-existing instance in this case?
                    if (populatingStrategy != CollectionPopulatingStrategy.CreateNew)
                    {
                        throw new NotSupportedException($"Types implementing generic interface {typeof(IList<>).Name} but not non-generic interface {typeof(IList).Name} are not yet supported when using {nameof(Deserializer.PopulateObject)}() in combination with {populatingStrategy}.");
                    }
                }
            }
            else if (typeof(IList).IsAssignableFrom(expectedType))
            {
                itemType = typeof(object);

                value = (currentValue == null || populatingStrategy == CollectionPopulatingStrategy.CreateNew) ? objectFactory.Create(expectedType) : currentValue;
                list = (IList)value;
            }
            else
            {
                value = null;
                return false;
            }

            DeserializeHelper(itemType, parser, nestedObjectDeserializer, list!, canUpdate, populatingStrategy);

            return true;
        }

        internal static void DeserializeHelper(Type tItem, IParser parser, Func<IParser, Type, object?, object?> nestedObjectDeserializer, IList result, bool canUpdate, CollectionPopulatingStrategy populatingStrategy = CollectionPopulatingStrategy.CreateNew)
        {
            parser.Consume<SequenceStart>();
            var i = 0;
            while (!parser.TryConsume<SequenceEnd>(out var _))
            {
                var current = parser.Current;

                var currentValue = populatingStrategy == CollectionPopulatingStrategy.PopulateOrAddItems ? (result.Count > i ? result[i] : null) : null;
                var isPopulating = currentValue != null;

                var value = nestedObjectDeserializer(parser, tItem, currentValue);
                if (value is IValuePromise promise)
                {
                    if (canUpdate)
                    {
                        if (isPopulating == false)
                        {
                            var index = result.Add(tItem.IsValueType() ? Activator.CreateInstance(tItem) : null);
                            promise.ValueAvailable += v => result[index] = TypeConverter.ChangeType(v, tItem);
                        }
                    }
                    else
                    {
                        throw new ForwardAnchorNotSupportedException(
                            current?.Start ?? Mark.Empty,
                            current?.End ?? Mark.Empty,
                            "Forward alias references are not allowed because this type does not implement IList<>"
                        );
                    }
                }
                else
                {
                    if (isPopulating == false)
                    {
                        result.Add(TypeConverter.ChangeType(value, tItem));
                    }
                }

                i++;
            }
        }
    }
}

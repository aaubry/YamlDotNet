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
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class StaticCollectionNodeDeserializer : INodeDeserializer
    {
        private readonly StaticObjectFactory factory;

        public StaticCollectionNodeDeserializer(StaticObjectFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
        {
            if (!factory.IsList(expectedType))
            {
                value = null;
                return false;
            }
            var list = (factory.Create(expectedType) as IList)!;
            value = list;

            DeserializeHelper(factory.GetValueType(expectedType), parser, nestedObjectDeserializer, list!, factory);

            return true;
        }

        internal static void DeserializeHelper(Type tItem, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IList result, IObjectFactory factory)
        {
            parser.Consume<SequenceStart>();
            while (!parser.TryConsume<SequenceEnd>(out var _))
            {
                var current = parser.Current;

                var value = nestedObjectDeserializer(parser, tItem);
                if (value is IValuePromise promise)
                {
                    var index = result.Add(factory.CreatePrimitive(tItem));
                    promise.ValueAvailable += v => result[index] = v;
                }
                else
                {
                    result.Add(value);
                }
            }
        }
    }
}

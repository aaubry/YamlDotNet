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
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public abstract class CollectionDeserializer
    {
        protected static void DeserializeHelper(Type tItem, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IList result, bool canUpdate, IObjectFactory objectFactory)
        {
            parser.Consume<SequenceStart>();
            while (!parser.TryConsume<SequenceEnd>(out var _))
            {
                var current = parser.Current;

                var value = nestedObjectDeserializer(parser, tItem);
                if (value is IValuePromise promise)
                {
                    if (canUpdate)
                    {
                        var index = result.Add(objectFactory.CreatePrimitive(tItem));
                        promise.ValueAvailable += v => result[index] = v;
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
                    result.Add(value);
                }
            }
        }
    }
}

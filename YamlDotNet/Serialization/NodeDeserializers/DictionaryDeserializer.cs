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
using System.Linq;
using YamlDotNet.Basic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public abstract class DictionaryDeserializer
    {
        private readonly bool duplicateKeyChecking;

        public DictionaryDeserializer(bool duplicateKeyChecking)
        {
            this.duplicateKeyChecking = duplicateKeyChecking;
        }

        private void TryAssign(IDictionary result, object key, object value, MappingStart propertyName)
        {
            if (duplicateKeyChecking && result.Contains(key))
            {
                throw new YamlException(propertyName.Start, propertyName.End, $"Encountered duplicate key {key}");
            }
            result[key] = value!;
        }

        protected virtual void Deserialize(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IDictionary result)
        {
            if (!parser.SkipComments)
            {
                DeserializeWithComments(tKey, tValue, parser, nestedObjectDeserializer, result);
                return;
            }

            var property = parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                var key = nestedObjectDeserializer(parser, tKey);
                var value = nestedObjectDeserializer(parser, tValue);
                var valuePromise = value as IValuePromise;
                AddKeyValue(result, property, key, value, valuePromise);
            }
        }

        private void AddKeyValue(IDictionary result, MappingStart property, object? key, object? value, IValuePromise? valuePromise)
        {
            if (key is IValuePromise keyPromise)
            {
                if (valuePromise == null)
                {
                    // Key is pending, value is known
                    keyPromise.ValueAvailable += v => result[v!] = value!;
                }
                else
                {
                    // Both key and value are pending. We need to wait until both of them become available.
                    var hasFirstPart = false;

                    keyPromise.ValueAvailable += v =>
                    {
                        if (hasFirstPart)
                        {
                            TryAssign(result, v!, value!, property);
                        }
                        else
                        {
                            key = v!;
                            hasFirstPart = true;
                        }
                    };

                    valuePromise.ValueAvailable += v =>
                    {
                        if (hasFirstPart)
                        {
                            TryAssign(result, key, v!, property);
                        }
                        else
                        {
                            value = v;
                            hasFirstPart = true;
                        }
                    };
                }
            }
            else
            {
                if (key == null)
                {
                    throw new ArgumentException("Empty key names are not supported yet.", "key");
                }

                if (valuePromise == null)
                {
                    // Happy path: both key and value are known
                    TryAssign(result, key, value!, property);
                }
                else
                {
                    // Key is known, value is pending
                    valuePromise.ValueAvailable += v => result[key!] = v!;
                }
            }
        }

        private void DeserializeWithComments(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IDictionary result)
        {
            parser.TryConsume<Comment>(out var fileStartComment);
            var property = parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                parser.SkipFollowingComments();
                var key = nestedObjectDeserializer(parser, tKey);
                parser.SkipFollowingComments();
                var originalValue = nestedObjectDeserializer(parser, tValue);
                var valueWithComment = ParseStringWithComment(parser, originalValue);
                var listValueWithComment = ParseListWithComment(valueWithComment);
                var valuePromise = listValueWithComment as IValuePromise;
                AddKeyValue(result, property, key, listValueWithComment, valuePromise);
            }
        }

        private static object? ParseListWithComment(object? value)
        {
            if (value is List<object> list)
            {
                var newValue = new List<ValueWithComment>();
                var stringValue = string.Empty;
                var comment = string.Empty;

                foreach (var listItem in list)
                {
                    if (listItem is string)
                    {
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            newValue.Add(new ValueWithComment(stringValue, comment));
                        }
                        stringValue = (string)listItem;
                        comment = string.Empty;
                    }
                    if (listItem is Comment && ((Comment)listItem).IsInline)
                    {
                        comment = ((Comment)listItem).Value;
                    }
                }
                if (!string.IsNullOrEmpty(stringValue))
                {
                    newValue.Add(new ValueWithComment(stringValue, comment));
                }
                if (newValue.Any())
                {
                    if (newValue.Any(v => !string.IsNullOrEmpty(v.Comment)))
                    {
                        value = newValue;
                    }
                    else
                    {
                        value = newValue.Select(v => v.Value).ToList();
                    }
                }
            }

            return value;
        }

        private static object? ParseStringWithComment(IParser parser, object? value)
        {
            if (value is string)
            {
                var comment = string.Empty;
                if (parser.TryConsume<Comment>(out var valueComment))
                {
                    if (value is string && valueComment.IsInline)
                    {
                        comment = valueComment.Value;
                    }
                    parser.SkipFollowingComments();
                }
                value = new ValueWithComment(value, comment);
            }
            return value;
        }
    }
}

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
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace YamlDotNet.Core.ParsingComments
{
    public abstract class DictionaryDeserializerWithComments : DictionaryDeserializer
    {
        public DictionaryDeserializerWithComments(bool duplicateKeyChecking) : base(duplicateKeyChecking)
        {
        }

        protected override void Deserialize(Type tKey, Type tValue, IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, IDictionary result)
        {
            parser.TryConsume<Comment>(out var _);
            var property = parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                ((IParserWithComments)parser).SkipFollowingComments();
                var key = nestedObjectDeserializer(parser, tKey);
                ((IParserWithComments)parser).SkipFollowingComments();
                var originalValue = nestedObjectDeserializer(parser, tValue);
                var valueWithComment = ParseStringWithComment((IParserWithComments)parser, originalValue);
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

        private static object? ParseStringWithComment(IParserWithComments parser, object? value)
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

// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class ObjectNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory _objectFactory;
        private readonly ITypeInspector _typeDescriptor;
        private readonly bool _ignoreUnmatched;

        public ObjectNodeDeserializer(IObjectFactory objectFactory, ITypeInspector typeDescriptor, bool ignoreUnmatched)
        {
            _objectFactory = objectFactory;
            _typeDescriptor = typeDescriptor;
            _ignoreUnmatched = ignoreUnmatched;
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
        {
            var mapping = parser.Allow<MappingStart>();
            if (mapping == null)
            {
                value = null;
                return false;
            }

            value = _objectFactory.Create(expectedType);
            while (!parser.Accept<MappingEnd>())
            {
                var propertyName = parser.Expect<Scalar>();
                var property = _typeDescriptor.GetProperty(expectedType, null, propertyName.Value, _ignoreUnmatched);
                if (property == null)
                {
                    parser.SkipThisAndNestedEvents();
                    continue;
                }

                var propertyValue = nestedObjectDeserializer(parser, property.Type);
                var propertyValuePromise = propertyValue as IValuePromise;
                if (propertyValuePromise == null)
                {
                    var convertedValue = TypeConverter.ChangeType(propertyValue, property.Type);
                    property.Write(value, convertedValue);
                }
                else
                {
                    var valueRef = value;
                    propertyValuePromise.ValueAvailable += v =>
                    {
                        var convertedValue = TypeConverter.ChangeType(v, property.Type);
                        property.Write(valueRef, convertedValue);
                    };
                }
            }

            parser.Expect<MappingEnd>();
            return true;
        }
    }
}

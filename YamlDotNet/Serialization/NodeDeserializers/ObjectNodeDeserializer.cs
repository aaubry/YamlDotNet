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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    public sealed class ObjectNodeDeserializer : INodeDeserializer
    {
        private readonly IObjectFactory objectFactory;
        private readonly ITypeInspector typeInspector;
        private readonly bool ignoreUnmatched;
        private readonly bool duplicateKeyChecking;
        private readonly ITypeConverter typeConverter;
        private readonly INamingConvention enumNamingConvention;
        private readonly bool enforceNullability;
        private readonly bool caseInsensitivePropertyMatching;
        private readonly bool enforceRequiredProperties;
        private readonly TypeConverterCache typeConverters;

        public ObjectNodeDeserializer(IObjectFactory objectFactory,
            ITypeInspector typeInspector,
            bool ignoreUnmatched,
            bool duplicateKeyChecking,
            ITypeConverter typeConverter,
            INamingConvention enumNamingConvention,
            bool enforceNullability,
            bool caseInsensitivePropertyMatching,
            bool enforceRequiredProperties,
            IEnumerable<IYamlTypeConverter> typeConverters)
        {
            this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            this.typeInspector = typeInspector ?? throw new ArgumentNullException(nameof(ObjectNodeDeserializer.typeInspector));
            this.ignoreUnmatched = ignoreUnmatched;
            this.duplicateKeyChecking = duplicateKeyChecking;
            this.typeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
            this.enumNamingConvention = enumNamingConvention ?? throw new ArgumentNullException(nameof(enumNamingConvention));
            this.enforceNullability = enforceNullability;
            this.caseInsensitivePropertyMatching = caseInsensitivePropertyMatching;
            this.enforceRequiredProperties = enforceRequiredProperties;
            this.typeConverters = new TypeConverterCache(typeConverters);
        }

        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
        {
            if (!parser.TryConsume<MappingStart>(out var mapping))
            {
                value = null;
                return false;
            }

            // Strip off the nullable & fsharp option type, if present. This is needed for nullable structs.
            var implementationType = Nullable.GetUnderlyingType(expectedType)
                ?? FsharpHelper.GetOptionUnderlyingType(expectedType)
                ?? expectedType;

            value = objectFactory.Create(implementationType);
            objectFactory.ExecuteOnDeserializing(value);

            var consumedProperties = new HashSet<string>(StringComparer.Ordinal);
            var consumedObjectProperties = new HashSet<string>(StringComparer.Ordinal);
            var mark = Mark.Empty;

            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                var propertyName = parser.Consume<Scalar>();
                if (duplicateKeyChecking && !consumedProperties.Add(propertyName.Value))
                {
                    throw new YamlException(propertyName.Start, propertyName.End, $"Encountered duplicate key {propertyName.Value}");
                }
                try
                {
                    var property = typeInspector.GetProperty(implementationType, null, propertyName.Value, ignoreUnmatched, caseInsensitivePropertyMatching);
                    if (property == null)
                    {
                        parser.SkipThisAndNestedEvents();
                        continue;
                    }
                    consumedObjectProperties.Add(property.Name);

                    object? propertyValue;
                    if (property.ConverterType != null)
                    {
                        var typeConverter = typeConverters.GetConverterByType(property.ConverterType);
                        propertyValue = typeConverter.ReadYaml(parser, property.Type, rootDeserializer);
                    }
                    else
                    {
                        propertyValue = nestedObjectDeserializer(parser, property.Type);
                    }

                    if (propertyValue is IValuePromise propertyValuePromise)
                    {
                        var valueRef = value;
                        propertyValuePromise.ValueAvailable += v =>
                        {
                            var convertedValue = typeConverter.ChangeType(v, property.Type, enumNamingConvention, typeInspector);

                            NullCheck(convertedValue, property, propertyName);

                            property.Write(valueRef, convertedValue);
                        };
                    }
                    else
                    {
                        var convertedValue = typeConverter.ChangeType(propertyValue, property.Type, enumNamingConvention, typeInspector);

                        NullCheck(convertedValue, property, propertyName);

                        property.Write(value, convertedValue);
                    }
                }
                catch (SerializationException ex)
                {
                    throw new YamlException(propertyName.Start, propertyName.End, ex.Message);
                }
                catch (YamlException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new YamlException(propertyName.Start, propertyName.End, "Exception during deserialization", ex);
                }
                mark = propertyName.End;
            }

            if (enforceRequiredProperties)
            {
                //TODO: Get properties marked as required on the object
                //TODO: Compare those properties agains the consumedObjectProperties, throw if any are missing.
                var properties = typeInspector.GetProperties(implementationType, value);
                var missingPropertyNames = new List<string>();
                foreach (var property in properties)
                {
                    if (property.Required && !consumedObjectProperties.Contains(property.Name))
                    {
                        missingPropertyNames.Add(property.Name);
                    }
                }

                if (missingPropertyNames.Count > 0)
                {
                    var propertyNames = string.Join(",", missingPropertyNames);
                    throw new YamlException(mark, mark, $"Missing properties, '{propertyNames}' in source yaml.");
                }
            }

            objectFactory.ExecuteOnDeserialized(value);
            return true;
        }

        public void NullCheck(object? value, IPropertyDescriptor property, Scalar propertyName)
        {
            if (enforceNullability &&
                value == null &&
                !property.AllowNulls)
            {
#pragma warning disable CA2201 // Do not raise reserved exception types
                throw new YamlException(propertyName.Start, propertyName.End, "Strict nullability enforcement error.", new NullReferenceException("Yaml value is null when target property requires non null values."));
#pragma warning restore CA2201
            }
        }
    }
}

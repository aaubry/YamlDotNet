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
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
    /// Construct objects when a single constructor is annotated with <see cref="YamlConstructorAttribute"/>.
    public sealed class AnnotatedConstructorObjectNodeDeserializer : INodeDeserializer
    {
        private readonly bool ignoreUnmatched;

        public AnnotatedConstructorObjectNodeDeserializer(bool ignoreUnmatched)
        {
            this.ignoreUnmatched = ignoreUnmatched;
        }

        bool INodeDeserializer.Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            // Strip off the nullable type, if present. This is needed for nullable structs.
            var implementationType = Nullable.GetUnderlyingType(expectedType) ?? expectedType;

            // Use reflection to look for annotated constructor to determine whether should continue
            if (!implementationType.TryGetFirstConstructorWithAttribute<YamlConstructorAttribute>(out var constructorInfo, out _))
            {
                value = null;
                return false;
            }

            if (!parser.TryConsume<MappingStart>(out _))
            {
                value = null;
                return false;
            }

            ParameterInfo[] parameters = constructorInfo.GetParameters();
            var parametersMap = ParseYamlIntoDictionary(parser, nestedObjectDeserializer, implementationType, parameters);
            var parametersList = LookUpParametersInDictionary(parameters, parametersMap, implementationType);

            // Actually construct while catching and describing any problems
            try
            {
                value = constructorInfo.Invoke(parametersList.ToArray());
            }
            catch (Exception ex)
            {
                throw new YamlException($"Exception while deserializing {implementationType.FullName} via constructor {constructorInfo} with [{string.Join(", ", parametersList.Select(p => p?.ToString()).ToArray())}]", ex);
            }
            return true;
        }

        /// Parse the YAML using the constructor's parameters to determine expected type based upon name-matching.
        private Dictionary<string, object?> ParseYamlIntoDictionary(IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer, Type implementationType, ParameterInfo[] parameters)
        {
            var parametersMap = new Dictionary<string, object?>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var propertyName = parser.Consume<Scalar>();
                try
                {
                    var parameterInfo = GetParameterInfoByParameterName(propertyName.Value, parameters);
                    if (parameterInfo == null)
                    {
                        if (!ignoreUnmatched)
                        {
                            throw new YamlException(propertyName.Start, propertyName.End, $"Constructor for type '{implementationType.FullName}' does not use {propertyName.Value}");
                        }

                        parser.SkipThisAndNestedEvents();
                        continue;
                    }

                    var propertyValue = nestedObjectDeserializer(parser, parameterInfo.ParameterType);
                    if (propertyValue is IValuePromise propertyValuePromise)
                    {
                        propertyValuePromise.ValueAvailable += v =>
                        {
                            var convertedValue = TypeConverter.ChangeType(v, parameterInfo.ParameterType);
                            parametersMap[propertyName.Value] = convertedValue;
                        };
                    }
                    else
                    {
                        var convertedValue = TypeConverter.ChangeType(propertyValue, parameterInfo.ParameterType);
                        parametersMap[propertyName.Value] = convertedValue;
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
            }

            return parametersMap;
        }

        private static ParameterInfo? GetParameterInfoByParameterName(string paramName, ParameterInfo[] parameters)
        {
            return parameters.FirstOrDefault(p => p.Name == paramName);
        }

        private List<object?> LookUpParametersInDictionary(ParameterInfo[] parameters, Dictionary<string, object?> parametersMap, Type implementationType)
        {
            var parametersList = new List<object?>();
            foreach (var p in parameters)
            {
                if (p.Name == null)
                {
                    continue;
                }

                // Only collect parameters we have
                if (parametersMap.TryGetValue(p.Name, out var paramValue))
                {
                    parametersList.Add(paramValue);
                }
                else if (p.IsOptional)
                {
                    // Optional parameters should supply `Missing` to use default values if available
                    // Per https://stackoverflow.com/a/9916197/814523
                    parametersList.Add(Type.Missing);
                }
                else if (!ignoreUnmatched)
                {
                    throw new YamlException($"Value for parameter '{p.Name}' not found in deserialized data for type '{implementationType.FullName}'.");
                }
                // else try ignoring and see what happens.
            }

            return parametersList;
        }

    }
}

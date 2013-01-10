//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry

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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.RepresentationModel
{
    public static class YamlDoc
    {
        public static YamlNode LoadFromFile(string fileName)
        {
            return LoadFromTextReader(File.OpenText(fileName));
        }

        public static YamlNode LoadFromString(string yamlText)
        {
            return LoadFromTextReader(new StringReader(yamlText));
        }

        public static YamlNode LoadFromTextReader(TextReader reader)
        {
            var yaml = new YamlStream();
            yaml.Load(reader);

            return yaml.Documents.First().RootNode;
        }

        private static object MapValue(object value)
        {
            object result;
            TryMapValue(value, out result);
            return result;
        }

        internal static bool TryMapValue(object value, out object result)
        {
            if (value is YamlNode)
            {
                result = new DynamicYaml((YamlNode)value);
                return true;
            }

            result = null;
            return false;
        }
    }

    public class DynamicYaml : DynamicObject
    {
        private static readonly Type[] ConvertableBasicTypes = new []
            {
                typeof(DynamicYaml),
                typeof(object),
                typeof(string),
                typeof(char),
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };


        private static readonly Type[] ConvertableGenericCollectionTypes = new[] 
            {
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(IList<>),
                typeof(List<>)
            };

        private static readonly Type[] ConvertableGenericDictionaryTypes = new []
            {
                typeof(IDictionary<,>),
                typeof(Dictionary<,>)
            };

        private static readonly Type[] ConvertableCollectionTypes = ConvertableGenericCollectionTypes.
                SelectMany(type => ConvertableBasicTypes.
                    Select(basicType => type.MakeGenericType(basicType)) 
                ).ToArray();

        private static readonly Type[] ConvertableDictionaryTypes = ConvertableGenericDictionaryTypes.
            SelectMany(type => ConvertableBasicTypes.
                    SelectMany(valueType => ConvertableBasicTypes.
                        Select(keyType => type.MakeGenericType(keyType, valueType)
                ))).ToArray();

        private static readonly Type[] ConvertableArrayTypes = ConvertableBasicTypes.Select(
                                        type => type.MakeArrayType()).ToArray();

        private YamlMappingNode mappingNode;
        private YamlSequenceNode sequenceNode;
        private YamlScalarNode scalarNode;
        private YamlNode yamlNode;

        public DynamicYaml(YamlNode node)
        {
            Reload(node);
        }

        public DynamicYaml(TextReader reader)
            : this(YamlDoc.LoadFromTextReader(reader))
        {
        }

        public DynamicYaml(string yaml)
            : this(YamlDoc.LoadFromString(yaml))
        {
        }

        public void Reload(YamlNode yamlNode)
        {
            this.yamlNode = yamlNode;
            mappingNode = yamlNode as YamlMappingNode;
            sequenceNode = yamlNode as YamlSequenceNode;
            scalarNode = yamlNode as YamlScalarNode;
            children = null;
        }

        public void Reload(TextReader reader)
        {
            Reload(YamlDoc.LoadFromTextReader(reader));
        }

        public void Reload(string yaml)
        {
            Reload(YamlDoc.LoadFromString(yaml));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValueByKeyAndType(binder.Name, binder.ReturnType, out result);
        }

        private static bool FailToGetValue(out object result)
        {
            result = null;
            return false;
        }

        private static bool SuccessfullyGetValue(out object result, object value)
        {
            result = value;
            return true;
        }

        private bool TryGetValueByKeyAndType(string key, 
            Type type, 
            out object result)
        {
            if (mappingNode == null)
            {
                return FailToGetValue(out result);
            }
            var yamlKey = new YamlScalarNode(key.Decapitalize());
            var yamlKey2 = new YamlScalarNode(key.Capitalize());
            return TryGetValueByYamlKeyAndType(yamlKey, type, out result) ||
                TryGetValueByYamlKeyAndType(yamlKey2, type, out result);
        }

        private bool TryGetValueByYamlKeyAndType(YamlScalarNode yamlKey, Type type, out object result)
        {
            if (mappingNode.Children.ContainsKey(yamlKey))
            {
                var value = mappingNode.Children[yamlKey];
                if (YamlDoc.TryMapValue(value, out result))
                {
                    return true;
                }
            }

            if (IsNullableType(type)) 
            {
                return SuccessfullyGetValue(out result, new DynamicYaml((YamlNode)null));
            }
            else
            {
                return FailToGetValue(out result);
            }
        }

        private static bool IsNullableType(Type type)
        {
            return type != null && (!type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        }

        private bool TryGetValueByIndex(int index, out object result)
        {
            if (sequenceNode == null)
            {
                return FailToGetValue(out result);
            }

            if (index >= sequenceNode.Count())
            {
                throw new IndexOutOfRangeException();
            }

            return YamlDoc.TryMapValue(sequenceNode.ToArray()[index], out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            var stringKey = indices[0] as string;
            if (stringKey != null)
            {
                if (TryGetValueByKeyAndType(stringKey, binder.ReturnType, out result))
                {
                    if (indices.Length > 1)
                    {
                        return TryGetIndex(binder, indices.Skip(1).ToArray(), out result);
                    }
                    return true;
                }

                return FailToGetValue(out result);
            }

            var intKey = indices[0] as int?;
            if (intKey != null)
            {
                if (TryGetValueByIndex(intKey.Value, out result))
                {
                    if (indices.Length > 1)
                    {
                        if (result is DynamicYaml)
                        {
                            return ((DynamicYaml)result).TryGetIndex(binder, indices.Skip(1).ToArray(), out result);
                        }
                        else
                        {
                            return FailToGetValue(out result);
                        }
                    }

                    return true;
                }

                return FailToGetValue(out result);
            }

            return base.TryGetIndex(binder, indices, out result);
        }

        private bool TryConvertToBasicType(Type type, bool isNullable, out object result)
        {
            if (type == typeof(object) || type == typeof(DynamicYaml))
            {
                return SuccessfullyGetValue(out result, this);
            }
            if (scalarNode == null)
            {
                if (isNullable)
                {
                    return SuccessfullyGetValue(out result, null);
                }
                return FailToGetValue(out result);
            }
            if (type == typeof(string))
            {
                return SuccessfullyGetValue(out result, scalarNode.Value);
            }
            if (type == typeof(char))
            {
                char charResult;
                bool success = char.TryParse(scalarNode.Value, out charResult);
                result = success ? (object)charResult : null;
                return success;
            }
            if (type == typeof(int))
            {
                int intResult;
                bool success = int.TryParse(scalarNode.Value, out intResult);
                result = success ? (object)intResult : null;
                return success;
            }
            if (type == typeof(long))
            {
                long longResult;
                bool success = long.TryParse(scalarNode.Value, out longResult);
                result = success ? (object)longResult : null;
                return success;
            }
            if (type == typeof(float))
            {
                float floatResult;
                bool success = float.TryParse(scalarNode.Value, out floatResult);
                result = success ? (object)floatResult : null;
                return success;
            }
            if (type == typeof(double))
            {
                double doubleResult;
                bool success = double.TryParse(scalarNode.Value, out doubleResult);
                result = success ? (object)doubleResult : null;
                return success;
            }
            if (type == typeof(decimal))
            {
                decimal decimalResult;
                bool success = decimal.TryParse(scalarNode.Value, out decimalResult);
                result = success ? (object)decimalResult : null;
                return success;
            }
            if (type.IsEnum)
            {
                long longResult;
                if (long.TryParse(scalarNode.Value, out longResult))
                {
                    result = longResult;
                    return true;
                }

                try
                {
                    result = Enum.Parse(type, scalarNode.Value);
                    return true;
                }
                catch
                {
                    return FailToGetValue(out result);
                }
            }

            return FailToGetValue(out result);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var type = binder.ReturnType;

            return TryConvertToType(type, out result);
        }

        private bool IsGenericEnumCollection(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type[] genericTypeArgs = type.GetGenericArguments();
            if(genericTypeArgs.Length != 1)
            {
                return false;
            }

            var elementType = genericTypeArgs.First();

            return elementType.IsEnum && ConvertableGenericCollectionTypes.Any(
                genericType => genericType.MakeGenericType(elementType) == type);
        }

        private bool IsLegalElementType(Type type)
        {
            return type.IsEnum || ConvertableBasicTypes.Contains(type);
        }

        private bool IsGenericEnumDictionary(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type[] genericTypeArgs = type.GetGenericArguments();
            if (genericTypeArgs.Length != 2)
            {
                return false;
            }
            Type keyType = genericTypeArgs[0], valueType = genericTypeArgs[1];
            return (keyType.IsEnum || valueType.IsEnum) && 
                ConvertableGenericDictionaryTypes.
                Any(genericType => genericType.MakeGenericType(keyType, valueType) == type) &&
                IsLegalElementType(keyType) && IsLegalElementType(valueType);
        }

        private bool TryConvertToType(Type type, out object result)
        {
            if (type.IsArray && 
                (ConvertableArrayTypes.Contains(type) || 
                 type.GetElementType().IsSubclassOf(typeof(Enum)) )
                )
            {
                return TryConvertToArray(type, out result);
            }
            if (ConvertableCollectionTypes.Contains(type) ||
                IsGenericEnumCollection(type))
            {
                return TryConvertToCollection(type, out result);
            }
            if (ConvertableDictionaryTypes.Contains(type) ||
                IsGenericEnumDictionary(type))
            {
                return TryConvertToDictionary(type, out result);
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            return TryConvertToBasicType(type, IsNullableType(type), out result);
        }

        private bool TryConvertToDictionary(Type type, out object result)
        {
            if (mappingNode == null)
            {
                return FailToGetValue(out result);
            }

            Type[] genericTypeArgs = type.GetGenericArguments();
            Type keyType = genericTypeArgs[0],
                 valueType = genericTypeArgs[1];

            Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dict = Activator.CreateInstance(dictType) as IDictionary;
            foreach (KeyValuePair<YamlNode, YamlNode> pair in mappingNode.Children)
            {
                object key;
                if (!new DynamicYaml(pair.Key).TryConvertToType(keyType, out key))
                {
                    return FailToGetValue(out result);
                }

                object value;
                if (!new DynamicYaml(pair.Value).TryConvertToType(valueType, out value))
                {
                    return FailToGetValue(out result);
                }

                dict.Add(key, value);
            }

            return SuccessfullyGetValue(out result, dict);
        }

        private bool TryConvertToCollection(Type type, out object result)
        {
            var elementType = type.GetGenericArguments().First();
            Type listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType) as IList;

            foreach (DynamicYaml child in Children)
            {
                object result2;
                if (!child.TryConvertToType(elementType, out result2))
                {
                    return FailToGetValue(out result);
                }

                list.Add(result2);
            }

            return SuccessfullyGetValue(out result, list);
        }

        private bool TryConvertToArray(Type type, out object result)
        {
            if (Children == null)
            {
                return FailToGetValue(out result);
            }
            var elementType = type.GetElementType();
            Array arrayResult = Array.CreateInstance(elementType, Children.Count);
            int index = 0;
            foreach (var child in Children)
            {
                object result2;
                if (!child.TryConvertToType(elementType, out result2))
                {
                    return FailToGetValue(out result);
                }
                arrayResult.SetValue(result2, index);
                index++;
            }

            return SuccessfullyGetValue(out result, arrayResult);
        }

        private IList<DynamicYaml> GetChilren()
        {
            if (mappingNode != null)
            {
                return mappingNode.Children.Values.Select(node => new DynamicYaml(node)).ToList();
            }

            if (sequenceNode != null)
            {
                return sequenceNode.Select(node => new DynamicYaml(node)).ToList();
            }

            return new List<DynamicYaml>();
        }

        private IList<DynamicYaml> children;
        public IList<DynamicYaml> Children
        {
            get
            {
                if (children == null)
                {
                    children = GetChilren();
                }

                return children;
            }
        }

        public int Count
        {
            get
            {
                return Children != null ? Children.Count : 0;
            }
        }
    }
}

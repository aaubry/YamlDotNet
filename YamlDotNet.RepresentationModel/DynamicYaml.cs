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
            if (value is YamlScalarNode)
            {
                result = ((YamlScalarNode)value).Value;
                return true;
            }


            if (value is YamlMappingNode ||
                value is YamlSequenceNode)
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
        private YamlMappingNode mappingNode;
        private YamlSequenceNode sequenceNode;

        public DynamicYaml(YamlNode node)
        {
            this.mappingNode = node as YamlMappingNode;
            this.sequenceNode = node as YamlSequenceNode;
        }

        public DynamicYaml(TextReader reader)
            : this(YamlDoc.LoadFromTextReader(reader))
        {
        }

        public DynamicYaml(string yaml)
            : this(YamlDoc.LoadFromString(yaml))
        {
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValueByKey(binder.Name, out result);
        }

        private static bool FailToGetValue(out object result)
        {
            result = null;
            return false;
        }

        private bool TryGetValueByKey(string key, out object result)
        {
            if (mappingNode == null)
            {
                return FailToGetValue(out result);
            }
            var yamlKey = new YamlScalarNode(key.Decapitalize());
            var yamlKey2 = new YamlScalarNode(key.Capitalize());
            return TryGetValueByYamlKey(yamlKey, out result) ||
                TryGetValueByYamlKey(yamlKey2, out result);
        }

        private bool TryGetValueByYamlKey(YamlScalarNode yamlKey, out object result)
        {
            if (mappingNode.Children.ContainsKey(yamlKey))
            {
                var value = mappingNode.Children[yamlKey];
                if (YamlDoc.TryMapValue(value, out result))
                {
                    return true;
                }
            }

            return FailToGetValue(out result);
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
                if (TryGetValueByKey(stringKey, out result))
                {
                    if (indices.Length > 1)
                    {
                        return TryGetIndex(binder, indices.Skip(1).ToArray(), out result);
                    }
                    return true;
                }
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
            }

            return base.TryGetIndex(binder, indices, out result);
        }

        public IEnumerable<YamlNode> ChildNodes
        {
            get
            {
                if (mappingNode != null)
                {
                    return mappingNode.Children.Values;
                }

                return sequenceNode;
            }
        }

        public int Count
        {
            get
            {
                return ChildNodes != null ? ChildNodes.Count() : 0;
            }
        }
    }
}

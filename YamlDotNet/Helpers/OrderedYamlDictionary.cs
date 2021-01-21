//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

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
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Helpers
{
    [Serializable]
    internal class OrderedYamlDictionary : IDictionary<YamlNode, YamlNode>
    {
        private readonly Dictionary<YamlNode,YamlNode> dic = new Dictionary<YamlNode, YamlNode>();

        private readonly List<KeyValuePair<YamlNode, YamlNode>> valuePairs = new List<KeyValuePair<YamlNode, YamlNode>>();

        public YamlNode this[YamlNode key]
        {
            get
            {
                return dic[key];
            }
            set
            {
                dic[key] = value;
            }
        }

        public void Add(KeyValuePair<YamlNode, YamlNode> item)
        {
            dic.Add(item.Key, item.Value);
            valuePairs.Add(item);
        }

        public void Add(YamlNode key, YamlNode value)
        {
            dic.Add(key, value);
            valuePairs.Add(new KeyValuePair<YamlNode, YamlNode>(key,value));
        }

        public void Clear()
        {
            dic.Clear();
            valuePairs.Clear();
        }


        public void CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex)
        {
            int modifier = 0;
            foreach(KeyValuePair<YamlNode,YamlNode> obj in dic)
            {
                valuePairs[arrayIndex + modifier] = obj;
                array.SetValue(new DictionaryEntry(obj.Key, obj.Value), arrayIndex + modifier);
                modifier++;
            }

        }


        public int Count
        {
            get
            {
                return dic.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Contains(YamlNode key)
        {
            return dic.ContainsKey(key);
        }

        public bool ContainsKey(YamlNode key)
        {
            return dic.ContainsKey(key);
        }

        public bool Remove(YamlNode key)
        {
            dic.Remove(key);
            valuePairs.RemoveAll(item => item.Key.Equals(key));
            return true;
        }

        public bool TryGetValue(YamlNode key, out YamlNode value)
        {
            if(dic.ContainsKey(key))
            {
                value = dic[key];
                return true;
            }
            value = default(YamlNode);
            return false;
        }

        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Contains(KeyValuePair<YamlNode, YamlNode> item)
        {
            return dic.ContainsKey(item.Key) && dic.ContainsValue(item.Value);
        }

        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Remove(KeyValuePair<YamlNode, YamlNode> item)
        {
            if (dic.ContainsKey(item.Key))
            {
                dic.Remove(item.Key);
                valuePairs.Remove(item);
                return true;
            }
            return false;

        }

        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            foreach (KeyValuePair<YamlNode,YamlNode> entry in dic)
            {
                yield return new KeyValuePair<YamlNode, YamlNode>(entry.Key, entry.Value);
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return valuePairs.GetEnumerator();
        }

        ICollection<YamlNode> IDictionary<YamlNode,YamlNode>.Keys
        {
            get
            {
                List<YamlNode> keys = new List<YamlNode>();
                foreach(YamlNode obj in dic.Keys)
                {
                    keys.Add(obj);
                }
                return keys;
            }
        }

        ICollection<YamlNode> IDictionary<YamlNode, YamlNode>.Values
        {
            get
            {
                List<YamlNode> values = new List<YamlNode>();
                foreach (YamlNode obj in dic.Values)
                {
                    values.Add(obj);
                }
                return values;
            }
        }


        public OrderedYamlDictionary()
        {

        }


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Helpers
{
    [Serializable]
    public class OrderedYamlDictionary : IDictionary<YamlNode, YamlNode>
    {
        private readonly OrderedDictionary dic = new OrderedDictionary();

        public YamlNode this[YamlNode key] { get { return (YamlNode)dic[key]; } set { dic[key] = value; } }

        public void Add(KeyValuePair<YamlNode, YamlNode> item)
        {
            dic.Add(item.Key, item.Value);
        }
        public void Add(YamlNode key, YamlNode value)
        {
            dic.Add(key, value);
        }

        public void Clear() { dic.Clear(); }


        public void CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex) { }


        public int Count { get { return dic.Count; } }
        public bool IsReadOnly { get { return false; } }

        public bool Contains(YamlNode key) { return dic.Contains(key); }
        public bool ContainsKey(YamlNode key) { return dic.Contains(key); }

        public bool Remove(YamlNode key) { dic.Remove(key); return true; }

        public bool TryGetValue(YamlNode key, out YamlNode value) { value = default(YamlNode); return false; }

        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Contains(KeyValuePair<YamlNode, YamlNode> item)
        {
            throw new NotImplementedException();
        }
        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Remove(KeyValuePair<YamlNode, YamlNode> item) { return false; }

        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in dic)
                yield return new KeyValuePair<YamlNode, YamlNode>((YamlNode)entry.Key, (YamlNode)entry.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly YamlNode[] keys = new YamlNode[0];
        private static readonly YamlNode[] values = new YamlNode[0];

        ICollection<YamlNode> IDictionary<YamlNode,YamlNode>.Keys { get { return keys; } }
        ICollection<YamlNode> IDictionary<YamlNode, YamlNode>.Values { get { return values; } }
    }
}

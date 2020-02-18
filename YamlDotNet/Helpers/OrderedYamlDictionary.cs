using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Helpers
{
    [Serializable]
    public class OrderedYamlDictionary : IDictionary<YamlNode, YamlNode>
    {
        private readonly OrderedDictionary dic = new OrderedDictionary();

        public YamlNode this[YamlNode key]
        {
            get
            {
                return (YamlNode)dic[key];
            }
            set
            {
                dic[key] = value;
            }
        }

        public void Add(KeyValuePair<YamlNode, YamlNode> item)
        {
            dic.Add(item.Key, item.Value);
        }

        public void Add(YamlNode key, YamlNode value)
        {
            dic.Add(key, value);
        }

        public void Clear()
        {
            dic.Clear();
        }


        public void CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex)
        {
            int modifier = 0;
            foreach(DictionaryEntry obj in dic)
            {
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
            return dic.Contains(key);
        }

        public bool ContainsKey(YamlNode key)
        {
            return dic.Contains(key);
        }

        public bool Remove(YamlNode key)
        {
            dic.Remove(key);
            return true;
        }

        public bool TryGetValue(YamlNode key, out YamlNode value)
        {
            if(dic.Contains(key))
            {
                value = dic[key] as YamlNode;
                return true;
            }
            value = default(YamlNode);
            return false;
        }

        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Contains(KeyValuePair<YamlNode, YamlNode> item)
        {
            return dic.Contains(item);
        }

        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Remove(KeyValuePair<YamlNode, YamlNode> item)
        {
            if (dic.Contains(item.Key))
            {
                dic.Remove(item.Key);
                return true;
            }
            return false;

        }

        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in dic)
            {
                yield return new KeyValuePair<YamlNode, YamlNode>((YamlNode)entry.Key, (YamlNode)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dic.GetObjectData(info, context);
        }

        ICollection<YamlNode> IDictionary<YamlNode,YamlNode>.Keys
        {
            get
            {
                List<YamlNode> keys = new List<YamlNode>();
                foreach(object obj in dic.Keys)
                {
                    keys.Add(obj as YamlNode);
                }
                return keys;
            }
        }

        ICollection<YamlNode> IDictionary<YamlNode, YamlNode>.Values
        {
            get
            {
                List<YamlNode> values = new List<YamlNode>();
                foreach (Object obj in dic.Values)
                {
                    values.Add(obj as YamlNode);
                }
                return values;
            }
        }


        public OrderedYamlDictionary()
        {

        }


    }
}

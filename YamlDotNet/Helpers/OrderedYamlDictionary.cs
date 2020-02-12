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
    public class OrderedYamlDictionary : OrderedDictionary, IDictionary<YamlNode, YamlNode>
    {
        public YamlNode this[YamlNode key]
        {
            get => base[key] as YamlNode;
            set => base[key] = value;
        }

        ICollection<YamlNode> IDictionary<YamlNode, YamlNode>.Keys => throw new NotImplementedException();

        ICollection<YamlNode> IDictionary<YamlNode, YamlNode>.Values => throw new NotImplementedException();

        public void Add(YamlNode key, YamlNode value)
        {
            base.Add(key, value);
        }

        public void Add(KeyValuePair<YamlNode, YamlNode> item)
        {
            base.Add(item.Key,item.Value);
        }

        public bool Contains(KeyValuePair<YamlNode, YamlNode> item)
        {
            return base.Contains(item.Key) && base[item.Key] == item.Value;
        }

        public bool ContainsKey(YamlNode key)
        {
            return base.Contains(key);
        }

        public void CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public bool Remove(YamlNode key)
        {
            if (base.Contains(key))
            {
                base.Remove(key);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool Remove(KeyValuePair<YamlNode, YamlNode> item)
        {
            if (base.Contains(item.Key))
            {
                base.Remove(item.Key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(YamlNode key, [MaybeNullWhen(false)] out YamlNode value)
        {

            if (base.Contains(key))
            {
                value = (YamlNode)base[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator<KeyValuePair<YamlNode, YamlNode>> IEnumerable<KeyValuePair<YamlNode, YamlNode>>.GetEnumerator()
        {
            foreach(DictionaryEntry entry in this)
            {
                yield return new KeyValuePair<YamlNode, YamlNode>((YamlNode)entry.Key, (YamlNode)entry.Value);
            }
        }

        protected OrderedYamlDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {

        }

        public OrderedYamlDictionary() : base()
        {
        }
    }
}

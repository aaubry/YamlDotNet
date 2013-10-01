using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public class AnchorAssigningSerializationBehavior : ISerializationBehavior
	{
		private class ObjectInfo
		{
			public string anchor;
			public bool serialized;
		}

		private readonly Dictionary<object, ObjectInfo> anchors = new Dictionary<object, ObjectInfo>();

		void ISerializationBehavior.SerializationStarting(Type type, object o)
		{
			//int nextId = 0;
			//LoadAliases(type, o, ref nextId);
		}


		//private void LoadAliases(Type type, object o, ref int nextId)
		//{
		//    if (type.IsValueType)
		//    {
		//        return;
		//    }

		//    if (anchors.ContainsKey(o))
		//    {
		//        if (anchors[o] == null)
		//        {
		//            anchors[o] = new ObjectInfo
		//            {
		//                anchor = string.Format(CultureInfo.InvariantCulture, "o{0}", nextId++)
		//            };
		//        }
		//    }
		//    else
		//    {
		//        anchors.Add(o, null);

		//        if (typeof(IDictionary).IsAssignableFrom(type))
		//        {
		//            LoadDictionaryAliases(type, (IDictionary)o, ref nextId);
		//            return;
		//        }

		//        Type iDictionaryType = ReflectionUtility.GetImplementedGenericInterface(type, typeof(IDictionary<,>));
		//        if (iDictionaryType != null)
		//        {
		//            LoadGenericDictionaryAliases(type, iDictionaryType, o, ref nextId);
		//            return;
		//        }

		//        if (typeof(IEnumerable).IsAssignableFrom(type))
		//        {
		//            LoadListAliases(type, (IEnumerable)o, ref nextId);
		//            return;
		//        }

		//        LoadObjectAliases(type, o, ref nextId);
		//    }
		//}

		//private void LoadObjectAliases(Type type, object o, ref int nextId)
		//{
		//    foreach (var property in ReflectionUtility.GetProperties(type))
		//    {
		//        object value = property.GetValue(o, null);
		//        if (value != null && value.GetType().IsClass && !(value is string))
		//        {
		//            LoadAliases(property.PropertyType, value, ref nextId);
		//        }
		//    }
		//}

		//private void LoadListAliases(Type type, IEnumerable list, ref int nextId)
		//{
		//    foreach (var item in list)
		//    {
		//        if (item != null)
		//        {
		//            LoadAliases(item.GetType(), item, ref nextId);
		//        }
		//    }
		//}

		//private void LoadGenericDictionaryAliases(Type type, Type iDictionaryType, object o, ref int nextId)
		//{
		//    foreach (var item in (IEnumerable)o)
		//    {
		//        LoadObjectAliases(item.GetType(), item, ref nextId);
		//    }
		//}

		//private void LoadDictionaryAliases(Type type, IDictionary dictionary, ref int nextId)
		//{
		//    foreach (DictionaryEntry item in dictionary)
		//    {
		//        LoadAliases(item.Key.GetType(), item.Key, ref nextId);
		//        if (item.Value != null)
		//        {
		//            LoadAliases(item.Value.GetType(), item.Value, ref nextId);
		//        }
		//    }
		//}
	}
}

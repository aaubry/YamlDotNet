//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Options that control the deserialization process.
	/// </summary>
	[Flags]
	public enum DeserializationFlags
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		None = 0,

		///// <summary>
		///// Ensures that it will be possible to deserialize the serialized objects.
		///// </summary>
		//Roundtrip = 1,

		///// <summary>
		///// If this flag is specified, if the same object appears more than once in the
		///// serialization graph, it will be serialized each time instead of just once.
		///// </summary>
		///// <remarks>
		///// If the serialization graph contains circular references and this flag is set,
		///// a <see cref="StackOverflowException" /> will be thrown.
		///// If this flag is not set, there is a performance penalty because the entire
		///// object graph must be walked twice.
		///// </remarks>
		//DisableAliases = 2,

		///// <summary>
		///// Forces every value to be serialized, even if it is the default value for that type.
		///// </summary>
		//EmitDefaults = 4,

		/// <summary>
		/// Ensures that the result of the serialization is valid JSON.
		/// </summary>
		JsonCompatible = 8,
	}

	/// <summary>
	/// Reads objects from YAML.
	/// </summary>
	public class Deserializer
	{
		private readonly IEnumerable<INodeDeserializer> _deserializers;
		private readonly IEnumerable<INodeTypeResolver> _typeResolvers;

		public Deserializer()
		{
			// TODO: Allow to override the object factory
			var objectFactory = new DefaultObjectFactory();

			_deserializers = new INodeDeserializer[]
			{
				new NullNodeDeserializer(),
				new ScalarNodeDeserializer(),
				new GenericDictionaryDeserializer(objectFactory),
				new NonGenericDictionaryDeserializer(objectFactory),
				new GenericCollectionDeserializer(objectFactory),
				new NonGenericListDeserializer(objectFactory),
				new EnumerableDeserializer(),
				new ObjectNodeDeserializer(objectFactory),
			};

			_typeResolvers = new INodeTypeResolver[]
			{
				new PredefinedTagsNodeTypeResolver(),
				new TypeNameInTagNodeTypeResolver(),
				new DefaultContainersNodeTypeResolver(),
			};
		}

		public object Deserialize(TextReader input, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(input, typeof(object), options);
		}

		public object Deserialize(TextReader input, Type type, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(new EventReader(new Parser(input)), type, options);
		}

		public object Deserialize(EventReader reader, DeserializationFlags options = DeserializationFlags.None)
		{
			return Deserialize(reader, typeof(object), options);
		}

		/// <summary>
		/// Deserializes an object of the specified type.
		/// </summary>
		/// <param name="reader">The <see cref="EventReader" /> where to deserialize the object.</param>
		/// <param name="type">The static type of the object to deserialize.</param>
		/// <param name="options">Options that control how the deserialization is to be performed.</param>
		/// <returns>Returns the deserialized object.</returns>
		public object Deserialize(EventReader reader, Type type, DeserializationFlags options = DeserializationFlags.None)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			var hasStreamStart = reader.Allow<StreamStart>() != null;

			var hasDocumentStart = reader.Allow<DocumentStart>() != null;

			object result = DeserializeValue(reader, type, null);

			if (hasDocumentStart)
			{
				reader.Expect<DocumentEnd>();
			}

			if (hasStreamStart)
			{
				reader.Expect<StreamEnd>();
			}

			return result;
		}

		private object DeserializeValue(EventReader reader, Type expectedType, object context)
		{
			var nodeEvent = reader.Peek<NodeEvent>();

			var nodeType = GetTypeFromEvent(nodeEvent, expectedType);

			foreach (var deserializer in _deserializers)
			{
				object value;
				if (deserializer.Deserialize(reader, nodeType, (r, t) => DeserializeValue(r, t, context), out value))
				{
					return value;
				}
			}

			throw new Exception("TODO");

			//return deserializer.Deserialize(reader, expectedType);

			//if (reader.Accept<AnchorAlias>())
			//{
			//	throw new NotImplementedException();
			//	//return context.Anchors[reader.Expect<AnchorAlias>().Value];
			//}

			//var nodeEvent = (NodeEvent)reader.Parser.Current;

			//if (IsNull(nodeEvent))
			//{
			//	reader.Expect<NodeEvent>();
			//	AddAnchoredObject(nodeEvent, null, context.Anchors);
			//	return null;
			//}

			//object result = DeserializeValueNotNull(reader, context, nodeEvent, expectedType);
			//return ObjectConverter.Convert(result, expectedType);
		}

		//private bool IsNull(NodeEvent nodeEvent)
		//{
		//	if (nodeEvent.Tag == "tag:yaml.org,2002:null")
		//	{
		//		return true;
		//	}

		//	if (JsonCompatible)
		//	{
		//		var scalar = nodeEvent as Scalar;
		//		if (scalar != null && scalar.Style == Core.ScalarStyle.Plain && scalar.Value == "null")
		//		{
		//			return true;
		//		}
		//	}

		//	return false;
		//}

	
		private Type GetTypeFromEvent(NodeEvent nodeEvent, Type currentType)//, TagMappings mappings)
		{
			foreach (var typeResolver in _typeResolvers)
			{
				if (typeResolver.Resolve(nodeEvent, ref currentType))
				{
					break;
				}
			}
			return currentType;
		}
	}

	public interface INodeTypeResolver
	{
		/// <summary>
		/// Determines the type of the specified node.
		/// </summary>
		/// <param name="nodeEvent">The node to be deserialized.</param>
		/// <param name="currentType">The type that has been determined so far.</param>
		/// <returns>
		/// true if <paramref name="currentType"/> has been resolved completely;
		/// false if the next type <see cref="INodeTypeResolver"/> should be invoked.
		/// </returns>
		bool Resolve(NodeEvent nodeEvent, ref Type currentType);
	}

	public sealed class PredefinedTagsNodeTypeResolver : INodeTypeResolver
	{
		private static readonly Dictionary<string, Type> predefinedTypes = new Dictionary<string, Type>
		{
			{ "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
			{ "tag:yaml.org,2002:bool", typeof(bool) },
			{ "tag:yaml.org,2002:float", typeof(double) },
			{ "tag:yaml.org,2002:int", typeof(int) },
			{ "tag:yaml.org,2002:str", typeof(string) },
			{ "tag:yaml.org,2002:timestamp", typeof(DateTime) },
		};

		bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
		{
			Type predefinedType;
			if (!string.IsNullOrEmpty(nodeEvent.Tag) && predefinedTypes.TryGetValue(nodeEvent.Tag, out predefinedType))
			{
				currentType = predefinedType;
				return true;
			}
			return false;
		}
	}

	public sealed class DefaultContainersNodeTypeResolver : INodeTypeResolver
	{
		bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
		{
			if (currentType == typeof(object))
			{
				if (nodeEvent is SequenceStart)
				{
					currentType = typeof(List<object>);
					return true;
				}
				if (nodeEvent is MappingStart)
				{
					currentType = typeof(Dictionary<object, object>);
					return true;
				}
			}

			return false;
		}
	}

	public sealed class TypeNameInTagNodeTypeResolver : INodeTypeResolver
	{
		bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
		{
			if (!string.IsNullOrEmpty(nodeEvent.Tag))
			{
				currentType = Type.GetType(nodeEvent.Tag.Substring(1), true);
				return true;
			}
			return false;
		}
	}

	public interface INodeDeserializer
	{
		bool Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value);
	}

	public sealed class NullNodeDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			value = null;
			var evt = reader.Peek<NodeEvent>();
			var isNull = evt != null
				&& evt.Tag == "tag:yaml.org,2002:null";

			if (isNull)
			{
				reader.Skip();
			}
			return isNull;
		}
	}

	public sealed class JsonNullNodeDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			value = null;
			var scalar = reader.Peek<Scalar>();
			var isNull = scalar != null
				&& scalar.Style == Core.ScalarStyle.Plain
				&& scalar.Value == "null";

			if (isNull)
			{
				reader.Skip();
			}
			return isNull;
		}
	}

	public sealed class ScalarNodeDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var scalar = reader.Allow<Scalar>();
			if (scalar == null)
			{
				value = null;
				return false;
			}

			if (expectedType.IsEnum)
			{
				value = Enum.Parse(expectedType, scalar.Value);
			}
			else
			{
				TypeCode typeCode = Type.GetTypeCode(expectedType);
				switch (typeCode)
				{
					case TypeCode.Boolean:
						value = bool.Parse(scalar.Value);
						break;

					case TypeCode.Byte:
						value = Byte.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int16:
						value = Int16.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int32:
						value = Int32.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int64:
						value = Int64.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.SByte:
						value = SByte.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt16:
						value = UInt16.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt32:
						value = UInt32.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt64:
						value = UInt64.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Single:
						value = Single.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Double:
						value = Double.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Decimal:
						value = Decimal.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.String:
						value = scalar.Value;
						break;

					case TypeCode.Char:
						value = scalar.Value[0];
						break;

					case TypeCode.DateTime:
						// TODO: This is probably incorrect. Use the correct regular expression.
						value = DateTime.Parse(scalar.Value, CultureInfo.InvariantCulture);
						break;

					default:
						if (expectedType == typeof(object))
						{
							// Default to string
							value = scalar.Value;
						}
						else
						{
							TypeConverter converter = TypeDescriptor.GetConverter(expectedType);
							if (converter != null && converter.CanConvertFrom(typeof(string)))
							{
								value = converter.ConvertFromInvariantString(scalar.Value);
							}
							else
							{
								value = Convert.ChangeType(scalar.Value, expectedType, CultureInfo.InvariantCulture);
							}
						}
						break;
				}
			}
			return true;
		}

		private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
		{
			CurrencyDecimalSeparator = ".",
			CurrencyGroupSeparator = "_",
			CurrencyGroupSizes = new[] { 3 },
			CurrencySymbol = string.Empty,
			CurrencyDecimalDigits = 99,
			NumberDecimalSeparator = ".",
			NumberGroupSeparator = "_",
			NumberGroupSizes = new[] { 3 },
			NumberDecimalDigits = 99
		};
	}

	public sealed class ObjectNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public ObjectNodeDeserializer(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var mapping = reader.Allow<MappingStart>();
			if (mapping == null)
			{
				value = null;
				return false;
			}

			value = _objectFactory.Create(expectedType);
			while (!reader.Accept<MappingEnd>())
			{
				var propertyName = reader.Expect<Scalar>();

				// TODO: Find property according to naming conventions
				var property = expectedType.GetProperty(propertyName.Value, BindingFlags.Instance | BindingFlags.Public);
				var propertyValue = nestedObjectDeserializer(reader, property.PropertyType);
				property.SetValue(value, propertyValue, null);
			}

			reader.Expect<MappingEnd>();
			return true;
		}
	}

	public sealed class GenericDictionaryDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public GenericDictionaryDeserializer(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
		}
	
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var iDictionary = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<,>));
			if (iDictionary == null)
			{
				value = false;
				return false;
			}

			reader.Expect<MappingStart>();

			value = _objectFactory.Create(expectedType);
			_deserializeHelperMethod
				.MakeGenericMethod(iDictionary.GetGenericArguments())
				.Invoke(null, new object[] { reader, expectedType, nestedObjectDeserializer, value });

			reader.Expect<MappingEnd>();

			return true;
		}

		private static MethodInfo _deserializeHelperMethod = typeof(GenericDictionaryDeserializer)
			.GetMethod("DeserializeHelper", BindingFlags.Static | BindingFlags.NonPublic);

		private static void DeserializeHelper<TKey, TValue>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, IDictionary<TKey, TValue> result)
		{
			while (!reader.Accept<MappingEnd>())
			{
				var key = (TKey)nestedObjectDeserializer(reader, typeof(TKey));
				var value = (TValue)nestedObjectDeserializer(reader, typeof(TValue));
				result.Add(key, value);
			}
		}
	}

	public sealed class NonGenericDictionaryDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public NonGenericDictionaryDeserializer(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			if(!typeof(IDictionary).IsAssignableFrom(expectedType))
			{
				value = false;
				return false;
			}

			reader.Expect<MappingStart>();

			var dictionary = (IDictionary)_objectFactory.Create(expectedType);
			while (!reader.Accept<MappingEnd>())
			{
				var key = nestedObjectDeserializer(reader, typeof(object));
				var keyValue = nestedObjectDeserializer(reader, typeof(object));
				dictionary.Add(key, keyValue);
			}
			value = dictionary;

			reader.Expect<MappingEnd>();

			return true;
		}
	}

	public sealed class GenericCollectionDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public GenericCollectionDeserializer(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var iCollection = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(ICollection<>));
			if (iCollection == null)
			{
				value = false;
				return false;
			}

			reader.Expect<SequenceStart>();

			value = _objectFactory.Create(expectedType);
			_deserializeHelperMethod
				.MakeGenericMethod(iCollection.GetGenericArguments())
				.Invoke(null, new object[] { reader, expectedType, nestedObjectDeserializer, value });

			reader.Expect<SequenceEnd>();

			return true;
		}

		private static MethodInfo _deserializeHelperMethod = typeof(GenericCollectionDeserializer)
			.GetMethod("DeserializeHelper", BindingFlags.Static | BindingFlags.NonPublic);

		private static void DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, ICollection<TItem> result)
		{
			while (!reader.Accept<SequenceEnd>())
			{
				var value = (TItem)nestedObjectDeserializer(reader, typeof(TItem));
				result.Add(value);
			}
		}
	}

	public sealed class NonGenericListDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public NonGenericListDeserializer(IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			if (!typeof(IList).IsAssignableFrom(expectedType))
			{
				value = false;
				return false;
			}

			reader.Expect<SequenceStart>();

			var list = (IList)_objectFactory.Create(expectedType);
			while (!reader.Accept<SequenceEnd>())
			{
				var item = nestedObjectDeserializer(reader, typeof(object));
				list.Add(item);
			}
			value = list;

			reader.Expect<SequenceEnd>();

			return true;
		}
	}

	public sealed class EnumerableDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			Type itemsType;
			if (expectedType == typeof(IEnumerable))
			{
				itemsType = typeof(object);
			}
			else
			{
				var iEnumerable = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IEnumerable<>));
				if (iEnumerable != expectedType)
				{
					value = null;
					return false;
				}

				itemsType = iEnumerable.GetGenericArguments()[0];
			}

			var collectionType = typeof(List<>).MakeGenericType(itemsType);
			value = nestedObjectDeserializer(reader, collectionType);
			return true;
		}
	}

}
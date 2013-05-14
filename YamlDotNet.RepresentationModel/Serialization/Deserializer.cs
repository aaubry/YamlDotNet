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
	public class DeserializerSkeleton
	{
		public IList<INodeDeserializer> Deserializers { get; private set; }
		public IList<INodeTypeResolver> TypeResolvers { get; private set; }

		public DeserializerSkeleton()
		{
			Deserializers = new List<INodeDeserializer>();
			TypeResolvers = new List<INodeTypeResolver>();
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

			var result = DeserializeValue(reader, type);

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

		private object DeserializeValue(EventReader reader, Type expectedType)
		{
			var nodeEvent = reader.Peek<NodeEvent>();

			var nodeType = GetTypeFromEvent(nodeEvent, expectedType);

			foreach (var deserializer in Deserializers)
			{
				object value;
				if (deserializer.Deserialize(reader, nodeType, DeserializeValue, out value))
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

		private Type GetTypeFromEvent(NodeEvent nodeEvent, Type currentType)
		{
			foreach (var typeResolver in TypeResolvers)
			{
				if (typeResolver.Resolve(nodeEvent, ref currentType))
				{
					break;
				}
			}
			return currentType;
		}
	}

	/// <summary>
	/// A façade for the YAML library with the standard configuration.
	/// </summary>
	public class Deserializer : DeserializerSkeleton
	{
		private static readonly Dictionary<string, Type> predefinedTagMappings = new Dictionary<string, Type>
		{
			{ "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
			{ "tag:yaml.org,2002:bool", typeof(bool) },
			{ "tag:yaml.org,2002:float", typeof(double) },
			{ "tag:yaml.org,2002:int", typeof(int) },
			{ "tag:yaml.org,2002:str", typeof(string) },
			{ "tag:yaml.org,2002:timestamp", typeof(DateTime) },
		};

		private readonly Dictionary<string, Type> tagMappings;
		private readonly List<IYamlTypeConverter> converters;

		public Deserializer()
			: this(new DefaultObjectFactory())
		{
		}

		public Deserializer(IObjectFactory objectFactory)
		{
			converters = new List<IYamlTypeConverter>();
			Deserializers.Add(new TypeConverterNodeDeserializer(converters));
			Deserializers.Add(new NullNodeDeserializer());
			Deserializers.Add(new ScalarNodeDeserializer());
			Deserializers.Add(new ArrayNodeDeserializer());
			Deserializers.Add(new GenericDictionaryNodeDeserializer(objectFactory));
			Deserializers.Add(new NonGenericDictionaryNodeDeserializer(objectFactory));
			Deserializers.Add(new GenericCollectionNodeDeserializer(objectFactory));
			Deserializers.Add(new NonGenericListNodeDeserializer(objectFactory));
			Deserializers.Add(new EnumerableNodeDeserializer());
			Deserializers.Add(new ObjectNodeDeserializer(objectFactory));

			tagMappings = new Dictionary<string, Type>(predefinedTagMappings);
			TypeResolvers.Add(new TagNodeTypeResolver(tagMappings));
			TypeResolvers.Add(new TypeNameInTagNodeTypeResolver());
			TypeResolvers.Add(new DefaultContainersNodeTypeResolver());
		}

		public void RegisterTagMapping(string tag, Type type)
		{
			tagMappings.Add(tag, type);
		}

		public void RegisterTypeConverter(IYamlTypeConverter typeConverter)
		{
			converters.Add(typeConverter);
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

	public sealed class TagNodeTypeResolver : INodeTypeResolver
	{
		private readonly IDictionary<string, Type> tagMappings;

		public TagNodeTypeResolver(IDictionary<string, Type> tagMappings)
		{
			if (tagMappings == null)
			{
				throw new ArgumentNullException("tagMappings");
			}

			this.tagMappings = tagMappings;
		}
		
		bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
		{
			Type predefinedType;
			if (!string.IsNullOrEmpty(nodeEvent.Tag) && tagMappings.TryGetValue(nodeEvent.Tag, out predefinedType))
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

	public sealed class GenericDictionaryNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public GenericDictionaryNodeDeserializer(IObjectFactory objectFactory)
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

		private static MethodInfo _deserializeHelperMethod = typeof(GenericDictionaryNodeDeserializer)
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

	public sealed class NonGenericDictionaryNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public NonGenericDictionaryNodeDeserializer(IObjectFactory objectFactory)
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

	public sealed class GenericCollectionNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public GenericCollectionNodeDeserializer(IObjectFactory objectFactory)
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

			value = _objectFactory.Create(expectedType);
			_deserializeHelper.InvokeStatic(iCollection.GetGenericArguments(), reader, expectedType, nestedObjectDeserializer, value);

			return true;
		}

		private static readonly GenericMethod _deserializeHelper = new GenericMethod(() => DeserializeHelper<object>(null, null, null, null));

		internal static void DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, ICollection<TItem> result)
		{
			reader.Expect<SequenceStart>();
			while (!reader.Accept<SequenceEnd>())
			{
				var value = (TItem)nestedObjectDeserializer(reader, typeof(TItem));
				result.Add(value);
			}
			reader.Expect<SequenceEnd>();
		}
	}

	public sealed class NonGenericListNodeDeserializer : INodeDeserializer
	{
		private readonly IObjectFactory _objectFactory;

		public NonGenericListNodeDeserializer(IObjectFactory objectFactory)
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

	public sealed class EnumerableNodeDeserializer : INodeDeserializer
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

	public sealed class ArrayNodeDeserializer : INodeDeserializer
	{
		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			if (!expectedType.IsArray)
			{
				value = false;
				return false;
			}

			value = _deserializeHelper.InvokeStatic(new[] { expectedType.GetElementType() }, reader, expectedType, nestedObjectDeserializer);
			return true;
		}

		private static readonly GenericMethod _deserializeHelper = new GenericMethod(() => DeserializeHelper<object>(null, null, null));

		private static TItem[] DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer)
		{
			var items = new List<TItem>();
			GenericCollectionNodeDeserializer.DeserializeHelper(reader, expectedType, nestedObjectDeserializer, items);
			return items.ToArray();
		}
	}

	public sealed class TypeConverterNodeDeserializer : INodeDeserializer
	{
		private readonly IEnumerable<IYamlTypeConverter> converters;

		public TypeConverterNodeDeserializer(IEnumerable<IYamlTypeConverter> converters)
		{
			if (converters == null)
			{
				throw new ArgumentNullException("converters");
			}

			this.converters = converters;
		}

		bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
		{
			var converter = converters.FirstOrDefault(c => c.Accepts(expectedType));
			if (converter == null)
			{
				value = null;
				return false;
			}

			value = converter.ReadYaml(reader.Parser, expectedType);
			return true;
		}
	}
}
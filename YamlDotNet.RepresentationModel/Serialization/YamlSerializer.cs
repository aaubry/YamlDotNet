using System;
using System.IO;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using System.Reflection;
using System.Globalization;
using YamlDotNet.Core.Events;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads and writes objects from and to YAML.
	/// </summary>
	public class YamlSerializer
	{
		private readonly YamlSerializerOptions options;
		private readonly Type serializedType;

		private class ObjectInfo
		{
			public string anchor;
			public bool serialized;
		}

		/// <summary>
		/// Contains additional information about a deserialization.
		/// </summary>
		private class DeserializationContext : IDeserializationContext
		{
			private readonly ObjectAnchorCollection anchors;

			/// <summary>
			/// Initializes a new instance of the <see cref="DeserializationContext"/> class.
			/// </summary>
			/// <param name="anchors">The anchors.</param>
			internal DeserializationContext(ObjectAnchorCollection anchors)
			{
				this.anchors = anchors;
			}

			/// <summary>
			/// Gets the anchor of the specified object.
			/// </summary>
			/// <param name="value">The object that has an anchor.</param>
			/// <returns>Returns the anchor of the object, or null if no anchor was defined.</returns>
			public string GetAnchor(object value)
			{
				string anchor;
				if (anchors.TryGetAnchor(value, out anchor))
				{
					return anchor;
				}
				return null;
			}
		}

		private readonly Dictionary<object, ObjectInfo> anchors;

		private bool Roundtrip
		{
			get
			{
				return (options & YamlSerializerOptions.Roundtrip) != 0;
			}
		}

		private bool DisableAliases
		{
			get
			{
				return (options & YamlSerializerOptions.DisableAliases) != 0;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer()
			: this(typeof(object), YamlSerializerOptions.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="options">The options the specify the behavior of the serializer.</param>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer(YamlSerializerOptions options)
			: this(typeof(object), options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		public YamlSerializer(Type serializedType)
			: this(serializedType, YamlSerializerOptions.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		/// <param name="options">The options the specify the behavior of the serializer.</param>
		public YamlSerializer(Type serializedType, YamlSerializerOptions options)
		{
			this.serializedType = serializedType;
			this.options = options;

			if (!DisableAliases)
			{
				anchors = new Dictionary<object, ObjectInfo>();
			}
		}

		#region Serialization
		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="output">The writer where to serialize the object.</param>
		/// <param name="o">The object to serialize.</param>
		public void Serialize(TextWriter output, object o)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output", "The output is null.");
			}

			if (!DisableAliases)
			{
				anchors.Clear();
				int nextId = 0;
				LoadAliases(serializedType, o, ref nextId);
			}

			Emitter emitter = new Emitter(output);

			emitter.Emit(new StreamStart());
			emitter.Emit(new DocumentStart());

			SerializeValue(emitter, serializedType, o);

			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());
		}

		private void LoadAliases(Type type, object o, ref int nextId)
		{
			if (anchors.ContainsKey(o))
			{
				if (anchors[o] == null)
				{
					anchors[o] = new ObjectInfo
					{
						anchor = string.Format(CultureInfo.InvariantCulture, "o{0}", nextId++)
					};
				}
			}
			else
			{
				anchors.Add(o, null);
				foreach (var property in GetProperties(type))
				{
					object value = property.GetValue(o, null);
					if (value != null && value.GetType().IsClass)
					{
						LoadAliases(property.PropertyType, value, ref nextId);
					}
				}
			}
		}

		/// <summary>
		/// Serializes the properties of the specified object into a mapping.
		/// </summary>
		/// <param name="emitter">The emitter.</param>
		/// <param name="type">The type of the object.</param>
		/// <param name="o">The o.</param>
		/// <param name="anchor">The anchor.</param>
		private void SerializeProperties(Emitter emitter, Type type, object o, string anchor)
		{
			if (Roundtrip && !HasDefaultConstructor(type))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
			}

			emitter.Emit(new MappingStart(anchor, null, true, MappingStyle.Block));

			foreach (var property in GetProperties(type))
			{
				emitter.Emit(new Scalar(null, null, property.Name, ScalarStyle.Plain, true, false));

				object value = property.GetValue(o, null);
				SerializeValue(emitter, property.PropertyType, value);
			}

			emitter.Emit(new MappingEnd());
		}

		private IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.CanRead && property.GetGetMethod().GetParameters().Length == 0)
				{
					if (!Roundtrip || property.CanWrite)
					{
						yield return property;
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the specified type has a default constructor.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// 	<c>true</c> if the type has a default constructor; otherwise, <c>false</c>.
		/// </returns>
		private static bool HasDefaultConstructor(Type type)
		{
			return type.IsValueType || type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
		}

		private static readonly NumberFormatInfo numberFormat = CreateNumberFormatInfo();

		private static NumberFormatInfo CreateNumberFormatInfo()
		{
			NumberFormatInfo format = new NumberFormatInfo();
			format.CurrencyDecimalSeparator = ".";
			format.CurrencyGroupSeparator = "_";
			format.CurrencyGroupSizes = new[] { 3 };
			format.CurrencySymbol = string.Empty;
			format.CurrencyDecimalDigits = 99;
			format.NumberDecimalSeparator = ".";
			format.NumberGroupSeparator = "_";
			format.NumberGroupSizes = new[] { 3 };
			format.NumberDecimalDigits = 99;
			return format;
		}

		/// <summary>
		/// Serializes the specified value.
		/// </summary>
		/// <param name="emitter">The emitter.</param>
		/// <param name="type">The type.</param>
		/// <param name="value">The value.</param>
		private void SerializeValue(Emitter emitter, Type type, object value)
		{
			if (value == null)
			{
				emitter.Emit(new Scalar(null, "tag:yaml.org,2002:null", "", ScalarStyle.Plain, false, false));
				return;
			}

			string anchor = null;
			ObjectInfo info;
			if (!DisableAliases && anchors.TryGetValue(value, out info) && info != null)
			{
				if (info.serialized)
				{
					emitter.Emit(new AnchorAlias(info.anchor));
					return;
				}

				info.serialized = true;
				anchor = info.anchor;
			}


			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean:
					emitter.Emit(new Scalar(anchor, "tag:yaml.org,2002:bool", value.ToString()));
					break;

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					emitter.Emit(new Scalar(anchor, "tag:yaml.org,2002:int", Convert.ToString(value, numberFormat)));
					break;

				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					emitter.Emit(new Scalar(anchor, "tag:yaml.org,2002:float", Convert.ToString(value, numberFormat)));
					break;

				case TypeCode.String:
				case TypeCode.Char:
					emitter.Emit(new Scalar(anchor, "tag:yaml.org,2002:str", value.ToString()));
					break;

				case TypeCode.DateTime:
					emitter.Emit(new Scalar(anchor, "tag:yaml.org,2002:timestamp", ((DateTime)value).ToString("o", CultureInfo.InvariantCulture)));
					break;

				case TypeCode.DBNull:
				case TypeCode.Empty:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));

				default:
					SerializeProperties(emitter, type, value, anchor);
					break;
			}
		}
		#endregion

		#region Deserialization
		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input, out IDeserializationContext context)
		{
			return Deserialize(new EventReader(new Parser(input)), out context);
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input)
		{
			return Deserialize(new EventReader(new Parser(input)));
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader)
		{
			IDeserializationContext context;
			return Deserialize(reader, out context);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader, out IDeserializationContext context)
		{
			ObjectAnchorCollection deserializedAnchors = new ObjectAnchorCollection();

			bool hasStreamStart = reader.Accept<StreamStart>();
			if (hasStreamStart)
			{
				reader.Expect<StreamStart>();
			}

			bool hasDocumentStart = reader.Accept<DocumentStart>();
			if (hasDocumentStart)
			{
				reader.Expect<DocumentStart>();
			}

			object result = DeserializeValue(reader, serializedType, deserializedAnchors);

			if (hasDocumentStart)
			{
				reader.Expect<DocumentEnd>();
			}

			if (hasStreamStart)
			{
				reader.Expect<StreamEnd>();
			}

			context = new DeserializationContext(deserializedAnchors);

			return result;
		}

		private object DeserializeValue(EventReader reader, Type type, ObjectAnchorCollection deserializedAnchors)
		{
			if(reader.Accept<AnchorAlias>())
			{
				return deserializedAnchors[reader.Expect<AnchorAlias>().Value];
			}

			if (reader.Accept<MappingStart>())
			{
				return DeserializeProperties(reader, type, deserializedAnchors);
			}

			if (reader.Accept<SequenceStart>())
			{
				return DeserializeList(reader, type, deserializedAnchors);
			}

			return DeserializeScalar(reader, type, deserializedAnchors);
		}

		private static object DeserializeScalar(EventReader reader, Type type, ObjectAnchorCollection deserializedAnchors)
		{
			Scalar scalar = reader.Expect<Scalar>();

			object result;
			if (scalar.Tag == "tag:yaml.org,2002:null")
			{
				result = null;
			}
			else
			{
				type = GetType(scalar.Tag, type);

				TypeCode typeCode = Type.GetTypeCode(type);
				switch (typeCode)
				{
					case TypeCode.Boolean:
						result = bool.Parse(scalar.Value);
						break;

					case TypeCode.Byte:
						result = Byte.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int16:
						result = Int16.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int32:
						result = Int32.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Int64:
						result = Int64.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.SByte:
						result = SByte.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt16:
						result = UInt16.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt32:
						result = UInt32.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.UInt64:
						result = UInt64.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Single:
						result = Single.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Double:
						result = Double.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.Decimal:
						result = Decimal.Parse(scalar.Value, numberFormat);
						break;

					case TypeCode.String:
						result = scalar.Value;
						break;

					case TypeCode.Char:
						result = scalar.Value[0];
						break;

					case TypeCode.DateTime:
						// TODO: This is probably incorrect. Use the correct regular expression.
						result = DateTime.Parse(scalar.Value, CultureInfo.InvariantCulture);
						break;

					default:
						// Default to string
						if (type != typeof(object))
						{
							throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
						}
						result = scalar.Value;
						break;
				}
			}

			AddAnchoredObject(scalar, result, deserializedAnchors);

			return result;
		}

		private static void AddAnchoredObject(INodeEvent node, object value, ObjectAnchorCollection deserializedAnchors)
		{
			if(!string.IsNullOrEmpty(node.Anchor))
			{
				deserializedAnchors.Add(node.Anchor, value);
			}
		}

		private static Type GetGenericInterface(Type implementorType, Type interfaceType)
		{
			foreach (var currentInterfaceType in implementorType.GetInterfaces())
			{
				if (currentInterfaceType.IsGenericType && currentInterfaceType.GetGenericTypeDefinition() == interfaceType)
				{
					return currentInterfaceType;
				}
			}
			return null;
		}

		private static void AddAdapter<T>(object list, object value)
		{
			((ICollection<T>)list).Add((T)value);
		}

		private static readonly MethodInfo addAdapterGeneric = typeof(YamlSerializer).GetMethod("AddAdapter", BindingFlags.Static | BindingFlags.NonPublic);

		private object DeserializeList(EventReader reader, Type type, ObjectAnchorCollection deserializedAnchors)
		{
			SequenceStart sequence = reader.Expect<SequenceStart>();

			type = GetType(sequence.Tag, type);
			object result = Activator.CreateInstance(type);

			Type iCollection = GetGenericInterface(type, typeof(ICollection<>));
			if (iCollection != null)
			{
				Type[] iCollectionArguments = iCollection.GetGenericArguments();
				Debug.Assert(iCollectionArguments.Length == 1, "ICollection<> must have one generic argument.");

				MethodInfo addAdapter = addAdapterGeneric.MakeGenericMethod(iCollectionArguments);
				Action<object, object> addAdapterDelegate = (Action<object, object>)Delegate.CreateDelegate(typeof(Action<object, object>), addAdapter);
				DeserializeGenericListInternal(reader, iCollectionArguments[0], result, addAdapterDelegate, deserializedAnchors);
			}
			else
			{
				IList list = result as IList;
				if(list != null)
				{
					while(!reader.Accept<SequenceEnd>())
					{
						list.Add(DeserializeValue(reader, typeof(object), deserializedAnchors));
					}
				}
				reader.Expect<SequenceEnd>();
			}

			AddAnchoredObject(sequence, result, deserializedAnchors);

			return result;
		}

		private void DeserializeGenericListInternal(EventReader reader, Type itemType, object list, Action<object, object> addAdapterDelegate, ObjectAnchorCollection deserializedAnchors)
		{
			while (!reader.Accept<SequenceEnd>())
			{
				addAdapterDelegate(list, DeserializeValue(reader, itemType, deserializedAnchors));
			}
			reader.Expect<SequenceEnd>();
		}

		private object DeserializeProperties(EventReader reader, Type type, ObjectAnchorCollection deserializedAnchors)
		{
			MappingStart mapping = reader.Expect<MappingStart>();

			type = GetType(mapping.Tag, type);
			object result = Activator.CreateInstance(type);

			IDictionary dictionary = result as IDictionary;
			if (dictionary != null)
			{
				Type keyType = typeof(object);
				Type valueType = typeof(object);

				foreach (var interfaceType in result.GetType().GetInterfaces())
				{
					if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
					{
						Type[] genericArguments = interfaceType.GetGenericArguments();
						Debug.Assert(genericArguments.Length == 2, "IDictionary<,> must contain two generic arguments.");
						keyType = genericArguments[0];
						valueType = genericArguments[1];
						break;
					}
				}

				while (!reader.Accept<MappingEnd>())
				{
					object key = DeserializeValue(reader, keyType, deserializedAnchors);
					object value = DeserializeValue(reader, valueType, deserializedAnchors);
					dictionary.Add(key, value);
				}
			}
			else
			{
				while (!reader.Accept<MappingEnd>())
				{
					Scalar key = reader.Expect<Scalar>();
					PropertyInfo property = type.GetProperty(key.Value, BindingFlags.Instance | BindingFlags.Public);
					property.SetValue(result, DeserializeValue(reader, property.PropertyType, deserializedAnchors), null);
				}
			}
			reader.Expect<MappingEnd>();

			AddAnchoredObject(mapping, result, deserializedAnchors);

			return result;
		}

		private static readonly Dictionary<string, Type> predefinedTypes = new Dictionary<string, Type>
		{
			{ "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
			{ "tag:yaml.org,2002:bool", typeof(bool) },
			{ "tag:yaml.org,2002:float", typeof(double) },
			{ "tag:yaml.org,2002:int", typeof(int) },
			{ "tag:yaml.org,2002:str", typeof(string) },
			{ "tag:yaml.org,2002:timestamp", typeof(DateTime) },
		};

		private static Type GetType(string tag, Type defaultType)
		{
			if (tag == null)
			{
				return defaultType;
			}

			Type predefinedType;
			if (predefinedTypes.TryGetValue(tag, out predefinedType))
			{
				return predefinedType;
			}

			return Type.GetType(tag.Substring(1), true);
		}
		#endregion
	}

	/// <summary>
	/// Extension of the <see cref="YamlSerializer"/> type that avoida the need for casting
	/// on the user's code.
	/// </summary>
	/// <typeparam name="TSerialized">The type of the serialized.</typeparam>
	public class YamlSerializer<TSerialized> : YamlSerializer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer&lt;TSerialized&gt;"/> class.
		/// </summary>
		public YamlSerializer()
			: base(typeof(TSerialized))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer&lt;TSerialized&gt;"/> class.
		/// </summary>
		/// <param name="options">The options.</param>
		public YamlSerializer(YamlSerializerOptions options)
			: base(typeof(TSerialized), options)
		{
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(TextReader input)
		{
			return (TSerialized)base.Deserialize(input);
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(TextReader input, out IDeserializationContext context)
		{
			return (TSerialized)base.Deserialize(input, out context);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(EventReader reader)
		{
			return (TSerialized)base.Deserialize(reader);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(EventReader reader, out IDeserializationContext context)
		{
			return (TSerialized)base.Deserialize(reader, out context);
		}
	}
}
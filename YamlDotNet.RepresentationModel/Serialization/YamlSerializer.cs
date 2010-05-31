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
using System.ComponentModel;
using System.Linq.Expressions;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads and writes objects from and to YAML.
	/// </summary>
	public class YamlSerializer
	{
		private readonly YamlSerializerMode mode;
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
			private readonly ObjectAnchorCollection anchors = new ObjectAnchorCollection();

			internal ObjectAnchorCollection Anchors
			{
				get
				{
					return anchors;
				}
			}

			private readonly DeserializationOptions options;

			internal DeserializationOptions Options
			{
				get
				{
					return options;
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="DeserializationContext"/> class.
			/// </summary>
			/// <param name="options">The mode.</param>
			internal DeserializationContext(DeserializationOptions options)
			{
				this.options = options ?? new DeserializationOptions();
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
				return (mode & YamlSerializerMode.Roundtrip) != 0;
			}
		}

		private bool DisableAliases
		{
			get
			{
				return (mode & YamlSerializerMode.DisableAliases) != 0;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer()
			: this(typeof(object), YamlSerializerMode.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="mode">The options the specify the behavior of the serializer.</param>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer(YamlSerializerMode mode)
			: this(typeof(object), mode)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		public YamlSerializer(Type serializedType)
			: this(serializedType, YamlSerializerMode.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		/// <param name="mode">The options the specify the behavior of the serializer.</param>
		public YamlSerializer(Type serializedType, YamlSerializerMode mode)
		{
			this.serializedType = serializedType;
			this.mode = mode;

			if (!DisableAliases)
			{
				anchors = new Dictionary<object, ObjectInfo>();
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="YamlSerializer{TSerialized}"/>.
		/// </summary>
		/// <typeparam name="TSerialized">The type of the serialized.</typeparam>
		/// <param name="serialized">An object of the serialized type. This parameter is necessary to allow type inference.</param>
		/// <returns></returns>
		public static YamlSerializer<TSerialized> Create<TSerialized>(TSerialized serialized)
		{
			return new YamlSerializer<TSerialized>();
		}

		/// <summary>
		/// Creates a new instance of <see cref="YamlSerializer{TSerialized}"/>.
		/// </summary>
		/// <typeparam name="TSerialized">The type of the serialized.</typeparam>
		/// <param name="serialized">An object of the serialized type. This parameter is necessary to allow type inference.</param>
		/// <param name="mode">The mode.</param>
		/// <returns></returns>
		public static YamlSerializer<TSerialized> Create<TSerialized>(TSerialized serialized, YamlSerializerMode mode)
		{
			return new YamlSerializer<TSerialized>(mode);
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

		private void SerializeObject(Emitter emitter, Type type, object value, string anchor)
		{
			if (Roundtrip && !HasDefaultConstructor(type))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
			}

			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				SerializeDictionary(emitter, value, anchor);
				return;
			}

			Type iDictionaryType = GetImplementedGenericInterface(type, typeof(IDictionary<,>));
			if(iDictionaryType != null)
			{
				SerializeGenericDictionary(emitter, iDictionaryType, type, value, anchor);
				return;
			}
			
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				SerializeList(emitter, type, value, anchor);
				return;
			}

			SerializeProperties(emitter, type, value, anchor);
		}

		private void SerializeGenericDictionary(Emitter emitter, Type iDictionaryType, Type type, object value, string anchor)
		{
			emitter.Emit(new MappingStart(anchor, null, true, MappingStyle.Any));

			Func<object, object> getKey = MakeKeyValuePairGetter(iDictionaryType, "Key");
			Func<object, object> getValue = MakeKeyValuePairGetter(iDictionaryType, "Value");

			foreach (object entry in (IEnumerable)value)
			{
				var entryKey = getKey(entry);
				SerializeValue(emitter, entryKey.GetType(), entryKey);

				var entryValue = getValue(entry);
				SerializeValue(emitter, entryValue.GetType(), entryValue);
			}

			emitter.Emit(new SequenceEnd());
		}

		private Func<object, object> MakeKeyValuePairGetter(Type iDictionaryType, string propertyName)
		{
			var getKeyValuePairKeyGeneric = GetType().GetMethod("GetKeyValuePair" + propertyName, BindingFlags.Static | BindingFlags.NonPublic);
			var getKeyValuePairKey = getKeyValuePairKeyGeneric.MakeGenericMethod(iDictionaryType.GetGenericArguments()[0].GetGenericArguments());
			return (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), getKeyValuePairKey);
		}

		// ReSharper disable UnusedPrivateMember
		// This methid is invoked using reflection.
		private static object GetKeyValuePairKey<T, U>(object pair)
		{
			return ((KeyValuePair<T, U>)pair).Key;
		}
		// ReSharper restore UnusedPrivateMember

		// ReSharper disable UnusedPrivateMember
		// This methid is invoked using reflection.
		private static object GetKeyValuePairValue<T, U>(object pair)
		{
			return ((KeyValuePair<T, U>)pair).Value;
		}
		// ReSharper restore UnusedPrivateMember

		private void SerializeDictionary(Emitter emitter, object value, string anchor)
		{
			emitter.Emit(new MappingStart(anchor, null, true, MappingStyle.Any));

			foreach(DictionaryEntry entry in (IDictionary)value)
			{
				SerializeValue(emitter, GetObjectType(entry.Key), entry.Key);
				SerializeValue(emitter, GetObjectType(entry.Value), entry.Value);
			}

			emitter.Emit(new MappingEnd());
		}

		private void SerializeList(Emitter emitter, Type type, object value, string anchor)
		{
			Type itemType = GetItemType(type, typeof(IEnumerable<>));

			emitter.Emit(new SequenceStart(anchor, null, true, SequenceStyle.Any));

			foreach (object item in (IEnumerable)value)
			{
				SerializeValue(emitter, itemType, item);
			}

			emitter.Emit(new SequenceEnd());
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

			IYamlSerializable serializable = value as IYamlSerializable;
			if (serializable != null)
			{
				serializable.WriteYaml(emitter);
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
					SerializeObject(emitter, type, value, anchor);
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
			return Deserialize(input, null, out context);
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="options">The mode.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input, DeserializationOptions options, out IDeserializationContext context)
		{
			return Deserialize(new EventReader(new Parser(input)), options, out context);
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input)
		{
			return Deserialize(input, null);
		}

		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="options">The mode.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input, DeserializationOptions options)
		{
			return Deserialize(new EventReader(new Parser(input)), options);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader)
		{
			return Deserialize(reader, null);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="options">The mode.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader, DeserializationOptions options)
		{
			IDeserializationContext context;
			return Deserialize(reader, options, out context);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader, out IDeserializationContext context)
		{
			return Deserialize(reader, null, out context);
		}

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="options">The mode.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public object Deserialize(EventReader reader, DeserializationOptions options, out IDeserializationContext context)
		{
			var internalContext = new DeserializationContext(options);

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

			object result = DeserializeValue(reader, serializedType, internalContext);

			if (hasDocumentStart)
			{
				reader.Expect<DocumentEnd>();
			}

			if (hasStreamStart)
			{
				reader.Expect<StreamEnd>();
			}

			context = internalContext;

			return result;
		}

		private object DeserializeValue(EventReader reader, Type expectedType, DeserializationContext context)
		{
			if (reader.Accept<AnchorAlias>())
			{
				return context.Anchors[reader.Expect<AnchorAlias>().Value];
			}

			NodeEvent nodeEvent = (NodeEvent)reader.Parser.Current;

			if (nodeEvent.Tag == "tag:yaml.org,2002:null")
			{
				reader.Expect<NodeEvent>();
				AddAnchoredObject(nodeEvent, null, context.Anchors);
				return null;
			}

			object result = DeserializeValueNotNull(reader, context, nodeEvent, expectedType);
			return ObjectConverter.Convert(result, expectedType);
		}

		private object DeserializeValueNotNull(EventReader reader, DeserializationContext context, INodeEvent nodeEvent, Type expectedType)
		{
			Type type = GetType(nodeEvent.Tag, expectedType, context.Options.Mappings);

			if (typeof(IYamlSerializable).IsAssignableFrom(type))
			{
				return DeserializeYamlSerializable(reader, type);
			}

			if (reader.Accept<MappingStart>())
			{
				return DeserializeProperties(reader, type, context);
			}

			if (reader.Accept<SequenceStart>())
			{
				return DeserializeList(reader, type, context);
			}

			if (reader.Accept<Scalar>())
			{
				return DeserializeScalar(reader, type, context);
			}

			throw new InvalidOperationException("Expected scalar, mapping or sequence.");
		}

		private static object DeserializeScalar(EventReader reader, Type type, DeserializationContext context)
		{
			Scalar scalar = reader.Expect<Scalar>();

			object result;
			type = GetType(scalar.Tag, type, context.Options.Mappings);

			if (type.IsEnum)
			{
				result = Enum.Parse(type, scalar.Value);
			}
			else
			{
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
						if (type == typeof(object))
						{
							// Default to string
							result = scalar.Value;
						}
						else
						{
							TypeConverter converter = TypeDescriptor.GetConverter(type);
							if(converter != null && converter.CanConvertFrom(typeof(string)))
							{
								result = converter.ConvertFromInvariantString(scalar.Value);
							}
							else
							{
								result = Convert.ChangeType(scalar.Value, type, CultureInfo.InvariantCulture);
							}
						}
						break;
				}
			}

			AddAnchoredObject(scalar, result, context.Anchors);

			return result;
		}

		private static void AddAnchoredObject(INodeEvent node, object value, ObjectAnchorCollection deserializedAnchors)
		{
			if (!string.IsNullOrEmpty(node.Anchor))
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

		// Called through reflection
		// ReSharper disable UnusedPrivateMember
		private static void AddAdapter<T>(object list, object value)
		{
			((ICollection<T>)list).Add((T)value);
		}
		// ReSharper restore UnusedPrivateMember

		private static readonly MethodInfo addAdapterGeneric = typeof(YamlSerializer).GetMethod("AddAdapter", BindingFlags.Static | BindingFlags.NonPublic);

		private object DeserializeList(EventReader reader, Type type, DeserializationContext context)
		{
			SequenceStart sequence = reader.Expect<SequenceStart>();

			type = GetType(sequence.Tag, type, context.Options.Mappings);

			// Choose a default list type in case there was no specific type specified.
			if (type == typeof(object))
			{
				type = typeof(ArrayList);
			}

			object result = Activator.CreateInstance(type);

			Type iCollection = GetGenericInterface(type, typeof(ICollection<>));
			if (iCollection != null)
			{
				Type[] iCollectionArguments = iCollection.GetGenericArguments();
				Debug.Assert(iCollectionArguments.Length == 1, "ICollection<> must have one generic argument.");

				MethodInfo addAdapter = addAdapterGeneric.MakeGenericMethod(iCollectionArguments);
				Action<object, object> addAdapterDelegate = (Action<object, object>)Delegate.CreateDelegate(typeof(Action<object, object>), addAdapter);
				DeserializeGenericListInternal(reader, iCollectionArguments[0], result, addAdapterDelegate, context);
			}
			else
			{
				IList list = result as IList;
				if (list != null)
				{
					while (!reader.Accept<SequenceEnd>())
					{
						list.Add(DeserializeValue(reader, typeof(object), context));
					}
				}
				reader.Expect<SequenceEnd>();
			}

			AddAnchoredObject(sequence, result, context.Anchors);

			return result;
		}

		private void DeserializeGenericListInternal(EventReader reader, Type itemType, object list, Action<object, object> addAdapterDelegate, DeserializationContext context)
		{
			while (!reader.Accept<SequenceEnd>())
			{
				addAdapterDelegate(list, DeserializeValue(reader, itemType, context));
			}
			reader.Expect<SequenceEnd>();
		}

		private static Type GetImplementedGenericInterface(Type type, Type genericInterfaceType)
		{
			foreach (Type interfacetype in type.GetInterfaces())
			{
				if (interfacetype.IsGenericType && interfacetype.GetGenericTypeDefinition() == genericInterfaceType)
				{
					return interfacetype;
				}
			}
			return null;
		}

		private static Type GetItemType(Type type, Type genericInterfaceType)
		{
			var implementedInterface = GetImplementedGenericInterface(type, genericInterfaceType);
			return implementedInterface != null ? implementedInterface.GetGenericArguments()[0] : typeof(object);
		}

		private static Type GetObjectType(object value)
		{
			return value != null ? value.GetType() : typeof(object);
		}

		private static object DeserializeYamlSerializable(EventReader reader, Type type)
		{
			IYamlSerializable result = (IYamlSerializable)Activator.CreateInstance(type);
			result.ReadYaml(reader.Parser);
			return result;
		}

		private object DeserializeProperties(EventReader reader, Type type, DeserializationContext context)
		{
			MappingStart mapping = reader.Expect<MappingStart>();

			type = GetType(mapping.Tag, type, context.Options.Mappings);
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
					object key = DeserializeValue(reader, keyType, context);
					object value = DeserializeValue(reader, valueType, context);
					dictionary.Add(key, value);
				}
			}
			else
			{
				while (!reader.Accept<MappingEnd>())
				{
					Scalar key = reader.Expect<Scalar>();

					bool isOverriden = false;
					if (context.Options != null)
					{
						var deserializer = context.Options.Overrides.GetOverride(type, key.Value);
						if (deserializer != null)
						{
							isOverriden = true;
							deserializer(result, reader);
						}
					}

					if (!isOverriden)
					{
						PropertyInfo property = type.GetProperty(key.Value, BindingFlags.Instance | BindingFlags.Public);
						if (property == null)
						{
							Console.WriteLine(key);

							throw new SerializationException(
								string.Format(
									CultureInfo.InvariantCulture,
									"Property '{0}' not found on type '{1}'",
									key.Value,
									type.FullName
								)
							);
						}
						property.SetValue(result, DeserializeValue(reader, property.PropertyType, context), null);
					}
				}
			}
			reader.Expect<MappingEnd>();

			AddAnchoredObject(mapping, result, context.Anchors);

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

		private static readonly Dictionary<Type, Type> defaultInterfaceImplementations = new Dictionary<Type, Type>
		{
			{ typeof(IEnumerable<>), typeof(List<>) },
			{ typeof(ICollection<>), typeof(List<>) },
			{ typeof(IList<>), typeof(List<>) },
			{ typeof(IDictionary<,>), typeof(Dictionary<,>) },
		};

		private static Type GetType(string tag, Type defaultType, TagMappings mappings)
		{
			Type actualType = GetTypeFromTag(tag, defaultType, mappings);

			if (actualType.IsInterface)
			{
				Type implementationType;
				if (defaultInterfaceImplementations.TryGetValue(actualType.GetGenericTypeDefinition(), out implementationType))
				{
					return implementationType.MakeGenericType(actualType.GetGenericArguments());
				}
			}

			return actualType;
		}

		private static Type GetTypeFromTag(string tag, Type defaultType, TagMappings mappings)
		{
			if (tag == null)
			{
				return defaultType;
			}

			Type predefinedType = mappings.GetMapping(tag);
			if (predefinedType != null || predefinedTypes.TryGetValue(tag, out predefinedType))
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
		/// Initializes a new instance of the <see cref="YamlSerializer{TSerialized}"/> class.
		/// </summary>
		public YamlSerializer()
			: base(typeof(TSerialized))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer{TSerialized}"/> class.
		/// </summary>
		/// <param name="mode">The options.</param>
		public YamlSerializer(YamlSerializerMode mode)
			: base(typeof(TSerialized), mode)
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
		/// <param name="options">The options.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(TextReader input, DeserializationOptions options)
		{
			return (TSerialized)base.Deserialize(input, options);
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
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="options">The options.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(TextReader input, DeserializationOptions options, out IDeserializationContext context)
		{
			return (TSerialized)base.Deserialize(input, options, out context);
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
		/// <param name="options">The options.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(EventReader reader, DeserializationOptions options)
		{
			return (TSerialized)base.Deserialize(reader, options);
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

		/// <summary>
		/// Deserializes the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="options">The options.</param>
		/// <param name="context">Returns additional information about the deserialization process.</param>
		/// <returns></returns>
		public new TSerialized Deserialize(EventReader reader, DeserializationOptions options, out IDeserializationContext context)
		{
			return (TSerialized)base.Deserialize(reader, options, out context);
		}
	}
}

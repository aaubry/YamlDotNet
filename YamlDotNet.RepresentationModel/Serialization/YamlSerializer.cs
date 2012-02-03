//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011 Antoine Aubry

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
using System.Linq.Expressions;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads and writes objects from and to YAML.
	/// </summary>
	public class YamlSerializer
	{
		private readonly YamlSerializerModes mode;
		private readonly Type serializedType;

		private readonly IList<IYamlTypeConverter> converters = new List<IYamlTypeConverter>();

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

		private class SerializationContext
		{
			public SerializationContext(ICollection<ISerializationBehavior> serializationBehaviors)
			{
				this.serializationBehaviors = serializationBehaviors;
			}

			private readonly ICollection<ISerializationBehavior> serializationBehaviors;

			public void InvokeBehaviors(Action<ISerializationBehavior> invoke)
			{
				foreach (var serializationBehavior in serializationBehaviors)
				{
					invoke(serializationBehavior);
				}
			}
		}

		private bool Roundtrip
		{
			get
			{
				return (mode & YamlSerializerModes.Roundtrip) != 0;
			}
		}

		private bool DisableAliases
		{
			get
			{
				return (mode & YamlSerializerModes.DisableAliases) != 0;
			}
		}

		private bool EmitDefaults
		{
			get
			{
				return (mode & YamlSerializerModes.EmitDefaults) != 0;
			}
		}

		private bool JsonCompatible
		{
			get
			{
				return (mode & YamlSerializerModes.JsonCompatible) != 0;
			}
		}

		private MappingStyle MappingStyle
		{
			get { return JsonCompatible ? MappingStyle.Flow : MappingStyle.Any; }
		}

		private SequenceStyle SequenceStyle
		{
			get { return JsonCompatible ? SequenceStyle.Flow : SequenceStyle.Any; }
		}

		private ScalarStyle ScalarStyle
		{
			get { return JsonCompatible ? ScalarStyle.DoubleQuoted : ScalarStyle.Plain; }
		}



		private readonly IEnumerable<ISerializationBehaviorFactory> serializationBehaviorFactories;



		private readonly IObjectGraphTraversalStrategy traversalStrategy;


		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer()
			: this(null, YamlSerializerModes.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="mode">The options the specify the behavior of the serializer.</param>
		/// <remarks>
		/// When deserializing, the stream must contain type information for the root element.
		/// </remarks>
		public YamlSerializer(YamlSerializerModes mode)
			: this(null, mode)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		public YamlSerializer(Type serializedType)
			: this(serializedType, YamlSerializerModes.None)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		/// <param name="mode">The options the specify the behavior of the serializer.</param>
		public YamlSerializer(Type serializedType, YamlSerializerModes mode)
		{
			this.serializedType = serializedType;
			this.mode = mode;

			if((mode & YamlSerializerModes.Roundtrip) != 0)
			{
				traversalStrategy = new RoundtripObjectGraphTraversalStrategy();
			}
			else
			{
				traversalStrategy = new FullObjectGraphTraversalStrategy();
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
		public static YamlSerializer<TSerialized> Create<TSerialized>(TSerialized serialized, YamlSerializerModes mode)
		{
			return new YamlSerializer<TSerialized>(mode);
		}

		/// <summary>
		/// Registers a type converter to be used to serialize and deserialize specific types.
		/// </summary>
		public void RegisterTypeConverter(IYamlTypeConverter converter)
		{
			converters.Add(converter);
		}

		#region Serialization
		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="output">The writer where to serialize the object.</param>
		/// <param name="graph">The object to serialize.</param>
		public void Serialize(TextWriter output, object graph)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output", "The output is null.");
			}

			var type = serializedType ?? graph.GetType();


			var behaviors = serializationBehaviorFactories.Select(f => f.Create()).ToList();
			var context = new SerializationContext(behaviors);


			context.InvokeBehaviors(b => b.SerializationStarting(type, graph));

			var emitter = new Emitter(output);

			emitter.Emit(new StreamStart());
			emitter.Emit(new DocumentStart());

			var visitor = new SerializationVisitor(emitter, this);
			traversalStrategy.Traverse(graph, type, visitor);
			
			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());
		}

		private class SerializationVisitor : IObjectGraphVisitor
		{
			private readonly Emitter emitter;
			private readonly YamlSerializer serializer;

			public SerializationVisitor(Emitter emitter, YamlSerializer serializer)
			{
				this.emitter = emitter;
				this.serializer = serializer;
			}

			bool IObjectGraphVisitor.Enter(object value, Type type)
			{
				return true;
			}

			bool IObjectGraphVisitor.EnterMapping(object key, Type keyType, object value, Type valueType)
			{
				return true;
			}

			void IObjectGraphVisitor.VisitScalar(object value, Type type)
			{
				if (value == null)
				{
					if (serializer.JsonCompatible)
					{
						emitter.Emit(new Scalar(null, null, "null", ScalarStyle.Plain, true, false));
					}
					else
					{
						emitter.Emit(new Scalar(null, "tag:yaml.org,2002:null", "", ScalarStyle.Plain, false, false));
					}
					return;
				}

				string scalarTag;
				string scalarValue;
				var scalarStyle = serializer.ScalarStyle;

				var typeCode = Type.GetTypeCode(type);
				switch (typeCode)
				{
					case TypeCode.Boolean:
						scalarTag = "tag:yaml.org,2002:bool";
						scalarValue = value.Equals(true) ? "true" : "false";
						scalarStyle = ScalarStyle.Plain;
						break;

					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
						scalarTag = "tag:yaml.org,2002:int";
						scalarValue = Convert.ToString(value, numberFormat);
						scalarStyle = ScalarStyle.Plain;
						break;

					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
						scalarTag = "tag:yaml.org,2002:float";
						scalarValue = Convert.ToString(value, numberFormat);
						scalarStyle = ScalarStyle.Plain;
						break;

					case TypeCode.String:
					case TypeCode.Char:
						scalarTag = "tag:yaml.org,2002:str";
						scalarValue = value.ToString();
						break;

					case TypeCode.DateTime:
						scalarTag = "tag:yaml.org,2002:timestamp";
						scalarValue = ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
						scalarStyle = ScalarStyle.Plain;
						break;

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));
				}

				if (serializer.JsonCompatible)
				{
					emitter.Emit(new Scalar(null /* TODO anchor */, null, scalarValue, scalarStyle, true, false));
				}
				else
				{
					emitter.Emit(new Scalar(null /* TODO anchor */, scalarTag, scalarValue));
				}
			}

			void IObjectGraphVisitor.VisitMappingStart(Type keyType, Type valueType)
			{
				emitter.Emit(new MappingStart(null /* TODO anchor */, null, true, serializer.MappingStyle));
			}

			void IObjectGraphVisitor.VisitMappingEnd()
			{
				emitter.Emit(new MappingEnd());
			}

			void IObjectGraphVisitor.VisitSequenceStart(Type elementType)
			{
				SequenceStyle sequenceStyle;
				switch (Type.GetTypeCode(elementType))
				{
					case TypeCode.String:
					case TypeCode.Object:
						sequenceStyle = serializer.SequenceStyle;
						break;

					default:
						sequenceStyle = SequenceStyle.Flow;
						break;
				}

				emitter.Emit(new SequenceStart(null /* TODO anchor */, null, true, sequenceStyle));
			}

			void IObjectGraphVisitor.VisitSequenceEnd()
			{
				emitter.Emit(new SequenceEnd());
			}
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
			if (serializedType == null)
			{
				throw new InvalidOperationException("Cannot deserialize when the serialized type is not specified in the constructor.");
			}

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

			if (IsNull(nodeEvent))
			{
				reader.Expect<NodeEvent>();
				AddAnchoredObject(nodeEvent, null, context.Anchors);
				return null;
			}

			object result = DeserializeValueNotNull(reader, context, nodeEvent, expectedType);
			return ObjectConverter.Convert(result, expectedType);
		}

		private bool IsNull(NodeEvent nodeEvent)
		{
			if (nodeEvent.Tag == "tag:yaml.org,2002:null")
			{
				return true;
			}

			if (JsonCompatible)
			{
				var scalar = nodeEvent as Scalar;
				if (scalar != null && scalar.Style == Core.ScalarStyle.Plain && scalar.Value == "null")
				{
					return true;
				}
			}

			return false;
		}

		private object DeserializeValueNotNull(EventReader reader, DeserializationContext context, INodeEvent nodeEvent, Type expectedType)
		{
			Type type = GetType(nodeEvent.Tag, expectedType, context.Options.Mappings);

			var converter = converters.FirstOrDefault(c => c.Accepts(type));
			if (converter != null)
			{
				return DeserializeWithYamlTypeConverter(reader, type, converter);
			}

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
							if (converter != null && converter.CanConvertFrom(typeof(string)))
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

			Type iCollection = ReflectionUtility.GetImplementedGenericInterface(type, typeof(ICollection<>));
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

		private static Type GetItemType(Type type, Type genericInterfaceType)
		{
			var implementedInterface = ReflectionUtility.GetImplementedGenericInterface(type, genericInterfaceType);
			return implementedInterface != null ? implementedInterface.GetGenericArguments()[0] : typeof(object);
		}

		private static Type GetObjectType(object value)
		{
			return value != null ? value.GetType() : typeof(object);
		}

		private object DeserializeWithYamlTypeConverter(EventReader reader, Type type, IYamlTypeConverter converter)
		{
			return converter.ReadYaml(reader.Parser, type);
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
			object result;
			try
			{
				result = Activator.CreateInstance(type);
			}
			catch (MissingMethodException err)
			{
				var message = string.Format("Failed to create an instance of type '{0}'.", type);
				throw new InvalidOperationException(message, err);
			}

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
		public YamlSerializer(YamlSerializerModes mode)
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

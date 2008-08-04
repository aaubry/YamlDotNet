using System;
using System.IO;
using YamlDotNet.Core;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Reads and writes objects from and to YAML.
	/// </summary>
	public class YamlSerializer
	{
		private readonly YamlSerializerOptions options;
		private readonly Type serializedType;

		private bool Roundtrip
		{
			get
			{
				return (options & YamlSerializerOptions.Roundtrip) != 0;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
		/// </summary>
		/// <param name="serializedType">Type of the serialized.</param>
		public YamlSerializer(Type serializedType)
			: this(serializedType, YamlSerializerOptions.Default)
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
		}

		#region Serialization
		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="stream">The stream where to serialize the object.</param>
		/// <param name="o">The object to serialize.</param>
		public void Serialize(Stream stream, object o)
		{
			if(stream == null)
			{
				throw new ArgumentNullException("stream", "The stream is null.");
			}

			using(Emitter emitter = new Emitter(stream))
			{
				emitter.Emit(new StreamStartEvent(Encoding.UTF8));
				emitter.Emit(new DocumentStartEvent());

				SerializeValue(emitter, serializedType, o);

				emitter.Emit(new DocumentEndEvent());
				emitter.Emit(new StreamEndEvent());
			}
		}

		/// <summary>
		/// Serializes the properties of the specified object into a mapping.
		/// </summary>
		/// <param name="emitter">The emitter.</param>
		/// <param name="type">The type of the object.</param>
		/// <param name="o">The o.</param>
		private void SerializeProperties(Emitter emitter, Type type, object o)
		{
			if(Roundtrip && !HasDefaultConstructor(type))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
			}

			emitter.Emit(new MappingStartEvent());

			foreach(PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if(property.CanRead && property.GetGetMethod().GetParameters().Length == 0)
				{
					if(!Roundtrip || property.CanWrite)
					{
						emitter.Emit(new ScalarEvent(property.Name));

						object value = property.GetValue(o, null);
						SerializeValue(emitter, property.PropertyType, value);
					}
				}
			}

			emitter.Emit(new MappingEndEvent());
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

		private static NumberFormatInfo numberFormat = CreateNumberFormatInfo();

		private static NumberFormatInfo CreateNumberFormatInfo()
		{
			NumberFormatInfo format = new NumberFormatInfo();
			format.CurrencyDecimalSeparator = ".";
			format.CurrencyGroupSeparator = "_";
			format.CurrencyGroupSizes = new int[] { 3 };
			format.CurrencySymbol = string.Empty;
			format.CurrencyDecimalDigits = 99;
			format.NumberDecimalSeparator = ".";
			format.NumberGroupSeparator = "_";
			format.NumberGroupSizes = new int[] { 3 };
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
			if(value == null)
			{
				emitter.Emit(new ScalarEvent("", "tag:yaml.org,2002:null", null, ScalarStyle.Plain, false, false));
				return;
			}

			TypeCode typeCode = Type.GetTypeCode(type);
			switch(typeCode)
			{
				case TypeCode.Boolean:
					emitter.Emit(new ScalarEvent(value.ToString(), "tag:yaml.org,2002:bool"));
					break;

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					emitter.Emit(new ScalarEvent(Convert.ToString(value, numberFormat), "tag:yaml.org,2002:int"));
					break;

				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					emitter.Emit(new ScalarEvent(Convert.ToString(value, numberFormat), "tag:yaml.org,2002:float"));
					break;

				case TypeCode.String:
				case TypeCode.Char:
					emitter.Emit(new ScalarEvent(value.ToString(), "tag:yaml.org,2002:str"));
					break;

				case TypeCode.DateTime:
					emitter.Emit(new ScalarEvent(((DateTime)value).ToString("o"), "tag:yaml.org,2002:timestamp"));
					break;

				case TypeCode.DBNull:
				case TypeCode.Empty:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));

				case TypeCode.Object:
				default:
					SerializeProperties(emitter, type, value);
					break;
			}
		}
		#endregion

		#region Deserialization
		/// <summary>
		/// Deserializes an object from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public object Deserialize(Stream stream)
		{
			using(Parser parser = new Parser(stream))
			{
				EventReader reader = new EventReader(parser);
				reader.Expect<StreamStartEvent>().Dispose();
				reader.Expect<DocumentStartEvent>().Dispose();
				object result = DeserializeValue(reader, serializedType);
				reader.Expect<DocumentEndEvent>().Dispose();
				reader.Expect<StreamEndEvent>().Dispose();
				return result;
			}
		}

		private object DeserializeValue(EventReader reader, Type type)
		{
			ScalarEvent scalar = null;
			if(reader.Accept<ScalarEvent>())
			{
				scalar = reader.Expect<ScalarEvent>();

				if(scalar.Tag == "tag:yaml.org,2002:null")
				{
					scalar.Dispose();
					return null;
				}
			}

			try
			{
				TypeCode typeCode = Type.GetTypeCode(type);
				switch(typeCode)
				{
					case TypeCode.Boolean:
						return bool.Parse(scalar.Value);

					case TypeCode.Byte:
						return Byte.Parse(scalar.Value, numberFormat);

					case TypeCode.Int16:
						return Int16.Parse(scalar.Value, numberFormat);

					case TypeCode.Int32:
						return Int32.Parse(scalar.Value, numberFormat);

					case TypeCode.Int64:
						return Int64.Parse(scalar.Value, numberFormat);

					case TypeCode.SByte:
						return SByte.Parse(scalar.Value, numberFormat);

					case TypeCode.UInt16:
						return UInt16.Parse(scalar.Value, numberFormat);

					case TypeCode.UInt32:
						return UInt32.Parse(scalar.Value, numberFormat);

					case TypeCode.UInt64:
						return UInt64.Parse(scalar.Value, numberFormat);

					case TypeCode.Single:
						return Single.Parse(scalar.Value, numberFormat);

					case TypeCode.Double:
						return Double.Parse(scalar.Value, numberFormat);

					case TypeCode.Decimal:
						return Decimal.Parse(scalar.Value, numberFormat);

					case TypeCode.String:
						return scalar.Value;

					case TypeCode.Char:
						return scalar.Value[0];

					case TypeCode.DateTime:
						// TODO: This is probably incorrect. Use the correct regular expression.
						return DateTime.Parse(scalar.Value);

					case TypeCode.DBNull:
					case TypeCode.Empty:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));

					case TypeCode.Object:
					default:
						return DeserializeProperties(reader, type);
				}
			}
			finally
			{
				if(scalar != null)
				{
					scalar.Dispose();
				}
			}
		}

		private object DeserializeProperties(EventReader reader, Type type)
		{
			object result = Activator.CreateInstance(type);

			reader.Expect<MappingStartEvent>().Dispose();
			while(!reader.Accept<MappingEndEvent>())
			{
				using(ScalarEvent key = reader.Expect<ScalarEvent>())
				{
					PropertyInfo property = type.GetProperty(key.Value, BindingFlags.Instance | BindingFlags.Public);
					property.SetValue(result, DeserializeValue(reader, property.PropertyType), null);
				}
			}
			reader.Expect<MappingEndEvent>().Dispose();

			return result;
		}
		#endregion
	}
}
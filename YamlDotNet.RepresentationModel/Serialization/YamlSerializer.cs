using System;
using System.IO;
using YamlDotNet.Core;
using System.Text;
using System.Reflection;
using System.Globalization;
using YamlDotNet.Core.Events;

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
		/// <param name="output">The writer where to serialize the object.</param>
		/// <param name="o">The object to serialize.</param>
		public void Serialize(TextWriter output, object o)
		{
			if (output == null)
			{
				throw new ArgumentNullException("output", "The output is null.");
			}

			Emitter emitter = new Emitter(output);

			emitter.Emit(new StreamStart());
			emitter.Emit(new DocumentStart());

			SerializeValue(emitter, serializedType, o);

			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());
		}

		/// <summary>
		/// Serializes the properties of the specified object into a mapping.
		/// </summary>
		/// <param name="emitter">The emitter.</param>
		/// <param name="type">The type of the object.</param>
		/// <param name="o">The o.</param>
		private void SerializeProperties(Emitter emitter, Type type, object o)
		{
			if (Roundtrip && !HasDefaultConstructor(type))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' cannot be deserialized because it does not have a default constructor.", type));
			}

			emitter.Emit(new MappingStart());

			foreach(var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.CanRead && property.GetGetMethod().GetParameters().Length == 0)
				{
					if (!Roundtrip || property.CanWrite)
					{
						emitter.Emit(new Scalar(property.Name));

						object value = property.GetValue(o, null);
						SerializeValue(emitter, property.PropertyType, value);
					}
				}
			}

			emitter.Emit(new MappingEnd());
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
			if (value == null)
			{
				emitter.Emit(new Scalar("", "tag:            yaml.org,2002:null", null, ScalarStyle.Plain, false, false));
				return;
			}

			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean:
					emitter.Emit(new Scalar("tag:yaml.org,2002:bool", value.ToString()));
					break;

				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					emitter.Emit(new Scalar("tag:yaml.org,2002:int", Convert.ToString(value, numberFormat)));
					break;

				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					emitter.Emit(new Scalar("tag:yaml.org,2002:float", Convert.ToString(value, numberFormat)));
					break;

				case TypeCode.String:
				case TypeCode.Char:
					emitter.Emit(new Scalar("tag:yaml.org,2002:str", value.ToString()));
					break;

				case TypeCode.DateTime:
					emitter.Emit(new Scalar("tag:yaml.org,2002:timestamp", ((DateTime)value).ToString("o")));
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
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public object Deserialize(TextReader input)
		{
			Parser parser = new Parser(input);
			
			EventReader reader = new EventReader(parser);
			reader.Expect<StreamStart>();
			reader.Expect<DocumentStart>();
			object result = DeserializeValue(reader, serializedType);
			reader.Expect<DocumentEnd>();
			reader.Expect<StreamEnd>();
			return result;
		}

		private object DeserializeValue(EventReader reader, Type type)
		{
			Scalar scalar = null;
			if (reader.Accept<Scalar>())
			{
				scalar = reader.Expect<Scalar>();

				if (scalar.Tag == "tag:yaml.org,2002:null")
				{
					return null;
				}
			}

			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode)
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

		private object DeserializeProperties(EventReader reader, Type type)
		{
			object result = Activator.CreateInstance(type);

			reader.Expect<MappingStart>();
			while (!reader.Accept<MappingEnd>())
			{
				Scalar key = reader.Expect<Scalar>();
				PropertyInfo property = type.GetProperty(key.Value, BindingFlags.Instance | BindingFlags.Public);
				property.SetValue(result, DeserializeValue(reader, property.PropertyType), null);
			}
			reader.Expect<MappingEnd>();

			return result;
		}
		#endregion
	}
}
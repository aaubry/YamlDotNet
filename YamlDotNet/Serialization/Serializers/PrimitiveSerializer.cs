using System;
using System.Globalization;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
	internal class PrimitiveSerializer : IYamlSerializable
	{
		public object ReadYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var primitiveType = (PrimitiveDescriptor) typeDescriptor;
			var type = primitiveType.Type;

			var scalar = context.Reader.Expect<Scalar>();
			var text = scalar.Value;

			// Return null if expected type is an object and scalar is null
			if (text == null)
			{
				if (Type.GetTypeCode(type) == TypeCode.Object)
				{
					return null;
				}
				text = string.Empty;
			}

			// Handle string
			if (type == typeof (string))
			{
				return text;
			}

			if (type.IsNumeric())
			{
				// Remove _ character from numeric values
				text = text.Replace("_", string.Empty);

				// Parse default types 
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Byte:
						return byte.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.SByte:
						return sbyte.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Int16:
						return short.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.UInt16:
						return ushort.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Int32:
						return int.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.UInt32:
						return uint.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Int64:
						return long.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.UInt64:
						return ulong.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Single:
						return float.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Double:
						return double.Parse(text, CultureInfo.InvariantCulture);
					case TypeCode.Decimal:
						return decimal.Parse(text, CultureInfo.InvariantCulture);
				}
			}

			// Decode boolean
			if (type == typeof (bool) && context.Settings.Schema.TryParse(scalar, type, out value))
			{
				return value;
			}

			// If type is an enum, try to parse it
			if (type.IsEnum)
			{
				return Enum.Parse(type, text, false);
			}

			// TODO handle timestamp

			// here insert some pluggable IScalarConverter

			throw new YamlException(scalar.Start, scalar.End, "Unable to decode scalar [{0}] not supported by current schema".DoFormat(scalar));
		}

		public void WriteYaml(SerializerContext context, object value, ITypeDescriptor typeDescriptor)
		{
			var primitiveType = (PrimitiveDescriptor)typeDescriptor;
			var type = primitiveType.Type;

			// Return null if expected type is an object and scalar is null
			if (value == null)
			{
				context.Writer.Emit(new ScalarEventInfo(null, type) { RenderedValue = null});
				return;
			}

			var valueType = value.GetType();
			var tag = valueType != type ? context.TagFromType(valueType) : null;


			string text = null;

			// Handle string
			text = value as string;
			if (text == null)
			{
				if (valueType.IsNumeric())
				{
					// Parse default types 
					switch (Type.GetTypeCode(valueType))
					{
						case TypeCode.Boolean:
							text = (bool) value ? "true" : "false";
							break;
						case TypeCode.Byte:
							text = ((byte) value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.SByte:
							text = ((sbyte)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Int16:
							text = ((short)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.UInt16:
							text = ((ushort)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Int32:
							text = ((int)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.UInt32:
							text = ((uint)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Int64:
							text = ((long)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.UInt64:
							text = ((ulong)value).ToString("G", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Single:
							text = ((float)value).ToString("R", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Double:
							text = ((double)value).ToString("R", CultureInfo.InvariantCulture);
							break;
						case TypeCode.Decimal:
							text = ((decimal)value).ToString("R", CultureInfo.InvariantCulture);
							break;
					}
				}
				else if (valueType.IsEnum)
				{
					text = ((Enum)Enum.ToObject(valueType, value)).ToString("G");
				}
				// TODO handle timestamp
			}

			if (text == null)
			{
				throw new YamlException("Unable to serialize scalar [{0}] not supported".DoFormat(value));	
			}

			context.Writer.Emit(new ScalarEventInfo(value, type) { RenderedValue = text, Tag = tag });
		}
	}
}
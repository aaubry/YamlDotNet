using System;
using System.Globalization;
using YamlDotNet.Events;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Processors
{
	public class PrimitiveProcessor : IYamlProcessor
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
			
		}
	}
}
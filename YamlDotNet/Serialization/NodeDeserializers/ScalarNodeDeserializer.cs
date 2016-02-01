// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Serialization.NodeDeserializers
{
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

            if (expectedType.IsEnum())
            {
                value = Enum.Parse(expectedType, scalar.Value);
            }
            else
            {
                TypeCode typeCode = expectedType.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        value = bool.Parse(scalar.Value);
                        break;

                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        value = DeserializeIntegerHelper(typeCode, scalar.Value, YamlFormatter.NumberFormat);
                        break;

                    case TypeCode.Single:
                        value = Single.Parse(scalar.Value, YamlFormatter.NumberFormat);
                        break;

                    case TypeCode.Double:
                        value = Double.Parse(scalar.Value, YamlFormatter.NumberFormat);
                        break;

                    case TypeCode.Decimal:
                        value = Decimal.Parse(scalar.Value, YamlFormatter.NumberFormat);
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
                            value = TypeConverter.ChangeType(scalar.Value, expectedType);
                        }
                        break;
                }
            }
            return true;
        }

        private object DeserializeIntegerHelper(TypeCode typeCode, string value, IFormatProvider formatProvider)
        {
            StringBuilder numberBuilder = new StringBuilder();
            int currentIndex = 0;
            bool isNegative = false;
            int numberBase = 0;
            long result = 0;

            if (value[0] == '-')
            {
                currentIndex++;
                isNegative = true;
            }

            else if (value[0] == '+')
            {
                currentIndex++;
            }

            if (value[currentIndex] == '0')
            {
                // Could be binary, octal, hex, decimal (0)

                // If there are no characters remaining, it's a decimal zero
                if (currentIndex == value.Length - 1)
                {
                    numberBase = 10;
                    result = 0;
                }

                else
                {
                    // Check the next character
                    currentIndex++;

                    if (value[currentIndex] == 'b')
                    {
                        // Binary
                        numberBase = 2;

                        currentIndex++;
                    }

                    else if (value[currentIndex] == 'x')
                    {
                        // Hex
                        numberBase = 16;

                        currentIndex++;
                    }

                    else
                    {
                        // Octal
                        numberBase = 8;
                    }
                }

                // Copy remaining digits to the number buffer (skip underscores)
                while (currentIndex < value.Length)
                {
                    if (value[currentIndex] != '_')
                    {
                        numberBuilder.Append(value[currentIndex]);
                    }
                    currentIndex++;
                }

                // Parse the magnitude of the number
                switch (numberBase)
                {
                    case 2:
                    case 8:
                        // TODO: how to incorporate the numberFormat?
                        result = Convert.ToInt64(numberBuilder.ToString(), numberBase);
                        break;

                    case 16:
                        result = Int64.Parse(numberBuilder.ToString(), NumberStyles.HexNumber, YamlFormatter.NumberFormat);
                        break;

                    case 10:
                        // Result is already zero
                        break;
                }
            }

            else
            {
                // Could be decimal or base 60
                string[] chunks = value.Substring(currentIndex).Split(':');
                result = 0;

                for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
                {
                    result *= 60;

                    // TODO: verify that chunks after the first are non-negative and less than 60
                    result += long.Parse(chunks[chunkIndex].Replace("_", ""));
                }
            }

            if (isNegative)
            {
                result = -result;
            }

            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (byte)result;

                case TypeCode.Int16:
                    return (short)result;

                case TypeCode.Int32:
                    return (int)result;

                case TypeCode.Int64:
                    return result;

                case TypeCode.SByte:
                    return (sbyte)result;

                case TypeCode.UInt16:
                    return (ushort)result;

                case TypeCode.UInt32:
                    return (uint)result;

                case TypeCode.UInt64:
                    return (ulong)result;

                default:
                    return result;
            }
        }
    }
}

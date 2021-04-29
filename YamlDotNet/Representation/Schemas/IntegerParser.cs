//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

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
using System.Globalization;

namespace YamlDotNet.Representation.Schemas
{
    /// <summary>
    /// Implementation of integer parsing for various bases.
    /// </summary>
    /// <remarks>
    /// These implementations assume that the input is well formed and therefore
    /// are not suitabble for general-purpose use. This is fine in our use-case
    /// since we use a regular expression to select the parsing function based on
    /// the value.
    /// DO NOT reuse this code unless you know what you are doing.
    /// </remarks>
    internal static class IntegerParser
    {
        // Assumes that value has the form: ^[-+]?0b[0-1_]+$
        public static long ParseBase2(string value)
        {
            int idx;
            long parsed;
            if (value[0] == '-')
            {
                parsed = -1;
                idx = 3;
            }
            else if (value[0] == '+')
            {
                parsed = 1;
                idx = 3;
            }
            else
            {
                parsed = 1;
                idx = 2;
            }

            for (; idx < value.Length; ++idx)
            {
                var chr = value[idx];
                if (chr != '_' && chr != '0')
                {
                    goto parse_non_zero;
                }
            }

            return 0;

        parse_non_zero:

            // If we reach this point, the next digit is necessarily a 1,
            // and was already accounted for during initialization.
            ++idx;

            try
            {
                if (parsed > 0)
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 2 + (chr & 1));
                        }
                    }
                }
                else
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 2 - (chr & 1));
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        // Assumes that value has the form: ^[-+]?0[0-7_]+$
        public static long ParseBase8(string value)
        {
            int idx;
            bool isPositive;

            if (value[0] == '-')
            {
                isPositive = false;
                idx = 2;
            }
            else if (value[0] == '+')
            {
                isPositive = true;
                idx = 2;
            }
            else
            {
                isPositive = true;
                idx = 1;
            }

            long parsed = 0;
            try
            {
                if (isPositive)
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 8 + (chr & 0b111));
                        }
                    }
                }
                else
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 8 - (chr & 0b111));
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        // Assumes that value has the form: ^0o[0-7]+$
        public static ulong ParseBase8Unsigned(string value)
        {
            ulong parsed = 0;
            try
            {
                for (var idx = 2; idx < value.Length; ++idx)
                {
                    parsed = checked(parsed * 8UL + (ulong)(value[idx] & 0b111));
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        // Assumes that value has the form: ^[-+]?(0|[1-9][0-9_]*)$
        public static long ParseBase10(string value)
        {
            int idx;
            bool isPositive;

            if (value[0] == '-')
            {
                isPositive = false;
                idx = 1;
            }
            else if (value[0] == '+')
            {
                isPositive = true;
                idx = 1;
            }
            else
            {
                isPositive = true;
                idx = 0;
            }

            long parsed = 0;
            try
            {
                if (isPositive)
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 10 + (chr & 0x0f));
                        }
                    }
                }
                else
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 10 - (chr & 0x0f));
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        // Assumes that value has the form: ^[-+]?0x[0-9a-fA-F_]+$
        public static long ParseBase16(string value)
        {
            int idx;
            bool isPositive;

            if (value[0] == '-')
            {
                isPositive = false;
                idx = 3;
            }
            else if (value[0] == '+')
            {
                isPositive = true;
                idx = 3;
            }
            else
            {
                isPositive = true;
                idx = 2;
            }

            long parsed = 0;
            try
            {
                if (isPositive)
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 16 + (chr & 0x0f));
                            if (chr > '9')
                            {
                                parsed = checked(parsed + 9);
                            }
                        }
                    }
                }
                else
                {
                    for (; idx < value.Length; ++idx)
                    {
                        var chr = value[idx];
                        if (chr != '_')
                        {
                            parsed = checked(parsed * 16 - (chr & 0x0f));
                            if (chr > '9')
                            {
                                parsed = checked(parsed - 9);
                            }
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        // Assumes that value has the form: ^0x[0-9a-fA-F]+$
        public static ulong ParseBase16Unsigned(string value)
        {
            ulong parsed = 0;
            try
            {
                for (var idx = 2; idx < value.Length; ++idx)
                {
                    var chr = value[idx];
                    if (chr != '_')
                    {
                        parsed = checked(parsed * 16UL + (ulong)(chr & 0x0f));
                        if (chr > '9')
                        {
                            parsed = checked(parsed + 9UL);
                        }
                    }
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }

        public static long ParseBase60(string value) => ParseBase60(value, value.Length, out _);

        // Assumes that value has the form: ^[-+]?[1-9][0-9_]*(:[0-5]?[0-9])+$
        public static long ParseBase60(string value, int length, out bool isPositive)
        {
            int idx;

            if (value[0] == '-')
            {
                isPositive = false;
                idx = 1;
            }
            else if (value[0] == '+')
            {
                isPositive = true;
                idx = 1;
            }
            else
            {
                isPositive = true;
                idx = 0;
            }

            long parsed = 0;
            try
            {
                for (; idx < length; ++idx)
                {
                    var chr = value[idx];
                    if (chr == '_')
                    {
                        continue;
                    }
                    if (chr == ':')
                    {
                        // Parse all groups except the last one
                        while (idx < length - 3)
                        {
                            var first = value[idx + 1];
                            var second = value[idx + 2];
                            if (second == ':')
                            {
                                parsed = parsed * 60 + (first & 0x0f);
                                idx += 2;
                            }
                            else
                            {
                                parsed = parsed * 60 + (first & 0x0f) * 10 + (second & 0x0f);
                                idx += 3;
                            }
                        }

                        // Parse the remaining group
                        {
                            long remainder = value[idx + 1] & 0x0f;
                            if (idx + 2 < length)
                            {
                                remainder = remainder * 10 + (value[idx + 2] & 0x0f);
                            }

                            parsed = isPositive
                                ? checked(parsed * 60 + remainder)
                                : checked(parsed * -60 - remainder);
                        }

                        break;
                    }

                    parsed = checked(parsed * 10 + (chr & 0x0f));
                }

                // Note that since there must always be at least a segagesimal group,
                // therefore when we reach this point, we have always entered the negative value handling
                // from the 'parse the remaining group' block.
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was too large for an integer.");
            }

            return parsed;
        }
    }

    internal static class IntegerFormatter
    {
        public static string FormatBase10(object? native)
        {
            return Convert.ToString(native, NumberFormat.Default)!;
        }

        public static string FormatBase2Signed(object? native)
        {
            var value = Convert.ToInt64(native);

            if (value == 0)
            {
                return "0b0";
            }

            var isNegative = value < 0;
            var absoluteValue = isNegative ?
                ~(ulong)value + 1UL
                : (ulong)value;

            const int digits = 64 + 2 + 1;
            var text = new char[digits];
            var idx = FormatBase2Unsigned(absoluteValue, text);

            text[--idx] = 'b';
            text[--idx] = '0';
            if (isNegative)
            {
                text[--idx] = '-';
            }

            return new string(text, idx, digits - idx);
        }

        private static int FormatBase2Unsigned(ulong value, char[] text)
        {
            var idx = text.Length;
            var block = (uint)value;
            if (value > 0xFFFFFFFFUL)
            {
                for (int i = 0; i < 32; ++i)
                {
                    --idx;

                    var chr = (block & 1) + '0';
                    text[idx] = (char)chr;
                    block >>= 1;
                }

                block = (uint)(value >> 32);
            }

            while (block != 0)
            {
                --idx;

                var chr = (block & 1) + '0';
                text[idx] = (char)chr;
                block >>= 1;
            }

            return idx;
        }

        public static string FormatBase8Unsigned(object? native)
        {
            var value = Convert.ToUInt64(native);

            if (value == 0)
            {
                return "0o0";
            }

            const int digits = 22 + 2;
            var text = new char[digits];
            var idx = FormatBase8Unsigned(value, text);

            text[--idx] = 'o';
            text[--idx] = '0';

            return new string(text, idx, digits - idx);
        }

        public static string FormatBase8Signed(object? native)
        {
            var value = Convert.ToInt64(native);

            if (value == 0)
            {
                return "0o0";
            }

            var isNegative = value < 0;
            var absoluteValue = isNegative ?
                ~(ulong)value + 1UL
                : (ulong)value;

            const int digits = 22 + 2 + 1;
            var text = new char[digits];
            var idx = FormatBase8Unsigned(absoluteValue, text);

            text[--idx] = 'o';
            text[--idx] = '0';
            if (isNegative)
            {
                text[--idx] = '-';
            }

            return new string(text, idx, digits - idx);
        }

        private static int FormatBase8Unsigned(ulong value, char[] text)
        {
            var idx = text.Length;

            var block = (uint)value & 0x00FFFFFFU;
            while (value > 0x00FFFFFFUL)
            {
                for (int i = 0; i < 8; ++i)
                {
                    --idx;
                    text[idx] = (char)(block % 8 + '0');
                    block /= 8;
                }

                value >>= 24;
                block = (uint)value & 0x00FFFFFFU;
            }

            while (block != 0)
            {
                --idx;
                text[idx] = (char)(block % 8 + '0');
                block /= 8;
            }

            return idx;
        }

        public static string FormatBase16Unsigned(object? native)
        {
            var value = Convert.ToUInt64(native);
            if (value == 0)
            {
                return "0x0";
            }

            const int digits = 16 + 2;
            var text = new char[digits];
            var idx = FormatBase16Unsigned(value, text);

            text[--idx] = 'x';
            text[--idx] = '0';

            return new string(text, idx, digits - idx);
        }

        public static string FormatBase16Signed(object? native)
        {
            var value = Convert.ToInt64(native);

            if (value == 0)
            {
                return "0x0";
            }

            var isNegative = value < 0;
            var absoluteValue = isNegative ?
                ~(ulong)value + 1UL
                : (ulong)value;

            const int digits = 16 + 2 + 1;
            var text = new char[digits];
            var idx = FormatBase16Unsigned(absoluteValue, text);

            text[--idx] = 'x';
            text[--idx] = '0';
            if (isNegative)
            {
                text[--idx] = '-';
            }

            return new string(text, idx, digits - idx);
        }

        private static int FormatBase16Unsigned(ulong value, char[] text)
        {
            var idx = text.Length;
            var block = (uint)value;
            if (value > 0xFFFFFFFFUL)
            {
                for (int i = 0; i < 8; ++i)
                {
                    --idx;

                    var chr = (block & 0xFU) + '0';
                    if (chr > '9')
                    {
                        chr += 'A' - '9' - 1;
                    }

                    text[idx] = (char)chr;
                    block /= 16;
                }

                block = (uint)(value >> 32);
            }

            while (block != 0)
            {
                --idx;

                var chr = (block & 0xFU) + '0';
                if (chr > '9')
                {
                    chr += 'A' - '9' - 1;
                }

                text[idx] = (char)chr;
                block /= 16;
            }

            return idx;
        }

        public static string FormatBase60Signed(object? native)
        {
            var value = Convert.ToInt64(native);

            if (value == 0)
            {
                return "0";
            }

            var isNegative = value < 0;
            var absoluteValue = isNegative ?
                ~(ulong)value + 1UL
                : (ulong)value;

            const int digits = 32 + 1;
            var text = new char[digits];
            var idx = digits;
            while (true)
            {
                var segment = (int)(absoluteValue % 60);
                if (segment > 10)
                {
                    text[--idx] = (char)(segment % 10 + '0');
                    segment /= 10;
                }
                text[--idx] = (char)(segment + '0');

                absoluteValue /= 60;
                if (absoluteValue == 0)
                {
                    break;
                }

                text[--idx] = ':';
            }

            if (isNegative)
            {
                text[--idx] = '-';
            }

            return new string(text, idx, digits - idx);
        }
    }

    internal static class NumberFormat
    {
        public static readonly NumberFormatInfo Default = new NumberFormatInfo
        {
            NumberDecimalSeparator = ".",
            NumberGroupSeparator = "_",
            NumberGroupSizes = new[] { 3 },
            NumberDecimalDigits = 16,
            NumberNegativePattern = 1, // -1,234.00
            NegativeSign = "-",
            PositiveSign = "+",
            NaNSymbol = ".nan",
            PositiveInfinitySymbol = ".inf",
            NegativeInfinitySymbol = "-.inf",
        };
    }
}
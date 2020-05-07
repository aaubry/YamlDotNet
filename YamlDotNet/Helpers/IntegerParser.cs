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

namespace YamlDotNet.Helpers
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
                            parsed = checked((parsed * 2) + (chr & 1));
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
                            parsed = checked((parsed * 2) - (chr & 1));
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
                            parsed = checked((parsed * 8) + (chr & 0b111));
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
                            parsed = checked((parsed * 8) - (chr & 0b111));
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
                    parsed = checked((parsed * 8UL) + (ulong)(value[idx] & 0b111));
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
                            parsed = checked((parsed * 10) + (chr & 0x0f));
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
                            parsed = checked((parsed * 10) - (chr & 0x0f));
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
                            parsed = checked((parsed * 16) + (chr & 0x0f));
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
                            parsed = checked((parsed * 16) - (chr & 0x0f));
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
                        parsed = checked((parsed * 16UL) + (ulong)(chr & 0x0f));
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
}
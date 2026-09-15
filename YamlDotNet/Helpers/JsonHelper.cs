// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Text;

namespace YamlDotNet.Helpers
{
    internal static class JsonHelper
    {
        public static bool IsJsonLiteral(StringBuilder s)
        {
            var l = s.Length;

            // Check whether the value is the literal "null", "true", or "false".
            switch (l)
            {
                case 0:
                    return false;

                case 4 when s[0] == 'n' && s[1] == 'u' && s[2] == 'l' && s[3] == 'l':
                case 4 when s[0] == 't' && s[1] == 'r' && s[2] == 'u' && s[3] == 'e':
                case 5 when s[0] == 'f' && s[1] == 'a' && s[2] == 'l' && s[3] == 's' && s[4] == 'e':
                    return true;
            }

            // Check whether the value represents a valid number.
            var i = 0;
            var hasDot = false;
            var hasExponent = false;

            // The first character after an optional minus sign must be a digit.
            // Leading zeros are not allowed.
            if (s[i] == '-' && ++i == l || (uint)(s[i] - '0') > 9 || s[i] == '0' && ++i < l && (uint)(s[i] - '0') <= 9)
            {
                return false;
            }

            for (; i < l; i++)
            {
                var c = s[i];
                if (c == '.')
                {
                    // The number may contain only a single decimal point,
                    // which must be followed by at least one digit.
                    if (hasDot || ++i == l || (uint)(s[i] - '0') > 9)
                    {
                        return false;
                    }

                    hasDot = true;
                }
                else if ((c | 0x20) == 'e')
                {
                    // Skip an optional plus or minus sign.
                    if (i + 1 < l && s[i + 1] is '+' or '-')
                    {
                        i++;
                    }

                    // The number may contain only a single exponent,
                    // which must be be followed by at least one digit.
                    if (hasExponent || i + 1 >= l)
                    {
                        return false;
                    }

                    hasDot = true;
                    hasExponent = true;
                }
                else if ((uint)(c - '0') > 9)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

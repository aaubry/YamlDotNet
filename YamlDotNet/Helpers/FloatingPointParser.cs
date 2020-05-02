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

namespace YamlDotNet.Helpers
{
    internal static class FloatingPointParser
    {
        private static readonly IFormatProvider format = new NumberFormatInfo
        {
            NegativeSign = "-",
            PositiveSign = "+",
            NumberDecimalSeparator = ".",
        };

        // Assumes that value has the form: ^[-+]?([0-9][0-9]*)?\.[0-9]*([eE][-+][0-9]+)?$
        public static double ParseBase10Unseparated(string value)
        {
            return double.Parse(value, format);
        }

        // Assumes that value has the form: ^[-+]?([0-9][0-9_]*)?\.[0-9_]*([eE][-+][0-9]+)?$
        public static double ParseBase10Separated(string value)
        {
            return double.Parse(value.Replace("_", ""), format);
        }

        // Assumes that value has the form: ^[-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\.[0-9_]*$
        public static double ParseBase60(string value)
        {
            var dotIndex = value.IndexOf('.');
            var integralPart = IntegerParser.ParseBase60(value, dotIndex, out var isPositive);
            if (dotIndex == value.Length - 1)
            {
                return integralPart;
            }

            var fractionalPart = ParseBase10Separated(value.Substring(dotIndex));
            return isPositive
                ? integralPart + fractionalPart
                : integralPart - fractionalPart;
        }
    }
}
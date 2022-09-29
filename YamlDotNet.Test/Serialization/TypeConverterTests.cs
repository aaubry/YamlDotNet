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

using Xunit;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Test.Serialization
{
    public class TypeConverterTests
    {
        public class ImplicitConversionIntWrapper
        {
            public readonly int value;

            public ImplicitConversionIntWrapper(int value)
            {
                this.value = value;
            }

            public static implicit operator int(ImplicitConversionIntWrapper wrapper)
            {
                return wrapper.value;
            }
        }

        public class ExplicitConversionIntWrapper
        {
            public readonly int value;

            public ExplicitConversionIntWrapper(int value)
            {
                this.value = value;
            }

            public static explicit operator int(ExplicitConversionIntWrapper wrapper)
            {
                return wrapper.value;
            }
        }

        [Fact]
        public void Implicit_conversion_operator_is_used()
        {
            var data = new ImplicitConversionIntWrapper(2);
            var actual = TypeConverter.ChangeType<int>(data);
            Assert.Equal(data.value, actual);
        }

        [Fact]
        public void Explicit_conversion_operator_is_used()
        {
            var data = new ExplicitConversionIntWrapper(2);
            var actual = TypeConverter.ChangeType<int>(data);
            Assert.Equal(data.value, actual);
        }
    }
}

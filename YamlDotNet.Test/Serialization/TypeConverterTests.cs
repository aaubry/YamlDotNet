using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

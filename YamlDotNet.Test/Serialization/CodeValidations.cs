using System;
using System.Linq;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Utilities;

namespace YamlDotNet.Test.Serialization
{
    public class CodeValidations
    {
        [Fact]
        public void AllBuiltInConvertersAreRegistered()
        {
            var interfaceType = typeof(IYamlTypeConverter);
            var converterTypes = interfaceType.Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && interfaceType.IsAssignableFrom(t));

            var unregisteredTypes = converterTypes
                .Where(t => !YamlTypeConverters.GetBuiltInConverters(false).Any(c => c.GetType() == t))
                .ToArray();

            Assert.Equal(new Type[0], unregisteredTypes);
        }
    }
}

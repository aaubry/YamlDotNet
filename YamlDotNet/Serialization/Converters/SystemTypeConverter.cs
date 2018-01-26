using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Serialization.Converters
{
    /// <summary>
    /// Converter for System.Type.
    /// </summary>
    /// <remarks>
    /// Converts <see cref="System.Type" /> to a scalar containing the assembly qualified name of the type.
    /// </remarks>
    public class SystemTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(Type).IsAssignableFrom(type);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var value = ((Scalar)parser.Current).Value;
            parser.MoveNext();
            return Type.GetType(value, throwOnError: true);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var systemType = (Type)value;
            emitter.Emit(new Scalar(null, null, systemType.AssemblyQualifiedName, ScalarStyle.Any, true, false));
        }
    }
}

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
            var value = parser.Consume<Scalar>().Value;
            return Type.GetType(value, throwOnError: true)!; // Will throw instead of returning null
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var systemType = (Type)value!;
            emitter.Emit(new Scalar(AnchorName.Empty, null, systemType.AssemblyQualifiedName!, ScalarStyle.Any, true, false));
        }
    }
}

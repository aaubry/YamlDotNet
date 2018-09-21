using System;
using System.IO;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization
{
    public interface IDeserializer
    {
        T Deserialize<T>(string input);
        T Deserialize<T>(TextReader input);
        object Deserialize(TextReader input);
        object Deserialize(string input, Type type);
        object Deserialize(TextReader input, Type type);
        T Deserialize<T>(IParser parser);
        object Deserialize(IParser parser);

        /// <summary>
        /// Deserializes an object of the specified type.
        /// </summary>
        /// <param name="parser">The <see cref="IParser" /> from where to deserialize the object.</param>
        /// <param name="type">The static type of the object to deserialize.</param>
        /// <returns>Returns the deserialized object.</returns>
        object Deserialize(IParser parser, Type type);
    }
}
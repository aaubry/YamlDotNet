using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    /// <summary>
    /// An ITypeDiscriminator provides an interface for discriminating which dotnet type to deserialize a yaml
    /// stream into. They require the yaml stream to be buffered <see cref="TypeDiscriminatingNodeDeserializer" /> as they
    /// can inspect the yaml value, determine the desired type, and reset the yaml stream to then deserialize into
    /// that type.
    /// </summary>
    public interface ITypeDiscriminator
    {
        /// <summary>
        /// Gets the BaseType of the discriminator. All types that an ITypeDiscriminator may discriminate into must
        /// inherit from this type. This enables the deserializer to only buffer values of matching types.
        /// If you would like an ITypeDiscriminator to discriminate all yaml values, the BaseType will be object.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Trys to discriminate a type from the current IParser. As discriminating the type will consume the parser, the
        /// parser will usually need to be a buffer so an instance of the discriminated type can be deserialized later.
        /// </summary>
        /// <param name="buffer">The IParser to consume and discriminate a type from.</param>
        /// <param name="suggestedType">The output type discriminated. Null if no type matched the discriminator.</param>
        /// <returns>Returns true if the discriminator matched the yaml stream.</returns>
        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }
}
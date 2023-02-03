using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators
{
    public interface ITypeDiscriminator
    {
        Type BaseType { get; }

        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }
}
using System;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.BufferedDeserialization
{
    public interface IValueTypeDiscriminator
    {
        Type BaseType { get; }

        bool TryDiscriminate(IParser buffer, out Type? suggestedType);
    }
}
using System;
using YamlDotNet.Serialization;

namespace YamlDotNet.Helpers
{
    public static class FsharpHelper
    {
        private static bool IsFsharp(Type t)
        {
            return t.Namespace == "Microsoft.FSharp.Core";
        }

        public static bool IsOptionType(Type t)
        {
            return IsFsharp(t) && t.Name == "FSharpOption`1";
        }

        public static Type? GetOptionUnderlyingType(Type t)
        {
            return t.IsGenericType && IsOptionType(t) ? t.GenericTypeArguments[0] : null;
        }

        public static object? GetValue(IObjectDescriptor objectDescriptor)
        {
            if (!IsOptionType(objectDescriptor.Type))
            {
                throw new InvalidOperationException("Should not be called on non-Option<> type");
            }

            if (objectDescriptor.Value is null)
            {
                return null;
            }

            return objectDescriptor.Type.GetProperty("Value").GetValue(objectDescriptor.Value);
        }
    }
}

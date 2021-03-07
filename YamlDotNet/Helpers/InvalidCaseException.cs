using System;

namespace YamlDotNet.Helpers
{
    internal static class Invariants
    {
        public static Exception InvalidCase<T>(T value) where T : notnull
        {
            return new Exception(
                $"The value '{value}' of type '{value.GetType().FullName}' was not expected as a specialization of '{typeof(T).FullName}'" +
                "\nThis probably indicates a bug in this library; please report it."
            );
        }

        public static T ValidEnum<T>(T value, string paramName) where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentOutOfRangeException(paramName, $"The value '{value}' is not value for enum '{typeof(T).FullName}'");
            }

            return value;
        }
    }
}
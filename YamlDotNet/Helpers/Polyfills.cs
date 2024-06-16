namespace YamlDotNet
{
    internal static class Polyfills
    {
#if NETFRAMEWORK || NETSTANDARD2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static bool Contains(this string source, char c) => source.IndexOf(c) != -1;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static bool EndsWith(this string source, char c) => source.Length > 0 && source[source.Length - 1] == c;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static bool StartsWith(this string source, char c) => source.Length > 0 && source[0] == c;
#endif
    }
}

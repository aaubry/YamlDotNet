namespace YamlDotNet.Helpers
{
    internal static class NumberExtensions
    {
        public static bool IsPowerOfTwo(this int value)
        {
            return (value & (value - 1)) == 0;
        }
    }
}

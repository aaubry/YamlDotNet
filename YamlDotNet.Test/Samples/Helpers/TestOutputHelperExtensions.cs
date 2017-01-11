using Xunit.Abstractions;

namespace YamlDotNet.Test.Samples.Helpers
{
    public static class TestOutputHelperExtensions
    {
        public static void WriteLine(this ITestOutputHelper output)
        {
            output.WriteLine(string.Empty);
        }
    }
}

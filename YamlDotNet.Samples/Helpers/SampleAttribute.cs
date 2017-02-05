using Xunit;

namespace YamlDotNet.Samples.Helpers
{
    /// <summary>
    /// Marks a test as being a code sample.
    /// </summary>
    public class SampleAttribute : FactAttribute
    {
        public string Description { get; set; }
    }
}

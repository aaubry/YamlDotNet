namespace YamlDotNet.Samples.Helpers
{
    /// <summary>
    /// Marks a test as being a code sample.
    /// </summary>
    internal class SampleAttribute : System.Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}

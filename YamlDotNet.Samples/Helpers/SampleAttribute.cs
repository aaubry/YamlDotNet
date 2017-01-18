using Xunit;

namespace YamlDotNet.Samples.Helpers
{
    /// <summary>
    /// Marks a test as being a code sample.
    /// </summary>
    internal class SampleAttribute : FactAttribute
    {
        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                DisplayName = "Sample: " + value;
            }
        }

        public string Description { get; set; }
    }
}

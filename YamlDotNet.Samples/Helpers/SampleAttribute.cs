using System;

namespace YamlDotNet.Samples.Helpers
{
    /// <summary>
    /// Marks a test as being a code sample.
    /// </summary>
    internal class SampleAttribute : Attribute
    {
        private string title;

        public string DisplayName { get; private set; }

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

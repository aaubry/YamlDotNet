namespace build
{
    internal static class GitHubApiModels
    {
        public class Release
        {
            public string? html_url { get; set; }
            public string? body { get; set; }
        }

        public class Issue
        {
        }
    }
}

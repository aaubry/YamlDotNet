using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace YamlDotNet.Test
{
    static class ColoredConsole
    {
        private static readonly Regex FormatParser = new Regex(@"(?:(?<text>[^{]*)\{\s*(?<index>\d+)\s*(?::(?<foreground>[\w]+))?\s*(?:,\s*(?<background>[\w]+))?(?<format>[^}]*)\}|(?<text>[^{]+))", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        private static readonly object ConsoleMutex = new object();

        public static readonly object Reset = new object();

        public static void WriteLine(FormattableString text)
        {
            lock (ConsoleMutex)
            {
                WriteImpl(text);
                Console.WriteLine();
            }
        }

        public static void Write(FormattableString text)
        {
            lock (ConsoleMutex)
            {
                WriteImpl(text);
            }
        }

        private static void WriteImpl(FormattableString text)
        {
            var matches = FormatParser.Matches(text.Format);
            foreach (var match in matches.Cast<Match>())
            {
                Console.Write(match.Groups["text"].Value);

                if (TryGetGroup(match, "index", ParseInt, out var index))
                {
                    var argument = text.GetArgument(index);

                    if (TryGetGroup(match, "foreground", ParseColor, out var foreground))
                    {
                        Console.ForegroundColor = foreground;
                    }

                    if (TryGetGroup(match, "background", ParseColor, out var background))
                    {
                        Console.BackgroundColor = background;
                    }

                    var format = match.Groups["format"].Value;
                    Console.Write($"{{0{format}}}", argument);
                    Console.ResetColor();
                }
            }
        }

        private static int ParseInt(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static ConsoleColor ParseColor(string value) => (ConsoleColor)Enum.Parse(typeof(ConsoleColor), value, true);

        private static bool TryGetGroup(Match match, string name, [NotNullWhen(true)] out string? value)
        {
            var group = match.Groups[name];
            if (group.Success)
            {
                value = group.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        private static bool TryGetGroup<T>(Match match, string name, Func<string, T> parser, [MaybeNullWhen(false)] out T value)
        {
            if (TryGetGroup(match, name, out var valueAsString))
            {
                value = parser(valueAsString);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}

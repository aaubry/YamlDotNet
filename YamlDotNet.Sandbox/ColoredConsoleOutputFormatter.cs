using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.Sandbox
{
    class ColoredConsoleOutputFormatter : IOutputFormatter
    {
        private readonly Stack<(ConsoleColor foreground, ConsoleColor background)> colorState = new Stack<(ConsoleColor foreground, ConsoleColor background)>();

        private void PushColor(ConsoleColor foreground) => PushColor(foreground, Console.BackgroundColor);
        private void PushColor(ConsoleColor foreground, ConsoleColor background)
        {
            colorState.Push((Console.ForegroundColor, Console.BackgroundColor));
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
        }

        private void PopColor()
        {
            var (foreground, background) = colorState.Pop();
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
        }

        public void AliasStart() => PushColor(ConsoleColor.DarkBlue);
        public void AliasEnd() => PopColor();
        public void AnchorStart() => PushColor(ConsoleColor.Blue);
        public void AnchorEnd() => PopColor();
        public void BlockScalarHintIndicatorStart() { }
        public void BlockScalarHintIndicatorEnd() { }
        public void BlockSequenceItemIndicatorEnd() { }
        public void BlockSequenceEnd() { }
        public void BlockSequenceStart() { }
        public void BlockSequenceItemIndicatorStart() { }
        public void DirectiveStart() { }
        public void DirectiveEnd() { }
        public void DocumentEndIndicatorStart() { }
        public void DocumentEndIndicatorEnd() { }
        public void DocumentSeparatorIndicatorStart() { }
        public void DocumentSeparatorIndicatorEnd() { }
        public void DocumentStart() { }
        public void DocumentEnd() { }
        public void DocumentStartIndicatorStart() { }
        public void DocumentStartIndicatorEnd() { }
        public void FlowMappingStartIndicatorStart() => PushColor(ConsoleColor.White);
        public void FlowMappingStartIndicatorEnd() => PopColor();
        public void FlowMappingEndIndicatorStart() => PushColor(ConsoleColor.White);
        public void FlowMappingEndIndicatorEnd() => PopColor();
        public void FlowMappingSeparatorStart() => PushColor(ConsoleColor.White);
        public void FlowMappingSeparatorEnd() => PopColor();
        public void FlowMappingStart() { }
        public void FlowMappingEnd() { }
        public void FlowSequenceStartIndicatorStart() => PushColor(ConsoleColor.White);
        public void FlowSequenceStartIndicatorEnd() => PopColor();
        public void FlowSequenceEndIndicatorStart() => PushColor(ConsoleColor.White);
        public void FlowSequenceEndIndicatorEnd() => PopColor();
        public void FlowSequenceSeparatorStart() => PushColor(ConsoleColor.White);
        public void FlowSequenceSeparatorEnd() => PopColor();
        public void FlowSequenceEnd() { }
        public void FlowSequenceStart() { }
        public void MappingKeyStart() => PushColor(ConsoleColor.DarkMagenta);
        public void MappingKeyEnd() => PopColor();
        public void MappingKeyIndicatorStart() => PushColor(ConsoleColor.Green);
        public void MappingKeyIndicatorEnd() => PopColor();
        public void MappingValueIndicatorStart() => PushColor(ConsoleColor.Green);
        public void MappingValueIndicatorEnd() => PopColor();
        public void MappingValueStart() => PushColor(ConsoleColor.Gray);
        public void MappingValueEnd() => PopColor();

        public void ScalarStart(ScalarStyle style)
        {
            switch (style)
            {
                case ScalarStyle.SingleQuoted:
                case ScalarStyle.DoubleQuoted:
                    PushColor(ConsoleColor.DarkYellow);
                    break;

                case ScalarStyle.Literal:
                case ScalarStyle.Folded:
                    PushColor(ConsoleColor.DarkCyan);
                    break;
            }
        }

        public void ScalarEnd(ScalarStyle style)
        {
            switch (style)
            {
                case ScalarStyle.SingleQuoted:
                case ScalarStyle.DoubleQuoted:
                case ScalarStyle.Literal:
                case ScalarStyle.Folded:
                    PopColor();
                    break;
            }
        }

        public void SequenceItemStart() { }
        public void SequenceItemEnd() { }
        public void StreamStart() { }
        public void StreamEnd() { }
        public void TagStart() { }
        public void TagEnd() { }
    }
}

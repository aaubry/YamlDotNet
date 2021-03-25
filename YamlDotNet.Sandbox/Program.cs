using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

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

        class Program
        {
            public static void Main()
            {
                var address = new Address
                {
                    street = "123 Tornado Alley\nSuite 16",
                    city = "East Westville",
                    state = "KS"
                };

                var receipt = new Receipt
                {
                    receipt = "Oz-Ware Purchase Invoice",
                    date = new DateTime(2007, 8, 6),
                    customer = new Customer
                    {
                        given = "Dorothy",
                        family = "Gale"
                    },
                    items = new Item[]
                    {
                    new Item
                    {
                        part_no = "A4786",
                        descrip = "Water Bucket (Filled)",
                        price = 1.47M,
                        quantity = 4
                    },
                    new Item
                    {
                        part_no = "E1628",
                        descrip = "High Heeled \"Ruby\" Slippers",
                        price = 100.27M,
                        quantity = 1
                    }
                    },
                    bill_to = address,
                    ship_to = address,
                    specialDelivery = "Follow the Yellow Brick\n" +
                                      "Road to the Emerald City.\n" +
                                      "Pay no attention to the\n" +
                                      "man behind the curtain."
                };

                var serializer = new SerializerBuilder().Build();
                serializer.Serialize(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, receipt);
            }

            public class Address
            {
                public string street { get; set; }
                public string city { get; set; }
                public string state { get; set; }
            }

            public class Receipt
            {
                public string receipt { get; set; }
                public DateTime date { get; set; }
                public Customer customer { get; set; }
                public Item[] items { get; set; }
                public Address bill_to { get; set; }
                public Address ship_to { get; set; }
                public string specialDelivery { get; set; }
            }

            public class Customer
            {
                public string given { get; set; }
                public string family { get; set; }
            }

            public class Item
            {
                public string part_no { get; set; }
                public string descrip { get; set; }
                public decimal price { get; set; }
                public int quantity { get; set; }
            }

            private const string Document = @"---
            receipt:    Oz-Ware Purchase Invoice
            date:        2007-08-06
            customer:
                given:   Dorothy
                family:  Gale

            items:
                - part_no:   A4786
                  descrip:   Water Bucket (Filled)
                  price:     1.47
                  quantity:  4

                - part_no:   E1628
                  descrip:   High Heeled ""Ruby"" Slippers
                  price:     100.27
                  quantity:  1

            bill-to:  &id001
                street: |
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery:  >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
...";

        }
    }
}

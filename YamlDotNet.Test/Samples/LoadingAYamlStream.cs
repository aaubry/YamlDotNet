using System;
using System.IO;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Test.Samples.Helpers;

namespace YamlDotNet.Test.Samples
{
    public class LoadingAYamlStream
    {
        private readonly ITestOutputHelper output;

        public LoadingAYamlStream(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            Title = "Loading a YAML Stream",
            Description = "Explains how to load YAML using the representation model."
        )]
        public void Main()
        {
            // Setup the input
            var input = new StringReader(Document);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

            // Examine the stream
            var mapping =
                (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                output.WriteLine(((YamlScalarNode)entry.Key).Value);
            }

            // List all the items
            var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("items")];
            foreach (YamlMappingNode item in items)
            {
                output.WriteLine(
                    "{0}\t{1}",
                    item.Children[new YamlScalarNode("part_no")],
                    item.Children[new YamlScalarNode("descrip")]
                );
            }
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
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace YamlDotNet.Samples
{
	class LoadYamlStream
	{
		public void Run(string[] args)
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
				Console.WriteLine(((YamlScalarNode)entry.Key).Value);
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
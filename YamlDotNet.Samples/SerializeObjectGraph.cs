using System;
using YamlDotNet.Serialization;

namespace YamlDotNet.Samples
{
	class SerializeObjectGraph
	{
		public void Run(string[] args)
		{
			var address = new
			{
				street = "123 Tornado Alley\nSuite 16",
				city = "East Westville",
				state = "KS"
			};

			var receipt = new
			{
				receipt = "Oz-Ware Purchase Invoice",
				date = new DateTime(2007, 8, 6),
				customer = new
				{
					given = "Dorothy",
					family = "Gale"
				},
				items = new[]
                {
                    new
                    {
                        part_no = "A4786",
                        descrip = "Water Bucket (Filled)",
                        price = 1.47M,
                        quantity = 4
                    },
                    new
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

			var serializer = new Serializer();
			serializer.Serialize(Console.Out, receipt);
		}
	}
}

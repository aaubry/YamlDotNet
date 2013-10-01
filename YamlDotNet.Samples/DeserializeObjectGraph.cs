using System;
using System.IO;
using YamlDotNet.Serialization;

namespace YamlDotNet.Samples
{
	public class DeserializeObjectGraph
	{
		public void Run(string[] args)
		{
			// Setup the input
			var input = new StringReader(_document);

			var deserializer = new Deserializer();
			var order = (Order)deserializer.Deserialize(input, typeof(Order));

			Console.WriteLine("Receipt: {0}", order.Receipt);
			Console.WriteLine("Customer: {0} {1}", order.Customer.Given, order.Customer.Family);
		}

		private class Order
		{
			public string Receipt { get; set; }
			public DateTime Date { get; set; }
			public Customer Customer { get; set; }
			public Item[] Items { get; set; }
			public Address BillTo { get; set; }
			public Address ShipTo { get; set; }
			public string SpecialDelivery { get; set; }
		}

		private class Customer
		{
			public string Given { get; set; }
			public string Family { get; set; }
		}

		public class Item
		{
			public string PartNo { get; set; }
			public decimal Price { get; set; }
			public int Quantity { get; set; }
			

			[YamlAlias("descrip")]
			public string Description { get; set; }
		}

		public class Address
		{
			public string Street { get; set; }
			public string City { get; set; }
			public string State { get; set; }
		}

		private const string _document = @"---
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

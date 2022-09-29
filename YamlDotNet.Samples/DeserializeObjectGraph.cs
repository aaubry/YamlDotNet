// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;
using YamlDotNet.Samples.Helpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Samples
{
    public class DeserializeObjectGraph
    {
        private readonly ITestOutputHelper output;

        public DeserializeObjectGraph(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            DisplayName = "Deserializing an object graph",
            Description = "Shows how to convert a YAML document to an object graph."
        )]
        public void Main()
        {
            var input = new StringReader(Document);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var order = deserializer.Deserialize<Order>(input);

            output.WriteLine("Order");
            output.WriteLine("-----");
            output.WriteLine();
            foreach (var item in order.Items)
            {
                output.WriteLine("{0}\t{1}\t{2}\t{3}", item.PartNo, item.Quantity, item.Price, item.Descrip);
            }
            output.WriteLine();

            output.WriteLine("Shipping");
            output.WriteLine("--------");
            output.WriteLine();
            output.WriteLine(order.ShipTo.Street);
            output.WriteLine(order.ShipTo.City);
            output.WriteLine(order.ShipTo.State);
            output.WriteLine();

            output.WriteLine("Billing");
            output.WriteLine("-------");
            output.WriteLine();
            if (order.BillTo == order.ShipTo)
            {
                output.WriteLine("*same as shipping address*");
            }
            else
            {
                output.WriteLine(order.ShipTo.Street);
                output.WriteLine(order.ShipTo.City);
                output.WriteLine(order.ShipTo.State);
            }
            output.WriteLine();

            output.WriteLine("Delivery instructions");
            output.WriteLine("---------------------");
            output.WriteLine();
            output.WriteLine(order.SpecialDelivery);
        }

        public class Order
        {
            public string Receipt { get; set; }
            public DateTime Date { get; set; }
            public Customer Customer { get; set; }
            public List<OrderItem> Items { get; set; }

            [YamlMember(Alias = "bill-to", ApplyNamingConventions = false)]
            public Address BillTo { get; set; }

            [YamlMember(Alias = "ship-to", ApplyNamingConventions = false)]
            public Address ShipTo { get; set; }

            public string SpecialDelivery { get; set; }
        }

        public class Customer
        {
            public string Given { get; set; }
            public string Family { get; set; }
        }

        public class OrderItem
        {
            [YamlMember(Alias = "part_no", ApplyNamingConventions = false)]
            public string PartNo { get; set; }
            public string Descrip { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
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
                street: |-
                        123 Tornado Alley
                        Suite 16
                city:   East Westville
                state:  KS

            ship-to:  *id001

            specialDelivery: >
                Follow the Yellow Brick
                Road to the Emerald City.
                Pay no attention to the
                man behind the curtain.
...";
    }
}

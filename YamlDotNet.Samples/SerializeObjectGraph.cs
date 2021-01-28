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
using Xunit.Abstractions;
using YamlDotNet.Samples.Helpers;
using YamlDotNet.Serialization;

namespace YamlDotNet.Samples
{
    public class SerializeObjectGraph
    {
        private readonly ITestOutputHelper output;

        public SerializeObjectGraph(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Sample(
            DisplayName = "Serializing an object graph",
            Description = "Shows how to convert an object to its YAML representation."
        )]
        public void Main()
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
            var yaml = serializer.Serialize(receipt);
            output.WriteLine(yaml);
        }
    }

#pragma warning disable IDE1006 // Naming Styles
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
#pragma warning restore IDE1006 // Naming Styles
}

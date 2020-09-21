//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;

namespace YamlDotNet.PerformanceTests
{
    public class TestData
    {
        public static Receipt Graph
        {
            get
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
                    items = new[]
                    {
                        new OrderItem
                        {
                            part_no = "A4786",
                            descrip = "Water Bucket (Filled)",
                            price = 1.47M,
                            quantity = 4
                        },
                        new OrderItem
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

                return receipt;
            }
        }
    }

    public class Receipt
    {
        public string receipt { get; set; }
        public DateTime date { get; set; }
        public Customer customer { get; set; }
        public OrderItem[] items { get; set; }
        public Address bill_to { get; set; }
        public Address ship_to { get; set; }
        public string specialDelivery { get; set; }
    }

    public class OrderItem
    {
        public string part_no { get; set; }
        public string descrip { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
    }

    public class Customer
    {
        public string given { get; set; }
        public string family { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }
}


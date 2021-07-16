//using System;
//using YamlDotNet.Core;
//using YamlDotNet.Representation;
//using YamlDotNet.Serialization;

//namespace YamlDotNet.Sandbox
//{
//    class Program
//    {
//        public static void Main()
//        {
//            var address = new Address
//            {
//                street = "123 Tornado Alley\nSuite 16",
//                city = "East Westville",
//                state = "KS"
//            };

//            var receipt = new Receipt
//            {
//                receipt = "Oz-Ware Purchase Invoice",
//                date = new DateTime(2007, 8, 6),
//                customer = new Customer
//                {
//                    given = "Dorothy",
//                    family = "Gale"
//                },
//                items = new Item[]
//                {
//                    new Item
//                    {
//                        part_no = "A4786",
//                        descrip = "Water Bucket (Filled)",
//                        price = 1.47M,
//                        quantity = 4
//                    },
//                    new Item
//                    {
//                        part_no = "E1628",
//                        descrip = "High Heeled \"Ruby\" Slippers",
//                        price = 100.27M,
//                        quantity = 1
//                    }
//                },
//                bill_to = address,
//                ship_to = address,
//                specialDelivery = "Follow the Yellow Brick\n" +
//                                    "Road to the Emerald City.\n" +
//                                    "Pay no attention to the\n" +
//                                    "man behind the curtain."
//            };

//            var serializer = new SerializerBuilder().Build();
//            var document = serializer.SerializeToDocument(receipt);
//            Stream.Dump(new Emitter(Console.Out) { OutputFormatter = new ColoredConsoleOutputFormatter() }, document);

//            var deserializer = new DeserializerBuilder().Build();
//            var parsed = deserializer.Deserialize<Receipt>(document);

//            Console.WriteLine(parsed.receipt);
//            Console.WriteLine(parsed.date);
//            Console.WriteLine(parsed.customer.given);
//            Console.WriteLine(parsed.customer.family);
//            foreach (var item in parsed.items)
//            {
//                Console.WriteLine(item.part_no);
//                Console.WriteLine(item.descrip);
//                Console.WriteLine(item.price);
//                Console.WriteLine(item.quantity);
//            }
//            Console.WriteLine(parsed.bill_to.street);
//            Console.WriteLine(parsed.bill_to.city);
//            Console.WriteLine(parsed.bill_to.state);
//            Console.WriteLine(parsed.ship_to.street);
//            Console.WriteLine(parsed.ship_to.city);
//            Console.WriteLine(parsed.ship_to.state);
//            Console.WriteLine(parsed.specialDelivery);
//        }

//        public class Address
//        {
//            public string street { get; set; }
//            public string city { get; set; }
//            public string state { get; set; }
//        }

//        public class Receipt
//        {
//            public string receipt { get; set; }
//            public DateTime date { get; set; }
//            public Customer customer { get; set; }
//            public Item[] items { get; set; }
//            public Address bill_to { get; set; }
//            public Address ship_to { get; set; }
//            public string specialDelivery { get; set; }
//        }

//        public class Customer
//        {
//            public string given { get; set; }
//            public string family { get; set; }
//        }

//        public class Item
//        {
//            public string part_no { get; set; }
//            public string descrip { get; set; }
//            public decimal price { get; set; }
//            public int quantity { get; set; }
//        }

//        private const string Document = @"---
//        receipt:    Oz-Ware Purchase Invoice
//        date:        2007-08-06
//        customer:
//            given:   Dorothy
//            family:  Gale

//        items:
//            - part_no:   A4786
//                descrip:   Water Bucket (Filled)
//                price:     1.47
//                quantity:  4

//            - part_no:   E1628
//                descrip:   High Heeled ""Ruby"" Slippers
//                price:     100.27
//                quantity:  1

//        bill-to:  &id001
//            street: |
//                    123 Tornado Alley
//                    Suite 16
//            city:   East Westville
//            state:  KS

//        ship-to:  *id001

//        specialDelivery:  >
//            Follow the Yellow Brick
//            Road to the Emerald City.
//            Pay no attention to the
//            man behind the curtain.
//...";

//    }
//}

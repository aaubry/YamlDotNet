using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace YamlDotNet.AotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //var serializer = new Serializer();

            //var data = new Model
            //{
            //    SingleItem = new Item<string> { Id = 1, Value = "single", },
            //    ItemList = new List<Item<int>>
            //    {
            //        new Item<int> { Id = 2, Value = 0, },
            //        new Item<int> { Id = 3, Value = 1, },
            //        new Item<int> { Id = 4, Value = 2, },
            //    },
            //    ItemDictionary = new Dictionary<string, Item<string>>
            //    {
            //        { "item-0", new Item<string> { Id = 5, Value = "value-0" } },
            //        { "item-1", new Item<string> { Id = 6, Value = "value-1" } },
            //    },
            //    Unknown = new Hashtable
            //    {
            //        { "scalar", new Item<int> { Id = 7, Value = 0, } },
            //        { "array", new Item<int>[] { new Item<int> { Id = 8, Value = 1, } } },
            //    },
            //    ItemArray = new[]
            //    {
            //        new Item<long> { Id = 9, Value = 0, },
            //        new Item<long> { Id = 10, Value = 1, },
            //        new Item<long> { Id = 11, Value = 2, },
            //    },
            //};

            //var buffer = new StringWriter();

            //serializer.Serialize(buffer, data);
            //Console.WriteLine(buffer.ToString());

            //var deserializer = new Deserializer();
            //var deserializedData = deserializer.Deserialize<Model>(new StringReader(buffer.ToString()));

            //Console.WriteLine(deserializedData.SingleItem);
            //foreach (var item in deserializedData.ItemList)
            //{
            //    Console.WriteLine("- {0}", item);
            //}
            //foreach (var item in deserializedData.ItemDictionary)
            //{
            //    Console.WriteLine("{0}: {1}", item.Key, item.Value);
            //}
            //foreach (KeyValuePair<object, object> item in (IEnumerable)deserializedData.Unknown)
            //{
            //    Console.WriteLine("{0}: {1}", item.Key, item.Value);
            //}
            //foreach (var item in deserializedData.ItemArray)
            //{
            //    Console.WriteLine("- {0}", item);
            //}

            //Console.WriteLine(typeof(Dictionary<string, Item<int>>).AssemblyQualifiedName);

            var deserializer = new Deserializer();
            var deserialized = deserializer.Deserialize<object>(new StringReader(@"
                - !<!YamlDotNet.AotTest.ArrayContainer%601%5B%5BYamlDotNet.AotTest.Item%601%5B%5BSystem.String%2C%20mscorlib%2C%20Version%3D2.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db77a5c561934e089%5D%5D%2C%20YamlDotNet.AotTest%2C%20Version%3D1.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull%5D%5D%2C%20YamlDotNet.AotTest%2C%20Version%3D1.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull>
                  Items:
                    - Id: 1
                      Value: abc
                    - Id: 2
                      Value: def
                - !<!System.Collections.Generic.Dictionary%602%5B%5BSystem.String%2C%20mscorlib%2C%20Version%3D2.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db77a5c561934e089%5D%2C%5BYamlDotNet.AotTest.Item%601%5B%5BSystem.Int32%2C%20mscorlib%2C%20Version%3D2.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db77a5c561934e089%5D%5D%2C%20YamlDotNet.AotTest%2C%20Version%3D1.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull%5D%5D%2C%20mscorlib%2C%20Version%3D2.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db77a5c561934e089>
                  first:
                    Id: 1
                    Value: 10
                  second:
                    Id: 2
                    Value: 20
            "));

            Console.WriteLine();
            foreach (var item in (IEnumerable)deserialized)
            {
                Console.WriteLine(item.GetType().FullName);
            }
            Console.WriteLine();

            var serializer = new Serializer();
            serializer.Serialize(Console.Out, deserialized);
        }
    }

    public class ArrayContainer<T>
    {
        public T[] Items { get; set; }
    }

    public class Model
    {
        public Item<string> SingleItem { get; set; }
        public List<Item<int>> ItemList { get; set; }
        public Dictionary<string, Item<string>> ItemDictionary { get; set; }
        public object Unknown { get; set; }
        public Item<long>[] ItemArray { get; set; }
    }

    public class Item<T>
    {
        public int Id { get; set; }

        public T Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id, Value);
        }
    }
}

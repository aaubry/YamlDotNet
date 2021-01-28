using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Test.Serialization;

namespace YamlDotNet.AotTest
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("\x1b[37m---------------------------------------------------------------------------------"); 
            Console.WriteLine();

            Console.WriteLine("\x1b[97m Running AOT tests...");
            Console.WriteLine();

            TryDeserialize<MyDictionary>("DictionaryNodeDeserializer", "myDictionary: { winners: 3 }");
            TryDeserialize<MyList>("CollectionNodeDeserializer", "myList: [ 1, 2, 3 ]");
            TryDeserialize<MyArray>("ArayNodeDeserializer", "myArray: [ 1, 2, 3 ]");
            TrySerialize("TraverseGenericDictionary", new GenericTestDictionary<long, long> { { 1, 2 } });

            Console.WriteLine();
            Console.WriteLine(" \x1b[93m{0}\x1b[97m test succeeded, \x1b[93m{1}\x1b[97m tests failed", succeededTestCount, failedTestCount);

            Console.WriteLine();
            Console.WriteLine("\x1b[37m---------------------------------------------------------------------------------");
            Console.WriteLine("\x1b[0m");

            return failedTestCount;
        }

        private static int succeededTestCount;
        private static int failedTestCount;

        private static void TrySerialize<T>(string testName, T graph)
        {
            var output = new StringWriter();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            PerformTest(testName, () => serializer.Serialize(output, graph));
        }

        private static void TryDeserialize<T>(string testName, string yaml)
        {
            var input = new StringReader(yaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            PerformTest(testName, () => deserializer.Deserialize<T>(input));
        }

        private static void PerformTest(string testName, Action act)
        {
            Console.Write("\x1b[37m ");
            Console.Write(testName.PadRight(70));

            try
            {
                act();
                Console.WriteLine("\x1b[92m[success]\x1b[37m");
                ++succeededTestCount;
            }
            catch (Exception ex)
            {
                var current = ex;
                while (current != null)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (current is ExecutionEngineException)
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        Console.WriteLine("\x1b[91m[failure]");
                        Console.Write("\x1b[93m ");
                        Console.WriteLine(current.Message);
                        Console.Write("\x1b[37m");
                        ++failedTestCount;
                        return;
                    }

                    current = current.InnerException;
                }
                Console.Write("\x1b[91m");
                throw;
            }
        }
    }

    public class MyDictionary
    {
        public Dictionary<string, int> myDictionary { get; set; }
    }

    public class MyList
    {
        public List<int> myList { get; set; }
    }

    public class MyArray
    {
        public int[] myArray { get; set; }
    }
}

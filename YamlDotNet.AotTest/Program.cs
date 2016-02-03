using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.AotTest
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------------------"); 
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" Running AOT tests...");
            Console.WriteLine();

            TryDeserialize<MyDictionary>("DictionaryNodeDeserializer", "myDictionary: { winners: 3 }");
            TryDeserialize<MyList>("CollectionNodeDeserializer", "myList: [ 1, 2, 3 ]");
            TryDeserialize<MyArray>("ArayNodeDeserializer", "myArray: [ 1, 2, 3 ]");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" {0} test succeeded, {1} tests failed", succeededTestCount, failedTestCount);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------------------");
            Console.WriteLine();

            return failedTestCount;
        }

        private static int succeededTestCount;
        private static int failedTestCount;

        private static void TryDeserialize<T>(string testName, string yaml)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" ");
            Console.Write(testName.PadRight(70));

            var input = new StringReader(yaml);
            var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());
            try
            {
                deserializer.Deserialize<T>(input);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[success]");
                Console.ForegroundColor = ConsoleColor.Gray;
                ++succeededTestCount;
            }
            catch (Exception ex)
            {
                var current = ex;
                while (current != null)
                {
                    if (current is ExecutionEngineException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[failure]");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" ");
                        Console.WriteLine(current.Message);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        ++failedTestCount;
                        return;
                    }

                    current = current.InnerException;
                }
                Console.ForegroundColor = ConsoleColor.Red;
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

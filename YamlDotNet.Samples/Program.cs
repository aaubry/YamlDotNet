using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace YamlDotNet.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var samples = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t != typeof(Program))
                .Select(t => t.GetMethod("Run"))
                .Where(m => m != null);

            foreach (var sample in samples)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Begin of sample {0}", sample.DeclaringType.Name);
                Console.WriteLine("--------------------------------");
                var target = Activator.CreateInstance(sample.DeclaringType);
                sample.Invoke(target, new object[] { args });
                Console.WriteLine("--------------------------------");
                Console.WriteLine("End of sample {0}", sample.DeclaringType.Name);
                Console.WriteLine("--------------------------------");
                Console.WriteLine();
            }
        }
    }
}

//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2013 Antoine Aubry
    
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

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

using System.Diagnostics;

namespace YamlDotNet.Core.Test
{
	public class Dump
	{
		[Conditional("TEST_DUMP")]
		public static void Write(object value)
		{
			Debug.Write(value);
		}

		[Conditional("TEST_DUMP")]
		public static void Write(string format, params object[] args)
		{
			Debug.Write(string.Format(format, args));
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine()
		{
			Debug.WriteLine(string.Empty);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(string value)
		{
			WriteLine((object)value);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(object value)
		{
			WriteLine("{0}", value);
		}

		[Conditional("TEST_DUMP")]
		public static void WriteLine(string format, params object[] args)
		{
			Debug.WriteLine(format, args);
		}
	}
}
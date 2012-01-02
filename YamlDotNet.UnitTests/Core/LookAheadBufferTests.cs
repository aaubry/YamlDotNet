//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011 Antoine Aubry
    
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
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.UnitTests
{
	[TestClass]
	public class LookAheadBufferTests
	{
		private static LookAheadBuffer CreateBuffer(string text, int capacity) {
			return new LookAheadBuffer(new StringReader(text), capacity);
		}
		
		[TestMethod]
		public void ReadingWorks()
		{
			LookAheadBuffer buffer = CreateBuffer("abcdefghi", 4);
			
			FieldInfo count = buffer.GetType().GetField("count", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(count, "Failed to obtain the count field.");
			
			Assert.AreEqual(0, count.GetValue(buffer), "-");
			Assert.AreEqual('a', buffer.Peek(0), "a");
			Assert.AreEqual(1, count.GetValue(buffer), "a");
			Assert.AreEqual('b', buffer.Peek(1), "b");
			Assert.AreEqual(2, count.GetValue(buffer), "b");
			Assert.AreEqual('c', buffer.Peek(2), "c");
			Assert.AreEqual(3, count.GetValue(buffer), "c");
			buffer.Skip(1);
			Assert.AreEqual(2, count.GetValue(buffer), "c1");
			Assert.AreEqual('b', buffer.Peek(0), "b1");
			Assert.AreEqual(2, count.GetValue(buffer), "b1");
			Assert.AreEqual('c', buffer.Peek(1), "c2");
			Assert.AreEqual(2, count.GetValue(buffer), "c2");
			Assert.AreEqual('d', buffer.Peek(2), "d");
			Assert.AreEqual(3, count.GetValue(buffer), "d");
			Assert.AreEqual('e', buffer.Peek(3), "e");
			Assert.AreEqual(4, count.GetValue(buffer), "e");
			buffer.Skip(1);
			Assert.AreEqual(3, count.GetValue(buffer), "e1");			
			buffer.Skip(1);
			Assert.AreEqual(2, count.GetValue(buffer), "e2");
			buffer.Skip(1);
			Assert.AreEqual(1, count.GetValue(buffer), "e3");
			buffer.Skip(1);
			Assert.AreEqual(0, count.GetValue(buffer), "e4");
			Assert.AreEqual('f', buffer.Peek(0), "f");
			Assert.AreEqual(1, count.GetValue(buffer), "f");
			buffer.Skip(1);
			Assert.AreEqual(0, count.GetValue(buffer), "f1");
			Assert.AreEqual('g', buffer.Peek(0), "g");
			Assert.AreEqual(1, count.GetValue(buffer), "g");
			buffer.Skip(1);
			Assert.AreEqual(0, count.GetValue(buffer), "g1");
			Assert.AreEqual('h', buffer.Peek(0), "h");
			Assert.AreEqual(1, count.GetValue(buffer), "h");
			buffer.Skip(1);
			Assert.AreEqual(0, count.GetValue(buffer), "h1");
			Assert.AreEqual('i', buffer.Peek(0), "i");
			Assert.AreEqual(1, count.GetValue(buffer), "i");
			buffer.Skip(1);
			Assert.AreEqual(0, count.GetValue(buffer), "i1");
			Assert.IsFalse(buffer.EndOfInput);			
			Assert.AreEqual('\0', buffer.Peek(0), "\\0");
			Assert.IsTrue(buffer.EndOfInput);
			Assert.AreEqual(0, count.GetValue(buffer), "\\0");
		}
	}
}
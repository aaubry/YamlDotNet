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
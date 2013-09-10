//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry
    
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
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Test
{
	public class LookAheadBufferTests
	{
		private static LookAheadBuffer CreateBuffer(string text, int capacity) {
			return new LookAheadBuffer(new StringReader(text), capacity);
		}
		
		[Fact]
		public void ReadingWorks()
		{
			LookAheadBuffer buffer = CreateBuffer("abcdefghi", 4);
			
			FieldInfo count = buffer.GetType().GetField("count", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(count);
			
			Assert.Equal(0, count.GetValue(buffer));
			Assert.Equal('a', buffer.Peek(0));
			Assert.Equal(1, count.GetValue(buffer));
			Assert.Equal('b', buffer.Peek(1));
			Assert.Equal(2, count.GetValue(buffer));
			Assert.Equal('c', buffer.Peek(2));
			Assert.Equal(3, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(2, count.GetValue(buffer));
			Assert.Equal('b', buffer.Peek(0));
			Assert.Equal(2, count.GetValue(buffer));
			Assert.Equal('c', buffer.Peek(1));
			Assert.Equal(2, count.GetValue(buffer));
			Assert.Equal('d', buffer.Peek(2));
			Assert.Equal(3, count.GetValue(buffer));
			Assert.Equal('e', buffer.Peek(3));
			Assert.Equal(4, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(3, count.GetValue(buffer));			
			buffer.Skip(1);
			Assert.Equal(2, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(1, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(0, count.GetValue(buffer));
			Assert.Equal('f', buffer.Peek(0));
			Assert.Equal(1, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(0, count.GetValue(buffer));
			Assert.Equal('g', buffer.Peek(0));
			Assert.Equal(1, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(0, count.GetValue(buffer));
			Assert.Equal('h', buffer.Peek(0));
			Assert.Equal(1, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(0, count.GetValue(buffer));
			Assert.Equal('i', buffer.Peek(0));
			Assert.Equal(1, count.GetValue(buffer));
			buffer.Skip(1);
			Assert.Equal(0, count.GetValue(buffer));
			Assert.False(buffer.EndOfInput);			
			Assert.Equal('\0', buffer.Peek(0));
			Assert.True(buffer.EndOfInput);
			Assert.Equal(0, count.GetValue(buffer));
		}
	}
}
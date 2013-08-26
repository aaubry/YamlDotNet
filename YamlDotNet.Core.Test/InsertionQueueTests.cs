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
using YamlDotNet.Core;
using Xunit;

namespace YamlDotNet.Core.Test
{
	public class InsertionQueueTests
	{
		[Fact]
		public void QueueWorks() {
			InsertionQueue<int> queue = new InsertionQueue<int>();
			
			for (int i = 0; i < 100; ++i) {
				queue.Enqueue(i);
			}
			
			for (int i = 0; i < 100; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}

			for (int i = 0; i < 50; ++i) {
				queue.Enqueue(i);
			}

			for (int i = 0; i < 10; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}		

			for (int i = 50; i < 100; ++i) {
				queue.Enqueue(i);
			}

			for (int i = 10; i < 100; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}
		}

		[Fact]
		public void InsertWorks() {
			InsertionQueue<int> queue = new InsertionQueue<int>();
		
			for(int j = 0; j < 2; ++j) {
				for (int i = 0; i < 10; ++i) {
					queue.Enqueue(i);
				}
				
				queue.Insert(5, 99);
				
				for (int i = 0; i < 5; ++i) {
					Assert.Equal(i, queue.Dequeue());
				}

				Assert.Equal(99, queue.Dequeue());
			
				for (int i = 5; i < 10; ++i) {
					Assert.Equal(i, queue.Dequeue());
				}
			}
			
			for (int i = 0; i < 5; ++i) {
				queue.Enqueue(i);
				queue.Dequeue();
			}

			for (int i = 0; i < 20; ++i) {
				queue.Enqueue(i);
			}

			queue.Insert(5, 99);

			for (int i = 0; i < 5; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}
			
			Assert.Equal(99, queue.Dequeue());
			
			for (int i = 5; i < 20; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}
		}
			
		[Fact]
		public void Dequeue_ThrowsExceptionWhenEmpty() {
			InsertionQueue<int> queue = new InsertionQueue<int>();

			for (int i = 0; i < 10; ++i) {
				queue.Enqueue(i);
			}
			
			for (int i = 0; i < 10; ++i) {
				Assert.Equal(i, queue.Dequeue());
			}
			
			Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
		}
	}
}
using System;
using YamlDotNet.Core;
using NUnit.Framework;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class InsertionQueueTests
	{
		[Test]
		public void QueueWorks() {
			InsertionQueue<int> queue = new InsertionQueue<int>();
			
			for (int i = 0; i < 100; ++i) {
				queue.Enqueue(i);
			}
			
			for (int i = 0; i < 100; ++i) {
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}

			for (int i = 0; i < 50; ++i) {
				queue.Enqueue(i);
			}

			for (int i = 0; i < 10; ++i) {
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}		

			for (int i = 50; i < 100; ++i) {
				queue.Enqueue(i);
			}

			for (int i = 10; i < 100; ++i) {
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}
		}

		[Test]
		public void InsertWorks() {
			InsertionQueue<int> queue = new InsertionQueue<int>();
		
			for(int j = 0; j < 2; ++j) {
				for (int i = 0; i < 10; ++i) {
					queue.Enqueue(i);
				}
				
				queue.Insert(5, 99);
				
				for (int i = 0; i < 5; ++i) {
					Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
				}

				Assert.AreEqual(99, queue.Dequeue(), "The queue order is wrong");
			
				for (int i = 5; i < 10; ++i) {
					Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
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
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}
			
			Assert.AreEqual(99, queue.Dequeue(), "The queue order is wrong");
			
			for (int i = 5; i < 20; ++i) {
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}
		}
			
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Dequeue_ThrowsExceptionWhenEmpty() {
			InsertionQueue<int> queue = new InsertionQueue<int>();

			for (int i = 0; i < 10; ++i) {
				queue.Enqueue(i);
			}
			
			for (int i = 0; i < 10; ++i) {
				Assert.AreEqual(i, queue.Dequeue(), "The queue order is wrong");
			}
			
			queue.Dequeue();
		}
	}
}
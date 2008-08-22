using System;
using System.Collections.Generic;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Generic queue on which items may be inserted
	/// </summary>
	public class InsertionQueue<T>
	{
		// TODO: Use a more efficient data structure
		
		private IList<T> items = new List<T>();
		
		public int Count {
			get {
				return items.Count;
			}
		}
		
		public void Enqueue(T item) {
			items.Add(item);
		}
		
		public T Dequeue() {
			if(Count == 0) {
				throw new InvalidOperationException("The queue is empty");
			}

			T item = items[0];
			items.RemoveAt(0);
			return item;
		}
		
		public void Insert(int index, T item) {
			items.Insert(index, item);
		}
	}
}
using System;
using System.Collections.Generic;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Implements an indexer through an IEnumerator<T>
	/// </summary>
	public class FakeList<T>
	{
		private readonly IEnumerator<T> collection;
		private int currentIndex = -1;
		
		public FakeList(IEnumerator<T> collection)
		{
			this.collection = collection; 
		}
		
		public FakeList(IEnumerable<T> collection)
			: this(collection.GetEnumerator())
		{
		}

		/// <summary>
		/// Gets the element at the specified index. 
		/// </summary>
		/// <remarks>
		/// If index is greater or equal than the last used index, this operation is O(index - lastIndex),
		/// else this operation is O(index).
		/// </remarks>
		public T this[int index] {
			get {
				if(index < currentIndex) {
					collection.Reset();
					currentIndex = -1;
				}
				
				while(currentIndex < index) {
					if(!collection.MoveNext()) {
						throw new ArgumentOutOfRangeException("index");
					}
					++currentIndex;
				}
				
				return collection.Current;
			}
		}
	}
}
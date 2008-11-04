using System;

namespace YamlDotNet.Core
{
	internal class StringLookAheadBuffer : ILookAheadBuffer
	{
		private readonly string value;
		private int currentIndex;
		
		public int Length {
			get {
				return value.Length;
			}
		}
		
		public int Position {
			get {
				return currentIndex;
			}
		}
		
		private bool IsOutside(int index) {
			return index >= value.Length;
		}
		
		public bool EndOfInput {
			get {
				return IsOutside(currentIndex);
			}
		}
		
		public StringLookAheadBuffer(string value)
		{
			this.value = value;
			currentIndex = 0;
		}

		public char Peek(int offset)
		{
			int index = currentIndex + offset;
			return IsOutside(index) ? '\0' : value[index];
		}

		public void Skip(int length)
		{
			if(length < 0) {
				throw new ArgumentOutOfRangeException("length", "The length must be positive.");
			}
			currentIndex += length;
		}
	}
}
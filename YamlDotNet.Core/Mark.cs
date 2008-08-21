using System;

namespace YamlDotNet.CoreCs
{
	/// <summary>
	/// Represents a location inside a file
	/// </summary>
	public struct Mark
	{
		private int index;
		private int line;
		private int column;

		/// <summary>
		/// Gets / sets the absolute offset in the file
		/// </summary>
		public int Index {
			get {
				return index;
			}
			set {
				if(value < 0) {
					throw new ArgumentOutOfRangeException("Index", "Index must be greater than or equal to zero.");
				}
				index = value;
			}
		}

		/// <summary>
		/// Gets / sets the number of the line
		/// </summary>
		public int Line {
			get {
				return line;
			}
			set {
				if(value < 0) {
					throw new ArgumentOutOfRangeException("Line", "Line must be greater than or equal to zero.");
				}
				line = value;
			}
		}

		/// <summary>
		/// Gets / sets the index of the column
		/// </summary>
		public int Column {
			get {
				return column;
			}
			set {
				if(value < 0) {
					throw new ArgumentOutOfRangeException("Column", "Column must be greater than or equal to zero.");
				}
				column = value;
			}
		}
		
		public static readonly Mark Empty = new Mark();
	}
}
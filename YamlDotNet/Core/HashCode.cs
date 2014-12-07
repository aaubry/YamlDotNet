using System;

namespace YamlDotNet.Core
{
	/// <summary>
    /// Supports implementations of <see cref="Object.GetHashCode"/> by providing methods to combine two hash codes.
    /// </summary>
	internal static class HashCode
	{
		/// <summary>
		/// Combines two hash codes.
		/// </summary>
		/// <param name="h1">The first hash code.</param>
		/// <param name="h2">The second hash code.</param>
		/// <returns></returns>
		public static int CombineHashCodes(int h1, int h2)
		{
			return ((h1 << 5) + h1) ^ h2;
		}
	}
}
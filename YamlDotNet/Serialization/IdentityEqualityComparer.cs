using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YamlDotNet.Serialization
{
	internal class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		public bool Equals(T left, T right)
		{
			return ReferenceEquals(left, right);
		}

		public int GetHashCode(T value)
		{
			return RuntimeHelpers.GetHashCode(value);
		}
	}
}
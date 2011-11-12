using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Comparer that is based on identity comparisons.
	/// </summary>
	public sealed class YamlNodeIdentityEqualityComparer : IEqualityComparer<YamlNode>
	{
		#region IEqualityComparer<YamlNode> Members

		/// <summary />
		public bool Equals(YamlNode x, YamlNode y)
		{
			return ReferenceEquals(x, y);
		}

		/// <summary />
		public int GetHashCode(YamlNode obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}
}
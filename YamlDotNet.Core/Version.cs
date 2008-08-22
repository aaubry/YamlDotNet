using System;

namespace YamlDotNet.Core
{
	public class Version
	{
		private readonly int major;
		
		public int Major {
			get {
				return major;
			}
		}

		private readonly int minor;

		public int Minor {
			get {
				return minor;
			}
		}
		
		public Version(int major, int minor)
		{
			this.major = major;
			this.minor = minor;
		}
		
		public override bool Equals (object o)
		{
			Version other = o as Version;
			return other != null && major == other.major && minor == other.minor;
		}

		public override int GetHashCode ()
		{
			return major.GetHashCode() ^ minor.GetHashCode();
		}

	}
}
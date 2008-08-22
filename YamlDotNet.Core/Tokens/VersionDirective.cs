using System;

namespace YamlDotNet.Core.Tokens
{
	public class VersionDirective : Token {
		private readonly Version version;
		
		public Version Version {
			get {
				return version;
			}
		}
		
		public VersionDirective(Version version)
			: this(version, Mark.Empty, Mark.Empty)
		{
		}

		public VersionDirective(Version version, Mark start, Mark end)
			: base(start, end)
		{
			this.version = version;
		}
	
		
		public override bool Equals (object o)
		{
			VersionDirective other = o as VersionDirective;
			return other != null && version.Equals(other.version);
		}
		
		public override int GetHashCode ()
		{
			return version.GetHashCode();
		}
	}
}
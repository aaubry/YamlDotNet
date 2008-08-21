using System;

namespace YamlDotNet.CoreCs.Tokens
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
	}
}
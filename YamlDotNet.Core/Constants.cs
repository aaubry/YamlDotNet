using System;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Defines constants thar relate to the YAML specification.
	/// </summary>
	internal static class Constants
	{
		public static readonly TagDirective[] DefaultTagDirectives = new TagDirective[]
		{
			new TagDirective("!", "!"),
			new TagDirective("!!", "tag:yaml.org,2002:"),
		};
		
		public const int MajorVersion = 1;
		public const int MinorVersion = 1;
		
		public const char HandleCharacter = '!';
		public const string DefaultHandle = "!";
	}
}
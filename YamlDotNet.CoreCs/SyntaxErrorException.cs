using System;
using System.Globalization;

namespace YamlDotNet.CoreCs
{
	public class SyntaxErrorException : Exception
	{
		
		public SyntaxErrorException(string description, Mark location)
			: base(string.Format(CultureInfo.InvariantCulture, "{0} On line {1}, column {2}", description, location.Line + 1, location.Column + 1))
		{
		}
	}
}

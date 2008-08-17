using System;
using NUnit.Framework;
using YamlDotNet.CoreCs;
using YamlDotNet.CoreCs.Events;

namespace YamlDotNet.UnitTests
{
	[TestFixture]
	public class ParserTests
	{	
		[Test]
		public void ParseStreamStart()
		{
			Parser parser = new Parser();
			Event evt = parser.Parse();
			Assert.IsInstanceOfType(typeof(StreamStart), evt, "Parsed the wrong type");
		}
	}
}
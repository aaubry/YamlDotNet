using System;

namespace YamlDotNet.UnitTests
{
	public class Runner
	{
		public static void Main()
		{
			YamlStreamTests test = new YamlStreamTests();
			test.BackwardAliasReferenceWorks();
			test.ForwardAliasReferenceWorks();
			test.LoadSimpleDocument();
		}
	}
}
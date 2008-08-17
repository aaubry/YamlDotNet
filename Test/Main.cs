using System;
using System.IO;
using System.Text;
using  YamlDotNet.UnitTests;

namespace Test
{
	class MainClass
	{
		public static void Main() {
			ScannerTests tests = new ScannerTests();
			tests.VerifyTokensOnExample7();
		}
	}
}
// Main.cs created with MonoDevelop
// User: aaubry at 1:24 PM 8/23/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using System.Reflection;
using YamlDotNet.UnitTests;

namespace Test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			ParserTests tests = new ParserTests();
			tests.VerifyTokensOnExample9();
			
			
//			if(args.Length != 2) {
//				Console.WriteLine("Invalid command line arguments");
//				Console.WriteLine("USAGE: Test test_type_name test_method_name");
//				return;
//			}
//			
//			Type testType = typeof(YamlTest).Assembly.GetType("YamlDotNet.UnitTests." + args[0]);
//			if(testType == null) {
//				Console.WriteLine("Type not found");
//				return;
//			}
//			
//			MethodInfo method = testType.GetMethod(args[1]);
//			if(method == null) {
//				Console.WriteLine("Method not found");
//				return;
//			}
//
//			object tests = Activator.CreateInstance(testType);
//			
//			method.Invoke(tests, null);
		}
	}
}
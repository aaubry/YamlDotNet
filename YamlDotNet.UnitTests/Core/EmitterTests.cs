using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.UnitTests
{
	[TestClass]
	public class EmitterTests : YamlTest
	{
		private void ParseAndEmit(string name) {
			string testText = YamlFile(name).ReadToEnd();

			Parser parser = new Parser(new StringReader(testText));
			using(StringWriter output = new StringWriter()) {
				Emitter emitter = new Emitter(output, 2, int.MaxValue, false);
				while(parser.MoveNext()) {
					//Console.WriteLine(parser.Current.GetType().Name);
					Console.Error.WriteLine(parser.Current);
					emitter.Emit(parser.Current);
				}
				
				string result = output.ToString();
				
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();
				Console.WriteLine(testText);
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();
				Console.WriteLine(result);
				Console.WriteLine();
				Console.WriteLine("------------------------------");
				Console.WriteLine();

				/*
				Parser resultParser = new Parser(new StringReader(result));
				while(resultParser.MoveNext()) {
					Console.WriteLine(resultParser.Current.GetType().Name);
				}
				*/
				/*
				
				if(testText != result) {
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
					Console.WriteLine("Expected:");
					Console.WriteLine();
					Console.WriteLine(testText);
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
					Console.WriteLine("Result:");
					Console.WriteLine();
					Console.WriteLine(result);
					Console.WriteLine();
					Console.WriteLine("------------------------------");
					Console.WriteLine();
				}
				
				Assert.AreEqual(testText, result, "The emitter did not generate the correct text.");
				*/
			}
		}
		
		[TestMethod]
		public void EmitExample1()
		{
			ParseAndEmit("test1.yaml");
		}
		
		[TestMethod]
		public void EmitExample2()
		{
			ParseAndEmit("test2.yaml");
		}
		
		[TestMethod]
		public void EmitExample3()
		{
			ParseAndEmit("test3.yaml");
		}
		
		[TestMethod]
		public void EmitExample4()
		{
			ParseAndEmit("test4.yaml");
		}
		
		[TestMethod]
		public void EmitExample5()
		{
			ParseAndEmit("test5.yaml");
		}
		
		[TestMethod]
		public void EmitExample6()
		{
			ParseAndEmit("test6.yaml");
		}
		
		[TestMethod]
		public void EmitExample7()
		{
			ParseAndEmit("test7.yaml");
		}
		
		[TestMethod]
		public void EmitExample8()
		{
			ParseAndEmit("test8.yaml");
		}
		
		[TestMethod]
		public void EmitExample9()
		{
			ParseAndEmit("test9.yaml");
		}
		
		[TestMethod]
		public void EmitExample10()
		{
			ParseAndEmit("test10.yaml");
		}
		
		[TestMethod]
		public void EmitExample11()
		{
			ParseAndEmit("test11.yaml");
		}
		
		[TestMethod]
		public void EmitExample12()
		{
			ParseAndEmit("test12.yaml");
		}
		
		[TestMethod]
		public void EmitExample13()
		{
			ParseAndEmit("test13.yaml");
		}
		
		[TestMethod]
		public void EmitExample14()
		{
			ParseAndEmit("test14.yaml");
		}
	}
}
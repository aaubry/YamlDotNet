using System;
using MbUnit.Framework;
using YamlDotNet.Core;
using System.IO;
using System.Text;

namespace YamlDotNet.UnitTests.RepresentationModel {
	[TestFixture]
	public class YamlStreamTests {
		[Test]
		public void Test()
		{
			const string Document = @"- Mark McGwire
- Sammy Sosa
- Ken Griffey
";

			using (Parser parser = new Parser(new MemoryStream(Encoding.UTF8.GetBytes(Document))))
			{
				MemoryStream output = new MemoryStream();
				using (Emitter emitter = new Emitter(output))
				{
					while (parser.MoveNext())
					{
						Console.Error.WriteLine(parser.Current);
						emitter.Emit(parser.Current);
						parser.Current.Dispose();
					}
				}
				output.Position = 0;
				Console.Write(new StreamReader(output).ReadToEnd());
			}
		}

		[Test]
		public void EmitSimpleDocument()
		{
			/*
			StreamStartEvent utf-8
			DocumentStartEvent implicit 0.0
			SequenceStartEvent   implicit Plain
			ScalarEvent   Mark McGwire 12 plain_implicit quoted_implicit Plain
			ScalarEvent   Sammy Sosa 10 plain_implicit quoted_implicit Plain
			ScalarEvent   Ken Griffey 11 plain_implicit quoted_implicit Plain
			SequenceEndEvent
			DocumentEndEvent implicit
			StreamEndEvent
			*/

			MemoryStream output = new MemoryStream();
			using (Emitter emitter = new Emitter(output))
			{
				emitter.Emit(new StreamStartEvent(Encoding.UTF8));
				emitter.Emit(new DocumentStartEvent(new YamlVersion(1, 1), true));
				emitter.Emit(new SequenceStartEvent(null, null, ScalarStyle.Plain, true));
				emitter.Emit(new ScalarEvent("Mark McGwire", null, null, ScalarStyle.Plain, true, true));
				emitter.Emit(new ScalarEvent("Sammy Sosa", null, null, ScalarStyle.Plain, true, true));
				emitter.Emit(new ScalarEvent("Ken Griffey", null, null, ScalarStyle.Plain, true, true));
				emitter.Emit(new SequenceEndEvent());
				emitter.Emit(new DocumentEndEvent());
				emitter.Emit(new StreamEndEvent());
			}
			output.Position = 0;
			Console.Write(new StreamReader(output).ReadToEnd());
		}

		[Test]
		public void EmitSimpleDocument2()
		{
			/*
			StreamStartEvent utf-8
			DocumentStartEvent implicit 0.0
			SequenceStartEvent   implicit Plain
			ScalarEvent   Mark McGwire 12 plain_implicit quoted_implicit Plain
			ScalarEvent   Sammy Sosa 10 plain_implicit quoted_implicit Plain
			ScalarEvent   Ken Griffey 11 plain_implicit quoted_implicit Plain
			SequenceEndEvent
			DocumentEndEvent implicit
			StreamEndEvent
			*/

			MemoryStream output = new MemoryStream();
			using (Emitter emitter = new Emitter(output))
			{
				emitter.Emit(new StreamStartEvent(Encoding.UTF8));
				emitter.Emit(new DocumentStartEvent());
				emitter.Emit(new SequenceStartEvent());
				emitter.Emit(new ScalarEvent("Mark McGwire"));
				emitter.Emit(new ScalarEvent("Sammy Sosa"));
				emitter.Emit(new ScalarEvent("Ken Griffey"));
				emitter.Emit(new SequenceEndEvent());
				emitter.Emit(new DocumentEndEvent());
				emitter.Emit(new StreamEndEvent());
			}
			output.Position = 0;
			Console.Write(new StreamReader(output).ReadToEnd());
		}
	}
}
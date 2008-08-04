using System;
using MbUnit.Framework;
using YamlDotNet.Core;
using System.IO;
using System.Text;
using System.Drawing;
using YamlDotNet.RepresentationModel.Serialization;
using System.Reflection;

namespace YamlDotNet.UnitTests.RepresentationModel {
	[TestFixture]
	public class YamlStreamTests {
		private MemoryStream output;

		[SetUp]
		public void SetUp()
		{
			output = new MemoryStream();
		}

		[TearDown]
		public void TearDown()
		{
			if(output.Length > 0)
			{
				output.Position = 0;
				Console.Write(new StreamReader(output).ReadToEnd());
			}
		}

		[Test]
		public void Test()
		{
			const string Document = @"- Mark McGwire
- Sammy Sosa
- Ken Griffey
- !!null
";

			using (Parser parser = new Parser(new MemoryStream(Encoding.UTF8.GetBytes(Document))))
			{
				using (Emitter emitter = new Emitter(output))
				{
					while (parser.MoveNext())
					{
						Console.Error.WriteLine(parser.Current);
						emitter.Emit(parser.Current);
						parser.Current.Dispose();
					}
				}
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
		}

		private class X
		{
			private bool myFlag;

			public bool MyFlag
			{
				get
				{
					return myFlag;
				}
				set
				{
					myFlag = value;
				}
			}

			private string nothing;

			public string Nothing
			{
				get
				{
					return nothing;
				}
				set
				{
					nothing = value;
				}
			}

			private int myInt = 1234;

			public int MyInt
			{
				get
				{
					return myInt;
				}
				set
				{
					myInt = value;
				}
			}

			private double myDouble = 6789.1011;

			public double MyDouble
			{
				get
				{
					return myDouble;
				}
				set
				{
					myDouble = value;
				}
			}

			private string myString = "Hello world";

			public string MyString
			{
				get
				{
					return myString;
				}
				set
				{
					myString = value;
				}
			}

			private DateTime myDate = DateTime.Now;

			public DateTime MyDate
			{
				get
				{
					return myDate;
				}
				set
				{
					myDate = value;
				}
			}

			private Point myPoint = new Point(100, 200);

			public Point MyPoint
			{
				get
				{
					return myPoint;
				}
				set
				{
					myPoint = value;
				}
			}
	
		}

		[Test]
		public void SerializeObject()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(X));
			serializer.Serialize(output, new X());

			serializer = new YamlSerializer(typeof(X), YamlSerializerOptions.Roundtrip);
			serializer.Serialize(output, new X());
		}

		[Test]
		public void Roundtrip()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(X), YamlSerializerOptions.Roundtrip);

			using(MemoryStream buffer = new MemoryStream())
			{
				X original = new X();
				serializer.Serialize(buffer, original);

				buffer.Position = 0;
				X copy = (X)serializer.Deserialize(buffer);

				foreach(PropertyInfo property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if(property.CanRead && property.CanWrite)
					{
						Assert.AreEqual(
							property.GetValue(original, null),
							property.GetValue(copy, null),
							string.Format("Property '{0}' is incorrect", property.Name)
						);
					}
				}
			}
		}
	}
}
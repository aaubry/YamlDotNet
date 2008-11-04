using System;
using System.Drawing;
using NUnit.Framework;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;
using System.Reflection;
using YamlDotNet.Core.Events;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	[TestFixture]
	public class YamlStreamTests
	{
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
		public void Roundtrip()
		{
			YamlSerializer serializer = new YamlSerializer(typeof(X), YamlSerializerOptions.Roundtrip);

			using (StringWriter buffer = new StringWriter())
			{
				X original = new X();
				serializer.Serialize(buffer, original);

				Console.WriteLine(buffer.ToString());

				X copy = (X)serializer.Deserialize(new StringReader(buffer.ToString()));

				foreach (var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.CanRead && property.CanWrite)
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

		[Test]
		public void Test()
		{
			string text = @"
MyFlag: False
Nothing: !!null ''
MyInt: 1234
MyDouble: 6789.1011
MyString: Hello world
MyDate: 2008-10-31T15:38:41.9189216+00:00
MyPoint:
  'X': '100'
  'Y': '200'
";

			Parser parser = new Parser(new StringReader(text));
			Emitter emitter = new Emitter(Console.Error, 2, 80, false);

			while (parser.MoveNext())
			{
				emitter.Emit(parser.Current);
				Console.WriteLine(parser.Current);
			}
		}

		[Test]
		public void Test2()
		{
			Console.WriteLine("###################");
			bool canonical = false;
			for (int i = 0; i < 2; ++i)
			{
				Emitter emitter = new Emitter(Console.Out, 2, 80, canonical);

				emitter.Emit(new StreamStart());
				emitter.Emit(new DocumentStart(null, null, true));
				emitter.Emit(new MappingStart(null, null, true, MappingStyle.Any));
				emitter.Emit(new Scalar("key"));
				emitter.Emit(new Scalar("value"));
				emitter.Emit(new Scalar("key2"));
				emitter.Emit(new Scalar("value2"));
				emitter.Emit(new MappingEnd());
				//emitter.Emit(new Scalar(null, null, "Hello", ScalarStyle.Any, true, false));
				emitter.Emit(new DocumentEnd(true));
				emitter.Emit(new StreamEnd());

				Console.WriteLine("###################");

				canonical = true;
			}

			/*
			Stream start
			Document start [isImplicit = True]
			Mapping start [anchor = , tag = , isImplicit = True, style = Block]
			Scalar [anchor = , tag = , value = MyFlag, style = Plain, isPlainImplicit = True, isQuotedImplicit = False]
			Scalar [anchor = , tag = , value = False, style = Plain, isPlainImplicit = True, isQuotedImplicit = False]
			Scalar [anchor = , tag = , value = MyInt, style = Plain, isPlainImplicit = True, isQuotedImplicit = False]
			Scalar [anchor = , tag = , value = 1234, style = Plain, isPlainImplicit = True, isQuotedImplicit = False]
			Mapping end
			Document end [isImplicit = True]
			Stream end

			 */

		}
	}
}
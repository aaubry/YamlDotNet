using System;
using System.Drawing;
using NUnit.Framework;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;
using System.Reflection;

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

				X copy = (X)serializer.Deserialize(new StringReader(buffer.ToString()));

				foreach(var property in typeof(X).GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class ObjectFactoryTests
	{
		public class FooBase
		{
		}

		public class FooDerived : FooBase
		{
		}

		[Fact]
		public void NotSpecifyingObjectFactoryUsesDefault()
		{
			var serializer = new YamlSerializer();
			var options = new DeserializationOptions();
			options.Mappings.Add("!foo", typeof(FooBase));
			var result = serializer.Deserialize(new StringReader("!foo {}"), options);

			Assert.IsType<FooBase>(result);
		}

		[Fact]
		public void ObjectFactoryIsInvoked()
		{
			var serializer = new YamlSerializer();
			var options = new DeserializationOptions();
			options.Mappings.Add("!foo", typeof(FooBase));

			options.ObjectFactory = new LambdaObjectFactory(t => new FooDerived());

			var result = serializer.Deserialize(new StringReader("!foo {}"), options);

			Assert.IsType<FooDerived>(result);
		}
	}
}

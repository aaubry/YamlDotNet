using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using YamlDotNet.Serialization;

namespace YamlDotNet.Serialization.Test
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
			var deserializer = new Deserializer();
			deserializer.RegisterTagMapping("!foo", typeof(FooBase));
			var result = deserializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooBase>(result);
		}

		[Fact]
		public void ObjectFactoryIsInvoked()
		{
			var deserializer = new Deserializer(new LambdaObjectFactory(t => new FooDerived()));
			deserializer.RegisterTagMapping("!foo", typeof(FooBase));

			var result = deserializer.Deserialize(new StringReader("!foo {}"));

			Assert.IsType<FooDerived>(result);
		}
	}
}

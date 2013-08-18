using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.UnitTests.RepresentationModel
{
	public class ExceptionWithNestedSerialization
	{
		[Fact]
		public void NestedDocumentShouldDeserializeProperly()
		{
			var s = new Serializer(SerializationOptions.EmitDefaults);
			var ds = new Deserializer();

			// serialize AMessage
			var tw = new StringWriter();
			s.Serialize(tw, new AMessage() { Payload = new PayloadA() { X = 5, Y = 6 } });
			Console.WriteLine(tw.ToString());

			// stick serialized AMessage in envelope and serialize it
			var e = new Env() { Type = "some-type", Payload = tw.ToString() };

			tw = new StringWriter();
			s.Serialize(tw, e);
			Console.WriteLine(tw.ToString());

			Console.WriteLine("${0}$", e.Payload);

			// deserialize envelope
			var e2 = ds.Deserialize<Env>(new StringReader(tw.ToString()));

			Console.WriteLine("${0}$", e2.Payload);

			// deserialize payload - fails if EmitDefaults is set
			ds.Deserialize<AMessage>(new StringReader(e2.Payload));
		}

		public class Env
		{
			public string Type { get; set; }
			public string Payload { get; set; }
		}

		public class Message<TPayload>
		{
			public int id { get; set; }
			public TPayload Payload { get; set; }
		}

		public class PayloadA
		{
			public int X { get; set; }
			public int Y { get; set; }
		}

		public class AMessage : Message<PayloadA> { }
	}
}

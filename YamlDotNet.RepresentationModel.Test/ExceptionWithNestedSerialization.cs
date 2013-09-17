using System.IO;
using Xunit;
using YamlDotNet.Core.Test;
using YamlDotNet.RepresentationModel.Serialization;

namespace YamlDotNet.RepresentationModel.Test
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
			s.Serialize(tw, new AMessage { Payload = new PayloadA { X = 5, Y = 6 } });
			Dump.WriteLine(tw);

			// stick serialized AMessage in envelope and serialize it
			var e = new Env { Type = "some-type", Payload = tw.ToString() };

			tw = new StringWriter();
			s.Serialize(tw, e);
			Dump.WriteLine(tw);

			Dump.WriteLine("${0}$", e.Payload);

			// deserialize envelope
			var e2 = ds.Deserialize<Env>(new StringReader(tw.ToString()));

			Dump.WriteLine("${0}$", e2.Payload);

			// deserialize payload - fails if EmitDefaults is set
			ds.Deserialize<AMessage>(new StringReader(e2.Payload));

			// Todo: proper assert
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

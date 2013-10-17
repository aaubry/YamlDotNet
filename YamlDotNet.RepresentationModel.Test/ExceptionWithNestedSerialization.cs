//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2013 Antoine Aubry
    
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:
    
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
    
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

﻿using System.IO;
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

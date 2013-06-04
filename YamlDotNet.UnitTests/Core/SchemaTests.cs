// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) 2013 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Schemas;
using YamlDotNet.Core.Events;

namespace YamlDotNet.UnitTests.Core
{

	public class SchemaTests : YamlTest
	{
		private void AssertParseWithSchemaProducesCorrectTags(ISchema schema, string yaml)
		{
			var parser = new SchemaEnforcingParser(
				new Parser(YamlText(yaml)),
				schema
			);
			
			var reader = new EventReader(parser);
			
			reader.Allow<StreamStart>();
			reader.Allow<DocumentStart>();
			reader.Allow<SequenceStart>();
			
			while(!reader.Accept<SequenceEnd>())
			{
				reader.Expect<MappingStart>();
				
				reader.Expect<Scalar>();
				var actual = ConsumeObject(reader);
				
				reader.Expect<Scalar>();
				var expected = ConsumeObject(reader);
				
				Console.WriteLine ("actual: {0}\nexpected: {1}\n\n", actual, expected);
				Assert.Equal(expected.Tag, actual.Tag);
				
				reader.Expect<MappingEnd>();
			}
		}
		
		private NodeEvent ConsumeObject(EventReader reader)
		{
			var scalar = reader.Allow<Scalar>();
			if(scalar != null)
			{
				return scalar;
			}
			
			var mapping = reader.Allow<MappingStart>();
			if(mapping != null)
			{
				while(reader.Allow<MappingEnd>() == null)
				{
					reader.Skip();
				}
				return mapping;
			}
			
			var sequence = reader.Allow<SequenceStart>();
			if(sequence != null)
			{
				while(reader.Allow<SequenceEnd>() == null)
				{
					reader.Skip();
				}
				return sequence;
			}
			
			throw new InvalidOperationException(
				string.Format(
					"Invalid node type {0} at {1}",
					reader.Peek<ParsingEvent>().GetType().Name,
					reader.Peek<ParsingEvent>().Start
				)
			);
		}
		
		[Fact]
		public void ParseWithFailsafeSchemaProducesCorrectTags()
		{
			AssertParseWithSchemaProducesCorrectTags(
				new FailsafeSchema(),
				@"
					- { actual: null, expected: !!str 'null' }
					- { actual: Null, expected: !!str 'Null' }
					- { actual: NULL, expected: !!str 'NULL' }
					- { actual: ~, expected: !!str '~' }
					- { actual: , expected: !!str '' }
	
					- { actual: true, expected: !!str 'true' }
					- { actual: True, expected: !!str 'True' }
					- { actual: TRUE, expected: !!str 'TRUE' }
					- { actual: false, expected: !!str 'false' }
					- { actual: False, expected: !!str 'False' }
					- { actual: FALSE, expected: !!str 'FALSE' }
	
					- { actual: 0, expected: !!str '0' }
					- { actual: 13, expected: !!str '13' }
					- { actual: -6, expected: !!str '-6' }
					- { actual: 0o10, expected: !!str '8' }
					- { actual: 0x3A, expected: !!str '58' }
	
					- { actual: 0., expected: !!str '0.' }
					- { actual: -0.0, expected: !!str '-0.0' }
					- { actual: .5, expected: !!str '.5' }
					- { actual: +12e03, expected: !!str '+12e03' }
					- { actual: -2E+05, expected: !!str '-2E+05' }
					- { actual: .inf, expected: !!str '.inf' }
					- { actual: -.Inf, expected: !!str '-.Inf' }
					- { actual: +.INF, expected: !!str '+.inf' }
					- { actual: .nan, expected: !!str '.nan' }

					- { actual: { a: b }, expected: !!map { a: b } }
					- { actual: [ a, b ], expected: !!seq [ a, b ] }
				"
			);
		}

		[Fact]
		public void ParseWithJsonSchemaProducesCorrectTags()
		{
			AssertParseWithSchemaProducesCorrectTags(
				new JsonSchema(),
				@"
					- { 'actual': null, 'expected': !!null }
	
					- { 'actual': true, 'expected': !!bool true }
					- { 'actual': false, 'expected': !!bool false }
	
					- { 'actual': 0, 'expected': !!int 0 }
					- { 'actual': 13, 'expected': !!int 13 }
					- { 'actual': -6, 'expected': !!int -6 }

					- { 'actual': 0., 'expected': !!float 0. }
					- { 'actual': -0.0, 'expected': !!float -0.0 }
					- { 'actual': 0.5, 'expected': !!float 0.5 }
					- { 'actual': 12e03, 'expected': !!float 12e03 }
					- { 'actual': -2E+05, 'expected': !!float -2E+05 }

					- { 'actual': { 'a': 'b' }, 'expected': !!map { 'a': 'b' } }
					- { 'actual': [ 'a', 'b' ], 'expected': !!seq [ 'a', 'b' ] }
				"
			);
		}

		[Fact]
		public void ParseInvalidUnquotedScalarWithJsonSchemaThrowsException()
		{
			Assert.Throws<SyntaxErrorException>(() =>
			{
				AssertParseWithSchemaProducesCorrectTags(
					new JsonSchema(),
					@"
						- { 'actual': invalid, 'expected': !!str '' }
					"
				);
			});
		}
		
		/*
		[Fact]
		public void ScalarsHaveCorrectTagByDefault()
		{
			// http://www.yaml.org/spec/1.2/spec.html#id2805071
			var parser = new Parser(YamlText(@"
				- { actual: null, expected: !!null }
				- { actual: Null, expected: !!null }
				- { actual: NULL, expected: !!null }
				- { actual: ~, expected: !!null }
				- { actual: , expected: !!null }

				- { actual: true, expected: !!bool true }
				- { actual: True, expected: !!bool true }
				- { actual: TRUE, expected: !!bool true }
				- { actual: false, expected: !!bool false }
				- { actual: False, expected: !!bool false }
				- { actual: FALSE, expected: !!bool false }

				- { actual: 0, expected: !!int 0 }
				- { actual: 13, expected: !!int 13 }
				- { actual: -6, expected: !!int -6 }
				- { actual: 0o10, expected: !!int 8 }
				- { actual: 0x3A, expected: !!int 58 }

				- { actual: 0., expected: !!float 0 }
				- { actual: -0.0, expected: !!float 0 }
				- { actual: .5, expected: !!float 0.5 }
				- { actual: +12e03, expected: !!float 12000 }
				- { actual: -2E+05, expected: !!float -200000 }
				- { actual: .inf, expected: !!float .inf }
				- { actual: -.Inf, expected: !!float -.Inf }
				- { actual: +.INF, expected: !!float +.inf }
				- { actual: .nan, expected: !!float .nan }
			"));
			
			var reader = new EventReader(parser);
			
			reader.Allow<StreamStart>();
			reader.Allow<DocumentStart>();
			reader.Allow<SequenceStart>();
			
			while(reader.Allow<SequenceEnd>() == null)
			{
				reader.Expect<MappingStart>();
				
				reader.Expect<Scalar>();
				var actual = reader.Expect<Scalar>();
				
				reader.Expect<Scalar>();
				var expected = reader.Expect<Scalar>();
				
				Console.WriteLine ("actual: {0}\nexpected: {1}\n\n", actual, expected);
				Assert.Equal(expected.Tag, actual.Tag);
				
				reader.Expect<MappingEnd>();
			}
		}
		 */
	}
}


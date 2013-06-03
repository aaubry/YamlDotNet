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
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core
{
	public sealed class SchemaEnforcingParser : IParser
	{
		private readonly ISchema schema;
		private readonly IParser innerParser;
		
		public SchemaEnforcingParser(IParser innerParser, ISchema schema)
		{
			if(innerParser == null)
			{
				throw new ArgumentNullException("innerParser");
			}

			this.innerParser = innerParser;

			if(schema == null)
			{
				throw new ArgumentNullException("schema");
			}

			this.schema = schema;
		}
		
		public bool MoveNext()
		{
			if(innerParser.MoveNext())
			{
				Current = schema.Apply(innerParser.Current);
				return true;
			}
			return false;
		}

		public ParsingEvent Current { get; private set; }
	}
}
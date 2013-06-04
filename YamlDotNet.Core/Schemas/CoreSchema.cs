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

namespace YamlDotNet.Core.Schemas
{
	/// <summary>
	/// Implements the YAML core schema.
	/// <see cref="http://www.yaml.org/spec/1.2/spec.html#id2805071"/>
	/// </summary>
	/// <remarks>
	/// The Core schema is an extension of the JSON schema,
	/// allowing for more human-readable presentation of the same types.
	/// This is the recommended default schema that YAML processor
	/// should use unless instructed otherwise.
	/// It is also strongly recommended that other schemas should be based on it. 
	/// </remarks>
	public class CoreSchema : RegexBasedSchema
	{
		private static readonly TagMapping[] tagMappings =
			new[]
			{
				new TagMapping("tag:yaml.org,2002:null", @"^(null | Null | NULL | ~ | )$"),
				new TagMapping("tag:yaml.org,2002:bool", @"^(true | True | TRUE | false | False | FALSE)$"),
				new TagMapping("tag:yaml.org,2002:int", @"^[-+]? [0-9]+$"),
				new TagMapping("tag:yaml.org,2002:int", @"^0o [0-7]+$"),
				new TagMapping("tag:yaml.org,2002:int", @"^0x [0-9a-fA-F]+$"),
				new TagMapping("tag:yaml.org,2002:float", @"^[-+]? ( \. [0-9]+ | [0-9]+ ( \. [0-9]* )? ) ( [eE] [-+]? [0-9]+ )?$"),
				new TagMapping("tag:yaml.org,2002:float", @"^[-+]? ( \.inf | \.Inf | \.INF )$"),
				new TagMapping("tag:yaml.org,2002:float", @"^(\.nan | \.NaN | \.NAN)$"),
			};
		
		public CoreSchema()
			: base(tagMappings)
		{
		}
	}
}
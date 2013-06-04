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
using System.Linq;
using YamlDotNet.Core.Events;
using System.Text.RegularExpressions;

namespace YamlDotNet.Core.Schemas
{
	/// <summary>
	/// Implements the YAML JSON schema.
	/// <see cref="http://www.yaml.org/spec/1.2/spec.html#id2803231"/>
	/// </summary>
	/// <remarks>
	/// The JSON schema is the lowest common denominator of most
	/// modern computer languages, and allows parsing JSON files.
	/// A YAML processor should therefore support this schema,
	/// at least as an option.
	/// It is also strongly recommended that other schemas
	/// should be based on it. 
	/// </remarks>
	public class JsonSchema : FailsafeSchema
	{
		private class TagMapping
		{
			public readonly string Tag;
			public readonly Regex Pattern;
			
			public TagMapping (string tag, string pattern)
			{
				Tag = tag;
				Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
			}
		}
		
		private static readonly TagMapping[] tagMappings =
			new[]
			{
				new TagMapping("tag:yaml.org,2002:null", @"^null$"),
				new TagMapping("tag:yaml.org,2002:bool", @"^(true|false)$"),
				new TagMapping("tag:yaml.org,2002:int", @"^-? ( 0 | [1-9] [0-9]* )$"),
				new TagMapping("tag:yaml.org,2002:float", @"^-? ( 0 | [1-9] [0-9]* ) ( \. [0-9]* )? ( [eE] [-+]? [0-9]+ )?$"),
			};
		
		protected override string GetTag (Scalar scalar)
		{
			if(scalar.Style == ScalarStyle.Plain)
			{
				var matchingMapping = tagMappings
					.FirstOrDefault(m => m.Pattern.IsMatch(scalar.Value));
				
				if(matchingMapping != null)
				{
					return matchingMapping.Tag;
				}
				
				throw new SyntaxErrorException(
					scalar.Start,
					scalar.End,
					string.Format(
						"Scalar '{0}' is not valid according to the JSON schema",
						scalar.Value
					)
				);
			}
			return base.GetTag(scalar);
		}
	}
}


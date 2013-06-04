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

using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core.Schemas
{
	public abstract class RegexBasedSchema : FailsafeSchema
	{
		protected class TagMapping
		{
			public readonly string Tag;
			public readonly Regex Pattern;
			
			public TagMapping (string tag, string pattern)
			{
				Tag = tag;
				Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
			}
		}
		
		private readonly TagMapping[] tagMappings;
		
		protected RegexBasedSchema(TagMapping[] tagMappings)
		{
			this.tagMappings = tagMappings;
		}
		
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
				else
				{
					OnUnresolvedTag(scalar);
				}
			}
			return base.GetTag(scalar);
		}
		
		protected virtual void OnUnresolvedTag(Scalar scalar) {}
	}
}

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
	/// Skeleton implementation of <see cref="ISchema"/>.
	/// </summary>
	public abstract class Schema : ISchema
	{
		Scalar ISchema.Apply (Scalar scalar)
		{
			var tag = GetTag(scalar);
			if(tag != null)
			{
				return new Scalar(
					scalar.Anchor,
					tag,
					scalar.Value,
					scalar.Style,
					scalar.IsPlainImplicit,
					scalar.IsQuotedImplicit,
					scalar.Start,
					scalar.End
				);
			}
			return scalar;
		}
		
		protected abstract string GetTag(Scalar scalar);

		SequenceStart ISchema.Apply (SequenceStart sequenceStart)
		{
			var tag = GetTag(sequenceStart);
			if(tag != null)
			{
				return new SequenceStart(
					sequenceStart.Anchor,
					tag,
					sequenceStart.IsImplicit,
					sequenceStart.Style,
					sequenceStart.Start,
					sequenceStart.End
				);
			}
			return sequenceStart;
		}
		
		protected abstract string GetTag(SequenceStart sequenceStart);

		MappingStart ISchema.Apply (MappingStart mappingStart)
		{
			var tag = GetTag(mappingStart);
			if(tag != null)
			{
				return new MappingStart(
					mappingStart.Anchor,
					tag,
					mappingStart.IsImplicit,
					mappingStart.Style,
					mappingStart.Start,
					mappingStart.End
				);
			}
			return mappingStart;
		}
		
		protected abstract string GetTag(MappingStart mappingStart);
	}
}

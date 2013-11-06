//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

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

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using TagDirective = YamlDotNet.Core.Tokens.TagDirective;
using VersionDirective = YamlDotNet.Core.Tokens.VersionDirective;

namespace YamlDotNet.Test.Core
{
	public class ParserTestHelper
	{
		protected const bool Explicit = false;
		protected const bool Implicit = true;
		protected const string TagYaml = "tag:yaml.org,2002:";

		protected static readonly TagDirective[] DefaultTags = new[] {
			new TagDirective("!", "!"),
			new TagDirective("!!", TagYaml)
		};

		protected static StreamStart StreamStart
		{
			get { return new StreamStart(); }
		}

		protected static StreamEnd StreamEnd
		{
			get { return new StreamEnd(); }
		}

		protected DocumentStart DocumentStart(bool isImplicit)
		{
			return DocumentStart(isImplicit, null, DefaultTags);
		}

		protected DocumentStart DocumentStart(bool isImplicit, VersionDirective version, params TagDirective[] tags)
		{
			return new DocumentStart(version, new TagDirectiveCollection(tags), isImplicit);
		}

		protected VersionDirective Version(int major, int minor)
		{
			return new VersionDirective(new Version(major, minor));
		}

		protected TagDirective TagDirective(string handle, string prefix)
		{
			return new TagDirective(handle, prefix);
		}

		protected DocumentEnd DocumentEnd(bool isImplicit)
		{
			return new DocumentEnd(isImplicit);
		}

		protected Scalar PlainScalar(string text)
		{
			return new Scalar(null, null, text, ScalarStyle.Plain, true, false);
		}

		protected Scalar SingleQuotedScalar(string text)
		{
			return new Scalar(null, null, text, ScalarStyle.SingleQuoted, false, true);
		}

		protected Scalar DoubleQuotedScalar(string text)
		{
			return DoubleQuotedScalar(null, text);
		}

		protected Scalar ExplicitDoubleQuotedScalar(string tag, string text)
		{
			return DoubleQuotedScalar(tag, text, false);
		}

		protected Scalar DoubleQuotedScalar(string tag, string text, bool quotedImplicit = true)
		{
			return new Scalar(null, tag, text, ScalarStyle.DoubleQuoted, false, quotedImplicit);
		}

		protected Scalar LiteralScalar(string text)
		{
			return new Scalar(null, null, text, ScalarStyle.Literal, false, true);
		}

		protected Scalar FoldedScalar(string text)
		{
			return new Scalar(null, null, text, ScalarStyle.Folded, false, true);
		}

		protected SequenceStart BlockSequenceStart
		{
			get { return new SequenceStart(null, null, true, SequenceStyle.Block); }
		}

		protected SequenceStart FlowSequenceStart
		{
			get { return new SequenceStart(null, null, true, SequenceStyle.Flow); }
		}

		protected SequenceStart AnchoredFlowSequenceStart(string anchor)
		{
			return new SequenceStart(anchor, null, true, SequenceStyle.Flow);
		}

		protected SequenceEnd SequenceEnd
		{
			get { return new SequenceEnd(); }
		}

		protected MappingStart BlockMappingStart
		{
			get { return new MappingStart(null, null, true, MappingStyle.Block); }
		}

		protected MappingStart TaggedBlockMappingStart(string tag)
		{
			return new MappingStart(null, tag, false, MappingStyle.Block);
		}

		protected MappingStart FlowMappingStart
		{
			get { return new MappingStart(null, null, true, MappingStyle.Flow); }
		}

		protected MappingEnd MappingEnd
		{
			get { return new MappingEnd(); }
		}

		protected AnchorAlias AnchorAlias(string alias)
		{
			return new AnchorAlias(alias);
		}
	}
}
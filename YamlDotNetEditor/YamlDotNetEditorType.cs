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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace YamlDotNetEditor
{
	internal static class YamlDotNetEditorClassificationDefinition
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlAnchor")]
		internal static ClassificationTypeDefinition YamlAnchorType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlAlias")]
		internal static ClassificationTypeDefinition YamlAliasType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlKey")]
		internal static ClassificationTypeDefinition YamlKeyType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlValue")]
		internal static ClassificationTypeDefinition YamlValueType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlNumber")]
		internal static ClassificationTypeDefinition YamlNumberType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlString")]
		internal static ClassificationTypeDefinition YamlStringType = null;
		
		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlTag")]
		internal static ClassificationTypeDefinition YamlTagType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlSymbol")]
		internal static ClassificationTypeDefinition YamlSymbolType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlDirective")]
		internal static ClassificationTypeDefinition YamlDirectiveType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name("YamlTab")]
		internal static ClassificationTypeDefinition YamlTabType = null;
	}
}

//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013, 2014 Antoine Aubry and contributors

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
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace YamlDotNetEditor
{
	#region Format definition
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlAnchor")]
	[Name("YamlAnchor")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlAnchorFormat : ClassificationFormatDefinition
	{
		public YamlAnchorFormat()
		{
			DisplayName = "YAML Anchor"; //human readable version of the name
			ForegroundColor = Color.FromRgb(255, 128, 64);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlAlias")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlAliasFormat : ClassificationFormatDefinition
	{
		public YamlAliasFormat()
		{
			DisplayName = "YAML Alias"; //human readable version of the name
			ForegroundColor = Color.FromRgb(115, 141, 0);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlKey")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlKeyFormat : ClassificationFormatDefinition
	{
		public YamlKeyFormat()
		{
			DisplayName = "YAML Key"; //human readable version of the name
			ForegroundColor = Color.FromRgb(68, 111, 189);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlValue")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlValueFormat : ClassificationFormatDefinition
	{
		public YamlValueFormat()
		{
			DisplayName = "YAML Value"; //human readable version of the name
			ForegroundColor = Color.FromRgb(83, 83, 83);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlTag")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlTagFormat : ClassificationFormatDefinition
	{
		public YamlTagFormat()
		{
			DisplayName = "YAML Tag"; //human readable version of the name
			ForegroundColor = Color.FromRgb(135, 87, 173);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlSymbol")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlSymbolFormat : ClassificationFormatDefinition
	{
		public YamlSymbolFormat()
		{
			DisplayName = "YAML Symbol"; //human readable version of the name
			ForegroundColor = Color.FromRgb(162, 162, 162);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlDirective")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlDirectiveFormat : ClassificationFormatDefinition
	{
		public YamlDirectiveFormat()
		{
			DisplayName = "YAML Directive"; //human readable version of the name
			ForegroundColor = Color.FromRgb(108, 226, 108);
			
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "YamlDotNetEditor")]
	[Name("YamlTab")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlTabFormat : ClassificationFormatDefinition
	{
		public YamlTabFormat()
		{
			DisplayName = "YAML Tab"; //human readable version of the name
			BackgroundColor = Color.FromRgb(182, 0, 0);
		}
	}
	#endregion //Format definition
}

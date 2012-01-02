//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011 Antoine Aubry
    
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

﻿using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Drawing;

namespace Company.YAMLLanguage
{
	internal enum ColorIndex
	{
		Text = 1,
		Anchor,
		AnchorReference,
		Tag,
		MappingSeparator,
		SequenceItem,
		BlockSequence,
		BlockMapping,
		BlockSeparator,
		Comment,
		Directive,
	}

	public class YAMLLanguageService : LanguageService
	{
		private LanguagePreferences preferences;

		public override LanguagePreferences GetLanguagePreferences()
		{
			if (preferences == null)
			{
				preferences = new LanguagePreferences(Site, typeof(YAMLLanguageService).GUID, Name);
				preferences.Init();
			}
			return preferences;
		}

		private static IVsColorableItem MakeColorableItem(string name, COLORINDEX foreColor, Color foreColorRgb, COLORINDEX backColor, Color backColorRgb, FONTFLAGS style)
		{
			return new ColorableItem(
				name,
				name,
				foreColor,
				backColor,
				foreColorRgb,
				backColorRgb,
				style
			);
		}

		private static IVsColorableItem MakeColorableItem(string name, COLORINDEX foreColor, Color foreColorRgb, COLORINDEX backColor, Color backColorRgb)
		{
			return MakeColorableItem(name, foreColor, foreColorRgb, backColor, backColorRgb, FONTFLAGS.FF_DEFAULT);
		}

		private static readonly IVsColorableItem[] colorableItems = new[]
		{
		    MakeColorableItem("YAML - Text", COLORINDEX.CI_MAROON, Color.Maroon, COLORINDEX.CI_YELLOW, Color.FromArgb(255, 254, 191)),
		    MakeColorableItem("YAML - Anchor", COLORINDEX.CI_DARKBLUE, Color.Navy, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Anchor reference", COLORINDEX.CI_BLUE, Color.RoyalBlue, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Tag", COLORINDEX.CI_PURPLE, Color.FromArgb(43, 145, 175), COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty),
		    MakeColorableItem("YAML - Mapping separator", COLORINDEX.CI_RED, Color.Red, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Sequence item", COLORINDEX.CI_RED, Color.Red, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Block sequence", COLORINDEX.CI_RED, Color.Red, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Block mapping", COLORINDEX.CI_RED, Color.Red, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Block separator", COLORINDEX.CI_RED, Color.Red, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty, FONTFLAGS.FF_BOLD),
		    MakeColorableItem("YAML - Comment", COLORINDEX.CI_GREEN, Color.Green, COLORINDEX.CI_SYSPLAINTEXT_BK, Color.Empty),
		    MakeColorableItem("YAML - Directive", COLORINDEX.CI_DARKGRAY, Color.FromArgb(85, 85, 85), COLORINDEX.CI_YELLOW, Color.FromArgb(255, 254, 191)),
		};

		public override int GetColorableItem(int index, out IVsColorableItem item)
		{
			item = colorableItems[index - 1];
			return VSConstants.S_OK;
		}

		public override int GetItemCount(out int count)
		{
			count = colorableItems.Length;
			return VSConstants.S_OK;
		}

		private YAMLScanner scanner;

		public override IScanner GetScanner(IVsTextLines buffer)
		{
			if (scanner == null)
			{
				scanner = new YAMLScanner(buffer);
			}
			return scanner;
		}

		public override AuthoringScope ParseSource(ParseRequest req)
		{
			return new YAMLAuthoringScope();
		}

		public override string GetFormatFilterList()
		{
			return "YAML files (*.yaml)\n*.yaml\n";
		}

		public override string Name
		{
			get
			{
				return "YAML";
			}
		}
	}
}
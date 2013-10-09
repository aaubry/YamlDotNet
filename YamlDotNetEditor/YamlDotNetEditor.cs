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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.IO;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.Text.RegularExpressions;
using YamlDotNet;
using YamlDotNet.Tokens;

namespace YamlDotNetEditor
{

	#region Provider definition
	/// <summary>
	/// This class causes a classifier to be added to the set of classifiers. Since 
	/// the content type is set to "text", this classifier applies to all text files
	/// </summary>
	[Export(typeof(IClassifierProvider))]
	[ContentType("yaml")]
	internal class YamlDotNetEditorProvider : IClassifierProvider
	{
		/// <summary>
		/// Import the classification registry to be used for getting a reference
		/// to the custom classification type later.
		/// </summary>
		[Import]
		internal IClassificationTypeRegistryService ClassificationRegistry = null; // Set via MEF

		public IClassifier GetClassifier(ITextBuffer buffer)
		{
			return buffer.Properties.GetOrCreateSingletonProperty<YamlDotNetEditor>(delegate { return new YamlDotNetEditor(ClassificationRegistry); });
		}
	}
	#endregion //provider def

	#region Classifier
	/// <summary>
	/// Classifier that classifies all text as an instance of the OrinaryClassifierType
	/// </summary>
	class YamlDotNetEditor : IClassifier
	{
		private readonly IClassificationType _comment;
		private readonly IClassificationType _anchor;
		private readonly IClassificationType _alias;
		private readonly IClassificationType _key;
		private readonly IClassificationType _value;
		private readonly IClassificationType _tag;
		private readonly IClassificationType _symbol;
		private readonly IClassificationType _directive;
		private readonly IClassificationType _tab;

		internal YamlDotNetEditor(IClassificationTypeRegistryService registry)
		{
			_comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
			_anchor = registry.GetClassificationType("YamlAnchor");
			_alias = registry.GetClassificationType("YamlAlias");
			_key = registry.GetClassificationType("YamlKey");
			_value = registry.GetClassificationType("YamlValue");
			_tag = registry.GetClassificationType("YamlTag");
			_symbol = registry.GetClassificationType("YamlSymbol");
			_directive = registry.GetClassificationType("YamlDirective");
			_tab = registry.GetClassificationType("YamlTab");
		}

		/// <summary>
		/// This method scans the given SnapshotSpan for potential matches for this classification.
		/// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
		/// </summary>
		/// <param name="trackingSpan">The span currently being classified</param>
		/// <returns>A list of ClassificationSpans that represent spans identified to be of this classification</returns>
		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			var classifications = new List<ClassificationSpan>();

			var text = span.GetText();

			var commentIndex = text.IndexOf('#');
			if (commentIndex >= 0)
			{
				classifications.Add(
					new ClassificationSpan(
						new SnapshotSpan(
							span.Snapshot,
							new Span(span.Start + commentIndex, span.Length - commentIndex)
						),
						_comment
					)
				);

				text = text.Substring(0, commentIndex);
			}

			var match = Regex.Match(text, @"^( *(\t+))+");
			if (match.Success)
			{
				foreach (Capture capture in match.Groups[2].Captures)
				{
					classifications.Add(
						new ClassificationSpan(
							new SnapshotSpan(
								span.Snapshot,
								new Span(span.Start + capture.Index, capture.Length)
							),
							_tab
						)
					);
				}
			}

			try
			{
				var scanner = new Scanner(new StringReader(text));

				Type previousTokenType = null;
				while (scanner.MoveNext())
				{
					IClassificationType classificationType = null;

					var currentTokenType = scanner.Current.GetType();
					var tokenLength = scanner.Current.End.Index - scanner.Current.Start.Index;

					if (currentTokenType == typeof(Anchor))
					{
						classificationType = _anchor;
					}
					else if (currentTokenType == typeof(AnchorAlias))
					{
						classificationType = _alias;
					}
					else if (currentTokenType == typeof(Scalar))
					{
						classificationType = previousTokenType == typeof(Key) ? _key : _value;
					}
					else if (currentTokenType == typeof(Tag))
					{
						classificationType = _tag;
					}
					else if (currentTokenType == typeof(TagDirective))
					{
						classificationType = _directive;
					}
					else if (currentTokenType == typeof(VersionDirective))
					{
						classificationType = _directive;
					}
					else if (tokenLength > 0)
					{
						classificationType = _symbol;
					}

					previousTokenType = currentTokenType;

					if (classificationType != null)
					{
						classifications.Add(
							new ClassificationSpan(
								new SnapshotSpan(
									span.Snapshot,
									new Span(span.Start + scanner.Current.Start.Index, tokenLength)
								),
								classificationType
							)
						);
					}
				}
			}
			catch
			{
			}

			return classifications;
		}

#pragma warning disable 67
		// This event gets raised if a non-text change would affect the classification in some way,
		// for example typing /* would cause the classification to change in C# without directly
		// affecting the span.
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67
	}
	#endregion //Classifier
}

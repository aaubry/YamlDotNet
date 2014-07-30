//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013, 2014 Antoine Aubry

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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNetEditor.CommandHandlers
{
	public abstract class SelectionModifierCommandHandler : ICommandHandler
	{
		public abstract VSConstants.VSStd2KCmdID CommandId { get; }

		public abstract bool IsEnabled(ITextView textView);

		public void Execute(ITextView textView, ITextUndoHistoryRegistry textUndoHistoryRegistry)
		{
			var isReversed = textView.Selection.IsReversed;
			var newSelection = new List<Tuple<int, int>>();

			var undoHistory = textUndoHistoryRegistry.RegisterHistory(textView);
			using (var transaction = undoHistory.CreateTransaction("Comment Selection"))
			{
				foreach (var span in textView.Selection.SelectedSpans)
				{
					var startLine = span.Start.GetContainingLine().LineNumber;
					var endLine = span.End.GetContainingLine().LineNumber;

					if (startLine > endLine)
					{
						var tmp = startLine;
						startLine = endLine;
						endLine = tmp;
					}

					var edit = span.Snapshot.TextBuffer.CreateEdit();
					for (int lineNumber = startLine; lineNumber <= endLine; ++lineNumber)
					{
						var line = span.Snapshot.GetLineFromLineNumber(lineNumber);
						Modify(edit, line);
					}

					edit.Apply();

					newSelection.Add(Tuple.Create(startLine, endLine));
				}

				transaction.Complete();
			}

			textView.Selection.Select(new NormalizedSnapshotSpanCollection(
				newSelection.Select(l => new SnapshotSpan(
					textView.TextSnapshot.GetLineFromLineNumber(l.Item1).Start,
					textView.TextSnapshot.GetLineFromLineNumber(l.Item2).End)
				)
			)[0], isReversed);
		}

		protected abstract void Modify(ITextEdit edit, ITextSnapshotLine line);
	}
}
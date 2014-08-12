//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2014 Antoine Aubry and contributors

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
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System.IO;
using YamlDotNet.Core;

namespace YamlDotNetEditor.CommandHandlers
{
	public sealed class FormatDocumentCommandHandler : ICommandHandler
	{
		public VSConstants.VSStd2KCmdID CommandId
		{
			get { return VSConstants.VSStd2KCmdID.FORMATDOCUMENT; }
		}

		public bool IsEnabled(ITextView textView)
		{
			return true;
		}

		public void Execute(ITextView textView, ITextUndoHistoryRegistry textUndoHistoryRegistry)
		{
			var undoHistory = textUndoHistoryRegistry.RegisterHistory(textView);
			using (var transaction = undoHistory.CreateTransaction("Format Document"))
			{
				var text = textView.TextBuffer.CurrentSnapshot.GetText();

				var formatted = new StringWriter();
				var parser = new Parser(new Scanner(new StringReader(text), skipComments: false));
				var emitter = new Emitter(formatted);

				while (parser.MoveNext())
				{
					emitter.Emit(parser.Current);
				}

				var edit = textView.TextBuffer.CreateEdit();
				edit.Replace(0, text.Length, formatted.ToString());
				edit.Apply();

				transaction.Complete();
			}
		}
	}
}
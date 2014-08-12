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

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using YamlDotNetEditor.CommandHandlers;

namespace YamlDotNetEditor
{
	[Export(typeof(IVsTextViewCreationListener))]
	[ContentType("yaml")]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
	public sealed class CommandService : IVsTextViewCreationListener
	{
		[Import]
		internal IVsEditorAdaptersFactoryService EditorAdaptersFactoryService = null; // Set via MEF

		[Import]
		internal ITextUndoHistoryRegistry TextUndoHistoryRegistry = null; // Set via MEF

		public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			ITextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
			if (textView == null)
				return;

			var dispatcher = new CommandHandlerDispatcher(textViewAdapter, textView, TextUndoHistoryRegistry,
				new CommentSelectionCommandHandler(),
				new UncommentSelectionCommandHandler(),
				new FormatDocumentCommandHandler()
			);

			textView.Properties.AddProperty(typeof(CommandHandlerDispatcher), dispatcher);
		}
	}
}

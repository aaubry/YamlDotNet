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
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using YamlDotNetEditor.CommandHandlers;
using VSStd2KCmdID = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;

namespace YamlDotNetEditor
{
	[ComVisible(true)]
	internal sealed class CommandHandlerDispatcher : IOleCommandTarget, IDisposable
	{
		private readonly IVsTextView _textViewAdapter;
		private readonly ITextView _textView;
		private readonly ITextUndoHistoryRegistry _textUndoHistoryRegistry;
		private readonly Dictionary<VSStd2KCmdID, ICommandHandler> _commandHandlers;
		private readonly IOleCommandTarget _nextTarget;

		public CommandHandlerDispatcher(IVsTextView textViewAdapter, ITextView textView, ITextUndoHistoryRegistry textUndoHistoryRegistry, params ICommandHandler[] commandHandlers)
		{
			_textViewAdapter = textViewAdapter;
			_textView = textView;
			_textUndoHistoryRegistry = textUndoHistoryRegistry;
			_commandHandlers = commandHandlers.ToDictionary(h => h.CommandId);

			_textViewAdapter.AddCommandFilter(this, out _nextTarget);
		}

		int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			var cmd = (OLECMDEXECOPT)(ushort)(nCmdexecopt & (uint)0xffff);
			switch (cmd)
			{
				case OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT:
				case OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER:
				case OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER:

					ICommandHandler handler;
					if (pguidCmdGroup == typeof(VSStd2KCmdID).GUID && _commandHandlers.TryGetValue((VSStd2KCmdID)nCmdID, out handler))
					{
						handler.Execute(_textView, _textUndoHistoryRegistry);
						return VSConstants.S_OK;
					}
					else if (_nextTarget != null)
					{
						return _nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
					}
					break;

				case OLECMDEXECOPT.OLECMDEXECOPT_SHOWHELP:
				default:
					break;
			}

			return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
		}

		int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (_nextTarget != null)
			{
				var result = _nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
				if (result != (int)VSConstants.S_OK)
				{
					return result;
				}
			}

			for (uint i = 0; i < cCmds; ++i)
			{
				if (pguidCmdGroup == typeof(VSStd2KCmdID).GUID)
				{
					ICommandHandler handler;
					if (pguidCmdGroup == typeof(VSStd2KCmdID).GUID && _commandHandlers.TryGetValue((VSStd2KCmdID)prgCmds[i].cmdID, out handler))
					{
						if (_textView.TextBuffer.CheckEditAccess() && handler.IsEnabled(_textView))
						{
							prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
						}
						else
						{
							prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
						}
					}
				}
			}

			return VSConstants.S_OK;
		}

		void IDisposable.Dispose()
		{
			_textViewAdapter.RemoveCommandFilter(this);
		}
	}
}

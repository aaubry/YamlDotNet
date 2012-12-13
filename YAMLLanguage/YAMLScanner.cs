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

using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using YamlDotNet.Core;
using System.IO;
using YamlDotNet.Core.Tokens;
using System.Diagnostics;

namespace Company.YAMLLanguage
{
	public class YAMLScanner : IScanner
	{
		private IVsTextLines buffer;

		public YAMLScanner(IVsTextLines buffer)
		{
			this.buffer = buffer;
		}

		#region IScanner Members
		private string currentSource;
		private int currentOffset;

		private char? PeekNextChar()
		{
			return currentOffset < currentSource.Length ? (char?)currentSource[currentOffset] : null;
		}

		private char ReadNextChar()
		{
			return currentSource[currentOffset++];
		}

		private Match ReadMatch(Regex pattern)
		{
			Match match = pattern.Match(currentSource, currentOffset);
			if(!match.Success)
			{
				throw new InvalidOperationException("The match should always succeed.");
			}
			currentOffset += match.Length;
			return match;
		}

		private static readonly Regex textPattern = new Regex(@"[^,:\-\{\}\[\]]+", RegexOptions.Compiled);
		private static readonly Regex whitespacePattern = new Regex(@"\s+", RegexOptions.Compiled);
		private static readonly Regex anchorPattern = new Regex(@"[&*][^\s]+", RegexOptions.Compiled);
		private static readonly Regex commentPattern = new Regex(@"#.*", RegexOptions.Compiled);
		private static readonly Regex directivePattern = new Regex(@"%.*", RegexOptions.Compiled);
		private static readonly Regex tagPattern = new Regex(@"!(([^!]*!)|(<[^>]+>))?", RegexOptions.Compiled);

		public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
		{
			if(scannedTokens.Count > 0)
			{
				TokenInfo token = scannedTokens.Dequeue();
				tokenInfo.StartIndex = token.StartIndex;
				tokenInfo.EndIndex = token.EndIndex;
				tokenInfo.Type = token.Type;
				tokenInfo.Color = token.Color;
				return true;
			}
			return false;

			//char? current = PeekNextChar();
			//if(current == null)
			//{
			//    return false;
			//}

			//if (current.Value == '%' && currentOffset == 0)
			//{
			//    ScanDirectiveToken(tokenInfo);
			//}
			//else
			//{
			//    switch(current.Value)
			//    {
			//        case '-':
			//        case ':':
			//            ScanDelimiterToken(tokenInfo);
			//            break;

			//        case '[':
			//        case ']':
			//        case '{':
			//        case '}':
			//            ScanPairedDelimiterToken(tokenInfo);
			//            break;

			//        case ' ':
			//        case '\t':
			//            ScanWhitespaceToken(tokenInfo);
			//            break;

			//        case '&':
			//            ScanAnchorToken(tokenInfo);
			//            break;

			//        case '*':
			//            ScanAnchorReferenceToken(tokenInfo);
			//            break;

			//        case '!':
			//            ScanTagToken(tokenInfo);
			//            break;

			//        case '#':
			//            ScanCommentToken(tokenInfo);
			//            break;

			//        default:
			//            ScanTextToken(tokenInfo);
			//            break;
			//    }
			//}
			//return true;
		}

		private void ScanTagToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, tagPattern);
			tokenInfo.Type = TokenType.Literal;
			tokenInfo.Color = (TokenColor)ColorIndex.Tag;
		}

		private void ScanDirectiveToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, directivePattern);
			tokenInfo.Type = TokenType.Keyword;
			tokenInfo.Color = (TokenColor)ColorIndex.Directive;
		}

		private void ScanCommentToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, commentPattern);
			tokenInfo.Type = TokenType.LineComment;
			tokenInfo.Color = (TokenColor)ColorIndex.Comment;
		}

		private void ScanAnchorToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, anchorPattern);
			tokenInfo.Type = TokenType.Identifier;
			tokenInfo.Color = (TokenColor)ColorIndex.Anchor;
		}

		private void ScanAnchorReferenceToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, anchorPattern);
			tokenInfo.Type = TokenType.Identifier;
			tokenInfo.Color = (TokenColor)ColorIndex.AnchorReference;
		}

		private static readonly IDictionary<char, ColorIndex> delimiterColors = new Dictionary<char, ColorIndex>
		{
			{ ':', ColorIndex.MappingSeparator },
			{ '-', ColorIndex.SequenceItem },
			{ '[', ColorIndex.BlockSequence },
			{ ']', ColorIndex.BlockSequence },
			{ '{', ColorIndex.BlockMapping },
			{ '}', ColorIndex.BlockMapping },
			{ ',', ColorIndex.BlockSeparator },
		};

		private void ScanDelimiterToken(TokenInfo tokenInfo)
		{
			tokenInfo.StartIndex = currentOffset;
			tokenInfo.EndIndex = currentOffset;
			tokenInfo.Type = TokenType.Keyword;
			tokenInfo.Color = (TokenColor)delimiterColors[ReadNextChar()];
		}

		private void ScanPairedDelimiterToken(TokenInfo tokenInfo)
		{
			ScanDelimiterToken(tokenInfo);
			tokenInfo.Trigger = TokenTriggers.MatchBraces;
		}

		private void ScanTextToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, textPattern);
			tokenInfo.Type = TokenType.Literal;
			tokenInfo.Color = (TokenColor)ColorIndex.Text;
		}

		private void ScanWhitespaceToken(TokenInfo tokenInfo)
		{
			ScanRegexToken(tokenInfo, whitespacePattern);
			tokenInfo.Type = TokenType.WhiteSpace;
			tokenInfo.Color = (TokenColor)ColorIndex.Text;
		}

		private void ScanRegexToken(TokenInfo tokenInfo, Regex pattern)
		{
			Match match = ReadMatch(pattern);
			tokenInfo.StartIndex = match.Index;
			tokenInfo.EndIndex = match.Index + match.Length - 1;
		}

		private readonly Queue<TokenInfo> scannedTokens = new Queue<TokenInfo>();

		public void SetSource(string source, int offset)
		{
			if(offset > 0)
			{
				source = source.Substring(offset);
			}

			Scanner scanner = new Scanner(new StringReader(source));
			try
			{
				List<Token> tokens = new

				int currentOffset = 0;
				Token previous = null;
				while(scanner.MoveNext())
				{
					Token token = scanner.Current;
					Debug.WriteLine(token.GetType().Name);
					if(token.Start.Index != token.End.Index)
					{
						TokenInfoParsed(token, TokenType.String, ColorIndex.Text);
						currentOffset = token.End.Index;
						previous = token;
					}
				}
			}
			catch(YamlException)
			{
			}
		}

		private void TokenInfoParsed(Token token, TokenType type, ColorIndex color)
		{
			TokenInfo info = new TokenInfo(token.Start.Index, token.End.Index - 1, type);
			info.Color = (TokenColor)color;
			scannedTokens.Enqueue(info);
		}
		#endregion
	}
}
using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
			char? current = PeekNextChar();
			if(current == null)
			{
				return false;
			}

			if (current.Value == '%' && currentOffset == 0)
			{
				ScanDirectiveToken(tokenInfo);
			}
			else
			{
				switch(current.Value)
				{
					case '-':
					case ':':
						ScanDelimiterToken(tokenInfo);
						break;

					case '[':
					case ']':
					case '{':
					case '}':
						ScanPairedDelimiterToken(tokenInfo);
						break;

					case ' ':
					case '\t':
						ScanWhitespaceToken(tokenInfo);
						break;

					case '&':
						ScanAnchorToken(tokenInfo);
						break;

					case '*':
						ScanAnchorReferenceToken(tokenInfo);
						break;

					case '!':
						ScanTagToken(tokenInfo);
						break;

					case '#':
						ScanCommentToken(tokenInfo);
						break;

					default:
						ScanTextToken(tokenInfo);
						break;
				}
			}
			return true;
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

		public void SetSource(string source, int offset)
		{
			currentSource = source;
			currentOffset = offset;
		}
		#endregion
	}
}
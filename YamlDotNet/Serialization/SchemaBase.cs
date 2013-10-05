// -----------------------------------------------------------------------------------
// The following code is a partial port of YamlSerializer
// https://yamlserializer.codeplex.com
// -----------------------------------------------------------------------------------
// Copyright (c) 2009 Osamu TAKEUCHI <osamu@big.jp>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Base implementation for a based schema.
	/// </summary>
	public abstract class SchemaBase : IYamlSchema
	{
		private readonly Dictionary<string, string> shortTagToLongTag = new Dictionary<string, string>();
		private readonly Dictionary<string, string> longTagToShortTag = new Dictionary<string, string>();
		private readonly List<ScalarResolutionRule> scalarTagResolutionRules = new List<ScalarResolutionRule>();
		private readonly Dictionary<string, Regex> algorithms = new Dictionary<string, Regex>();

		private readonly Dictionary<string, List<ScalarResolutionRule>> mapTagToScalarResolutionRuleList =
			new Dictionary<string, List<ScalarResolutionRule>>();

		private readonly Dictionary<Type, ScalarResolutionRule> mapTypeToScalarResolutionRule =
			new Dictionary<Type, ScalarResolutionRule>();

		private int updateCountter;
		private bool needFirstUpdate = true;

		/// <summary>
		/// The string short tag: !!str
		/// </summary>
		public const string StrShortTag = "!!str";

		/// <summary>
		/// The string long tag: tag:yaml.org,2002:str
		/// </summary>
		public const string StrLongTag = "tag:yaml.org,2002:str";

		public string ExpandTag(string shortTag)
		{
			if (shortTag == null)
				return null;

			string tagExpanded;
			return shortTagToLongTag.TryGetValue(shortTag, out tagExpanded) ? tagExpanded : shortTag;
		}

		public string ShortenTag(string longTag)
		{
			if (longTag == null)
				return null;

			string tagShortened;
			return longTagToShortTag.TryGetValue(longTag, out tagShortened) ? tagShortened : longTag;
		}

		public string GetDefaultTag(NodeEvent nodeEvent)
		{
			EnsureScalarRules();

			if (nodeEvent == null) throw new ArgumentNullException("nodeEvent");

			var mapping = nodeEvent as MappingStart;
			if (mapping != null)
			{
				return GetDefaultTag(mapping);
			}

			var sequence = nodeEvent as SequenceStart;
			if (sequence != null)
			{
				return GetDefaultTag(sequence);
			}

			var scalar = nodeEvent as Scalar;
			if (scalar != null)
			{
				object value;
				string tag;
				TryParse(scalar, false, out tag, out value);
				return tag;
			}

			throw new NotSupportedException("NodeEvent [{0}] not supported".DoFormat(nodeEvent.GetType().FullName));
		}

		/// <summary>
		/// Registers a long/short tag association.
		/// </summary>
		/// <param name="shortTag">The short tag.</param>
		/// <param name="longTag">The long tag.</param>
		/// <exception cref="System.ArgumentNullException">
		/// shortTag
		/// or
		/// longTag
		/// </exception>
		protected void RegisterTag(string shortTag, string longTag)
		{
			if (shortTag == null) throw new ArgumentNullException("shortTag");
			if (longTag == null) throw new ArgumentNullException("longTag");

			shortTagToLongTag[shortTag] = longTag;
			longTagToShortTag[longTag] = shortTag;
		}

		/// <summary>
		/// Gets the default tag for a <see cref="MappingStart"/> event.
		/// </summary>
		/// <param name="nodeEvent">The node event.</param>
		/// <returns>The default tag for a map.</returns>
		protected abstract string GetDefaultTag(MappingStart nodeEvent);

		/// <summary>
		/// Gets the default tag for a <see cref="SequenceStart"/> event.
		/// </summary>
		/// <param name="nodeEvent">The node event.</param>
		/// <returns>The default tag for a seq.</returns>
		protected abstract string GetDefaultTag(SequenceStart nodeEvent);

		public virtual bool TryParse(Scalar scalar, bool parseValue, out string defaultTag, out object value)
		{
			EnsureScalarRules();

			defaultTag = null;
			value = null;

			// DoubleQuoted and SingleQuoted string are always decoded
			if (scalar.Style == ScalarStyle.DoubleQuoted || scalar.Style == ScalarStyle.SingleQuoted)
			{
				defaultTag = StrLongTag;
				if (parseValue)
				{
					value = scalar.Value;
				}
				return true;
			}

			// Parse only values if we have some rules
			if (scalarTagResolutionRules.Count > 0)
			{
				foreach (var rule in scalarTagResolutionRules)
				{
					var match = rule.Pattern.Match(scalar.Value);
					if (!match.Success) continue;

					defaultTag = rule.Tag;
					if (parseValue)
					{
						value = rule.Decode(match);
					}
					return true;
					break;
				}
			}
			else
			{
				// Expand the tag to a default tag.
				defaultTag = ExpandTag(scalar.Tag);
			}

			// Value was not successfully decoded
			return false;
		}

		public Type GetTypeForDefaultTag(string longTag)
		{
			EnsureScalarRules();

			List<ScalarResolutionRule> resolutionRules;
			if (mapTagToScalarResolutionRuleList.TryGetValue(longTag, out resolutionRules) && resolutionRules.Count > 0)
				return resolutionRules[0].GetTypeOfValue();

			return null;
		}

		/// <summary>
		/// Prepare scalar rules. In the implementation of this method, should call <see cref="AddScalarRule{T}"/>
		/// </summary>
		protected virtual void PrepareScalarRules()
		{
		}

		/// <summary>
		/// Add a tag resolution rule that is invoked when <paramref name="regex" /> matches
		/// the <see cref="Scalar">Value of</see> a <see cref="Scalar" /> node.
		/// The tag is resolved to <paramref name="tag" /> and <paramref name="decode" /> is
		/// invoked when actual value of type <typeparamref name="T" /> is extracted from
		/// the node text.
		/// </summary>
		/// <typeparam name="T">Type of the scalar</typeparam>
		/// <param name="tag">The tag.</param>
		/// <param name="regex">The regex.</param>
		/// <param name="decode">The decode function.</param>
		/// <param name="encode">The encode function.</param>
		/// <example>
		///   <code>
		/// BeginUpdate(); // to avoid invoking slow internal calculation method many times.
		/// Add( ... );
		/// Add( ... );
		/// Add( ... );
		/// Add( ... );
		/// EndUpdate();   // automaticall invoke internal calculation method
		///   </code></example>
		/// <remarks>Surround sequential calls of this function by <see cref="BeginScalarRuleUpdate" /> / <see cref="EndScalarRuleUpdate" />
		/// pair to avoid invoking slow internal calculation method many times.</remarks>
		protected void AddScalarRule<T>(string tag, string regex, Func<Match, T> decode, Func<T, string> encode)
		{
			// Make sure the tag is expanded to its long form
			var longTag = ExpandTag(tag);
			scalarTagResolutionRules.Add(new ScalarResolutionRule<T>(longTag, regex, decode, encode));
		}

		private void EnsureScalarRules()
		{
			if (needFirstUpdate || updateCountter != scalarTagResolutionRules.Count)
			{
				PrepareScalarRules();
				Update();
				needFirstUpdate = false;
			}
		}

		private void Update()
		{
			// Tag to joined regexp source
			var mapTagToPartialRegexPattern = new Dictionary<string, string>();
			foreach (var rule in scalarTagResolutionRules)
			{
				if (!mapTagToPartialRegexPattern.ContainsKey(rule.Tag))
				{
					mapTagToPartialRegexPattern.Add(rule.Tag, rule.PatternSource);
				}
				else
				{
					mapTagToPartialRegexPattern[rule.Tag] += "|" + rule.PatternSource;
				}
			}

			// Tag to joined regexp
			algorithms.Clear();
			foreach (var entry in mapTagToPartialRegexPattern)
			{
				algorithms.Add(
					entry.Key,
					new Regex("^(" + entry.Value + ")$")
					);
			}

			// Tag to decoding methods
			mapTagToScalarResolutionRuleList.Clear();
			foreach (var rule in scalarTagResolutionRules)
			{
				if (!mapTagToScalarResolutionRuleList.ContainsKey(rule.Tag))
					mapTagToScalarResolutionRuleList[rule.Tag] = new List<ScalarResolutionRule>();
				mapTagToScalarResolutionRuleList[rule.Tag].Add(rule);
			}

			mapTypeToScalarResolutionRule.Clear();
			foreach (var rule in scalarTagResolutionRules)
				if (rule.HasEncoder())
					mapTypeToScalarResolutionRule[rule.GetTypeOfValue()] = rule;

			// Update the counter
			updateCountter = scalarTagResolutionRules.Count;
		}

		private abstract class ScalarResolutionRule
		{
			public string Tag { get; protected set; }
			public Regex Pattern { get; protected set; }
			public string PatternSource { get; protected set; }
			public abstract object Decode(Match m);
			public abstract string Encode(object obj);
			public abstract Type GetTypeOfValue();
			public abstract bool HasEncoder();

			public bool IsMatch(string value)
			{
				return Pattern.IsMatch(value);
			}
		}

		private class ScalarResolutionRule<T> : ScalarResolutionRule
		{
			public ScalarResolutionRule(string longTag, string regex, Func<Match, T> decoder, Func<T, string> encoder)
			{
				Tag = longTag;
				PatternSource = regex;
				Pattern = new Regex("^(?:" + regex + ")$");
				Decoder = decoder;
				Encoder = encoder;
			}

			private Func<Match, T> Decoder;
			private Func<T, string> Encoder;

			public override object Decode(Match m)
			{
				return Decoder(m);
			}

			public override string Encode(object obj)
			{
				return Encoder((T) obj);
			}

			public override Type GetTypeOfValue()
			{
				return typeof (T);
			}

			public override bool HasEncoder()
			{
				return Encoder != null;
			}
		}
	}
}
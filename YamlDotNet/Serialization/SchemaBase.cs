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
		private readonly Dictionary<string, List<ScalarResolutionRule>> mapTagToScalarResolutionRuleList = new Dictionary<string, List<ScalarResolutionRule>>();
		private readonly Dictionary<Type, ScalarResolutionRule> mapTypeToScalarResolutionRule = new Dictionary<Type, ScalarResolutionRule>();
		private int updateCountter;
		private bool isInitialized;

		public string ExpandTag(string shortTag)
		{
			EnsureInitialized();
			string tagExpanded;
			return shortTagToLongTag.TryGetValue(shortTag, out tagExpanded) ? tagExpanded : shortTag;
		}

		public string ShortenTag(string longTag)
		{
			EnsureInitialized();
			string tagShortened;
			return longTagToShortTag.TryGetValue(longTag, out tagShortened) ? tagShortened : longTag;
		}

		public string GetDefaultTag(NodeEvent nodeEvent)
		{
			EnsureInitialized();

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
				DecodeScalar(scalar, false, out tag, out value);
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
		public void RegisterTag(string shortTag, string longTag)
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

		public virtual bool DecodeScalar(Scalar scalar, bool parseValue, out string defaultTag, out object value)
		{
			defaultTag = null;
			value = null;
			if (scalar.Tag == null || scalar.Value == null)
				return false;
			
			// Expand the tag to a default tag.
			defaultTag = ExpandTag(scalar.Tag);

			List<ScalarResolutionRule> scalarResolutionRules;
			// Parse only values if we have some rules
			if (parseValue && mapTagToScalarResolutionRuleList.Count > 0 && mapTagToScalarResolutionRuleList.TryGetValue(defaultTag, out scalarResolutionRules))
			{
				foreach (var rule in scalarResolutionRules)
				{
					var m = rule.Pattern.Match(scalar.Value);
					if (m.Success)
					{
						value = rule.Decode(m);
						return true;
					}
				}
			}

			// Value was not successfully decoded
			return false;
		}

		public Type GetDefaultTypeForTag(string tag)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Prepare scalar rules. In the implementation of this method, should call <see cref="AddScalarRule{T}"/>
		/// </summary>
		protected virtual void PrepareScalarRules()
		{
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		public void Initialize()
		{
			if (isInitialized) return;

			BeginScalarRuleUpdate();
			PrepareScalarRules();
			EndScalarRuleUpdate();
			isInitialized = true;
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
			if (updateCountter == 0)
			{
				throw new InvalidOperationException("AddScalarRule can only be called from PrepareScalarRules");
			}

			// Make sure the tag is expanded to its long form
			var longTag = ExpandTag(tag);
			scalarTagResolutionRules.Add(new ScalarResolutionRule<T>(longTag, regex, decode, encode));
		}

        /// <summary>
        /// Supress invoking slow internal calculation method when 
		/// <see cref="AddScalarRule&lt;T&gt;(string,string,Func&lt;Match,T&gt;,Func&lt;T,string&gt;)"/> called.
        /// 
        /// BeginUpdate / <see cref="EndScalarRuleUpdate"/> can be called nestedly.
        /// </summary>
        private void BeginScalarRuleUpdate()
        {
            updateCountter++;
        }

        /// <summary>
        /// Quit to supress invoking slow internal calculation method when 
		/// <see cref="AddScalarRule&lt;T&gt;(string,string,Func&lt;Match,T&gt;,Func&lt;T,string&gt;)"/> called.
        /// </summary>
        private void EndScalarRuleUpdate()
        {
            if ( updateCountter == 0 )
                throw new InvalidOperationException("BeginUpdate was not called");
            updateCountter--;

            if ( updateCountter == 0 )
                Update();
        }

		private void EnsureInitialized()
		{
			if (!isInitialized) throw new InvalidOperationException("This instance is not initialized. Call Initialize() method before using it");
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
			public bool IsMatch(string value) { return Pattern.IsMatch(value); }
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
				return Encoder((T)obj);
			}
			public override Type GetTypeOfValue()
			{
				return typeof(T);
			}
			public override bool HasEncoder()
			{
				return Encoder != null;
			}
		}
	}
}
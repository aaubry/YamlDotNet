using System;
using YamlDotNet.Events;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// Provides schema information for tag resolution. This is used by <see cref="ITagTypeRegistry"/>
	/// </summary>
	public interface IYamlSchema
	{
		/// <summary>
		/// Expands the tag. Example, transforms a short tag '!!str' to its long version 'tag:yaml.org,2002:str'
		/// </summary>
		/// <param name="shortTag">The tag.</param>
		/// <returns>Expanded version of the tag.</returns>
		string ExpandTag(string shortTag);

		/// <summary>
		/// Shortens the tag. Example, transforms a long tag 'tag:yaml.org,2002:str' to its short version '!!str'
		/// </summary>
		/// <param name="tag">The tag.</param>
		/// <returns>Expanded version of the tag.</returns>
		string ShortenTag(string tag);

		/// <summary>
		/// Gets the default tag for the specified <see cref="NodeEvent"/>. The default tag can be different from a actual tag of this <see cref="NodeEvent"/>.
		/// </summary>
		/// <param name="nodeEvent">The node event.</param>
		/// <returns>System.String.</returns>
		string GetDefaultTag(NodeEvent nodeEvent);

		/// <summary>
		/// Gets the default tag and value for the specified <see cref="Scalar" />. The default tag can be different from a actual tag of this <see cref="NodeEvent" />.
		/// </summary>
		/// <param name="scalar">The scalar event.</param>
		/// <param name="decodeValue">if set to <c>true</c> [decode value].</param>
		/// <param name="defaultTag">The default tag decoded from the scalar.</param>
		/// <param name="value">The value extracted from a scalar.</param>
		/// <returns>System.String.</returns>
		bool TryParse(Scalar scalar, bool decodeValue, out string defaultTag, out object value);

		/// <summary>
		/// Gets the type for a default tag.
		/// </summary>
		/// <param name="tag">The tag in long form.</param>
		/// <returns>The type for a default tag or null if no default tag associated</returns>
		Type GetTypeForDefaultTag(string tag);
	}
}
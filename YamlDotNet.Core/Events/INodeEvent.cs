
using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Specifies the interface that the events related to YAML nodes must implement.
	/// </summary>
	public interface INodeEvent
	{
		/// <summary>
		/// Gets the anchor.
		/// </summary>
		string Anchor
		{
			get;
		}

		/// <summary>
		/// Gets the tag.
		/// </summary>
		string Tag
		{
			get;
		}
	}
}
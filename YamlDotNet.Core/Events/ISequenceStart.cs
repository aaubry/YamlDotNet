using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a sequence start event.
	/// </summary>
	public interface ISequenceStart : INodeEvent
	{
		/// <summary>
		/// Gets the style.
		/// </summary>
		/// <value>The style.</value>
		SequenceStyle Style
		{
			get;
		}
	}
}
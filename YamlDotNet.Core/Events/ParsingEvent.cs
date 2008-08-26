using System;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Base class for parsing events.
	/// </summary>
	public abstract class ParsingEvent
	{
		private Mark start;

		/// <summary>
		/// Gets the position in the input stream where the event start.
		/// </summary>
		public Mark Start
		{
			get
			{
				return start;
			}
		}

		private Mark end;

		/// <summary>
		/// Gets the position in the input stream where the event ends.
		/// </summary>
		public Mark End
		{
			get
			{
				return end;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParsingEvent"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		protected ParsingEvent(Mark start, Mark end)
		{
			this.start = start;
			this.end = end;
		}
	}
}

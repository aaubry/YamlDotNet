using System;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a document start event.
	/// </summary>
	public class DocumentStart : ParsingEvent
	{
		private readonly IList<TagDirective> tags;
		private readonly VersionDirective version;

		/// <summary>
		/// Gets the tags.
		/// </summary>
		/// <value>The tags.</value>
		public IList<TagDirective> Tags
		{
			get
			{
				return tags;
			}
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		/// <value>The version.</value>
		public VersionDirective Version
		{
			get
			{
				return version;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentStart"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="tags">The tags.</param>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public DocumentStart(Tokens.VersionDirective version, IList<Tokens.TagDirective> tags, Mark start, Mark end)
			: base(start, end)
		{
			this.version = version;
			this.tags = tags;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentStart"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="tags">The tags.</param>
		public DocumentStart(Tokens.VersionDirective version, IList<Tokens.TagDirective> tags)
			: this(version, tags, Mark.Empty, Mark.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentStart"/> class.
		/// </summary>
		/// <param name="start">The start position of the event.</param>
		/// <param name="end">The end position of the event.</param>
		public DocumentStart(Mark start, Mark end)
			: this(null, null, start, end)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DocumentStart"/> class.
		/// </summary>
		public DocumentStart()
		{
		}
	}
}
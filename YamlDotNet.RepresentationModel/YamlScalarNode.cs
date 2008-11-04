using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents a scalar node in the YAML document.
	/// </summary>
	[DebuggerDisplay("{Value}")]
	public class YamlScalarNode : YamlNode
	{
		private string value;
		private ScalarStyle style;

		/// <summary>
		/// Gets or sets the value of the node.
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Gets or sets the style of the node.
		/// </summary>
		/// <value>The style.</value>
		public ScalarStyle Style
		{
			get
			{
				return style;
			}
			set
			{
				style = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
		/// </summary>
		/// <param name="events">The events.</param>
		/// <param name="state">The state.</param>
		internal YamlScalarNode(EventReader events, DocumentLoadingState state)
		{
			Scalar scalar = events.Expect<Scalar>();
			Load(scalar, state);
			value = scalar.Value;
			style = scalar.Style;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
		/// </summary>
		public YamlScalarNode()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlScalarNode"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public YamlScalarNode(string value)
		{
			this.value = value;
		}

		/// <summary>
		/// Resolves the aliases that could not be resolved when the node was created.
		/// </summary>
		/// <param name="state">The state of the document.</param>
		internal override void ResolveAliases(DocumentLoadingState state)
		{
			throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
		}
		
		internal override void Save(Emitter emitter)
		{
			emitter.Emit(new Scalar(Anchor, Tag, Value, ScalarStyle.Any, false, false));
		}
		
		/// <summary>
		/// Accepts the specified visitor by calling the appropriate Visit method on it.
		/// </summary>
		/// <param name="visitor">
		/// A <see cref="IYamlVisitor"/>.
		/// </param>
		public override void Accept(IYamlVisitor visitor) {
			visitor.Visit(this);
		}
	}
}
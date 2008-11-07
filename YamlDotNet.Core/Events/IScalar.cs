namespace YamlDotNet.Core.Events
{
	/// <summary>
	/// Represents a scalar event.
	/// </summary>
	public interface IScalar : INodeEvent
	{
		/// <summary>
		/// Gets the style of the scalar.
		/// </summary>
		/// <value>The style.</value>
		ScalarStyle Style
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the tag is optional for the plain style.
		/// </summary>
		bool IsPlainImplicit
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the tag is optional for any non-plain style.
		/// </summary>
		bool IsQuotedImplicit
		{
			get;
		}
	}
}
using System;
using System.Runtime.Serialization;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Base exception that is thrown when the a problem occurs in the YamlDotNet library.
	/// </summary>
	[Serializable]
	public class YamlException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="YamlException"/> class.
		/// </summary>
		public YamlException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public YamlException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public YamlException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected YamlException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

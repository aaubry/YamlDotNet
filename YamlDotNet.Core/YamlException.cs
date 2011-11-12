using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Base exception that is thrown when the a problem occurs in the YamlDotNet library.
	/// </summary>
	[Serializable]
	public class YamlException : Exception
	{
		/// <summary>
		/// Gets the position in the input stream where the event that originated the exception starts.
		/// </summary>
		public Mark Start { get; private set; }

		/// <summary>
		/// Gets the position in the input stream where the event that originated the exception ends.
		/// </summary>
		public Mark End { get; private set; }

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
		public YamlException(Mark start, Mark end, string message)
			: this(start, end, message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlException"/> class.
		/// </summary>
		public YamlException(Mark start, Mark end, string message, Exception innerException)
			: base(string.Format("({0}) - ({1}): {2}", start, end, message), innerException)
		{
			Start = start;
			End = end;
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
			Start = (Mark)info.GetValue("Start", typeof(Mark));
			End = (Mark)info.GetValue("End", typeof(Mark));
		}

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
		/// </PermissionSet>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Start", Start);
			info.AddValue("End", End);
		}
	}
}

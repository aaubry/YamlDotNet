using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace YamlDotNet.Core
{
	/// <summary>
	/// Exception that is thrown when a syntax error is detected on a YAML stream.
	/// </summary>
	[Serializable]
	public class SyntaxErrorException : Exception
	{
		private readonly Mark location;

		/// <summary>
		/// Gets the location where the exception has occured.
		/// </summary>
		/// <value>The location.</value>
		public Mark Location
		{
			get
			{
				return location;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
		/// </summary>
		/// <param name="description">The description.</param>
		/// <param name="location">The location where the exception has occured.</param>
		public SyntaxErrorException(string description, Mark location)
			: base(string.Format(CultureInfo.InvariantCulture, "({0}, {1}): {2}", location.Line + 1, location.Column + 1, description))
		{
			this.location = location;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
		/// </summary>
		public SyntaxErrorException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public SyntaxErrorException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public SyntaxErrorException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			location = (Mark)info.GetValue("location", typeof(Mark));
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
			info.AddValue("location", location);
		}
	}
}
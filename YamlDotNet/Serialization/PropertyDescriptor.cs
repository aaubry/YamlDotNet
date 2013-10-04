using System;
using System.Reflection;

namespace YamlDotNet.Serialization
{
	/// <summary>
	/// A <see cref="IMemberDescriptor"/> for a <see cref="PropertyInfo"/>
	/// </summary>
	public class PropertyDescriptor : MemberDescriptorBase
	{
		private readonly PropertyInfo propertyInfo;
		private readonly MethodInfo getMethod;
		private readonly MethodInfo setMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyDescriptor"/> class.
		/// </summary>
		/// <param name="propertyInfo">The property information.</param>
		public PropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");

			this.propertyInfo = propertyInfo;

			getMethod = propertyInfo.GetGetMethod(false);
			if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(false) != null)
			{
				setMethod = propertyInfo.GetSetMethod(false);
			}
		}

		/// <summary>
		/// Gets the property information attached to this instance.
		/// </summary>
		/// <value>The property information.</value>
		public PropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
		}

		public override Type Type
		{
			get { return propertyInfo.PropertyType; }
		}

		public override object Get(object thisObject)
		{
			return getMethod.Invoke(thisObject, null);
		}

		public override void Set(object thisObject, object value)
		{
			if (HasSet)
				setMethod.Invoke(thisObject, new [] {value});
		}

		public override bool HasSet
		{
			get { return setMethod != null; }
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="System.String" /> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format("Property [{0}] from Type [{1}]", Name,  PropertyInfo.DeclaringType != null ? PropertyInfo.DeclaringType.FullName : string.Empty);
		}
	}
}

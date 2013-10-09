using System;
using System.Reflection;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Base class for <see cref="IMemberDescriptor"/> for a <see cref="MemberInfo"/>
	/// </summary>
	public abstract class MemberDescriptorBase : IMemberDescriptor
	{
		protected MemberDescriptorBase(MemberInfo memberInfo)
		{
			if (memberInfo == null) throw new ArgumentNullException("memberInfo");

			MemberInfo = memberInfo;
			Name = MemberInfo.Name;
			DeclaringType = memberInfo.DeclaringType;
			Order = -1;
		}

		public string Name { get; internal set; }
		public abstract Type Type { get; }
		public int Order { get; internal set; }

		/// <summary>
		/// Gets the type of the declaring this member.
		/// </summary>
		/// <value>The type of the declaring.</value>
		public Type DeclaringType { get; private set; }

		public SerializeMemberMode SerializeMemberMode { get; internal set; }
		public abstract object Get(object thisObject);
		public abstract void Set(object thisObject, object value);
		public abstract bool HasSet { get; }
		public Func<object, bool> ShouldSerialize { get; internal set; }

		/// <summary>
		/// Gets the member information.
		/// </summary>
		/// <value>The member information.</value>
		public MemberInfo MemberInfo { get; private set; }
	}
}
// -----------------------------------------------------------------------------------
// The following code is a partial port of YamlSerializer
// https://yamlserializer.codeplex.com
// -----------------------------------------------------------------------------------
// Copyright (c) 2009 Osamu TAKEUCHI <osamu@big.jp>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in the 
// Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.Serialization.Descriptors
{
	/// <summary>
	/// Default implementation of a <see cref="ITypeDescriptor"/>.
	/// </summary>
	public class ObjectDescriptor : ITypeDescriptor
	{
		private readonly static object[] EmptyObjectArray = new object[0];
		private readonly Type type;
		private readonly IMemberDescriptor[] members;
		private readonly Dictionary<string, IMemberDescriptor> mapMembers;
		private bool emitDefaultValues;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
		/// </summary>
		/// <param name="attributeRegistry">The attribute registry.</param>
		/// <param name="type">The type.</param>
		/// <param name="emitDefaultValues"></param>
		/// <exception cref="System.ArgumentNullException">type</exception>
		/// <exception cref="YamlException">Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}].DoFormat(type.FullName, member, existingMember)</exception>
		public ObjectDescriptor(IAttributeRegistry attributeRegistry, Type type, bool emitDefaultValues)
		{
			if (attributeRegistry == null) throw new ArgumentNullException("attributeRegistry");
			if (type == null) throw new ArgumentNullException("type");

			this.emitDefaultValues = emitDefaultValues;
			this.AttributeRegistry = attributeRegistry;
			this.type = type;
			var memberList = PrepareMembers();

			// Sort members by name
			// This is to make sure that properties/fields for an object 
			// are always displayed in the same order
			memberList.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));

			// Free the member list
			this.members = memberList.ToArray();

			// If no members found, we don't need to build a dictionary map
			if (members.Length <= 0) return;

			mapMembers = new Dictionary<string, IMemberDescriptor>(members.Length);
			
			foreach (var member in members)
			{
				IMemberDescriptor existingMember;
				if (mapMembers.TryGetValue(member.Name, out existingMember))
				{
					throw new YamlException("Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}]".DoFormat(type.FullName, member, existingMember));
				}

				mapMembers.Add(member.Name, member);
			}
		}

		protected IAttributeRegistry AttributeRegistry { get; private set; }

		public Type Type
		{
			get { return type; }
		}

		public IEnumerable<IMemberDescriptor> Members
		{
			get { return members; }
		}

		public int Count
		{
			get { return members.Length; }
		}

		public bool HasMembers
		{
			get { return members.Length > 0; }
		}

		public IMemberDescriptor this[string name]
		{
			get 
			{ 
				if (mapMembers == null) throw new KeyNotFoundException(name);
				return mapMembers[name];
			}
		}

		public bool Contains(string memberName)
		{
			return mapMembers != null && mapMembers.ContainsKey(memberName);
		}

		protected virtual List<IMemberDescriptor> PrepareMembers()
		{

			// Add all public properties with a readable get method
			var memberList = (from propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			                  where
				                  propertyInfo.CanRead && propertyInfo.GetGetMethod(false) != null &&
				                  propertyInfo.GetIndexParameters().Length == 0
			                  select new PropertyDescriptor(propertyInfo)
			                  into member
			                  where PrepareMember(member)
			                  select member).Cast<IMemberDescriptor>().ToList();

			// Add all public fields
			memberList.AddRange((from fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
			                     where fieldInfo.IsPublic
			                     select new FieldDescriptor(fieldInfo)
			                     into member where PrepareMember(member) select member));

			return memberList;
		}

		protected virtual bool PrepareMember(MemberDescriptorBase member)
		{
			var memberType = member.Type;

			// If the member has a set, this is a conventional assign method
			if (member.HasSet)
			{
				member.SerializeMemberMode = SerializeMemberMode.Assign;
			}
			else
			{
				// Else we cannot only assign its content if it is a class
				member.SerializeMemberMode = memberType.IsClass || memberType.IsInterface ? SerializeMemberMode.Content : SerializeMemberMode.Never;
			}


			// Member is not displayed if there is a YamlIgnore attribute on it
			if (AttributeRegistry.GetAttribute<YamlIgnoreAttribute>(member.MemberInfo, false) != null)
				return false; 

			var memberAttribute = AttributeRegistry.GetAttribute<YamlMemberAttribute>(member.MemberInfo, false);
			if (memberAttribute != null)
			{
				if (!member.HasSet)
				{
					if (memberAttribute.SerializeMethod == SerializeMemberMode.Assign ||
						(memberType.IsValueType && member.SerializeMemberMode == SerializeMemberMode.Content))
						throw new ArgumentException("{0} {1} is not writeable by {2}.".DoFormat(memberType.FullName, member.Name, memberAttribute.SerializeMethod.ToString()));
				}
				member.SerializeMemberMode = memberAttribute.SerializeMethod;
			}

			if (member.SerializeMemberMode == SerializeMemberMode.Binary)
			{
				if (!memberType.IsArray)
					throw new InvalidOperationException("{0} {1} of {2} is not an array. Can not be serialized as binary."
															.DoFormat(memberType.FullName, member.Name, type.FullName));
				if (!memberType.GetElementType().IsPureValueType())
					throw new InvalidOperationException("{0} is not a pure ValueType. {1} {2} of {3} can not serialize as binary.".DoFormat(memberType.GetElementType(), memberType.FullName, member.Name, type.FullName));
			}

			// If this member cannot be serialized, remove it from the list
			if (member.SerializeMemberMode == SerializeMemberMode.Never)
			{
				return false;
			}

			// ShouldSerialize
			//      YamlSerializeAttribute(Never) => false
			//      ShouldSerializeSomeProperty => call it
			//      DefaultValueAttribute(default) => compare to it
			//      otherwise => true
			var shouldSerialize = type.GetMethod("ShouldSerialize" + member.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool) && member.ShouldSerialize == null)
				member.ShouldSerialize = obj => (bool)shouldSerialize.Invoke(obj, EmptyObjectArray);


			var defaultValueAttribute = AttributeRegistry.GetAttribute<DefaultValueAttribute>(member.MemberInfo);
			if (defaultValueAttribute != null && member.ShouldSerialize == null && !emitDefaultValues)
			{
				object defaultValue = defaultValueAttribute.Value;
				Type defaultType = defaultValue == null ? null : defaultValue.GetType();
				if (defaultType.IsNumeric() && defaultType != memberType)
					defaultValue = memberType.CastToNumericType(defaultValue);
				member.ShouldSerialize = obj => !TypeExtensions.AreEqual(defaultValue, member.Get(obj));
			}

			if (member.ShouldSerialize == null)
				member.ShouldSerialize = obj => true;

			if (memberAttribute != null && !string.IsNullOrEmpty(memberAttribute.Name))
			{
				member.Name = memberAttribute.Name;
			}

			return true;
		}
	}
}
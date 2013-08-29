using System;
using System.Collections.Generic;
using System.Globalization;

namespace YamlDotNet.RepresentationModel.Serialization.ObjectGraphVisitors
{
	public sealed class AnchorAssigner : IObjectGraphVisitor, IAliasProvider
	{
		private class AnchorAssignment
		{
			public string Anchor;
		}

		private readonly IDictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();
		private uint nextId;

		bool IObjectGraphVisitor.Enter(IObjectDescriptor value)
		{
			// Do not assign anchors to basic types
			if (value.Value == null || Type.GetTypeCode(value.Type) != TypeCode.Object)
			{
				return false;
			}

			AnchorAssignment assignment;
			if (assignments.TryGetValue(value.Value, out assignment))
			{
				if (assignment.Anchor == null)
				{
					assignment.Anchor = "o" + nextId.ToString(CultureInfo.InvariantCulture);
					++nextId;
				}
				return false;
			}
			else
			{
				assignments.Add(value.Value, new AnchorAssignment());
				return true;
			}
		}

		bool IObjectGraphVisitor.EnterMapping(IObjectDescriptor key, IObjectDescriptor value)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor key, object value)
		{
			return true;
		}

		void IObjectGraphVisitor.VisitScalar(IObjectDescriptor scalar) { }
		void IObjectGraphVisitor.VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType) { }
		void IObjectGraphVisitor.VisitMappingEnd(IObjectDescriptor mapping) { }
		void IObjectGraphVisitor.VisitSequenceStart(IObjectDescriptor sequence, Type elementType) { }
		void IObjectGraphVisitor.VisitSequenceEnd(IObjectDescriptor sequence) { }

		string IAliasProvider.GetAlias(object target)
		{
			AnchorAssignment assignment;
			if (target != null && assignments.TryGetValue(target, out assignment))
			{
				return assignment.Anchor;
			}
			return null;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public sealed class AnchorAssigner : IObjectGraphVisitor, IAliasProvider
	{
		private class AnchorAssignment
		{
			public string Anchor;
		}

		private readonly IDictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();
		private uint nextId;

		bool IObjectGraphVisitor.Enter(object value, Type type)
		{
			// Do not assign anchors to basic types
			if (value == null || Type.GetTypeCode(value.GetType()) != TypeCode.Object)
			{
				return false;
			}

			AnchorAssignment assignment;
			if (assignments.TryGetValue(value, out assignment))
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
				assignments.Add(value, new AnchorAssignment());
				return true;
			}
		}

		bool IObjectGraphVisitor.EnterMapping(object key, Type keyType, object value, Type valueType)
		{
			return true;
		}

		bool IObjectGraphVisitor.EnterMapping(IPropertyDescriptor key, object value)
		{
			return true;
		}

		void IObjectGraphVisitor.VisitScalar(object scalar, Type scalarType) { }
		void IObjectGraphVisitor.VisitMappingStart(object mapping, Type mappingType, Type keyType, Type valueType) { }
		void IObjectGraphVisitor.VisitMappingEnd(object mapping, Type mappingType) { }
		void IObjectGraphVisitor.VisitSequenceStart(object sequence, Type sequenceType, Type elementType) { }
		void IObjectGraphVisitor.VisitSequenceEnd(object sequence, Type sequenceType) { }

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
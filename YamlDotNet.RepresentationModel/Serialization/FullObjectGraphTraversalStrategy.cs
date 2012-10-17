using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// An implementation of <see cref="IObjectGraphTraversalStrategy"/> that traverses
	/// readable properties, collections and dictionaries.
	/// </summary>
	public class FullObjectGraphTraversalStrategy : IObjectGraphTraversalStrategy
	{
		private readonly int maxRecursion;

		public FullObjectGraphTraversalStrategy(int maxRecursion)
		{
			if(maxRecursion <= 0)
			{
				throw new ArgumentOutOfRangeException("maxRecursion", maxRecursion, "maxRecursion must be greater than 1");
			}

			this.maxRecursion = maxRecursion;
		}

		void IObjectGraphTraversalStrategy.Traverse(object graph, Type type, IObjectGraphVisitor visitor)
		{
			Traverse(graph, type, visitor, 0);
		}

		protected virtual void Traverse(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			if (++currentDepth > maxRecursion)
			{
				throw new InvalidOperationException("Too much recursion when traversing the object graph");
			}

			if(!visitor.Enter(value, type))
			{
				return;
			}

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.String:
				case TypeCode.Char:
				case TypeCode.DateTime:
					visitor.VisitScalar(value, type);
					break;

				case TypeCode.DBNull:
					visitor.VisitScalar(null, type);
					break;

				case TypeCode.Empty:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "TypeCode.{0} is not supported.", typeCode));

				default:
					if (value == null)
					{
						visitor.VisitScalar(value, type);
					}
					else
					{
						TraverseObject(value, type, visitor, currentDepth);
					}
					break;
			}
		}

		protected virtual void TraverseObject(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				TraverseDictionary(value, type, visitor, currentDepth);
				return;
			}

			var dictionaryType = ReflectionUtility.GetImplementedGenericInterface(type, typeof(IDictionary<,>));
			if (dictionaryType != null)
			{
				TraverseGenericDictionary(value, type, dictionaryType, visitor);
				return;
			}

			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				SerializeList(value, type, visitor, currentDepth);
				return;
			}

			SerializeProperties(value, type, visitor, currentDepth);
		}

		protected virtual void TraverseDictionary(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			visitor.VisitMappingStart(value, type, typeof(object), typeof(object));

			foreach (DictionaryEntry entry in (IDictionary)value)
			{
				var keyType = GetObjectType(entry.Key);
				var valueType = GetObjectType(entry.Value);
				if (visitor.EnterMapping(entry.Key, keyType, entry.Value, valueType))
				{
					Traverse(entry.Key, keyType, visitor, currentDepth);
					Traverse(entry.Value, valueType, visitor, currentDepth);
				}
			}

			visitor.VisitMappingEnd(value, type);
		}

		private void TraverseGenericDictionary(object value, Type type, Type dictionaryType, IObjectGraphVisitor visitor)
		{
			var entryTypes = dictionaryType.GetGenericArguments();

			// dictionaryType is IDictionary<TKey, TValue>
			visitor.VisitMappingStart(value, type, entryTypes[0], entryTypes[1]);

			// Invoke TraverseGenericDictionaryHelper<,>
			traverseGenericDictionaryHelperGeneric
				.MakeGenericMethod(entryTypes)
				.Invoke(null, new object[] { this, value, visitor });

			visitor.VisitMappingEnd(value, type);
		}

		private static readonly MethodInfo traverseGenericDictionaryHelperGeneric =
			ReflectionUtility.GetMethod(() => TraverseGenericDictionaryHelper<int, int>(null, null, null, 0));

		private static void TraverseGenericDictionaryHelper<TKey, TValue>(
			// "this" is passed as parameter so that we can cache the generic method definition
			FullObjectGraphTraversalStrategy self,
			IDictionary<TKey, TValue> value,
			IObjectGraphVisitor visitor, int currentDepth)
		{
			foreach (var entry in value)
			{
				if (visitor.EnterMapping(entry.Key, typeof(TKey), entry.Value, typeof(TValue)))
				{
					self.Traverse(entry.Key, typeof(TKey), visitor, currentDepth);
					self.Traverse(entry.Value, typeof(TValue), visitor, currentDepth);
				}
			}
		}

		private void SerializeList(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			var enumerableType = ReflectionUtility.GetImplementedGenericInterface(type, typeof(IEnumerable<>));
			var itemType = enumerableType != null ? enumerableType.GetGenericArguments()[0] : typeof(object);

			visitor.VisitSequenceStart(value, type, itemType);

			foreach (var item in (IEnumerable)value)
			{
				Traverse(item, itemType, visitor, currentDepth);
			}

			visitor.VisitSequenceEnd(value, type);
		}

		protected virtual void SerializeProperties(object value, Type type, IObjectGraphVisitor visitor, int currentDepth)
		{
			visitor.VisitMappingStart(value, type, typeof(string), typeof(object));

			foreach (var property in GetTraversableProperties(type))
			{
				var propertyValue = property.GetValue(value, null);
				var propertyType = property.PropertyType;

				if(visitor.EnterMapping(property.Name, typeof(string), propertyValue, propertyType))
				{
					Traverse(property.Name, typeof(string), visitor, currentDepth);
					Traverse(propertyValue, propertyType, visitor, currentDepth);
				}
			}

			visitor.VisitMappingEnd(value, type);
		}

		private IEnumerable<PropertyInfo> GetTraversableProperties(Type type)
		{
			return type
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(IsTraversableProperty);
		}

		protected virtual bool IsTraversableProperty(PropertyInfo property)
		{
      return
        property.CanRead &&
        property.GetGetMethod().GetParameters().Length == 0 &&
        property.GetCustomAttributes(typeof(YamlIgnoreAttribute), true).Length == 0;
		}

		private static Type GetObjectType(object value)
		{
			return value != null ? value.GetType() : typeof(object);
		}
	}
}
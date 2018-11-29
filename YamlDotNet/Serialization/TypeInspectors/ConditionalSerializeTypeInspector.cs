using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.Serialization.TypeInspectors
{
    /// <summary>
    /// On serialization searches for functions that match the key '{PREFIX}{PROPERTY_NAME}' 
    /// The property is not serialized iff the function can be properly invoked and the return
    /// value equals false.
    /// 
    /// Example of the string property ID which only serializes if the value is not null or empty.
    /// <code>
    ///     public string ID
    ///     {
    ///         get;
    ///         set;
    ///     }
    ///     
    ///     
    ///     public bool ShouldSerializeID()
    ///     {
    ///         return !String.IsNullOrEmpty(ID);
    ///     }
    /// </code>
    /// </summary>
    public sealed class ConditionalSerializeTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;
        private readonly string _prefix;

        public ConditionalSerializeTypeInspector(ITypeInspector innerTypeDescriptor, string prefix = "ShouldSerialize")
        {
            this._innerTypeDescriptor = innerTypeDescriptor;
            this._prefix = prefix;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            var kvMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .ToLookup(method => method.Name, method => method);

            return _innerTypeDescriptor.GetProperties(type, container)
                .Where(p =>
                {
                    var key = _prefix + p.Name;

                    return kvMethods[key]
                            .DefaultIfEmpty(null)
                            .FirstOrDefault()
                            ?.Invoke(container, null)// ?. not found
                            ?.Equals(true) ?? true;  // ?. return type was void or some other error 
                                                     // ?? we only skip serializing if we found assoicated function and return value is false
                });
        }
    }
}


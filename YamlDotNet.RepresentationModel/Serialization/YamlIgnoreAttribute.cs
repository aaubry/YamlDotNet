using System;

namespace YamlDotNet.RepresentationModel.Serialization
{
  /// <summary>
  /// Instructs the YamlSerializer not to serialize the public field or public read/write property value.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public sealed class YamlIgnoreAttribute : Attribute
  {
  }
}


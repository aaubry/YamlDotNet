using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public abstract class EventInfo
	{
		public object SourceValue { get; private set; }
		public Type SourceType { get; private set; }

		protected EventInfo(object sourceValue, Type sourceType)
		{
			SourceValue = sourceValue;
			SourceType = sourceType;
		}
	}

	public class AliasEventInfo : EventInfo
	{
		public AliasEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}

		public string Alias { get; set; }
	}

	public class ObjectEventInfo : EventInfo
	{
		protected ObjectEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}

		public string Anchor { get; set; }
		public string Tag { get; set; }
	}

	public sealed class ScalarEventInfo : ObjectEventInfo
	{
		public ScalarEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}

		public string RenderedValue { get; set; }
		public ScalarStyle Style { get; set; }
		public bool IsPlainImplicit { get; set; }
		public bool IsQuotedImplicit { get; set; }
	}

	public sealed class MappingStartEventInfo : ObjectEventInfo
	{
		public MappingStartEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}

		public bool IsImplicit { get; set; }
		public MappingStyle Style { get; set; }
	}

	public sealed class MappingEndEventInfo : EventInfo
	{
		public MappingEndEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}
	}

	public sealed class SequenceStartEventInfo : ObjectEventInfo
	{
		public SequenceStartEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}

		public bool IsImplicit { get; set; }
		public SequenceStyle Style { get; set; }
	}

	public sealed class SequenceEndEventInfo : EventInfo
	{
		public SequenceEndEventInfo(object sourceValue, Type sourceType)
			: base(sourceValue, sourceType)
		{
		}
	}
}
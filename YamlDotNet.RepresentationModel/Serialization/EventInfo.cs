using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization
{
	public abstract class EventInfo
	{
		public IObjectDescriptor Source { get; private set; }

		protected EventInfo(IObjectDescriptor source)
		{
			Source = source;
		}
	}

	public class AliasEventInfo : EventInfo
	{
		public AliasEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}

		public string Alias { get; set; }
	}

	public class ObjectEventInfo : EventInfo
	{
		protected ObjectEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}

		public string Anchor { get; set; }
		public string Tag { get; set; }
	}

	public sealed class ScalarEventInfo : ObjectEventInfo
	{
		public ScalarEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}

		public string RenderedValue { get; set; }
		public ScalarStyle Style { get; set; }
		public bool IsPlainImplicit { get; set; }
		public bool IsQuotedImplicit { get; set; }
	}

	public sealed class MappingStartEventInfo : ObjectEventInfo
	{
		public MappingStartEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}

		public bool IsImplicit { get; set; }
		public MappingStyle Style { get; set; }
	}

	public sealed class MappingEndEventInfo : EventInfo
	{
		public MappingEndEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}
	}

	public sealed class SequenceStartEventInfo : ObjectEventInfo
	{
		public SequenceStartEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}

		public bool IsImplicit { get; set; }
		public SequenceStyle Style { get; set; }
	}

	public sealed class SequenceEndEventInfo : EventInfo
	{
		public SequenceEndEventInfo(IObjectDescriptor source)
			: base(source)
		{
		}
	}
}
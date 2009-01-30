using System;
using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// 
	/// </summary>
	public class LazyDeserializationValue<T> : IYamlSerializable
	{
		private T value;
		private List<ParsingEvent> events;
		
		public LazyDeserializationValue()
		{
		}

		public void ReadYaml(Parser parser) {
			events = new List<ParsingEvent>();
			
			int depth = 0;
			do {
				if(!parser.MoveNext()) {
					throw new InvalidOperationException("The parser has reached the end before deserialization completed.");
				}

				events.Add(parser.Current);
				depth += parser.Current.NestingIncrease;
			} while(depth > 0);

			Debug.Assert(depth == 0);
		}
		
		public void WriteYaml(Emitter emitter) {
		}
	}
}

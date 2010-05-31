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

		/// <summary>
		/// Initializes a new instance of the <see cref="LazyDeserializationValue&lt;T&gt;"/> class.
		/// </summary>
		public LazyDeserializationValue()
		{
		}

		/// <summary>
		/// Reads this object's state from a YAML parser.
		/// </summary>
		/// <param name="parser"></param>
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

		/// <summary>
		/// Writes this object's state to a YAML emitter.
		/// </summary>
		/// <param name="emitter"></param>
		public void WriteYaml(Emitter emitter) {
		}
	}
}

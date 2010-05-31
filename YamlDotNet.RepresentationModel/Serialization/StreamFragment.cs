using System;
using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// An object that contains part of a YAML stream.
	/// </summary>
	public class StreamFragment : IYamlSerializable
	{
		private readonly List<ParsingEvent> events = new List<ParsingEvent>();

		/// <summary>
		/// Gets or sets the events.
		/// </summary>
		/// <value>The events.</value>
		public IList<ParsingEvent> Events
		{
			get
			{
				return events;
			}
		}

		#region IYamlSerializable Members
		/// <summary>
		/// Reads this object's state from a YAML parser.
		/// </summary>
		/// <param name="parser"></param>
		void IYamlSerializable.ReadYaml(Parser parser)
		{
			events.Clear();

			int depth = 0;
			do
			{
				if (!parser.MoveNext())
				{
					throw new InvalidOperationException("The parser has reached the end before deserialization completed.");
				}

				events.Add(parser.Current);
				depth += parser.Current.NestingIncrease;
			} while (depth > 0);

			Debug.Assert(depth == 0);
		}

		/// <summary>
		/// Writes this object's state to a YAML emitter.
		/// </summary>
		/// <param name="emitter"></param>
		void IYamlSerializable.WriteYaml(Emitter emitter)
		{
			foreach (var item in events)
			{
				emitter.Emit(item);
			}
		}
		#endregion
	}
}
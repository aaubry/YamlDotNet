using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents an YAML stream.
	/// </summary>
	public class YamlStream
	{
		private IList<YamlDocument> documents;

		/// <summary>
		/// Gets the documents inside the stream.
		/// </summary>
		/// <value>The documents.</value>
		public IList<YamlDocument> Documents
		{
			get
			{
				return documents;
			}
		}

		/// <summary>
		/// Loads the stream from the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		public void Load(TextReader input)
		{
			documents = new List<YamlDocument>();

			Parser parser = new Parser(input);

			EventReader events = new EventReader(parser);
			events.Expect<StreamStart>();
			while (!events.Accept<StreamEnd>())
			{
				YamlDocument document = new YamlDocument(events);
				documents.Add(document);
			}
			events.Expect<StreamEnd>();
		}

		//public void Save(Stream output)
		//{
		//    using(Emitter emitter = new Emitter(output))
		//    {
		//        //emitter.Emit();
		//    }
		//}
	}
}
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

		/// <summary>
		/// Saves the stream to the specified output.
		/// </summary>
		/// <param name="output">The output.</param>
		public void Save(TextWriter output)
		{
		    Emitter emitter = new Emitter(output);
			emitter.Emit(new StreamStart());
			
			foreach (var document in documents) {
				document.Save(emitter);
			}
			
			emitter.Emit(new StreamEnd());
		}
		
		/// <summary>
		/// Accepts the specified visitor by calling the appropriate Visit method on it.
		/// </summary>
		/// <param name="visitor">
		/// A <see cref="IYamlVisitor"/>.
		/// </param>
		public void Accept(IYamlVisitor visitor) {
			visitor.Visit(this);
		}
	}
}
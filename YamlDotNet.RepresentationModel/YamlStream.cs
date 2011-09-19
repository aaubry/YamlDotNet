using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents an YAML stream.
	/// </summary>
	public class YamlStream : IEnumerable<YamlDocument>
	{
		private readonly IList<YamlDocument> documents = new List<YamlDocument>();

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
		/// Initializes a new instance of the <see cref="YamlStream"/> class.
		/// </summary>
		public YamlStream()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlStream"/> class.
		/// </summary>
		public YamlStream(params YamlDocument[] documents)
			: this((IEnumerable<YamlDocument>)documents)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlStream"/> class.
		/// </summary>
		public YamlStream(IEnumerable<YamlDocument> documents)
		{
			foreach (var document in documents)
			{
				this.documents.Add(document);
			}
		}

		/// <summary>
		/// Adds the specified document to the <see cref="Documents"/> collection.
		/// </summary>
		/// <param name="document">The document.</param>
		public void Add(YamlDocument document)
		{
			documents.Add(document);
		}

		/// <summary>
		/// Loads the stream from the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		public void Load(TextReader input)
		{
			documents.Clear();

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

			foreach (var document in documents)
			{
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
		public void Accept(IYamlVisitor visitor)
		{
			visitor.Visit(this);
		}

		#region IEnumerable<YamlDocument> Members

		/// <summary />
		public IEnumerator<YamlDocument> GetEnumerator()
		{
			return documents.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
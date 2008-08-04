using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;

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
        public void Load(Stream input)
        {
            documents = new List<YamlDocument>();

            using (Parser parser = new Parser(input))
            {
				EventReader events = new EventReader(parser);
                events.Expect<StreamStartEvent>().Dispose();
                while (!events.Accept<StreamEndEvent>())
                {
                    YamlDocument document = new YamlDocument(events);
                    documents.Add(document);
                }
                events.Expect<StreamEndEvent>().Dispose();
            }
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
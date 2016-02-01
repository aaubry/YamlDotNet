//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Test.Core
{
    public class EmitterTestsHelper : EventsHelper
    {
        protected const string FooTag = "%TAG !foo! tag:example.org:foo";
        protected const string ExTag = "%TAG ! !";
        protected const string ExExTag = "%TAG !! tag:yaml.org,2002:";

        protected string EmittedTextFrom(IEnumerable<ParsingEvent> events)
        {
            return Emit(events, EmitterWithIndentCreator);
        }

        private Func<TextWriter, Emitter> EmitterWithIndentCreator
        {
            get { return writer => new Emitter(writer, 2, int.MaxValue, false); }
        }

        protected string Emit(IEnumerable<ParsingEvent> events, Func<TextWriter, Emitter> createEmitter)
        {
            var writer = new StringWriter();
            var emitter = createEmitter(writer);
            events.Run(emitter.Emit);
            return writer.ToString();
        }

        protected IEnumerable<ParsingEvent> StreamedDocumentWith(IEnumerable<ParsingEvent> events)
        {
            return StreamOf(DocumentWith(events.ToArray()));
        }

        protected IEnumerable<ParsingEvent> StreamOf(params IEnumerable<ParsingEvent>[] documents)
        {
            var allEvents = documents.SelectMany(x => x);
            return Wrap(allEvents, StreamStart, StreamEnd);
        }

        protected IEnumerable<ParsingEvent> DocumentWithVersion(params ParsingEvent[] events)
        {
            var version = new VersionDirective(new YamlDotNet.Core.Version(1, 1));
            return Wrap(events, DocumentStart(Explicit, version), DocumentEnd(Implicit));
        }

        protected IEnumerable<ParsingEvent> DocumentWithDefaultTags(params ParsingEvent[] events)
        {
            var tags = Constants.DefaultTagDirectives;
            return Wrap(events, DocumentStart(Explicit, null, tags), DocumentEnd(Implicit));
        }

        protected IEnumerable<ParsingEvent> DocumentWithCustomTags(params ParsingEvent[] events)
        {
            var parts = FooTag.Split(' ');
            var tags = new TagDirective(parts[1], parts[2]);
            return Wrap(events, DocumentStart(Explicit, null, tags), DocumentEnd(Implicit));
        }

        protected IEnumerable<ParsingEvent> DocumentWith(IEnumerable<ParsingEvent> events)
        {
            return DocumentWith(events.ToArray());
        }

        protected IEnumerable<ParsingEvent> DocumentWith(params ParsingEvent[] events)
        {
            return Wrap(events, DocumentStart(Implicit), DocumentEnd(Implicit));
        }

        protected IEnumerable<ParsingEvent> SequenceWith(params ParsingEvent[] events)
        {
            return Wrap(events, BlockSequenceStart.Explicit, SequenceEnd);
        }

        protected IEnumerable<ParsingEvent> MappingWith(params ParsingEvent[] events)
        {
            return Wrap(events, MappingStart, MappingEnd);
        }

        private IEnumerable<ParsingEvent> Wrap(IEnumerable<ParsingEvent> events, ParsingEvent start, ParsingEvent end)
        {
            yield return start;
            foreach (var @event in events)
            {
                yield return @event;
            }
            yield return end;
        }
    }
}
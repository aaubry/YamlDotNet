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
using System.Linq;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core
{
    /// <summary>
    /// Simple implementation of <see cref="IParser"/> that implements merging: http://yaml.org/type/merge.html
    /// </summary>
    public sealed class MergingParser : IParser
    {
        private readonly List<ParsingEvent> _allEvents = new List<ParsingEvent>();
        private readonly IParser _innerParser;
        private int _currentIndex = -1;

        public MergingParser(IParser innerParser)
        {
            _innerParser = innerParser;
        }

        public ParsingEvent Current { get; private set; }

        public bool MoveNext()
        {
            if (_currentIndex < 0)
            {
                while (_innerParser.MoveNext())
                {
                    _allEvents.Add(_innerParser.Current);
                }

                for (int i = _allEvents.Count - 2; i >= 0; --i)
                {
                    var merge = _allEvents[i] as Scalar;
                    if (merge != null && merge.Value == "<<")
                    {
                        var anchorAlias = _allEvents[i + 1] as AnchorAlias;
                        if (anchorAlias != null)
                        {
                            var mergedEvents = GetMappingEvents(anchorAlias.Value);
                            _allEvents.RemoveRange(i, 2);
                            _allEvents.InsertRange(i, mergedEvents);
                            continue;
                        }

                        var sequence = _allEvents[i + 1] as SequenceStart;
                        if (sequence != null)
                        {
                            var mergedEvents = new List<IEnumerable<ParsingEvent>>();
                            var sequenceEndFound = false;
                            for (var itemIndex = i + 2; itemIndex < _allEvents.Count; ++itemIndex)
                            {
                                anchorAlias = _allEvents[itemIndex] as AnchorAlias;
                                if (anchorAlias != null)
                                {
                                    mergedEvents.Add(GetMappingEvents(anchorAlias.Value));
                                    continue;
                                }

                                if (_allEvents[itemIndex] is SequenceEnd)
                                {
                                    _allEvents.RemoveRange(i, itemIndex - i + 1);
                                    _allEvents.InsertRange(i, mergedEvents.SelectMany(e => e));
                                    sequenceEndFound = true;
                                    break;
                                }
                            }

                            if (sequenceEndFound)
                            {
                                continue;
                            }
                        }

                        throw new SemanticErrorException(merge.Start, merge.End, "Unrecognized merge key pattern");
                    }
                }
            }

            var nextIndex = _currentIndex + 1;
            if (nextIndex < _allEvents.Count)
            {
                Current = _allEvents[nextIndex];
                _currentIndex = nextIndex;
                return true;
            }
            return false;
        }

        private IEnumerable<ParsingEvent> GetMappingEvents(string mappingAlias)
        {
            var cloner = new ParsingEventCloner();

            var nesting = 0;
            return _allEvents
                .SkipWhile(e =>
                {
                    var mappingStart = e as MappingStart;
                    return mappingStart == null || mappingStart.Anchor != mappingAlias;
                })
                .Skip(1)
                .TakeWhile(e => (nesting += e.NestingIncrease) >= 0)
                .Select(e => cloner.Clone(e))
                .ToList();
        }

        private class ParsingEventCloner : IParsingEventVisitor
        {
            private ParsingEvent clonedEvent;

            public ParsingEvent Clone(ParsingEvent e)
            {
                e.Accept(this);
                return clonedEvent;
            }

            void IParsingEventVisitor.Visit(AnchorAlias e)
            {
                clonedEvent = new AnchorAlias(e.Value, e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(StreamStart e)
            {
                throw new NotSupportedException();
            }

            void IParsingEventVisitor.Visit(StreamEnd e)
            {
                throw new NotSupportedException();
            }

            void IParsingEventVisitor.Visit(DocumentStart e)
            {
                throw new NotSupportedException();
            }

            void IParsingEventVisitor.Visit(DocumentEnd e)
            {
                throw new NotSupportedException();
            }

            void IParsingEventVisitor.Visit(Scalar e)
            {
                clonedEvent = new Scalar(null, e.Tag, e.Value, e.Style, e.IsPlainImplicit, e.IsQuotedImplicit, e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(SequenceStart e)
            {
                clonedEvent = new SequenceStart(null, e.Tag, e.IsImplicit, e.Style, e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(SequenceEnd e)
            {
                clonedEvent = new SequenceEnd(e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(MappingStart e)
            {
                clonedEvent = new MappingStart(null, e.Tag, e.IsImplicit, e.Style, e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(MappingEnd e)
            {
                clonedEvent = new MappingEnd(e.Start, e.End);
            }

            void IParsingEventVisitor.Visit(Comment e)
            {
                throw new NotSupportedException();
            }
        }
    }
}

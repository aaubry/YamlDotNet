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
using System.Collections;
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
        private readonly ParsingEventCollection _events;
        private readonly IParser _innerParser;
        private IEnumerator<LinkedListNode<ParsingEvent>> _iterator;
        private bool _merged;

        public MergingParser(IParser innerParser)
        {
            _events = new ParsingEventCollection();
            _merged = false;
            _iterator = _events.GetEnumerator();
            _innerParser = innerParser;
        }

        public ParsingEvent Current => _iterator.Current?.Value;

        public bool MoveNext()
        {
            if (!_merged)
            {
                Merge();
                _events.CleanMarked();
                _iterator = _events.GetEnumerator();
                _merged = true;
            }

            return _iterator.MoveNext();
        }

        private void Merge()
        {
            while (_innerParser.MoveNext())
            {
                _events.Add(_innerParser.Current);
            }

            foreach (var node in _events)
            {
                if (IsMergeToken(node))
                {
                    _events.MarkDeleted(node);
                    if (!HandleMerge(node.Next))
                        throw new SemanticErrorException(node.Value.Start, node.Value.End, "Unrecognized merge key pattern");
                }
            }
        }

        private bool HandleMerge(LinkedListNode<ParsingEvent> node)
        {
            if (node == null)
                return false;

            if (node.Value is AnchorAlias)
                return HandleAnchorAlias(node);

            if (node.Value is SequenceStart)
                return HandleSequence(node);

            return false;
        }

        private bool IsMergeToken(LinkedListNode<ParsingEvent> node)
        {
            return node.Value is Scalar merge && merge.Value == "<<";
        }

        private bool HandleAnchorAlias(LinkedListNode<ParsingEvent> node)
        {
            if (node == null || !(node.Value is AnchorAlias))
                return false;

            var anchorAlias = (AnchorAlias)node.Value;
            var mergedEvents = GetMappingEvents(anchorAlias.Value);

            _events.AddAfter(node, mergedEvents);
            _events.MarkDeleted(node);

            return true;
        }

        private bool HandleSequence(LinkedListNode<ParsingEvent> node)
        {
            if (node == null || !(node.Value is SequenceStart))
                return false;

            _events.MarkDeleted(node);

            while (node != null)
            {
                if (node.Value is SequenceEnd)
                {
                    _events.MarkDeleted(node);
                    return true;
                }

                var next = node.Next;
                HandleMerge(next);
                node = next;
            }

            return true;
        }

        private IEnumerable<ParsingEvent> GetMappingEvents(string anchor)
        {
            var cloner = new ParsingEventCloner();
            var nesting = 0;

            return _events.FromAnchor(anchor)
                .Select(e => e.Value)
                .TakeWhile(e => (nesting += e.NestingIncrease) >= 0)
                .Select(e => cloner.Clone(e));
        }

        private sealed class ParsingEventCollection : IEnumerable<LinkedListNode<ParsingEvent>>
        {
            private readonly LinkedList<ParsingEvent> _events;
            private readonly HashSet<LinkedListNode<ParsingEvent>> _deleted;
            private readonly Dictionary<string, LinkedListNode<ParsingEvent>> _references;

            public ParsingEventCollection()
            {
                _events = new LinkedList<ParsingEvent>();
                _deleted = new HashSet<LinkedListNode<ParsingEvent>>();
                _references = new Dictionary<string, LinkedListNode<ParsingEvent>>();
            }

            public void AddAfter(LinkedListNode<ParsingEvent> node, IEnumerable<ParsingEvent> items)
            {
                foreach (var item in items)
                {
                    node = _events.AddAfter(node, item);
                }
            }

            public void Add(ParsingEvent item)
            {
                var node = _events.AddLast(item);
                AddReference(item, node);
            }

            public void MarkDeleted(LinkedListNode<ParsingEvent> node)
            {
                _deleted.Add(node);
            }

            public void CleanMarked()
            {
                foreach (var node in _deleted)
                {
                    _events.Remove(node);
                }
            }

            public IEnumerable<LinkedListNode<ParsingEvent>> FromAnchor(string anchor)
            {
                var node = _references[anchor].Next;
                var iterator = GetEnumerator(node);

                while (iterator.MoveNext())
                    yield return iterator.Current;
            }

            public IEnumerator<LinkedListNode<ParsingEvent>> GetEnumerator()
            {
                return GetEnumerator(_events.First);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private IEnumerator<LinkedListNode<ParsingEvent>> GetEnumerator(LinkedListNode<ParsingEvent> node)
            {
                for (; node != null; node = node.Next)
                    yield return node;
            }

            private void AddReference(ParsingEvent item, LinkedListNode<ParsingEvent> node)
            {
                if (!(item is MappingStart))
                    return;

                var mappingStart = (MappingStart)item;
                var anchor = mappingStart.Anchor;

                if (!string.IsNullOrEmpty(anchor))
                    _references[anchor] = node;
            }
        }

        private sealed class ParsingEventCloner : IParsingEventVisitor
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

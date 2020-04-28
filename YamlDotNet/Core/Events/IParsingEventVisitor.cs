//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2014 Leon Mlakar
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

namespace YamlDotNet.Core.Events
{
    /// <summary>
    /// Callback interface for external event Visitor without a return value.
    /// </summary>
    public interface IParsingEventVisitor
    {
        void Visit(AnchorAlias anchorAlias);
        void Visit(StreamStart streamStart);
        void Visit(StreamEnd streamEnd);
        void Visit(DocumentStart documentStart);
        void Visit(DocumentEnd documentEnd);
        void Visit(Scalar scalar);
        void Visit(SequenceStart sequenceStart);
        void Visit(SequenceEnd sequenceEnd);
        void Visit(MappingStart mappingStart);
        void Visit(MappingEnd mappingEnd);
        void Visit(Comment comment);
    }

    /// <summary>
    /// Callback interface for external event Visitor that returns a value.
    /// </summary>
    public interface IParsingEventVisitor<T>
    {
        T Visit(AnchorAlias anchorAlias);
        T Visit(StreamStart streamStart);
        T Visit(StreamEnd streamEnd);
        T Visit(DocumentStart documentStart);
        T Visit(DocumentEnd documentEnd);
        T Visit(Scalar scalar);
        T Visit(SequenceStart sequenceStart);
        T Visit(SequenceEnd sequenceEnd);
        T Visit(MappingStart mappingStart);
        T Visit(MappingEnd mappingEnd);
        T Visit(Comment comment);
    }
}
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

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Abstract implementation of <see cref="IYamlVisitor"/> that knows how to walk a complete YAML object model.
    /// </summary>
    public abstract class YamlVisitorBase : IYamlVisitor {
        /// <summary>
        /// Called when this object is visiting a <see cref="YamlStream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="YamlStream"/> that is being visited.
        /// </param>
        protected virtual void Visit(YamlStream stream)
        {
            VisitChildren(stream);
        }

        /// <summary>
        /// Called after this object finishes visiting a <see cref="YamlStream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="YamlStream"/> that has been visited.
        /// </param>
        protected virtual void Visited(YamlStream stream)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when this object is visiting a <see cref="YamlDocument"/>.
        /// </summary>
        /// <param name="document">
        /// The <see cref="YamlDocument"/> that is being visited.
        /// </param>
        protected virtual void Visit(YamlDocument document)
        {
            VisitChildren(document);
        }

        /// <summary>
        /// Called after this object finishes visiting a <see cref="YamlDocument"/>.
        /// </summary>
        /// <param name="document">
        /// The <see cref="YamlDocument"/> that has been visited.
        /// </param>
        protected virtual void Visited(YamlDocument document)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when this object is visiting a <see cref="YamlScalarNode"/>.
        /// </summary>
        /// <param name="scalar">
        /// The <see cref="YamlScalarNode"/> that is being visited.
        /// </param>
        protected virtual void Visit(YamlScalarNode scalar)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called after this object finishes visiting a <see cref="YamlScalarNode"/>.
        /// </summary>
        /// <param name="scalar">
        /// The <see cref="YamlScalarNode"/> that has been visited.
        /// </param>
        protected virtual void Visited(YamlScalarNode scalar)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when this object is visiting a <see cref="YamlSequenceNode"/>.
        /// </summary>
        /// <param name="sequence">
        /// The <see cref="YamlSequenceNode"/> that is being visited.
        /// </param>
        protected virtual void Visit(YamlSequenceNode sequence)
        {
            VisitChildren(sequence);
        }

        /// <summary>
        /// Called after this object finishes visiting a <see cref="YamlSequenceNode"/>.
        /// </summary>
        /// <param name="sequence">
        /// The <see cref="YamlSequenceNode"/> that has been visited.
        /// </param>
        protected virtual void Visited(YamlSequenceNode sequence)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when this object is visiting a <see cref="YamlMappingNode"/>.
        /// </summary>
        /// <param name="mapping">
        /// The <see cref="YamlMappingNode"/> that is being visited.
        /// </param>
        protected virtual void Visit(YamlMappingNode mapping)
        {
            VisitChildren(mapping);
        }

        /// <summary>
        /// Called after this object finishes visiting a <see cref="YamlMappingNode"/>.
        /// </summary>
        /// <param name="mapping">
        /// The <see cref="YamlMappingNode"/> that has been visited.
        /// </param>
        protected virtual void Visited(YamlMappingNode mapping)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when this object is visiting a key-value pair.
        /// </summary>
        /// <param name="key">The left (key) <see cref="YamlNode"/> that is being visited.</param>
        /// <param name="value">The right (value) <see cref="YamlNode"/> that is being visited.</param>
        protected virtual void VisitPair(YamlNode key, YamlNode value)
        {
            key.Accept(this);
            value.Accept(this);
        }

        /// <summary>
        /// Visits every child of a <see cref="YamlStream"/>.
        /// </summary>
        /// <param name="stream">
        /// The <see cref="YamlStream"/> that is being visited.
        /// </param>
        protected virtual void VisitChildren(YamlStream stream)
        {
            foreach (var document in stream.Documents)
            {
                document.Accept(this);
            }
        }

        /// <summary>
        /// Visits every child of a <see cref="YamlDocument"/>.
        /// </summary>
        /// <param name="document">
        /// The <see cref="YamlDocument"/> that is being visited.
        /// </param>
        protected virtual void VisitChildren(YamlDocument document)
        {
            if (document.RootNode != null)
            {
                document.RootNode.Accept(this);
            }
        }

        /// <summary>
        /// Visits every child of a <see cref="YamlSequenceNode"/>.
        /// </summary>
        /// <param name="sequence">
        /// The <see cref="YamlSequenceNode"/> that is being visited.
        /// </param>
        protected virtual void VisitChildren(YamlSequenceNode sequence)
        {
            foreach (var node in sequence.Children)
            {
                node.Accept(this);
            }
        }

        /// <summary>
        /// Visits every child of a <see cref="YamlMappingNode"/>.
        /// </summary>
        /// <param name="mapping">
        /// The <see cref="YamlMappingNode"/> that is being visited.
        /// </param>
        protected virtual void VisitChildren(YamlMappingNode mapping)
        {
            foreach (var pair in mapping.Children)
            {
                VisitPair(pair.Key, pair.Value);
            }
        }

        void IYamlVisitor.Visit(YamlStream stream)
        {
            Visit(stream);
            Visited(stream);
        }

        void IYamlVisitor.Visit(YamlDocument document)
        {
            Visit(document);
            Visited(document);
        }

        void IYamlVisitor.Visit(YamlScalarNode scalar)
        {
            Visit(scalar);
            Visited(scalar);
        }

        void IYamlVisitor.Visit(YamlSequenceNode sequence)
        {
            Visit(sequence);
            Visited(sequence);
        }

        void IYamlVisitor.Visit(YamlMappingNode mapping)
        {
            Visit(mapping);
            Visited(mapping);
        }
    }
}

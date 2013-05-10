//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

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
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel.Serialization.NamingConventions;

namespace YamlDotNet.RepresentationModel.Serialization
{
	/// <summary>
	/// Options that control the serialization process.
	/// </summary>
	[Flags]
	public enum SerializationOptions
	{
		/// <summary>
		/// Serializes using the default options
		/// </summary>
		None = 0,

		/// <summary>
		/// Ensures that it will be possible to deserialize the serialized objects.
		/// </summary>
		Roundtrip = 1,

		/// <summary>
		/// If this flag is specified, if the same object appears more than once in the
		/// serialization graph, it will be serialized each time instead of just once.
		/// </summary>
		/// <remarks>
		/// If the serialization graph contains circular references and this flag is set,
		/// a <see cref="StackOverflowException" /> will be thrown.
		/// If this flag is not set, there is a performance penalty because the entire
		/// object graph must be walked twice.
		/// </remarks>
		DisableAliases = 2,

		/// <summary>
		/// Forces every value to be serialized, even if it is the default value for that type.
		/// </summary>
		EmitDefaults = 4,

		/// <summary>
		/// Ensures that the result of the serialization is valid JSON.
		/// </summary>
		JsonCompatible = 8,
	}

	/// <summary>
	/// Writes objects to YAML.
	/// </summary>
	public sealed class Serializer
	{
		internal IList<IYamlTypeConverter> Converters { get; private set; }

		public Serializer()
		{
			Converters = new List<IYamlTypeConverter>();
		}

		/// <summary>
		/// Registers a type converter to be used to serialize and deserialize specific types.
		/// </summary>
		public void RegisterTypeConverter(IYamlTypeConverter converter)
		{
			Converters.Add(converter);
		}

		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
		/// <param name="graph">The object to serialize.</param>
		/// <param name="options">Options that control how the serialization is to be performed.</param>
		/// <param name="namingConvention">Naming strategy to use for serialized property names</param>
		public void Serialize(TextWriter writer, object graph, SerializationOptions options = SerializationOptions.None, INamingConvention namingConvention = null)
		{
			Serialize(new Emitter(writer), graph, options, namingConvention);
		}

		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
		/// <param name="graph">The object to serialize.</param>
		/// <param name="type">The static type of the object to serialize.</param>
		/// <param name="options">Options that control how the serialization is to be performed.</param>
		/// <param name="namingConvention">Naming strategy to use for serialized property names</param>
		public void Serialize(TextWriter writer, object graph, Type type, SerializationOptions options = SerializationOptions.None, INamingConvention namingConvention = null)
		{
			Serialize(new Emitter(writer), graph, type, options, namingConvention);
		}

		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="emitter">The <see cref="Emitter" /> where to serialize the object.</param>
		/// <param name="graph">The object to serialize.</param>
		/// <param name="options">Options that control how the serialization is to be performed.</param>
		/// <param name="namingConvention">Naming strategy to use for serialized property names</param>
		public void Serialize(Emitter emitter, object graph, SerializationOptions options = SerializationOptions.None, INamingConvention namingConvention = null)
		{
			Serialize(emitter, graph, graph != null ? graph.GetType() : typeof(object), options, namingConvention);
		}

		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="emitter">The <see cref="Emitter" /> where to serialize the object.</param>
		/// <param name="graph">The object to serialize.</param>
		/// <param name="type">The static type of the object to serialize.</param>
		/// <param name="options">Options that control how the serialization is to be performed.</param>
		/// <param name="namingConvention">Naming strategy to use for serialized property names</param>
		public void Serialize(Emitter emitter, object graph, Type type, SerializationOptions options = SerializationOptions.None, INamingConvention namingConvention = null)
		{
			if (emitter == null)
			{
				throw new ArgumentNullException("emitter");
			}

			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			var traversalStrategy = CreateTraversalStrategy(options, namingConvention ?? new NullNamingConvention());
			var eventEmitter = CreateEventEmitter(emitter, options);
			var emittingVisitor = CreateEmittingVisitor(emitter, options, traversalStrategy, eventEmitter, graph, type);
			EmitDocument(emitter, traversalStrategy, emittingVisitor, graph, type);
		}

		private void EmitDocument(Emitter emitter, IObjectGraphTraversalStrategy traversalStrategy, IObjectGraphVisitor emittingVisitor, object graph, Type type)
		{
			emitter.Emit(new StreamStart());
			emitter.Emit(new DocumentStart());

			traversalStrategy.Traverse(graph, type, emittingVisitor);

			emitter.Emit(new DocumentEnd(true));
			emitter.Emit(new StreamEnd());
		}

		private IObjectGraphVisitor CreateEmittingVisitor(Emitter emitter, SerializationOptions options, IObjectGraphTraversalStrategy traversalStrategy, IEventEmitter eventEmitter, object graph, Type type)
		{
			IObjectGraphVisitor emittingVisitor = new EmittingObjectGraphVisitor(eventEmitter);

			emittingVisitor = new CustomSerializationObjectGraphVisitor(emitter, emittingVisitor, Converters);

			if ((options & SerializationOptions.DisableAliases) == 0)
			{
				var anchorAssigner = new AnchorAssigner();
				traversalStrategy.Traverse(graph, type, anchorAssigner);

				emittingVisitor = new AnchorAssigningObjectGraphVisitor(emittingVisitor, eventEmitter, anchorAssigner);
			}

			if ((options & SerializationOptions.EmitDefaults) == 0)
			{
				emittingVisitor = new DefaultExclusiveObjectGraphVisitor(emittingVisitor);
			}

			return emittingVisitor;
		}

		private IEventEmitter CreateEventEmitter(Emitter emitter, SerializationOptions options)
		{
			var writer = new WriterEventEmitter(emitter);

			if ((options & SerializationOptions.JsonCompatible) != 0)
			{
				return new JsonEventEmitter(writer);
			}
			else
			{
				return new TypeAssigningEventEmitter(writer);
			}
		}

		private IObjectGraphTraversalStrategy CreateTraversalStrategy(SerializationOptions options, INamingConvention namingConvention)
		{
			ITypeDescriptor typeDescriptor;
			if ((options & SerializationOptions.Roundtrip) != 0)
			{
				typeDescriptor = new ReadableAndWritablePropertiesTypeDescriptor();
			}
			else
			{
				typeDescriptor = new ReadablePropertiesTypeDescriptor();
			}

			typeDescriptor = new NamingConventionTypeDescriptor(typeDescriptor, namingConvention);
			typeDescriptor = new YamlAttributesTypeDescriptor(typeDescriptor);

			if ((options & SerializationOptions.Roundtrip) != 0)
			{
				return new RoundtripObjectGraphTraversalStrategy(this, typeDescriptor, 50);
			}
			else
			{
				return new FullObjectGraphTraversalStrategy(this, typeDescriptor, 50);
			}
		}
	}
}
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents an YAML document.
	/// </summary>
	public class YamlDocument
	{
		private YamlNode rootNode;

		/// <summary>
		/// Gets or sets the root node.
		/// </summary>
		/// <value>The root node.</value>
		public YamlNode RootNode
		{
			get
			{
				return rootNode;
			}
			set
			{
				rootNode = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlDocument"/> class.
		/// </summary>
		public YamlDocument()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="YamlDocument"/> class.
		/// </summary>
		/// <param name="events">The events.</param>
		internal YamlDocument(EventReader events)
		{
			DocumentLoadingState state = new DocumentLoadingState();

			events.Expect<DocumentStart>();

			while (!events.Accept<DocumentEnd>())
			{
				Debug.Assert(rootNode == null);
				rootNode = YamlNode.ParseNode(events, state);

				if (rootNode is YamlAliasNode)
				{
					throw new YamlException();
				}
			}

			state.ResolveAliases();

			events.Expect<DocumentEnd>();
		}

		/// <summary>
		/// Visitor that assigns anchors to nodes that are referenced more than once but have no anchor.
		/// </summary>
		private class AnchorAssigningVisitor : YamlVisitor
		{
			private readonly Dictionary<string, bool> existingAnchors = new Dictionary<string, bool>();
			private readonly Dictionary<YamlNode, bool> visitedNodes = new Dictionary<YamlNode, bool>();

			public void AssignAnchors(YamlDocument document)
			{
				existingAnchors.Clear();
				visitedNodes.Clear();

				document.Accept(this);

				Random random = new Random();
				foreach (var visitedNode in visitedNodes)
				{
					if (visitedNode.Value)
					{
						string anchor;
						do
						{
							anchor = random.Next().ToString(CultureInfo.InvariantCulture);
						} while (existingAnchors.ContainsKey(anchor));
						existingAnchors.Add(anchor, false);

						visitedNode.Key.Anchor = anchor;
					}
				}
			}

			private void VisitNode(YamlNode node)
			{
				if (string.IsNullOrEmpty(node.Anchor))
				{
					bool isDuplicate;
					if (visitedNodes.TryGetValue(node, out isDuplicate))
					{
						if (!isDuplicate)
						{
							visitedNodes[node] = true;
						}
					}
					else
					{
						visitedNodes.Add(node, false);
					}
				}
				else
				{
					existingAnchors.Add(node.Anchor, false);
				}
			}

			protected override void Visit(YamlScalarNode scalar)
			{
				VisitNode(scalar);
			}

			protected override void Visit(YamlMappingNode mapping)
			{
				VisitNode(mapping);
			}

			protected override void Visit(YamlSequenceNode sequence)
			{
				VisitNode(sequence);
			}
		}

		private void AssignAnchors() {
			AnchorAssigningVisitor visitor = new AnchorAssigningVisitor();
			visitor.AssignAnchors(this);
		}
		
		internal void Save(Emitter emitter) {
			AssignAnchors();
			
			emitter.Emit(new DocumentStart());
			rootNode.Save(emitter);
			emitter.Emit(new DocumentEnd(false));
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
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.RepresentationModel
{
	/// <summary>
	/// Represents an YAML document.
	/// </summary>
	public class YamlDocument : IEnumerable<YamlNode>
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
		
		private void AssignAnchors() {
			Dictionary<string, bool> existingAnchors = new Dictionary<string, bool>();
			Dictionary<YamlNode, bool> visitedNodes = new Dictionary<YamlNode, bool>();
			foreach (var node in this) {
				if(string.IsNullOrEmpty(node.Anchor)) {
					bool isDuplicate;
					if(visitedNodes.TryGetValue(node, out isDuplicate)) {
						if(!isDuplicate) {
							visitedNodes[node] = true;
						}
					} else {
						visitedNodes.Add(node, false);
					}
				} else {
					existingAnchors.Add(node.Anchor, false);
				}
			}
			
			Random random = new Random();
			foreach (var visitedNode in visitedNodes) {
				if(visitedNode.Value) {
					string anchor;
					do {
						anchor = random.Next().ToString();
					} while(existingAnchors.ContainsKey(anchor));
					existingAnchors.Add(anchor, false);
					
					visitedNode.Key.Anchor = anchor;
				}
			}
		}
		
		internal void Save(Emitter emitter) {
			AssignAnchors();
			
			emitter.Emit(new DocumentStart());
			rootNode.Save(emitter);
			emitter.Emit(new DocumentEnd(false));
		}
		
		IEnumerator<YamlNode> IEnumerable<YamlNode>.GetEnumerator() {
			yield return rootNode;
			foreach (var node in rootNode) {
				yield return node;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable<YamlNode>)this).GetEnumerator();
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
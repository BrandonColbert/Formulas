using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Formulas {
	/// <summary>
	/// A single node in a formula syntax tree
	/// 
	/// Iteration traverses the tree in post-order fashion (left subtree, right subtree, self node)
	/// </summary>
	public abstract class Node : IEnumerable<Node> {
		/// <summary>Left child</summary>
		public Node Left { get; private set; }

		/// <summary>Right child</summary>
		public Node Right { get; private set; }

		/// <summary>Parent node</summary>
		public Node Parent { get; private set; }

		/// <summary>Top most grand parent</summary>
		public Node Top {
			get {
				var top = this;

				while(top.Parent)
					top = top.Parent;

				return top;
			}
		}

		/// <summary>Set the given node as right child</summary>
		/// <param name="node">Node to become right child</param>
		public void Append(Node node) {
			if(Right)
				throw new ParseException($"Append failed, right child exists in '{this}'");

			Right = node;
			node.Parent = this;
		}

		/// <summary>Steal the position of the given node</summary>
		/// <param name="node">Node to become left child</param>
		public void Usurp(Node node) {
			Replace(node);

			//Make the given node this one's child
			Left = node;
			node.Parent = this;
		}

		/// <summary>Replaces an existing node (including its children) entirely</summary>
		/// <param name="node">Node to replace</param>
		public void Replace(Node node) {
			Parent = node.Parent;

			if(!Parent)
				return;

			//Assume the given node's status under its parent
			if(ReferenceEquals(Parent.Left, node))
				Parent.Left = this;
			else if(ReferenceEquals(Parent.Right, node))
				Parent.Right = this;
			else
				throw new ParseException($"Replace failed, '{node}' is not a child of '{node.Parent}'");
		}

		/// <returns>Neatly formatted multiline string representing the tree for this node</returns>
		public virtual string DisplayString() {
			var builder = new StringBuilder();

			if(Left || Right) {
				if(Left)
					builder.Append($"\n{string.Join("\n", Left.DisplayString().Split('\n').Select(v => $"\t{v}"))}");

				if(Right) {
					if(!Left)
						builder.AppendLine();

					builder.Append($"\n{string.Join("\n", Right.DisplayString().Split('\n').Select(v => $"\t{v}"))}");
				}
			}

			return builder.ToString();
		}

		/// <returns>Node enumerator for left, right, self tree traversal</returns>
		public IEnumerator<Node> GetEnumerator() {
			//Left subtree
			if(Left)
				foreach(var node in Left)
					yield return node;

			//Right subtree
			if(Right)
				foreach(var node in Right)
					yield return node;

			//Self node
			yield return this;
		}

		/// <summary>Attempts to reduce this node</summary>
		/// <param name="node">Reduced node</param>
		/// <returns>Whether reduction was possible</returns>
		public virtual bool Reduce(out Node node) {
			node = null;
			return false;
		}

		/// <param name="spec">Formula specification</param>
		/// <param name="inputs">Map of variables to their values</param>
		/// <param name="result">Calculated value result of the node</param>
		/// <returns>Whether a calculation was done</returns>
		public virtual bool Calculate(Specification spec, Dictionary<string, object> inputs, out object result) {
			result = null;
			return false;
		}

		/// <summary>Converts this node into an expression</summary>
		/// <param name="spec">Formula specification</param>
		/// <param name="args">Formula arguments</param>
		/// <returns>Expression that this node represents</returns>
		public virtual Expression Compile(Specification spec, ParameterExpression args) => throw new CompileException($"Compilation is not supported on '{ToString()}'");

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>Whether the node exists</summary>
		/// <param name="node">Node to check</param>
		public static implicit operator bool(Node node) => node != null;
	}
}
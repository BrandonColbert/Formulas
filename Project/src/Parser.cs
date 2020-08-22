using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Content = System.Collections.Generic.LinkedList<char>;

namespace Formulas {
	/// <summary>Parses sections of a formula descriptions</summary>
	static class Parser {
		/// <summary>Peeks at the next operator without modifying the content</summary>
		/// <param name="content">Text to search through</param>
		/// <returns>Next operator seen in text</returns>
		public static char PeekOperator(Content content) {
			var next = content.First;

			//Iterate characters until operator is found
			while(!Ops.Is(next.Value)) {
				if(next.Next == null)
					return '\0';

				next = next.Next;
			}

			//If none left, return null terminal signifying no operator
			return next.Value;
		}

		/// <param name="content">Text to search through</param>
		/// <returns>Next variable extracted from text text</returns>
		public static string NextVariable(Content content) {
			if(content.Count == 0)
				throw new ParseException("Text cannot come next in an empty string");

			while(char.IsWhiteSpace(content.First.Value))
				content.RemoveFirst();

			if(!char.IsLetter(content.First.Value))
				throw new ParseException($"A variable does not come next in '{string.Concat(content)}'");

			var value = new StringBuilder();

			//Add variable name
			value.Append(content.First.Value);
			content.RemoveFirst();

			//If no subscript delimiter found, variable has no subscript
			if(content.Count == 0 || content.First.Value != '_')
				return value.ToString();

			//Add subscript symbol
			value.Append(content.First.Value);
			content.RemoveFirst();

			//Add next word as subscript
			value.Append(NextName(content));

			return value.ToString();
		}

		/// <param name="content">Text to search through</param>
		/// <returns>Next property or function name extracted from text</returns>
		public static string NextName(Content content) {
			var value = new StringBuilder();

			//Append [0-9a-zA-Z_] and ignore whitespace until next character fails to match
			while(content.Count > 0) {
				switch(content.First.Value) {
					case '_':
						break;
					default:
						if(char.IsLetterOrDigit(content.First.Value))
							break;
						if(char.IsWhiteSpace(content.First.Value)) {
							content.RemoveFirst();
							continue;
						}

						return value.ToString();
				}

				value.Append(content.First.Value);
				content.RemoveFirst();
			}

			return value.ToString();
		}

		/// <param name="content">Text to search through</param>
		/// <returns>Next number extracted from text as a string</returns>
		public static string NextNumberText(Content content) {
			var value = new StringBuilder();

			//Append [0-9\.] and ignore whitespace until next character fails to match
			while(content.Count > 0) {
				switch(content.First.Value) {
					case '.':
						break;
					default:
						if(char.IsDigit(content.First.Value))
							break;
						if(char.IsWhiteSpace(content.First.Value)) {
							content.RemoveFirst();
							continue;
						}

						return value.ToString();
				}

				value.Append(content.First.Value);
				content.RemoveFirst();
			}

			return value.ToString();
		}

		/// <param name="content">Text to search through</param>
		/// <returns>Next number extracted from text</returns>
		public static Number NextNumber(Content content) {
			var value = NextNumberText(content);

			//Parse numeric result if possible
			if(Number.TryParse(value, out var number))
				return number;

			throw new ParseException($"Unable to convert '{value}' from '{value}{string.Concat(content)}' to a number");
		}

		/// <param name="content">Text to search through</param>
		/// <returns>Next typename extracted from text</returns>
		public static string NextTypename(Content content) {
			var value = new StringBuilder();
			var depth = 0;

			while(content.Count > 0) {
				switch(content.First.Value) {
					case '<':
						depth++;
						break;
					case '>':
						depth--;
						break;
					case ',':
						if(depth == 0)
							return value.ToString();

						break;
					case '_':
						break;
					default:
						if(char.IsLetterOrDigit(content.First.Value))
							break;
						if(char.IsWhiteSpace(content.First.Value)) {
							content.RemoveFirst();
							continue;
						}

						return value.ToString();
				}

				value.Append(content.First.Value);
				content.RemoveFirst();
			}

			return value.ToString();
		}

		/// <summary>Parses the next possible node from the right hand side of a formula description</summary>
		/// <param name="content">Right hand side of a formula description</param>
		/// <returns>Next sequential node extracted from text</returns>
		public static Node NextNode(Content content) {
			var next = content.First.Value;
			content.RemoveFirst();

			//Ignore leading whitespace
			while(char.IsWhiteSpace(next)) {
				next = content.First.Value;
				content.RemoveFirst();
			}

			switch(next) {
				case Ops.Add: //Account for operators
					return new OpNode(Operation.Add);
				case Ops.Sub:
					return new OpNode(Operation.Subtract);
				case Ops.Mul:
					return new OpNode(Operation.Multiply);
				case Ops.Div:
					return new OpNode(Operation.Divide);
				case Ops.Mod:
					return new OpNode(Operation.Modulo);
				case Ops.Pow:
					return new OpNode(Operation.Power);
				case Ops.Idx:
					return new OpNode(Operation.Index);
				case Ops.Prp:
					return new OpNode(Operation.Property);
				case Ops.Gpo: {
					var depth = 0;
					var subtext = new StringBuilder();

					//Capture all content within same-level parenthesis
					while(content.Count > 0) {
						next = content.First.Value;
						content.RemoveFirst();

						if(next == Ops.Gpo)
							depth++;
						else if(next == Ops.Gpc) {
							if(depth == 0)
								break;
							else
								depth--;
						}

						subtext.Append(next);
					}

					//Created grouped node from content
					return new GroupNode(subtext.ToString());
				}
				case Ops.Mag: {
					var depth = 0;
					var subtext = new StringBuilder();

					//Capture all content within same-level vertical bars
					while(content.Count > 0) {
						next = content.First.Value;
						content.RemoveFirst();

						if(next == Ops.Gpo)
							depth++;
						else if(next == Ops.Gpc)
							depth--;
						else if(depth == 0 && next == Ops.Mag)
							break;

						subtext.Append(next);
					}

					//Create grouped magnitude node from content
					return new MagnitudeNode(subtext.ToString());
				}
				default: //Acount for variables, numbers, properties, and functions
					//Undo dequeue since letter will be used in text/number node
					content.AddFirst(next);

					if(char.IsLetter(next)) //Beginning with a letter indicates variable, property, or function
						switch(PeekOperator(content)) {
							case Ops.Mag:
							case Ops.Gpo:
								return new NameNode(NextName(content));
							default:
								return new VariableNode(NextVariable(content));
						}
					else if(char.IsDigit(next)) //Beginning with a digit indicates a number
						return new NumberNode(NextNumber(content));
					else //Unrecognized symbol otherwise
						throw new ParseException($"Unexpected symbol '{next}' in '{string.Concat(content)}'");
			}
		}

		/// <summary>Inserts a node into the tree based on the node previously inserted</summary>
		/// <param name="lastNode">Previously inserted node</param>
		/// <param name="currentNode">Node to insert</param>
		/// <returns>Last node inserted during the extension process</returns>
		public static Node ExtendTree(Node lastNode, Node currentNode) {
			switch(currentNode) {
				case OpNode currentOpNode:
					switch(lastNode) {
						case OpNode lastOpNode:
							switch(currentOpNode.value) {
								case Operation.Subtract:
									return ExtendTree(lastOpNode, new OpNode(Operation.Negate));
								default:
									if(currentOpNode.value.Precedence() <= lastOpNode.value.Precedence())
										currentNode.Usurp(lastNode);
									else
										lastNode.Append(currentNode);
									break;
							}
							break;
						default:
							var target = lastNode;

							while(target.Parent is OpNode next && currentOpNode.value.Precedence() <= next.value.Precedence())
								target = next;

							currentNode.Usurp(target);
							break;
					}
					break;
				case GroupNode currentGroupedNode: {
					if(!(lastNode is TextNode lastTextNode))
						goto default;

					return ExtendTree(ExtendTree(lastNode, new OpNode(Operation.Transform)), currentNode);
				}
				case TextNode currentTextNode: {
					switch(currentNode) {
						case VariableNode currentVariableNode:
							switch(lastNode) {
								case OpNode lastOpNode:
									switch(lastOpNode.value) {
										case Operation.Index:
										case Operation.Property:
											currentTextNode = new NameNode(currentVariableNode.value);
											currentNode = currentTextNode;
											break;
									}
									break;
							}
							break;
					}

					switch(lastNode) {
						case NameNode lastNameNode: {
							var aggregate = new NameNode($"{lastNameNode.value}{currentTextNode.value}");
							aggregate.Replace(lastNameNode);
							return aggregate;
						}
					}

					goto default;
				}
				default:
					switch(lastNode) {
						case OpNode lastOpNode:
							if(lastNode.Left)
								lastOpNode.Append(currentNode);
							else
								switch(lastOpNode.value) {
									case Operation.Negate:
										lastOpNode.Append(currentNode);
										break;
									case Operation.Subtract:
										var negateNode = new OpNode(Operation.Negate);
										negateNode.Replace(lastOpNode);
										negateNode.Append(currentNode);
										break;
									default:
										throw new ParseException($"'{lastOpNode.value}' is not a unary operation");
								}
							break;
						default:
							return ExtendTree(ExtendTree(lastNode, new OpNode(Operation.Multiply)), currentNode);
					}
					break;
			}

			return currentNode;
		}

		/// <summary>Creates an syntax tree from the part of a formula description</summary>
		/// <param name="content">Right hand side of a description</param>
		/// <returns>The created syntax tree</returns>
		public static Node CreateTree(Content content) {
			//Get the first node
			var current = Parser.NextNode(content);

			if(!current)
				throw new ParseException($"Empty formula '{string.Concat(content)}'");

			//Extend the tree with newly parsed nodes until none left
			while(content.Count > 0) {
				var next = Parser.NextNode(content);

				if(!next)
					break;

				current = ExtendTree(current, next);
			}

			//Reduce the tree
			foreach(var node in current.Top) {
				if(!node.Reduce(out var result))
					continue;

				result.Replace(node);
				current = result;
			}

			//Return top of tree
			return current.Top;
		}

		/// <summary>Approximates a specification based on a syntax tree</summary>
		/// <param name="tree">The syntax tree</param>
		/// <returns>The created specification</returns>
		public static Specification CreateSpecification(Node tree) {
			if(!tree.Left && !tree.Right) {
				//A tree consisting of only a text node must be a variable
				if(tree is TextNode textNode)
					return new Specification("", new List<string>(){textNode.value}, new Dictionary<string, Type>(){[textNode.value] = typeof(object)});

				return new Specification("");
			}

			var variables = new List<string>();

			foreach(var node in tree) {
				//A node will only be considered when it has text and an operator parent
				if(!(node is TextNode textNode && node.Parent is OpNode parentOpNode))
					continue;

				switch(parentOpNode.value) {
					case Operation.Add:
					case Operation.Subtract:
					case Operation.Multiply:
					case Operation.Divide:
					case Operation.Modulo:
					case Operation.Negate:
					case Operation.Power: //Any of the above imply its a variable
						if(!variables.Contains(textNode.value))
							variables.Add(textNode.value);
						break;
					case Operation.Index:
					case Operation.Property: //If it is the left child, its a variable
						if(ReferenceEquals(parentOpNode.Left, textNode) && !variables.Contains(textNode.value))
							variables.Add(textNode.value);
						break;
					case Operation.Transform: //If it is the right child, its a variable
						if(ReferenceEquals(parentOpNode.Right, textNode) && !variables.Contains(textNode.value))
							variables.Add(textNode.value);
						break;
				}
			}

			//Since types weren't specified, they can all be assumed as object
			return new Specification("", variables, variables.ToDictionary(v => v, v => typeof(object)));
		}

		/// <summary>Creates a specification for the formula</summary>
		/// <param name="content">Left hand side of a description</param>
		/// <returns>The created specification</returns>
		public static Specification CreateSpecification(Content content) {
			if(content.Count < 0)
				throw new ParseException("Specification is empty");

			//First part is formula name
			var name = NextName(content);

			//Ignore whitespace
			while(content.Count > 0 && char.IsWhiteSpace(content.First.Value))
				content.RemoveFirst();

			//If done, only a name was specified
			if(content.Count == 0)
				return new Specification(name);

			//Start acquiring variable names/types once open parentheses is encountered
			if(content.First.Value == Ops.Gpo)
				content.RemoveFirst();
			else
				throw new ParseException($"Expected '{Ops.Gpo}' to start declaring variables in '{string.Concat(content)}'");

			var variables = new List<string>();
			var types = new Dictionary<string, Type>();

			//Capture the variables and their types until close parenthesis is encountered
			while(content.Count > 0 && content.First.Value != Ops.Gpc) {
				var variable = NextVariable(content);
				variables.Add(variable);

				if(content.Count > 0 && content.First.Value == ':') {
					content.RemoveFirst();

					var typename = NextTypename(content);
					if(!Features.FindType(typename, out var type))
						throw new Exception($"Type '{typename}' could not be found for variable '{variable}'");

					types.Add(variable, type);
				} else
					types.Add(variable, typeof(object));

				if(content.Count == 0)
					continue;

				switch(content.First.Value) {
					case Ops.Gpc:
						break;
					case ',':
						content.RemoveFirst();
						break;
					default:
						throw new ParseException($"Expected comma to separate variables in '{string.Concat(content)}'");
				}
			}

			//Ensure close parenthesis was specified to stop acquiring variables
			if(content.Count > 0 && content.First.Value == Ops.Gpc)
				content.RemoveFirst();
			else
				throw new ParseException($"Expected '{Ops.Gpc}' to stop declaring variables in '{string.Concat(content)}'");

			return new Specification(name, variables, types);
		}

		/// <param name="type">Type to parse name of</param>
		/// <param name="fullName">Whether the types full name should be used</param>
		/// <returns>Neatly formatted typename with generic parameters if any</returns>
		public static string GetTypename(Type type, bool fullName = false) {
			var name = fullName ? type.FullName : type.Name;

			if(!type.IsGenericType)
				return name;

			return $"{name.Split('`').First()}<{string.Join(", ", type.GetGenericArguments().Select(t => GetTypename(t, fullName)))}>";
		}
	}
}
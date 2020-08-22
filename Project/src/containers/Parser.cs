using System;
using System.Collections.Generic;
using System.Linq;

namespace Formulas {
	/// <summary>Parses formula text in useable information</summary>
	class Parser {
		/// <summary>Whether the formula was given with a description</summary>
		public bool HasDescription => description != null;

		private readonly string description;
		private readonly string definition;

		/// <param name="formula">Formula text</param>
		public Parser(string formula) {
			if(formula.IndexOf('=') != -1) {
				var sections = formula.Split(new[]{'='}, 2);
				description = sections[0];
				definition = sections[1];
			} else {
				definition = formula;
				description = null;
			}
		}

		/// <summary>Creates a syntax tree from the right side of the formula</summary>
		/// <returns>The created syntax tree</returns>
		public Node CreateDefinition() {
			var tokenizer = new Tokenizer(definition);

			//Get the first node
			var current = tokenizer.ConsumeNode();

			if(!current)
				throw new ParseException("Formula is empty!");

			//Extend the tree with newly parsed nodes until none left
			while(!tokenizer.Empty) {
				var next = tokenizer.ConsumeNode();

				if(!next)
					break;

				current = Extend(current, next);
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

		/// <summary>Creates a description from the left side of the formula</summary>
		/// <returns>The created description</returns>
		public Description CreateDescription() {
			var tokenizer = new Tokenizer(description);

			if(tokenizer.Empty)
				throw new ParseException("Description is empty");

			//First part is formula name
			var name = tokenizer.ConsumeName();

			//Ignore whitespace
			while(!tokenizer.Empty && char.IsWhiteSpace(tokenizer.Next))
				tokenizer.Consume();

			//If done, only a name was specified
			if(tokenizer.Empty)
				return new Description(name);

			//Start acquiring variable names/types once open parentheses is encountered
			if(tokenizer.Next == Op.Gpo)
				tokenizer.Consume();
			else
				throw new ParseException($"Expected '{Op.Gpo}' to start declaring variables in '{description}'");

			var variables = new List<string>();
			var types = new Dictionary<string, Type>();

			//Capture the variables and their types until close parenthesis is encountered
			while(!tokenizer.Empty && tokenizer.Next != Op.Gpc) {
				var variable = tokenizer.ConsumeVariable();
				variables.Add(variable);

				if(!tokenizer.Empty && tokenizer.Consume() == ':') {
					tokenizer.Consume();

					var typename = tokenizer.ConsumeTypename();
					if(!Features.FindType(typename, out var type))
						throw new ParseException($"Type '{typename}' could not be found for variable '{variable}' in '{description}'");

					types.Add(variable, type);
				} else
					types.Add(variable, typeof(object));

				if(tokenizer.Empty)
					continue;

				switch(tokenizer.Next) {
					case Op.Gpc:
						break;
					case ',':
						tokenizer.Consume();
						break;
					default:
						throw new ParseException($"Expected comma to separate variables in '{description}'");
				}
			}

			//Ensure close parenthesis was specified to stop acquiring variables
			if(!tokenizer.Empty && tokenizer.Next == Op.Gpc)
				tokenizer.Consume();
			else
				throw new ParseException($"Expected '{Op.Gpc}' to stop declaring variables in '{description}'");

			return new Description(name, variables, types);
		}

		/// <summary>Inserts a node into the syntax tree based on the node previously inserted</summary>
		/// <param name="lastNode">Previously inserted node</param>
		/// <param name="currentNode">Node to insert</param>
		/// <returns>Last node inserted during the extension process</returns>
		public static Node Extend(Node lastNode, Node currentNode) {
			switch(currentNode) {
				case OpNode currentOpNode:
					switch(lastNode) {
						case OpNode lastOpNode:
							switch(currentOpNode.value) {
								case Operation.Subtract:
									return Extend(lastOpNode, new OpNode(Operation.Negate));
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

					return Extend(Extend(lastNode, new OpNode(Operation.Transform)), currentNode);
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
							return Extend(Extend(lastNode, new OpNode(Operation.Multiply)), currentNode);
					}
					break;
			}

			return currentNode;
		}

		/// <summary>Approximates a description based on a syntax tree</summary>
		/// <param name="tree">The syntax tree</param>
		/// <returns>The created description</returns>
		public static Description CreateDescription(Node tree) {
			if(!tree.Left && !tree.Right) {
				//A tree consisting of only a text node must be a variable
				if(tree is TextNode textNode)
					return new Description("", new List<string>(){textNode.value}, new Dictionary<string, Type>(){[textNode.value] = typeof(object)});

				return new Description("");
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
			return new Description("", variables, variables.ToDictionary(v => v, v => typeof(object)));
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
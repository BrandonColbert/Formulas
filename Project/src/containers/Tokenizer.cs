using System.Collections.Generic;
using System.Text;

namespace Formulas {
	/// <summary>Allows tokens to be extracted from text</summary>
	class Tokenizer {
		/// <summary>Whether any text remains to be tokenized</summary>
		public bool Empty => content.Count == 0;

		/// <summary>Next character (not consumed)</summary>
		public char Next => content.First.Value;

		/// <summary>Next encountered operator or null terminator if none found (not consumed)</summary>
		public char NextOperator {
			get {
				var next = content.First;

				//Iterate characters until operator is found
				while(!Op.Is(next.Value)) {
					if(next.Next == null)
						return '\0';

					next = next.Next;
				}

				//If none left, return null terminal signifying no operator
				return next.Value;
			}
		}

		private LinkedList<char> content;

		/// <param name="text">Text to tokenize</param>
		public Tokenizer(string text) => content = new LinkedList<char>(text);

		/// <returns>The next character</returns>
		public char Consume() {
			var value = content.First.Value;
			content.RemoveFirst();

			return value;
		}

		/// <returns>Next property or function name</returns>
		public string ConsumeName() {
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

		/// <returns>Next variable</returns>
		public string ConsumeVariable() {
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
			value.Append(ConsumeName());

			return value.ToString();
		}

		/// <returns>Next numeric text</returns>
		public string ConsumeNumericText() {
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

		/// <returns>Next number</returns>
		public Number ConsumeNumber() {
			var value = ConsumeNumericText();

			//Parse numeric result if possible
			if(Number.TryParse(value, out var number))
				return number;

			throw new ParseException($"Unable to convert '{value}' from '{value}{string.Concat(content)}' to a number");
		}

		/// <returns>Next typename</returns>
		public string ConsumeTypename() {
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

		/// <returns>Next node</returns>
		public Node ConsumeNode() {
			var next = content.First.Value;
			content.RemoveFirst();

			//Ignore leading whitespace
			while(char.IsWhiteSpace(next)) {
				next = content.First.Value;
				content.RemoveFirst();
			}

			switch(next) {
				case Op.Add: //Account for operators
					return new OpNode(Operation.Add);
				case Op.Sub:
					return new OpNode(Operation.Subtract);
				case Op.Mul:
					return new OpNode(Operation.Multiply);
				case Op.Div:
					return new OpNode(Operation.Divide);
				case Op.Mod:
					return new OpNode(Operation.Modulo);
				case Op.Pow:
					return new OpNode(Operation.Power);
				case Op.Idx:
					return new OpNode(Operation.Index);
				case Op.Prp:
					return new OpNode(Operation.Property);
				case Op.Gpo: {
					var depth = 0;
					var subtext = new StringBuilder();

					//Capture all content within same-level parenthesis
					while(content.Count > 0) {
						next = content.First.Value;
						content.RemoveFirst();

						if(next == Op.Gpo)
							depth++;
						else if(next == Op.Gpc) {
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
				case Op.Mag: {
					var depth = 0;
					var subtext = new StringBuilder();

					//Capture all content within same-level vertical bars
					while(content.Count > 0) {
						next = content.First.Value;
						content.RemoveFirst();

						if(next == Op.Gpo)
							depth++;
						else if(next == Op.Gpc)
							depth--;
						else if(depth == 0 && next == Op.Mag)
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
						switch(NextOperator) {
							case Op.Mag:
							case Op.Gpo:
								return new NameNode(ConsumeName());
							default:
								return new VariableNode(ConsumeVariable());
						}
					else if(char.IsDigit(next)) //Beginning with a digit indicates a number
						return new NumberNode(ConsumeNumber());
					else //Unrecognized symbol otherwise
						throw new ParseException($"Unexpected symbol '{next}' in '{string.Concat(content)}'");
			}
		}
	}
}
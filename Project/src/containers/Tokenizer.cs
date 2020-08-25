using System.Collections.Generic;
using System.Text;

namespace Formulas {
	/// <summary>Allows tokens to be extracted from text</summary>
	class Tokenizer {
		private LinkedList<char> content;

		/// <param name="text">Text to tokenize</param>
		public Tokenizer(string text) => content = new LinkedList<char>(text);

		/// <summary>Whether any text remains to be tokenized</summary>
		public bool Empty => content.Count == 0;

		/// <summary>Next character (not consumed)</summary>
		public char Next => content.First.Value;

		/// <summary>Last encountered operator or null terminator if none yet</summary>
		public char LastOperator { get; private set; } = '\0';

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

		/// <returns>The next character</returns>
		public char Consume() {
			var value = Next;
			content.RemoveFirst();

			return value;
		}

		/// <returns>Next property or function name</returns>
		public string ConsumeName() {
			var value = new StringBuilder();

			//Append [0-9a-zA-Z_] and ignore whitespace until next character fails to match
			while(!Empty) {
				switch(Next) {
					case '_':
						break;
					default:
						if(char.IsLetterOrDigit(Next))
							break;
						if(char.IsWhiteSpace(Next)) {
							Consume();
							continue;
						}

						return value.ToString();
				}

				value.Append(Consume());
			}

			return value.ToString();
		}

		/// <returns>Next variable</returns>
		public string ConsumeVariable() {
			if(content.Count == 0)
				throw new ParseException("Text cannot come next in an empty string");

			while(char.IsWhiteSpace(Next))
				Consume();

			if(!char.IsLetter(Next))
				throw new ParseException($"A variable does not come next in '{string.Concat(content)}'");

			var value = new StringBuilder();

			//Add variable name
			value.Append(Consume());

			//If no subscript delimiter found, variable has no subscript
			if(content.Count == 0 || Next != '_')
				return value.ToString();

			//Add subscript symbol
			value.Append(Consume());

			//Add next word as subscript
			value.Append(ConsumeName());

			return value.ToString();
		}

		/// <returns>Next numeric text</returns>
		public string ConsumeNumericText() {
			var value = new StringBuilder();

			//Append [0-9\.] and ignore whitespace until next character fails to match
			while(!Empty) {
				switch(Next) {
					case '.':
						break;
					default:
						if(char.IsDigit(Next))
							break;
						if(char.IsWhiteSpace(Next)) {
							Consume();
							continue;
						}

						return value.ToString();
				}

				value.Append(Consume());
			}

			return value.ToString();
		}

		/// <returns>Next number</returns>
		public Number ConsumeNumber() {
			var value = ConsumeNumericText();

			//Parse numeric result if possible
			if(Number.TryParse(value, out var number))
				return number;

			throw new ParseException($"Unable to convert '{value}' before '{string.Concat(content)}' to a number");
		}

		/// <returns>Next typename</returns>
		public string ConsumeTypename() {
			var value = new StringBuilder();
			var depth = 0;

			while(!Empty) {
				switch(Next) {
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
						if(char.IsLetterOrDigit(Next))
							break;
						if(char.IsWhiteSpace(Next)) {
							Consume();
							continue;
						}

						return value.ToString();
				}

				value.Append(Consume());
			}

			return value.ToString();
		}

		/// <returns>Next node</returns>
		public Node ConsumeNode() {
			var next = Consume();

			//Ignore leading whitespace
			while(char.IsWhiteSpace(next))
				next = Consume();

			//Save last operator
			if(Op.Is(next))
				LastOperator = next;

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
					while(!Empty) {
						next = Consume();

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
					while(!Empty) {
						next = Consume();

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

					if(char.IsLetter(next)) { //Beginning with a letter indicates variable, property, or function
						switch(LastOperator) {
							case Op.Idx:
								return new NameNode(ConsumeName());
						}

						switch(NextOperator) {
							case Op.Mag:
							case Op.Gpo:
								return new NameNode(ConsumeName());
						}

						return new VariableNode(ConsumeVariable());
					} else if(char.IsDigit(next)) //Beginning with a digit indicates a number
						return new NumberNode(ConsumeNumber());
					else //Unrecognized symbol otherwise
						throw new ParseException($"Unexpected symbol '{next}' at '{string.Concat(content)}'");
			}
		}
	}
}
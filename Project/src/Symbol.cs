namespace Formulas {
	/// <summary>Indicates variables and operators</summary>
	struct CharSymbol {
		public char value;
		public CharSymbol(char value) => this.value = value;
		public override string ToString() => value.ToString();
	}

	/// <summary>Indicates functions and variables with member access</summary>
	struct StringSymbol {
		public string value;
		public StringSymbol(string value) => this.value = value;
		public StringSymbol(char[] value) => this.value = new string(value);
		public override string ToString() => value;
	}
}
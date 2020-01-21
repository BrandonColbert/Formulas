namespace Formulas {
	/// <summary>Indicates functions and variables with member access</summary>
	struct StringSymbol {
		public string value;
		public StringSymbol(string value) => this.value = value;
		public StringSymbol(char[] value) => this.value = new string(value);
		public override string ToString() => value;
	}
}
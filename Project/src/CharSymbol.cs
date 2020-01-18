namespace Formulas {
	/// <summary>Indicates variables and operators</summary>
	public struct CharSymbol {
		public char value;
		public CharSymbol(char value) => this.value = value;
		public override string ToString() => value.ToString();
	}
}
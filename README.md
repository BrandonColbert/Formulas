# Formulas
A Formula describes a way to convert multiple inputs into an output.
They allow for minimal context switching between a host program and its scripting language.
## Example
```C#
            Input Mapping           Initial Input
                   |                      |
             ------------                ---
            |            |              |   |
new Formula("f(x, y, z) = z(x/y + z^2)", 0.5).Solve(20, 4);
                         |            |            |     |
                          ------------              -----
                               |                      |
                           Operations               Input
```
## Input Mapping (optional)
- Specifies what variables to assign each value in the order that input is received.
- Defaults to the order in which each variable first appears in "Operations".
## Operations
- What operations to execute on the variables and values to acquire a solution.
## Initial Input (optional)
- Any amount of inputs to be mapped to variables in the given order every time a solution is calculated.
- Always mapped before "Input".
## Input
- Any amount of inputs to be mapped to variables.
# General
- Notation is 'f(v1,v2,...,vn)=t' where vn is the nth value input and t is the expected return type.
- Values and inputs used must also support the operators used on them.
- Math functions involving angles use radians.
# Operations (precedence)
	.  - Member access 1
	:  - Indexer access 1
	() - Grouping 2
	|| - Magnitude grouping 2
	^  - Exponentiation 3
	*  - Multiplication 4
	/  - Division 4
	%  - Modulus 4
	+  - Addition 5
	-  - Subtraction/negation 5
# Customization
- Unary functions can registered in Features to make them callable from any formula.
- Usable types can be specified in Features to enable compilation when present in a formula.
# Rules
- Symbols are case-sensitive
- Whitespace is ignored
- Any '-' preceding a variable without a left-hand side value/variable incurs negation
- Adjacent values, variables, or non-magnitude groups incur multiplication
- Values/variables preceding '(' and proceding ')' incur multiplication with the group's result
- Functions preceding '(' incur function application to the group's result
- Functions must not be directly preceeded by variables, values, or other functions
- Operator '|' can be applied to the same rules as '(' or ')'
- Unlike operators '(' and ')', operator '|' groups as soon as possible
- A '.' proceeded by a digit is a number. If proceeded by a letter, a member reference.
- All indexer acceses convert the proceeding symbol to a string
# Formulas
A Formula describes a way to convert multiple inputs into an output.

The intended use case is to minimize context switching between a host program and its scripting language when computing numeric values.
## Example
```C#
            Description
                 |
             ----------
            |          |
new Formula("f(x, y, z) = 3(x/y + z^2)").Solve(8, 2, 4);
                         |            |       |       |
                          ------------         -------
                               |                  |
                           Definition           Input
```
## Description (Optional)
- Indicates what variables to assign each value in the order that input is received.
- If excluded, the variables are inferred in the order of their first appearance in the definition.
## Definition
- How to operate on the variables and values to acquire a solution.
## Input
- Any amount of inputs to be mapped to their corresponding variables based on their order.
# General
- Notation is 'f(v1,v2,...,vn)=t' where vn is the nth value input and t is the expected return type.
- Values and inputs used must also support the operators used on them.
- Math functions involving angles use radians.
# Operators
	.	Member Access
	:	Indexer
	()	Grouping
	||	Magnitude
	^ 	Exponentiation
	* 	Multiplication
	/ 	Division
	% 	Modulus
	+ 	Addition
	- 	Subtraction/Negation

## Precedence (Highest First)
| Symbol | Type | Associativity |
|---|---|---|
|`. :`| Binary | -> |
|`-`| Unary | <- |
|`( ) \|`| Grouping | -> |
|`^`|  Binary | -> |
|`* / %`| Binary | -> |
|`+ -`| Binary | -> |

# Customization
- Transform functions can registered in Features to be called from any Formula.
- Usable types can be added in Features to enable compilation when present in a formula.
- By default, type deduction is used determine types from their specified name when not found in Features.
# Rules
- Variables are case-sensitive.
- Whitespace is ignored.
- Any `-` preceding a variable without a left-hand side value/variable incurs negation.
- Adjacent non-operators (excluding `(`, `)`, and `|`) incur multiplication.
- Names preceding `(` or `|` incur function application to the group's result.
- Functions must not be directly preceeded by text.
- When `.` is proceeded by a digit, it serves as a decimal point. If proceeded by a letter, a member access.
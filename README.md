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
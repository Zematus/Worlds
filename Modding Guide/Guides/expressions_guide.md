# Expressions Guide

Expressions are scripting elements within a mod that are evaluated during the game
execution. They can be used to evaluate conditions, set parameters, and modify a
game entity behavior through effects.

An expression takes the form of a logical or mathematical statement enclosed in quotes
like the following:

`"[unary-operator]element-A [binary-operator element-B]"`

The square bracket enclosed elements are optional, and an expression `element`
can be any of the following:

- a base 10 numeric value (`1`, `3`, `5.3442`, `0.2332`, etc...)
- a boolean value (`true`, `false`)
- a single word string value (`hello`)
- a string phrase enclosed in double single quotes (`''hello world!''`)
- an entity value (polities, factions, agents, cells, groups, properties, etc...)
- an entity attribute value (example: `faction.leader`, `faction.core_group`,
  `polity.get_random_group()`)
- a function (`min()`, `max()`, `normalize()`, etc...)
- a sub-expression (`1 + 1`, `3 * (2 + 1)`, etc...)

NOTE: numeric values are always base 10, and use decimal points instead of commas

There are two main types of expressions, *value* expressions, and *effect* expressions.
*Value* expressions are expressions that upon being evaluated, return a value of a
specific type such as a number, a boolean, a string, or an entity. *Effect* expressions,
when evaluated, do not return anything. Instead, they have an effect within the game,
such as modifying an entity's attribute value, creating a new entity, or destroying
it. More info below.

*Function* expressions are a special type of expression that are composed of a function
identifier and a list of 0 or more parameters enclosed within parentheses. For example,
`"max(2,3)"` is a function expression that returns the greater value within the list
of inputs (`3` in this case). There's only a limited set of valid function expressions.
A list of all currently available functions can be found in *function_expressions_guide.md*

### VALUE TYPES

Each value expression is expected to return a value of a specific type. Here's a
list of each value type and its properties.

- **Numeric Values:**
  Numeric value expressions are expressions that return a number as a result. This
  number can be an integer or a decimal value, and the range of values can go from
  **-3.40282347E+38** to **3.40282347E+38**. Though the expression system does not support
  fixed number representations larger than 10 digits. See *expression_operators_guide.md*
  for a list of operators to work with and/or return numeric values.

- **Boolean Values:**
  Boolean value expressions are expressions that return a boolean value as a result.
  The result of the expression has to resolve to either `true` or `false`, uppercase
  representations of boolean values, like `True` or `TRUE`, are considered equivalent
  to their lowercase representations. See *expression_operators_guide.md* for a list of
  operators to work with and/or return boolean values.

- **String Values:**
  String value expressions are expressions that return a string of characters that
  do not match any id assigned to an existing entity or property within a context.
  strings composed of multiple words must be enclosed within double single quotes when
  used as sub-expressions. String values can embed expressions within them by enclosing
  those expressions within `<<` `>>` blocks. Please read *string_values_guide.md* for
  more details

- **Entity Values:**
  Entity value expressions are expressions that resolve into an *entity*. *Entities*
  are a special kind of value that encapsulate in-game entities like polities, factions,
  agents, cells, groups, etc. An entity can have one more attributes that can be
  accessed through the `.` operator. Entity attributes can have a value type (number,
  boolean, string, entity) or return a function with resolves into a value or an
  effect. See *entities_guide.md* for more details on entities.

### EFFECTS

Effect expressions are a special kind of expression that do not resolve into a value.
Instead, they produce an in-game effect which depends on the type of expression.
Most effects are associated with with entity attributes. Some others are associated
with function expressions. More details about specific effect expressions can be
found in either *entities_guide.md*, *function_expressions_guide.md*, or *expression_operators_guide.md*

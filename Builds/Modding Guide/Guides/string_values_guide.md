# String Values Guide

String Values and Expressions have some properties that make them different from
other value types (numerics, booleans).

There are two types of string values, *identifier strings*, and *text strings*.

*Identifier strings* are strings that are composed by a single word with no spaces
or special characters within other than dash `-` or underscore `_`. These are mostly
used as inputs or parameters and do not need to be enclosed within quotes themselves
when used as part of an expression.

Example: `"target.type == clan"`, Here, `target.type` is an entity attribute that returns
an identifier string. `clan` is the identifier string the attribute is being compared
against.

*Text strings* are strings that can be composed of multiple words and can include special
characters. Text strings can only be assigned to the `text` attributes of certain
JSON definitions, like decision descriptions and effects. Text strings must be
enclosed within double single quotes `''` themselves to be recognized as such when used as elements within larger expressions.

Text strings can contain embedded sub-expressions whose values get resolved when the
text is used within the game. Embedded expressions must be enclosed within `<<` and `>>`
for the game to be able to recognize them within a string value.

Example:
`"text": "A new clan splits from clan <<target>> taking from <<percent(min_inf_percent_to_transfer)>> to <<percent(max_inf_percent_to_transfer)>> of its influence from it"`.
In this example of a decision's effect text, `target` resolves to the name of the
decision's target clan. `percent(min_inf_percent_to_transfer)` is a function that
returns the value of `min_inf_percent_to_transfer` formatted as a percentage string.
Likewise, `percent(max_inf_percent_to_transfer)` returns `max_inf_percent_to_transfer`
formatted as a percentage string.

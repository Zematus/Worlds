# Expression Operators Guide

Expression operators are a set of reserved symbols that can be used to perform logical
or mathematical operations over one or more expressions (see *expressions_guide.md*
for general details on expressions)

There are two types of operators, **unary**, and **binary**. Unary operators are prefixed
to a single expression, While binary operators are infixed between two expressions
and operate over both.

Multiple expressions can be chained together using binary operators. Any set of
expressions to the right of a binary operator are considered to be a single sub-expression
and sub-expressions are evaluated in right to left order. So, for example, the expression
`"5 * 1 + 1"` will be interpreted as `"5 * (1 + 1)"`, where the sub-expression `"1 + 1"`
is evaluated first, and the whole expression evaluates to `10`. Parenthesis can be
inserted to change the order in which sub-expressions are evaluated. For example in
`"(5 * 1) + 1"`, `"(5 * 1)"` is evaluated first, so the whole expression now evaluates
to `6`.

Following is a list of current supported operators enclosed in single quotes.

### UNARY OPERATORS

- Negation (mathematical): `-`
  This operator will negate the value of a **numeric** expression. For example, `"-(3 + 4)"`
  will return `-7`, `"-(-5)"` will return `5`, etc.
  Examples: `"-3"`, `"-(3 + 4)"`, `"-target.administrative_load"`

- Negation (logical): `!`
  This operator will negate the value of a **boolean** expression. For example, `"!true"` will
  return `false`, `"!(3 > 7)"` will return `true`.
  Examples: `"!false"`. `"!(target.preferences.cohesion > 0.7)"`
  in single quotes.

### BINARY OPERATORS

- Addition: `+`
  This operator will add the value of two **numeric** expressions and return the result.
  For example, `"4 + 5"` will return `9`, `"2 + 1.5 + 3"` will return `6.5`, etc.
  Examples: `"1 + 3.1"`, `"(3 + 4) + 3"`, `"target.administrative_load + 10.5"`

- Subtraction: `-`
  This operator will subtract the value of the right-side **numeric** value from the
  left-side **numeric** value and return the result. For example, `"4 - 5"` will return
  `-1`, `"8 - 1.4 - 3"` will return `3.6`, etc.
  Examples: `"4 - 3.1"`, `"(5 - 4) - 3"`, `"target.administrative_load - 30.7"`

- Multiplication: `*`
  This operator will multiply the value of two **numeric** expressions and return the
  result.  For example, `"4 * 5"` will return `20`, `"1.5 * 2 * 2"` will return `6`, etc.
  Examples: `"4 * 3.1"`, `"(5 * 4) * 3"`, `"2 * target.administrative_load"`

- Division: `/`
  This operator will divide the value of the left-side **numeric** value by the
  right-side **numeric** value and return the result. For example, `"8 / 2"` will return
  `4`, `"8 / 4 / 2"` will return 4, etc.
  NOTE: Avoid performing divisions by zero. The results are undefined.
  Examples: `"4 / 2"`, `"(5 / 5) * 2"`, `"target.administrative_load / 10"`

- Assignment: `=`
  This is a special operator that takes the result of the right-side value expression
  and assigns it to the *assignable* value expression on the left-side. For example,
  `"target.preferences.authority = (1 - 0.3)"` will assign the result of `"1 - 0.3"`
  and assign it to the authority preference of the `target` entity.
  NOTE A: An assignment expression is an **effect** expression. Not a value expression.
  NOTE B: Only certain entity attributes, like faction preferences, are considered
  assignable. Specific entity assignable attributes are described in *entities_guide.md*

- Increment: `+=`
  This is a special operator that takes the result of the right-side **numeric** value
  expression, adds to it the current *assignable* **numeric** value expression on the
  left-side and assigns the result to value expression on the left-side. For example,
  `"target.knowledges.shipbuilding.limit += 10 + 3"` will add the result of `"10 + 3"`
  to the value stored in `target.knowledges.shipbuilding.limit` and assign the resulting
  value back to `target.knowledges.shipbuilding.limit`.
  NOTE A: An increment expression is an **effect** expression. Not a value expression.
  NOTE B: Only certain entity attributes, like faction preferences, are considered
  assignable. Specific entity assignable attributes are described in *entities_guide.md*

- Decrement: `+=`
  This is a special operator that takes the result of the right-side **numeric** value
  expression, substracts it from the current *assignable* **numeric** value expression on
  the left-side and assigns the result to value expression on the left-side. For example,
  `"target.knowledges.shipbuilding.limit -= 10 + 3"` will substract the result of `"10 + 3"`
  from the value stored in `target.knowledges.shipbuilding.limit` and assign the resulting
  value back to `target.knowledges.shipbuilding.limit`.
  NOTE A: An decrement expression is an **effect** expression. Not a value expression.
  NOTE B: Only certain entity attributes, like faction preferences, are considered
  assignable. Specific entity assignable attributes are described in *entities_guide.md*

- Equality: `==`
  This operator will compare the result of the left-side value expression and compare
  it against the result of the right-side value expression. The return value will
  be `true` if both expressions are equal. `false` otherwise. For example `"2 == (1 + 1)"`
  will return `true`. `"true == false"` will return `false`.
  NOTE: Both side expressions must return the same value type for the comparison
  to be valid. Thus, a **numeric** expression can only be compared agains another **numeric**
  expression. A string can only be compared against another string. And so on...

- Inequality: `!=`
  This operator will compare the result of the left-side value expression and compare
  it against the result of the right-side value expression. The return value will
  be `false` if both expressions are equal. `true` otherwise. For example `"4 != 4"`
  will return `false`. `"true != false"` will return `true`.
  NOTE: Both side expressions must return the same value type for the comparison
  to be valid. Thus, a **numeric** expression can only be compared agains another **numeric**
  expression. A string can only be compared against another string. And so on...

- More than: `>`
  This operator will compare the result of the left-side **numeric** expression and compare
  it against the result of the right-side **numeric** expression. The return value will
  be `true` if the left-side value is greater than the right side value. `false`
  otherwise. For example `"4 > 3"` will return `true`. `"2.5 > 3"` will return `false`.

- Less than: `<`
  This operator will compare the result of the left-side **numeric** expression and compare
  it against the result of the right-side **numeric** expression. The return value will
  be `true` if the left-side value is lesser than the right side value. `false`
  otherwise. For example `"4 < 3"` will return `false`. `"2.5 < 3"` will return `true`.

- More Than or Equal: `>=`
  This operator will compare the result of the left-side **numeric** expression and compare
  it against the result of the right-side **numeric** expression. The return value will
  be `true` if the left-side value is equal or greater than the right side value.
  `false` otherwise. For example `"4 >= 3"` will return `true`. `"3 >= 3"` will also
  return `true`. `"2.5 >= 3"` will return `false`.

- Less Than or Equal: `<=`
  This operator will compare the result of the left-side **numeric** expression and compare
  it against the result of the right-side **numeric** expression. The return value will
  be `true` if the left-side value is equal or lesser than the right side value.
  `false` otherwise. For example `"4 <= 3"` will return `false`. `"2 <= 3"` will return
  `true`, `"3 <= 3"` will also return `true`.

- Logical *and*: `&&`
  This operator will return `true` if both the left-side **boolean** expression and the
  right-side **boolean** expression return `true`. The operation returns `false` otherwise.
  For example: `"(2 == 2) && (3 > 1)"` will return `true`. `"(4 > 3) && (2 == 1)"` will
  return `false` because `"2 == 1"` evaluates to `false` even though `"4 > 3"` evaluates
  to `true`.

- Logical *or*: `||`
  This operator will return `true` if either the left-side **boolean** expression or the
  right-side **boolean** expression return `true`. The operation returns `false` only
  if both side expressions return `false`.
  For example: `"(2 == 2) && (3 > 1)"` will return `true`. `"(4 > 3) && (2 == 1)"` will
  also return `true` because `"4 > 3"` evaluates to `true` even though `"2 == 1"` evaluates
  to `false`.

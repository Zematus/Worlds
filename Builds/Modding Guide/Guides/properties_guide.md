# Properties Guide

**Properties** are modding elements that can be added to a particular context, like
an event, decision, description, option, etc. Properties are meant to abstract and
reuse large expressions that need to be evaluated just once. Properties only exist
within the context where they were defined and any of its sub-contexts that do not
override said property.

### Property Structure

```
{                 -- parent context --
  "properties": [ -- list of properties --
    {
      "id":       -- (required) Unique adjective identifier, this id will be used
                     to access the property within the context. If a sub-context
                     overrides the property, then the id will refer to the sub-context
                     property, but only within that sub-context.

      "value":    -- (required) VALUE EXPRESSION to be evaluated (once) when using
                     the property (see note 2)
    },
    ...           -- additional properties --
  ]
}
```

A property value is evaluated just once per context instance. So, for example, when
evaluating an event, Properties defined within its context will be evaluated once
upon being referenced and retain their values while that evaluation is taking place.
Upon reevaluating the event, properties will be reevaluated again as needed.

A property's value can be accesses by its id. For example, a property with id `"two_plus_three"`
and value `"2 + 3"` can be used within an expression like this `"two_plus_three + 4"`
which evaluates to **9**. A property value can also be accessed by it's attribute `"value"`
like in this example expression `"1 + two_plus_three.value"` which evaluates to **6**.
See *expressions_guide.md* for more details on how to define expressions.

A property can reference properties that were defined previously within that same
context like in the following example:

  `{ "id": "val_2", "value": "2" },`  
  `{ "id": "val_4", "value": "val_2 + val_2" }`

`"val_2"` evaluates to **2**, and `"val_4"` evaluates to **4** by adding the value of `"val_2"`
to itself in the previous example.

-- Notes --
1. Remove any trailing commas or the file won't be parsed
2. A value expression is any expression that returns a value like a **boolean**, **numeric**,
   **entity**, or **string** expression. But not an **effect** expression. Refer to
   *expressions_guide.md* for more details

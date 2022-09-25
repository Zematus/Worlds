# Effects Modding Guide

Effects are JSON sub-objects that are added to decision options or other mod objects that
define a set of effects to occur within the simulation when triggered. They have the
following structure:

### Object Structure

```
    {
      "id":                         -- (required) Unique effect identifier. Each effect
                                       must have a unique id within the list of effects
                                       of the mod object they are part of.

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the effect definition. Properties are predefined
                                       values and expressions that can be reused in
                                       different places within the definition. Please
                                       read properties_guide.md for more details
                                       on how to define valid properties.

      "text":                       -- (required) Text to generate when this effect
                                       object is evaluated. This text will be displayed
                                       when needed by the object this effect is associated
                                       with (for example, as an option tooltip) See
                                       string_values_guide.md to find more about how
                                       to define valid dynamic text values.

      "result":                     -- (required) EFFECT EXPRESSION to evaluate after
                                       this effect has been triggered. This can introduce
                                       changes to the target or any related entity.
                                       Please read expressions_guide.md for more
                                       details on how to define valid effect expressions.
    }
```

## Notes
1. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a JSON parsing error.

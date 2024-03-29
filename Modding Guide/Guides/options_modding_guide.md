# Options Modding Guide

Options are JSON sub-objects that are added to decisions or other mod objects that
can generate buttons to display within a dialog. They have the following structure:

### Object Structure

```
    {
      "id":                         -- (required) Unique option identifier. Each option
                                       must have a unique id within the list of options
                                       of the mod object they are part of.

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the option definition. Properties are predefined
                                       values and expressions that can be reused in
                                       different places within the definition. Please
                                       read properties_guide.md for more details
                                       on how to define valid properties.

      "text":                       -- (required) Text to generate when this option
                                       object is evaluated. See string_values_guide.md
                                       to find more about how to define valid dynamic
                                       text values.

      "conditions":                 -- (optional) List of BOOLEAN EXPRESSIONS to evaluate
                                       before using this option object. If any of
                                       the expressions evaluate to 'false', then the
                                       option object is skipped and its button not
                                       generated. If no conditions are given, then
                                       the the option will always be presented.

      "weight":                     -- (optional) NUMERIC EXPRESSION that will give
                                       this option an AI selection weight value. Whenever
                                       the AI has to randomly pick within a set of
                                       options, it will use the weight value of each
                                       option to adjust the chances of picking it.
                                       If none is given, then the default weight value
                                       given will be 1.

      "effects":                    -- (required) List of EFFECTS that will trigger
                                       when this option is selected either by a human
                                       player or the AI. Please read effects_guide.md
                                       for more details on how to define valid effects.
    }
```

## Notes
1. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a JSON parsing error.

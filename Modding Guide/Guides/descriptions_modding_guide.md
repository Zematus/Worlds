# Descriptions Modding Guide

Descriptions are JSON sub-objects that are added to decisions or other mod objects
that can generate texts to display within the game. They have the following structure:

### Object Structure

```
    {
      "id":                         -- (required) Unique description identifier.
                                       Each description must have a unique id within
                                       the list of descriptions of the mod object
                                       they are part of.

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the description definition. Properties are
                                       predefined values and expressions that can be
                                       reused in different places within the definition.
                                       Please read properties_guide.md for more details
                                       on how to define valid properties.

      "text":                       -- (required) Text to generate when this description
                                       object is evaluated. See string_values_guide.txt
                                       to find more about how to define valid dynamic
                                       text values.

      "conditions":                 -- (optional) List of BOOLEAN EXPRESSIONS to evaluate
                                       before using this description object. If any
                                       of the expressions evaluate to 'false', then
                                       the description object is skipped and its text
                                       not generated. If no conditions are given, then
                                       the the text will always be presented.
    }
```

## Notes
1. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a JSON parsing error.

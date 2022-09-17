# Element Modding Guide

Element modding files are located within the **Elements** folder. To be valid, mod files must have the **.json** extension and have the following file structure:

### File Structure

```
{
  "elements": [ -- list of elements --
    {
      "id":                  -- (required) Unique element identifier, if more than one definition
                                share ids, only the last one loaded will be used

      "name":                -- (required) Name of element (in plural form, see note #2)

      "adjectives":          -- (optional) List of applicable adjective words or ids, separated by commas.
                                If the if the word/id is present on a adjective mod file then the element
                                will use the adjective word described in the mod entry (if applicable).
                                
      "regionConstraints":   -- (optional) List of region constraints, separated by commas (see note #3)

      "phraseAssociations":  -- (required) List of phrase association strings, separated by commas (see note #4)
    },
    ... -- additional elements --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed
2. The name string should be written in translatable plural form. Examples: "stone:s", "grass:es". Refer to language_modding_guide.md for more details on how to define translatable nouns.
3. Each region constraint is used to decide whether or not to associate the element with a particular region. If any of the constraints fails then the element won't be associated with the region. Refer to region_constraints_guide.md for more details on how to define and use region constraints
4. Each phrase association is used to form nouns or noun phrases for procedurally generated proper names and other types of texts. Refer to phrase_association_guide.md for more details on how to define phrase associations

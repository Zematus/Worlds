# Adjective Modding Guide

**Adjective** mod files are located within the *Adjectives* folder. To be valid, mod files must have the **.json** extension and have the following file structure:

#### File Structure

```
{
  "adjectives": [ -- list of adjectives --
    {
      "id":                  -- (required) Unique adjective identifier, if more than one definition share ids, only the last one loaded will be used
      "word":                -- (required) the adjective

      "elements":            -- (optional) List of element ids this adjective can be applied to, separated by commas
      "regionAttributes":    -- (optional) List of region attribute ids this adjective can be applied to, separated by commas

      "regionConstraints":   -- (optional) List of region constraints, separated by commas (see note #2)
    },
    ...                      -- additional adjectives --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed.
2. These are used to decide whether or not to associate the element with a particular **region**. If any of the constraints fails then the adjective won't be used when generating names of phrases within the region. Refer to *region_constraints_guide.txt* for more details on how to define and use region constraints.

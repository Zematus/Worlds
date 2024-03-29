# Region Attribute Modding Guide

Region attribute mod script files are located within the *RegionAttributes* folder. To be valid, mod files must have the **.json** extension and have the following file structure:

### File Structure

```
{
  "region_attributes": [ -- list of attributes --
    {
      "id":                  -- (required) Unique region attribute identifier, if more than one definition share ids, only the last one loaded will be used
      "name":                -- (required) Name of attribute
      "adjectives":          -- (optional) List of applicable adjective words or ids, separated by commas. If the if the word/id is present on a adjective mod file then the element will use the adjective word described in the mod entry (if applicable).
      "variants":            -- (required) List of naming variations, separated by commas (see note #2)
      "regionConstraints":   -- (optional) List of region constraints, separated by commas (see note #3)
      "phraseAssociations":  -- (required) List of phrase association strings, separated by commas (see note #4)
      "secondary":           -- (optional) 'true' if this attribute should only be validated after all primary attributes have been validated (default: false)
    },
    ... -- additional attributes --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed
2. At least one variant must be present for each attribute:

  Variant nouns are defined as follow: `<noun>({(<relation>):<noun or suffix>})(:<noun or suffix>({...}))...`
  Parts enclosed within parenthesis denote optional definition elements. Parts enclosed within brackets indicate that the variant should be decomposed into two variants, one of which omits the enclosed part. The `<relation>` keyword (enclosed in `<` and `>`) is used to denote parts that should be filtered when constructing specific types of phrases (mostly used to filter plural nouns).
  Here are some variant examples:
    - `"glacier{<relation>:s}"` which decomposes into variants `"glacier"` and `"glacier:s"`
    - `"shrub:land{:s}"` which decomposes into variants `"shrub:land"` and `"shrub:land:s"`
    - `"grass:land{:s}"` which decomposes into variants `"grass:land"` and `"grass:land:s"`
    - `"forest{<relation>:s}"` which decomposes into variants `"forest"` and `"forest:s"`
    - `"waste{:land}{:s}"` which decomposes into variants `"waste"`, `"waste:s"`, `"waste:land"`, and `"waste:land:s"`

  Refer to language_modding_guide.md for more details on how to define translatable nouns.

3. Each region constraint is used to decide whether or not to associate the element with a particular region. If any of the constraints fails then the attribute won't be associated with the region. Refer to *region_constraints_guide.md* for more details on how to define and use region constraints
4. Each phrase association is used to form nouns or noun phrases for procedurally generated proper names and other types of texts. Refer to *phrase_association_guide.md* for more details on how to define phrase associations

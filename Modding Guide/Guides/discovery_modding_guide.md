# Discovery Modding Guide

Discovery mod files are located within the *Discoveries* folder. To be valid, mod files must have the **.json** extension and have the following file structure:

### File Structure

```
{
  "discoveries": [ -- list of discoveries --
    {
      "id":                         -- (required) Unique discovery identifier, if more
                                       than one definition share ids, only the last one
                                       loaded will be used

      "name":                       -- (required) Name of discovery

      "gainEffects":                -- (optional) List of EFFECT EXPRESSIONS to
                                       evaluate after the target group 'gains' the discovery.
                                       These can introduce changes to the target entity.
                                       Please read expressions_guide.md for more details on
                                       how to define valid effect expressions.

      "lossEffects":                -- (optional) List of EFFECT EXPRESSIONS to
                                       evaluate after the target group 'losses' the discovery.
                                       These can introduce changes to the target entity.
                                       Please read expressions_guide.md for more details on
                                       how to define valid effect expressions.
    },
    ... -- additional discoveries --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed
2. A discovery **target** is always the cell group entity that gains or loses the discovery
3. Discoveries are normally gained or lost as the result of events, actions, or decisions that perform the **add** or **remove** effects on the **discoveries** attribute of a cell group entity

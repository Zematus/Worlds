# Cultural Preferences Modding Guide

Cultural Preferences modding files are located within the **Preferences** folder. To be valid, mod files must have the **.json** extension and have the following file structure:

#### File Structure
Note: .json files do not support comments. Remove texts enclosed within double dashes '--'

```
{
  "preferences": [ -- list of preferences --
    {
      "id":                  -- (required) Unique preference identifier, if more than one definition share ids, only the last one loaded will be used
      "name":                -- (required) text to use when displaying the preference name within the game
    },
    ... -- additional preferences --
  ]
}
```

#### Additional Notes
1. Remove any trailing commas or the file won't be parsed

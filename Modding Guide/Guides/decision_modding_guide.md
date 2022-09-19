# Decision Modding Guide

Decision mod files are located within the **Decisions** folder. To be valid, mod
files must have the **.json** extension and have the following file structure:

### File Structure

```
{
  "decisions": [                    -- list of decisions --
    {
      "id":                         -- (required) Unique decision identifier (see note 2).

      "name":                       -- (required) Name of decision.

      "target":                     -- (required) Target entity type for the decision.
                                       For now, the only acceptable value is "faction".

      "parameters":                 -- (optional) List of parameters that are required
                                       by the decision to take place and which will
                                       be supplied by the event triggering the decision.
                                       Each parameter is a JSON object with an 'id'
                                       and a 'type' attributes. 'id' has to be
                                       unique within the decision and will be used
                                       to obtain the parameter value. 'type' indicates
                                       the type of value and can be any of the following:
                                       - "group": a group entity value
                                       - "faction": a faction entity value
                                       - "polity": a polity entity value
                                       - "cell": a cell entity value
                                       - "agent": a agent entity value
                                       - "string": a single word string value
                                       - "text": a multi-word string value
                                       - "number": a numeric value
                                       - "boolean": a boolean value

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the decision definition. Properties are
                                       predefined values and expressions that can be
                                       reused in different places within the definition.
                                       Please read properties_guide.md for more details
                                       on how to define valid properties.

      "description":                -- (required) List of DESCRIPTION objects. These
                                       will be evaluated before a decision dialog is
                                       loaded to generate the body text within the
                                       dialog. The texts extracted will be presented
                                       in the same order as they appear on this list.
                                       Please read descriptions_modding_guide.md for
                                       more details on how to define decision description
                                       objects.

      "options":                    -- (required) List of OPTION objects. These will
                                       be evaluated before a decision dialog is loaded
                                       to generate the list of options to present to
                                       the player. The options will appear in the
                                       order they are defined on this list. Please
                                       read options_modding_guide.md for more details
                                       on how to define decision option objects.

      "enableDebugLog":             -- (optional) Can only have 'true' or 'false'
                                       as value (default: 'false'). This an option
                                       to assist in mod development. If this is
                                       'true', and 'Debug Mode' is enabled within
                                       the game, then debug information specific
                                       to this decision will be logged during the
                                       game execution.

      "debugPlayerGuidance":        -- (optional) Can only have 'true' or 'false'
                                       as value (default: 'false'). This an option
                                       to assist in mod development. If this is 'true',
                                       the game will pause whenever this decision is
                                       invoked and will request the player to choose
                                       an option instead of letting the simulation do
                                       it.

    },
    ...                             -- additional decisions --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed
2. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a JSON parsing error.
3. Do not duplicate decision *id* values unless you want to specifically replace another decision

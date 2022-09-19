# Event Modding Guide

Event mod files are located within the **Events** folder. To be valid, mod files
must have the **.json** extension and have the following file structure:

### File Structure

```
{
  "events": [                       -- list of events --
    {
      "id":                         -- (required) Unique event identifier (see note 2).

      "name":                       -- (required) Name of event.

      "target":                     -- (required) Target entity type for the event.
                                       Has to be either "group" or "faction".

      "assignOn":                   -- (optional) List of types of situations that
                                       would make an event of this type to attempt to
                                       be assigned to the target. Defaults to "event"
                                       (see notes 3).

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the event definition. Properties are predefined
                                       values and expressions that can be reused in
                                       different places within the definition. Please
                                       read properties_guide.md for more details on
                                       how to define valid properties.

      "assignmentConditions":       -- (optional) List of BOOLEAN EXPRESSIONS to
                                       evaluate before assigning the event to the
                                       target. If any of the expressions evaluate
                                       to 'false', then the event is not assigned.
                                       Please read expressions_guide.md for more
                                       details on how to define valid boolean
                                       expressions. NOTE: assignment conditions are
                                       also evaluated before triggering an event.

      "triggerConditions":          -- (optional) List of BOOLEAN EXPRESSIONS to
                                       evaluate before triggering the event already
                                       assigned to the target. If any of the expressions
                                       evaluate to 'false', then the event is not
                                       triggered. Please read expressions_guide.md
                                       for more details on how to define valid boolean
                                       expressions.

      "timeToTrigger":              -- (required) NUMERIC EXPRESSION that defines
                                       the amount of time to pass before triggering
                                       an event. the expression must evaluate to a
                                       value between 1 and 9,223,372,036,854,775,807.

      "effects":                    -- (required) List of EFFECT EXPRESSIONS to
                                       evaluate after an event has successfully
                                       triggered. These can introduce changes to
                                       the target or any related entity. Please
                                       read expressions_guide.md for more details
                                       on how to define valid effect expressions.

      "repeatable":                 -- (optional) Can only have 'true' or 'false'
                                       as value (default: 'false'). If 'true', then
                                       the will be an attempt to reassign the event
                                       to the same target after every successful
                                       or unsuccessful trigger attempt.

      "enableDebugLog":             -- (optional) Can only have 'true' or 'false'
                                       as value (default: 'false'). This an option
                                       to assist in mod development. If this is
                                       'true', and 'Debug Mode' is enabled within
                                       the game, then debug information specific
                                       to this event will be logged during the game
                                       execution.
    },
    ...                             -- additional events --
  ]
}
```

## Notes
1. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a JSON parsing error.
2. Do not duplicate event ids unless you want to specifically replace another event
   already loaded.
3. List of valid assignment types:
  - "event": This event type might be assigned by other events (default).
  - "spawn": This event type might be assigned when the target entity spawns.
  - "status_change" This event type might be assigned when a status change occurs to
    the target entity (only applicable to faction target types).
  - "polity_contact_change" This event type might be assigned when the target's polity
    adds or removes a contact (only applicable to faction target types).

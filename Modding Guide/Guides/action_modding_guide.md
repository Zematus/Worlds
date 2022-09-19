# Action Modding Guide

**Action** mod files are located within the *Actions* folder.
To be valid, mod files must have the .json extension and have the following file structure:

#### File Structure

```
{
  "actions": [                    -- list of actions --
    {
      "id":                         -- (required) Unique action identifier (see note 2).

      "name":                       -- (required) Name of action.

      "category":                   -- (required) The category the action should
                                       be sorted into (see the 'Categories' section down below)

      "target":                     -- (required) Target entity type for the action.
                                       For now, the only acceptable value is "faction".

      "properties":                 -- (optional) List of PROPERTIES that are available
                                       to the action definition. Properties are
                                       predefined values and expressions that can be
                                       reused in different places within the definition.
                                       Please read properties_guide.md for more details
                                       on how to define valid properties.

      "accessConditions":           -- (optional) List of BOOLEAN EXPRESSIONS to be evaluated
                                       to decide if action will be present on the list a available actions within the assigned category.
                                       If any of the expressions evaluate to 'false', then
                                       the action will not be present within the category
                                       menu. Please read expressions_guide.md for more
                                       details on how to define valid boolean
                                       expressions.

      "executeConditions":          -- (optional) List of CONDITION objects which are
                                       to be used to decide if an action can be executed
                                       at any particular time. If the enclosed expression
                                       evaluates to false, then the action will appear
                                       disabled on the UI. The condition object also contains
                                       instructions to display info tooltips over the
                                       action. See the 'Execution Conditions' section down
                                       below for more information.

      "effects":                    -- (required) List of EFFECT EXPRESSIONS to
                                       evaluate after an action has been executed.
                                       These can introduce changes to the target or any
                                       related entity. Please read expressions_guide.md
                                       for more details on how to define valid effect
                                       expressions.

      "enableDebugLog":             -- (optional) Can only have 'true' or 'false'
                                       as value (default: 'false'). This an option
                                       to assist in mod development. If this is
                                       'true', and 'Debug Mode' is enabled within
                                       the game, then debug information specific
                                       to this action will be logged during the
                                       game execution.
    },
    ...                             -- additional actions --
  ]
}
```

## Execute Conditions

Execute Conditions are *json* sub-objects that are added to actions that are used to evaluate
if an action is available for execution, They also define the tooltips to be displayed over the action to indicate the reason for their availability state. They have the following structure:

#### Object Structure

```
    {
      "condition":                  -- (required) BOOLEAN EXPRESSION to be evaluated
                                       to decide if action can be executed at a particular
                                       time. Please read expressions_guide.md for more
                                       details on how to define valid boolean
                                       expressions.

      "info":                       -- (required) Text to generate when a tooltip needs
                                       to be displayed. See string_values_guide.md
                                       to find more about how to define valid dynamic
                                       text values.
    }
```

## Categories

A folder called *Categories*, located within the *Actions* folder, should
contain a file named **categories.json** with a list of categories on which to group
the available actions, and a set of **PNG** image files to use as thumbnails for each
of the categories.

The file structure of **categories.json** is a follows:

```
{
  "actionCategories": [
    {
      "id":                         -- (required) Unique category identifier (see note 2).
      "name":                       -- (required) Category name.
      "image":                      -- (required) thumbnail file name.
    },
    ...                             -- additional categories --
  ]
}
```

## Notes
1. List of values must be enclosed within square brackets and separated by commas.
   Remove any trailing commas on any list enclosed by square brackets, or you'll
   get a **json** parsing error.
2. Do not duplicate action or category ids unless you want to specifically replace
another decision already loaded.

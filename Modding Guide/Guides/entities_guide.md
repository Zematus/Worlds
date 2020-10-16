# Entities Guide

**Entities** are a special type of value that represent components within the simulation or components used to encapsulate other components. Entities can have one or more attributes which can be accessed using the dot ('.') *access* operator, in the following manner: `<entity>.<attribute>`
*Example:* `target.preferences.cohesion`

An access operation is handled as an expression and as such, can either ver resolved into a value or an effect. A value returned by an access operator can be of any of the supported types: **numbers**, **booleans**, **strings** or other **entities**. Certain attributes are function-like and expect one or more parameters, and they should be invoked in this manner: `"<entity>.<attribute>(<parameters>)"`

## Currently Supported Entities:

### CELL

  This type of entity encapsulates a fixed area on the world map and has general information about that area's characteristics: longitude, latitude, altitude, temperature, rainfall, present biomes, etc.

  ***Properties:***

  - **biome_trait_presence**
    Attribute function that returns the presence of a specific biome trait within a cell as a **numeric** value between 0 and 1.
    *Examples:* `"cell.biome_trait_presence(wood)"`, `"target.cell.biome_trait_presence(sea)"`

### GROUP

  This type of entity encapsulates a population group occupying a cell on the surface of the map and contains information about characteristics like: population, cultural attributes, influences, etc.

  ***Properties:***

  - **cell**
    Attribute that returns the **cell entity** where this group is located.
    *Examples:* `"group.cell"`, `"target.cell"`

  - **prominence**
    Attribute function that returns the prominence that a particular polity has on the group as a **numeric** value.
    *Examples:* `"group.prominence(target.polity)"`, `"target.prominence(polity)"`

  - **faction_cores_count**
    Attribute that returns the amount of faction cores located in the group as a **numeric** value.
    *Examples:* `"group.faction_cores_count"`, `"target.faction_cores_count"`

  - **faction_core_distance**
    Attribute function that returns the distance between this group's cell and the closest faction core cell in the given polity as a **numeric** value.
    *Examples:* `"group.faction_core_distance(target.polity)"`, `"target.faction_core_distance(polity)"`

  - **preferences**
    Attribute that returns an entity containing all the groups's cultural preferences, whose **numeric** values can later be read from or written to.
    *Example:* `"target.preferences.authority <= 0.5"`

  - **knowledges**
    Attribute that returns an entity containing all the groups's cultural knowledges, whose **numeric** values can later be read from.
    *Example:* `"target.knowledges.social_organization <= 0.6"`

  - **polity_with_highest_prominence**
    Attribute that returns the **polity entity** that has the highest prominence value within this group.
    *Examples:* `"group.polity_with_highest_prominence"`, `"target.polity_with_highest_prominence"`


### POLITY

  This type of entity encapsulates a polity in the planet and information about it's culture, territory, factions, etc.

  ***Properties:***

  - **type**
    Attribute that returns the type of polity. Current available type: **tribe**
    *Example:* `"polity.type == tribe"`

  - **get_random_group**
    Attribute that selects and returns a random **group entity** that falls within the polity's prominence sphere.
    *Example:* `"polity.get_random_group()"`

  - **get_random_contact**
    Attribute that selects and returns a random **contact entity** encapsulating another polity that is currently in contact with this polity.
    *Example:* `"polity.get_random_contact()"`

  - **dominant_faction**
    Attribute that returns the polity's current *dominant* **faction entity**.
    *Example:* `"target == polity.dominant_faction"`

  - **transfer_influence**
    Attribute function that transfers influence between two factions within the polity.
    *Example:* `"polity.transfer_influence(source_faction, target_faction, influence_to_transfer)"`

  - **contact_strength**
    Attribute function that returns the contact strength between this polity and another one. Contact strength will be greater than 0 if both polities share a border with each other.
    *Example:* `"polity.contact_strength(neighbor_polity)"`

  - **leader**
    Attribute that returns the leader of the polity's current dominant faction as an **agent entity**.
    *Examples:* `"polity.leader.wisdom"`


### CONTACT

  This type of entity encapsulates relationship information between two polities.

  ***Properties:***

  - **polity**
    Attribute that returns the **polity entity** that this contact encapsulates relationship information for.
    *Examples:* `"contact.polity"`, `"polity.get_random_contact().polity"`


### AGENT

  This type of entity encapsulates any individual that is or was present within the simulation. That includes faction leaders.

  ***Properties:***

  - **charisma**
    Attribute that returns an agent's *charisma* level as a value between 0 and 1.
    *Example:* `"leader.charisma < 0.5"`

  - **wisdom**
    Attribute that returns an agent's *wisdom* level as a value between 0 and 1.
    *Example:* `"0.3 >= current_leader.wisdom"`


### FACTION

  This type of entity encapsulates any faction that can be part of a polity. most decisions have a faction as target entity.
  
  ***Properties:***

  - **type**
    Attribute that returns the type of faction. Current available type: **clan**
    *Example:* `"target.type == clan"`

  - **administrative_load**
    Attribute that returns the faction's current administrative load as a **numeric** value.
    *Example:* `"target.administrative_load > 100000"`

  - **influence**
    Attribute that returns the faction's influence on its containing polity as a **numeric** value.
    *Example:* `"faction.influence <= 0.6"`

  - **preferences**
    Attribute that returns an entity containing all the groups's cultural preferences, whose **numeric** values can later be read from or written to.
    *Example:* `"target.preferences.authority <= 0.5"`

  - **leader**
    Attribute that returns the leader of the polity's current dominant faction as an **agent entity**.
    *Example:* `"polity.leader.charisma"`

  - **polity**
    Attribute that returns the **polity entity** to which this faction belongs to.
    *Example:* `"faction.polity.type == tribe"`

  - **trigger_decision**
    Attribute function that will trigger a decision referenced by the given *id*. The function needs to receive as extra parameters all of the parameters needed to execute the given decision.
    *Example:* `"dominant_faction.trigger_decision(influence_demanded_from_clan, target)"`

  - **split**
    Attribute function that will trigger a faction to split a new faction from itself. The function expects two parameters: a group to become the new faction's core, and a percentage of influence (as a value between 0 and 1) to transfer from the parent faction.
    *Example:* `"target.split(new_core_group, 0.5)"`

  - **relationship**
    Attribute function that will return a **numeric** value corresponding to the current relationship value between this faction and the faction given as parameter.
    *Example:* `"target.relationship(dominant_faction)"`

  - **set_relationship**
    Attribute function that will update the relationship value between this faction and the faction given as first parameter, using the **numeric** value given in the second parameter.
    *Example:* `"dominant_faction.set_relationship(target, input_value / 2)"`

  - **core_group**
    Attribute that returns the core of this faction as a **group entity**.
    *Example:* `"target.core_group != new_core_group"`

# Entities Guide

**Entities** are a special type of value that represent components within the simulation or components used to encapsulate other components. Entities can have one or more attributes which can be accessed using the dot ('.') *access* operator, in the following manner: `<entity>.<attribute>`
*Example:* `target.preferences.cohesion`

An access operation is handled as an expression and as such, can either ver resolved into a value or an effect. A value returned by an access operator can be of any of the supported types: **numbers**, **booleans**, **strings** or other **entities**. Certain attributes are function-like and expect one or more parameters, and they should be invoked in this manner: `"<entity>.<attribute>(<parameters>)"`

## Currently Supported Entities:


### CELL

  This type of entity encapsulates a fixed area on the world map and has general information about that area's characteristics: longitude, latitude, altitude, temperature, rainfall, present biomes, etc.

  ***Attributes:***

  - **biome_trait_presence**
    Attribute function that returns the presence of a specific biome trait within a cell as a **numeric** value between 0 and 1.
    *Examples:* `"cell.biome_trait_presence(wood) < 0.3"`, `"target.cell.biome_trait_presence(sea)"`

  - **biome_type_presence**
    Attribute function that returns the presence of a specific biome type within a cell as a **numeric** value between 0 and 1. There are only three possible biome types: land, water, and ice.
    *Examples:* `"cell.biome_type_presence(water)" > 0.5`, `"target.biome_type_presence(land)"`

  - **neighbors**
    Attribute that returns a **collection** entity containing the cell entities that are adjacent to the cell (including those in the NW, NE, SW, and SE corners of it).
    *Examples:* `"cell.neighbors"`, `"target.cell.neighbors.sum[cell](cell.biome_type_presence(water))"`

  - **group**
    Attribute that returns the **group** entity that inhabits that cell (if any). PMake sure to perform an **is_null** check on the **group** attribute before using it.
    *Examples:* `"cell.group"`, `"target.cell.group.is_null"`

  - **arability**
    Attribute that returns the arability quality value of a cell as a **numeric** value between 0 and 1.
    *Examples:* `"cell.arability"`, `"target.cell.arability > 0.8"`

  - **accessibility**
    Attribute that returns the accessibility value of a cell as a **numeric** value between 0 and 1.
    *Examples:* `"cell.accessibility"`, `"target.cell.accessibility <= 0.2"`

  - **hilliness**
    Attribute that returns the hilliness value of a cell as a **numeric** value between 0 and 1. Where 0 represents mostly flat terrain, and 1 represent extremely high verticality in terrain features.
    *Examples:* `"cell.hilliness"`, `"target.cell.hilliness > 0.3"`

  - **flowing_water**
    Attribute that returns the amount of flowing water (in *mm/year*) in a cell as a **numeric** value between 0 and 100000.
    *Examples:* `"cell.flowing_water"`, `"target.cell.flowing_water > 100"`


### GROUP

  This type of entity encapsulates a population group occupying a cell on the surface of the map and contains information about characteristics like: population, cultural attributes, influences, etc.

  ***Attributes:***

  - **cell**
    Attribute that returns the **cell entity** where this group is located.
    *Examples:* `"group.cell"`, `"target.cell"`

  - **prominence_value(*polity*)**
    Attribute function that returns the prominence value that a particular polity has on the group as a **numeric** value between 0 and 1.
    *Examples:* `"group.prominence_value(target.polity)"`, `"target.prominence_value(polity)"`

  - **get_core_distance(*faction*)**
    Attribute function that returns the distance (in *km*) that a particular faction's core has on the group as a **numeric** value.
    *Examples:* `"group.get_core_distance(target)"`, `"target.get_core_distance(selected_faction)"`

  - **most_prominent_polity**
    Attribute that returns the **polity** entity that has the highest prominence value within this group.
    *Examples:* `"group.most_prominent_polity"`, `"target.most_prominent_polity"`

  - **present_polities**
    Attribute that returns a **collection** entity containing the polity entities that have a prominence value greater than 0 in this particular cell group.
    *Examples:* `"group.present_polities"`, `"target.present_polities.sum[polity](target.prominence_value(polity))"`

  - **closest_factions**
    Attribute that returns a **collection** entity containing the factions entities, whose polities have a presence in this group and whose core groups are closest to this particular group
    *Examples:* `"group.closest_factions"`, `"target.closest_factions.sum[faction](target.prominence_value(faction.polity))"`

  - **navigation_range**
    Read/write attribute which contains the current navigation range (as a **numeric** value) of boats/ships that can be built by this group.
    *Examples:* `"group.navigation_range"`, `"target.navigation_range += 10"`

  - **arability_modifier**
    Read/write attribute which contains the current arability modifier (as a **numeric** value) applied to the arability value (as a multiplier to the difference between the cell's base arability and 1) of the cell the group inhabits. NOTE: the starting arability modifier for all groups is 0.
    *Examples:* `"group.arability_modifier"`, `"target.arability_modifier -= 0.3"`

  - **accessibility_modifier**
    Read/write attribute which contains the current accessibility modifier (as a **numeric** value) applied to the accessibility value (as a multiplier to the difference between the cell's base accessibility and 1) of the cell the group inhabits. NOTE: the starting accessibility modifier for all groups is 0.
    *Examples:* `"group.accessibility_modifier"`, `"target.accessibility_modifier += 0.2"`

  - **properties**
    Attribute that returns a modifiable **container** entity of all the group's properties, which are just string keywords. This container entity has three attributes, **add**, **remove**, and **contains**.
    *Example:* `"target.properties.add(can_form_tribe)"`, `"target.properties.remove(can_form_tribe)"`, `"target.properties.contains(can_form_tribe)"`

  - **population**
    Read-only attribute that returns the current population of a cell group.
    *Example:* `"target.population"`, `"group.population <= 1000"`

  - **neighbors**
    Attribute that returns a **collection** entity containing the group's entities that inhabit cells adjacent to this group's cell (including those in the NW, NE, SW, and SE corners of it).
    *Examples:* `"group.neighbors"`, `"target.neighbors.sum[group](group.preferences.authority) / target.neighbors.count"`

  - **preferences**
    Attribute that returns a non-modifiable **container** entity of all the group's cultural preferences, whose **numeric** values can later be read from or written to.
    *Example:* `"target.preferences.authority <= 0.5"`

  - **skills**
    Attribute that returns a modifiable **container** entity of all the group's cultural skills, whose **numeric** values can later be read from.
    *Example:* `"target.skills.seafaring <= 0.5"`, `"target.skills.add(seafaring)"`

  - **activities**
    Attribute that returns a modifiable **container** entity of all the group's cultural activities, whose **numeric** values can later be read from.
    *Example:* `"target.activities.foraging <= 0.4"`, `"target.activities.remove(farming)"`

  - **knowledges**
    Attribute that returns a modifiable **container** entity of all the group's cultural knowledges, whose **numeric** values can later be read from.
    *Example:* `"target.knowledges.social_organization <= 0.6"`, `"target.knowledges.add(shipbuilding)"`

  - **discoveries**
    Attribute that returns a modifiable **container** entity of all the group's cultural discoveries.
    *Example:* `"target.discoveries.contains(sailing)"`, `"target.discoveries.add(keels)"`


### POLITY

  This type of entity encapsulates a polity in the planet and information about it's culture, territory, factions, etc.

  ***Attributes:***

  - **contacts**
    Attribute that returns a **collection** entity containing all **contact** entities that are associated with this polity.
    *Examples:* `"polity.contacts.count > 0"`, `"tribe_a.contacts.select[contact](contact.polity == tribe_b)"`

  - **dominant_faction**
    Attribute that returns the polity's current *dominant* **faction entity**.
    *Example:* `"target == polity.dominant_faction"`

  - **transfer_influence(*source_faction*,*target_faction*,*influence_to_transfer*)**
    Attribute function that transfers a percentage of influence, *influence_to_transfer*, from *source_faction* towards *target_faction*.
    *Example:* `"polity.transfer_influence(source_faction, target_faction, 0.2)"`

  - **type**
    Attribute that returns the type of polity. At the moment, the only available type is **tribe**.
    *Example:* `"polity.type == tribe"`

  - **leader**
    Attribute that returns the leader of the polity's current dominant faction as an **agent entity**.
    *Examples:* `"polity.leader.wisdom"`

  - **split(*polity_type*,*splitting_faction*)**
    Attribute function that splits the polity by creating a new polity of type *polity_type*, with *splitting_faction* as the source of the split.
    NOTE: supported polity types: *tribe*
    *Example:* `"polity.split(tribe, faction)"`

  - **merge(*polity_to_merge*)**
    Attribute function that merges *polity_to_merge* into the calling polity entity.
    *Example:* `"polity.merge(other_polity)"`

  - **neighbor_regions**
    Attribute that returns a **collection** entity containing all **region** entities which intersect or border the polity's territory and are not already part of its set of core regions.
    *Examples:* `"tribe.neighbor_regions.count > 0"`, `"target.polity.neighbor_regions.select_random()"`

  - **add_core_region(*region_to_add*)**
    Attribute function that adds the *region_to_add* entity to the polity's set of core regions.
    *Example:* `"polity.add_core_region(region)"`

  - **core_region_saturation**
    Attribute that returns a **numeric** value representing the percentage of area from the polity's core regions which is already covered by the polity's territory.
    *Example:* `"polity.core_region_saturation > 0.5"`

  - **factions**
    Attribute that returns a **collection** entity containing all **faction** entities that are members of this polity.
    *Examples:* `"polity.factions.count > 1"`, `"contact.polity.factions.select_subset[faction](faction.has_contact_with(target.polity))"`

  - **preferences**
    Attribute that returns a non-modifiable **container** entity of all the polity's cultural preferences, whose **numeric** values can later be read from.
    *Example:* `"target.preferences.authority <= 0.5"`

  - **skills**
    Attribute that returns a non-modifiable **container** entity of all the polity's cultural skills, whose **numeric** values can later be read from.
    *Example:* `"target.skills.seafaring <= 0.5"`

  - **activities**
    Attribute that returns a non-modifiable **container** entity of all the polity's cultural activities, whose **numeric** values can later be read from.
    *Example:* `"target.activities.foraging <= 0.4"`

  - **knowledges**
    Attribute that returns a non-modifiable **container** entity of all the polity's cultural knowledges, whose **numeric** values can later be read from.
    *Example:* `"target.knowledges.social_organization <= 0.6"`

  - **discoveries**
    Attribute that returns a non-modifiable **container** entity of all the polity's cultural discoveries.
    *Example:* `"target.discoveries.contains(sailing)"`


### FACTION

  This type of entity encapsulates any faction that can be part of a polity. Most decisions have a faction as target entity.

  ***Attributes:***

  - **administrative_load**
    Attribute that returns the faction's current administrative load as a **numeric** value.
    *Example:* `"target.administrative_load > 100000"`

  - **influence**
    Attribute that returns the faction's influence on its containing polity as a **numeric** value.
    *Example:* `"faction.influence <= 0.6"`

  - **leader**
    Attribute that returns the leader of the polity's current dominant faction as an **agent entity**.
    *Example:* `"polity.leader.charisma"`

  - **polity**
    Attribute that returns the **polity entity** to which this faction belongs to.
    *Example:* `"faction.polity.type == tribe"`

  - **trigger_decision(*decision_id*,*...*)**
    Attribute function that will trigger a decision referenced by the given *decision_id*. The function needs to receive as extra parameters all of the parameters needed to execute the given decision.
    *Example:* `"dominant_faction.trigger_decision(influence_demanded_from_clan, target)"`

  - **split(*core_group*,*influence_to_transfer*)**
    Attribute function that will trigger a faction to split a new faction from itself. The function expects two parameters: a group to become the new faction's core, *core_group*, and a percentage of influence, *influence_to_transfer*, (as a value between 0 and 1) to transfer from the parent faction.
    *Example:* `"target.split(new_core_group, 0.5)"`

  - **remove()**
    Attribute function that will make this faction dissapear completely. NOTE: If this faction is the only member faction of a polity, then the polity will also be removed.
    *Example:* `"faction.remove()"`

  - **migrate_core_to_group(*group*)**
    Attribute function that will make a faction replace its core group with a different one.
    *Example:* `"target.migrate_core_to_group(new_core_group)"`

  - **core_group**
    Attribute that returns the core of this faction as a **group** entity.
    *Example:* `"target.core_group != new_core_group"`

  - **type**
    Attribute that returns the type of faction. Currently the only option is **clan**.
    *Example:* `"target.type == clan"`

  - **guide**
    Attribute that returns who is currently guiding the faction. The possible values are *simulation* (as in the computer running the game simulation) or *player* (as in the person playing the game).
    *Example:* `"faction.guide == player"`

  - **get_relationship(*faction*)**
    Attribute function that will return a **numeric** value corresponding to the current relationship value between this faction and the faction given as parameter, *faction*.
    *Example:* `"target.get_relationship(dominant_faction)"`

  - **set_relationship(*faction*,*value*)**
    Attribute function that will update the relationship value between this faction and the faction given as the first parameter *faction*, using the **numeric** value given in the second parameter, *value*.
    *Example:* `"dominant_faction.set_relationship(target, input_value / 2)"`

  - **groups**
    Attribute that returns a **collection** entity containing all **group** entities that are under the direct control of this faction.
    *Examples:* `"faction.groups.count > 1"`, `"target.groups.select_subset[group](group.get_core_distance(target) < 5000)"`

  - **has_contact_with(*polity*)**
    Attribute function that returns *true* if any of the faction's groups overlaps with the territory of the *polity* entity, or is neighbor with any group that overlaps that same territory. Otherwise returns *false*.
    *Example:* `"dominant_faction.has_contact_with(other_polity)"`

  - **change_polity(*polity*)**
    Attribute function that changes the polity this faction belongs to.
    *Example:* `"faction.change_polity(other_polity)"`

  - **preferences**
    Attribute that returns a non-modifiable **container** entity of all the faction's cultural preferences, whose **numeric** values can later be read from or written to.
    *Example:* `"target.preferences.authority <= 0.5"`, `"target.preferences.agression = 0.3"`

  - **skills**
    Attribute that returns a non-modifiable **container** entity of all the faction's cultural skills, whose **numeric** values can later be read from.
    *Example:* `"target.skills.seafaring <= 0.5"`

  - **activities**
    Attribute that returns a non-modifiable **container** entity of all the faction's cultural activities, whose **numeric** values can later be read from.
    *Example:* `"target.activities.foraging <= 0.4"`

  - **knowledges**
    Attribute that returns a non-modifiable **container** entity of all the faction's cultural knowledges, whose **numeric** values can later be read from.
    *Example:* `"target.knowledges.social_organization <= 0.6"`

  - **discoveries**
    Attribute that returns a non-modifiable **container** entity of all the faction's cultural discoveries.
    *Example:* `"target.discoveries.contains(sailing)"`


### REGION

  This type of entity encapsulates a world region. Currently it has no public attributes.


### CONTACT

  This type of entity encapsulates relationship information between two polities.

  ***Attributes:***

  - **polity**
    Attribute that returns the **polity entity** that this contact encapsulates relationship information for.
    *Examples:* `"contact.polity"`, `"polity.get_random_contact().polity"`

  - **strength**
    Attribute that returns the contact strength between the polity that contained this contact entity and the polity contained in the contact. Contact strength will be greater than `0` if both polities share a border with each other.
    *Examples:* `"contact.strength"`, `"target_polity.get_contact(polity).strength"`


### AGENT

  This type of entity encapsulates any individual that is or was present within the simulation. That includes faction leaders.

  ***Attributes:***

  - **wisdom**
    Attribute that returns an agent's *wisdom* level as a value between 0 and 1.
    *Example:* `"0.3 >= current_leader.wisdom"`

  - **charisma**
    Attribute that returns an agent's *charisma* level as a value between 0 and 1.
    *Example:* `"leader.charisma < 0.5"`


### KNOWLEDGE

  This type of entity encapsulates a cultural knowledge.

  ***Attributes:***

  - **limit**
    Attribute that gets or sets a knowledge limit level. The limit defines the maximum level a knowledge can reach. NOTE: Limits can only be set for knowledges at the **group** entity's level.
    *Example:* `"group.knowledges.shipbuilding.limit += 5"`


### COLLECTION

  This type of entity encapsulates a collection of items, which could be other entities.

  ***Attributes:***

  - **count**
    Attribute that returns the number of items currently within the collection.
    *Example:* `"polity.contacts.count > 0"`

  - **request_selection(*description*)**
    Attribute function that will ask the player to select an item within the collection. The *description* parameter should be a *text* string that will be presented to the player to instruct them on the action they must perform. NOTE: Some collections might not support this attribute.
    *Example:* `"neighbor_regions.request_selection(''Select the region the <<target.polity>> tribe should expand to...'')"`

  - **select_random()**
    Attribute function that will return a randomly picked item within the collection.
    *Example:* `"near_factions.select_random()"`

  - **select\[ *item* \](*selection_expression*)**
    Attribute function that will return the first item in the collection that satisfies the **boolean** expression defined in *selection_expression*. The expression might use the string value given in *item* as a placeholder keyword for each one of the items in the collection. The function will replace the *item* keyword with an actual item each time the expression is evaluated.
    *Example:* `"target.polity.contacts.select[contact](contact.polity == attempting_polity).strength"`

  - **select_subset\[ *item* \](*selection_expression*)**
    Attribute function that will return a **collection** entity with all the items within the original collection that satisfy the **boolean** expression defined in *selection_expression*. The expression might use the string value given in *item* as a placeholder keyword for each one of the items in the collection. The function will replace the *item* keyword with an actual item each time the expression is evaluated.
    *Example:* `"target.present_polities.select_subset[polity](polity.type == tribe)"`

  - **select_best\[ *item_a*, *item_b* \](*comparison_expression*)**
    Attribute function that will return the best item in the collection according to the criteria defined by the **boolean** expression defined in *comparison_expression*. The expression might use the string values given in *item_a* and *item_b* as a placeholder keywords for pairs of items in the collection. The function will replace the *item_\** keywords with actual items each time the expression is evaluated. The expression must return *true* if *item_a* is better than *item_b*, or return *false* otherwise.
    *Example:* `"polity.contacts.select_best[contact_a,contact_b](contact_a.strength >= contact_b.strength)"`

  - **sum\[ *item* \](*numeric_expression*)**
    Attribute function that will return a **numeric** value that is the sum of the results given by *numeric_expression* as evaluated for each item in the collection. The expression might use the string value given in *item* as a placeholder keyword for each one of the items in the collection. The function will replace the *item* keyword with an actual item each time the expression is evaluated.
    *Example:* `"target.cell.neighbors.sum[cell](cell.biome_type_presence(water))"`


### CONTAINER

  This type of entity encapsulates a simple container of items. Some containers can be modified (add or remove items) while others are stricly read-only. NOTE: Although similar in concept, a **container** entity is functionaly very different from a **collection** entity and shouldn't be confused with it.

  ***Attributes:***

  - **contains(*item_identifier*)**
    Attribute function that returns *true* if the container contains the item idenfied by *item_identifier*, otherwise returns *false*.
    *Example:* `"target.discoveries.contains(boat_making)"`

  - **add(*item_identifier*,*...*)**
    Attribute function that adds the item idenfied by *item_identifier* to the container entity. The function can receive additional parameters in *...* to set the initial value for the given item. NOTE: This function is only available in modifiable container entities.
    *Example:* `"target.knowledges.add(shipbuilding, 1)"`

  - **remove(*item_identifier*)**
    Attribute function that removes the item idenfied by *item_identifier* from the container entity. NOTE: This function is only available in modifiable container entities.
    *Example:* `"target.knowledges.remove(shipbuilding)"`

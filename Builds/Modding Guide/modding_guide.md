# Creating a new mod for *Worlds - History Simulator*

The first step to create a a new mod is to create a folder within the *Mods* folder. The folder's name will be considered the mod's name within the game, and there's no restriction for how a mod should be named as long as it is a valid folder name.

Each mod root folder must contain a *version.json* file that defines the earliest game version the mod is intended for. Please read the *mod_version_guide.md* in the *Guides* folder for more details on how to write a version file for your mod.

If the game fails to find a *version.json* file, then it will assume the mod is intended for version 0.3.3 of *Worlds*, which is the earliest game version to support mods.

Additionally, a mod must contain at least one component folder containing mod definition files. Depending the type of mod file, each file must be placed within any of the following component folders.

- Adjectives
- Biomes
- Actions
- Decisions
- Discoveries
- Elements
- Events
- Layers
- RegionAttributes
- Preferences

Each folder can contain one or more JSON definition files. These files can have any name as long as they have the *.json* extension. Although it is important is that they follow the definition structure required by the type of mod, and that they are located in the correct folder. If a specific folder would not have any files on it, then the folder doesn't need to be present within the mod.

The game can load more than one mod at a time. It will load them in alphabetical order. So make sure that mods that are dependent on other mods are loaded in the correct order.

Here's a quick preview of each supported mod component:

### Adjective Mods

Adjective definitions are used to define the possible adjectives used in language generation. You can find more information on how to write an adjective mod within the *adjective_modding_guide.md* file located in the *Guides* folder.

### Biome Mods

Biome definitions are used to define the biomes assigned to each cell on a newly generated planet. You can find more information on how to write a biome mod within the *biome_modding_guide.md* file located in the *Guides* folder.

### Action Mods

Action definitions are used to define the possible action that player led factions can use during a game. You can find more information on how to write a discovery mod within the *action_modding_guide.md* file located in the *Guides* folder.

### Decision Mods

Decision definitions are used to define the possible decisions that player and non-player led factions can be confronted with during a game. You can find more information on how to write a discovery mod within the *decision_modding_guide.md* file located in the *Guides* folder.

### Discovery Mods

Discovery definitions are used to define the possible discoveries that can be made by humans across a world's history. You can find more information on how to write a discovery mod within the *discovery_modding_guide.md* file located in the *Guides* folder.

### Element Mods

Element definitions are used to define the types of associations between elements and regions. You can find more information on how to write a element mod within the *element_modding_guide.md* file located in the *Guides* folder.

### Event Mods

Event definitions are used to define group and faction events that can occur during the game. You can find more information on how to write a element mod within the *element_modding_guide.md* file located in the *Guides* folder.

### Layer Mods

Layer definitions are used to define the layers assigned to each cell on a newly generated planet. You can find more information on how to write a layer mod within the *layer_modding_guide.md* file located in the *Guides* folder.

### Region Attribute Mods

Region Attribute definitions are used to define the attributes of a region. You can find more information on how to write a region attribute mod within the *region_attribute_modding_guide.md* file located in the *Guides* folder.

### Cultural Preference Mods

Cultural Preference definitions are used to define the preferences that cultures can acquire. You can find more information on how to write a cultural preference mod within the *cultural_preferences_modding_guide.md* file located in the *Guides* folder.

# Region Constraints Guide

Region Constraints are rules that are used by region attributes and elements to decide whether or not to be assigned or associated with a particular region. When a new region is created, every attribute and every element tests their constraints against the region. If any of the constraints assigned to the element or attribute fail, then the element or attribute is ignored.

Here's a list of the current types of region constraints (more to be added in future versions) and how they work:

- `coast_percentage_above`
    Tests if the amount of *sea* cells surrounding a particular region is above (or equal) a certain percentage (expressed as a number between 0.0 and 1.0 inclusive). Example: `coast_percentage_above:0.65`

- `coast_percentage_below`
    Tests if the amount of *sea* cells surrounding a particular region is below a certain percentage (expressed as a number between 0.0 and 1.0 inclusive). Example: `coast_percentage_below:1.0`

- `altitude_above`
    Tests if the average altitude of a region is above (or equal) a certain altitude (expressed in meters). Example: `altitude_above:0`

- `altitude_below`
    Tests if the average altitude of a region is below a certain altitude (expressed in meters). Example: `altitude_below:1000`

- `relative_altitude_above`
    Tests if the average altitude of a region is above (or equal) a certain altitude relative the surrounding borders (expressed in meters). Example: `relative_altitude_above:200`

- `relative_altitude_below`
    Tests if the average altitude of a region is below a certain altitude relative the surrounding borders (expressed in meters). Example: `relative_altitude_below:-200`

- `rainfall_above`
    Tests if the average yearly rainfall of a region is above (or equal) a certain value (expressed in millimeters per year). Example: `rainfall_above:675`

- `rainfall_below`
    Tests if the average yearly rainfall of a region is below a certain value (expressed in millimeters per year). Example: `rainfall_below:1775`

- `temperature_above`
    Tests if the average temperature of a region is above (or equal) a certain value (expressed in centigrade). Example: `temperature_above:-15`

- `temperature_below`
    Tests if the average temperature of a region is below a certain value (expressed in centigrade). Example: `temperature_below:10`

- `flowing_water_above`
    Tests is the average amount of flowing water within a region is above a certain value (expressed as  a number between 0.0 and 100000000.0 inclusive). Example: `flowing_water_above:100.0`

- `flowing_water_below`
    Tests is the average amount of flowing water within a region is below a certain value (expressed as  a number between 0.0 and 100000000.0 inclusive). Example: `flowing_water_below:10000.0`

- `biome_presence_above`
    Tests if the number of cells within a region that have a particular biome is above (or equal) a certain percentage (expressed as comma separated pair of a biome identifier (id) and a number between 0.0 and 1.0 inclusive). Example: `biome_presence_above:ice_sheet,0.65`

- `biome_presence_below`
    Tests if the number of cells within a region that have a particular biome is below a certain percentage (expressed as comma separated pair of a biome identifier (id) and a number between 0.0 and 1.0 inclusive). Example: `biome_presence_below:desert,0.50`

- `layer_value_above`
    Tests if the number of cells within a region that have a particular layer is above (or equal) a certain value (expressed as comma separated pair of a layer identifier (id) and a number between 0.0 and 1000000.0 inclusive). Example: `layer_value_above:<layer_id>,20.0`

- `layer_value_below`
    Tests if the number of cells within a region that have a particular layer is below a certain value (expressed as comma separated pair of a biome layer (id) and a number between 0.0 and 1000000.0 inclusive). Example: `layer_value_below:<layer_id>,100.0`

- `main_biome`
    Tests if any of the biomes within the given list is the biome with the most presence within the region (expressed as comma separated list of biome ids). Example: `main_biome:forest,taiga,tundra,rainforest`

- `any_attribute`
    Tests if any attribute within the given list is assigned to the region (expressed as comma separated list of strings). Example: `any_attribute:desert,delta,peninsula,island,coast`

- `no_attribute`
    Tests if no attribute within the given list is assigned to the region (expressed as comma separated list of strings). Example: `no_attribute:rainforest,jungle,taiga,forest`

- `zero_primary_attributes`
    Tests if no attribute has been assigned to the region. Example: `zero_primary_attributes`
    NOTE: This constraint should only be used with secondary attributes

# Layer Modding guide

Layer scripting mod files are located within the *Layers* folder. To be valid, mod files must have the .json extension and have the following file structure:

#### File Structure
Note: *.json* files do not support comments. Remove texts enclosed within double dashes `--`

```
{
  "layers": [ -- list of layers --
    {
      "id":                         -- (required) Unique layer identifier, if more than one layer definition share ids, only the last one loaded will be used
      "name":                       -- (required) String to be used by the game's UI. Please avoid using overlong strings or non-unicode characters
      "units":                      -- (optional) String to be used to mark the units used to measure this value (preferably no longer than 5 characters)
      "color":                      -- (required) HTML color code (#RRGGBB) to use to display layer on map (transparency values, if added, will be ignored)

      "noiseScale":                 -- (required) This affects the size of the layer splotches. Larger values will generate larger splotches (Value between 0.01 and 2)
      "secondaryNoiseInfluence":    -- (optional) This substracts lower scale noise of the layer splotches. A value of 0 means no secondary noise influence effect (Value between 0 and 1)
      "maxPossibleValue":           -- (required) What is the maximum possible value this layer can have on a cell (Value between 1 and 1000000)
      "frequency":                  -- (required) What is the probability if this layer being present at all on a cell (Value between 0.01 and 1)

      "minAltitude":                -- (optional) Minimum cell altitude at which this layer can be present, a value of '0' refers to sea level (meters)
      "maxAltitude":                -- (optional) Maximum cell altitude at which this layer can be present, a value of '0' refers to sea level (meters)
      "altitudeSaturationSlope":    -- (optional) Slope with which the layer will reach max saturation within the cell (between 0.001 and 1000 inclusive) (see note #4)
      "minRainfall":                -- (optional) Minimum cell rainfall at which this layer can be present (mm per year)
      "maxRainfall":                -- (optional) Maximum cell rainfall at which this layer can be present (mm per year)
      "minFlowingWater":            -- (optional) Minimum cell non-sea flowing water accumulation at which this layer can be present (mm per m^2)
      "maxFlowingWater":            -- (optional) Maximum cell non-sea flowing water accumulation at which this layer can be present (mm per m^2)
      "waterSaturationSlope":       -- (optional) Slope with which the layer will reach max rainfall or moisture saturation within the cell (between 0.001 and 1000 inclusive) (see note #7)
      "minTemperature":             -- (optional) Minimum cell temperature at which this layer can be present (centigrade, yearly average)
      "maxTemperature":             -- (optional) Maximum cell temperature at which this layer can be present (centigrade, yearly average)
      "temperatureSaturationSlope": -- (optional) Slope with which the layer will reach max saturation within the cell (between 0.001 and 1000 inclusive) (see note #4)
    },
    ... -- additional layers --
  ]
}
```

## Notes
1. Remove any trailing commas or the file won't be parsed
2. Optional `min` or `max` attribute values (`altitude`, `rainfall`, `temperature`) not defined will be assigned the minimum or maximum attribute value of its category, respectively
3. *Saturation* represents how close is particular biome from filling a particular cell and it helps calculate the relative strength of a particular biome in regards to others within a single cell. The saturation slope indicates how quickly a particular biome reaches its saturation point within a cell. A saturation slope of `0` indicates that the biome never increases its saturation level above 0. A  slope of `1` indicates that the biome reaches its maximum saturation at the exact midpoint between the minimum and maximum values of a particular property. A slope greater than `1` indicates the biome reaches its maximum saturation faster. A slope less than `1` indicates the biome won't reach its saturation point. When undefined, the default saturation slope is `1`


--

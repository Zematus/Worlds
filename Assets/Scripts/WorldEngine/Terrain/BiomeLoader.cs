using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class BiomeLoader
{
#pragma warning disable 0649

    public LoadedBiome[] biomes;

    [Serializable]
    public class LoadedBiome
    {
        public LoadedLayerConstraint[] layerConstraints;

        [Serializable]
        public class LoadedLayerConstraint
        {
            public string layerId;
            public string minValue;
            public string maxValue;
            public string saturationSlope;
        }

        public string id;
        public string name;
        public string color;
        public string type;
        public string[] traits;
        public string minAltitude;
        public string maxAltitude;
        public string altitudeSaturationSlope;
        public string minRainfall;
        public string maxRainfall;
        public string minFlowingWater;
        public string maxFlowingWater;
        public string waterSaturationSlope;
        public string minTemperature;
        public string maxTemperature;
        public string temperatureSaturationSlope;
        public float survivability;
        public float foragingCapacity;
        public float accessibility;
        public float arability;
    }

#pragma warning restore 0649

    public static IEnumerable<Biome> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        BiomeLoader loader = JsonUtility.FromJson<BiomeLoader>(jsonStr);

        for (int i = 0; i < loader.biomes.Length; i++)
        {
            yield return CreateBiome(loader.biomes[i]);
        }
    }

    private static Biome CreateBiome(LoadedBiome b)
    {
        if (string.IsNullOrEmpty(b.id))
        {
            throw new ArgumentException("biome id can't be null or empty");
        }

        if (string.IsNullOrEmpty(b.name))
        {
            throw new ArgumentException("biome name can't be null or empty");
        }

        if (string.IsNullOrEmpty(b.color))
        {
            throw new ArgumentException("biome color can't be null or empty");
        }

        if (string.IsNullOrEmpty(b.type))
        {
            throw new ArgumentException("biome type can't be null or empty");
        }

        if (!b.survivability.IsInsideRange(0, 1))
        {
            throw new ArgumentException("biome survibability must be a value between 0 and 1 (inclusive)");
        }

        if (!b.foragingCapacity.IsInsideRange(0, 1))
        {
            throw new ArgumentException("biome foraging capacity must be a value between 0 and 1 (inclusive)");
        }

        if (!b.accessibility.IsInsideRange(0, 1))
        {
            throw new ArgumentException("biome accessibility must be a value between 0 and 1 (inclusive)");
        }

        if (!b.arability.IsInsideRange(0, 1))
        {
            throw new ArgumentException("biome arability must be a value between 0 and 1 (inclusive)");
        }

        Biome biome = new Biome()
        {
            Id = b.id,
            IdHash = b.id.GetHashCode(),
            Name = b.name,
            Survivability = b.survivability,
            ForagingCapacity = b.foragingCapacity,
            Accessibility = b.accessibility,
            Arability = b.arability
        };

        switch (b.type)
        {
            case "land":
                biome.TerrainType = BiomeTerrainType.Land;
                break;
            case "water":
                biome.TerrainType = BiomeTerrainType.Water;
                break;
            case "ice":
                biome.TerrainType = BiomeTerrainType.Ice;
                break;
            default:
                throw new ArgumentException("Unknown biome terrain type: " + b.type);
        }

        if (b.traits != null)
        {
            foreach (string trait in b.traits)
            {
                string traitId = trait.Trim().ToLower();

                biome.Traits.Add(traitId);
                Biome.AllTraits.Add(traitId);
            }
        }

        if (!Manager.EnqueueTaskAndWait(() => ColorUtility.TryParseHtmlString(b.color, out biome.Color)))
        {
            throw new ArgumentException("Invalid color value: " + b.color);
        }
        biome.Color.a = 1;

        if (b.maxAltitude != null)
        {
            if (!float.TryParse(b.maxAltitude, out biome.MaxAltitude))
            {
                throw new ArgumentException("Invalid maxAltitude value: " + b.maxAltitude);
            }

            if (!biome.MaxAltitude.IsInsideRange(Biome.MinBiomeAltitude, Biome.MaxBiomeAltitude))
            {
                throw new ArgumentException("maxAltitude must be a value between " + Biome.MinBiomeAltitude + " and " + Biome.MaxBiomeAltitude);
            }
        }
        else
        {
            biome.MaxAltitude = Biome.MaxBiomeAltitude;
        }

        if (b.minAltitude != null)
        {
            if (!float.TryParse(b.minAltitude, out biome.MinAltitude))
            {
                throw new ArgumentException("Invalid minAltitude value: " + b.minAltitude);
            }

            if (!biome.MinAltitude.IsInsideRange(Biome.MinBiomeAltitude, Biome.MaxBiomeAltitude))
            {
                throw new ArgumentException("minAltitude must be a value between " + Biome.MinBiomeAltitude + " and " + Biome.MaxBiomeAltitude);
            }
        }
        else
        {
            biome.MinAltitude = Biome.MinBiomeAltitude;
        }

        if (b.altitudeSaturationSlope != null)
        {
            if (!float.TryParse(b.altitudeSaturationSlope, out biome.AltSaturationSlope))
            {
                throw new ArgumentException("Invalid altitudeSaturationSlope value: " + b.altitudeSaturationSlope);
            }

            if (!biome.AltSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("altitudeSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            biome.AltSaturationSlope = 1;
        }

        if (b.maxRainfall != null)
        {
            if (!float.TryParse(b.maxRainfall, out biome.MaxRainfall))
            {
                throw new ArgumentException("Invalid maxRainfall value: " + b.maxRainfall);
            }

            if (!biome.MaxRainfall.IsInsideRange(Biome.MinBiomeRainfall, Biome.MaxBiomeRainfall))
            {
                throw new ArgumentException("maxRainfall must be a value between " + Biome.MinBiomeRainfall + " and " + Biome.MaxBiomeRainfall);
            }
        }
        else
        {
            biome.MaxRainfall = Biome.MaxBiomeRainfall;
        }

        if (b.minRainfall != null)
        {
            if (!float.TryParse(b.minRainfall, out biome.MinRainfall))
            {
                throw new ArgumentException("Invalid minRainfall value: " + b.minRainfall);
            }

            if (!biome.MinRainfall.IsInsideRange(Biome.MinBiomeRainfall, Biome.MaxBiomeRainfall))
            {
                throw new ArgumentException("minRainfall must be a value between " + Biome.MinBiomeRainfall + " and " + Biome.MaxBiomeRainfall);
            }
        }
        else
        {
            biome.MinRainfall = Biome.MinBiomeRainfall;
        }

        if (b.maxFlowingWater != null)
        {
            if (!float.TryParse(b.maxFlowingWater, out biome.MaxFlowingWater))
            {
                throw new ArgumentException("Invalid minFlowingWater value: " + b.maxFlowingWater);
            }

            if (!biome.MaxRainfall.IsInsideRange(Biome.MinBiomeFlowingWater, Biome.MaxBiomeFlowingWater))
            {
                throw new ArgumentException("minFlowingWater must be a value between " + Biome.MinBiomeFlowingWater + " and " + Biome.MaxBiomeFlowingWater);
            }
        }
        else
        {
            biome.MaxFlowingWater = Biome.MaxBiomeFlowingWater;
        }

        if (b.minFlowingWater != null)
        {
            if (!float.TryParse(b.minFlowingWater, out biome.MinFlowingWater))
            {
                throw new ArgumentException("Invalid minFlowingWater value: " + b.minFlowingWater);
            }

            if (!biome.MinRainfall.IsInsideRange(Biome.MinBiomeFlowingWater, Biome.MaxBiomeFlowingWater))
            {
                throw new ArgumentException("minFlowingWater must be a value between " + Biome.MinBiomeFlowingWater + " and " + Biome.MaxBiomeFlowingWater);
            }
        }
        else
        {
            biome.MinFlowingWater = Biome.MinBiomeFlowingWater;
        }

        if (b.waterSaturationSlope != null)
        {
            if (!float.TryParse(b.waterSaturationSlope, out biome.WaterSaturationSlope))
            {
                throw new ArgumentException("Invalid waterSaturationSlope value: " + b.waterSaturationSlope);
            }

            if (!biome.WaterSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("waterSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            biome.WaterSaturationSlope = 1;
        }

        if (b.maxTemperature != null)
        {
            if (!float.TryParse(b.maxTemperature, out biome.MaxTemperature))
            {
                throw new ArgumentException("Invalid maxTemperature value: " + b.maxTemperature);
            }

            if (!biome.MaxTemperature.IsInsideRange(Biome.MinBiomeTemperature, Biome.MaxBiomeTemperature))
            {
                throw new ArgumentException("maxTemperature must be a value between " + Biome.MinBiomeTemperature + " and " + Biome.MaxBiomeTemperature);
            }
        }
        else
        {
            biome.MaxTemperature = Biome.MaxBiomeTemperature;
        }

        if (b.minTemperature != null)
        {
            if (!float.TryParse(b.minTemperature, out biome.MinTemperature))
            {
                throw new ArgumentException("Invalid minTemperature value: " + b.minTemperature);
            }

            if (!biome.MinTemperature.IsInsideRange(Biome.MinBiomeTemperature, Biome.MaxBiomeTemperature))
            {
                throw new ArgumentException("minTemperature must be a value between " + Biome.MinBiomeTemperature + " and " + Biome.MaxBiomeTemperature);
            }
        }
        else
        {
            biome.MinTemperature = Biome.MinBiomeTemperature;
        }

        if (b.temperatureSaturationSlope != null)
        {
            if (!float.TryParse(b.temperatureSaturationSlope, out biome.TempSaturationSlope))
            {
                throw new ArgumentException("Invalid temperatureSaturationSlope value: " + b.temperatureSaturationSlope);
            }

            if (!biome.TempSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("temperatureSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            biome.TempSaturationSlope = 1;
        }

        if (b.layerConstraints != null)
        {
            biome.LayerConstraints = new Dictionary<string, Biome.LayerConstraint>(b.layerConstraints.Length);

            for (int i = 0; i < b.layerConstraints.Length; i++)
            {
                Biome.LayerConstraint constraint = CreateLayerConstraint(b.layerConstraints[i]);

                biome.LayerConstraints.Add(constraint.LayerId, constraint);
            }
        }

        return biome;
    }

    private static Biome.LayerConstraint CreateLayerConstraint(LoadedBiome.LoadedLayerConstraint c)
    {
        if (string.IsNullOrEmpty(c.layerId))
        {
            throw new ArgumentException("constraint's layerId can't be null or empty");
        }

        Biome.LayerConstraint constraint = new Biome.LayerConstraint()
        {
            LayerId = c.layerId
        };

        if (c.maxValue != null)
        {
            if (!float.TryParse(c.maxValue, out constraint.MaxValue))
            {
                throw new ArgumentException("Invalid constraint maxValue value: " + c.maxValue);
            }

            if (!constraint.MaxValue.IsInsideRange(Layer.MinLayerPossibleValue, Layer.MaxLayerPossibleValue))
            {
                throw new ArgumentException("maxValue must be a value between " + Layer.MinLayerPossibleValue + " and " + Layer.MaxLayerPossibleValue);
            }
        }
        else
        {
            constraint.MaxValue = Layer.MaxLayerPossibleValue;
        }

        if (c.minValue != null)
        {
            if (!float.TryParse(c.minValue, out constraint.MinValue))
            {
                throw new ArgumentException("Invalid constraint minValue value: " + c.minValue);
            }

            if (!constraint.MinValue.IsInsideRange(Layer.MinLayerPossibleValue, Layer.MaxLayerPossibleValue))
            {
                throw new ArgumentException("minValue must be a value between " + Layer.MinLayerPossibleValue + " and " + Layer.MaxLayerPossibleValue);
            }
        }
        else
        {
            constraint.MinValue = Layer.MinLayerPossibleValue;
        }

        Layer layer = Layer.Layers[c.layerId];

        if (constraint.MaxValue > layer.MaxPossibleValue)
        {
            constraint.MaxValue = layer.MaxPossibleValue;
        }

        if (constraint.MinValue < -layer.MaxPossibleValue)
        {
            constraint.MinValue = -layer.MaxPossibleValue;
        }

        if (c.saturationSlope != null)
        {
            if (!float.TryParse(c.saturationSlope, out constraint.SaturationSlope))
            {
                throw new ArgumentException("Invalid constraint saturationSlope value: " + c.saturationSlope);
            }

            if (!constraint.SaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("constraint saturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            constraint.SaturationSlope = 1;
        }

        return constraint;
    }
}

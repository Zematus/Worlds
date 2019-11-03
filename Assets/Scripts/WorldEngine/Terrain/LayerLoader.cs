using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class LayerLoader
{
#pragma warning disable 0649

    public LoadedLayer[] layers;

    [Serializable]
    public class LoadedLayer
    {
        public string id;
        public string name;
        public string units;
        public string color;
        public float noiseScale;
        public string secondaryNoiseInfluence;
        public float maxPossibleValue;
        public float frequency;
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
    }

#pragma warning restore 0649

    public static IEnumerable<Layer> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);
        
        LayerLoader loader = JsonUtility.FromJson<LayerLoader>(jsonStr);
        
        for (int i = 0; i < loader.layers.Length; i++)
        {
            yield return CreateLayer(loader.layers[i]);
        }
    }

    private static Layer CreateLayer(LoadedLayer l)
    {
        if (string.IsNullOrEmpty(l.id))
        {
            throw new ArgumentException("layer id can't be null or empty");
        }

        if (string.IsNullOrEmpty(l.name))
        {
            throw new ArgumentException("layer name can't be null or empty");
        }

        if (!l.noiseScale.IsInsideRange(0.01f, 2))
        {
            throw new ArgumentException("layer noise scale must be a value between 0.01 and 1 (inclusive)");
        }

        if (!l.maxPossibleValue.IsInsideRange(1, Layer.MaxLayerPossibleValue))
        {
            throw new ArgumentException("layer max possible value must be between 1 and " + Layer.MaxLayerPossibleValue + " (inclusive)");
        }

        if (!l.frequency.IsInsideRange(0.01f, 1))
        {
            throw new ArgumentException("layer frequency must be a value between 0.01 and 1 (inclusive)");
        }

        Layer layer = new Layer()
        {
            Id = l.id,
            Name = l.name,
            NoiseScale = l.noiseScale,
            MaxPossibleValue = l.maxPossibleValue,
            Frequency = l.frequency
        };

        if (l.units != null)
        {
            layer.Units = l.units;
        }
        else
        {
            layer.Units = string.Empty;
        }

        if (!Manager.EnqueueTaskAndWait(() => ColorUtility.TryParseHtmlString(l.color, out layer.Color)))
        {
            throw new ArgumentException("Invalid color value: " + l.color);
        }
        layer.Color.a = 1;

        if (l.secondaryNoiseInfluence != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.secondaryNoiseInfluence, out layer.SecondaryNoiseInfluence))
            {
                throw new ArgumentException("Invalid secondaryNoiseInfluence value: " + l.secondaryNoiseInfluence);
            }

            if (!layer.SecondaryNoiseInfluence.IsInsideRange(0, 1))
            {
                throw new ArgumentException("secondaryNoiseInfluence must be a value between 0 and 1");
            }
        }
        else
        {
            layer.SecondaryNoiseInfluence = 0;
        }

        if (l.maxAltitude != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.maxAltitude, out layer.MaxAltitude))
            {
                throw new ArgumentException("Invalid maxAltitude value: " + l.maxAltitude);
            }

            if (!layer.MaxAltitude.IsInsideRange(Layer.MinLayerAltitude, Layer.MaxLayerAltitude))
            {
                throw new ArgumentException("maxAltitude must be a value between " + Layer.MinLayerAltitude + " and " + Layer.MaxLayerAltitude);
            }
        }
        else
        {
            layer.MaxAltitude = Biome.MaxBiomeAltitude;
        }

        if (l.minAltitude != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.minAltitude, out layer.MinAltitude))
            {
                throw new ArgumentException("Invalid minAltitude value: " + l.minAltitude);
            }

            if (!layer.MinAltitude.IsInsideRange(Layer.MinLayerAltitude, Layer.MaxLayerAltitude))
            {
                throw new ArgumentException("minAltitude must be a value between " + Layer.MinLayerAltitude + " and " + Layer.MaxLayerAltitude);
            }
        }
        else
        {
            layer.MinAltitude = Biome.MinBiomeAltitude;
        }

        if (l.altitudeSaturationSlope != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.altitudeSaturationSlope, out layer.AltSaturationSlope))
            {
                throw new ArgumentException("Invalid altitudeSaturationSlope value: " + l.altitudeSaturationSlope);
            }

            if (!layer.AltSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("altitudeSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            layer.AltSaturationSlope = 1;
        }

        if (l.maxRainfall != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.maxRainfall, out layer.MaxRainfall))
            {
                throw new ArgumentException("Invalid maxRainfall value: " + l.maxRainfall);
            }

            if (!layer.MaxRainfall.IsInsideRange(Layer.MinLayerRainfall, Layer.MaxLayerRainfall))
            {
                throw new ArgumentException("maxRainfall must be a value between " + Layer.MinLayerRainfall + " and " + Layer.MaxLayerRainfall);
            }
        }
        else
        {
            layer.MaxRainfall = Layer.MaxLayerRainfall;
        }

        if (l.minRainfall != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.minRainfall, out layer.MinRainfall))
            {
                throw new ArgumentException("Invalid minRainfall value: " + l.minRainfall);
            }

            if (!layer.MinRainfall.IsInsideRange(Layer.MinLayerRainfall, Layer.MaxLayerRainfall))
            {
                throw new ArgumentException("minRainfall must be a value between " + Layer.MinLayerRainfall + " and " + Layer.MaxLayerRainfall);
            }
        }
        else
        {
            layer.MinRainfall = Layer.MinLayerRainfall;
        }

        if (l.maxFlowingWater != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.maxFlowingWater, out layer.MaxFlowingWater))
            {
                throw new ArgumentException("Invalid minFlowingWater value: " + l.maxFlowingWater);
            }

            if (!layer.MaxRainfall.IsInsideRange(Layer.MinLayerFlowingWater, Layer.MaxLayerFlowingWater))
            {
                throw new ArgumentException("minFlowingWater must be a value between " + Layer.MinLayerFlowingWater + " and " + Layer.MaxLayerFlowingWater);
            }
        }
        else
        {
            layer.MaxFlowingWater = Layer.MaxLayerFlowingWater;
        }

        if (l.minFlowingWater != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.minFlowingWater, out layer.MinFlowingWater))
            {
                throw new ArgumentException("Invalid minFlowingWater value: " + l.minFlowingWater);
            }

            if (!layer.MinRainfall.IsInsideRange(Layer.MinLayerFlowingWater, Layer.MaxLayerFlowingWater))
            {
                throw new ArgumentException("minFlowingWater must be a value between " + Layer.MinLayerFlowingWater + " and " + Layer.MaxLayerFlowingWater);
            }
        }
        else
        {
            layer.MinFlowingWater = Layer.MinLayerFlowingWater;
        }

        if (l.waterSaturationSlope != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.waterSaturationSlope, out layer.WaterSaturationSlope))
            {
                throw new ArgumentException("Invalid waterSaturationSlope value: " + l.waterSaturationSlope);
            }

            if (!layer.WaterSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("waterSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            layer.WaterSaturationSlope = 1;
        }

        if (l.maxTemperature != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.maxTemperature, out layer.MaxTemperature))
            {
                throw new ArgumentException("Invalid maxTemperature value: " + l.maxTemperature);
            }

            if (!layer.MaxTemperature.IsInsideRange(Layer.MinLayerTemperature, Layer.MaxLayerTemperature))
            {
                throw new ArgumentException("maxTemperature must be a value between " + Layer.MinLayerTemperature + " and " + Layer.MaxLayerTemperature);
            }
        }
        else
        {
            layer.MaxTemperature = Biome.MaxBiomeTemperature;
        }

        if (l.minTemperature != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.minTemperature, out layer.MinTemperature))
            {
                throw new ArgumentException("Invalid minTemperature value: " + l.minTemperature);
            }

            if (!layer.MinTemperature.IsInsideRange(Layer.MinLayerTemperature, Layer.MaxLayerTemperature))
            {
                throw new ArgumentException("minTemperature must be a value between " + Layer.MinLayerTemperature + " and " + Layer.MaxLayerTemperature);
            }
        }
        else
        {
            layer.MinTemperature = Biome.MinBiomeTemperature;
        }

        if (l.temperatureSaturationSlope != null)
        {
            if (!MathUtility.TryParseCultureInvariant(l.temperatureSaturationSlope, out layer.TempSaturationSlope))
            {
                throw new ArgumentException("Invalid temperatureSaturationSlope value: " + l.temperatureSaturationSlope);
            }

            if (!layer.TempSaturationSlope.IsInsideRange(0.001f, 1000))
            {
                throw new ArgumentException("temperatureSaturationSlope must be a value between 0.001 and 1000");
            }
        }
        else
        {
            layer.TempSaturationSlope = 1;
        }

        return layer;
    }
}

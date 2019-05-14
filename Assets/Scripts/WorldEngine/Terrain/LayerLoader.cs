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
        public string minRainfall;
        public string maxRainfall;
        public string minTemperature;
        public string maxTemperature;
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

        if (!l.maxPossibleValue.IsInsideRange(0.001f, 1000000))
        {
            throw new ArgumentException("layer max possible value must be between 0.001 and 1000000 (inclusive)");
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
            Frequency = l.frequency,
            Rarity = 1 - l.frequency
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
            if (!float.TryParse(l.secondaryNoiseInfluence, out layer.SecondaryNoiseInfluence))
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
            if (!float.TryParse(l.maxAltitude, out layer.MaxAltitude))
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
            if (!float.TryParse(l.minAltitude, out layer.MinAltitude))
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

        if (l.maxRainfall != null)
        {
            if (!float.TryParse(l.maxRainfall, out layer.MaxRainfall))
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
            layer.MaxRainfall = Biome.MaxBiomeRainfall;
        }

        if (l.minRainfall != null)
        {
            if (!float.TryParse(l.minRainfall, out layer.MinRainfall))
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
            layer.MinRainfall = Biome.MinBiomeRainfall;
        }

        if (l.maxTemperature != null)
        {
            if (!float.TryParse(l.maxTemperature, out layer.MaxTemperature))
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
            if (!float.TryParse(l.minTemperature, out layer.MinTemperature))
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

        return layer;
    }
}

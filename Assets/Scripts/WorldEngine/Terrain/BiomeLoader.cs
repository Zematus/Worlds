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
        public string id;
        public string name;
        public string color;
        public string minAltitude;
        public string maxAltitude;
        public string minRainfall;
        public string maxRainfall;
        public string minTemperature;
        public string maxTemperature;
        public float survivability;
        public float foragingCapacity;
        public float accessibility;
    }

#pragma warning restore 0649

    public static Biome[] Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);
        
        BiomeLoader loader = JsonUtility.FromJson<BiomeLoader>(jsonStr);

        int length = loader.biomes.Length;

        if (length == 0)
        {
            throw new Exception("No biomes loaded from " + filename);
        }

        Biome[] biomes = new Biome[length];

        for (int i = 0; i < length; i++)
        {
            biomes[i] = ParseBiome(loader.biomes[i]);
        }

        return biomes;
    }

    private static Biome ParseBiome(LoadedBiome b)
    {
        Biome biome = new Biome()
        {
            Id = b.id,
            Name = b.name,
            Survivability = b.survivability,
            ForagingCapacity = b.foragingCapacity,
            Accessibility = b.accessibility
        };

        if (!ColorUtility.TryParseHtmlString(b.color, out biome.Color))
        {
            throw new Exception("Invalid color value: " + b.color);
        }
        biome.Color.a = 1;

        if (b.maxAltitude != null)
        {
            if (!float.TryParse(b.maxAltitude, out biome.MaxAltitude))
            {
                throw new Exception("Invalid maxAltitude value: " + b.maxAltitude);
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
                throw new Exception("Invalid minAltitude value: " + b.minAltitude);
            }
        }
        else
        {
            biome.MinAltitude = Biome.MinBiomeAltitude;
        }

        if (b.maxRainfall != null)
        {
            if (!float.TryParse(b.maxRainfall, out biome.MaxRainfall))
            {
                throw new Exception("Invalid maxRainfall value: " + b.maxRainfall);
            }
        }
        else
        {
            biome.MaxRainfall = Biome.MinBiomeRainfall;
        }

        if (b.minRainfall != null)
        {
            if (!float.TryParse(b.minRainfall, out biome.MinRainfall))
            {
                throw new Exception("Invalid minRainfall value: " + b.minRainfall);
            }
        }
        else
        {
            biome.MinRainfall = Biome.MinBiomeRainfall;
        }

        if (b.maxTemperature != null)
        {
            if (!float.TryParse(b.maxTemperature, out biome.MaxTemperature))
            {
                throw new Exception("Invalid maxTemperature value: " + b.maxTemperature);
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
                throw new Exception("Invalid minTemperature value: " + b.minTemperature);
            }
        }
        else
        {
            biome.MinTemperature = Biome.MinBiomeTemperature;
        }

        return biome;
    }
}

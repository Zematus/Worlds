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
        public string type;
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

        Biome biome = new Biome()
        {
            Id = b.id,
            IdHash = b.id.GetHashCode(),
            Name = b.name,
            Survivability = b.survivability,
            ForagingCapacity = b.foragingCapacity,
            Accessibility = b.accessibility
        };

        switch (b.type)
        {
            case "land":
                biome.LocationType = Biome.Type.Land;
                break;
            case "sea":
                biome.LocationType = Biome.Type.Sea;
                break;
            default:
                throw new ArgumentException("Unknown biome location type: " + b.type);
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
        }
        else
        {
            biome.MinAltitude = Biome.MinBiomeAltitude;
        }

        if (b.maxRainfall != null)
        {
            if (!float.TryParse(b.maxRainfall, out biome.MaxRainfall))
            {
                throw new ArgumentException("Invalid maxRainfall value: " + b.maxRainfall);
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
        }
        else
        {
            biome.MinRainfall = Biome.MinBiomeRainfall;
        }

        if (b.maxTemperature != null)
        {
            if (!float.TryParse(b.maxTemperature, out biome.MaxTemperature))
            {
                throw new ArgumentException("Invalid maxTemperature value: " + b.maxTemperature);
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
        }
        else
        {
            biome.MinTemperature = Biome.MinBiomeTemperature;
        }

        return biome;
    }
}

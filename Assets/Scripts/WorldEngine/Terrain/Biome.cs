using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Biome
{
    public enum LocactionType
    {
        Land,
        Sea
    }

    public static float MinBiomeTemperature = World.MinPossibleTemperature * 3 - 1;
    public static float MaxBiomeTemperature = World.MaxPossibleTemperature * 3 + 1;

    public static float MinBiomeRainfall = World.MinPossibleRainfall * 3 - 1;
    public static float MaxBiomeRainfall = World.MaxPossibleRainfall * 3 + 1;

    public static float MinBiomeAltitude = World.MinPossibleAltitude * 3 - 1;
    public static float MaxBiomeAltitude = World.MaxPossibleAltitude * 3 + 1;

    public static Dictionary<string, Biome> Biomes;

    public string Name;
    public string Id;
    public int IdHash;

    public Color Color;
    public LocactionType Type;

    public float MinAltitude;
    public float MaxAltitude;

    public float MinRainfall;
    public float MaxRainfall;

    public float MinTemperature;
    public float MaxTemperature;

    public float Survivability;
    public float ForagingCapacity;
    public float Accessibility;

    public static void ResetBiomes()
    {
        Biomes = new Dictionary<string, Biome>();
    }

    public static void LoadBiomesFile(string filename)
    {
        foreach (Biome biome in BiomeLoader.Load(filename))
        {
            if (Biomes.ContainsKey(biome.Id))
            {
                Biomes[biome.Id] = biome;
            }
            else
            {
                Biomes.Add(biome.Id, biome);
            }
        }
    }
}

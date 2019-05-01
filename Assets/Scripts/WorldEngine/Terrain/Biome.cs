using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Biome
{
    public enum Type
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
    //public static Dictionary<string, Biome> Biomes = new Dictionary<string, Biome>()
    //{
    //    {"Glacier", Glacier},
    //    {"Ice Cap", IceCap},
    //    {"Ocean", Ocean},
    //    {"Grassland", Grassland},
    //    {"Forest", Forest},
    //    {"Taiga", Taiga},
    //    {"Tundra", Tundra},
    //    {"Desertic Tundra", DeserticTundra},
    //    {"Desert", Desert},
    //    {"Rainforest", Rainforest}
    //};

    public string Name;
    public string Id;
    public int IdHash;

    public Color Color;
    public Type LocationType;

    public float MinAltitude;
    public float MaxAltitude;

    public float MinRainfall;
    public float MaxRainfall;

    public float MinTemperature;
    public float MaxTemperature;

    public float Survivability;
    public float ForagingCapacity;
    public float Accessibility;

    public static void LoadBiomes(string filename)
    {
        Biomes = new Dictionary<string, Biome>();

        foreach (Biome biome in BiomeLoader.Load(filename))
        {
            if (Biomes.ContainsKey(biome.Id))
            {
                throw new System.Exception("duplicate biome id: " + biome.Id);
            }

            Biomes.Add(biome.Id, biome);
        }

        if (Biomes.Count == 0)
        {
            throw new System.Exception("No biomes loaded from " + filename);
        }
    }
}

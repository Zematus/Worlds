using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum BiomeTerrainType
{
    Land,
    Water
}

public enum BiomeTrait
{
    Wood,
    Sea,
    Lake,
    River
}

public class Biome
{
    public class LayerConstraint
    {
        public string LayerId;
        public float MinValue;
        public float MaxValue;
        public float SaturationSlope;
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
    public BiomeTerrainType TerrainType;
    public HashSet<BiomeTrait> Traits = new HashSet<BiomeTrait>();

    public float MinAltitude;
    public float MaxAltitude;
    public float AltSaturationSlope;

    public float MinRainfall;
    public float MaxRainfall;
    public float RainSaturationSlope;

    public float MinTemperature;
    public float MaxTemperature;
    public float TempSaturationSlope;

    public float Survivability;
    public float ForagingCapacity;
    public float Accessibility;
    public float Arability;

    public Dictionary<string, LayerConstraint> LayerConstraints = null;

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

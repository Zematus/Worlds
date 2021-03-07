using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum BiomeTerrainType
{
    Land,
    Water,
    Ice
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

    public static float MinBiomeFlowingWater = World.MinPossibleRainfall * 3 - 1;
    public static float MaxBiomeFlowingWater = World.MaxPossibleRainfall * 10000 + 1;

    public static float MinBiomeAltitude = World.MinPossibleAltitude * 3 - 1;
    public static float MaxBiomeAltitude = World.MaxPossibleAltitude * 3 + 1;

    public static float MaxLoadedIceBiomeTemperature = float.MinValue;
    public static float MinLoadedIceBiomeTemperature = float.MaxValue;
    public static float MaxLoadedIceBiomeRainfall = float.MinValue;
    public static float MinLoadedIceBiomeRainfall = float.MaxValue;
    public static float MaxLoadedIceBiomeAltitude = float.MinValue;
    public static float MinLoadedIceBiomeAltitude = float.MaxValue;

    public static Dictionary<string, Biome> Biomes = null; // Only initialize during mod reset

    public static HashSet<string> AllTraits = new HashSet<string>();

    public string Name;
    public string Id;
    public string SkillId;
    public string SkillName;
    public int IdHash;
    
    public Color Color = Color.black;
    public BiomeTerrainType TerrainType;
    public HashSet<string> Traits = new HashSet<string>();

    public float MinAltitude;
    public float MaxAltitude;
    public float AltSaturationSlope;

    public float MinRainfall;
    public float MaxRainfall;
    public float MinFlowingWater;
    public float MaxFlowingWater;
    public float WaterSaturationSlope;

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

            if (biome.TerrainType == BiomeTerrainType.Ice)
            {
                MaxLoadedIceBiomeTemperature = Mathf.Max(biome.MaxTemperature, MaxLoadedIceBiomeTemperature);
                MinLoadedIceBiomeTemperature = Mathf.Min(biome.MinTemperature, MinLoadedIceBiomeTemperature);
                MaxLoadedIceBiomeRainfall = Mathf.Max(biome.MaxRainfall, MaxLoadedIceBiomeRainfall);
                MinLoadedIceBiomeRainfall = Mathf.Min(biome.MinRainfall, MinLoadedIceBiomeRainfall);
                MaxLoadedIceBiomeAltitude = Mathf.Max(biome.MaxAltitude, MaxLoadedIceBiomeAltitude);
                MinLoadedIceBiomeAltitude = Mathf.Min(biome.MinAltitude, MinLoadedIceBiomeAltitude);
            }
        }
    }

    public static bool CellHasIce(TerrainCell cell)
    {
        if ((cell.Temperature > MaxLoadedIceBiomeTemperature) || (cell.Temperature < MinLoadedIceBiomeTemperature))
            return false;

        if ((cell.Rainfall > MaxLoadedIceBiomeRainfall) || (cell.Rainfall < MinLoadedIceBiomeRainfall))
            return false;

        if ((cell.Altitude > MaxLoadedIceBiomeAltitude) || (cell.Altitude < MinLoadedIceBiomeAltitude))
            return false;

        bool hasIce = false;
        foreach (Biome biome in Biomes.Values)
        {
            if (biome.TerrainType == BiomeTerrainType.Ice)
            {
                if ((cell.Temperature > biome.MaxTemperature) || (cell.Temperature < biome.MinTemperature))
                    continue;

                if ((cell.Rainfall > biome.MaxRainfall) || (cell.Rainfall < biome.MinRainfall))
                    continue;

                if ((cell.Altitude > biome.MaxAltitude) || (cell.Altitude < biome.MinAltitude))
                    continue;

                hasIce = true;
            }
        }

        return hasIce;
    }
}

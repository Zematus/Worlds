using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

public enum Direction
{
    Null = -1,
    North = 0,
    Northeast = 1,
    East = 2,
    Southeast = 3,
    South = 4,
    Southwest = 5,
    West = 6,
    Northwest = 7
}

[Flags]
public enum CellUpdateType
{
    None = 0x0,
    Cell = 0x1,
    Group = 0x2,
    Region = 0x4,
    Territory = 0x8,
    Route = 0x10,
    Cluster = 0x20,
    Language = 0x40,
    All = 0xFF,
    GroupTerritoryClusterAndLanguage = Group | Territory | Cluster | Language
}

[Flags]
public enum CellUpdateSubType
{
    None = 0x0,
    Culture = 0x1,
    Population = 0x2,
    Terrain = 0x4,
    Relationship = 0x8,
    Membership = 0x10,
    Core = 0x20,
    All = 0xFF,
    AllButTerrain = All & ~Terrain,
    PopulationAndCulture = Population | Culture,
    MembershipAndCore = Membership | Core
}

public class TerrainCell
{
#if DEBUG
    public delegate void GetNextLocalRandomCalledDelegate(string callerMethod);

    //	public static int LastRandomInteger = 0; 
    public static GetNextLocalRandomCalledDelegate GetNextLocalRandomCalled = null;
#endif

    public const int MaxNeighborDirections = 8;
    public const int NeighborSearchOffset = 3;

    public const int MaxNeighborhoodCellCount = 9;

    public const float HillinessSlopeFactor = 0.01f;

    public const float TemperatureHoldOffFactor = 0.35f;

    public bool Modified = false; // This will be true if the cell has been modified after/during generation by using a heighmap, using the map editor, or by running the simulation

    public int Longitude;
    public int Latitude;

    public float Height;
    public float Width;

    public float BaseAltitudeValue;
    public float BaseRainfallValue;
    public float BaseTemperatureValue;

    public float BaseRainfallOffset;
    public float BaseTemperatureOffset;

    public float OriginalAltitude;
    public float Altitude;
    public float Rainfall;
    public float Temperature;
    public float OriginalTemperature;

    public struct RiverBuffers
    {
        public float DrainageTransfer;
        public float TemperatureTransfer;
        public float DirectionFactor;
    }

    public Dictionary<TerrainCell, RiverBuffers> FeedingCells =
        new Dictionary<TerrainCell, RiverBuffers>();

    public int RiverId = -1;
    public float RiverLength = 0;
    public bool DrainageDone = true;
    public bool TerrainAlteredBeforeDrainageRegen = false;

    public float FlowingWater
    {
        get
        {
            return WaterAccumulation - Rainfall;
        }
    }

    public float WaterAccumulation
    {
        get
        {
            return _waterAccumulation;
        }
        set
        {
#if DEBUG
            if (value > Biome.MaxBiomeFlowingWater)
            {
                Debug.LogWarning("Water accumulation at " + Position + " above max supported maximum: " + value);
            }
#endif

            _waterAccumulation = Mathf.Min(Biome.MaxBiomeFlowingWater, value);
        }
    }

    public float Survivability;
    public float ForagingCapacity;
    public float BaseAccessibility;
    public float BaseArability;
    public float Hilliness;

    public bool IsPartOfCoastline;

    public float FarmlandPercentage = 0;
    public float Arability = 0;
    public float Accessibility = 0;

    public string BiomeWithMostPresence = null;
    public float MostBiomePresence = 0;

    public static float MaxArea;
    public static float MaxWidth;

    public List<string> PresentBiomeIds = new List<string>();
    public List<float> BiomePresences = new List<float>();
    public List<float> BiomeAbsPresences = new List<float>();

    public List<string> PresentLayerIds = new List<string>();
    public List<CellLayerData> LayerData = new List<CellLayerData>();

    public CellGroup Group;

    public float Alpha;
    public float Beta;

    public List<string> PresentWaterBiomeIds = new List<string>();

    public float WaterBiomePresence = 0;

    private float? _neighborhoodWaterBiomePresence = null;

    public float NeighborhoodWaterBiomePresence
    {
        get {
            if (_neighborhoodWaterBiomePresence == null)
            {
                _neighborhoodWaterBiomePresence = WaterBiomePresence;

                foreach (TerrainCell nCell in Neighbors.Values)
                {
                    _neighborhoodWaterBiomePresence += nCell.WaterBiomePresence;
                }
            }

            return _neighborhoodWaterBiomePresence.Value;
        }
    }

    public WorldPosition Position;

    public Region Region = null;

    public Territory EncompassingTerritory = null;

    public List<Route> CrossingRoutes = new List<Route>();

    public bool HasCrossingRoutes = false;

    public float Area;
    public float MaxAreaPercent;

    public World World;

    public bool IsSelected = false;

    public List<TerrainCell> RainfallDependentCells = new List<TerrainCell>();

    public Dictionary<Direction, TerrainCell> Neighbors { get; private set; }
    public HashSet<TerrainCell> NeighborSet { get; private set; }
    public Dictionary<Direction, float> NeighborDistances { get; private set; }

    private float _waterAccumulation = 0;

    private Dictionary<string, float> _biomePresences = new Dictionary<string, float>();
    private Dictionary<string, CellLayerData> _layerData = new Dictionary<string, CellLayerData>();

    public TerrainCell()
    {

    }

    public TerrainCell(World world, int longitude, int latitude, float height, float width)
    {
        Position = new WorldPosition(longitude, latitude);

        World = world;
        Longitude = longitude;
        Latitude = latitude;

        Alpha = (latitude / (float)world.Height) * Mathf.PI;
        Beta = (longitude / (float)world.Width) * Mathf.PI * 2;

        Height = height;
        Width = width;

        Area = height * width;
        MaxAreaPercent = Area / MaxArea;
    }

    public static int CompareOriginalAltitude(TerrainCell a, TerrainCell b)
    {
        if (a.OriginalAltitude > b.OriginalAltitude) return -1;
        if (a.OriginalAltitude < b.OriginalAltitude) return 1;

        return 0;
    }

    public static Direction ReverseDirection(Direction dir)
    {
        return (Direction)(((int)dir + 4) % 8);
    }

    public Direction GetDirection(TerrainCell targetCell)
    {
        foreach (KeyValuePair<Direction, TerrainCell> pair in Neighbors)
        {
            if (pair.Value == targetCell)
            {
                return pair.Key;
            }
        }

        return Direction.Null;
    }

    public static bool IsDiagonalDirection(Direction dir)
    {
        return ((dir == Direction.Northeast) ||
            (dir == Direction.Northwest) ||
            (dir == Direction.Southeast) ||
            (dir == Direction.Southwest));
    }

    public bool IsBelowSeaLevel => Altitude <= 0;

    public bool IsLiquidSea
    {
        get
        {
            if (!IsBelowSeaLevel) return false;

            return GetBiomeTypePresence(BiomeTerrainType.Water) >= 1;
        }
    }

    public IEnumerable<KeyValuePair<Direction, TerrainCell>> GetNonDiagonalNeighbors()
    {
        foreach (KeyValuePair<Direction, TerrainCell> pair in Neighbors)
        {
            if (IsDiagonalDirection(pair.Key)) continue;

            yield return pair;
        }
    }

    public Direction TryGetNeighborDirection(int offset)
    {
        if (Neighbors.Count <= 0)
            return Direction.Null;

        int dir = (int)Mathf.Repeat(offset, MaxNeighborDirections);

        while (true)
        {
            if (Neighbors.ContainsKey((Direction)dir))
                return (Direction)dir;

            dir = (dir + NeighborSearchOffset) % MaxNeighborDirections;
        }
    }

    public TerrainCell TryGetNeighborCell(int offset)
    {
        return Neighbors[TryGetNeighborDirection(offset)];
    }

    public long GenerateUniqueIdentifier(long date, long oom = 1L, long offset = 0L)
    {
        if (oom > 1000L)
        {
            Debug.LogWarning("'oom' shouldn't be greater than 1000 (oom = " + oom + ")");
        }

        if (date >= World.MaxSupportedDate)
        {
            Debug.LogWarning("TerrainCell.GenerateUniqueIdentifier - 'date' is greater than " + World.MaxSupportedDate + " (date = " + date + ")");
        }

        return (((date * 1000000) + ((long)Longitude * 1000) + (long)Latitude) * oom) + (offset % oom);
    }

    /// <summary>
    /// Adds the current buffer value to the cell's water accumulation and recalculates global maximum.
    /// </summary>
    public void UpdateDrainage()
    {
#if DEBUG
        if ((Longitude == 229) && (Latitude == 133))
        {
            Debug.Log("Debugging cell " + Position);
        }
#endif

        WaterAccumulation = Rainfall;

        float tempAcc = 0;
        float maxDrainTransfer = 0;

        foreach (KeyValuePair<TerrainCell, RiverBuffers> pair in FeedingCells)
        {
            TerrainCell dCell = pair.Key;
            RiverBuffers buffers = pair.Value;

            WaterAccumulation += buffers.DrainageTransfer;
            tempAcc += buffers.TemperatureTransfer;

            if (maxDrainTransfer < buffers.DrainageTransfer)
            {
                maxDrainTransfer = buffers.DrainageTransfer;
                RiverLength = dCell.RiverLength + buffers.DirectionFactor;
                RiverId = dCell.RiverId;
            }
        }

        World.MaxWaterAccumulation = Mathf.Max(World.MaxWaterAccumulation, WaterAccumulation);

        if ((WaterAccumulation <= Rainfall) || (WaterAccumulation <= 0))
        {
            return;
        }

        // 'drain' temperature

        tempAcc += OriginalTemperature * Rainfall;

        float newTemp =
            Mathf.Lerp(OriginalTemperature, tempAcc / WaterAccumulation, TemperatureHoldOffFactor);

#if DEBUG
        if (!newTemp.IsInsideRange(
            World.MinPossibleTemperatureWithOffset - 0.5f,
            World.MaxPossibleTemperatureWithOffset + 0.5f))
        {
            Debug.LogWarning("DrainModifyCell - Invalid newTemp: " + newTemp + ", position: " + Position);
        }
#endif

        Temperature = Mathf.Clamp(
            newTemp,
            World.MinPossibleTemperatureWithOffset,
            World.MaxPossibleTemperatureWithOffset);
    }

    public TerrainCellAlteration GetAlteration(bool regardless = false, bool addLayerData = true)
    {
        // If cell hasn't been modified there's no need to create a TerrainCellChanges object (unless it is regardless)
        if (!regardless && !Modified)
        {
            return null;
        }

        TerrainCellAlteration alteration = new TerrainCellAlteration(this, addLayerData);

        return alteration;
    }

    public void SetAlteration(TerrainCellAlteration alteration)
    {
        BaseAltitudeValue = alteration.BaseAltitudeValue;
        BaseTemperatureValue = alteration.BaseTemperatureValue;
        BaseRainfallValue = alteration.BaseRainfallValue;

        BaseTemperatureOffset = alteration.BaseTemperatureOffset;
        BaseRainfallOffset = alteration.BaseRainfallOffset;

        FarmlandPercentage = alteration.FarmlandPercentage;
        Accessibility = alteration.Accessibility;
        Arability = alteration.Arability;

        foreach (CellLayerData data in alteration.LayerData)
        {
            SetLayerData(Layer.Layers[data.Id], data.Value, data.Offset);
        }

        Modified = alteration.Modified;

        if (Altitude > World.MaxAltitude) World.MaxAltitude = Altitude;
        if (Altitude < World.MinAltitude) World.MinAltitude = Altitude;

        if (Rainfall > World.MaxRainfall) World.MaxRainfall = Rainfall;
        if (Rainfall < World.MinRainfall) World.MinRainfall = Rainfall;

        if (Temperature > World.MaxTemperature) World.MaxTemperature = Temperature;
        if (Temperature < World.MinTemperature) World.MinTemperature = Temperature;
    }

    public void AddCrossingRoute(Route route)
    {
        CrossingRoutes.Add(route);

        HasCrossingRoutes = true;
    }

    public void RemoveCrossingRoute(Route route)
    {
        CrossingRoutes.Remove(route);

        HasCrossingRoutes = CrossingRoutes.Count > 0;
    }

    public int GetNextLocalRandomInt(int queryOffset, int maxValue, bool registerForTesting = true)
    {
        int value = GetLocalRandomInt(World.CurrentDate, queryOffset, maxValue);

#if DEBUG
        if (registerForTesting && (GetNextLocalRandomCalled != null))
        {
            if (Manager.TrackGenRandomCallers)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string callingMethod = method.Name;

                int frame = 2;
                while (callingMethod.Contains("GetNextLocalRandom")
                    || callingMethod.Contains("GetNextRandom"))
                {
                    method = stackTrace.GetFrame(frame).GetMethod();
                    callingMethod = method.Name;

                    frame++;
                }

                string callingClass = method.DeclaringType.ToString();

                GetNextLocalRandomCalled(callingClass + ":" + callingMethod);
            }
            else
            {
                GetNextLocalRandomCalled(null);
            }
        }
#endif

        //		#if DEBUG
        //		if (Manager.RecordingEnabled) {
        //			LastRandomInteger = value;
        //			string key = "Long:" + Longitude + "-Lat:" + Latitude + "-Date:" + date + "-LocalIteration:" + LocalIteration;

        //			Manager.Recorder.Record (key, "LastRandomInteger:" + value);
        //		}
        //		#endif

        return value;
    }

    public int GetLocalRandomInt(long date, int queryOffset, int maxValue)
    {
        if (maxValue < 2) return 0;

        maxValue = Mathf.Min(PerlinNoise.MaxPermutationValue, maxValue);

        // This operation will reduce to zero or almost zero the number of artifacts 
        // resulting from (date & 255) being a constant value in some circumstances
        int dateFactor = unchecked((int)((date % 256) + (date % 7843)));

        int x = MathUtility.ProtectedAbs(unchecked(World.Seed + Longitude + queryOffset));
        int y = MathUtility.ProtectedAbs(unchecked(World.Seed + Latitude + queryOffset));
        int z = MathUtility.ProtectedAbs(unchecked(World.Seed + dateFactor + queryOffset));

        int value = PerlinNoise.GetPermutationValue(x, y, z) % maxValue;

        return value;
    }

    public float GetNextLocalRandomFloat(int queryOffset, bool registerForTesting = true)
    {
        int value = GetNextLocalRandomInt(queryOffset, PerlinNoise.MaxPermutationValue, registerForTesting);

        return value / (float)PerlinNoise.MaxPermutationValue;
    }

    public float GetLocalRandomFloat(long date, int queryOffset)
    {
        int value = GetLocalRandomInt(date, queryOffset, PerlinNoise.MaxPermutationValue);

        return value / (float)PerlinNoise.MaxPermutationValue;
    }

    public void ResetBiomes()
    {
        PresentBiomeIds.Clear();
        PresentWaterBiomeIds.Clear();
        BiomePresences.Clear();

        _biomePresences.Clear();

        WaterBiomePresence = 0;
        MostBiomePresence = 0;
        BiomeWithMostPresence = null;
    }

    public void AddBiomeRelPresence(Biome biome, float relPresence)
    {
        PresentBiomeIds.Add(biome.Id);
        BiomePresences.Add(relPresence);

        if (biome.TerrainType == BiomeTerrainType.Water)
        {
            PresentWaterBiomeIds.Add(biome.Id);
            WaterBiomePresence += relPresence;
        }

        _biomePresences[biome.Id] = relPresence;

        if (MostBiomePresence < relPresence)
        {
            MostBiomePresence = relPresence;
            BiomeWithMostPresence = biome.Id;
        }
    }

    public IEnumerable<KeyValuePair<string, float>> GetBiomePresencePairs()
    {
        for (int i = 0; i < PresentBiomeIds.Count; i++)
        {
            yield return
                new KeyValuePair<string, float>(
                    PresentBiomeIds[i],
                    BiomePresences[i]);
        }
    }

    public Dictionary<string, float> GetLocalAndNeighborhoodBiomePresences(
        bool ignoreWaterType = false)
    {
        Dictionary<string, float> biomePresences = new Dictionary<string, float>();

        foreach (KeyValuePair<string, float> pair in GetBiomePresencePairs())
        {
            if (ignoreWaterType && (Biome.Biomes[pair.Key].TerrainType == BiomeTerrainType.Water))
                continue;

            biomePresences[pair.Key] = pair.Value;
        }

        foreach (TerrainCell nCell in Neighbors.Values)
        {
            foreach (KeyValuePair<string, float> pair in nCell.GetBiomePresencePairs())
            {
                if (ignoreWaterType && (Biome.Biomes[pair.Key].TerrainType == BiomeTerrainType.Water))
                    continue;

                if (biomePresences.ContainsKey(pair.Key))
                {
                    biomePresences[pair.Key] += pair.Value;
                }
                else
                {
                    biomePresences[pair.Key] = pair.Value;
                }
            }
        }

        return biomePresences;
    }

    public string GetLocalAndNeighborhoodMostPresentBiome(
        bool ignoreWaterType = false)
    {
        string mostPresent = null;
        float maxPresence = -1;

        foreach (KeyValuePair<string, float> pair in
            GetLocalAndNeighborhoodBiomePresences(ignoreWaterType))
        {
            if (pair.Value > maxPresence)
            {
                maxPresence = pair.Value;
                mostPresent = pair.Key;
            }
        }

        return mostPresent;
    }

    public float GetBiomeTypePresence(BiomeTerrainType type)
    {
        float typePresence = 0;

        for (int i = 0; i < PresentBiomeIds.Count; i++)
        {
            Biome biome = Biome.Biomes[PresentBiomeIds[i]];

            if (biome.TerrainType == type)

            typePresence += BiomePresences[i];
        }

        return typePresence;
    }

    public float GetNeighborhoodBiomeTypePresence(BiomeTerrainType type)
    {
        float presence = GetBiomeTypePresence(type);

        foreach (TerrainCell nCell in Neighbors.Values)
        {
            presence += nCell.GetBiomeTypePresence(type);
        }

        return presence;
    }

    public float GetBiomeTraitPresence(string trait)
    {
        float traitPresence = 0;

        for (int i = 0; i < PresentBiomeIds.Count; i++)
        {
            Biome biome = Biome.Biomes[PresentBiomeIds[i]];

            if (biome.Traits.Contains(trait))

                traitPresence += BiomePresences[i];
        }

        return traitPresence;
    }

    public float GetNeighborhoodBiomeTraitPresence(string trait)
    {
        float presence = GetBiomeTraitPresence(trait);

        foreach (TerrainCell nCell in Neighbors.Values)
        {
            presence += nCell.GetBiomeTraitPresence(trait);
        }

        return presence;
    }

    public float GetBiomePresence(string biomeId)
    {
        float value = 0;

        if (!_biomePresences.TryGetValue(biomeId, out value))
            return 0;

        return value;
    }

    public void ResetLayerData(Layer layer)
    {
        CellLayerData data = GetLayerData(layer.Id);

        if (data != null)
        {
            data.Value = Mathf.Clamp01(data.BaseValue);
            data.Offset = 0;
//#if DEBUG
//            if (Manager.WorldIsReady)
//            {
//                Debug.Log("Reseting cell " + Position + " layer data. data.Value: " + data.Value + ", data.BaseValue: " + data.BaseValue + ", data.BaseOffset: " + data.BaseOffset);
//            }
//#endif
        }
        else
        {
//#if DEBUG
//            if (Manager.WorldIsReady)
//            {
//                Debug.Log("Reseting cell " + Position + " layer data. Data is null");
//            }
//#endif
        }

        if (layer.MaxPresentValue < data.Value)
        {
            layer.MaxPresentValue = data.Value;
        }
    }

    public void SetLayerData(Layer layer, float value, float offset)
    {
        SetLayerData(layer, value, offset, GetLayerData(layer.Id));
    }

    public void SetLayerData(Layer layer, float value, float offset, CellLayerData data)
    {
        if (data == null)
        {
            if ((value <= 0) && (offset == 0)) return;

            data = new CellLayerData();

            PresentLayerIds.Add(layer.Id);
            LayerData.Add(data);

            _layerData[layer.Id] = data;
        }

        data.BaseValue = value - offset;
        data.Offset = offset;

        data.Id = layer.Id;

        data.Value = Mathf.Clamp01(value);

        //#if DEBUG
        //        if (Manager.WorldIsReady)
        //        {
        //            Debug.Log("Setting cell " + Position + " layer data. data.Value: " + data.Value + ", data.BaseValue: " + data.BaseValue + ", data.BaseOffset: " + data.BaseOffset);
        //        }
        //#endif

        if (layer.MaxPresentValue < data.Value)
        {
            layer.MaxPresentValue = data.Value;
        }
    }

    public CellLayerData GetLayerData(string layerId)
    {
        CellLayerData data;

        if (!_layerData.TryGetValue(layerId, out data))
            return null;

        return data;
    }

    public float GetLayerValue(string layerId)
    {
        CellLayerData data;

        if (!_layerData.TryGetValue(layerId, out data))
            return 0;

        return data.Value;
    }

    public void InitializeNeighbors()
    {
        SetNeighborCells();

        NeighborDistances = new Dictionary<Direction, float>(Neighbors.Count);

        foreach (KeyValuePair<Direction, TerrainCell> pair in Neighbors)
        {
            float distance = CalculateDistance(pair.Value, pair.Key);

            NeighborDistances.Add(pair.Key, distance);
        }
    }

    public float CalculateDistance(TerrainCell cell, Direction direction)
    {
        float distance = TerrainCell.MaxWidth;

        if ((direction == Direction.Northeast) ||
            (direction == Direction.Northwest) ||
            (direction == Direction.Southeast) ||
            (direction == Direction.Southwest))
        {
            float sqMaxWidth = TerrainCell.MaxWidth * TerrainCell.MaxWidth;

            float widthSum = (Width + cell.Width) / 2f;
            float sqCellWidth = widthSum * widthSum;

            distance = Mathf.Sqrt(sqMaxWidth + sqCellWidth);
        }
        else if ((direction == Direction.East) ||
          (direction == Direction.West))
        {
            distance = cell.Width;
        }

        float altitudeDiff = Altitude - cell.Altitude;
        float sqAltDif = altitudeDiff * altitudeDiff;
        float sqDistance = distance * distance;

        distance = Mathf.Sqrt(sqAltDif + sqDistance);

        return distance;
    }

    public void InitializeMiscellaneous()
    {
        IsPartOfCoastline = FindIfCoastline();
    }

    public TerrainCell GetNeighborCell(Direction direction)
    {
        TerrainCell nCell;

        if (!Neighbors.TryGetValue(direction, out nCell))
            return null;

        return nCell;
    }

    private void SetNeighborCells()
    {
        Neighbors = new Dictionary<Direction, TerrainCell>(8);
        NeighborSet = new HashSet<TerrainCell>();

        int wLongitude = (World.Width + Longitude - 1) % World.Width;
        int eLongitude = (Longitude + 1) % World.Width;

        if (Latitude < (World.Height - 1))
        {
            Neighbors.Add(Direction.Northwest, World.TerrainCells[wLongitude][Latitude + 1]);
            Neighbors.Add(Direction.North, World.TerrainCells[Longitude][Latitude + 1]);
            Neighbors.Add(Direction.Northeast, World.TerrainCells[eLongitude][Latitude + 1]);
        }

        Neighbors.Add(Direction.West, World.TerrainCells[wLongitude][Latitude]);
        Neighbors.Add(Direction.East, World.TerrainCells[eLongitude][Latitude]);

        if (Latitude > 0)
        {
            Neighbors.Add(Direction.Southwest, World.TerrainCells[wLongitude][Latitude - 1]);
            Neighbors.Add(Direction.South, World.TerrainCells[Longitude][Latitude - 1]);
            Neighbors.Add(Direction.Southeast, World.TerrainCells[eLongitude][Latitude - 1]);
        }

        NeighborSet.UnionWith(Neighbors.Values);
    }

    private bool FindIfCoastline()
    {
        if (WaterBiomePresence > 0.5f)
        {
            foreach (TerrainCell nCell in Neighbors.Values)
            {
                if (nCell.WaterBiomePresence < 0.5f)
                    return true;
            }
        }
        else
        {
            foreach (TerrainCell nCell in Neighbors.Values)
            {
                if (nCell.WaterBiomePresence >= 0.5f)
                    return true;
            }
        }

        return false;
    }
}

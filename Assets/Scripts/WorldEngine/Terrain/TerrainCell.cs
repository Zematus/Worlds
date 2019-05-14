using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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

public class TerrainCell : ISynchronizable
{
#if DEBUG

    public delegate void GetNextLocalRandomCalledDelegate(string callerMethod);

    //	public static int LastRandomInteger = 0; 
    public static GetNextLocalRandomCalledDelegate GetNextLocalRandomCalled = null;

#endif

    public const int MaxNeighborDirections = 8;
    public const int NeighborSearchOffset = 3;

    [XmlAttribute]
    public bool Modified = false; // This will be true if the cell has been modified after/during generation by using a heighmap, using the map editor, or by running the simulation

    [XmlAttribute]
    public int Longitude;
    [XmlAttribute]
    public int Latitude;

    [XmlAttribute]
    public float Height;
    [XmlAttribute]
    public float Width;

    [XmlAttribute]
    public float BaseAltitudeValue;
    [XmlAttribute]
    public float BaseRainfallValue;
    [XmlAttribute]
    public float BaseTemperatureValue;
    
    [XmlAttribute]
    public float BaseRainfallOffset;
    [XmlAttribute]
    public float BaseTemperatureOffset;

    [XmlAttribute]
    public float Altitude;
    [XmlAttribute]
    public float Rainfall;
    [XmlAttribute]
    public float Temperature;

    [XmlAttribute]
    public float Survivability;
    [XmlAttribute]
    public float ForagingCapacity;
    [XmlAttribute]
    public float Accessibility;
    [XmlAttribute]
    public float Arability;

    [XmlAttribute]
    public bool IsPartOfCoastline;

    [XmlAttribute]
    public float FarmlandPercentage = 0;

    [XmlAttribute]
    public string BiomeWithMostPresence = null;
    [XmlAttribute]
    public float MostBiomePresence = 0;

    public static float MaxArea;

    public static float MaxWidth;

    public List<string> PresentBiomeIds = new List<string>();
    public List<float> BiomePresences = new List<float>();

    public List<string> PresentLayerIds = new List<string>();
    public List<float> LayerValue = new List<float>();

    public CellGroup Group;

    [XmlIgnore]
    public float Alpha;
    [XmlIgnore]
    public float Beta;

    [XmlIgnore]
    public List<string> PresentSeaBiomeIds = new List<string>();

    [XmlIgnore]
    public float SeaBiomePresence = 0;

    [XmlIgnore]
    public WorldPosition Position;

    [XmlIgnore]
    public Region Region = null;

    [XmlIgnore]
    public Territory EncompassingTerritory = null;

    [XmlIgnore]
    public List<Route> CrossingRoutes = new List<Route>();

    [XmlIgnore]
    public bool HasCrossingRoutes = false;

    [XmlIgnore]
    public float Area;

    [XmlIgnore]
    public float MaxAreaPercent;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public bool IsSelected = false;

    [XmlIgnore]
    public List<TerrainCell> RainfallDependentCells = new List<TerrainCell>();

    [XmlIgnore]
    public Dictionary<Direction, TerrainCell> Neighbors { get; private set; }

    [XmlIgnore]
    public Dictionary<Direction, float> NeighborDistances { get; private set; }

    private HashSet<string> _flags = new HashSet<string>();

    private Dictionary<string, float> _biomePresences = new Dictionary<string, float>();
    private Dictionary<string, float> _layerValues = new Dictionary<string, float>();

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
#if DEBUG
        if (oom > 1000L)
        {
            Debug.LogWarning("'oom' shouldn't be greater than 1000 (oom = " + oom + ")");
        }

        if (date >= World.MaxSupportedDate)
        {
            Debug.LogWarning("'date' shouldn't be greater than " + World.MaxSupportedDate + " (date = " + date + ")");
        }
#endif

        return (((date * 1000000) + ((long)Longitude * 1000) + (long)Latitude) * oom) + (offset % oom);
    }

    public TerrainCellAlteration GetAlteration(bool regardless = false)
    {
        // If cell hasn't been modified there's no need to create a TerrainCellChanges object (unless it is regardless)
        if (!regardless && !Modified)
        {
            return null;
        }

        TerrainCellAlteration alteration = new TerrainCellAlteration(this);

        alteration.Flags.AddRange(_flags);

        return alteration;
    }

    public void SetAlteration(TerrainCellAlteration alteration)
    {
        BaseAltitudeValue = alteration.BaseAltitudeValue;
        BaseTemperatureValue = alteration.BaseTemperatureValue;
        BaseRainfallValue = alteration.BaseRainfallValue;

        BaseTemperatureOffset = alteration.BaseTemperatureOffset;
        BaseRainfallOffset = alteration.BaseRainfallOffset;

        Altitude = alteration.Altitude;
        Temperature = alteration.Temperature;
        Rainfall = alteration.Rainfall;

        FarmlandPercentage = alteration.FarmlandPercentage;

        Modified = alteration.Modified;

        foreach (string flag in alteration.Flags)
        {
            _flags.Add(flag);
        }

        if (Altitude > World.MaxAltitude) World.MaxAltitude = Altitude;
        if (Altitude < World.MinAltitude) World.MinAltitude = Altitude;

        if (Rainfall > World.MaxRainfall) World.MaxRainfall = Rainfall;
        if (Rainfall < World.MinRainfall) World.MinRainfall = Rainfall;

        if (Temperature > World.MaxTemperature) World.MaxTemperature = Temperature;
        if (Temperature < World.MinTemperature) World.MinTemperature = Temperature;
    }

    public void SetFlag(string flag)
    {
        if (_flags.Contains(flag))
            return;

        _flags.Add(flag);
    }

    public bool IsFlagSet(string flag)
    {
        return _flags.Contains(flag);
    }

    public void UnsetFlag(string flag)
    {
        if (!_flags.Contains(flag))
            return;

        _flags.Remove(flag);
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

        int dateFactor = unchecked((int)((date % 256) + (date % 7843))); // This operation will reduce to zero or almost zero the number of artifacts resulting from (date & 255) being a constant value in some circumstances

        int x = Mathf.Abs(unchecked(World.Seed + Longitude + queryOffset));
        int y = Mathf.Abs(unchecked(World.Seed + Latitude + queryOffset));
        int z = Mathf.Abs(unchecked(World.Seed + dateFactor + queryOffset));

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
        PresentSeaBiomeIds.Clear();
        BiomePresences.Clear();

        _biomePresences.Clear();

        MostBiomePresence = 0;
        BiomeWithMostPresence = null;
    }

    public void ResetLayers()
    {
        PresentLayerIds.Clear();
        LayerValue.Clear();
        
        _layerValues.Clear();
    }

    public float GetBiomePresence(Biome biome)
    {
        return GetBiomePresence(biome.Id);
    }

    public void AddBiomePresence(Biome biome, float presence)
    {
        PresentBiomeIds.Add(biome.Id);
        BiomePresences.Add(presence);

        if (biome.Type == Biome.LocactionType.Sea)
        {
            PresentSeaBiomeIds.Add(biome.Id);
            SeaBiomePresence += presence;
        }

        _biomePresences[biome.Id] = presence;

        if (MostBiomePresence < presence)
        {
            MostBiomePresence = presence;
            BiomeWithMostPresence = biome.Id;
        }
    }

    public void AddLayerValue(Layer layer, float value)
    {
        PresentLayerIds.Add(layer.Id);
        LayerValue.Add(value);

        _layerValues[layer.Id] = value;

        if (layer.MaxPresentValue < value)
        {
            layer.MaxPresentValue = value;
        }
    }

    public float GetBiomePresence(string biomeId)
    {
        float value = 0;

        if (!_biomePresences.TryGetValue(biomeId, out value))
            return 0;

        return value;
    }

    public float GetLayerValue(string layerId)
    {
        float value = 0;

        if (!_layerValues.TryGetValue(layerId, out value))
            return 0;

        return value;
    }

    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
        Position = new WorldPosition(Longitude, Latitude);

        Alpha = (Latitude / (float)World.Height) * Mathf.PI;
        Beta = (Longitude / (float)World.Width) * Mathf.PI * 2;

        for (int i = 0; i < PresentBiomeIds.Count; i++)
        {
            string biomeId = PresentBiomeIds[i];

            _biomePresences[biomeId] = BiomePresences[i];

            Biome biome = Biome.Biomes[biomeId];

            if (biome.Type == Biome.LocactionType.Sea)
            {
                PresentSeaBiomeIds.Add(biomeId);
                SeaBiomePresence += BiomePresences[i];
            }
        }

        for (int i = 0; i < PresentLayerIds.Count; i++)
        {
            string layerId = PresentLayerIds[i];

            _layerValues[layerId] = LayerValue[i];
        }

        InitializeNeighbors();

        if (Group != null)
        {
            Group.World = World;
            Group.Cell = this;

            World.AddGroup(Group);

            Group.FinalizeLoad();
        }
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
    }

    private bool FindIfCoastline()
    {
        if (Altitude <= 0)
        {
            foreach (TerrainCell nCell in Neighbors.Values)
            {
                if (nCell.Altitude > 0)
                    return true;
            }
        }
        else
        {
            foreach (TerrainCell nCell in Neighbors.Values)
            {
                if (nCell.Altitude <= 0)
                    return true;
            }
        }

        return false;
    }
}

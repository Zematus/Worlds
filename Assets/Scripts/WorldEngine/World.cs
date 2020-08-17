using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.Profiling;

public delegate void ProgressCastDelegate(float value, string message = null, bool reset = false);

public static class RngOffsets
{
    //public const int CELL_GROUP_CONSIDER_LAND_MIGRATION_TARGET = 0;
    public const int CELL_GROUP_CONSIDER_LAND_MIGRATION_CHANCE = 1;

    public const int CELL_GROUP_CONSIDER_SEA_MIGRATION = 2;

    public const int CELL_GROUP_CALCULATE_NEXT_UPDATE = 3;

    public const int CELL_GROUP_SET_POLITY_UPDATE = 4;

    public const int CELL_GROUP_CONSIDER_POLITY_PROMINENCE_EXPANSION_POLITY = 5;
    public const int CELL_GROUP_CONSIDER_POLITY_PROMINENCE_EXPANSION_CHANCE = 6;

    public const int CELL_GROUP_PICK_MIGRATION_DIRECTION = 7;
    public const int CELL_GROUP_PICK_PROMINENCE_TRANSFER_DIRECTION = 8;

    public const int PREFERENCE_UPDATE = 10000;
    public const int PREFERENCE_POLITY_PROMINENCE = 10100;

    public const int ACTIVITY_UPDATE = 11000;
    public const int ACTIVITY_POLITY_PROMINENCE = 11100;

    public const int KNOWLEDGE_MERGE = 20000;
    public const int KNOWLEDGE_MODIFY_VALUE = 20100;
    public const int KNOWLEDGE_UPDATE_VALUE_INTERNAL = 20200;
    public const int KNOWLEDGE_POLITY_PROMINENCE = 20300;
    public const int KNOWLEDGE_FACTION_CORE_UPDATE = 20400;

    public const int SKILL_UPDATE = 30000;
    public const int SKILL_POLITY_PROMINENCE = 30100;

    public const int POLITY_CULTURE_NORMALIZE_ATTRIBUTE_VALUES = 40000;
    public const int POLITY_CULTURE_GENERATE_NEW_LANGUAGE = 40100;

    public const int POLITY_UPDATE_EFFECTS = 50000;

    public const int REGION_GENERATE_NAME = 60000;
    public const int REGION_SELECT_BORDER_REGION_TO_REPLACE_WITH = 61000;
    public const int REGION_SELECT_SUBSET_CELL = 62000;

    public const int TRIBE_GENERATE_NEW_TRIBE = 70000;
    public const int TRIBE_GENERATE_NAME = 71000;

    public const int FACTION_CULTURE_DISCOVERY_LOSS_CHANCE = 80500;

    public const int CLAN_GENERATE_NAME = 85000;
    public const int CLAN_CHOOSE_CORE_GROUP = 85100;
    public const int CLAN_CHOOSE_TARGET_GROUP = 85200;
    public const int CLAN_LEADER_GEN_OFFSET = 85300;
    public const int CLAN_SPLIT = 85500;

    public const int AGENT_GENERATE_BIO = 90000;
    public const int AGENT_GENERATE_NAME = 91000;

    public const int ROUTE_CHOOSE_NEXT_DEPTH_SEA_CELL = 100000;
    public const int ROUTE_CHOOSE_NEXT_COASTAL_CELL = 110000;
    public const int ROUTE_CHOOSE_NEXT_COASTAL_CELL_2 = 120000;

    public const int TRIBE_FORMATION_EVENT_CALCULATE_TRIGGER_DATE = 900003;

    public const int CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE = 900006;
    public const int CLAN_SPLITTING_EVENT_PREFER_SPLIT = 900007;
    public const int CLAN_SPLITTING_EVENT_LEADER_PREVENTS_MODIFY_ATTRIBUTE = 900008;

    public const int TRIBE_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE = 900010;
    public const int TRIBE_SPLITTING_EVENT_SPLITCLAN_PREFER_SPLIT = 900011;
    public const int TRIBE_SPLITTING_EVENT_TRIBE_PREFER_SPLIT = 900012;
    public const int TRIBE_SPLITTING_EVENT_SPLITCLAN_LEADER_PREVENTS_MODIFY_ATTRIBUTE = 900013;
    public const int TRIBE_SPLITTING_EVENT_TRIBE_LEADER_PREVENTS_MODIFY_ATTRIBUTE = 900014;

    public const int CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE = 900020;

    public const int CLAN_DEMANDS_INFLUENCE_EVENT_CALCULATE_TRIGGER_DATE = 900021;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_PERFORM_DEMAND = 900022;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_ACCEPT_DEMAND = 900023;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_DEMANDCLAN_LEADER_AVOIDS_DEMAND_MODIFY_ATTRIBUTE = 900024;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_DEMANDCLAN_LEADER_DEMANDS_MODIFY_ATTRIBUTE = 900025;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_DOMINANTCLAN_LEADER_REJECTS_DEMAND_MODIFY_ATTRIBUTE = 900026;
    public const int CLAN_DEMANDS_INFLUENCE_EVENT_DOMINANTCLAN_LEADER_ACCEPTS_DEMAND_MODIFY_ATTRIBUTE = 900027;

    public const int FOSTER_TRIBE_RELATION_EVENT_CALCULATE_TRIGGER_DATE = 900030;
    public const int FOSTER_TRIBE_RELATION_EVENT_MAKE_ATTEMPT = 900031;
    public const int FOSTER_TRIBE_RELATION_EVENT_REJECT_OFFER = 900032;
    public const int FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE = 900033;
    public const int FOSTER_TRIBE_RELATION_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE = 900034;
    public const int FOSTER_TRIBE_RELATION_EVENT_TARGETTRIBE_LEADER_ACCEPT_OFFER = 900035;
    public const int FOSTER_TRIBE_RELATION_EVENT_TARGETTRIBE_LEADER_REJECTS_OFFER_MODIFY_ATTRIBUTE = 900036;
    public const int FOSTER_TRIBE_RELATION_EVENT_TARGETTRIBE_LEADER_ACCEPTS_OFFER_MODIFY_ATTRIBUTE = 900037;

    public const int MERGE_TRIBES_EVENT_CALCULATE_TRIGGER_DATE = 900040;
    public const int MERGE_TRIBES_EVENT_MAKE_ATTEMPT = 900041;
    public const int MERGE_TRIBES_EVENT_REJECT_OFFER = 900042;
    public const int MERGE_TRIBES_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE = 900043;
    public const int MERGE_TRIBES_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE = 900044;
    public const int MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_ACCEPT_OFFER = 900045;
    public const int MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_REJECTS_OFFER_MODIFY_ATTRIBUTE = 900046;
    public const int MERGE_TRIBES_EVENT_TARGETTRIBE_LEADER_ACCEPTS_OFFER_MODIFY_ATTRIBUTE = 900047;

    public const int OPEN_TRIBE_EVENT_CALCULATE_TRIGGER_DATE = 900050;
    public const int OPEN_TRIBE_EVENT_MAKE_ATTEMPT = 900051;
    public const int OPEN_TRIBE_EVENT_SOURCETRIBE_LEADER_AVOIDS_ATTEMPT_MODIFY_ATTRIBUTE = 900052;
    public const int OPEN_TRIBE_EVENT_SOURCETRIBE_LEADER_MAKES_ATTEMPT_MODIFY_ATTRIBUTE = 900053;

    public const int EVENT_TRIGGER = 1000000;
    public const int EVENT_CAN_TRIGGER = 1100000;

    public const int MIGRATING_GROUP_MOVE_FACTION_CORE = 2000000;
    public const int EXPAND_POLITY_MOVE_FACTION_CORE = 2100000;
}

public enum GenerationType
{
    Temperature = 0x03, // also generate rainfall
    Rainfall = 0x02,
    TerrainNormal = 0x07, // generate altitude, temperature, and rainfall
    TerrainRegeneration = 0x0B, // regenerate altitude, generate temperature and rainfall
    TemperatureRegeneration = 0x12, // also generate rainfall
    RainfallRegeneration = 0x20,
    LayerRegeneration = 0x40,
    DrainageGeneration = 0x00
}

[XmlRoot]
public class World : ISynchronizable
{
    //public const long MaxSupportedDate = 9223372036L;
    public const long MaxSupportedDate = long.MaxValue;

    public const int YearLength = 365;

    public const long MaxPossibleTimeToSkip = int.MaxValue / 10;

    public const float Circumference = 40075; // In kilometers;

    //public const int NumContinents = 7;
    public const int NumContinents = 15;
    //public const float ContinentBaseWidthFactor = 0.95f;
    public const float ContinentBaseWidthFactor = 1.1f; // Smaller number == bigger masses
    public const float ContinentMinWidthFactor = ContinentBaseWidthFactor * 5.7f;
    public const float ContinentMaxWidthFactor = ContinentBaseWidthFactor * 8.7f;

    public const float DefaultAltitudeScale = 0.75f;
    public const float AvgPossibleRainfall = 990f;
    public const float AvgPossibleTemperature = 13.7f;
    public const float DefaultRiverStrength = 0.50f;

    public const float MinPossibleAltitude = -15000;
    public const float MaxPossibleAltitude = 15000;

    public const float MinPossibleRainfall = -AvgPossibleRainfall;
    public const float MaxPossibleRainfall = 13000;

    public const float MinPossibleTemperature = -50 - AvgPossibleTemperature;
    public const float MaxPossibleTemperature = 30 + AvgPossibleTemperature;

    public const float OceanDispersalFactor = 0.25f;
    public const float MinOceanDispersal = 50f;
    public const float WaterErosionFactor = 600f;
    public const float HeightToRainfallConversionFactor = 1000f;
    public const float RainfallToHeightConversionFactor = 1f / HeightToRainfallConversionFactor;
    public const float MinRiverFlow = HeightToRainfallConversionFactor / 50f;

    public const float StartPopulationDensity = 0.5f;

    public const int MinStartingPopulation = 100;
    public const int MaxStartingPopulation = 100000;

    public const float MinSurvivabilityForRandomGroupPlacement = 0.15f;

    public const float TerrainGenerationSteps = 9;

    public static Dictionary<string, IWorldEventGenerator> EventGenerators;

    public int EventsTriggered = 0;

    [XmlAttribute]
    public int Width { get; set; }
    [XmlAttribute]
    public int Height { get; set; }

    [XmlAttribute]
    public int Seed { get; set; }

    [XmlAttribute]
    public long CurrentDate { get; set; }

    [XmlAttribute]
    public long MaxTimeToSkip { get; set; }

    [XmlAttribute]
    public int CellGroupCount { get; set; }

    [XmlAttribute]
    public int MemorableAgentCount { get; set; }

    [XmlAttribute]
    public int FactionCount { get; set; }

    [XmlAttribute]
    public int PolityCount { get; set; }

    [XmlAttribute]
    public int RegionCount { get; set; }

    [XmlAttribute]
    public int LanguageCount { get; set; }

    [XmlAttribute]
    public int TerrainCellAlterationListCount { get; set; }

    [XmlAttribute]
    public float AltitudeScale { get; set; }

    [XmlAttribute]
    public float SeaLevelOffset { get; set; }

    [XmlAttribute]
    public float RiverStrength { get; set; }

    [XmlAttribute]
    public float RainfallOffset { get; set; }

    [XmlAttribute]
    public float TemperatureOffset { get; set; }

    [XmlAttribute]
    public int SerializedEventCount { get; set; }

    public List<string> ModPaths;

    public List<LayerSettings> LayerSettings;

    // Start wonky segment (save failures might happen here)

    [XmlArrayItem(Type = typeof(UpdateCellGroupEvent)),
        XmlArrayItem(Type = typeof(MigratePopulationEvent)),
        XmlArrayItem(Type = typeof(ExpandPolityProminenceEvent)),
        XmlArrayItem(Type = typeof(TribeFormationEvent)),
        // TODO: cleanup
        //XmlArrayItem(Type = typeof(ClanSplitDecisionEvent)),
        //XmlArrayItem(Type = typeof(ClanDemandsInfluenceDecisionEvent)),
        XmlArrayItem(Type = typeof(TribeSplitDecisionEvent)),
        XmlArrayItem(Type = typeof(ClanCoreMigrationEvent)),
        XmlArrayItem(Type = typeof(FosterTribeRelationDecisionEvent)),
        XmlArrayItem(Type = typeof(MergeTribesDecisionEvent)),
        XmlArrayItem(Type = typeof(OpenTribeDecisionEvent)),
        XmlArrayItem(Type = typeof(Discovery.DiscoveryEvent)),
        XmlArrayItem(Type = typeof(FactionModEvent)),
        XmlArrayItem(Type = typeof(CellGroupModEvent))]
    public List<WorldEvent> EventsToHappen;

    public List<TerrainCellAlteration> TerrainCellAlterationList = new List<TerrainCellAlteration>();

    public List<CulturalPreferenceInfo> CulturalPreferenceInfoList = new List<CulturalPreferenceInfo>();
    public List<CulturalActivityInfo> CulturalActivityInfoList = new List<CulturalActivityInfo>();
    public List<CulturalSkillInfo> CulturalSkillInfoList = new List<CulturalSkillInfo>();
    public List<CulturalKnowledgeInfo> CulturalKnowledgeInfoList = new List<CulturalKnowledgeInfo>();

    public List<string> ExistingDiscoveryIds = new List<string>();

    public List<CellGroup> CellGroups;

    [XmlArrayItem(Type = typeof(Agent))]
    public List<Agent> MemorableAgents;

    public List<FactionInfo> FactionInfos = null;
    public List<PolityInfo> PolityInfos = null;
    public List<RegionInfo> RegionInfos = null;

    public List<Language> Languages = null;

    // End wonky segment

    public List<long> EventMessageIds;

    [XmlIgnore]
    public int EventsToHappenCount { get; private set; }

    [XmlIgnore]
    public TerrainCell SelectedCell = null;
    [XmlIgnore]
    public Region SelectedRegion = null;
    [XmlIgnore]
    public Territory SelectedTerritory = null;
    [XmlIgnore]
    public Faction GuidedFaction = null;
    [XmlIgnore]
    public HashSet<Polity> PolitiesUnderPlayerFocus = new HashSet<Polity>();

    [XmlIgnore]
    public float MinPossibleAltitudeWithOffset = MinPossibleAltitude - Manager.SeaLevelOffset;
    [XmlIgnore]
    public float MaxPossibleAltitudeWithOffset = MaxPossibleAltitude - Manager.SeaLevelOffset;

    [XmlIgnore]
    public float MinPossibleRainfallWithOffset = MinPossibleRainfall + Manager.RainfallOffset;
    [XmlIgnore]
    public float MaxPossibleRainfallWithOffset = MaxPossibleRainfall + Manager.RainfallOffset;

    [XmlIgnore]
    public float MinPossibleTemperatureWithOffset = MinPossibleTemperature + Manager.TemperatureOffset;
    [XmlIgnore]
    public float MaxPossibleTemperatureWithOffset = MaxPossibleTemperature + Manager.TemperatureOffset;

    [XmlIgnore]
    public float MaxAltitude = MaxPossibleAltitude;
    [XmlIgnore]
    public float MinAltitude = MinPossibleAltitude;

    [XmlIgnore]
    public float MaxRainfall = MaxPossibleRainfall;
    [XmlIgnore]
    public float MinRainfall = MinPossibleRainfall;

    [XmlIgnore]
    public float MaxTemperature = MaxPossibleTemperature;
    [XmlIgnore]
    public float MinTemperature = MinPossibleTemperature;

    [XmlIgnore]
    public float MaxWaterAccumulation = 0;

    [XmlIgnore]
    public TerrainCell[][] TerrainCells;

    [XmlIgnore]
    public CellGroup MostPopulousGroup = null;

    [XmlIgnore]
    public ProgressCastDelegate ProgressCastMethod { get; set; }

    [XmlIgnore]
    public CellGroup MigrationTaggedGroup = null;

    [XmlIgnore]
    public bool GroupsHaveBeenUpdated = false;
    [XmlIgnore]
    public bool FactionsHaveBeenUpdated = false;
    [XmlIgnore]
    public bool PolitiesHaveBeenUpdated = false;
    [XmlIgnore]
    public bool PolityClustersHaveBeenUpdated = false;

#if DEBUG
    private bool _needsDrainageRegeneration = true;

    [XmlIgnore]
    public bool NeedsDrainageRegeneration
    {
        get {
            return _needsDrainageRegeneration;
        }
        set {
            if (value == true)
            {
                Debug.Log("NeedsDrainageRegeneration set to true");
            }

            _needsDrainageRegeneration = value;
        }
    }
#else
    [XmlIgnore]
    public bool NeedsDrainageRegeneration = true;
#endif

#if DEBUG
    [XmlIgnore]
    public int PolityMergeCount = 0;
#endif

    [XmlIgnore]
    public Dictionary<string, Discovery> ExistingDiscoveries = new Dictionary<string, Discovery>();

    private Dictionary<Identifier, FactionInfo> _factionInfos =
        new Dictionary<Identifier, FactionInfo>();
    private Dictionary<Identifier, PolityInfo> _polityInfos =
        new Dictionary<Identifier, PolityInfo>();
    private Dictionary<Identifier, RegionInfo> _regionInfos =
        new Dictionary<Identifier, RegionInfo>();

    private BinaryTree<long, WorldEvent> _eventsToHappen = new BinaryTree<long, WorldEvent>();

    private List<WorldEvent> _eventsToHappenNow = new List<WorldEvent>();

    private HashSet<string> _culturalPreferenceIdList = new HashSet<string>();
    private HashSet<string> _culturalActivityIdList = new HashSet<string>();
    private HashSet<string> _culturalSkillIdList = new HashSet<string>();
    private HashSet<string> _culturalKnowledgeIdList = new HashSet<string>();

    private Dictionary<Identifier, CellGroup> _cellGroups =
        new Dictionary<Identifier, CellGroup>();

    private HashSet<CellGroup> _updatedGroups = new HashSet<CellGroup>();
    private HashSet<CellGroup> _groupsToUpdate = new HashSet<CellGroup>();
    private HashSet<CellGroup> _groupsToRemove = new HashSet<CellGroup>();

    private HashSet<CellGroup> _groupsToPostUpdate_afterPolityUpdates = new HashSet<CellGroup>();
    private HashSet<CellGroup> _groupsToCleanupAfterUpdate = new HashSet<CellGroup>();

    private List<MigratingUnorganizedBands> _migratingBands = new List<MigratingUnorganizedBands>();

    private Dictionary<Identifier, Agent> _memorableAgents =
        new Dictionary<Identifier, Agent>();

    private HashSet<Faction> _factionsToSplit = new HashSet<Faction>();
    private HashSet<Faction> _factionsToUpdate = new HashSet<Faction>();
    private HashSet<Faction> _factionsToRemove = new HashSet<Faction>();

    private HashSet<Polity> _politiesToUpdate = new HashSet<Polity>();
    private HashSet<Polity> _politiesThatNeedClusterUpdate = new HashSet<Polity>();
    private HashSet<Polity> _politiesToRemove = new HashSet<Polity>();

    private Dictionary<Identifier, Language> _languages =
        new Dictionary<Identifier, Language>();

    private HashSet<long> _eventMessageIds = new HashSet<long>();
    private Queue<WorldEventMessage> _eventMessagesToShow = new Queue<WorldEventMessage>();

    [System.Obsolete]
    private Queue<Decision> _decisionsToResolve = new Queue<Decision>();
    private readonly Queue<ModDecision> _modDecisionsToResolve = new Queue<ModDecision>();

    private Vector2[] _continentOffsets;
    private float[] _continentWidths;
    private float[] _continentHeights;
    private float[] _continentAltitudeOffsets;

    private float _progressIncrement = 0.25f;

    private float _accumulatedProgress = 0;

    private float _cellMaxSideLength;

    private long _dateToSkipTo;

    private bool _justLoaded = false;

    private Vector3 _altitudeBrushNoiseOffset;
    private Vector3 _temperatureBrushNoiseOffset;
    private Vector3 _rainfallBrushNoiseOffset;

    private Dictionary<string, Vector3> _layerBrushNoiseOffsets = new Dictionary<string, Vector3>();

    private ManagerTask<Vector3> _altitudeNoiseOffset1;
    private ManagerTask<Vector3> _altitudeNoiseOffset2;
    private ManagerTask<Vector3> _altitudeNoiseOffset3;
    private ManagerTask<Vector3> _altitudeNoiseOffset4;
    private ManagerTask<Vector3> _altitudeNoiseOffset5;
    private ManagerTask<Vector3> _altitudeNoiseOffset6;
    private ManagerTask<Vector3> _altitudeNoiseOffset7;
    private ManagerTask<Vector3> _altitudeNoiseOffset8;
    private ManagerTask<Vector3> _altitudeNoiseOffset9;
    //private ManagerTask<Vector3> _altitudeNoiseOffset10;

    private ManagerTask<Vector3> _arabilityNoiseOffset;

    private ManagerTask<Vector3> _tempNoiseOffset1;
    private ManagerTask<Vector3> _tempNoiseOffset2;

    private ManagerTask<Vector3> _rainfallNoiseOffset1;
    private ManagerTask<Vector3> _rainfallNoiseOffset2;
    private ManagerTask<Vector3> _rainfallNoiseOffset3;

    private Dictionary<string, ManagerTask<Vector3>[]> _layerNoiseOffsets = new Dictionary<string, ManagerTask<Vector3>[]>();

    private static HashSet<TerrainCell> _cellsToRegen = new HashSet<TerrainCell>();
    private static HashSet<TerrainCell> _cellsToInit = new HashSet<TerrainCell>();
    private static HashSet<TerrainCell> _cellsToInitAfterDrainageRegen = new HashSet<TerrainCell>();

    private static HashSet<TerrainCell> _cellsToDrain = new HashSet<TerrainCell>();
    private static BinaryHeap<TerrainCell> _drainageEvalHeap = new BinaryHeap<TerrainCell>(TerrainCell.CompareOriginalAltitude);
    private static HashSet<TerrainCell> _cellsToFinalizeDrainageRegen = new HashSet<TerrainCell>();

    //private OpenSimplexNoise _openSimplexNoise;

    public World()
    {
        Manager.WorldBeingLoaded = this;

        ProgressCastMethod = (value, message, reset) => { };
    }

    public World(int width, int height, int seed)
    {
        ProgressCastMethod = (value, message, reset) => { };

        Width = width;
        Height = height;
        Seed = seed;

#if DEBUG
        CurrentDate = 0; // To set custom date for debugging
#else
        CurrentDate = 0;
#endif

        MaxTimeToSkip = MaxPossibleTimeToSkip;
        EventsToHappenCount = 0;
        CellGroupCount = 0;
        PolityCount = 0;
        RegionCount = 0;
        TerrainCellAlterationListCount = 0;
    }

    /// <summary>Initializes the parameters to be used when generating a new world.</summary>
    /// <param name="justLoaded">Indicate if the world being generated is based on a loaded save file.</param>
    private void InitializeTerrainLimitsAndSettings(bool justLoaded)
    {
        _justLoaded = justLoaded;

        foreach (LayerSettings settings in LayerSettings)
        {
            LayerSettings mSettings = Manager.GetLayerSettings(settings.Id);
            settings.CopyValues(mSettings);
        }

        AltitudeScale = Manager.AltitudeScale;
        SeaLevelOffset = Manager.SeaLevelOffset;
        RiverStrength = Manager.RiverStrength;
        RainfallOffset = Manager.RainfallOffset;
        TemperatureOffset = Manager.TemperatureOffset;

        MinPossibleAltitudeWithOffset = MinPossibleAltitude - Manager.SeaLevelOffset;
        MaxPossibleAltitudeWithOffset = MaxPossibleAltitude - Manager.SeaLevelOffset;

        MinPossibleRainfallWithOffset = MinPossibleRainfall + Manager.RainfallOffset;
        MaxPossibleRainfallWithOffset = MaxPossibleRainfall + Manager.RainfallOffset;

        MinPossibleTemperatureWithOffset = MinPossibleTemperature + Manager.TemperatureOffset;
        MaxPossibleTemperatureWithOffset = MaxPossibleTemperature + Manager.TemperatureOffset;
    }

    public void StartReinitialization(float accumulatedProgress, float maxExpectedProgress)
    {
        InitializeTerrainLimitsAndSettings(false);

        _accumulatedProgress = accumulatedProgress;
        _progressIncrement = (maxExpectedProgress - _accumulatedProgress) / TerrainGenerationSteps;

        Manager.EnqueueTaskAndWait(() =>
        {
            Random.InitState(Seed);
            return true;
        });
    }

    /// <summary>
    /// Initialize the world's terrain cells and other general parameters
    /// </summary>
    public void TerrainInitialization()
    {
        MaxAltitude = float.MinValue;
        MinAltitude = float.MaxValue;

        MaxRainfall = float.MinValue;
        MinRainfall = float.MaxValue;

        MaxTemperature = float.MinValue;
        MinTemperature = float.MaxValue;

        _cellMaxSideLength = Circumference / Width;
        TerrainCell.MaxArea = _cellMaxSideLength * _cellMaxSideLength;
        TerrainCell.MaxWidth = _cellMaxSideLength;
        CellGroup.TravelWidthFactor = _cellMaxSideLength;

        TerrainCells = new TerrainCell[Width][];

        for (int i = 0; i < Width; i++)
        {
            TerrainCell[] column = new TerrainCell[Height];

            for (int j = 0; j < Height; j++)
            {
                float alpha = (j / (float)Height) * Mathf.PI;

                float cellHeight = _cellMaxSideLength;
                float cellWidth = Mathf.Sin(alpha) * _cellMaxSideLength;

                TerrainCell cell = new TerrainCell(this, i, j, cellHeight, cellWidth);

                column[j] = cell;
            }

            TerrainCells[i] = column;
        }

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                cell.InitializeNeighbors();
            }
        }

        _continentOffsets = new Vector2[NumContinents];
        _continentHeights = new float[NumContinents];
        _continentWidths = new float[NumContinents];
        _continentAltitudeOffsets = new float[NumContinents];
    }

    /// <summary>
    /// Performs the general initialization steps of a generated or loaded world
    /// </summary>
    /// <param name="accumulatedProgress">
    /// How much progress has the world generation/load has already been carried out</param>
    /// <param name="maxExpectedProgress">How much progress is expected to be completed by this process</param>
    /// <param name="justLoaded">Has the world just been loaded from a save file?</param>
    public void StartInitialization(float accumulatedProgress, float maxExpectedProgress, bool justLoaded = false)
    {
        //_openSimplexNoise = new OpenSimplexNoise(Seed);

        InitializeTerrainLimitsAndSettings(justLoaded);

        _accumulatedProgress = accumulatedProgress;
        _progressIncrement = (maxExpectedProgress - _accumulatedProgress) / TerrainGenerationSteps;

        TerrainInitialization();

        // When it's a loaded world there might be already terrain modifications that we need to set
        foreach (TerrainCellAlteration changes in TerrainCellAlterationList)
        {
            SetTerrainCellAlteration(changes);
        }

        Manager.EnqueueTaskAndWait(() =>
        {
            Random.InitState(Seed);
            return true;
        });
    }

    public void FinishInitialization()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                cell.InitializeMiscellaneous();
            }
        }
    }

    public static void ResetStaticModData()
    {
        EventGenerators = new Dictionary<string, IWorldEventGenerator>();
    }

    public static IWorldEventGenerator GetEventGenerator(string id)
    {
        if (!EventGenerators.TryGetValue(id, out IWorldEventGenerator generator))
        {
            return null;
        }

        return generator;
    }

    public List<WorldEvent> GetFilteredEventsToHappenForSerialization()
    {
        return _eventsToHappen.GetValues(FilterEventsToHappenNodeForSerialization);
    }

    public void Synchronize()
    {
        EventsToHappen = _eventsToHappen.GetValues(FilterEventsToHappenNodeForSerialization, FilterEventsToHappenNodeEffect, true);
        SerializedEventCount = EventsToHappen.Count;

#if DEBUG
        Dictionary<System.Type, int> eventTypes = new Dictionary<System.Type, int>();
#endif

        foreach (WorldEvent e in EventsToHappen)
        {
#if DEBUG
            System.Type type = e.GetType();

            if (!eventTypes.ContainsKey(type))
            {
                eventTypes.Add(type, 1);
            }
            else
            {
                eventTypes[type]++;
            }
#endif

            e.Synchronize();
        }

#if DEBUG
        string debugMsg = "Total Groups: " + _cellGroups.Count + "\nSerialized event types:";

        foreach (KeyValuePair<System.Type, int> pair in eventTypes)
        {
            debugMsg += "\n\t" + pair.Key + " : " + pair.Value;
        }

        Debug.Log(debugMsg);
#endif

        CellGroups = new List<CellGroup>(_cellGroups.Values);

        foreach (CellGroup g in CellGroups)
        {
            g.Synchronize();
        }

        MemorableAgents = new List<Agent>(_memorableAgents.Values);

        foreach (Agent a in MemorableAgents)
        {
            a.Synchronize();
        }

        foreach (FactionInfo f in _factionInfos.Values)
        {
            f.Synchronize();
        }

        foreach (PolityInfo p in _polityInfos.Values)
        {
            p.Synchronize();
        }

        foreach (RegionInfo r in _regionInfos.Values)
        {
            r.Synchronize();
        }

        foreach (Language l in _languages.Values)
        {
            l.Synchronize();
        }

        TerrainCellAlterationList.Clear();
        TerrainCellAlterationListCount = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                GetTerrainCellAlteration(cell);
            }
        }

        EventMessageIds = new List<long>(_eventMessageIds);

        Languages = new List<Language>(_languages.Values);

        // Reload languages to make sure they are in the same order as in the save file
        _languages.Clear();
        LoadLanguages();

        RegionInfos = new List<RegionInfo>(_regionInfos.Values);
        PolityInfos = new List<PolityInfo>(_polityInfos.Values);
        FactionInfos = new List<FactionInfo>(_factionInfos.Values);

        // Reload infos to make sure they appear in the same order as in the save file
        _regionInfos.Clear();
        _polityInfos.Clear();
        _factionInfos.Clear();

        LoadRegionInfos();
        LoadPolityInfos();
        LoadFactionInfos();
    }

    public void GetTerrainCellAlteration(TerrainCell cell)
    {
        TerrainCellAlteration Alteration = cell.GetAlteration();

        if (Alteration == null)
            return;

        TerrainCellAlterationList.Add(Alteration);

        TerrainCellAlterationListCount++;
    }

    public TerrainCell SetTerrainCellAlteration(TerrainCellAlteration alteration)
    {
        TerrainCell cell = TerrainCells[alteration.Longitude][alteration.Latitude];

        cell.SetAlteration(alteration);

        return cell;
    }

    public void SetTerrainCellAlterationAndFinishRegenCell(TerrainCellAlteration alteration)
    {
        TerrainCell cell = SetTerrainCellAlteration(alteration);

        CalculateTerrainHilliness(cell);
        GenerateTerrainLayers(cell);
        GenerateTerrainBiomes(cell);
        CalculateTerrainArability(cell);

        cell.InitializeMiscellaneous();

        Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);

        Manager.ResetSlantsAround(cell);

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            Manager.AddUpdatedCell(nCell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
        }
    }

    public void SetTerrainCellLayerDataAndFinishRegenCell(WorldPosition position, string layerId, CellLayerData data)
    {
        TerrainCell cell = TerrainCells[position.Longitude][position.Latitude];

        if (data != null)
        {
            cell.SetLayerData(Layer.Layers[layerId], data.Value, data.Offset);
        }
        else
        {
            cell.ResetLayerData(Layer.Layers[layerId]);
        }

        GenerateTerrainBiomes(cell);
        CalculateTerrainArability(cell);

        cell.InitializeMiscellaneous();

        Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
    }

    public void AddExistingCulturalPreferenceInfo(CulturalPreferenceInfo baseInfo)
    {
        if (_culturalPreferenceIdList.Contains(baseInfo.Id))
            return;

        CulturalPreferenceInfoList.Add(new CulturalPreferenceInfo(baseInfo));
        _culturalPreferenceIdList.Add(baseInfo.Id);
    }

    public void AddExistingCulturalActivityInfo(CulturalActivityInfo baseInfo)
    {
        if (_culturalActivityIdList.Contains(baseInfo.Id))
            return;

        CulturalActivityInfoList.Add(new CulturalActivityInfo(baseInfo));
        _culturalActivityIdList.Add(baseInfo.Id);
    }

    public void AddExistingCulturalSkillInfo(CulturalSkillInfo baseInfo)
    {
        if (_culturalSkillIdList.Contains(baseInfo.Id))
            return;

        CulturalSkillInfoList.Add(new CulturalSkillInfo(baseInfo));
        _culturalSkillIdList.Add(baseInfo.Id);
    }

    public void AddExistingCulturalKnowledgeInfo(CulturalKnowledgeInfo baseInfo)
    {
        if (_culturalKnowledgeIdList.Contains(baseInfo.Id))
            return;

        CulturalKnowledgeInfoList.Add(new CulturalKnowledgeInfo(baseInfo));
        _culturalKnowledgeIdList.Add(baseInfo.Id);
    }

    public void AddExistingDiscovery(Discovery discovery)
    {
        if (ExistingDiscoveries.ContainsKey(discovery.Id))
            return;

        ExistingDiscoveryIds.Add(discovery.Id);
        ExistingDiscoveries.Add(discovery.Id, discovery);
    }

    public void UpdateMostPopulousGroup(CellGroup contenderGroup)
    {
        if (MostPopulousGroup == null)
        {
            MostPopulousGroup = contenderGroup;
        }
        else if (MostPopulousGroup.Population < contenderGroup.Population)
        {
            MostPopulousGroup = contenderGroup;
        }
    }

    public void AddUpdatedGroup(CellGroup group)
    {
        _updatedGroups.Add(group);
    }

    public void AddGroupToPostUpdate_AfterPolityUpdate(CellGroup group)
    {
        _groupsToPostUpdate_afterPolityUpdates.Add(group);
    }

    public void AddGroupToCleanupAfterUpdate(CellGroup group)
    {
        _groupsToCleanupAfterUpdate.Add(group);
    }

    public TerrainCell GetCell(WorldPosition position)
    {
        return GetCell(position.Longitude, position.Latitude);
    }

    public TerrainCell GetCell(int longitude, int latitude)
    {
        if ((longitude < 0) || (longitude >= Width))
            return null;

        if ((latitude < 0) || (latitude >= Height))
            return null;

        return TerrainCells[longitude][latitude];
    }

    public TerrainCell GetCellWithSphericalWrap(int longitude, int latitude)
    {
        if (latitude < 0)
        {
            latitude = -latitude - 1;
            longitude += Width / 2;
        }
        else if (latitude >= Height)
        {
            latitude = 2 * Height - latitude - 1;
            longitude += Width / 2;
        }

        longitude = (longitude + Width) % Width;

        return TerrainCells[longitude][latitude];
    }

    public void SetMaxTimeToSkip(long value)
    {
        MaxTimeToSkip = (value > 1) ? value : 1;

        long maxDate = CurrentDate + MaxTimeToSkip;

        if (maxDate >= MaxSupportedDate)
        {
            Debug.LogWarning("World.SetMaxTimeToSkip - 'maxDate' is greater than " + MaxSupportedDate + " (date = " + maxDate + ")");
        }

        if (maxDate < 0)
        {
            throw new System.Exception("Surpassed date limit (Int64.MaxValue)");
        }

        _dateToSkipTo = (_dateToSkipTo < maxDate) ? _dateToSkipTo : maxDate;
    }

    private bool ValidateEventsToHappenNode(BinaryTreeNode<long, WorldEvent> node)
    {
        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((node.Value.Id == 160349336613603015) || (node.Value.Id == 160349354613603010))
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("ValidateEventsToHappenNode:node.Value - Id: " + node.Value.Id,
        //                    "node.Value.TriggerDate: " + node.Value.TriggerDate +
        //                    ", event type: " + node.Value.GetType() +
        //                    ", event spawn date: " + node.Value.SpawnDate +
        //                    ", node.Valid: " + node.Valid +
        //                    ", node.Value.IsStillValid (): " + node.Value.IsStillValid() +
        //                    ", node.Key: " + node.Key +
        //                    ", current date: " + CurrentDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (!node.Valid)
        {
            node.MarkedForRemoval = true;
            return false;
        }

        if (!node.Value.IsStillValid())
        {
            node.MarkedForRemoval = true;
            return false;
        }

        if (node.Key != node.Value.TriggerDate)
        {
            node.MarkedForRemoval = true;
            return false;
        }

        return true;
    }

    private bool FilterEventsToHappenNodeForSerialization(BinaryTreeNode<long, WorldEvent> node)
    {
        if (ValidateEventsToHappenNode(node))
        {
            return !node.Value.DoNotSerialize;
        }

        return false;
    }

    private void InvalidEventsToHappenNodeEffect(BinaryTreeNode<long, WorldEvent> node)
    {
        EventsToHappenCount--;

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Removal", "Removal");
        //
        //			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //		}
        //		#endif
    }

    private void FilterEventsToHappenNodeEffect(BinaryTreeNode<long, WorldEvent> node)
    {
        if (node.MarkedForRemoval)
        {
            InvalidEventsToHappenNodeEffect(node);
        }
    }

    private void UpdateGroups()
    {
        GroupsHaveBeenUpdated = true;

        foreach (CellGroup group in _groupsToUpdate)
        {
            group.Update();
        }

        _groupsToUpdate.Clear();
    }

    /// <summary>
    /// Performs the migration actions over all bands migrating during this iteration
    /// </summary>
    private void MigrateBands()
    {
        foreach (MigratingUnorganizedBands bands in _migratingBands)
        {
            bands.SplitFromSourceGroup();
        }

        foreach (MigratingUnorganizedBands bands in _migratingBands)
        {
            bands.MoveToCell();
        }

        _migratingBands.Clear();
    }

    private void PostUpdateGroups_BeforePolityUpdates()
    {
        // ops in step 1 need to be executed for all updated groups before
        // any op in step 2 is made
        foreach (CellGroup group in _updatedGroups)
        {
            group.PostUpdate_BeforePolityUpdates_Step1();
        }

        // ops in step 2 need to be executed after all ops in step 1
        // have been made for all updated groups
        foreach (CellGroup group in _updatedGroups)
        {
            group.PostUpdate_BeforePolityUpdates_Step2();
        }
    }

    private void ExecuteDeferredEffectsOnGroups()
    {
        foreach (CellGroup group in _updatedGroups)
        {
            group.ExecuteDeferredEffects();
        }
    }

    private void RemoveGroups()
    {
        foreach (CellGroup group in _groupsToRemove)
        {
            group.Destroy();
        }

        _groupsToRemove.Clear();
    }

    private void SetNextGroupUpdates()
    {
        foreach (CellGroup group in _updatedGroups)
        {
            group.SetupForNextUpdate();
        }

        _updatedGroups.Clear();
    }

    private void PostUpdateGroups_AfterPolityUpdates() // This function takes care of groups afected by polity updates
    {
        foreach (CellGroup group in _groupsToPostUpdate_afterPolityUpdates)
        {
            group.PostUpdate_AfterPolityUpdates();
        }

        _groupsToPostUpdate_afterPolityUpdates.Clear();
    }

    private void AfterUpdateGroupCleanup() // This function cleans up flags and other properties of cell groups set by events or faction/polity updates
    {
        foreach (CellGroup group in _groupsToCleanupAfterUpdate)
        {
            group.AfterUpdateCleanup();
        }

        _groupsToCleanupAfterUpdate.Clear();
    }

    private void SplitFactions()
    {
        foreach (Faction faction in _factionsToSplit)
        {
            faction.Split();
        }

        _factionsToSplit.Clear();
    }

    private void UpdateFactions()
    {
        FactionsHaveBeenUpdated = true;

        foreach (Faction faction in _factionsToUpdate)
        {
            faction.Update();
        }

        _factionsToUpdate.Clear();
    }

    private void RemoveFactions()
    {
        foreach (Faction faction in _factionsToRemove)
        {
            faction.Destroy();
        }

        _factionsToRemove.Clear();
    }

    private void UpdatePolities()
    {
        PolitiesHaveBeenUpdated = true;

        foreach (Polity polity in _politiesToUpdate)
        {
            polity.Update();
        }

        _politiesToUpdate.Clear();
    }

    private void UpdatePolityClusters()
    {
        PolityClustersHaveBeenUpdated = true;

        foreach (Polity polity in _politiesThatNeedClusterUpdate)
        {
            if (!polity.StillPresent)
                continue;

            polity.ClusterUpdate();
        }

        _politiesThatNeedClusterUpdate.Clear();
    }

    private void RemovePolities()
    {
        foreach (Polity polity in _politiesToRemove)
        {
            polity.Destroy();
        }

        _politiesToRemove.Clear();
    }

    public long Iterate()
    {
        EvaluateEventsToHappen();

        return Update();
    }

    public bool EvaluateEventsToHappen()
    {
        if (CellGroupCount <= 0)
            return false;

        //
        // Evaluate Events that will happen at the current date
        //

        _dateToSkipTo = CurrentDate + 1;

        Profiler.BeginSample("Evaluate Events");

        while (true)
        {
            //if (_eventsToHappen.Count <= 0) break;

            _eventsToHappen.FindLeftmost(ValidateEventsToHappenNode, InvalidEventsToHappenNodeEffect);

            // FindLeftMost() might have removed events so we need to check if there are events to happen left
            if (_eventsToHappen.Count <= 0) break;

            WorldEvent eventToHappen = _eventsToHappen.Leftmost;

            if (eventToHappen.TriggerDate < CurrentDate)
            {
                throw new System.Exception("World.EvaluateEventsToHappen - eventToHappen.TriggerDate (" + eventToHappen.TriggerDate +
                    ") less than CurrentDate (" + CurrentDate + "), eventToHappen: " + eventToHappen);
            }
            else if (eventToHappen.TriggerDate > MaxSupportedDate)
            {
                throw new System.Exception("World.EvaluateEventsToHappen - eventToHappen.TriggerDate (" + eventToHappen.TriggerDate +
                    ") greater than MaxSupportedDate (" + MaxSupportedDate + "), eventToHappen: " + eventToHappen);
            }

            if (eventToHappen.TriggerDate > CurrentDate)
            {
#if DEBUG
                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
                {
                    string message = "TriggerDate: " + eventToHappen.TriggerDate +
                        ", event type: " + eventToHappen.GetType();

                    message += ", current date: " + CurrentDate;

                    SaveLoadTest.DebugMessage debugMessage =
                        new SaveLoadTest.DebugMessage("EvaluateEventsToHappen.eventToHappen - Id: " + eventToHappen.Id, message);

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
#endif

                long maxDate = CurrentDate + MaxTimeToSkip;

                if (maxDate >= MaxSupportedDate)
                {
                    Debug.LogWarning("World.EvaluateEventsToHappen - 'maxDate' is greater than " + MaxSupportedDate + " (date = " + maxDate + ")");
                }

                if (maxDate < 0)
                {
                    throw new System.Exception("Surpassed date limit (Int64.MaxValue)");
                }

                _dateToSkipTo = (eventToHappen.TriggerDate < maxDate) ? eventToHappen.TriggerDate : maxDate;
                break;
            }

            _eventsToHappen.RemoveLeftmost();
            EventsToHappenCount--;

            //#if DEBUG
            //            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //            {
            //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Being Triggered", "Triggering");

            //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //            }
            //#endif

#if DEBUG
            //string eventTypeName = eventToHappen.GetType().ToString();

            Profiler.BeginSample("Event CanTrigger");
            //Profiler.BeginSample("Event CanTrigger - " + eventTypeName);
#endif

            bool canTrigger = eventToHappen.CanTrigger();

#if DEBUG
            //Profiler.EndSample();
            Profiler.EndSample();
#endif

            if (canTrigger)
            {
                _eventsToHappenNow.Add(eventToHappen);
            }
            else
            {
                eventToHappen.FailedToTrigger = true;
                eventToHappen.Destroy();
            }
        }

        foreach (WorldEvent eventToHappen in _eventsToHappenNow)
        {
#if DEBUG
            string eventTypeName = eventToHappen.GetType().ToString();

            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "EvaluateEventsToHappen eventToHappen.Id: " + eventToHappen.Id + ", eventTypeName: " + eventTypeName,
                    "eventToHappen.SpawnDate: " + eventToHappen.SpawnDate, CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }

            Profiler.BeginSample("Event Trigger");
            Profiler.BeginSample("Event Trigger - " + eventTypeName);
#endif

            eventToHappen.Trigger();

            if (Manager.DebugModeEnabled)
            {
                EventsTriggered++;
            }

#if DEBUG
            Profiler.EndSample();
            Profiler.EndSample();
#endif
            eventToHappen.Destroy();
        }

        _eventsToHappenNow.Clear();

        Profiler.EndSample();

        return true;
    }

    public long Update()
    {
        if (CellGroupCount <= 0)
            return 0;

        Profiler.BeginSample("UpdateGroups");

        UpdateGroups();

        Profiler.EndSample();

        Profiler.BeginSample("MigrateBands");

        MigrateBands();

        Profiler.EndSample();

        Profiler.BeginSample("PostUpdateGroups_BeforePolityUpdates");

        PostUpdateGroups_BeforePolityUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("ExecuteDeferredEffectsOnGroups");

        ExecuteDeferredEffectsOnGroups();

        Profiler.EndSample();

        Profiler.BeginSample("RemoveGroups");

        RemoveGroups();

        Profiler.EndSample();

        Profiler.BeginSample("UpdatePolityClusters");

        UpdatePolityClusters();

        Profiler.EndSample();

        Profiler.BeginSample("SetNextGroupUpdates");

        SetNextGroupUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("SplitFactions");

        SplitFactions();

        Profiler.EndSample();

        Profiler.BeginSample("RemoveFactions");

        RemoveFactions();

        Profiler.EndSample();

        Profiler.BeginSample("UpdateFactions");

        UpdateFactions();

        Profiler.EndSample();

        Profiler.BeginSample("UpdatePolities");

        UpdatePolities();

        Profiler.EndSample();

        Profiler.BeginSample("RemovePolities");

        RemovePolities();

        Profiler.EndSample();

        Profiler.BeginSample("PostUpdateGroups_AfterPolityUpdates");

        PostUpdateGroups_AfterPolityUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("AfterUpdateGroupCleanup");

        AfterUpdateGroupCleanup();

        Profiler.EndSample();

        //
        // Skip to Next Event's Date
        //

        if (_eventsToHappen.Count > 0)
        {
            WorldEvent futureEventToHappen = _eventsToHappen.Leftmost;

            if (futureEventToHappen.TriggerDate < _dateToSkipTo)
            {
#if DEBUG
                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
                {
                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                        "Update:futureEventToHappen - Id: " + futureEventToHappen.Id,
                        "TriggerDate: " + futureEventToHappen.TriggerDate +
                        ", event type: " + futureEventToHappen.GetType() +
                        //", event spawn date: " + futureEventToHappen.SpawnDate +
                        //", datespan: " + (futureEventToHappen.TriggerDate - futureEventToHappen.SpawnDate) +
                        ", current date: " + CurrentDate +
                        "");

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
#endif

                _dateToSkipTo = futureEventToHappen.TriggerDate;

                if (futureEventToHappen.TriggerDate <= 0)
                {
                    throw new System.Exception(
                        "Update - futureEventToHappen.TriggerDate less than or equal to 0: " +
                        futureEventToHappen.TriggerDate + ", futureEventToHappen: " + futureEventToHappen);
                }
            }
        }

        long dateSpan = _dateToSkipTo - CurrentDate;

        CurrentDate = _dateToSkipTo;

        if (CurrentDate <= 0)
        {
            throw new System.Exception("Update - CurrentDate less than or equal to 0: " + CurrentDate +
                ", _dateToSkipTo: " + _dateToSkipTo);
        }
        else if (CurrentDate > MaxSupportedDate)
        {
            throw new System.Exception("World.Update - CurrentDate (" + CurrentDate +
                ") greater than MaxSupportedDate (" + MaxSupportedDate + "). You have reached and unnoficial end of the game...");
        }

        // reset update flags
        GroupsHaveBeenUpdated = false;
        FactionsHaveBeenUpdated = false;
        PolitiesHaveBeenUpdated = false;
        PolityClustersHaveBeenUpdated = false;

        return dateSpan;
    }

    public List<WorldEvent> GetEventsToHappen()
    {
        return _eventsToHappen.Values;
    }

    public void InsertEventToHappen(WorldEvent eventToHappen)
    {
        //		Profiler.BeginSample ("Insert Event To Happen");

        EventsToHappenCount++;

        _eventsToHappen.Insert(eventToHappen.TriggerDate, eventToHappen, eventToHappen.AssociateNode);

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Event Added - Id: " + eventToHappen.Id, "TriggerDate: " + eventToHappen.TriggerDate);
        //
        //			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //		}
        //		#endif

        //		Profiler.EndSample ();
    }

#if DEBUG
    public delegate void AddMigratingBandsCalledDelegate();
    public static AddMigratingBandsCalledDelegate AddMigratingBandsCalled = null;
#endif

    /// <summary>
    /// Adds a group of migrating bands to migrate during this iteration
    /// </summary>
    /// <param name="bands">group of bands to add</param>
    public void AddMigratingBands(MigratingUnorganizedBands bands)
    {
#if DEBUG
        if (AddMigratingBandsCalled != null)
        {
            AddMigratingBandsCalled();
        }
#endif

        _migratingBands.Add(bands);

        if (!bands.SourceGroup.StillPresent)
        {
            Debug.LogWarning("Sourcegroup is no longer present. Group Id: " + bands.SourceGroup);
        }

        // Source Group needs to be updated
        AddGroupToUpdate(bands.SourceGroup);

        // If Target Group is present, it also needs to be updated
        if ((bands.TargetCell.Group != null) && (bands.TargetCell.Group.StillPresent))
        {
            AddGroupToUpdate(bands.TargetCell.Group);
        }
    }

    public void AddGroup(CellGroup group)
    {
        _cellGroups.Add(group.Id, group);

        Manager.AddUpdatedCell(group.Cell, CellUpdateType.GroupTerritoryClusterAndLanguage, CellUpdateSubType.AllButTerrain);

        CellGroupCount++;
    }

    public void RemoveGroup(CellGroup group)
    {
        _cellGroups.Remove(group.Id);

        Manager.AddUpdatedCell(group.Cell, CellUpdateType.GroupTerritoryClusterAndLanguage, CellUpdateSubType.AllButTerrain);

        CellGroupCount--;
    }

    public CellGroup GetGroup(Identifier id)
    {
        CellGroup group;

        _cellGroups.TryGetValue(id, out group);

        return group;
    }

#if DEBUG
    public delegate void AddGroupToUpdateCalledDelegate(string callingMethod);

    public static AddGroupToUpdateCalledDelegate AddGroupToUpdateCalled = null;
#endif

    public void AddGroupToUpdate(CellGroup group)
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 1))
        {
            if (group.Id == Manager.TracingData.GroupId)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string callingMethod = method.Name;
                string callingClass = method.DeclaringType.ToString();

                method = stackTrace.GetFrame(2).GetMethod();
                string callingMethod2 = method.Name;
                string callingClass2 = method.DeclaringType.ToString();

                string groupId = "Id: " + group + " Pos: " + group.Position;

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "AddGroupToUpdate - Group:" + groupId,
                    "CurrentDate: " + CurrentDate +
                    ", Call 1: " + callingClass + ":" + callingMethod +
                    ", Call 2: " + callingClass2 + ":" + callingMethod2 +
                    "", CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }

        if (AddGroupToUpdateCalled != null)
        {
            //System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

            //System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
            //string callingMethod = method.Name;
            //string callingClass = method.DeclaringType.ToString();

            //AddGroupToUpdateCalled(callingClass + ":" + callingMethod);
            AddGroupToUpdateCalled(null);
        }
#endif

        if (GroupsHaveBeenUpdated)
        {
            Debug.LogWarning(
                "Trying to add group to update after groups have already been updated this iteration. Id: " +
                group);
        }

        if (!group.StillPresent)
        {
            Debug.LogWarning("Group to update is no longer present. Id: " + group);
        }

        _groupsToUpdate.Add(group);
    }

    public void AddGroupToRemove(CellGroup group)
    {
        _groupsToRemove.Add(group);
    }

    public void AddLanguage(Language language)
    {
        _languages.Add(language.Id, language);

        LanguageCount++;
    }

    public void RemoveLanguage(Language language)
    {
        _languages.Remove(language.Id);

        LanguageCount--;
    }

    public Language GetLanguage(Identifier id)
    {
        _languages.TryGetValue(id, out Language language);

        return language;
    }

    public void AddRegionInfo(RegionInfo regionInfo)
    {
        _regionInfos.Add(regionInfo.Id, regionInfo);

        RegionCount++;
    }

    public RegionInfo GetRegionInfo(Identifier id)
    {
        RegionInfo regionInfo;

        _regionInfos.TryGetValue(id, out regionInfo);

        return regionInfo;
    }

    public void AddMemorableAgent(Agent agent)
    {
        if (!_memorableAgents.ContainsKey(agent.Id))
        {
            _memorableAgents.Add(agent.Id, agent);

            MemorableAgentCount++;
        }
    }

    public Agent GetMemorableAgent(Identifier id)
    {
        Agent agent;

        _memorableAgents.TryGetValue(id, out agent);

        return agent;
    }

    public void AddFactionInfo(FactionInfo factionInfo)
    {
        _factionInfos.Add(factionInfo.Id, factionInfo);

        FactionCount++;
    }

    public FactionInfo GetFactionInfo(Identifier id)
    {
        FactionInfo factionInfo = null;

        _factionInfos.TryGetValue(id, out factionInfo);

        return factionInfo;
    }

    public Faction GetFaction(Identifier id)
    {
        FactionInfo factionInfo;

        if (!_factionInfos.TryGetValue(id, out factionInfo))
        {
            return null;
        }

        return factionInfo.Faction;
    }

    public bool ContainsFactionInfo(Identifier id)
    {
        return _factionInfos.ContainsKey(id);
    }

    [System.Obsolete]
    public void AddFactionToSplit(Faction faction)
    {
        if (!faction.StillPresent)
        {
            Debug.LogWarning("Faction to split no longer present. Id: " + faction.Id + ", Date: " + CurrentDate);
        }

        _factionsToSplit.Add(faction);
    }

    public void AddFactionToUpdate(Faction faction)
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 1))
        {
            if (Manager.TracingData.FactionId == faction.Id)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string callingMethod = method.Name;

                int frame = 2;
                while (callingMethod.Contains("SetFactionUpdates")
                    || callingMethod.Contains("SetToUpdate"))
                {
                    method = stackTrace.GetFrame(frame).GetMethod();
                    callingMethod = method.Name;

                    frame++;
                }

                string callingClass = method.DeclaringType.ToString();

                faction.Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out int knowledgeValue);

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "World:AddFactionToUpdate - Faction Id:" + faction.Id,
                    "CurrentDate: " + CurrentDate +
                    ", Social organization knowledge value: " + knowledgeValue +
                    ", Calling method: " + callingClass + "." + callingMethod +
                    "", CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (FactionsHaveBeenUpdated)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

            Debug.LogWarning(
                "Trying to add faction to update after factions have already been updated this iteration. Id: " +
                faction.Id + ", stackTrace:\n" + stackTrace);
        }

        if (!faction.StillPresent)
        {
            Debug.LogWarning("Faction to update no longer present. Id: " + faction.Id + ", Date: " + CurrentDate);
        }

        _factionsToUpdate.Add(faction);
    }

    public void AddFactionToRemove(Faction faction)
    {
        _factionsToRemove.Add(faction);
    }

    public void AddPolityInfo(Polity polity)
    {
        _polityInfos.Add(polity.Id, polity.Info);

        PolityCount++;
    }

    public void AddPolityInfo(PolityInfo info)
    {
        _polityInfos.Add(info.Id, info);

        PolityCount++;
    }

    public ICollection<PolityInfo> GetPolityInfos()
    {
        return _polityInfos.Values;
    }

    public PolityInfo GetPolityInfo(Identifier id)
    {
        if (!_polityInfos.TryGetValue(id, out PolityInfo polityInfo))
        {
            return null;
        }

        return polityInfo;
    }

    public Polity GetPolity(Identifier id)
    {
        if (!_polityInfos.TryGetValue(id, out PolityInfo polityInfo))
        {
            return null;
        }

        return polityInfo.Polity;
    }

    public void AddPolityToUpdate(Polity polity)
    {
#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 1))
        {
            if (polity.Id == Manager.TracingData.PolityId)
            {
                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string callingMethod = method.Name;

                //				int frame = 2;
                //				while (callingMethod.Contains ("GetNextLocalRandom") || callingMethod.Contains ("GetNextRandom")) {
                //					method = stackTrace.GetFrame(frame).GetMethod();
                //					callingMethod = method.Name;
                //
                //					frame++;
                //				}

                string callingClass = method.DeclaringType.ToString();

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "AddPolityToUpdate - Polity:" + polity.Id,
                    "CurrentDate: " + CurrentDate +
                    ", caller: " + callingClass + "::" + callingMethod +
                    "", CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (PolitiesHaveBeenUpdated)
        {
            Debug.LogWarning("Trying to add polity to update after polities have already been updated this iteration. Id: " + polity.Id);
        }

        if (!polity.StillPresent)
        {
            Debug.LogWarning("Polity to update no longer present. Id: " + polity.Id + ", Date: " + CurrentDate);
        }

        _politiesToUpdate.Add(polity);
        polity.WillBeUpdated = true;
    }

    public void AddPolityThatNeedsClusterUpdate(Polity polity)
    {
        if (PolityClustersHaveBeenUpdated)
        {
            Debug.LogWarning("Trying to add polity with clusters to update after polity clusters have already been updated this iteration. Id: " + polity.Id);
        }

        if (!polity.StillPresent)
        {
            Debug.LogWarning("Polity with clusters to update no longer present. Id: " + polity.Id + ", Date: " + CurrentDate);
        }

        _politiesThatNeedClusterUpdate.Add(polity);
    }

    public void AddPolityToRemove(Polity polity)
    {
        _politiesToRemove.Add(polity);
    }

    [System.Obsolete]
    public void AddDecisionToResolve(Decision decision)
    {
        _decisionsToResolve.Enqueue(decision);
    }

    public void AddDecisionToResolve(ModDecision decision)
    {
        _modDecisionsToResolve.Enqueue(decision);
    }

    [System.Obsolete]
    public bool HasDecisionsToResolve()
    {
        return _decisionsToResolve.Count > 0;
    }

    public bool HasModDecisionsToResolve()
    {
        return _modDecisionsToResolve.Count > 0;
    }

    [System.Obsolete]
    public Decision PullDecisionToResolve()
    {
        return _decisionsToResolve.Dequeue();
    }

    public ModDecision PullModDecisionToResolve()
    {
        return _modDecisionsToResolve.Dequeue();
    }

    public void AddEventMessage(WorldEventMessage eventMessage)
    {
        _eventMessagesToShow.Enqueue(eventMessage);

        _eventMessageIds.Add(eventMessage.Id);
    }

    public void AddEventMessageToShow(WorldEventMessage eventMessage)
    {
        if (_eventMessagesToShow.Contains(eventMessage))
            return;

        _eventMessagesToShow.Enqueue(eventMessage);
    }

    public bool HasEventMessage(long id)
    {
        return _eventMessageIds.Contains(id);
    }

    public WorldEventMessage GetNextMessageToShow()
    {
        return _eventMessagesToShow.Dequeue();
    }

    public int EventMessagesLeftToShow()
    {
        return _eventMessagesToShow.Count;
    }

    private void LoadRegionInfos()
    {
        foreach (RegionInfo rInfo in RegionInfos)
        {
            _regionInfos.Add(rInfo.Id, rInfo);
        }

#if DEBUG
        Debug.Log("Loaded Region Infos: " + RegionInfos.Count);
#endif
    }

    private void LoadPolityInfos()
    {
        foreach (PolityInfo pInfo in PolityInfos)
        {
            _polityInfos.Add(pInfo.Id, pInfo);
        }

#if DEBUG
        Debug.Log("Loaded Polity Infos: " + PolityInfos.Count);
#endif
    }

    private void LoadFactionInfos()
    {
        foreach (FactionInfo fInfo in FactionInfos)
        {
            _factionInfos.Add(fInfo.Id, fInfo);
        }

#if DEBUG
        Debug.Log("Loaded Faction Infos: " + FactionInfos.Count);
#endif
    }

    private void LoadLanguages()
    {
        foreach (Language language in Languages)
        {
            _languages.Add(language.Id, language);
        }

#if DEBUG
        Debug.Log("Loaded Languages: " + Languages.Count);
#endif
    }

    public void FinalizeLoad(float startProgressValue, float endProgressValue, ProgressCastDelegate castProgress)
    {
        if (castProgress == null)
            castProgress = (value, message, reset) => { };

        float progressFactor = 1 / (endProgressValue - startProgressValue);

        LoadRegionInfos();
        LoadPolityInfos();
        LoadFactionInfos();
        LoadLanguages();

        // Segment 1

        foreach (long messageId in EventMessageIds)
        {
            _eventMessageIds.Add(messageId);
        }

        foreach (RegionInfo r in _regionInfos.Values)
        {
            r.World = this;
        }

        foreach (PolityInfo pInfo in _polityInfos.Values)
        {
            if (pInfo.Polity != null)
            {
                pInfo.Polity.Info = pInfo;
                pInfo.Polity.World = this;

                if (pInfo.Polity.IsUnderPlayerFocus)
                {
                    PolitiesUnderPlayerFocus.Add(pInfo.Polity);
                }
            }
        }

        foreach (FactionInfo fInfo in _factionInfos.Values)
        {
            if (fInfo.Faction != null)
            {
                fInfo.Faction.Info = fInfo;
                fInfo.Faction.World = this;

                if (fInfo.Faction.IsUnderPlayerGuidance)
                {
                    GuidedFaction = fInfo.Faction;
                }
            }
        }

        foreach (CellGroup group in CellGroups)
        {
            group.World = this;
            group.PrefinalizeLoad();

            _cellGroups.Add(group.Id, group);
        }

        // Segment 2

        int elementCount = 0;
        float totalElementsFactor = progressFactor * (Languages.Count + _regionInfos.Count + _factionInfos.Count + _polityInfos.Count + CellGroups.Count + EventsToHappen.Count);

        foreach (Language l in Languages)
        {
            l.FinalizeLoad();

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Languages...");
        }

        // Segment 3

        foreach (RegionInfo r in _regionInfos.Values)
        {
            r.FinalizeLoad();

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Regions...");
        }

        // Segment 4

        foreach (PolityInfo pInfo in _polityInfos.Values)
        {
            pInfo.FinalizeLoad();

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Polities...");
        }

        // Segment 5

        foreach (FactionInfo fInfo in _factionInfos.Values)
        {
            fInfo.FinalizeLoad();

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Factions...");
        }

        // Segment 6

#if DEBUG
        CellGroup.Debug_LoadedGroups = 0;
#endif

        foreach (CellGroup g in _cellGroups.Values)
        {
            g.FinalizeLoad();

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Cell Groups...");
        }

        // Segment 7

        foreach (WorldEvent e in EventsToHappen)
        {
            e.World = this;
            e.FinalizeLoad();

            InsertEventToHappen(e);
            //			_eventsToHappen.Insert (e.TriggerDate, e);

            castProgress(startProgressValue + (++elementCount / totalElementsFactor), "Initializing Events...");
        }

        // Segment 8

        foreach (CulturalPreferenceInfo p in CulturalPreferenceInfoList)
        {
            p.FinalizeLoad();
            _culturalPreferenceIdList.Add(p.Id);
        }

        foreach (CulturalActivityInfo a in CulturalActivityInfoList)
        {
            a.FinalizeLoad();
            _culturalActivityIdList.Add(a.Id);
        }

        foreach (CulturalSkillInfo s in CulturalSkillInfoList)
        {
            s.FinalizeLoad();
            _culturalSkillIdList.Add(s.Id);
        }

        foreach (CulturalKnowledgeInfo k in CulturalKnowledgeInfoList)
        {
            k.FinalizeLoad();
            _culturalKnowledgeIdList.Add(k.Id);
        }

        foreach (string id in ExistingDiscoveryIds)
        {
            Discovery discovery = Discovery.GetDiscovery(id);

            if (discovery == null)
            {
                throw new System.Exception("Unable to find existing discovery: " + id);
            }

            ExistingDiscoveries.Add(id, discovery);
        }
    }

    public void FinalizeLoad()
    {
        FinalizeLoad(0, 1, null);
    }

    public void MigrationTagGroup(CellGroup group)
    {
        MigrationUntagGroup();

        MigrationTaggedGroup = group;

        group.MigrationTagged = true;
    }

    public void MigrationUntagGroup()
    {
        if (MigrationTaggedGroup != null)
            MigrationTaggedGroup.MigrationTagged = false;
    }

    public void GenerateTerrain(GenerationType type, Texture2D heightmap)
    {
        GenerateRandomNoiseAltitudeOffsetsAndContinents();
        GenerateRandomNoiseTemperatureOffsets();
        GenerateRandomNoiseRainfallOffsets();

        if ((type & GenerationType.TerrainNormal) == GenerationType.TerrainNormal)
        {
            if (heightmap == null)
            {
                ProgressCastMethod(_accumulatedProgress, "Generating terrain altitude...");

                GenerateTerrainAltitude();
            }
            else
            {
                ProgressCastMethod(_accumulatedProgress, "Generating terrain altitude using heightmap...");

                GenerateTerrainAltitudeFromHeightmap(heightmap);
            }
        }
        else if ((type & GenerationType.TerrainRegeneration) == GenerationType.TerrainRegeneration)
        {
            ProgressCastMethod(_accumulatedProgress, "Regenerating terrain altitude...");

            RegenerateTerrainAltitude();
        }
        else
        {
            ResetToOriginalAltitudes();
        }

        if ((type & GenerationType.Temperature) == GenerationType.Temperature)
        {
            ProgressCastMethod(_accumulatedProgress, "Calculating temperatures...");

            GenerateTerrainTemperature();
        }
        else if ((type & GenerationType.TemperatureRegeneration) == GenerationType.TemperatureRegeneration)
        {
            ProgressCastMethod(_accumulatedProgress, "Recalculating temperatures...");

            RegenerateTerrainTemperature();
        }
        else
        {
            ResetToOriginalTemperatures();
        }

        if ((type & GenerationType.Rainfall) == GenerationType.Rainfall)
        {
            ProgressCastMethod(_accumulatedProgress, "Calculating rainfall...");

            ResetRainfallDependencies();
            GenerateTerrainRainfall();
        }
        else if ((type & GenerationType.RainfallRegeneration) == GenerationType.RainfallRegeneration)
        {
            ProgressCastMethod(_accumulatedProgress, "Recalculating rainfall...");

            RegenerateTerrainRainfall();
        }
        else
        {
            _accumulatedProgress += _progressIncrement;
        }

        ProgressCastMethod(_accumulatedProgress, "Generating Drainage Basins...");

        SetCellsToEvalForDrainange();

        GenerateDrainageBasins();
        //GenerateDrainageBasins(false); // repeat to simulate geological scale erosion

        _cellsToDrain.Clear();
        NeedsDrainageRegeneration = false;

        ProgressCastMethod(_accumulatedProgress, "Calculating hilliness...");

        CalculateTerrainHilliness();

        ProgressCastMethod(_accumulatedProgress, "Generating layers...");

        GenerateTerrainLayers();

        ProgressCastMethod(_accumulatedProgress, "Generating biomes...");

        GenerateTerrainBiomes();

        ProgressCastMethod(_accumulatedProgress, "Generating arability...");

        CalculateTerrainArability();

        // These rng values will be later used by the editor brushes when needed but we need to prepare them beforehand
        OffsetBrushRngCalls();
    }

    public void Generate(Texture2D heightmap)
    {
        GenerateTerrain(GenerationType.TerrainNormal, heightmap);

        ProgressCastMethod(_accumulatedProgress, "Finalizing...");
    }

    public void Regenerate(GenerationType type)
    {
        GenerateTerrain(type, null);

        ProgressCastMethod(_accumulatedProgress, "Finalizing...");
    }

    public void GenerateHumanGroup(int longitude, int latitude, int initialPopulation)
    {
        TerrainCell cell = GetCell(longitude, latitude);

        CellGroup group = new CellGroup(this, cell, initialPopulation);

        Debug.Log(string.Format("Adding population group at {0} with population: {1}", cell.Position, initialPopulation));

        AddGroup(group);
    }

    public void GenerateRandomHumanGroups(int maxGroups, int initialPopulation)
    {
        ProgressCastMethod(_accumulatedProgress, "Adding Random Human Groups...");

        int sizeX = Width;
        int sizeY = Height;

        List<TerrainCell> SuitableCells = new List<TerrainCell>();

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                if (cell.Survivability < MinSurvivabilityForRandomGroupPlacement) continue;

                SuitableCells.Add(cell);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;

        maxGroups = Mathf.Min(SuitableCells.Count, maxGroups);

        bool first = true;

        for (int i = 0; i < maxGroups; i++)
        {
            ManagerTask<int> n = GenerateRandomInteger(0, SuitableCells.Count);

            TerrainCell cell = SuitableCells[n];

            CellGroup group = new CellGroup(this, cell, initialPopulation);

            Debug.Log(string.Format("Adding random population group [{0}] at {1} with population: {2}", i, cell.Position, initialPopulation));

            AddGroup(group);

            if (first)
            {
                MigrationTagGroup(group);

                first = false;
            }
        }
    }

    private void GenerateContinents()
    {
        float longitudeFactor = 15f;
        float latitudeFactor = 6f;

        float minLatitude = Height / latitudeFactor;
        float maxLatitude = Height * (latitudeFactor - 1f) / latitudeFactor;

        Manager.EnqueueTaskAndWait(() =>
        {
            Vector2 prevPos = new Vector2(
                RandomUtility.Range(0f, Width),
                RandomUtility.Range(minLatitude, maxLatitude));

            float altitudeOffsetIncrement = 0.7f;
            float altitudeOffset = 0;

            for (int i = 0; i < NumContinents; i++)
            {
                int widthOff = Random.Range(0, 2) * 3;

                _continentOffsets[i] = prevPos;
                _continentWidths[i] = RandomUtility.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);
                _continentHeights[i] = RandomUtility.Range(ContinentMinWidthFactor + widthOff, ContinentMaxWidthFactor + widthOff);
                _continentAltitudeOffsets[i] = Mathf.Repeat(altitudeOffset + RandomUtility.Range(0f, altitudeOffsetIncrement), 1f);

                altitudeOffset = Mathf.Repeat(altitudeOffset + altitudeOffsetIncrement, 1f);

                float xPos = Mathf.Repeat(prevPos.x + RandomUtility.Range(Width / longitudeFactor, Width * 2 / longitudeFactor), Width);
                float yPos = RandomUtility.Range(minLatitude, maxLatitude);

                if (i % 3 == 2)
                {
                    xPos = Mathf.Repeat(prevPos.x + RandomUtility.Range(Width * 4 / longitudeFactor, Width * 5 / longitudeFactor), Width);
                }

                Vector2 newPos = new Vector2(xPos, yPos);

                prevPos = newPos;
            }

            return true;
        });
    }

    private float GetContinentModifier(int x, int y)
    {
        float maxValue = 0;
        float widthF = Width;

        for (int i = 0; i < NumContinents; i++)
        {
            float dist = GetContinentDistance(i, x, y);

            float value = Mathf.Clamp01(1f - dist / widthF);

            float otherValue = value;

            if (maxValue < value)
            {
                otherValue = maxValue;
                maxValue = value;
            }

            float valueMod = otherValue;
            otherValue *= 2;
            otherValue = Mathf.Clamp01(otherValue);

            maxValue = Mathf.Lerp(maxValue, otherValue, valueMod);
        }

        return maxValue;
    }

    private float GetContinentDistance(int id, int x, int y)
    {
        float betaFactor = Mathf.Sin(Mathf.PI * y / Height);

        Vector2 continentOffset = _continentOffsets[id];
        float contX = continentOffset.x;
        float contY = continentOffset.y;

        float distX = Mathf.Min(Mathf.Abs(contX - x), Mathf.Abs(Width + contX - x));
        distX = Mathf.Min(distX, Mathf.Abs(contX - x - Width));
        distX *= betaFactor;

        float distY = Mathf.Abs(contY - y);

        float continentWidth = _continentWidths[id];
        float continentHeight = _continentHeights[id];

        return MathUtility.GetMagnitude(distX * continentWidth, distY * continentHeight);
    }

    private void GenerateRandomNoiseAltitudeOffsetsAndContinents()
    {
        GenerateContinents();

        _altitudeNoiseOffset1 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset2 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset3 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset4 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset5 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset6 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset7 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset8 = GenerateRandomOffsetVectorTask();
        _altitudeNoiseOffset9 = GenerateRandomOffsetVectorTask();
        //_altitudeNoiseOffset10 = GenerateRandomOffsetVectorTask();
    }

    private void RegenerateTerrainAltitude()
    {
        GenerateRandomNoiseAltitudeOffsetsAndContinents();

        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                RecalculateAndSetAltitude(i, j);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void RegenerateTerrainTemperature()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                RecalculateAndSetTemperature(i, j);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void RegenerateTerrainRainfall()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                RecalculateAndSetRainfall(i, j);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void GenerateTerrainAltitudeFromHeightmap(Texture2D heightmap)
    {
        int sizeX = Width;
        int sizeY = Height;

        int hmWidth = 0;
        int hmHeight = 0;
        Color[] hmColors = null;

        Manager.EnqueueTaskAndWait(() =>
        {
            hmWidth = heightmap.width;
            hmHeight = heightmap.height;

            hmColors = heightmap.GetPixels();
        });

        float rowPixelsPerCell = hmWidth / (float)sizeX;
        float columnPixelsPerCell = hmHeight / (float)sizeY;

        for (int i = 0; i < sizeX; i++)
        {
            int heightmapOffsetX = (int)(rowPixelsPerCell * i);

            for (int j = 0; j < sizeY; j++)
            {
                int heightmapOffsetY = (int)(columnPixelsPerCell * j);

                int totalPixels = 0;
                float greyscaleValue = 0;

                int nextOffsetX = (int)(rowPixelsPerCell * (i + 1));
                for (int k = heightmapOffsetX; k < nextOffsetX; k++)
                {
                    int nextOffsetY = (int)(columnPixelsPerCell * (j + 1));
                    for (int l = heightmapOffsetY; l < nextOffsetY; l++)
                    {
                        totalPixels++;
                        int cIndex = k + (l * hmWidth);

                        greyscaleValue += hmColors[cIndex].grayscale;
                    }
                }

                if (totalPixels > 0)
                {
                    greyscaleValue /= totalPixels;
                }

                CalculateAndSetAltitude(i, j, greyscaleValue, true);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    public void ModifyCellAltitude(TerrainCell cell, float valueOffset, float noiseFactor = 0, float noiseRadius = 0)
    {
        if (noiseFactor > 0)
        {
            float rngValue = GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, noiseRadius, _altitudeBrushNoiseOffset);

            valueOffset *= Mathf.Lerp(1, rngValue, noiseFactor);
        }

        if (valueOffset == 0)
            return; // No actual changes being made to cell

        Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);

        // Add some extra noise to enhance river formation
        float radius6 = 64f;
        float radius7 = 128f;

        float value6 = GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, radius6, _altitudeNoiseOffset6);
        float value7 = GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, radius7, _altitudeNoiseOffset7);

        float valueOffsetA = Mathf.Lerp(valueOffset, value6 * valueOffset, 0.2f);
        valueOffsetA = Mathf.Lerp(valueOffsetA, value7 * valueOffset, 0.1f);

        float value = cell.BaseAltitudeValue + valueOffsetA;

        CalculateAndSetAltitude(cell, value, true, true);

        _cellsToRegen.Add(cell);
        _cellsToInit.Add(cell);

        // Add rainfall dependent cells that will need to be regen
        foreach (TerrainCell rCell in cell.RainfallDependentCells)
        {
            _cellsToRegen.Add(rCell);
            _cellsToInit.Add(rCell);
        }

        // Add neighboor cells that will need to be reinitialized
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            _cellsToInit.Add(nCell);
        }
    }

    /// <summary>
    /// Generates drainage basins within a set of cells.
    /// TODO: unused function. Make it work or remove
    /// </summary>
    public void PerformTerrainAlterationDrainageRegen()
    {
        while (_drainageEvalHeap.Count > 0)
        {
            TerrainCell cell = _drainageEvalHeap.Extract(false);

            if (!cell.DrainageDone)
            {
                bool reInsert = false;

                float cellAltitude = Mathf.Max(0, cell.Altitude);

                foreach (TerrainCell nCell in cell.NeighborList)
                {
                    float nCellAltitude = Mathf.Max(0, nCell.Altitude);

                    if (nCellAltitude > cellAltitude)
                    {
                        reInsert |= AddToDrainageRegen(nCell, false);
                    }
                }

                if (reInsert)
                {
                    _drainageEvalHeap.Insert(cell);
                    continue;
                }

                cell.UpdateDrainage();
            }

            DrainToNeighbors(cell, AddToDrainageRegen);
        }
    }

    /// <summary>
    /// Finishes terrain generation on cells afected by drainage regeneration and then cleanups.
    /// </summary>
    public void FinalizeTerrainAlterationDrainageRegen()
    {
        foreach (TerrainCell cell in _cellsToFinalizeDrainageRegen)
        {
            CalculateTerrainHilliness(cell);
            GenerateTerrainLayers(cell);
            GenerateTerrainBiomes(cell);
            CalculateTerrainArability(cell);
        }

        foreach (TerrainCell cell in _cellsToInitAfterDrainageRegen)
        {
            cell.InitializeMiscellaneous();

            Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
            Manager.ActiveEditorBrushAction.AddCellAfterModification(cell);
        }

        _cellsToInitAfterDrainageRegen.Clear();
        _cellsToFinalizeDrainageRegen.Clear();
        _cellsToDrain.Clear();
        _drainageEvalHeap.Clear();
    }

    /// <summary>
    /// This function will add a cell to the heap of cells that need their drainage values regenerated
    /// after applying a terrain modification editor brush.
    /// </summary>
    /// <param name="cell"> The cell to regenerate drainage for.</param>
    /// <param name="resetDrainage"> Indicate if the cells drainage values should be reset
    /// to defaults before adding to the heap.</param>
    /// <param name="resetTerrain"> Indicate if the cell's altitude and temperature should also be reset
    /// to their original values before adding to the heap.</param>
    /// <param name="callByBrush"> Indicates if this cells was added directly by effect of an editor brush.</param>
    /// <returns>
    ///   <c>true</c> if this is the first time this function has been called on this
    ///   particular cell during this regeneration cycle. Otherwise, <c>false</c>.
    /// </returns>
    public bool AddToDrainageRegen(
        TerrainCell cell,
        bool resetDrainage = true,
        bool resetTerrain = true,
        bool callByBrush = false)
    {
        bool justAdded = true;

        if (_cellsToDrain.Contains(cell))
        {
            justAdded = false;
        }
        else
        {
            _cellsToInitAfterDrainageRegen.Add(cell);
            Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);
            foreach (TerrainCell nCell in cell.NeighborList)
            {
                _cellsToInitAfterDrainageRegen.Add(nCell);
                Manager.ActiveEditorBrushAction.AddCellBeforeModification(nCell);
            }

            _cellsToDrain.Add(cell);
            _drainageEvalHeap.Insert(cell);

            if (resetDrainage)
            {
                ResetDrainage(cell, resetTerrain);

                cell.RiverId = cell.Latitude * Width + cell.Longitude;

                _cellsToFinalizeDrainageRegen.Add(cell);
            }
        }

        if ((justAdded || callByBrush) && resetDrainage)
        {
            if (!cell.IsBelowSeaLevel)
            {
                // Rainfall could have been altered after this cell had already been added to the drainageHeap,
                // so we need to update the cell's water acc accordingly.
                cell.UpdateDrainage();
            }
        }

        return justAdded;
    }

    public void FinishTerrainGenerationForModifiedCells()
    {
        foreach (TerrainCell cell in _cellsToInit)
        {
            Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);
        }

        foreach (TerrainCell cell in _cellsToRegen)
        {
            GenerateTerrainTemperature(cell);
            GenerateTerrainRainfall(cell, setDependencies: false);
            CalculateTerrainHilliness(cell);
            GenerateTerrainLayers(cell);
            GenerateTerrainBiomes(cell);
            CalculateTerrainArability(cell);

            if (!cell.IsBelowSeaLevel)
            {
                //AddToDrainageRegen(cell, callByBrush: true);
                ResetDrainage(cell, false);
            }
        }

        _cellsToRegen.Clear();

        foreach (TerrainCell cell in _cellsToInit)
        {
            cell.InitializeMiscellaneous();

            Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
            Manager.ActiveEditorBrushAction.AddCellAfterModification(cell);
        }

        _cellsToInit.Clear();
    }

    public void ModifyCellTemperature(TerrainCell cell, float valueOffset, float noiseFactor = 0, float noiseRadius = 0)
    {
        if (noiseFactor > 0)
        {
            float rngValue = GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, noiseRadius, _temperatureBrushNoiseOffset);

            valueOffset *= Mathf.Lerp(1, rngValue, noiseFactor);
        }

        if (valueOffset == 0)
            return; // No actual changes being made to cell

        // Make sure to record cell state before changes are made to it
        Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);

        CalculateAndSetTemperature(cell, cell.BaseTemperatureValue, valueOffset, true);

        GenerateTerrainLayers(cell);
        GenerateTerrainBiomes(cell);
        CalculateTerrainArability(cell);

        if (!cell.IsBelowSeaLevel)
        {
            //AddToDrainageRegen(cell, callByBrush: true);
            ResetDrainage(cell, false);
        }

        Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
        Manager.ActiveEditorBrushAction.AddCellAfterModification(cell);
    }

    public void ModifyCellRainfall(TerrainCell cell, float valueOffset, float noiseFactor = 0, float noiseRadius = 0)
    {
        if (noiseFactor > 0)
        {
            float rngValue = GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, noiseRadius, _rainfallBrushNoiseOffset);

            valueOffset *= Mathf.Lerp(1, rngValue, noiseFactor);
        }

        if (valueOffset == 0)
            return; // No actual changes being made to cell

        // Make sure to record cell state before changes are made to it
        Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);

        CalculateAndSetRainfall(cell, cell.BaseRainfallValue, valueOffset, true);

        GenerateTerrainLayers(cell);
        GenerateTerrainBiomes(cell);
        CalculateTerrainArability(cell);

        if (!cell.IsBelowSeaLevel)
        {
            //AddToDrainageRegen(cell, callByBrush: true);
            ResetDrainage(cell, false);
        }

        Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
        Manager.ActiveEditorBrushAction.AddCellAfterModification(cell);
    }

    public void ModifyCellLayerData(TerrainCell cell, float valueOffset, string layerId, float noiseFactor = 0, float noiseRadius = 0)
    {
        if (noiseFactor > 0)
        {
            float rngValue =
                GetRandomNoiseFromPolarCoordinates(cell.Alpha, cell.Beta, noiseRadius, _layerBrushNoiseOffsets[layerId]);

            valueOffset *= Mathf.Lerp(1, rngValue, noiseFactor);
        }

        if (valueOffset == 0)
            return; // No actual changes being made to cell

        // Make sure to record cell state before changes are made to it
        Manager.ActiveEditorBrushAction.AddCellBeforeModification(cell);

        CalculateAndSetTerrainLayerValue(cell, Layer.Layers[layerId], valueOffset);

        GenerateTerrainBiomes(cell);
        CalculateTerrainArability(cell);

        Manager.AddUpdatedCell(cell, CellUpdateType.Cell, CellUpdateSubType.Terrain);
        Manager.ActiveEditorBrushAction.AddCellAfterModification(cell);
    }

    private void GenerateTerrainAltitude()
    {
        int sizeX = Width;
        int sizeY = Height;

        float radius1 = 0.75f;
        float radius2 = 8f;
        float radius3 = 4f;
        float radius4 = 8f;
        float radius5 = 16f;
        float radius6 = 64f;
        float radius7 = 128f;
        float radius8 = 1.5f;
        float radius9 = 0.5f;
        //float radius10 = 4f;

        for (int i = 0; i < sizeX; i++)
        {
            float beta = (i / (float)sizeX) * Mathf.PI * 2;

            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                if (SkipIfLoaded(cell))
                {
                    CalculateAndSetAltitude(cell, cell.BaseAltitudeValue);
                    continue;
                }

                float alpha = j / (float)sizeY * Mathf.PI;

                float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, _altitudeNoiseOffset1);
                float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, _altitudeNoiseOffset2);
                float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, _altitudeNoiseOffset3);
                float value4 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius4, _altitudeNoiseOffset4);
                float value5 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius5, _altitudeNoiseOffset5);
                float value6 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius6, _altitudeNoiseOffset6);
                float value7 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius7, _altitudeNoiseOffset7);
                float value8 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius8, _altitudeNoiseOffset8);
                float value9 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius9, _altitudeNoiseOffset9);
                //float value10 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius10, _altitudeNoiseOffset10);

                //float angleValue9 = GetRandomAngleNoiseFromPolarCoordinates(alpha, beta, radius9, _altitudeNoiseOffset9);

                value8 = value8 * 1.5f + 0.25f;
                //float value10 = value9 * 0.8f + 0.1f;

                float valueA = GetContinentModifier(i, j);
                valueA = Mathf.Lerp(valueA, value3, 0.22f * value8);
                valueA = Mathf.Lerp(valueA, value4, 0.15f * value8);
                float valueAa = Mathf.Lerp(valueA, value5, 0.1f * value8);
                valueAa = Mathf.Lerp(valueAa, value6, 0.03f * value8);
                valueAa = Mathf.Lerp(valueAa, value7, 0.005f * value8);

                float valueC = Mathf.Lerp(value1, value9, 0.5f * value8);
                valueC = Mathf.Lerp(valueC, value2, 0.04f * value8);
                valueC = Mathf.Lerp(valueC, value3, 0.02f * value8);

                float valueCa = GetMountainRangeFromRandomNoise(valueC, valueAa, 40, 40, lengthFactor: 0.8f);
                float valueCd = GetMountainRangeFromRandomNoise(valueC, valueAa, 4, 40, lengthFactor: 0.8f);
                valueCa = Mathf.Lerp(valueCa, valueCd, 0.2f);

                float valueCb = GetMountainRangeFromRandomNoise(valueC, valueAa, 40, 40, 15, lengthFactor: 0.2f);
                float valueCe = GetMountainRangeFromRandomNoise(valueC, valueAa, 4, 40, 15, lengthFactor: 0.2f);
                valueCb = Mathf.Lerp(valueCb, valueCe, 0.2f);

                float valueCc = GetMountainRangeFromRandomNoise(valueC, valueAa, 40, 40, -15, lengthFactor: 0.2f);
                float valueCf = GetMountainRangeFromRandomNoise(valueC, valueAa, 4, 40, -15, lengthFactor: 0.2f);
                valueCc = Mathf.Lerp(valueCc, valueCf, 0.2f);

                valueC = Mathf.Max(valueCa, valueCb, valueCc);

                //valueCb = Mathf.Lerp(valueCb, valueCa, (value10 * 2) - 0.5f);
                //valueCc = Mathf.Lerp(valueCc, valueCa, ((1 - value10) * 2) - 0.5f);
                //valueC = Mathf.Lerp(valueCb, valueCc, 0.5f);

                //float valueE = Mathf.Lerp(value1, value4, 0.5f * value8);
                //valueE = Mathf.Lerp(valueE, value2, 0.04f * value8);
                //valueE = Mathf.Lerp(valueE, value3, 0.02f * value8);
                //valueE = GetMountainRangeFromRandomNoise(valueE, valueAa, 20, 10);
                //valueE = valueE * valueCa;
                //valueCa = Mathf.Lerp(valueCa, valueE, 0.25f);

                //float valueF = Mathf.Lerp(value1, angleValue9, 0.5f * value8);
                //valueF = Mathf.Lerp(valueF, value2, 0.4f * value8);
                //valueF = Mathf.Lerp(valueF, value3, 0.2f * value8);
                //valueF = GetMountainInterjectionFromAngleValue(valueF, 200);
                //valueCa = Mathf.Lerp(valueCa, valueF * valueCa, 0.5f);

                float valueB = Mathf.Lerp(valueAa, valueC, 0.35f * value8);
                float valueBe = ErodeNoise(valueB, 25, 10);
                valueBe = Mathf.Lerp(valueBe, 0.5f, 0.5f);

                float valueD = Mathf.Lerp(valueC, valueBe, 0.68f);
                valueD = Mathf.Lerp(valueD, value5, 0.1f * valueC);
                valueD = Mathf.Lerp(valueD, value6, 0.03f * valueC);
                valueD = Mathf.Lerp(valueD, value7, 0.005f * valueC);

                //valueD = valueF;
                CalculateAndSetAltitude(cell, valueD);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private float GetMountainRangeFromContinentCollision(float[] noises, int i, int j)
    {
        float widthF = (float)Width;
        float widthFactor = 7f / widthF;
        float widthFactor2 = ContinentBaseWidthFactor * 0.01f / widthF;

        float distance1 = float.MaxValue;
        float distance2 = float.MaxValue;
        float altitude1 = 1f;
        float altitude2 = 1f;

        for (int k = 0; k < NumContinents; k++)
        {
            float dist = GetContinentDistance(k, i, j) * (noises[k] * 0.4f + 0.8f);

            if (dist < distance1)
            {
                distance2 = distance1;
                altitude2 = altitude1;

                distance1 = dist;
                altitude1 = _continentAltitudeOffsets[k];
            }
            else if (dist < distance2)
            {
                distance2 = dist;
                altitude2 = _continentAltitudeOffsets[k];
            }
        }

        if (altitude1 < altitude2)
        {
            float temp = altitude1;
            altitude1 = altitude2;
            altitude2 = temp;

            temp = distance1;
            distance1 = distance2;
            distance2 = temp;
        }

        float worldWidthFactor = 5f * Width / ContinentBaseWidthFactor;
        float worldWidthFactor2 = 7f * Width / ContinentBaseWidthFactor;

        float distSum = distance1 + distance2;
        float distSumFactor = Mathf.Pow(worldWidthFactor / (worldWidthFactor + distSum), 2f);
        float distSumFactor2 = Mathf.Pow(worldWidthFactor2 / (worldWidthFactor2 + distSum), 2f);

        float distDiff = distance1 - distance2;
        float altitudeDiffFactor = -0.5f + 2f * Mathf.Abs(altitude1 - altitude2);
        float altitudeDiffFactor2 = 0;
        float trenchMagnitude = Mathf.Clamp01(altitudeDiffFactor);
        float trenchMagnitude2 = 0.25f * Mathf.Clamp01(altitudeDiffFactor2);

        float mountainValue = Mathf.Exp(-Mathf.Pow(distDiff * widthFactor + altitudeDiffFactor, 2));
        float trenchValue = -trenchMagnitude * Mathf.Exp(-Mathf.Pow(distDiff * widthFactor - altitudeDiffFactor, 2));
        float mountRangeValue = distSumFactor * (mountainValue + trenchValue);

        float continentMountainValue = Mathf.Exp(-Mathf.Pow(distDiff * widthFactor2 + altitudeDiffFactor2, 2));
        float continentTrenchValue = -trenchMagnitude2 * Mathf.Exp(-Mathf.Pow(distDiff * widthFactor2 - altitudeDiffFactor2, 2));
        float continentValue = distSumFactor2 * (continentMountainValue + continentTrenchValue);

        float collisionValue = Mathf.Lerp(continentValue, mountRangeValue, 0.4f);

        return collisionValue;
    }

    private ManagerTask<int> GenerateRandomInteger(int min, int max)
    {
        return Manager.EnqueueTask(() => Random.Range(min, max));
    }

    private Vector3 GenerateRandomOffsetVector()
    {
        return RandomUtility.insideUnitSphere * 1000;
    }

    private ManagerTask<Vector3> GenerateRandomOffsetVectorTask()
    {
        return Manager.EnqueueTask(() =>
        {
            return GenerateRandomOffsetVector();
        });
    }

    // Returns a value between 0 and 1
    private float GetRandomNoiseFromPolarCoordinates(float alpha, float beta, float radius, Vector3 offset)
    {
        Vector3 pos = MathUtility.GetCartesianCoordinates(alpha, beta, radius) + offset;

        return PerlinNoise.GetValue(pos.x, pos.y, pos.z);
    }

    // Returns an angle from vector noise (in radians, 0 to 2*Pi)
    private float GetRandomAngleNoiseFromPolarCoordinates(float alpha, float beta, float radius, Vector3 offset)
    {
        Vector3 pos = MathUtility.GetCartesianCoordinates(alpha, beta, radius) + offset;

        float[] vector3D = PerlinNoise.Get3DVector(pos.x, pos.y, pos.z);
        Vector2 projection = MathUtility.GetSphereProjection(vector3D, alpha, beta);

        return Mathf.Atan2(projection.y, projection.x);
    }

    // Get mountain interjections
    private float GetMountainInterjectionFromAngleValue(float angleNoise, float scaleFactor)
    {
        float scaledNoise = scaleFactor * angleNoise / (Mathf.PI * 2f);

        return (Mathf.Sin(scaledNoise) + 1) / 2f;
    }

    //// Returns a value between 0 and 1
    //private float GetRandomNoiseFromPolarCoordinates_OpenSimplex(float alpha, float beta, float radius, Vector3 offset)
    //{
    //    Vector3 pos = MathUtility.GetCartesianCoordinates(alpha, beta, radius) + offset;

    //    return (float)_openSimplexNoise.eval(pos.x, pos.y, pos.z);
    //}

    private float ErodeNoise(float noise, float widthFactor, float offset)
    {
        noise = (noise * 2) - 1;

        float value = 1 / (1 + Mathf.Exp(-(noise * widthFactor + offset)));

        return value;
    }

    private float GetMountainRangeFromRandomNoise(float noise, float filter, float widthFactor, float widthDivisor, float baseOffset = 0, float lengthFactor = 1)
    {
        lengthFactor = 1f - lengthFactor;
        filter = filter * (1 + lengthFactor) - lengthFactor;

        float scaledNoise = (noise * 2) - 1;

        int count = 5;
        float sideOffset = 0.5f + (count / 2f);

        float offsetFactor = widthFactor / widthDivisor;
        float overalOffset = 40 * baseOffset / widthDivisor;

        float value = 0;

        for (int i = 1; i <= count; i++)
        {
            float countFactor = i / (float)count;
            float countFactor2 = 1 - countFactor;
            float fi = Mathf.Clamp01(filter - countFactor2);

            if (fi <= 0)
                continue;

            fi /= 1 - countFactor2;
            fi *= countFactor;

            float si = (i - sideOffset) * 2 * offsetFactor + overalOffset;
            value += Mathf.Exp(-Mathf.Pow(scaledNoise * widthFactor + si, 2)) * fi;
        }

        return value;
    }

    private float CalculateAltitude(float baseValue)
    {
        float span = MaxPossibleAltitude - MinPossibleAltitude;

        float altitude = ((baseValue * span) + MinPossibleAltitude) * AltitudeScale;

        altitude -= SeaLevelOffset;

        altitude = Mathf.Clamp(altitude, MinPossibleAltitudeWithOffset, MaxPossibleAltitudeWithOffset);

        return altitude;
    }

    private bool SkipIfLoaded(int longitude, int latitude)
    {
        return SkipIfLoaded(TerrainCells[longitude][latitude]);
    }

    private bool SkipIfLoaded(TerrainCell cell)
    {
        if (!_justLoaded)
            return false;

        return cell.Modified;
    }

    private void ResetRainfallDependencies()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCells[i][j].RainfallDependentCells.Clear();
            }
        }
    }

    private void CalculateAndSetAltitude(TerrainCell cell, float value, bool modified = false, bool alteredByBrush = false)
    {
        float altitude = CalculateAltitude(value);

        cell.OriginalAltitude = altitude;
        cell.Altitude = altitude;
        cell.BaseAltitudeValue = value;

        cell.Modified |= modified;
        cell.TerrainAlteredBeforeDrainageRegen |= alteredByBrush;

        if (altitude > MaxAltitude) MaxAltitude = altitude;
        if (altitude < MinAltitude) MinAltitude = altitude;
    }

    private void CalculateAndSetAltitude(int longitude, int latitude, float value, bool modified = false)
    {
        CalculateAndSetAltitude(TerrainCells[longitude][latitude], value, modified);
    }

    private void RecalculateAndSetAltitude(int longitude, int latitude)
    {
        TerrainCell cell = TerrainCells[longitude][latitude];
        float value = cell.BaseAltitudeValue;

        float altitude = CalculateAltitude(value);
        cell.OriginalAltitude = altitude;
        cell.Altitude = altitude;

        if (altitude > MaxAltitude) MaxAltitude = altitude;
        if (altitude < MinAltitude) MinAltitude = altitude;
    }

    private void OffsetBrushRngCalls()
    {
        // Store values to be used later when regenerating specific cells
        _altitudeBrushNoiseOffset = GenerateRandomOffsetVectorTask();
        _temperatureBrushNoiseOffset = GenerateRandomOffsetVectorTask();
        _rainfallBrushNoiseOffset = GenerateRandomOffsetVectorTask();

        _layerBrushNoiseOffsets.Clear();

        foreach (Layer layer in Layer.Layers.Values)
        {
            _layerBrushNoiseOffsets.Add(layer.Id, GenerateRandomOffsetVectorTask());
        }
    }

    private void GenerateRandomNoiseRainfallOffsets()
    {
        // Store values to be used later when regenerating specific cells
        _rainfallNoiseOffset1 = GenerateRandomOffsetVectorTask();
        _rainfallNoiseOffset2 = GenerateRandomOffsetVectorTask();
        _rainfallNoiseOffset3 = GenerateRandomOffsetVectorTask();
    }

    private void CalculateAndSetRainfall(TerrainCell cell, float value, float? offset = null, bool modified = false)
    {
        if (offset != null)
        {
            cell.BaseRainfallOffset += offset.Value;
        }

        float rainfall = CalculateRainfall(value + cell.BaseRainfallOffset);

        cell.Rainfall = rainfall;
        cell.BaseRainfallValue = value;

        cell.Modified |= modified;

        if (rainfall > MaxRainfall) MaxRainfall = rainfall;
        if (rainfall < MinRainfall) MinRainfall = rainfall;
    }

    private void RecalculateAndSetRainfall(int longitude, int latitude)
    {
        TerrainCell cell = TerrainCells[longitude][latitude];
        float value = cell.BaseRainfallValue;
        float offset = cell.BaseRainfallOffset;

        float rainfall = CalculateRainfall(value + offset);
        cell.Rainfall = rainfall;

        if (rainfall > MaxRainfall) MaxRainfall = rainfall;
        if (rainfall < MinRainfall) MinRainfall = rainfall;
    }

    private void GenerateTerrainRainfall(TerrainCell cell, bool justSetRainfallSources = false, bool setDependencies = true)
    {
        int longitude = cell.Longitude;
        int latitude = cell.Latitude;

        float radius1 = 2f;
        float radius2 = 1f;
        float radius3 = 16f;

        float alpha = cell.Alpha;
        float beta = cell.Beta;

        float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, _rainfallNoiseOffset1);
        float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, _rainfallNoiseOffset2);
        float value3 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius3, _rainfallNoiseOffset3);

        value2 = value2 * 1.5f + 0.25f;

        float valueA = Mathf.Lerp(value1, value3, 0.15f);

        float latitudeFactor = alpha + (((valueA * 2) - 1f) * Mathf.PI * 0.15f);
        float latitudeModifier1 = Mathf.Sin(latitudeFactor);
        float latitudeFactor2 = (latitudeFactor * 3);
        float latitudeModifier2 = -latitudeModifier1 * Mathf.Sin(latitudeFactor2);
        float latitudeFactor3 = (latitudeFactor * 6);
        float latitudeModifier3 = -Mathf.Sin(latitudeFactor3);

        int offCellX = (Width + longitude + (int)Mathf.Floor(latitudeModifier2 * Width / 40f)) % Width;
        int offCellX2 = (Width + longitude + (int)Mathf.Floor(latitudeModifier2 * Width / 30f)) % Width;
        int offCellX3 = (Width + longitude + (int)Mathf.Floor(latitudeModifier2 * Width / 20f)) % Width;
        int offCellX4 = (Width + longitude + (int)Mathf.Floor(latitudeModifier2 * Width / 10f)) % Width;
        int offCellX5 = (Width + longitude + (int)Mathf.Floor(latitudeModifier2 * Width / 5f)) % Width;

        int offCellY = Mathf.Clamp(latitude + Mathf.FloorToInt(latitudeModifier3 * Height / 20f), 0, Height - 1);

        TerrainCell offCell = TerrainCells[offCellX][latitude];
        TerrainCell offCell2 = TerrainCells[offCellX2][latitude];
        TerrainCell offCell3 = TerrainCells[offCellX3][latitude];
        TerrainCell offCell4 = TerrainCells[offCellX4][latitude];
        TerrainCell offCell5 = TerrainCells[offCellX5][latitude];
        TerrainCell offCell6 = TerrainCells[longitude][offCellY];

        if (setDependencies)
        {
            offCell.RainfallDependentCells.Add(cell);
            offCell2.RainfallDependentCells.Add(cell);
            offCell3.RainfallDependentCells.Add(cell);
            offCell4.RainfallDependentCells.Add(cell);
            offCell5.RainfallDependentCells.Add(cell);
            offCell6.RainfallDependentCells.Add(cell);
        }

        if (justSetRainfallSources)
        {
            CalculateAndSetRainfall(cell, cell.BaseRainfallValue);
            return;
        }

        float altitudeValue = Mathf.Max(0, cell.Altitude);
        float offAltitude = Mathf.Max(0, offCell.Altitude);
        float offAltitude2 = Mathf.Max(0, offCell2.Altitude);
        float offAltitude3 = Mathf.Max(0, offCell3.Altitude);
        float offAltitude4 = Mathf.Max(0, offCell4.Altitude);
        float offAltitude5 = Mathf.Max(0, offCell5.Altitude);
        float offAltitude6 = Mathf.Max(0, offCell6.Altitude);

        float altitudeModifier = (altitudeValue -
                                  (offAltitude * 0.7f) -
                                  (offAltitude2 * 0.6f) -
                                  (offAltitude3 * 0.5f) -
                                  (offAltitude4 * 0.4f) -
                                  (offAltitude5 * 0.3f) -
                                  (offAltitude6 * 0.2f) +
                                  (MaxPossibleAltitude * 0.17f * value2) -
                                  (altitudeValue * 0.25f)) / MaxPossibleAltitude;

        float tempFactorOffset = 15f;
        float temperatureFactor = Mathf.Clamp01((cell.Temperature + tempFactorOffset) / (MinPossibleTemperature + tempFactorOffset));

        float rainfallValue = Mathf.Lerp(Mathf.Abs(latitudeModifier2), altitudeModifier, 0.95f);
        rainfallValue = Mathf.Max(rainfallValue, temperatureFactor * 0.1f);

        CalculateAndSetRainfall(cell, rainfallValue);
    }

    private void GenerateTerrainRainfall()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                GenerateTerrainRainfall(cell, SkipIfLoaded(cell));
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void ResetToOriginalAltitudes()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                cell.Altitude = cell.OriginalAltitude;
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void ResetToOriginalTemperatures()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                cell.Temperature = cell.OriginalTemperature;
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    /// <summary>
    /// Resets drainage values on cell.
    /// </summary>
    /// <param name="cell"> Cell to reset drainage on.</param>
    /// <param name="resetTerrain"> Indicates if terrain alterations due to drainage should be reset on cell.</param>
    private void ResetDrainage(TerrainCell cell, bool resetTerrain)
    {
        cell.FeedingCells.Clear();

        cell.RiverId = -1;
        cell.RiverLength = 0;
        cell.DrainageDone = false;
        cell.WaterAccumulation = 0;

        if (resetTerrain)
        {
            cell.Altitude = cell.OriginalAltitude;
            cell.Temperature = cell.OriginalTemperature;
        }
    }

    private float GetDirectionDistanceFactor(Direction dir)
    {
        switch (dir)
        {
            case Direction.Northeast:
                return 1.41f;
            case Direction.Northwest:
                return 1.41f;
            case Direction.Southeast:
                return 1.41f;
            case Direction.Southwest:
                return 1.41f;
        }

        return 1;
    }

    private struct CellDepth
    {
        public TerrainCell Cell;
        public int Depth;
    }

    private int CompareCellDepthAltitudes(CellDepth a, CellDepth b)
    {
        if (a.Cell.Altitude > b.Cell.Altitude) return 1;
        if (a.Cell.Altitude < b.Cell.Altitude) return -1; 

        return 0;
    }

    /// <summary>
    /// Returns the altitude of the target cell or that of a neighbor with lower altitude than the source
    /// cell. The idea is that drainage can channel through the target cell if it has no other option.
    /// </summary>
    /// <param name="targetCell"></param>
    /// <param name="sourceCell"></param>
    /// <returns></returns>
    private float GetChannelledAltitude(TerrainCell targetCell, TerrainCell sourceCell, int maxDepth)
    {
        float targetAltitude = targetCell.Altitude;
        float sourceAltitude = sourceCell.Altitude;

        HashSet<TerrainCell> exploredCells = new HashSet<TerrainCell>();

        exploredCells.Add(targetCell);

        // Skip searching through the source cell
        exploredCells.Add(sourceCell);

        // Also skip cells that neighbor the source cell
        foreach (TerrainCell cell in sourceCell.NeighborList)
        {
            exploredCells.Add(cell);
        }

        int heapSize = (maxDepth * 2) + 1;
        heapSize *= heapSize;

        BinaryHeap<CellDepth> cellsToExplore =
            new BinaryHeap<CellDepth>(CompareCellDepthAltitudes, heapSize);

        cellsToExplore.Insert(new CellDepth()
        {
            Cell = targetCell,
            Depth = 1
        });

        float lowestAltitude = targetAltitude;
        float lDepth = 1;

        while (cellsToExplore.Count > 0)
        {
            CellDepth cd = cellsToExplore.Extract();
            TerrainCell cell = cd.Cell;

            float cellAltitude = Mathf.Max(0, cell.Altitude);

            if (cellAltitude < lowestAltitude)
            {
                lowestAltitude = cellAltitude;
                lDepth = cd.Depth;
            }

            if (lowestAltitude < sourceAltitude)
            {
                break;
            }

            if (cd.Depth > maxDepth)
            {
                continue;
            }

            foreach (TerrainCell nCell in cell.NeighborList)
            {
                if (exploredCells.Contains(nCell))
                {
                    continue;
                }

                cellsToExplore.Insert(new CellDepth()
                {
                    Cell = nCell,
                    Depth = cd.Depth + 1
                });

                exploredCells.Add(nCell);
            }
        }

        float maxAltitude = Mathf.Min(sourceAltitude, targetAltitude);
        float newAltitude = Mathf.Lerp(maxAltitude, lowestAltitude, 1f / (float)lDepth);

        return newAltitude;
    }

    private delegate bool addToCellsToDrainDelegate(TerrainCell cell, bool resetDrainage, bool resetAltitude, bool callByBrush);

    private void DrainToNeighbors(TerrainCell cell, addToCellsToDrainDelegate addToCellsToDrain)
    {
        float diffPow = 2.5f;

        bool drainageWasDone = cell.DrainageDone;
        bool redoDrainageForAll = !cell.DrainageDone;

        cell.DrainageDone = true;
        cell.TerrainAlteredBeforeDrainageRegen = false;

        if (cell.WaterAccumulation < MinRiverFlow)
        {
            return;
        }

        float cellAltitude = Mathf.Max(0, cell.Altitude);

        Dictionary<TerrainCell, float> nAltitudes = new Dictionary<TerrainCell, float>();

#if DEBUG
        if ((cell.Longitude == 250) && (cell.Latitude == 125))
        {
            Debug.Log("Debugging drainage on cell " + cell.Position);
        }
#endif

        float totalAltDifference = 0;
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            float nCellAltitude = Mathf.Max(0, nCell.Altitude);
            nAltitudes[nCell] = nCellAltitude;

            float diff = Mathf.Max(0, cellAltitude - nCellAltitude);
            diff = Mathf.Pow(diff, diffPow);

            totalAltDifference += diff;
        }

        // try using altitude channelling
        if (totalAltDifference <= 0)
        {
            TerrainCell bestCell = cell;
            float minAltitude = cell.Altitude;
            float minMinAltitude = cell.Altitude;
            foreach (TerrainCell nCell in cell.NeighborList)
            {
                float nCellAltitude = Mathf.Max(0, nCell.Altitude);
                float nCellMinAltitude = Mathf.Max(0, GetChannelledAltitude(nCell, cell, 5));

                if (nCellMinAltitude < minMinAltitude)
                {
                    minAltitude = nCellAltitude;
                    minMinAltitude = nCellMinAltitude;
                    bestCell = nCell;
                    continue;
                }

                // solve draws
                if (nCellMinAltitude == minMinAltitude)
                {
                    if (nCellAltitude < minAltitude)
                    {
                        minAltitude = nCellAltitude;
                        bestCell = nCell;
                    }
                }
            }

            if (bestCell != cell)
            {
                nAltitudes[bestCell] = minMinAltitude;

                float diff = Mathf.Max(0, cellAltitude - minMinAltitude);
                diff = Mathf.Pow(diff, diffPow);

                totalAltDifference += diff;
            }
        }

        if (totalAltDifference <= 0)
        {
            return;
        }

        List<KeyValuePair<Direction, TerrainCell>> cellsToKeep = new List<KeyValuePair<Direction, TerrainCell>>(cell.Neighbors.Count);
        float totalDiffBuffer = totalAltDifference;

        foreach (KeyValuePair<Direction, TerrainCell> nPair in cell.Neighbors)
        {
            TerrainCell nCell = nPair.Value;

            redoDrainageForAll |= nCell.TerrainAlteredBeforeDrainageRegen;

            float nCellAltitude = nAltitudes[nCell];

            float diff = Mathf.Max(0, cellAltitude - nCellAltitude);
            diff = Mathf.Pow(diff, diffPow);

            float percent = diff / totalDiffBuffer;

            if (percent == 0)
                continue;

            float rainfallTransfer = cell.WaterAccumulation * percent;

            if (rainfallTransfer < MinRiverFlow)
            {
                totalAltDifference -= diff;
                continue;
            }

            cellsToKeep.Add(nPair);
        }

        foreach (KeyValuePair<Direction, TerrainCell> nPair in cellsToKeep)
        {
            TerrainCell nCell = nPair.Value;

            // Skip neighbor cell if it already had its drainage calculated and this cell's drainage didn't change
            if (!redoDrainageForAll && nCell.DrainageDone)
                continue;

            //#if DEBUG
            //            if (nCell == _lowestEvaluatedCell)
            //            {
            //                Debug.Log("_lowestEvaluatedCell (" + nCell.Position + ") being drained on again, now from " + cell.Position);
            //            }
            //#endif

            float nCellAltitude = nAltitudes[nCell];

            float diff = Mathf.Max(0, cellAltitude - nCellAltitude);
            diff = Mathf.Pow(diff, diffPow);

            float percent = diff / totalAltDifference;

            if (percent == 0)
                continue;

            float dirFactor = GetDirectionDistanceFactor(nPair.Key);

            float drainageTransfer = cell.WaterAccumulation * percent;
            float waterLoss = CalculateWaterLoss(nCell, drainageTransfer) * dirFactor;
            float drainageTransferMinusLoss = drainageTransfer - waterLoss;

            if (drainageTransferMinusLoss <= 0)
                continue;

#if DEBUG
            if ((nCell.Longitude == 229) && (nCell.Latitude == 133))
            {
                Debug.Log("Debugging nCell " + nCell.Position);
            }
#endif

            if (nCell.Altitude > nCellAltitude)
            {
                nCell.Altitude = nCellAltitude;
            }

            nCell.FeedingCells[cell] = new TerrainCell.RiverBuffers
            {
                DrainageTransfer = drainageTransferMinusLoss,
                TemperatureTransfer = cell.OriginalTemperature * drainageTransferMinusLoss,
                DirectionFactor = dirFactor
            };

            addToCellsToDrain(nCell, true, true, false);
        }
    }

    public IEnumerable<TerrainCell> GetCellsInRectangle(int longitude, int latitude, int width, int height)
    {
        TerrainCell[] cells = new TerrainCell[width * height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                yield return GetCellWithSphericalWrap(longitude + i, latitude + j);
            }
        }
    }

    /// <summary>
    /// Sets the cells on which drainage is to be evaluated.
    /// </summary>
    private void SetCellsToEvalForDrainange()
    {
        // find cells that are higher than neighbors
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                _cellsToDrain.Add(cell);
            }
        }
    }

    /// <summary>
    /// Resets drainage on the set of cells to evaluate.
    /// </summary>
    private void ResetDrainage()
    {
        // find cells that are higher than neighbors
        foreach (TerrainCell cell in _cellsToDrain)
        {
            ResetDrainage(cell, false);
        }
    }

    /// <summary>
    /// Generates drainage basins within a set of cells.
    /// </summary>
    /// <param name="doTerrainModification"> Indicate if additionally the terrain should be modified by drainage action.</param>
    public void GenerateDrainageBasins(bool doTerrainModification = true)
    {
        // reset drainage before proceeding
        ResetDrainage();

        MaxWaterAccumulation = 0;

        float firstPartLength = 0.1f;
        float secondPartLength = 0.8f;
        float thirdPartLength = 0.1f;

        HashSet<TerrainCell> queuedDrainCells = new HashSet<TerrainCell>();
        BinaryHeap<TerrainCell> cellsToDrain =
            new BinaryHeap<TerrainCell>(TerrainCell.CompareOriginalAltitude, 500000);

        int i = 0;
        int evalCount = _cellsToDrain.Count;
        int segSize = 100;

        // find cells that are higher than neighbors
        foreach (TerrainCell cell in _cellsToDrain)
        {
            cell.RiverId = cell.Latitude * Width + cell.Longitude;

            if (cell.IsBelowSeaLevel)
                continue;

            cell.UpdateDrainage();

            if (cell.WaterAccumulation <= 0)
            {
                cell.DrainageDone = true;
                continue;
            }

            bool higherThanNeighborsWithWater = true;
            foreach (TerrainCell nCell in cell.NeighborList)
            {
                if ((nCell.Altitude > cell.Altitude) && (nCell.WaterAccumulation > MinRiverFlow))
                {
                    higherThanNeighborsWithWater = false;
                    break;
                }
            }

            if (!higherThanNeighborsWithWater)
            {
                cell.DrainageDone = true;
                continue;
            }

            cellsToDrain.Insert(cell);
            queuedDrainCells.Add(cell);

            if ((i++ % segSize) == 0)
            {
                ProgressCastMethod(_accumulatedProgress + _progressIncrement * firstPartLength * i / evalCount);
            }
        }

        int cellsDrained = 0;

        // drain cells
        int drainedCells = 0;
        while (cellsToDrain.Count > 0)
        {
            TerrainCell cell = cellsToDrain.Extract();
            queuedDrainCells.Remove(cell);

            cellsDrained++;

            cell.UpdateDrainage();

            drainedCells++;

            if ((drainedCells % Height) == 0)
            {
                float progressPercent = drainedCells / (float)(drainedCells + cellsToDrain.Count);

                ProgressCastMethod(_accumulatedProgress + _progressIncrement * secondPartLength * progressPercent);
            }

            DrainToNeighbors(cell, (nCell, rD, rA, cB) =>
            {
                if (queuedDrainCells.Contains(nCell))
                    return false;

                nCell.DrainageDone = false;

                cellsToDrain.Insert(nCell);
                queuedDrainCells.Add(nCell);

                return true;
            });
        }

        if (doTerrainModification && (MaxWaterAccumulation > 0))
        {
            i = 0;
            foreach (TerrainCell cell in _cellsToDrain)
            {
                DrainModifyCell(cell);

                if ((i++ % segSize) == 0)
                {
                    ProgressCastMethod(_accumulatedProgress + _progressIncrement * thirdPartLength * i / evalCount);
                }
            }
        }

        _accumulatedProgress += _progressIncrement;
    }

    private float CalculateWaterLoss(TerrainCell cell, float rainfallTransfer)
    {
        if (rainfallTransfer <= 0)
            return 0;

        if (cell.IsBelowSeaLevel)
        {
            return Mathf.Max(rainfallTransfer * OceanDispersalFactor, MinOceanDispersal);
        }
        else
        {
            float maxTransferProtected = 1500f;

            float protectedTransfer = Mathf.Min(maxTransferProtected * RiverStrength, rainfallTransfer);
            float unprotectedTransfer = Mathf.Max(0, rainfallTransfer - protectedTransfer) * RiverStrength;
            float preservedTransfer = protectedTransfer + unprotectedTransfer;

            return rainfallTransfer - preservedTransfer;
        }
    }

    private void DrainModifyCell(TerrainCell cell)
    {
        if (cell.WaterAccumulation > 0)
        {
            // perform erosion

            float rainfallFactor = Mathf.Max(Mathf.Pow(cell.Rainfall, 2), 0);
            rainfallFactor = 0.2f + MaxPossibleRainfall / (rainfallFactor + MaxPossibleRainfall);
            float rainfallScalingFactor = 0.2f * MaxPossibleRainfall;
            float waterAccFactor = cell.WaterAccumulation / (cell.WaterAccumulation + rainfallScalingFactor);

            cell.Altitude -= WaterErosionFactor * waterAccFactor * rainfallFactor;
        }
    }

    private void GenerateRandomNoiseTemperatureOffsets()
    {
        // Store values to be used later when regenerating specific cells
        _tempNoiseOffset1 = GenerateRandomOffsetVectorTask();
        _tempNoiseOffset2 = GenerateRandomOffsetVectorTask();
    }

    private void CalculateAndSetTemperature(TerrainCell cell, float value, float? offset = null, bool modified = false)
    {
        if (offset != null)
        {
            cell.BaseTemperatureOffset += offset.Value;
        }

        float temperature = CalculateTemperature(value + cell.BaseTemperatureOffset);

        if (!temperature.IsInsideRange(MinPossibleTemperatureWithOffset - 0.5f, MaxPossibleTemperatureWithOffset + 0.5f))
        {
            Debug.LogWarning("CalculateAndSetTemperature - Invalid temperature: " + temperature);
        }

        cell.Temperature = temperature;
        cell.OriginalTemperature = temperature;
        cell.BaseTemperatureValue = value;

        cell.Modified |= modified;

        if (temperature > MaxTemperature) MaxTemperature = temperature;
        if (temperature < MinTemperature) MinTemperature = temperature;
    }

    private void RecalculateAndSetTemperature(int longitude, int latitude)
    {
        TerrainCell cell = TerrainCells[longitude][latitude];
        float value = cell.BaseTemperatureValue;
        float offset = cell.BaseTemperatureOffset;

        float temperature = CalculateTemperature(value + offset);

        if (!temperature.IsInsideRange(MinPossibleTemperatureWithOffset - 0.5f, MaxPossibleTemperatureWithOffset + 0.5f))
        {
            Debug.LogWarning("RecalculateAndSetTemperature - Invalid temperature: " + temperature);
        }

        cell.Temperature = temperature;
        cell.OriginalTemperature = temperature;

        if (temperature > MaxTemperature) MaxTemperature = temperature;
        if (temperature < MinTemperature) MinTemperature = temperature;
    }

    private void CalculateTerrainHilliness(TerrainCell cell)
    {
        float altitudeDelta = 0;
        float cellAltitude = cell.Altitude;

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            altitudeDelta += Mathf.Abs(cellAltitude - nCell.Altitude);
        }

        altitudeDelta /= cell.Neighbors.Count;

        float slope = (altitudeDelta * altitudeDelta) / cell.Area;

        cell.Hilliness = Mathf.Clamp01(slope * TerrainCell.HillinessSlopeFactor);
    }

    private void GenerateTerrainTemperature(TerrainCell cell)
    {
        float radius1 = 2f;
        float radius2 = 16f;

        float alpha = cell.Alpha;
        float beta = cell.Beta;

        float value1 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, _tempNoiseOffset1);
        float value2 = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, _tempNoiseOffset2);

        float latitudeModifier = (alpha * 0.9f) + ((value1 + value2) * 0.05f * Mathf.PI);

        float altitudeSpan = MaxPossibleAltitude - MinPossibleAltitude;

        float absAltitude = cell.Altitude - MinPossibleAltitudeWithOffset;

        float altitudeFactor1 = (absAltitude / altitudeSpan) * 0.7f;
        float altitudeFactor2 = (Mathf.Clamp01(cell.Altitude / MaxPossibleAltitude) * 1.3f);
        float altitudeFactor3 = -0.18f;

        CalculateAndSetTemperature(cell, Mathf.Sin(latitudeModifier) - altitudeFactor1 - altitudeFactor2 - altitudeFactor3);

        //#if DEBUG
        //        if ((i == 269) && (j == 136))
        //        {
        //            Debug.Log(
        //                "temperature:" + cell.Temperature +
        //                ", TemperatureOffset:" + TemperatureOffset +
        //                ", MaxPossibleTemperature:" + MaxPossibleTemperature +
        //                ", MinPossibleTemperature:" + MinPossibleTemperature +
        //                ", MaxPossibleTemperatureWithOffset:" + MaxPossibleTemperatureWithOffset +
        //                ", MinPossibleTemperatureWithOffset:" + MinPossibleTemperatureWithOffset +
        //                ", alpha:" + alpha +
        //                ", beta:" + beta +
        //                ", value1:" + value1 +
        //                ", offset1:" + offset1.Result +
        //                ", latitudeModifier:" + latitudeModifier +
        //                ", altitudeFactor1:" + altitudeFactor1 +
        //                ", altitudeFactor2:" + altitudeFactor2 +
        //                ", altitudeFactor3:" + altitudeFactor3 +
        //                ", Seed:" + Seed
        //                );
        //        }
        //#endif
    }

    private void CalculateTerrainHilliness()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                CalculateTerrainHilliness(TerrainCells[i][j]);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void GenerateTerrainTemperature()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                TerrainCell cell = TerrainCells[i][j];

                if (SkipIfLoaded(cell))
                {
                    CalculateAndSetTemperature(cell, cell.BaseTemperatureValue);
                    continue;
                }

                GenerateTerrainTemperature(cell);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void CalculateTerrainArability(TerrainCell cell)
    {
        float radius = 2f;

        float alpha = cell.Alpha;
        float beta = cell.Beta;

        float biomeFactor = 0;

        for (int i = 0; i < cell.PresentBiomeIds.Count; i++)
        {
            Biome biome = Biome.Biomes[cell.PresentBiomeIds[i]];

            biomeFactor += biome.Arability * cell.BiomePresences[i];
        }

        float hillinessFactor = 1 - cell.Hilliness;
        hillinessFactor = Mathf.Clamp01(hillinessFactor);

        float baseArability = hillinessFactor * biomeFactor;

        if (baseArability <= 0)
        {
            cell.BaseArability = 0;
            cell.Arability = 0;
            return;
        }

        // This simulates things like stoniness, impracticality of drainage, excessive salts, etc.
        float noiseFactor = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius, _arabilityNoiseOffset);

        cell.BaseArability = baseArability * noiseFactor;
        cell.Arability = cell.BaseArability;
    }

    private void CalculateTerrainArability()
    {
        int sizeX = Width;
        int sizeY = Height;

        _arabilityNoiseOffset = GenerateRandomOffsetVectorTask();

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                CalculateTerrainArability(TerrainCells[i][j]);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void CalculateAndSetTerrainLayerValue(TerrainCell cell, Layer layer)
    {
        CellLayerData data = cell.GetLayerData(layer.Id);

        float offset = 0;

        if (data != null)
            offset = data.Offset;

        float value = CalculateLayerValue(cell, layer, offset);

        cell.SetLayerData(layer, value, offset, data);
    }

    private void CalculateAndSetTerrainLayerValue(TerrainCell cell, Layer layer, float offset)
    {
        CellLayerData data = cell.GetLayerData(layer.Id);

        if (data != null)
            offset += data.Offset;

        float value = CalculateLayerValue(cell, layer, offset);

        cell.SetLayerData(layer, value, offset, data);

        cell.Modified = true;
    }

    private void GenerateTerrainLayers(TerrainCell cell)
    {
        foreach (Layer layer in Layer.Layers.Values)
        {
            CalculateAndSetTerrainLayerValue(cell, layer);
        }
    }

    private void GenerateTerrainBiomes(TerrainCell cell)
    {
#if DEBUG
        if ((cell.Longitude == 229) && (cell.Latitude == 133))
        {
            Debug.Log("Debugging cell " + cell.Position);
        }
#endif

        float totalBiomePresence = 0;

        Dictionary<string, float> biomePresences = new Dictionary<string, float>();

        foreach (Biome biome in Biome.Biomes.Values)
        {
            float presence = CalculateBiomePresence(cell, biome);

            if (presence <= 0) continue;

            biomePresences.Add(biome.Id, presence);

            totalBiomePresence += presence;
        }

        cell.ResetBiomes();

        cell.Survivability = 0;
        cell.ForagingCapacity = 0;
        cell.BaseAccessibility = 0;

        foreach (Biome biome in Biome.Biomes.Values)
        {
            float absPresence = 0;

            if (biomePresences.TryGetValue(biome.Id, out absPresence))
            {
                float relPresence = absPresence / totalBiomePresence;

                cell.BiomeAbsPresences.Add(absPresence);
                cell.AddBiomeRelPresence(biome, relPresence);

                cell.Survivability += biome.Survivability * relPresence;
                cell.ForagingCapacity += biome.ForagingCapacity * relPresence;
                cell.BaseAccessibility += biome.Accessibility * relPresence;
            }
        }

        cell.BaseAccessibility *= 1 - cell.Hilliness;
        cell.Accessibility = cell.BaseAccessibility;

        float altitudeSurvivabilityFactor = 1 - Mathf.Clamp01(cell.Altitude / MaxPossibleAltitude);

        cell.Survivability *= altitudeSurvivabilityFactor;
    }

    private void GenerateTerrainLayers()
    {
        int sizeX = Width;
        int sizeY = Height;

        _layerNoiseOffsets.Clear();

        foreach (Layer layer in Layer.Layers.Values)
        {
            layer.Reset();

            ManagerTask<Vector3>[] offsetVectors = new ManagerTask<Vector3>[2];
            offsetVectors[0] = GenerateRandomOffsetVectorTask();
            offsetVectors[1] = GenerateRandomOffsetVectorTask();

            _layerNoiseOffsets.Add(layer.Id, offsetVectors);
        }

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                GenerateTerrainLayers(TerrainCells[i][j]);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private void GenerateTerrainBiomes()
    {
        int sizeX = Width;
        int sizeY = Height;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                GenerateTerrainBiomes(TerrainCells[i][j]);
            }

            ProgressCastMethod(_accumulatedProgress + _progressIncrement * (i + 1) / (float)sizeX);
        }

        _accumulatedProgress += _progressIncrement;
    }

    private float CalculateCellBaseArability(TerrainCell cell)
    {
        float biomeFactor = 0;

        for (int i = 0; i < cell.PresentBiomeIds.Count; i++)
        {
            Biome biome = Biome.Biomes[cell.PresentBiomeIds[i]];

            biomeFactor += biome.Arability * cell.BiomePresences[i];
        }

        float hillinessFactor = 1 - cell.Hilliness;
        hillinessFactor = Mathf.Clamp01(hillinessFactor);

        return hillinessFactor * biomeFactor;
    }

    private float CalculateLayerNoiseFactor(TerrainCell cell, Layer layer)
    {
        float radius1 = 1 / layer.NoiseScale;
        float radius2 = 10 / layer.NoiseScale;

        float alpha = cell.Alpha;
        float beta = cell.Beta;

        ManagerTask<Vector3>[] noiseOffsets = _layerNoiseOffsets[layer.Id];

        Vector3 noiseOffset1 = noiseOffsets[0].Result;
        Vector3 noiseOffset2 = noiseOffsets[1].Result;

        float value = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius1, noiseOffset1);

        LayerSettings layerSettings = Manager.GetLayerSettings(layer.Id);

        float rarity = 1 - layerSettings.Frequency;
        value = (value - rarity) / layerSettings.Frequency;

        if (value < 0)
            return value; // Values less than 0 will be ignored anyway so no need to continue

        if (layerSettings.SecondaryNoiseInfluence > 0)
        {
            float secondaryNoise = GetRandomNoiseFromPolarCoordinates(alpha, beta, radius2, noiseOffset2);
            value = value - (layerSettings.SecondaryNoiseInfluence * secondaryNoise);
        }

        return value;
    }

    private float CalculateLayerAltitudeFactor(TerrainCell cell, Layer layer)
    {
        float altitudeSpan = layer.MaxAltitude - layer.MinAltitude;

        float altitudeDiff = cell.Altitude - layer.MinAltitude;

        if (altitudeDiff < 0)
            return -1;

        float altitudeFactor = altitudeDiff / altitudeSpan;

        if (float.IsInfinity(altitudeFactor))
            return -1;

        if (altitudeFactor > 1)
            return -1;

        if (altitudeFactor > 0.5f)
            altitudeFactor = 1f - altitudeFactor;

        altitudeFactor *= layer.AltSaturationSlope;

        return Mathf.Min(1, altitudeFactor * 2);
    }

    private float CalculateLayerWaterFactor(TerrainCell cell, Layer layer)
    {
        float flowingWaterSpan = layer.MaxFlowingWater - layer.MinFlowingWater;
        float rainfallSpan = layer.MaxRainfall - layer.MinRainfall;

        float flowingWaterDiff = cell.FlowingWater - layer.MinFlowingWater;
        float rainfallDiff = cell.Rainfall - layer.MinRainfall;

        if ((flowingWaterDiff < 0) && (rainfallDiff < 0))
            return -1f;

        float moistureFactor = flowingWaterDiff / flowingWaterSpan;
        float rainfallFactor = rainfallDiff / rainfallSpan;

        float waterFactor = Mathf.Max(moistureFactor, rainfallFactor);

        if (waterFactor > 1)
            return -1f;

        if (waterFactor > 0.5f)
            waterFactor = 1f - waterFactor;

        waterFactor *= layer.WaterSaturationSlope;

        return Mathf.Max(1, waterFactor * 2);
    }

    private float CalculateLayerTemperatureFactor(TerrainCell cell, Layer layer)
    {
        float temperatureSpan = layer.MaxTemperature - layer.MinTemperature;

        float temperatureDiff = cell.Temperature - layer.MinTemperature;

        if (temperatureDiff < 0)
            return -1f;

        float temperatureFactor = temperatureDiff / temperatureSpan;

        if (float.IsInfinity(temperatureSpan))
            return -1;

        if (temperatureFactor > 1)
            return -1;

        if (temperatureFactor > 0.5f)
            temperatureFactor = 1f - temperatureFactor;

        temperatureFactor *= layer.TempSaturationSlope;

        return Mathf.Min(1, temperatureFactor * 2);
    }

    private float CalculateLayerValue(TerrainCell cell, Layer layer, float offset)
    {
        float value = 1;

        if (value <= 0)
            return value;

        value *= CalculateLayerAltitudeFactor(cell, layer);

        if (value <= 0)
            return value;

        value *= CalculateLayerWaterFactor(cell, layer);

        if (value <= 0)
            return value;

        value *= CalculateLayerTemperatureFactor(cell, layer);

        if (value <= 0)
            return value;

        value *= CalculateLayerNoiseFactor(cell, layer);

        value = Mathf.Clamp01(value) + offset;

        return value;
    }

    private float CalculateBiomeAltitudeFactor(TerrainCell cell, Biome biome)
    {
        float altitudeSpan = biome.MaxAltitude - biome.MinAltitude;

        float altitudeDiff = cell.Altitude - biome.MinAltitude;

        if (altitudeDiff < 0)
            return -1f;

        float altitudeFactor = altitudeDiff / altitudeSpan;

        if (float.IsInfinity(altitudeFactor))
        {
            altitudeFactor = 0.5f;
        }

        if (altitudeFactor > 1)
            return -1f;

        if (altitudeFactor > 0.5f)
            altitudeFactor = 1f - altitudeFactor;

        altitudeFactor *= biome.AltSaturationSlope;

        if (altitudeFactor == 0)
        {
            altitudeFactor = 0.005f; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return altitudeFactor * 2;
    }

    private float CalculateWaterBiomeWaterFactor(TerrainCell cell, Biome biome)
    {
        float flowingWaterSpan = biome.MaxFlowingWater - biome.MinFlowingWater;
        float flowingWaterDiff = cell.FlowingWater - biome.MinFlowingWater;

        if (flowingWaterDiff < 0)
            return -1f;

        float waterFactor = flowingWaterDiff / flowingWaterSpan;

        if (waterFactor > 1)
            return -1f;

        if (waterFactor > 0.5f)
            waterFactor = 1f - waterFactor;

        waterFactor *= biome.WaterSaturationSlope;

        return Mathf.Max(0.005f, waterFactor * 2);
    }

    private float CalculateBiomeWaterFactor(TerrainCell cell, Biome biome)
    {
        if (biome.TerrainType == BiomeTerrainType.Water)
        {
            return CalculateWaterBiomeWaterFactor(cell, biome);
        }

        float flowingWaterSpan = biome.MaxFlowingWater - biome.MinFlowingWater;
        float rainfallSpan = biome.MaxRainfall - biome.MinRainfall;

        float flowingWaterDiff = cell.FlowingWater - biome.MinFlowingWater;
        float rainfallDiff = cell.Rainfall - biome.MinRainfall;

        if ((flowingWaterDiff < 0) && (rainfallDiff < 0))
            return -1f;

        float moistureFactor = flowingWaterDiff / flowingWaterSpan;
        float rainfallFactor = rainfallDiff / rainfallSpan;

        float waterFactor = Mathf.Max(moistureFactor, rainfallFactor);

        if (waterFactor > 1)
            return -1f;

        if (waterFactor > 0.5f)
            waterFactor = 1f - waterFactor;

        waterFactor *= biome.WaterSaturationSlope;

        return Mathf.Max(0.005f, waterFactor * 2);
    }

    private float CalculateBiomeTemperatureFactor(TerrainCell cell, Biome biome)
    {
        float temperatureSpan = biome.MaxTemperature - biome.MinTemperature;

        float temperatureDiff = cell.Temperature - biome.MinTemperature;

        if (temperatureDiff < 0)
            return -1f;

        float temperatureFactor = temperatureDiff / temperatureSpan;

        if (float.IsInfinity(temperatureSpan))
        {
            temperatureFactor = 0.5f;
        }

        if (temperatureFactor > 1)
            return -1f;

        if (temperatureFactor > 0.5f)
            temperatureFactor = 1f - temperatureFactor;

        temperatureFactor *= biome.TempSaturationSlope;

        if (temperatureFactor == 0)
        {
            temperatureFactor = 0.005f; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return temperatureFactor * 2;
    }

    private float CalculateBiomeLayerFactor(TerrainCell cell, Biome.LayerConstraint constraint)
    {
        Layer layer = Layer.Layers[constraint.LayerId];

        float cellValue = cell.GetLayerValue(constraint.LayerId) * layer.MaxPossibleValue;

        float valueSpan = constraint.MaxValue - constraint.MinValue;

        float valueDiff = cellValue - constraint.MinValue;

        if (valueDiff < 0)
            return -1f;

        float valueFactor = valueDiff / valueSpan;

        if (float.IsInfinity(valueFactor))
        {
            valueFactor = 0.5f;
        }

        if (valueFactor > 1)
            return -1f;

        if (valueFactor > 0.5f)
            valueFactor = 1f - valueFactor;

        if (valueFactor == 0)
        {
            valueFactor = 0.005f; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return valueFactor * 2;
    }

    private float CalculateBiomeLayerFactor(TerrainCell cell, Biome biome)
    {
        if (biome.LayerConstraints == null)
            return 1;

        float layerFactor = 1;

        foreach (Biome.LayerConstraint constraint in biome.LayerConstraints.Values)
        {
            layerFactor *= CalculateBiomeLayerFactor(cell, constraint);

            if (layerFactor <= 0)
                return layerFactor;
        }

        return layerFactor;
    }

    private float CalculateBiomePresence(TerrainCell cell, Biome biome)
    {
        float presence = 1f;

        presence *= CalculateBiomeAltitudeFactor(cell, biome);

        if (presence <= 0)
            return presence;

        if (!biome.Traits.Contains("sea"))
        {
            presence *= CalculateBiomeWaterFactor(cell, biome);
        }

        if (presence <= 0)
            return presence;

        presence *= CalculateBiomeTemperatureFactor(cell, biome);

        if (presence <= 0)
            return presence;

        presence *= CalculateBiomeLayerFactor(cell, biome);

        return presence;
    }

    private float CalculateRainfall(float value)
    {
        float span = MaxPossibleRainfallWithOffset - MinPossibleRainfallWithOffset;

        float rainfall = (value * span) + MinPossibleRainfallWithOffset;

        float minRainfall = Mathf.Max(0, MinPossibleRainfallWithOffset);

        rainfall = Mathf.Clamp(rainfall, minRainfall, MaxPossibleRainfallWithOffset);

        return rainfall;
    }

    private float CalculateTemperature(float value)
    {
        float span = MaxPossibleTemperature - MinPossibleTemperature;

        float temperature = (value * span) + MinPossibleTemperature;

        temperature += TemperatureOffset;

        temperature = Mathf.Clamp(temperature, MinPossibleTemperatureWithOffset, MaxPossibleTemperatureWithOffset);

        return temperature;
    }
}

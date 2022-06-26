﻿using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CellGroup : Identifiable, ISynchronizable, IFlagHolder
{

    [XmlIgnore]
    public World World;

    public const long QuarterGenSpan = 5 * World.YearLength;
    public const long GenerationSpan = QuarterGenSpan * 4;

    public const long MaxUpdateSpan = GenerationSpan * 8000;

    public const float MaxUpdateSpanFactor = MaxUpdateSpan / GenerationSpan;

    public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
    public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
    public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

    public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;

    public const float SeaTravelBaseFactor = 25f;

    public const float MinProminencePopulation = 2;

    public static float TravelWidthFactor;

    private HashSet<ICellGroupEventGenerator> _generatorsToTestAssignmentFor =
        new HashSet<ICellGroupEventGenerator>();

    public static List<IWorldEventGenerator> OnSpawnEventGenerators;
    public static List<IWorldEventGenerator> OnCoreHighestProminenceChangeEventGenerators;
    public static List<IWorldEventGenerator> OnPolityCountChangeEventGenerators;
    public static List<IWorldEventGenerator> OnCoreCountChangeEventGenerators;
    public static Dictionary<string, List<IWorldEventGenerator>> OnKnowledgeLevelFallsBelowEventGenerators;
    public static Dictionary<string, List<IWorldEventGenerator>> OnKnowledgeLevelRaisesAboveEventGenerators;
    public static Dictionary<string, List<IWorldEventGenerator>> OnGainedDiscoveryEventGenerators;

    public static HashSet<CellGroupEventGenerator> EventGeneratorsThatNeedCleanup;

    [XmlAttribute("MT")]
    public bool MigrationTagged = false;

    [XmlAttribute("PEP")]
    public float PreviousExactPopulation;

    [XmlAttribute("EP")]
    public float ExactPopulation; // TODO: Get rid of 'float' population values

    [XmlAttribute("P")]
    public bool StillPresent = true;

    [XmlAttribute("LUD")]
    public long LastUpdateDate;
    [XmlAttribute("NUD")]
    public long NextUpdateDate;
    [XmlAttribute("UESD")]
    public long UpdateEventSpawnDate;

    [XmlAttribute("OP")]
    public int OptimalPopulation;

    [XmlAttribute("Lo")]
    public int Longitude;
    [XmlAttribute("La")]
    public int Latitude;

    [XmlAttribute("STF")]
    public float SeaTravelFactor = 0;

    [XmlAttribute("TPP")]
    public float TotalPolityProminenceValueFloat = 0;

    [XmlAttribute("MEv")]
    public bool HasMigrationEvent = false;
    [XmlAttribute("MD")]
    public long MigrationEventDate;
    [XmlAttribute("MSD")]
    public long MigrationEventSpawnDate;
    [XmlAttribute("MLo")]
    public int MigrationTargetLongitude;
    [XmlAttribute("MLa")]
    public int MigrationTargetLatitude;
    [XmlAttribute("MED")]
    public int MigrationEventDirectionInt;
    [XmlAttribute("MET")]
    public int MigrationEventTypeInt;
    [XmlAttribute("MPPer")]
    public float MigrationProminencePercent;

    [XmlAttribute("TFEv")]
    public bool HasTribeFormationEvent = false;
    [XmlAttribute("TFD")]
    public long TribeFormationEventDate;

    [XmlAttribute("ArM")]
    public int ArabilityModifier = 0;
    [XmlAttribute("AcM")]
    public int AccessibilityModifier = 0;
    [XmlAttribute("NvM")]
    public int NavigationRangeModifier = 0;

    #region MigratingPopPolId
    [XmlAttribute("MPPId")]
    public string MigratingPopPolIdStr
    {
        get { return MigratingPopPolId; }
        set { MigratingPopPolId = value; }
    }
    [XmlIgnore]
    public Identifier MigratingPopPolId;
    #endregion

    public Route SeaMigrationRoute = null;

    public List<string> Flags;

    public CellCulture Culture;

    public List<string> Properties;

    public List<Identifier> FactionCoreIds;

    public List<PolityProminence> PolityProminences = null;

    public MigratingPopulationSnapshot LastPopulationMigration = null;

    [XmlIgnore]
    public WorldPosition Position
    {
        get
        {
            return Cell.Position;
        }
    }

    [XmlIgnore]
    public float ScaledNavigationRangeModifier
    {
        get => NavigationRangeModifier * MathUtility.IntToFloatScalingFactor;
        set => NavigationRangeModifier = (int)(value * MathUtility.FloatToIntScalingFactor);
    }

    [XmlIgnore]
    public float ScaledArabilityModifier
    {
        get => ArabilityModifier * MathUtility.IntToFloatScalingFactor;
        set
        {
            if (!value.IsInsideRange(0, 1))
            {
                Debug.LogWarning($"ScaledArabilityModifier outside of range between 0, 1: {value}");
            }

            ArabilityModifier = (int)(Mathf.Clamp01(value) * MathUtility.FloatToIntScalingFactor);
        }
    }

    [XmlIgnore]
    public float ScaledAccessibilityModifier
    {
        get => AccessibilityModifier * MathUtility.IntToFloatScalingFactor;
        set
        {
            if (!value.IsInsideRange(0, 1))
            {
                Debug.LogWarning($"ScaledAccessibilityModifier outside of range between 0, 1: {value}");
            }

            AccessibilityModifier = (int)(Mathf.Clamp01(value) * MathUtility.FloatToIntScalingFactor);
        }
    }

    [XmlIgnore]
    public float TotalPolityProminenceValue
    {
        get
        {
            return TotalPolityProminenceValueFloat;
        }
        set
        {
            TotalPolityProminenceValueFloat = MathUtility.RoundToSixDecimals(Mathf.Clamp01(value));
        }
    }

    [XmlIgnore]
    public Dictionary<Identifier, Faction> FactionCores = new Dictionary<Identifier, Faction>();

    [XmlIgnore]
    public UpdateCellGroupEvent UpdateEvent;

    [XmlIgnore]
    public MigratePopulationEvent PopulationMigrationEvent;

    [XmlIgnore]
    public TribeFormationEvent TribeFormationEvent;

    [XmlIgnore]
    public TerrainCell Cell;

    [XmlIgnore]
    public MigratingUnorganizedBands MigratingUnorganizedBands = null;
    [XmlIgnore]
    public MigratingPolityPopulation MigratingPolityPopulation = null;

    [XmlIgnore]
    public Faction WillBecomeCoreOfFaction = null;

#if DEBUG
    [XmlIgnore]
    public bool DebugTagged = false;
#endif

    [XmlIgnore]
    public float MigrationPressure = 0;

    [XmlIgnore]
    public Dictionary<string, BiomeSurvivalSkill> _biomeSurvivalSkills = new Dictionary<string, BiomeSurvivalSkill>();

    // Not necessarily ordered, do not use during serialization or algorithms that
    // have a dependency on consistent order
    [XmlIgnore]
    public Dictionary<Direction, CellGroup> Neighbors;

    //#if DEBUG
    //    [XmlIgnore]
    //    public PolityProminence HighestPolityProminence
    //    {
    //        get
    //        {
    //            return _highestPolityProminence;
    //        }
    //        set
    //        {
    //            if ((Cell.Latitude == 108) && (Cell.Longitude == 362))
    //            {
    //                Debug.Log("HighestPolityProminence:set - Cell:" + Cell.Position +
    //                    ((value != null) ?
    //                    (", value.PolityId: " + value.PolityId + ", value.Polity.Id: " + value.Polity.Id) :
    //                    ", null value"));
    //            }

    //            _highestPolityProminence = value;
    //        }
    //    }

    //    private PolityProminence _highestPolityProminence = null;
    //#else
    [XmlIgnore]
    public PolityProminence HighestPolityProminence = null;
    //#endif

    [XmlIgnore]
    public ICollection<Polity> PresentPolities => GetPresentPolities();

    [XmlIgnore]
    public ICollection<Faction> ClosestFactions => GetClosestFactions();

    private Dictionary<Identifier, PolityProminence> _polityProminences =
        new Dictionary<Identifier, PolityProminence>();

    private CellUpdateType _cellUpdateType = CellUpdateType.None;
    private CellUpdateSubType _cellUpdateSubtype = CellUpdateSubType.None;

    private HashSet<Identifier> _polityProminencesToRemove =
        new HashSet<Identifier>();

    private HashSet<string> _flags = new HashSet<string>();

    private bool _alreadyUpdated = false;
    private bool _willBeRemoved = false;

    private List<Effect> _deferredEffects = new List<Effect>();

    private HashSet<string> _properties = new HashSet<string>();

    private HashSet<string> _propertiesToAquire = new HashSet<string>();
    private HashSet<string> _propertiesToLose = new HashSet<string>();

    private bool _hasRemovedProminences = false;
    private bool _hasPromValueDeltas = false;
    private float _unorgBandsPromDelta = 0;
    private Dictionary<Polity, float> _polityPromDeltas =
        new Dictionary<Polity, float>();

    [XmlIgnore]
    public int PreviousPopulation
    {
        get
        {
            return (int)PreviousExactPopulation;
        }
    }

    [XmlIgnore]
    public int Population
    {
        get
        {
            int population = (int)ExactPopulation;

            if (population < 0)
            {
                throw new System.Exception("Negative Population: " + population + ", Id: " + this);
            }

            return population;
        }
    }

    public CellGroup()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    /// <summary>
    /// Creates a new cell group from a migrating population of unorganized bands
    /// </summary>
    /// <param name="bands">migrating bands that will form the group</param>
    public CellGroup(MigratingUnorganizedBands bands) :
        this(
            bands.World,
            bands.TargetCell,
            bands.Population,
            bands.Culture)
    {
    }

    /// <summary>
    /// Creates a new cell group from a migrating polity population
    /// </summary>
    /// <param name="polityPop">migrating population that will form the group</param>
    public CellGroup(MigratingPolityPopulation polityPop) :
        this(
            polityPop.World,
            polityPop.TargetCell,
            polityPop.Population,
            polityPop.Culture)
    {
        bool addProminence = polityPop.Type != MigrationType.Sea;

        if (addProminence)
        {
            AddPolityProminence(polityPop.Polity, 1.0f, true);
            FindHighestPolityProminence();
        }
    }

    public CellGroup(
        World world,
        TerrainCell cell,
        int initialPopulation,
        Culture baseCulture = null)
    {
        World = world;

        LastUpdateDate = World.CurrentDate;

        PreviousExactPopulation = 0;
        ExactPopulation = initialPopulation;

        Cell = cell;
        Longitude = cell.Longitude;
        Latitude = cell.Latitude;

        Cell.Group = this;

        TotalPolityProminenceValue = 0;

        Init(World.CurrentDate, Cell.GenerateInitId());

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CellGroup:constructor - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", initialPopulation: " + initialPopulation +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        Neighbors = new Dictionary<Direction, CellGroup>(8);

        InitializeNeighbors();

        bool initialGroup = false;

        if (baseCulture == null)
        {
            initialGroup = true;
            Culture = new CellCulture(this);
        }
        else
        {
            Culture = new CellCulture(this, baseCulture);
        }

        InitializePreferences(initialGroup);
        InitializeDefaultActivities(initialGroup);
        InitializeDefaultSkills(initialGroup);
        InitializeDefaultKnowledges(initialGroup);

        Culture.Initialize();

        InitializeDefaultEvents();

        World.AddUpdatedGroup(this);
    }

    private void InitializeNeighbors(bool bothDirections = true)
    {
        foreach (var pair in Cell.Neighbors)
        {
            var group = pair.Value.Group;

            if (group != null)
            {
                Neighbors.Add(pair.Key, group);

                if (bothDirections)
                {
                    Direction dir = TerrainCell.ReverseDirection(pair.Key);

                    group.AddNeighbor(dir, this);
                }
            }
        }
    }

    // This accessor ensures neighbors are always accessed in the same order
    // which is important for serialization purposes
    [XmlIgnore]
    public IEnumerable<CellGroup> NeighborGroups
    {
        get
        {
            if (Neighbors.TryGetValue(Direction.North, out CellGroup group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.Northeast, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.East, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.Southeast, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.South, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.Southwest, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.West, out group))
                yield return group;
            if (Neighbors.TryGetValue(Direction.Northwest, out group))
                yield return group;
        }
    }

    /// <summary>
    /// Gets a random polity to migrate if it has a prominence in the group.
    /// It if returns null, then the population to migrate will be unorganized bands
    /// </summary>
    /// <returns>The polity the population to migrate belongs to. Or null if migrating
    /// unorganized bands</returns>
    public Polity GetRandomPopPolityToMigrate()
    {
        // the number of population sets is qual to the number of prominences present
        int popSetCount = _polityProminences.Count;

        if (popSetCount == 0)
        {
            return null;
        }

        if (TotalPolityProminenceValue < 1)
        {
            // Add to the count one if there are still unorganized bands present
            popSetCount++;
        }

        int popIndex = GetNextLocalRandomInt(
            RngOffsets.CELL_GROUP_PICK_MIGRATING_POPULATION, popSetCount);

        int i = 0;
        foreach (PolityProminence prom in _polityProminences.Values)
        {
            if (i == popIndex)
            {
                return prom.Polity;
            }

            i++;
        }

        return null;
    }

    private ICollection<Polity> GetPresentPolities()
    {
        var polities = new List<Polity>();

        foreach (var prominence in _polityProminences.Values)
        {
            polities.Add(prominence.Polity);
        }

        return polities;
    }

    private ICollection<Faction> GetClosestFactions()
    {
        var factions = new List<Faction>();

        foreach (var prominence in _polityProminences.Values)
        {
            factions.Add(prominence.ClosestFaction);
        }

        return factions;
    }

    /// <summary>
    /// Defines population to migrate
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="direction">the direction this group is moving out from the source</param>
    /// <param name="type">the migration type (Land/Sea)</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="polity">the polity whose population will migrate</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public void SetMigratingPopulation(
        TerrainCell targetCell,
        Direction direction,
        MigrationType type,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        Polity polity,
        long startDate,
        long endDate)
    {
        if (polity == null)
        {
            SetMigratingUnorganizedBands(
                targetCell,
                direction,
                type,
                prominencePercent,
                prominenceValueDelta,
                population,
                startDate,
                endDate);
            return;
        }

        SetMigratingPolityPopulation(
            targetCell,
            direction,
            type,
            prominencePercent,
            prominenceValueDelta,
            population,
            polity,
            startDate,
            endDate);
    }

    /// <summary>
    /// Sets Migrating Bands object
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="direction">the direction this group is moving out from the source</param>
    /// <param name="type">the migration type (Land/Sea)</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public void SetMigratingUnorganizedBands(
        TerrainCell targetCell,
        Direction direction,
        MigrationType type,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        long startDate,
        long endDate)
    {
        if (!prominencePercent.IsInsideRange(0, 1))
        {
            Debug.LogWarning("Prominence percent outside of range [0,1]: " + prominencePercent);
            prominencePercent = Mathf.Clamp01(prominencePercent);
        }

        if (MigratingUnorganizedBands == null)
        {
            MigratingUnorganizedBands =
                new MigratingUnorganizedBands(
                    World,
                    prominencePercent,
                    prominenceValueDelta,
                    population,
                    this,
                    targetCell,
                    direction,
                    type,
                    startDate,
                    endDate);
        }
        else
        {
            MigratingUnorganizedBands.Set(
                prominencePercent,
                prominenceValueDelta,
                population,
                this,
                targetCell,
                direction,
                type,
                startDate,
                endDate);
        }

        World.AddMigratingPopulation(MigratingUnorganizedBands);
    }

    /// <summary>
    /// Sets Migrating Polity Population object
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="direction">the direction this group is moving out from the source</param>
    /// <param name="type">the migration type (Land/Sea)</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="polity">the polity whose population will migrate</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public void SetMigratingPolityPopulation(
        TerrainCell targetCell,
        Direction direction,
        MigrationType type,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        Polity polity,
        long startDate,
        long endDate)
    {
        if (!prominencePercent.IsInsideRange(0, 1))
        {
            Debug.LogWarning("Prominence percent outside of range [0,1]: " + prominencePercent);
            prominencePercent = Mathf.Clamp01(prominencePercent);
        }

        if (MigratingPolityPopulation == null)
        {
            MigratingPolityPopulation =
                new MigratingPolityPopulation(
                    World,
                    prominencePercent,
                    prominenceValueDelta,
                    population,
                    this,
                    polity,
                    targetCell,
                    direction,
                    type,
                    startDate,
                    endDate);
        }
        else
        {
            MigratingPolityPopulation.Set(
                prominencePercent,
                prominenceValueDelta,
                population,
                this,
                polity,
                targetCell,
                direction,
                type,
                startDate,
                endDate);
        }

        World.AddMigratingPopulation(MigratingPolityPopulation);
    }

    public void AddDeferredEffect(Effect effect)
    {
        _deferredEffects.Add(effect);
    }

    public void TryAssignEvents()
    {
        foreach (var generator in _generatorsToTestAssignmentFor)
        {
            generator.TryGenerateEventAndAssign(this);
        }

        _generatorsToTestAssignmentFor.Clear();
    }

    public void AddGeneratorToTestAssignmentFor(ICellGroupEventGenerator generator)
    {
        _generatorsToTestAssignmentFor.Add(generator);
        World.AddGroupToAssignEventsTo(this);
    }

    public static void ResetEventGenerators()
    {
        OnSpawnEventGenerators = new List<IWorldEventGenerator>();
        OnCoreHighestProminenceChangeEventGenerators = new List<IWorldEventGenerator>();
        OnCoreCountChangeEventGenerators = new List<IWorldEventGenerator>();
        OnPolityCountChangeEventGenerators = new List<IWorldEventGenerator>();
        OnKnowledgeLevelFallsBelowEventGenerators = new Dictionary<string, List<IWorldEventGenerator>>();
        OnKnowledgeLevelRaisesAboveEventGenerators = new Dictionary<string, List<IWorldEventGenerator>>();
        OnGainedDiscoveryEventGenerators = new Dictionary<string, List<IWorldEventGenerator>>();
        EventGeneratorsThatNeedCleanup = new HashSet<CellGroupEventGenerator>();
    }

    private void InitializeOnSpawnEvents()
    {
        foreach (var generator in OnSpawnEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"CellGroup.InitializeOnSpawnEvents: adding '{context.Id}' to list of events to try to assign");
            }

            if (generator is ICellGroupEventGenerator gGenerator)
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    /// <summary>
    /// Applies the effects of changing the highest prominence on a core group
    /// </summary>
    public void ApplyCoreHighestProminenceChange()
    {
        foreach (var generator in OnCoreHighestProminenceChangeEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"CellGroup.ApplyCoreHighestProminenceChange: adding '{context.Id}' to list of events to try to assign");
            }

            if (generator is IFactionEventGenerator fGenerator)
            {
                foreach (Faction faction in FactionCores.Values)
                {
                    faction.AddGeneratorToTestAssignmentFor(fGenerator);
                }
            }
            else if (generator is ICellGroupEventGenerator gGenerator)
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    public void AddFactionCore(Faction faction)
    {
        if (!FactionCores.ContainsKey(faction.Id))
        {
            FactionCores.Add(faction.Id, faction);
            Manager.AddUpdatedCell(Cell, CellUpdateType.Territory, CellUpdateSubType.Core);
        }

        World.AddGroupWithCoreCountChange(this);
    }

    public void RemoveFactionCore(Faction faction)
    {
        if (FactionCores.ContainsKey(faction.Id))
        {
            FactionCores.Remove(faction.Id);
            Manager.AddUpdatedCell(Cell, CellUpdateType.Territory, CellUpdateSubType.Core);
        }

        World.AddGroupWithCoreCountChange(this);
    }

    public bool FactionHasCoreHere(Faction faction)
    {
        return FactionCores.ContainsKey(faction.Id);
    }

    public ICollection<Faction> GetFactionCores()
    {
        return FactionCores.Values;
    }

    public bool HasPolityOfType(PolityType type)
    {
        foreach (var prominence in _polityProminences.Values)
        {
            if (_polityProminencesToRemove.Contains(prominence.PolityId))
                continue;

            if (type == PolityType.Any)
                return true;

            if (prominence.Polity.Type == type)
                return true;
        }

        return false;
    }

    public CellGroupSnapshot GetSnapshot()
    {
        return new CellGroupSnapshot(this);
    }

    public long GenerateInitId(long idOffset = 0L)
    {
        return Cell.GenerateInitId(idOffset);
    }

    public void SetHighestPolityProminence(PolityProminence prominence)
    {
        if (HighestPolityProminence == prominence)
            return;

        if (Cell.EncompassingTerritory != null)
        {
            Cell.EncompassingTerritory.SetCellToRemove(Cell);
        }

        if (Cell.TerritoryToAddTo != null)
        {
            Cell.TerritoryToAddTo.TryRemoveCellToAdd(Cell);
        }

        if (prominence != null)
        {
            prominence.Polity.Territory.SetCellToAdd(Cell);
        }

        HighestPolityProminence = prominence;

        if (FactionCores.Count > 0)
        {
            ApplyCoreHighestProminenceChange();
        }
    }

    public void InitializeDefaultEvents()
    {
        InitializeOnSpawnEvents();
    }

    /// <summary>
    /// Sets all the preferences this group should start with
    /// </summary>
    /// <param name="initialGroup">indicates if this is one of the world's initial groups</param>
    public void InitializePreferences(bool initialGroup)
    {
        if (initialGroup)
        {
            foreach (PreferenceGenerator generator in World.PreferenceGenerators.Values)
            {
                Culture.AddPreferenceToAcquire(generator.GenerateCellPreference(this, 0.5f));
            }
        }
    }

    public void InitializeDefaultActivities(bool initialGroup)
    {
        if (initialGroup)
        {
            Culture.AddActivityToPerform(
                CellCulturalActivity.CreateActivity(CellCulturalActivity.ForagingActivityId, this, 1f, 1f));
        }

        if (Cell.NeighborhoodWaterBiomePresence > 0)
        {
            Culture.AddActivityToPerform(
                CellCulturalActivity.CreateActivity(CellCulturalActivity.FishingActivityId, this));
        }
    }

    public void InitializeDefaultKnowledges(bool initialGroup)
    {
        if (initialGroup)
        {
            Culture.AddKnowledgeToLearn(
                SocialOrganizationKnowledge.KnowledgeId, 
                SocialOrganizationKnowledge.InitialValue, 
                SocialOrganizationKnowledge.InitialLimit);
        }
    }

    public void SetFlag(string flag)
    {
        _flags.Add(flag);
    }

    public bool IsFlagSet(string flag)
    {
        return _flags.Contains(flag);
    }

    public void UnsetFlag(string flag)
    {
        _flags.Remove(flag);
    }

    public int GetNextLocalRandomInt(int iterationOffset, int maxValue)
    {
        return Cell.GetNextLocalRandomInt(iterationOffset, maxValue);
    }

    public int GetLocalRandomInt(long date, int iterationOffset, int maxValue)
    {
        return Cell.GetLocalRandomInt(date, iterationOffset, maxValue);
    }

    public float GetNextLocalRandomFloat(int iterationOffset)
    {
        return Cell.GetNextLocalRandomFloat(iterationOffset);
    }

    public float GetLocalRandomFloat(long date, int iterationOffset)
    {
        return Cell.GetLocalRandomFloat(date, iterationOffset);
    }

    public void AddNeighbor(Direction direction, CellGroup group)
    {
        if (group == null)
            return;

        if (!group.StillPresent)
            return;

        if (Neighbors.ContainsValue(group))
            return;

        Neighbors.Add(direction, group);
    }

    public void RemoveNeighbor(Direction direction)
    {
        if (!Neighbors.ContainsKey(direction))
            return;

        Neighbors.Remove(direction);
    }

    public void InitializeDefaultSkills(bool initialGroup)
    {
        float baseValue = 0;
        if (initialGroup)
        {
            baseValue = 1f;
        }

        foreach (Biome biome in GetPresentBiomesInNeighborhood())
        {
            if (biome.TerrainType != BiomeTerrainType.Water)
            {
                string skillId = biome.SkillId;

                if (Culture.GetSkill(skillId) == null)
                {
                    Culture.AddSkillToLearn(new BiomeSurvivalSkill(this, biome, baseValue));
                }
            }
        }
    }

    public void AddBiomeSurvivalSkill(BiomeSurvivalSkill skill)
    {
        if (_biomeSurvivalSkills.ContainsKey(skill.BiomeId))
        {
            Debug.Break();
            throw new System.Exception("Debug.Break");
        }

        _biomeSurvivalSkills.Add(skill.BiomeId, skill);
    }

    public HashSet<Biome> GetPresentBiomesInNeighborhood()
    {
        HashSet<Biome> biomes = new HashSet<Biome>();

        foreach (string id in Cell.PresentBiomeIds)
        {
            biomes.Add(Biome.Biomes[id]);
        }

        foreach (TerrainCell neighborCell in Cell.NeighborList)
        {
            foreach (string id in neighborCell.PresentBiomeIds)
            {
                biomes.Add(Biome.Biomes[id]);
            }
        }

        return biomes;
    }

    /// <summary>
    /// Modifies the group's current population
    /// </summary>
    /// <param name="popDelta">amount of population to add or remove from the group</param>
    public void ChangePopulation(float popDelta)
    {
        ExactPopulation += popDelta;

//#if DEBUG
//        if (ExactPopulation < 0)
//        {
//            Debug.LogWarning(
//                "Exact Population changed to less than zero: " + ExactPopulation +
//                ", Group: " + Id);
//        }
//#endif

        ExactPopulation = Mathf.Max(0, ExactPopulation);
    }

    public void ExecuteDeferredEffects()
    {
        foreach (Effect effect in _deferredEffects)
        {
            effect.Apply(this);
        }

        _deferredEffects.Clear();
    }

    public void UpdateProperties()
    {
        foreach (string property in _propertiesToAquire)
        {
            AddProperty(property);
        }

        foreach (string property in _propertiesToLose)
        {
            RemoveProperty(property);
        }

        _propertiesToAquire.Clear();
        _propertiesToLose.Clear();
    }

    /// <summary>
    /// Performs post update operations for this group before polities are updates, and before
    /// all step 2 group post updates are performed
    public void PostUpdate_BeforePolityUpdates_Step1()
    {
        _alreadyUpdated = false;

        if (Population < 2)
        {
            _willBeRemoved = true;

            World.AddGroupToRemove(this);
            return;
        }

        UpdateTerrainAttributes();

        UpdateTerrainFarmlandPercentage();

        Culture.PostUpdate();

        UpdateProperties();

        SetFactionUpdates();

        Culture.CleanUpAtributesToGet();

        SetPolityUpdates();

        PostUpdatePolityProminences();

        PostUpdateProminenceCulturalProperties();
    }

    /// <summary>
    /// Performs post update operations for this group before polities are updates, but after
    /// all step 1 group post updates have been performed
    /// </summary>
    public void PostUpdate_BeforePolityUpdates_Step2()
    {
        if (_willBeRemoved)
        {
            return;
        }

        PostUpdateProminences();
    }

    public void PostUpdate_AfterPolityUpdates()
    {
        // These operations might have been done already for this group in
        // PostUpdate_BeforePolityUpdates_Step1. This is ok since we can't
        // be sure if a group might get affected by a polity update after
        // it has already been updated

        PostUpdatePolityProminences(true);

        PostUpdateProminenceCulturalProperties();
    }

    public void SetToBecomeFactionCore(Faction faction)
    {
        WillBecomeCoreOfFaction = faction;

        World.AddGroupToCleanupAfterUpdate(this);
    }

    public void AfterUpdateCleanup()
    {
        WillBecomeCoreOfFaction = null;
    }

    public bool InfluencingPolityHasKnowledge(string id)
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            if (pi.Polity.Culture.HasKnowledge(id))
            {
                return true;
            }
        }

        return false;
    }

    public void SetupForNextUpdate()
    {
        if (!StillPresent)
            return;

        World.UpdateMostPopulousGroup(this);

        //Profiler.BeginSample("Calculate Optimal Population");

        OptimalPopulation = Cell.EstimateOptimalPopulation(Culture);

        //Profiler.EndSample();

        //Profiler.BeginSample("Consider Land Migration");

        ConsiderLandMigration();

        //Profiler.EndSample();

        //Profiler.BeginSample("Consider Sea Migration");

        ConsiderSeaMigration();

        //Profiler.EndSample();

        //Profiler.BeginSample("Calculate Next Update Date");

        NextUpdateDate = CalculateNextUpdateDate();

        //Profiler.EndSample();

        LastUpdateDate = World.CurrentDate;

        if (NextUpdateDate < 0)
        {
            // Do not generate event
            return;
        }

        if (UpdateEvent == null)
        {
            UpdateEvent = new UpdateCellGroupEvent(this, NextUpdateDate);
        }
        else
        {
            UpdateEvent.Reset(NextUpdateDate);
        }

        UpdateEventSpawnDate = UpdateEvent.SpawnDate;

        World.InsertEventToHappen(UpdateEvent);

        _cellUpdateType |= CellUpdateType.Group;
        _cellUpdateSubtype |= CellUpdateSubType.PopulationAndCulture;

        Manager.AddUpdatedCell(Cell, _cellUpdateType, _cellUpdateSubtype);

        _cellUpdateType = CellUpdateType.None;
        _cellUpdateSubtype = CellUpdateSubType.None;
    }

    public long GeneratePastSpawnDate(long baseDate, int cycleLength, int offset = 0)
    {
        long currentDate = World.CurrentDate;
        long startCycleDate = baseDate + GetLocalRandomInt(baseDate, offset, cycleLength);
        long currentCycleDate = currentDate - (currentDate - startCycleDate) % cycleLength;
        long spawnDate = currentCycleDate + GetLocalRandomInt(currentCycleDate, offset, cycleLength);

        if (currentDate < spawnDate)
        {
            if (currentCycleDate == startCycleDate)
            {
                return baseDate;
            }

            long prevCycleDate = currentCycleDate - cycleLength;

            long prevSpawnDate = prevCycleDate + GetLocalRandomInt(prevCycleDate, offset, cycleLength);

            return prevSpawnDate;
        }

        return spawnDate;
    }

    public long GenerateFutureSpawnDate(long baseDate, int cycleLength, int offset = 0)
    {
        long currentDate = World.CurrentDate;

        long startCycleDate = baseDate + GetLocalRandomInt(baseDate, offset, cycleLength);

        long currentCycleDate = currentDate - (currentDate - startCycleDate) % cycleLength;

        long spawnDate = currentCycleDate + GetLocalRandomInt(currentCycleDate, offset, cycleLength);

        if (currentDate >= spawnDate)
        {

            long nextCycleDate = currentCycleDate + cycleLength;

            long nextSpawnDate = nextCycleDate + GetLocalRandomInt(nextCycleDate, offset, cycleLength);

            return nextSpawnDate;
        }

        return spawnDate;
    }

    public void TriggerInterference()
    {
        ResetSeaMigrationRoute();
    }

    public void ResetSeaMigrationRoute()
    {
        if (SeaMigrationRoute == null)
            return;

        SeaMigrationRoute.Reset();
    }

    public void DestroySeaMigrationRoute()
    {
        if (SeaMigrationRoute == null)
            return;

        SeaMigrationRoute.Destroy();
        SeaMigrationRoute = null;
    }

    public void GenerateSeaMigrationRoute()
    {
        if (!Cell.IsPartOfCoastline)
            return;

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                bool routePresent = SeaMigrationRoute == null;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "GenerateSeaMigrationRoute - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", route present: " + routePresent +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (SeaMigrationRoute == null)
        {
            SeaMigrationRoute = new Route(Cell);
        }
        else
        {
            if (SeaMigrationRoute.FirstCell == null)
            {
                throw new System.Exception("SeaMigrationRoute.FirstCell is null at " + Cell.Position);
            }

            SeaMigrationRoute.Erase();
            SeaMigrationRoute.Build();
        }

        if (SeaMigrationRoute.LastCell == null)
            return;

        if (SeaMigrationRoute.LastCell == SeaMigrationRoute.FirstCell)
            return;

        if (SeaMigrationRoute.MigrationDirection == Direction.Null)
            return;

        if (SeaMigrationRoute.FirstCell.Neighbors.ContainsValue(SeaMigrationRoute.LastCell))
            return;

        SeaMigrationRoute.Consolidate();

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {

        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                TerrainCell targetCell = SeaMigrationRoute.LastCell;

        //                string cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "GenerateSeaMigrationRoute - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", target cell: " + cellInfo +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif
    }

    /// <summary>
    /// Calculates the chance of a successful migration to the target cell.
    /// </summary>
    /// <param name="cell">the target cell</param>
    /// <param name="targetValue">the relative value of the target cell as a migration
    /// target</param>
    /// <param name="migratingPolity">the polity that intend to migrate
    /// (null if migrating unorganized bands)</param>
    /// <returns>Migration chance as a value between 0 and 1</returns>
    public float CalculateMigrationChance(
        TerrainCell cell,
        out float targetValue,
        Polity migratingPolity = null)
    {
        float offset = -0.1f * (1 - MigrationPressure);
        targetValue = cell.CalculateRelativeMigrationValue(this, migratingPolity);

        float unbiasedChance = Mathf.Clamp01(targetValue + offset);

        return unbiasedChance;
    }

    /// <summary>
    /// Evaluates and chooses a neighbor land cell as a migration target
    /// </summary>
    private void ConsiderLandMigration()
    {
        if (HasMigrationEvent)
            return;

        Polity polity = GetRandomPopPolityToMigrate();

        int targetCellIndex =
            Cell.GetNextLocalRandomInt(
                RngOffsets.CELL_GROUP_PICK_MIGRATION_DIRECTION,
                Cell.NeighborList.Count);

        TerrainCell targetCell = Cell.NeighborList[targetCellIndex];
        Direction migrationDirection = Cell.DirectionList[targetCellIndex];

//#if DEBUG
//        if (Cell.IsSelected)
//        {
//            Debug.LogWarning("Debugging ConsiderLandMigration for cell " + Cell.Position);
//        }
//#endif

        float cellChance = CalculateMigrationChance(targetCell, out float targetValue, polity);

        if (cellChance <= 0)
            return;

        Identifier polityId = polity?.Id;

        float prominencePercent = CalculateProminencePercentToMigrate(targetValue, polityId);

        if (prominencePercent <= 0)
            return;

        float attemptValue =
            Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION_CHANCE);

        if (attemptValue > cellChance)
            return;

        targetCell.CalculateAdaptation(Culture, out _, out float cellSurvivability);

        if (cellSurvivability <= 0)
            return;

        float cellAltitudeDeltaFactor =
            targetCell.CalculateMigrationAltitudeDeltaFactor(Cell);

        float travelFactor =
            cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
            cellSurvivability * cellSurvivability * targetCell.Accessibility;

        travelFactor = Mathf.Clamp(travelFactor, 0.0001f, 1);

        float travelSlownessConstant = 0.01f;

        float travelTime =
            Mathf.Ceil(travelSlownessConstant *
            World.YearLength * Cell.Width / (TravelWidthFactor * travelFactor));

        long arrivalDate = World.CurrentDate + (long)travelTime;

        if (arrivalDate <= World.CurrentDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderLandMigration - nextDate (" + arrivalDate +
                ") less or equal to World.CurrentDate (" + World.CurrentDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width +
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }
        else if (arrivalDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderLandMigration - nextDate (" + arrivalDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width +
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }

        SetPopulationMigrationEvent(
            targetCell,
            migrationDirection,
            MigrationType.Land,
            prominencePercent,
            polityId,
            arrivalDate);
    }

    /// <summary>
    /// Evaluates and chooses a land cell across a body of water as a migration target
    /// </summary>
    private void ConsiderSeaMigration()
    {
        if (SeaTravelFactor <= 0)
            return;

        if (HasMigrationEvent)
            return;

        Polity polity = GetRandomPopPolityToMigrate();

        if ((SeaMigrationRoute == null) ||
            (!SeaMigrationRoute.Consolidated))
        {
            GenerateSeaMigrationRoute();

            if ((SeaMigrationRoute == null) ||
                (!SeaMigrationRoute.Consolidated))
                return;
        }

        TerrainCell targetCell = SeaMigrationRoute.LastCell;
        Direction migrationDirection = SeaMigrationRoute.MigrationDirection;

        if (targetCell == Cell)
        {
            throw new System.Exception("target cell is equal to origin cell. Group: " + Id);
        }

        if (targetCell == null)
        {
            throw new System.Exception("target cell is null. Group: " + Id);
        }

        float cellChance = CalculateMigrationChance(targetCell, out float targetValue, polity);

        if (cellChance <= 0)
            return;

        Identifier polityId = polity?.Id;

        float prominencePercent = CalculateProminencePercentToMigrate(targetValue, polityId);

        if (prominencePercent <= 0)
            return;

        targetCell.CalculateAdaptation(Culture, out _, out float cellSurvivability);

        if (cellSurvivability <= 0)
            return;

        float routeLength = SeaMigrationRoute.Length;
        float routeLengthFactor = Mathf.Pow(routeLength / 1000, 2) * routeLength;

        float migrationChance = cellChance * SeaTravelFactor / (SeaTravelFactor + routeLengthFactor);

        float attemptValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_SEA_MIGRATION);

        if (attemptValue > migrationChance)
            return;

        int travelTime = (int)Mathf.Ceil(World.YearLength * routeLength / SeaTravelFactor);

        long nextDate = World.CurrentDate + travelTime;

        if (nextDate <= World.CurrentDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderSeaMigration - nextDate (" + nextDate +
                ") less or equal to World.CurrentDate (" + World.CurrentDate +
                "). travelTime: " + travelTime + ", routeLength: " + routeLength + ", SeaTravelFactor: " + SeaTravelFactor);

            // Do not generate event
            return;
        }
        else if (nextDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderSeaMigration - nextDate (" + nextDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). travelTime: " + travelTime + ", routeLength: " + routeLength + ", SeaTravelFactor: " + SeaTravelFactor);

            // Do not generate event
            return;
        }

        SetPopulationMigrationEvent(
            targetCell,
            migrationDirection,
            MigrationType.Sea,
            prominencePercent,
            polity?.Id,
            nextDate);
    }

    /// <summary>
    /// Calculates the percent of the prominence population to migrate during
    /// a migration event
    /// </summary>
    /// <param name="cellValue">the migration value of the target cell</param>
    /// <param name="polityId">the id of the polity to migrate (null for unorganized bands)</param>
    /// <returns>the percent of prominence population to migrate</returns>
    private float CalculateProminencePercentToMigrate(float cellValue, Identifier polityId)
    {
        float prominenceValue = GetPolityProminenceValue(polityId);

        float promPop = ExactPopulation * prominenceValue;

        if (promPop < MinProminencePopulation)
            return 0;

        float minProminenceVal = prominenceValue * MinProminencePopulation / promPop;

        cellValue = Mathf.Clamp01(cellValue);

        float valueFactor = cellValue / (cellValue + 1);

        float randomFactor = GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_PICK_PROMINENCE_PERCENT);

        float prominenceValToMove = minProminenceVal + (prominenceValue - minProminenceVal) * valueFactor * randomFactor;

        float promPercent = prominenceValToMove / prominenceValue;

        return Mathf.Clamp01(promPercent);
    }

    /// <summary>
    /// Resets of generates a new population migration event
    /// </summary>
    /// <param name="targetCell">cell which the group population will migrate toward</param>
    /// <param name="migrationDirection">direction toward which the migration will occur</param>
    /// <param name="migrationType">'Land' or 'Sea' migration</param>
    /// <param name="prominencePercent">prominence percent to migrate</param>
    /// <param name="nextDate">the next date on which this event should trigger</param>
    private void SetPopulationMigrationEvent(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        float prominencePercent,
        Identifier polityId,
        long nextDate)
    {
        if (PopulationMigrationEvent == null)
        {
            PopulationMigrationEvent =
                new MigratePopulationEvent(
                    this,
                    targetCell,
                    migrationDirection,
                    migrationType,
                    prominencePercent,
                    polityId,
                    nextDate);
        }
        else
        {
            PopulationMigrationEvent.Reset(
                targetCell,
                migrationDirection,
                migrationType,
                prominencePercent,
                polityId,
                nextDate);
        }

        World.InsertEventToHappen(PopulationMigrationEvent);

        HasMigrationEvent = true;

        MigrationEventDate = nextDate;
        MigrationEventSpawnDate = PopulationMigrationEvent.SpawnDate;
        MigrationTargetLongitude = targetCell.Longitude;
        MigrationTargetLatitude = targetCell.Latitude;
        MigrationEventDirectionInt = (int)migrationDirection;
        MigrationEventTypeInt = (int)migrationType;
        MigrationProminencePercent = prominencePercent;
        MigratingPopPolId = polityId;
    }

    public void Destroy()
    {
        StillPresent = false;

        foreach (Faction faction in GetFactionCores())
        {
            faction.SetToRemove();
        }

        Destroy_RemovePolityProminences();

        Cell.Group = null;
        World.RemoveGroup(this);

        foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors)
        {
            pair.Value.RemoveNeighbor(TerrainCell.ReverseDirection(pair.Key));
        }

        DestroySeaMigrationRoute();

        Cell.FarmlandPercentage = 0;
        Cell.Accessibility = Cell.BaseAccessibility;
        Cell.Arability = Cell.BaseArability;

        foreach (var generator in EventGeneratorsThatNeedCleanup)
        {
            generator.RemoveReferences(this);
        }

        _cellUpdateType |= CellUpdateType.Cell;
        _cellUpdateSubtype |= CellUpdateSubType.Terrain;
    }

    public void Destroy_RemovePolityProminences() // This should be called only when destroying a group
    {
        // Make sure all influencing polities get updated
        SetPolityUpdates(true);

        PolityProminence[] polityProminences = new PolityProminence[_polityProminences.Count];
        _polityProminences.Values.CopyTo(polityProminences, 0);

        foreach (PolityProminence polityProminence in polityProminences)
        {
            Polity polity = polityProminence.Polity;

            polity.RemoveGroup(polityProminence);

            // We want to update the polity if a group is removed.
            SetPolityUpdate(polityProminence, true);

            polityProminence.Destroy();
        }

        if (Cell.TerritoryToAddTo != null)
        {
            Cell.TerritoryToAddTo.TryRemoveCellToAdd(Cell);
        }

        if (Cell.EncompassingTerritory != null)
        {

//#if DEBUG
//            if (Cell.Position.Equals(6, 111))
//            {
//                Debug.LogWarning("Debugging Destroy_RemovePolityProminences, cell: " + Cell.Position + ", group: " +
//                    Cell.Group + ", polity: " + Cell.EncompassingTerritory.Polity.Id);
//            }
//#endif

            Cell.EncompassingTerritory.SetCellToRemove(Cell);
        }

        Cell.TryResetGroupPolityProminenceList(true, false);
    }

#if DEBUG
    public delegate void UpdateCalledDelegate();

    public static UpdateCalledDelegate UpdateCalled = null;
#endif

    /// <summary>
    /// Performs all update operations on a group without finalizing them, which is
    /// done during the post updates
    /// </summary>
    public void Update()
    {
        if (!StillPresent)
        {
            Debug.LogWarning("Group is no longer present. Id: " + this);
            return;
        }

        if (_alreadyUpdated)
            return;

        PreviousExactPopulation = ExactPopulation;

        long timeSpan = World.CurrentDate - LastUpdateDate;

        if (timeSpan <= 0)
            return;

#if DEBUG
        UpdateCalled?.Invoke();
#endif

        _alreadyUpdated = true;

        //Profiler.BeginSample("Update Population");

        UpdatePopulation(timeSpan);

        //Profiler.EndSample();

        //Profiler.BeginSample("Update Culture");

        Culture.Update(timeSpan);

        //Profiler.EndSample();

        //Profiler.BeginSample("Update Polity Prominences");

        UpdatePolityProminences(timeSpan);

        //Profiler.EndSample();

        //Profiler.BeginSample("Update Prominence Cultural Promerties");

        UpdateProminenceCulturalProperties(timeSpan);

        //Profiler.EndSample();

        //Profiler.BeginSample("Polity Update Effects");

        PolityUpdateEffects(timeSpan);

        //Profiler.EndSample();

        //Profiler.BeginSample("Update Travel Factors");

        UpdateSeaTravelFactor();

        //Profiler.EndSample();

        //Profiler.BeginSample("Update Add Updated Group");

        World.AddUpdatedGroup(this);

        //Profiler.EndSample();
    }

    private void SetFactionUpdates()
    {
        foreach (Faction faction in FactionCores.Values)
        {
            World.AddFactionToUpdate(faction);
        }
    }

    private void SetPolityUpdates(bool forceUpdate = false)
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            SetPolityUpdate(pi, forceUpdate);
        }
    }

    public void SetPolityUpdate(PolityProminence pi, bool forceUpdate)
    {
        Polity p = pi.Polity;

        int groupCount = p.Groups.Count;

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Id == Manager.TracingData.GroupId)
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
                    "SetPolityUpdate - Group Id: " + this,
                    "CurrentDate: " + World.CurrentDate +
                    ", forceUpdate: " + forceUpdate +
                    ", polity Id: " + p.Id +
                    //", p.WillBeUpdated: " + p.WillBeUpdated +
                    ", (p.CoreGroup == this): " + (p.CoreGroup == this) +
                    ", groupCount: " + groupCount +
                    ", caller: " + callingClass + "::" + callingMethod +
                    "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (p.WillBeUpdated)
            return;

        if (groupCount <= 0)
            return;

        if (forceUpdate || (p.CoreGroup == this))
        {
            World.AddPolityToUpdate(p);
            return;
        }

        // If group is not the core group then there's a chance no polity update will happen

        float chanceFactor = 1f / (float)groupCount;

        int offset = RngOffsets.CELL_GROUP_SET_POLITY_UPDATE + unchecked(p.GetHashCode());

        float rollValue = Cell.GetNextLocalRandomFloat(offset, registerForTesting: false);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //            System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
        //            string callingMethod = method.Name;

        //            //				int frame = 2;
        //            //				while (callingMethod.Contains ("GetNextLocalRandom") || callingMethod.Contains ("GetNextRandom")) {
        //            //					method = stackTrace.GetFrame(frame).GetMethod();
        //            //					callingMethod = method.Name;
        //            //
        //            //					frame++;
        //            //				}

        //            string callingClass = method.DeclaringType.ToString();

        //            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                "SetPolityUpdate - After roll - Group:" + Id,
        //                "CurrentDate: " + World.CurrentDate +
        //                ", polity Id: " + p.Id +
        //                ", chanceFactor: " + chanceFactor +
        //                ", rollValue: " + rollValue +
        //                ", forceUpdate: " + forceUpdate +
        //                ", caller: " + callingClass + "::" + callingMethod +
        //                "");

        //            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //        }
        //#endif

        if (rollValue <= chanceFactor)
            World.AddPolityToUpdate(p);
    }

    private void UpdatePopulation(long timeSpan)
    {
        ExactPopulation = PopulationAfterTime(timeSpan);
    }

    /// <summary>
    /// Updates the groups polity prominences
    /// </summary>
    /// <param name="timeSpan">the time span since the last cell update</param>
    private void UpdatePolityProminences(long timeSpan)
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            UpdatePolityProminence(pi, timeSpan);
        }
    }

    /// <summary>
    /// Updates a polity prominence in the group
    /// </summary>
    /// <param name="polityProminence">the prominence to update</param>
    /// <param name="timeSpan">the time span since the last cell update</param>
    private void UpdatePolityProminence(PolityProminence polityProminence, long timeSpan)
    {
        EvaluateAcculturation(polityProminence, timeSpan);
    }

    /// <summary>
    /// Evaluate the acculturation effect
    /// </summary>
    /// <param name="prominence">the prominence to evaluate acculturation for</param>
    /// <param name="timeSpan">the time span since the last cell update</param>
    private void EvaluateAcculturation(PolityProminence prominence, long timeSpan)
    {
        if (prominence.FactionCoreDistance < 0)
        {
            // the faction core distance is still not set, so avoid applying acculturation effects
            // just yet
            return;
        }

        if (_polityProminencesToRemove.Contains(prominence.PolityId))
        {
            // since this prominence will be removed, there's no need to calculate acculturation effects
            return;
        }

        float expectedSpanConstant = GenerationSpan;
        float timeFactor = timeSpan / (timeSpan + expectedSpanConstant);

        Culture polityCulture = prominence.Polity.Culture;

        float polityIsolationPrefValue =
            polityCulture.GetIsolationPreferenceValue();

        float groupIsolationPrefValue =
            Culture.GetIsolationPreferenceValue();

        float maxCoreDist = 1000;

        float coreDistFactor = prominence.FactionCoreDistance / maxCoreDist;
        coreDistFactor = Mathf.Pow(coreDistFactor, 2);

        // acculturation on unorganized bands

        float ubTransferConstant = 3;

        float ubCoreDistanceFactor = ubTransferConstant - coreDistFactor;

        float ubOpennessFactor = 1 - polityIsolationPrefValue;
        float prominenceOnUBFactor = prominence.Value;

        if (ubCoreDistanceFactor < 0)
        {
            ubOpennessFactor = 1 - groupIsolationPrefValue;
            prominenceOnUBFactor = 1;
        }

        int polityIdHash = prominence.PolityId.GetHashCode();

        if ((ubCoreDistanceFactor < 0) || (TotalPolityProminenceValue < 1))
        {
            float ubRandomFactor = 0.8f + 0.4f * Cell.GetNextLocalRandomFloat(
                RngOffsets.CELL_GROUP_UB_ACCULTURATION + polityIdHash);

            float ubAcculturation =
                ubOpennessFactor *
                ubCoreDistanceFactor *
                prominenceOnUBFactor *
                ubRandomFactor *
                timeFactor;

            ubAcculturation = Mathf.Clamp(ubAcculturation, -0.75f, 0.75f);

            AddUBandsProminenceValueDelta(-ubAcculturation);
        }

        // acculturation on prominence from other prominences

        float othersOnPromFactor = TotalPolityProminenceValue - prominence.Value;

        if (othersOnPromFactor > 0)
        {
            float promRandomFactor = 0.8f + 0.4f * Cell.GetNextLocalRandomFloat(
                RngOffsets.CELL_GROUP_PROM_ACCULTURATION + polityIdHash);

            float promOpennessFactor = 1 - polityIsolationPrefValue;

            float promAcculturation =
                promOpennessFactor *
                othersOnPromFactor *
                promRandomFactor *
                timeFactor *
                coreDistFactor;

            promAcculturation = Mathf.Clamp(promAcculturation, 0, 0.5f);

            AddPolityProminenceValueDelta(prominence.Polity, -promAcculturation);
        }
    }

    /// <summary>
    /// Updates a cell's culture with the influence of its prominences
    /// </summary>
    /// <param name="timeSpan">the time span since the last cell update</param>
    private void UpdateProminenceCulturalProperties(long timeSpan)
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            Culture.UpdateProminenceCulturalProperties(pi, timeSpan);
        }
    }

    /// <summary>
    /// Post updates a cell culture through the influence of its polity prominences
    /// </summary>
    private void PostUpdateProminenceCulturalProperties()
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            Culture.PostUpdateProminenceCulturalProperties(pi);
        }
    }

    private void PolityUpdateEffects(long timeSpan)
    {
        foreach (PolityProminence polityProminence in _polityProminences.Values)
        {
            if (_polityProminencesToRemove.Contains(polityProminence.PolityId))
                continue;

            Polity polity = polityProminence.Polity;

            polity.GroupUpdateEffects(this, polityProminence.Value, TotalPolityProminenceValue, timeSpan);
        }

        if (HasTribeFormationEvent)
            return;

        if (TribeFormationEvent.CanSpawnIn(this))
        {
            long triggerDate = TribeFormationEvent.CalculateTriggerDate(this);

            if (triggerDate == int.MinValue)
                return;

            if (TribeFormationEvent == null)
            {
                TribeFormationEvent = new TribeFormationEvent(this, triggerDate);
            }
            else
            {
                TribeFormationEvent.Reset(triggerDate);
            }

            World.InsertEventToHappen(TribeFormationEvent);

            HasTribeFormationEvent = true;

            TribeFormationEventDate = triggerDate;
        }
    }

    private void UpdateTerrainAttributes()
    {
        if (ArabilityModifier > 0)
        {
            float modifiedArability = Cell.BaseArability + (1 - Cell.BaseArability) * ScaledArabilityModifier;
            modifiedArability = Mathf.Clamp01(modifiedArability);

            if (modifiedArability != Cell.Arability)
            {
                Cell.Arability = modifiedArability;
                Cell.Modified = true; // We need to make sure to store the cell changes to file when saving.

                _cellUpdateType |= CellUpdateType.Cell;
                _cellUpdateSubtype |= CellUpdateSubType.Terrain;
            }
        }

        if (AccessibilityModifier > 0)
        {
            float modifiedAccessibility = Cell.BaseAccessibility + (1 - Cell.BaseAccessibility) * ScaledAccessibilityModifier;
            modifiedAccessibility = Mathf.Clamp01(modifiedAccessibility);

            if (modifiedAccessibility != Cell.Accessibility)
            {
                Cell.Accessibility = modifiedAccessibility;
                Cell.Modified = true; // We need to make sure to store the cell changes to file when saving.

                _cellUpdateType |= CellUpdateType.Cell;
                _cellUpdateSubtype |= CellUpdateSubType.Terrain;
            }
        }
    }

    private void UpdateTerrainFarmlandPercentage()
    {
        AgricultureKnowledge knowledge =
            Culture.GetKnowledge(AgricultureKnowledge.KnowledgeId) as AgricultureKnowledge;

        if (knowledge == null)
        {
            if (Cell.FarmlandPercentage > 0)
            {
                Cell.FarmlandPercentage = 0;

                _cellUpdateType |= CellUpdateType.Cell;
                _cellUpdateSubtype |= CellUpdateSubType.Terrain;
            }

            return;
        }

        float knowledgeValue = knowledge.ScaledValue;

        float techValue = Mathf.Sqrt(knowledgeValue);

        float areaPerFarmWorker = techValue / 5f;

        float terrainFactor = knowledge.TerrainFactor;

        float farmingPopulation =
            Cell.GetActivityContribution(Culture, CulturalActivity.FarmingActivityId) * Population;

        float maxWorkableArea = areaPerFarmWorker * farmingPopulation;

        float availableArea = Cell.Area * terrainFactor;

        float farmlandPercentage = 0;

        if ((maxWorkableArea > 0) && (availableArea > 0))
        {
            float farmlandPercentageAvailableArea = maxWorkableArea / (maxWorkableArea + availableArea);

            farmlandPercentage = farmlandPercentageAvailableArea * terrainFactor;
        }

        if (farmlandPercentage != Cell.FarmlandPercentage)
        {
            Cell.FarmlandPercentage = farmlandPercentage;
            Cell.Modified = true; // We need to make sure to store the cell changes to file when saving.

            _cellUpdateType |= CellUpdateType.Cell;
            _cellUpdateSubtype |= CellUpdateSubType.Terrain;
        }
    }

    /// <summary>
    /// Updates the travel factor for all sea voyages
    /// </summary>
    public void UpdateSeaTravelFactor()
    {
        Culture.TryGetSkillValue(SeafaringSkill.SkillId, out float seafaringValue);
        Culture.TryGetKnowledgeScaledValue(ShipbuildingKnowledge.KnowledgeId, out float shipbuildingValue);

        float rangeFactor = 1 + (NavigationRangeModifier * MathUtility.IntToFloatScalingFactor);

        SeaTravelFactor =
            SeaTravelBaseFactor * seafaringValue * shipbuildingValue * TravelWidthFactor * rangeFactor;
    }

    /// <summary>
    /// Calculates how much pressure there is to migrate
    /// out of this cell
    /// </summary>
    /// <param name="migratingPolity">the polity the pressure will be calculated for</param>
    /// <returns>the pressure value</returns>
    public float CalculateNeighborhoodMigrationPressure(Polity migratingPolity)
    {
        Profiler.BeginSample("CalculateMigrationPressure");

        float prominenceValue = GetPolityProminenceValue(migratingPolity);

        if (prominenceValue <= 0)
            return 0;

        float prominenceFactor = Mathf.Clamp01(20f * prominenceValue);

        float neighborhoodValue = 0;
        foreach (TerrainCell cell in Cell.NeighborList)
        {
            // 1 is the top value possible. No need to evaluate further
            if (neighborhoodValue >= 1)
                break;

            neighborhoodValue =
                Mathf.Max(
                    neighborhoodValue,
                    cell.CalculateRelativeMigrationValue(this, migratingPolity));
        }

        Profiler.EndSample(); // ("CalculateMigrationPressure");

        return Mathf.Clamp01(neighborhoodValue * prominenceFactor);
    }

    /// <summary>
    /// Calculates how much pressure there is for population sets to migrate
    /// out of this cell
    /// </summary>
    /// <returns>the migration presure value</returns>
    public float CalculateOverallMigrationPressure()
    {
        // Reset stored pressures
        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            prominence.MigrationPressure = 0;
        }
        MigrationPressure = 0;

        // There's low pressure if there's already a migration event occurring
        if (HasMigrationEvent)
            return 0;

        float populationFactor;
        if (OptimalPopulation > 0)
        {
            populationFactor = Population / (float)OptimalPopulation;
        }
        else
        {
            // Set stored pressures to 1
            foreach (PolityProminence prominence in _polityProminences.Values)
            {
                prominence.MigrationPressure = 1;
            }
            MigrationPressure = 1;

            return 1;
        }

        float minPopulationConstant = 0.90f;

        // if the population is not near its optimum then don't add pressure
        if (populationFactor < minPopulationConstant)
            return 0;

        // Get the pressure from unorganized bands
        float pressure = CalculateNeighborhoodMigrationPressure(null);

        // Get the pressure from polity populations
        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            float pPressure = CalculateNeighborhoodMigrationPressure(prominence.Polity);
            prominence.MigrationPressure = pPressure;

            pressure = Mathf.Max(pressure, pPressure);
        }

        MigrationPressure = pressure;

        return pressure;
    }

    public long CalculateNextUpdateDate()
    {
#if DEBUG
        if (FactionCores.Count > 0)
        {
            foreach (Faction faction in FactionCores.Values)
            {
                if (faction.CoreGroupId != Id)
                {
                    throw new System.Exception(
                        "Group identifies as faction core when it no longer is. Id: " + this +
                        ", CoreId: " + faction.CoreGroupId + ", current date: " + World.CurrentDate);
                }
            }
        }
#endif

        float randomFactor = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CALCULATE_NEXT_UPDATE);
        randomFactor = 1f - Mathf.Pow(randomFactor, 4);

        float migrationPressure = CalculateOverallMigrationPressure();
        float migrationFactor = 1 - migrationPressure;
        migrationFactor = Mathf.Pow(migrationFactor, 4f);

        float skillLevelFactor = Culture.MinimumSkillAdaptationLevel();
        float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel();

        float circumstancesFactor =
            Mathf.Min(migrationFactor, skillLevelFactor, knowledgeLevelFactor);

        float populationFactor = 0.0001f + Mathf.Abs(OptimalPopulation - Population);
        populationFactor = OptimalPopulation / populationFactor;

        populationFactor = Mathf.Min(populationFactor, MaxUpdateSpanFactor);

        float prominenceFactor = 1;

        if (TotalPolityProminenceValue < 1)
        {
            // if there are unorganized bands, then set the prom factor accordingly
            prominenceFactor = 1 - TotalPolityProminenceValue;
        }

        float minPromFactor = 0.00001f;

        foreach (var prominence in _polityProminences.Values)
        {
            if (prominenceFactor < minPromFactor)
                break;

            if (prominence.Value < prominenceFactor)
            {
                prominenceFactor = prominence.Value;
            }
        }
        
        // avoid setting a prom factor equal or less than zero
        prominenceFactor = Mathf.Max(minPromFactor, prominenceFactor);

        float SlownessConstant = 100 * GenerationSpan;

        float mixFactor = 
            SlownessConstant * randomFactor * circumstancesFactor * populationFactor * prominenceFactor;
        long updateSpan = QuarterGenSpan + (long)Mathf.Ceil(mixFactor);

        if (updateSpan < 0)
            updateSpan = MaxUpdateSpan;

        updateSpan = (updateSpan < QuarterGenSpan) ? QuarterGenSpan : updateSpan;
        updateSpan = (updateSpan > MaxUpdateSpan) ? MaxUpdateSpan : updateSpan;

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Id == Manager.TracingData.GroupId)
            {
                string groupId = "Id: " + this + " Pos: " + Position;

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "CalculateNextUpdateDate - Group: " + groupId,
                    "CurrentDate: " + World.CurrentDate +
                    ", OptimalPopulation: " + OptimalPopulation +
                    ", ExactPopulation: " + ExactPopulation +
                    ", randomFactor: " + randomFactor +
                    ", migrationFactor: " + migrationFactor +
                    ", skillLevelFactor: " + skillLevelFactor +
                    ", knowledgeLevelFactor: " + knowledgeLevelFactor +
                    ", populationFactor: " + populationFactor +
                    ", LastUpdateDate: " + LastUpdateDate +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        long nextDate = World.CurrentDate + updateSpan;

        if (nextDate <= World.CurrentDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.CalculateNextUpDateDate - nextDate (" + nextDate +
                ") less or equal to World.CurrentDate (" + World.CurrentDate +
                "). updateSpan: " + updateSpan + ", randomFactor: " + randomFactor +
                ", migrationFactor: " + migrationFactor +
                ", skillLevelFactor: " + skillLevelFactor +
                ", knowledgeLevelFactor: " + knowledgeLevelFactor +
                ", populationFactor: " + populationFactor);

            nextDate = int.MinValue;
        }
        else if (nextDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.CalculateNextUpDateDate - nextDate (" + nextDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). updateSpan: " + updateSpan + ", randomFactor: " + randomFactor +
                ", migrationFactor: " + migrationFactor +
                ", skillLevelFactor: " + skillLevelFactor +
                ", knowledgeLevelFactor: " + knowledgeLevelFactor +
                ", populationFactor: " + populationFactor);

            nextDate = int.MinValue;
        }

        return nextDate;
    }

    public float PopulationAfterTime(long time) // in years
    {
        float population = ExactPopulation;

        if (population == OptimalPopulation)
            return population;

        float timeFactor = NaturalGrowthRate * time / (float)GenerationSpan;

        if (population < OptimalPopulation)
        {
            float geometricTimeFactor = Mathf.Pow(2, timeFactor);
            float populationFactor = 1 - ExactPopulation / (float)OptimalPopulation;

            population =
                OptimalPopulation * MathUtility.RoundToSixDecimals(1 - Mathf.Pow(populationFactor, geometricTimeFactor));

#if DEBUG
            if ((int)population < -1000)
            {
                Debug.Break();
                throw new System.Exception("Debug.Break");
            }
#endif

            //			#if DEBUG
            //			if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
            //				if (Id == Manager.TracingData.GroupId) {
            //					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
            //
            //					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //						"PopulationAfterTime:increase - Group:" + groupId,
            //						"CurrentDate: " + World.CurrentDate +
            //						", OptimalPopulation: " + OptimalPopulation +
            //						", ExactPopulation: " + ExactPopulation +
            //						", new population: " + population +
            //						"");
            //
            //					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
            //				}
            //			}
            //			#endif

            return population;
        }

        if (population > OptimalPopulation)
        {
            float decayFactor = -5 * timeFactor;
            population =
                OptimalPopulation +
                (ExactPopulation - OptimalPopulation) * MathUtility.RoundToSixDecimals(Mathf.Exp(decayFactor));

#if DEBUG
            if ((int)population < -1000)
            {
                Debug.Break();
                throw new System.Exception("Debug.Break");
            }
#endif

            //			#if DEBUG
            //			if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
            //				if (Id == Manager.TracingData.GroupId) {
            //					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
            //
            //					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //						"PopulationAfterTime:decrease - Group:" + groupId,
            //						"CurrentDate: " + World.CurrentDate +
            //						", OptimalPopulation: " + OptimalPopulation +
            //						", ExactPopulation: " + ExactPopulation +
            //						", new population: " + population +
            //						"");
            //
            //					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
            //				}
            //			}
            //			#endif

            return population;
        }

        return 0;
    }

    public IEnumerable<PolityProminence> GetPolityProminencesNotBeingRemoved()
    {
        foreach (var prominence in _polityProminences.Values)
        {
            if (_polityProminencesToRemove.Contains(prominence.PolityId))
            {
                continue;
            }

            yield return prominence;
        }
    }

    public ICollection<PolityProminence> GetPolityProminences()
    {
        return _polityProminences.Values;
    }

    public Faction GetClosestFaction(Polity polity)
    {
        return GetPolityProminence(polity)?.ClosestFaction;
    }

    public Faction GetMostProminentClosestFaction()
    {
        PolityProminence mostProminence = null;

        foreach (var prominence in _polityProminences.Values)
        {
            if (prominence.ClosestFaction == null)
                continue;

            if (prominence.Value > (mostProminence?.Value ?? 0))
            {
                mostProminence = prominence;
            }
        }

        return mostProminence?.ClosestFaction;
    }

    /// <summary>
    /// Try obtain the prominence associated with a polity Id
    /// </summary>
    /// <param name="polityId">the polity Id to search for</param>
    /// <param name="polityProminence">the polity prominence to return</param>
    /// <returns>'true' iff the polity porminence exists</returns>
    public bool TryGetPolityProminence(Identifier polityId, out PolityProminence polityProminence)
    {
        return _polityProminences.TryGetValue(polityId, out polityProminence);
    }

    /// <summary>
    /// Obtain the prominence associated with a polity Id
    /// </summary>
    /// <param name="polityId">the polity Id to search for</param>
    /// <returns>the polity prominence</returns>
    public PolityProminence GetPolityProminence(Identifier polityId)
    {
        if (!_polityProminences.TryGetValue(polityId, out PolityProminence polityProminence))
            return null;

        return polityProminence;
    }

    /// <summary>
    /// Try obtain the prominence associated with a polity
    /// </summary>
    /// <param name="polity">the polity to search for</param>
    /// <param name="polityProminence">the polity prominence to return</param>
    /// <returns>'true' iff the polity porminence exists</returns>
    public bool TryGetPolityProminence(Polity polity, out PolityProminence polityProminence)
    {
        return _polityProminences.TryGetValue(polity.Id, out polityProminence);
    }

    /// <summary>
    /// Obtain the prominence associated with a polity
    /// </summary>
    /// <param name="polityId">the polity to search for</param>
    /// <returns>the polity prominence</returns>
    public PolityProminence GetPolityProminence(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
            return null;

        return polityProminence;
    }

    public float GetPolityProminenceValue(Identifier polityId)
    {
        // return the prominence of unorganized bands
        if (polityId is null)
            return 1f - TotalPolityProminenceValue;

        if (!_polityProminences.TryGetValue(polityId, out PolityProminence polityProminence))
            return 0;

        return polityProminence.Value;
    }

    public float GetPolityProminenceValue(Polity polity)
    {
        return GetPolityProminenceValue(polity?.Id);
    }

    public float GetFactionCoreDistance(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            return float.MaxValue;
        }

        return polityProminence.FactionCoreDistance;
    }

    public float GetCoreDistance(Polity polity) => GetPolityCoreDistance(polity);

    public float GetCoreDistance(Faction faction)
    {
        var polity = faction.Polity;

        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            return float.MaxValue;
        }

        if (polityProminence.ClosestFaction != faction)
        {
            return float.MaxValue;
        }

        return polityProminence.FactionCoreDistance;
    }

    public Faction GetFaction(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            return null;
        }

        return polityProminence.ClosestFaction;
    }

    public float GetPolityCoreDistance(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            return float.MaxValue;
        }

        return polityProminence.PolityCoreDistance;
    }

    public void ResetCoreDistances(Identifier polityId, bool addToRecalcs = false)
    {
        var prom = GetPolityProminence(polityId);

        if (prom == null)
        {
            throw new System.Exception(
                $"Unable to find prominence with Id {polityId} in group {Id}");
        }

        prom.ResetCoreDistances(addToRecalcs: addToRecalcs);
    }

#if DEBUG
    /// <summary>
    /// Updates polity prominences and values (for unit tests only)
    /// TODO: try get rid of this function without making UpdatePolityProminences public
    /// </summary>
    public void UpdatePolityProminences_test()
    {
        PostUpdatePolityProminences();
    }

    /// <summary>
    /// Sets a prominence polity and faction core distances (for unit tests only)
    /// TODO: try get rid of this function
    /// </summary>
    public void SetProminenceCoreDistances_test(
        Polity polity, float polityCoreDistance, float factionCoreDistance)
    {
        PolityProminence prominence = GetPolityProminence(polity);

        prominence.PolityCoreDistance = polityCoreDistance;
        prominence.FactionCoreDistance = factionCoreDistance;
    }
#endif

    /// <summary>
    /// Post updates polity prominences and values
    /// </summary>
    /// <param name="afterPolityUpdates">
    /// Set to true if this function is being called after polity updates have been done</param>
    private void PostUpdatePolityProminences(bool afterPolityUpdates = false)
    {
        // Remove prominences that were forcibly declared to be removed
        RemovePolityProminences(!afterPolityUpdates);

#if DEBUG
        if (!afterPolityUpdates && !_hasPromValueDeltas)
        {
            if ((_polityProminences.Count > 0) && (HighestPolityProminence == null))
            {
                throw new System.Exception("Invalid state. Group: " + Id);
            }
        }

        if (_hasPromValueDeltas && (_polityPromDeltas.Count == 0))
        {
            throw new System.Exception("Invalid state. Group: " + Id);
        }
#endif

        if (CalculateNewPolityProminenceValues(afterPolityUpdates) || _hasRemovedProminences)
        {
            // Only update if there was a change in values
            CalculateProminenceValueTotals();

            _hasRemovedProminences = false;
        }

        foreach (var pair in FactionCores)
        {
            Faction faction = pair.Value;

            float promValue = GetPolityProminenceValue(faction.PolityId);
            {
                faction.GenerateCoreGroupProminenceValueBelowEvents(promValue);
            }
        }
    }

    /// <summary>
    /// Tries to generate and apply all events related to knowledges going below a minimun value
    /// </summary>
    public void GenerateKnowledgeLevelFallsBelowEvents(string knowledgeId, float scaledValue)
    {
        if (!OnKnowledgeLevelFallsBelowEventGenerators.ContainsKey(knowledgeId))
        {
            return;
        }

        foreach (var generator in OnKnowledgeLevelFallsBelowEventGenerators[knowledgeId])
        {
            if ((generator is CellGroupEventGenerator gGenerator) &&
                gGenerator.TestOnKnowledgeLevelFallsBelow(knowledgeId, this, scaledValue))
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    /// <summary>
    /// Tries to generate and apply all events related to knowledges going above a maximum value
    /// </summary>
    public void GenerateKnowledgeLevelRaisesAboveEvents(string knowledgeId, float scaledValue)
    {
        if (!OnKnowledgeLevelRaisesAboveEventGenerators.ContainsKey(knowledgeId))
        {
            return;
        }

        foreach (var generator in OnKnowledgeLevelRaisesAboveEventGenerators[knowledgeId])
        {
            if ((generator is CellGroupEventGenerator gGenerator) &&
                gGenerator.TestOnKnowledgeLevelRaisesAbove(knowledgeId, this, scaledValue))
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    /// <summary>
    /// Tries to generate and apply all events related to gaining a discovery
    /// </summary>
    public void GenerateGainedDiscoveryEvents(string discoveryId)
    {
        if (!OnGainedDiscoveryEventGenerators.ContainsKey(discoveryId))
        {
            return;
        }

        foreach (var generator in OnGainedDiscoveryEventGenerators[discoveryId])
        {
            if (generator is CellGroupEventGenerator gGenerator)
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    /// <summary>
    /// Finalizes polity prominence updates
    /// </summary>
    private void PostUpdateProminences()
    {
        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            prominence.PostUpdate();
        }
    }

    /// <summary>
    /// Finalizes polity prominence updates
    /// </summary>
    private void CalculateProminenceValueTotals()
    {
        TotalPolityProminenceValue = 0;

        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            if (float.IsNaN(prominence.Value))
            {
                throw new System.Exception(
                    $"Prominence value is Nan. Group: {Id}, Polity: {prominence.PolityId}");
            }

            TotalPolityProminenceValue += prominence.Value;
        }

#if DEBUG
        if ((_polityProminences.Count > 0) && (TotalPolityProminenceValue <= 0))
        {
            throw new System.Exception($"Invalid state. Group: {Id}");
        }
#endif

        if (TotalPolityProminenceValue > 1.0)
        {
            Debug.LogWarning(
                $"Total Polity Prominence Value greater than 1: {TotalPolityProminenceValue}, Group: {Id}");
        }

        if (TotalPolityProminenceValue <= 0)
        {
            foreach (Faction faction in GetFactionCores())
            {
                if (!faction.BeingRemoved)
                {
                    throw new System.Exception(
                        $"Group with no polity prominence has cores for factions " +
                        $"not being removed. Group: {Id}, Faction: {faction.Id}");
                }
            }
        }

        FindHighestPolityProminence();
    }

    /// <summary>
    /// Returns the current prominence value of the unorganized bands in the group
    /// </summary>
    /// <returns>the unorganized bands prominence value</returns>
    public float GetUBandsProminenceValue()
    {
        return 1f - TotalPolityProminenceValue;
    }

    /// <summary>
    /// Adds a delta to apply to the unorganized bands' prominence value in this group
    /// </summary>
    /// <param name="delta">value delta to apply</param>
    public void AddUBandsProminenceValueDelta(float delta)
    {
        _unorgBandsPromDelta += delta;
    }

    /// <summary>
    /// Adds a delta to apply to a polity's prominence value in this group
    /// </summary>
    /// <param name="polity">polity to apply prominence value delta</param>
    /// <param name="delta">value delta to apply</param>
    /// <param name="ignoreRemoval">if 'true', apply delta even if prominence
    /// was going to be removed</param>
    public void AddPolityProminenceValueDelta(
        Polity polity, float delta, bool ignoreRemoval = false)
    {
        if (delta == 0)
        {
            return;
        }

        if (_polityProminencesToRemove.Contains(polity.Id))
        {
            if (delta > 0)
            {
                if (ignoreRemoval)
                {
                    // prevent removal as long as delta is positive
                    _polityProminencesToRemove.Remove(polity.Id);
                }
                else
                {
                    // This is a situation that shouldn't happen but is not critical...
                    Debug.LogWarning($"Trying to add value delta to prominence that will be removed...");
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (float.IsNaN(delta) || float.IsInfinity(delta))
        {
            throw new System.Exception("prominence delta is Nan or Infinity. Group: " + Id);
        }

        if (_polityPromDeltas.ContainsKey(polity))
        {
            _polityPromDeltas[polity] += delta;
        }
        else
        {
            _polityPromDeltas.Add(polity, delta);
        }

        _hasPromValueDeltas = true;
    }

    /// <summary>
    /// Clean up all the unorganized bands and prominence value deltas
    /// </summary>
    private void ResetProminenceValueDeltas()
    {
        // reset delta for unorganized bands
        _unorgBandsPromDelta = 0;

        // reset all prominence deltas
        _polityPromDeltas.Clear();

        _hasPromValueDeltas = false;
    }

    private void AddProminenceValuesToDeltas()
    {
        // first the set the unorganized bands prominence delta
        AddUBandsProminenceValueDelta(1f - TotalPolityProminenceValue);

        // now do the same for every polity prominence delta
        foreach (PolityProminence p in _polityProminences.Values)
        {
            if (_polityPromDeltas.ContainsKey(p.Polity))
            {
                _polityPromDeltas[p.Polity] += p.Value;
            }
            else
            {
                _polityPromDeltas.Add(p.Polity, p.Value);
            }
        }
    }

    private float CalculateProminenceDeltaOffset()
    {
        // -----
        // NOTE: This is not a proper solution. A better one would require for every
        // prominence value transfer between polities to be recorded as a transaction,
        // and balancing out those transactions that push a prominence value below zero
        // independently from all others
        // -----
        float offset = Mathf.Min(0, _unorgBandsPromDelta);
        foreach (float delta in _polityPromDeltas.Values)
        {
            offset = Mathf.Min(offset, delta);
        }

        if (float.IsNaN(offset))
        {
            throw new System.Exception("prominence delta offset is Nan. Group: " + Id);
        }

        return offset;
    }

    public PolityProminence IncreasePolityProminenceValue(
        Polity polity,
        float toAdd)
    {
        float value = GetPolityProminenceValue(polity);

        return SetPolityProminenceValue(polity, value + toAdd);
    }

    public PolityProminence SetPolityProminenceValue(
        Polity polity,
        float newValue,
        bool afterPolityUpdates = false,
        bool throwIfCantRemove = true)
    {
        float promPopulation = ExactPopulation * newValue;

        if (promPopulation < MinProminencePopulation)
        {
            // try to remove prominences that would end up with a value far too small
            // NOTE: Can't do that after polities have been updated
            if (afterPolityUpdates || !SetPolityProminenceToRemove(polity, false, false))
            {
                if (throwIfCantRemove)
                {
                    throw new System.Exception(
                        $"Unable to remove prominence {polity.Id} from group {Id}");
                }

                // if not possible to remove this prominence, set it to a min value
                newValue = MinProminencePopulation / ExactPopulation;
            }
            else
            {
                return null;
            }
        }

        if (!_polityProminences.ContainsKey(polity.Id))
        {
            if (afterPolityUpdates)
            {
                Debug.LogWarning("Trying to add polity " + polity.Id +
                    " after polity updates have already happened.  Group: " + Id);
            }

            // add missing prominences that have values greater than MinPolityProminenceValue
            AddPolityProminence(polity);
        }

        var prominence = _polityProminences[polity.Id];

        prominence.Value = newValue;

        return prominence;
    }

    private float UpdateProminenceValuesWithDeltas(
        float promDeltaOffset,
        bool afterPolityUpdates)
    {
        float totalValue = 0;
        float ubProminenceValue = _unorgBandsPromDelta - promDeltaOffset;

        foreach (KeyValuePair<Polity, float> pair in _polityPromDeltas)
        {
            Polity polity = pair.Key;
            float newValue = pair.Value;

            newValue -= promDeltaOffset;

            if (SetPolityProminenceValue(polity, newValue, afterPolityUpdates, false) != null)
            {
                totalValue += newValue;
            }
            else
            {
                // We will "transfer" its prominence value to unorganized bands
                // instead
                ubProminenceValue += newValue;
            }
        }

        // add in the prominence value of unorganized bands
        // if there's enough population to sustain it
        float upPopulation = ExactPopulation * ubProminenceValue;
        if (upPopulation >= MinProminencePopulation)
        {
            totalValue += ubProminenceValue;
        }

        return totalValue;
    }

    private void NormalizeProminenceValues(float totalValue)
    {
        foreach (PolityProminence prom in _polityProminences.Values)
        {
            if (_polityProminencesToRemove.Contains(prom.PolityId))
                continue;

            if (totalValue <= 0)
            {
                throw new System.Exception("Unexpected total prominence value of: " + totalValue +
                    ", group: " + Id + ", date: " + World.CurrentDate);
            }

            float prevValue = prom.Value;
            float finalValue = prevValue / totalValue;

#if DEBUG
            if (!finalValue.IsInsideRange(0, 1))
            {
                Debug.LogWarning("prominence value outside of (0,1) range: " + finalValue +
                    ", prev value: " + prevValue + ", total value: " + totalValue +
                    ", group: " + Id + ", date: " + World.CurrentDate);
            }
#endif

            // round value to six decimals to avoid hidden bit serialization issues
            prom.Value = MathUtility.RoundToSixDecimals(finalValue);

            if (float.IsNaN(prom.Value))
            {
                throw new System.Exception("Prominence value is Nan. Group: " + Id +
                    ", Polity: " + prom.PolityId);
            }
        }
    }

    /// <summary>
    /// Update all prominence values using all the applied value deltas so far
    /// </summary>
    /// <param name="afterPolityUpdates">
    /// Set to true if this function is being called after polity updates have been done</param>
    /// <returns>'true' if there was a change in prominence values</returns>
    private bool CalculateNewPolityProminenceValues(bool afterPolityUpdates = false)
    {
        // NOTE: after polity updates there might be no deltas, bu we might still need
        // to recalculate if the amount of prominences changed
        bool calculateRegardless = afterPolityUpdates && _polityProminences.Count > 0;

        if (!calculateRegardless && !_hasPromValueDeltas)
        {
            // There was no new deltas so there's nothing to calculate
            ResetProminenceValueDeltas();
            return false;
        }

        // add current prominence values to deltas
        AddProminenceValuesToDeltas();

        // get the offset to apply to all deltas so that there are no negative values
        float promDeltaOffset = CalculateProminenceDeltaOffset();

        // replace prom values with deltas minus the offset, and get the total sum
        float totalValue = UpdateProminenceValuesWithDeltas(promDeltaOffset, afterPolityUpdates);

        // normalize values
        NormalizeProminenceValues(totalValue);

        // remove any prominences set to be removed after the value updates
        RemovePolityProminences();

        ResetProminenceValueDeltas();
        return true;
    }

    private bool ValidateProminenceRemoval(
        Polity polity,
        bool forcedCoreRemoval = true,
        bool ignoreFactionCores = false)
    {
        if (ignoreFactionCores)
        {
            return true;
        }

        // throw warning if this groups was set to become a faction core
        // even if the polity is about to be removed (even more so)
        if ((WillBecomeCoreOfFaction != null) &&
            (WillBecomeCoreOfFaction.PolityId == polity.Id))
        {
            Debug.LogWarning(
                $"Group is set to become a faction core - group: {Id}" +
                $" - faction: {WillBecomeCoreOfFaction.Id}" +
                $" - polity: {polity.Id} - Date: {World.CurrentDate}");

            return false;
        }

        if (!forcedCoreRemoval)
        {
            var prominence = GetPolityProminence(polity.Id);

            foreach (var pair in FactionCores)
            {
                Faction faction = pair.Value;

                if (faction.PolityId == prominence.PolityId)
                {
                    if (!faction.BeingRemoved)
                    {
                        Debug.LogWarning(
                            $"Group has valid faction core, group: {Id}" +
                            $", faction: {faction.Id}" +
                            $", polity: {polity.Id}" +
                            $", date: {World.CurrentDate}");
                    }

                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// add a polity prominence to remove
    /// </summary>
    /// <param name="polity">the polity to which the prominence belongs</param>
    /// <param name="throwIfNotPresent">throw if prominence is not present</param>
    /// <returns>'false' if the polity prominence can't be removed</returns>
    public bool SetPolityProminenceToRemove(
        Polity polity,
        bool throwIfNotPresent = true,
        bool forcedCoreRemoval = true,
        bool ignoreFactionCores = false)
    {
        if (!_polityProminences.ContainsKey(polity.Id))
        {
            if (throwIfNotPresent)
            {
                throw new System.ArgumentException(
                    $"Prominence of polity {polity.Id}" +
                    $" not present in {Id}");
            }

            return true;
        }

        if (_polityProminencesToRemove.Contains(polity.Id))
        {
            return true;
        }

        if (!ValidateProminenceRemoval(polity, forcedCoreRemoval, ignoreFactionCores))
        {
            return false;
        }

        _polityProminencesToRemove.Add(polity.Id);
        return true;
    }

    public void OnPolityCountChange() 
    {
        foreach (var generator in OnPolityCountChangeEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"CellGroup.OnPolityCountChangeEventGenerators: adding '{context.Id}' to list of events to try to assign");
            }

            if (generator is CellGroupEventGenerator gGenerator)
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    public void OnCoreCountChange()
    {
        foreach (var generator in OnCoreCountChangeEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"CellGroup.OnCoreCountChange: adding '{context.Id}' to list of events to try to assign");
            }

            if (generator is CellGroupEventGenerator gGenerator)
            {
                AddGeneratorToTestAssignmentFor(gGenerator);
            }
        }
    }

    /// <summary>
    /// Add a new polity prominence
    /// </summary>
    /// <param name="polity">polity to associate the new prominence with</param>
    /// <param name="initialValue">starting prominence value</param>
    /// <param name="modifyTotalValue">'true' if the total prominence value has to
    /// be updated</param>
    public void AddPolityProminence(
        Polity polity,
        float initialValue = 0,
        bool modifyTotalValue = false)
    {
        var prominence = new PolityProminence(this, polity, initialValue);

        if ((HighestPolityProminence != null) &&
            (polity.Id == HighestPolityProminence.PolityId))
        {
            throw new System.Exception(
                $"Trying to add a prominence already set as highest polity prominence. " +
                $"Group id: {Id}, polity id: {polity.Id}");
        }

        _polityProminences.Add(polity.Id, prominence);
        Cell.AddGroupPolityProminence(prominence);

        // We want to update the polity if a group is added.
        SetPolityUpdate(prominence, true);

        polity.AddGroup(prominence);

        if (modifyTotalValue)
        {
            TotalPolityProminenceValue += initialValue;
        }

        World.AddPromToSetCoreDistFor(prominence);
        World.AddGroupWithPolityCountChange(this);
    }

    public void RemovePolityProminence(PolityProminence polityProminence, bool removeFactionCores = true)
    {
        ValidateProminenceRemoval(polityProminence.Polity, ignoreFactionCores: !removeFactionCores);

        polityProminence.InitDestruction(removeFactionCores);

        FinishRemovingPolityProminence(polityProminence, removeFactionCores: removeFactionCores);

        if (HighestPolityProminence == polityProminence)
        {
            FindHighestPolityProminence();
        }
    }

    private void FinishRemovingPolityProminence(
        PolityProminence polityProminence, 
        bool updatePolity = true, 
        bool removeFromSetToRemove = true,
        bool removeFactionCores = true)
    {
        if (removeFactionCores)
        {
            // Remove any faction that belongs to the same polity and has it's core in this group
            foreach (Faction faction in GetFactionCores())
            {
                if (faction.PolityId == polityProminence.PolityId)
                {
                    Debug.LogWarning(
                        $"Removing polity prominence of faction that had core in group {Id}" +
                        $", removing faction {faction.Id}" +
                        $" - polity: {polityProminence.PolityId} - Date: { World.CurrentDate}");

                    faction.SetToRemove();
                }
            }
        }

        _polityProminences.Remove(polityProminence.PolityId);
        Cell.SetGroupPolityProminenceListToReset();

        polityProminence.Polity.RemoveGroup(polityProminence);

        if (updatePolity)
        {
            // We want to update the polity if a group is removed.
            SetPolityUpdate(polityProminence, true);
        }

        polityProminence.FinishDestruction();

        if (removeFromSetToRemove)
        {
            _polityProminencesToRemove.Remove(polityProminence.PolityId);
        }
    }

    /// <summary>
    /// Remove all polities that where set to be removed
    /// </summary>
    /// <param name="updatePolity">
    /// Set to false if there's no need to update the removed polities after calling this</param>
    private void RemovePolityProminences(bool updatePolity = true)
    {
        bool removedHighestPolityProminence = false;

        // destroy the prominence object before removing it
        foreach (Identifier polityId in _polityProminencesToRemove)
        {
            _polityProminences[polityId].InitDestruction();
        }

        foreach (Identifier polityId in _polityProminencesToRemove)
        {
            var polityProminence = _polityProminences[polityId];

            FinishRemovingPolityProminence(polityProminence, updatePolity, false);

            if (HighestPolityProminence == polityProminence)
            {
                removedHighestPolityProminence = true;
            }

            _hasRemovedProminences = true;
        }

        if (_hasRemovedProminences)
        {
            World.AddGroupWithPolityCountChange(this);
        }

        _polityProminencesToRemove.Clear();

        Cell.TryResetGroupPolityProminenceList();

        // CAUTION: We should make sure we find the new highest polity prominence
        // afterwards. We don't do it right away because normally we would add prominences
        // after calling this function and then we do a find highest.
        if (removedHighestPolityProminence)
        {
            SetHighestPolityProminence(null);
        }
    }

    /// <summary>
    /// Compares all polity prominences and sets the one with the highest value
    /// </summary>
    public void FindHighestPolityProminence()
    {
        float highestProminenceValue = float.MinValue;
        PolityProminence highestProminence = null;

        foreach (PolityProminence pi in _polityProminences.Values)
        {
            if (pi.Value > highestProminenceValue)
            {
                highestProminenceValue = pi.Value;
                highestProminence = pi;
            }
        }

        if ((_polityProminences.Count > 0) && (highestProminence == null))
        {
            throw new System.Exception("Highest prominence value not found event though " +
                "there is at least one prominence in Group: " + Id);
        }

        SetHighestPolityProminence(highestProminence);
    }

    public void SetToUpdate(bool warnIfUnexpected = true)
    {
        World.AddGroupToUpdate(this, warnIfUnexpected);
    }

    public void AddProperty(string property)
    {
        _properties.Add(property);
    }

    public bool HasOrWillProperty(string property)
    {
        if (_properties.Contains(property))
        {
            return true;
        }

        return _propertiesToAquire.Contains(property);
    }

    public bool HasProperty(string property)
    {
        return _properties.Contains(property);
    }

    public ICollection<string> GetProperties()
    {
        return _properties;
    }

    public void ApplyArabilityModifier(int delta)
    {
        int value = ArabilityModifier + delta;

        if (value < 0)
        {
            Debug.LogWarning("Can't set an arability modifier lower than 0: " + value);
        }

        ArabilityModifier = value;
    }

    public void ApplyAccessibilityModifier(int delta)
    {
        int value = AccessibilityModifier + delta;

        if (value < 0)
        {
            Debug.LogWarning("Can't set an accessibility modifier lower than 0: " + value);
        }

        AccessibilityModifier = value;
    }

    public void ApplyNavigationRangeModifier(int delta)
    {
        int value = NavigationRangeModifier + delta;

        if (value < 0)
        {
            Debug.LogWarning("Can't set an navigation range modifier lower than 0: " + value);
        }

        NavigationRangeModifier = Mathf.Max(value, 0);
    }

    public void RemoveProperty(string property)
    {
        _properties.Remove(property);
    }

    public void AddPropertyToAquire(string property)
    {
        _propertiesToAquire.Add(property);
    }

    public void AddPropertyToLose(string property)
    {
        _propertiesToLose.Add(property);
    }

    public Polity GetRandomPolity(int rngOffset, PolityType type)
    {
        if (_polityProminences.Count < 1)
            return null;

        int selectionValue = GetNextLocalRandomInt(rngOffset, _polityProminences.Count);

        int count = 0;
        foreach (var prominence in _polityProminences.Values)
        {
            if ((type != PolityType.Any) && (prominence.Polity.Type != type))
                continue;

            if (_polityProminencesToRemove.Contains(prominence.PolityId))
                continue;

            if (count == selectionValue)
                return prominence.Polity;

            count++;
        }

        return null;
    }

    public void Synchronize()
    {
        PolityProminences = new List<PolityProminence>(_polityProminences.Values);

        // Reload prominences to reorder them as they would appear in the save file
        _polityProminences.Clear();
        LoadPolityProminences();

        Flags = new List<string>(_flags);
        Properties = new List<string>(_properties);

        Culture.Synchronize();

        if (SeaMigrationRoute != null)
        {
            if (!SeaMigrationRoute.Consolidated)
            {
                SeaMigrationRoute = null;
            }
            else
            {
                SeaMigrationRoute.Synchronize();
            }
        }

        FactionCoreIds = new List<Identifier>(FactionCores.Keys);
    }

#if DEBUG
    public static int Debug_LoadedGroups = 0;
#endif

    private void LoadPolityProminences()
    {
        foreach (PolityProminence p in PolityProminences)
        {
            if (Cell == null)
            {
                throw new System.Exception("Cell can't be null");
            }

            if (p.ClosestFactionId == null)
            {
                throw new System.Exception(
                    $"Missing ClosestFactionId for prominence {Id} of Polity {p.PolityId}");
            }

            _polityProminences.Add(p.PolityId, p);
            Cell.AddGroupPolityProminence(p);
        }
    }

    public void PrefinalizeLoad()
    {
        Cell = World.GetCell(Longitude, Latitude);

        if (Cell == null)
        {
            throw new System.Exception($"Cell [{Longitude},{Latitude}] is null");
        }

        Cell.Group = this;

        LoadPolityProminences();

        Neighbors = new Dictionary<Direction, CellGroup>(8);
    }

    public void FinalizeLoad()
    {
        if (LastPopulationMigration != null)
        {
            LastPopulationMigration.World = World;
            LastPopulationMigration.FinalizeLoad();
        }

        if (MigrationTagged)
        {
            World.MigrationTagGroup(this);
        }

        foreach (Identifier id in FactionCoreIds)
        {
            Faction faction = World.GetFaction(id);

            if (faction == null)
            {
                throw new System.Exception("Missing faction with id: " + id);
            }

            FactionCores.Add(id, faction);
        }

        foreach (string f in Flags)
        {
            _flags.Add(f);
        }

        foreach (string property in Properties)
        {
            _properties.Add(property);
        }

        InitializeNeighbors(false);

        World.UpdateMostPopulousGroup(this);

        Culture.World = World;
        Culture.Group = this;
        Culture.FinalizeLoad();

        if (SeaMigrationRoute != null)
        {
            SeaMigrationRoute.World = World;

            if (SeaMigrationRoute.Consolidated)
            {
                SeaMigrationRoute.FinalizeLoad();
            }
            else
            {
                SeaMigrationRoute.FirstCell = Cell;
            }
        }

        foreach (PolityProminence p in _polityProminences.Values)
        {
            if (p.ClosestFaction == null)
            {
                throw new System.Exception("Missing closest faction with id: " + p.ClosestFactionId);
            }

            if (p.Polity == null)
            {
                throw new System.Exception("Missing polity with id:" + p.PolityId);
            }

            if ((HighestPolityProminence == null) || (HighestPolityProminence.Value < p.Value))
            {
                HighestPolityProminence = p;
            }

            p.SetAllNeighborProminences(false);
        }

        // Generate Update Event

        UpdateEvent = new UpdateCellGroupEvent(this, NextUpdateDate, originalSpawnDate: UpdateEventSpawnDate);
        World.InsertEventToHappen(UpdateEvent);

        // Generate Migration Event

        if (HasMigrationEvent)
        {
            TerrainCell targetCell =
                World.GetCell(MigrationTargetLongitude, MigrationTargetLatitude);

            PopulationMigrationEvent = new MigratePopulationEvent(
                this,
                targetCell,
                (Direction)MigrationEventDirectionInt,
                (MigrationType)MigrationEventTypeInt,
                MigrationProminencePercent,
                MigratingPopPolId,
                MigrationEventDate);

            World.InsertEventToHappen(PopulationMigrationEvent);
        }

        // Generate Tribe Formation Event

        if (HasTribeFormationEvent)
        {
            TribeFormationEvent = new TribeFormationEvent(this, TribeFormationEventDate);
            World.InsertEventToHappen(TribeFormationEvent);
        }

#if DEBUG
        Debug_LoadedGroups++;
#endif
    }
}

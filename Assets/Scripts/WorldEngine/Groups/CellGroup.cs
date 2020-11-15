using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CellGroup : Identifiable, IFlagHolder
{
    [XmlIgnore]
    public World World;

    public const long GenerationSpan = 25 * World.YearLength;

    public const long MaxUpdateSpan = GenerationSpan * 8000;

    public const float MaxUpdateSpanFactor = MaxUpdateSpan / GenerationSpan;

    public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
    public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
    public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

    public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;

    public const float MinKnowledgeTransferValue = 0.25f;

    public const float SeaTravelBaseFactor = 25f;

    public const float MaxCoreDistance = 1000000000000f;

    public static float TravelWidthFactor;

    public static List<ICellGroupEventGenerator> OnSpawnEventGenerators;

    public static List<IWorldEventGenerator> OnCoreHighestProminenceChangeEventGenerators;

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

    public Identifier MigratingPopPolId = null;

    public Route SeaMigrationRoute = null;

    public List<string> Flags;

    public CellCulture Culture;

    public List<string> Properties;

    public List<Identifier> FactionCoreIds;

    public List<PolityProminence> PolityProminences = null;

    [XmlIgnore]
    public WorldPosition Position
    {
        get
        {
            return Cell.Position;
        }
    }

    [XmlIgnore]
    public float ScaledArabilityModifier
    {
        get
        {
            return Mathf.Clamp01(ArabilityModifier * MathUtility.IntToFloatScalingFactor);
        }
    }

    [XmlIgnore]
    public float ScaledAccessibilityModifier
    {
        get
        {
            return Mathf.Clamp01(AccessibilityModifier * MathUtility.IntToFloatScalingFactor);
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
    public Dictionary<string, BiomeSurvivalSkill> _biomeSurvivalSkills = new Dictionary<string, BiomeSurvivalSkill>();

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

    private Dictionary<Identifier, PolityProminence> _polityProminences =
        new Dictionary<Identifier, PolityProminence>();

    private CellUpdateType _cellUpdateType = CellUpdateType.None;
    private CellUpdateSubType _cellUpdateSubtype = CellUpdateSubType.None;

    private HashSet<Identifier> _polityProminencesToRemove =
        new HashSet<Identifier>();
    //private HashSet<Polity> _polityProminencesToAdd =
    //    new HashSet<Polity>();

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

    public int PreviousPopulation
    {
        get
        {
            return (int)PreviousExactPopulation;
        }
    }

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
            bands.Culture,
            bands.MigrationDirection)
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
            polityPop.Culture,
            polityPop.MigrationDirection)
    {
        AddPolityProminence(polityPop.Polity, 1.0f, true);
    }

    public CellGroup(
        World world,
        TerrainCell cell,
        int initialPopulation,
        Culture baseCulture = null,
        Direction migrationDirection = Direction.Null)
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

#if DEBUG
        if (Longitude > 1000)
        {
            Debug.LogError("Longitude[" + Longitude + "] > 1000");
        }

        if (Latitude > 1000)
        {
            Debug.LogError("Latitude[" + Latitude + "] > 1000");
        }
#endif

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

        foreach (KeyValuePair<Direction, TerrainCell> pair in Cell.Neighbors)
        {
            if (pair.Value.Group != null)
            {
                CellGroup group = pair.Value.Group;

                Neighbors.Add(pair.Key, group);

                Direction dir = TerrainCell.ReverseDirection(pair.Key);

                group.AddNeighbor(dir, this);
            }
        }

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

    [XmlIgnore]
    public IEnumerable<CellGroup> NeighborGroups // This method ensures neighbors are always accessed in the same order
    {
        get
        {
            CellGroup group = null;

            if (Neighbors.TryGetValue(Direction.North, out group))
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

    /// <summary>
    /// Defines population to migrate
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="polity">the polity whose population will migrate</param>
    public void SetMigratingPopulation(
        TerrainCell targetCell,
        Direction migrationDirection,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        Polity polity)
    {
        if (polity == null)
        {
            SetMigratingUnorganizedBands(
                targetCell, migrationDirection, prominencePercent, prominenceValueDelta, population);
            return;
        }

        SetMigratingPolityPopulation(
            targetCell, migrationDirection, prominencePercent, prominenceValueDelta, population, polity);
    }

    /// <summary>
    /// Sets Migrating Bands object
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    public void SetMigratingUnorganizedBands(
        TerrainCell targetCell,
        Direction migrationDirection,
        float prominencePercent,
        float prominenceValueDelta,
        int population)
    {
        if (!prominencePercent.IsInsideRange(0, 1))
        {
            Debug.LogWarning("Prominence percent outside of range [0,1]: " + prominencePercent);
            prominencePercent = Mathf.Clamp01(prominencePercent);
        }

        if (MigratingUnorganizedBands == null)
        {
            MigratingUnorganizedBands = new MigratingUnorganizedBands(
                World, prominencePercent, prominenceValueDelta, population, this, targetCell, migrationDirection);
        }
        else
        {
            MigratingUnorganizedBands.Set(
                prominencePercent, prominenceValueDelta, population, this, targetCell, migrationDirection);
        }

        World.AddMigratingPopulation(MigratingUnorganizedBands);
    }

    /// <summary>
    /// Sets Migrating Polity Population object
    /// </summary>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    /// <param name="prominencePercent">prominence value to migrate out</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="polity">the polity whose population will migrate</param>
    public void SetMigratingPolityPopulation(
        TerrainCell targetCell,
        Direction migrationDirection,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        Polity polity)
    {
        if (!prominencePercent.IsInsideRange(0, 1))
        {
            Debug.LogWarning("Prominence percent outside of range [0,1]: " + prominencePercent);
            prominencePercent = Mathf.Clamp01(prominencePercent);
        }

        if (MigratingPolityPopulation == null)
        {
            MigratingPolityPopulation = new MigratingPolityPopulation(
                World, prominencePercent, prominenceValueDelta, population, this, polity, targetCell, migrationDirection);
        }
        else
        {
            MigratingPolityPopulation.Set(
                prominencePercent, prominenceValueDelta, population, this, polity, targetCell, migrationDirection);
        }

        World.AddMigratingPopulation(MigratingPolityPopulation);
    }

    public void AddDeferredEffect(Effect effect)
    {
        _deferredEffects.Add(effect);
    }

    public static void ResetEventGenerators()
    {
        OnSpawnEventGenerators = new List<ICellGroupEventGenerator>();
        OnCoreHighestProminenceChangeEventGenerators = new List<IWorldEventGenerator>();
    }

    private void InitializeOnSpawnEvents()
    {
        foreach (ICellGroupEventGenerator generator in OnSpawnEventGenerators)
        {
            generator.TryGenerateEventAndAssign(this);
        }
    }

    /// <summary>
    /// Applies the effects of changing the highest prominence on a core group
    /// </summary>
    public void ApplyCoreHighestProminenceChange()
    {
        foreach (IWorldEventGenerator generator in OnCoreHighestProminenceChangeEventGenerators)
        {
            if (generator is IFactionEventGenerator fGenerator)
            {
                foreach (Faction faction in FactionCores.Values)
                {
                    fGenerator.TryGenerateEventAndAssign(faction);
                }
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
    }

    public void RemoveFactionCore(Faction faction)
    {
        if (FactionCores.ContainsKey(faction.Id))
        {
            FactionCores.Remove(faction.Id);
            Manager.AddUpdatedCell(Cell, CellUpdateType.Territory, CellUpdateSubType.Core);
        }
    }

    public bool FactionHasCoreHere(Faction faction)
    {
        return FactionCores.ContainsKey(faction.Id);
    }

    public ICollection<Faction> GetFactionCores()
    {
        return FactionCores.Values;
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
        if (prominence == null)
        {
            if (_polityProminences.Count > 0)
            {
                throw new System.Exception("Trying to set HighestPolityProminence to null when there are still polity prominences in group");
            }
        }

        if (HighestPolityProminence == prominence)
            return;

        if (HighestPolityProminence != null)
        {
            HighestPolityProminence.Polity.Territory.RemoveCell(Cell);
        }

        HighestPolityProminence = prominence;

        if (prominence != null)
        {
            prominence.Polity.Territory.AddCell(Cell);
        }

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
            Culture.TryAddKnowledgeToLearn(SocialOrganizationKnowledge.KnowledgeId, SocialOrganizationKnowledge.InitialValue);
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
            if (biome.Traits.Contains("sea"))
            {
                if (Culture.GetSkill(SeafaringSkill.SkillId) == null)
                {
                    Culture.AddSkillToLearn(new SeafaringSkill(this));
                }
            }

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

    public void MergePolityProminence(Polity polity, float value, float percentOfTarget)
    {
        AddPolityProminenceValueDelta(polity, value * percentOfTarget);
    }

    public void MergePolityProminences(
        Dictionary<Polity, float> sourcePolityProminences,
        float percentOfTarget = 1)
    {
        foreach (KeyValuePair<Polity, float> pair in sourcePolityProminences)
        {
            MergePolityProminence(pair.Key, pair.Value, percentOfTarget);
        }
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
//#if DEBUG
//        if ((World.CurrentDate >= Manager.GetDateNumber(2488878, 219)) &&
//            (Id == "0000000000908440689:5109863400567975564"))
//        {
//            Debug.LogWarning("PostUpdate_BeforePolityUpdates_Step1: Debugging group: " + Id);
//        }
//#endif

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

        UpdatePolityProminences();

        PostUpdatePolityCulturalProminences();

        CalculatePolityPromCoreDistances();
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

        SetPolityPromCoreDistancesAndAdminLoad();
    }

    public void PostUpdate_AfterPolityUpdates()
    {
        // These operations might have been done already for this group in
        // PostUpdate_BeforePolityUpdates_Step1. This is ok since we can't
        // be sure if a group might get affected by a polity update after
        // it has already been updated

        UpdatePolityProminences(true);

        PostUpdatePolityCulturalProminences();
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

            SeaMigrationRoute.Reset();
            SeaMigrationRoute.Build();
        }

        bool invalidRoute = false;

        if (SeaMigrationRoute.LastCell == null)
            invalidRoute = true;

        if (SeaMigrationRoute.LastCell == SeaMigrationRoute.FirstCell)
            invalidRoute = true;

        if (SeaMigrationRoute.MigrationDirection == Direction.Null)
            invalidRoute = true;

        if (SeaMigrationRoute.FirstCell.Neighbors.ContainsValue(SeaMigrationRoute.LastCell))
            invalidRoute = true;

        if (invalidRoute)
        {
            return;
        }

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

    private class PolityProminenceWeight : CollectionUtility.ElementWeightPair<PolityProminence>
    {
        public PolityProminenceWeight(PolityProminence polityProminence, float weight) : base(polityProminence, weight)
        {

        }
    }

    /// <summary>
    /// Calculates the current migration value of a cell that is a possible target for
    /// migration of unorganized bands.
    /// The value returned will be a value between 0 and 1.
    /// </summary>
    /// <param name="cell">the target cell</param>
    /// <returns>a migration value between 0 and 1</returns>
    public float CalculateUBMigrationValue(TerrainCell cell)
    {
        return cell.CalculateMigrationValue(this, Culture);
    }

    /// <summary>
    /// Returns the respective unorganized bands and prominences values on a group for
    /// a given preference
    /// </summary>
    /// <param name="preferenceId">the id of the preference</param>
    /// <param name="ubValue">(out) the unorganized bands value</param>
    /// <param name="promValue">the prominences value (average)</param>
    public void CalculateGroupPrefValueSplit(
        string preferenceId, out float ubValue, out float promValue)
    {
        if (TotalPolityProminenceValue >= 1f)
        {
            ubValue = 0;
            promValue = Culture.GetPreferenceValue(preferenceId);

            return;
        }

        if (TotalPolityProminenceValue <= 0f)
        {
            ubValue = Culture.GetPreferenceValue(preferenceId);
            promValue = 0;

            return;
        }

        float accPolPrefValue = 0;

        foreach (PolityProminence p in _polityProminences.Values)
        {
            accPolPrefValue +=
                p.Value * p.Polity.Culture.GetPreferenceValue(preferenceId);
        }

        float groupPrefValue = Culture.GetPreferenceValue(preferenceId);

        if (groupPrefValue > accPolPrefValue)
        {
            float maxPrefValueDelta = Mathf.Min(1 - groupPrefValue, groupPrefValue - accPolPrefValue);

            ubValue = groupPrefValue + (maxPrefValueDelta * TotalPolityProminenceValue);
            promValue = ubValue - maxPrefValueDelta;
        }
        else
        {
            float maxPrefValueDelta = Mathf.Min(groupPrefValue, accPolPrefValue - groupPrefValue);

            ubValue = groupPrefValue - (maxPrefValueDelta * TotalPolityProminenceValue);
            promValue = ubValue + maxPrefValueDelta;
        }
    }

    /// <summary>
    /// Estimates how encroached are unorganized bands on this cell
    /// </summary>
    /// <returns>the encroachment value on unorganized bands</returns>
    public float CalculateEncroachmentUnorganizedBands()
    {
        CalculateGroupPrefValueSplit(
            CulturalPreference.AggressionPreferenceId,
            out float ubAggrValue,
            out float prominenceAggrValue);

        return Mathf.Clamp(prominenceAggrValue - ubAggrValue, -1, 1);
    }

    public float EstimateUnorganizedBandsFreeSpace()
    {
        float encroachment = CalculateEncroachmentUnorganizedBands();

        if (encroachment > 0)
        {
            return Mathf.Max(0, (OptimalPopulation - Population) * (1 - encroachment));
        }
        else
        {
            return Mathf.Max(0, OptimalPopulation - Population * (1 + encroachment));
        }
    }

    /// <summary>
    /// Calculates the current value of a cell considered as a migration target
    /// The value returned will be a value between 0 and 1.
    /// </summary>
    /// <param name="cell">the target cell</param>
    /// <param name="migratingPolity">the polity that intend to migrate
    /// (null if migrating unorganized bands)</param>
    /// <returns>Migration value</returns>
    public float CalculateMigrationValue(TerrainCell cell, Polity migratingPolity = null)
    {
        // if no polity is given, then return the value calculated for the unorganized bands 
        if (migratingPolity == null)
        {
            return CalculateUBMigrationValue(cell);
        }

        return migratingPolity.CalculateMigrationValue(this, cell);
    }

    /// <summary>
    /// Calculates the chance of a successful migration to the target cell.
    /// </summary>
    /// <param name="cell">the target cell</param>
    /// <param name="migratingPolity">the polity that intend to migrate
    /// (null if migrating unorganized bands)</param>
    /// <returns>Migration chance as a value between 0 and 1</returns>
    public float CalculateMigrationChance(TerrainCell cell, Polity migratingPolity = null)
    {
        float offset = -0.1f;
        float migrationValue = CalculateMigrationValue(cell, migratingPolity);

        float unbiasedChance = Mathf.Clamp01(migrationValue + offset);

        // Bias the value toward 1
        float chance = 1 - Mathf.Pow(1 - unbiasedChance, 4);

        return chance;
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

        float cellChance = CalculateMigrationChance(targetCell, polity);

        if (cellChance <= 0)
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
            targetCell, migrationDirection, MigrationType.Land, cellChance, polity?.Id, arrivalDate);
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
            return;

        if (targetCell == null)
            return;

        float cellChance = CalculateMigrationChance(targetCell, polity);

        if (cellChance <= 0)
            return;

        targetCell.CalculateAdaptation(Culture, out _, out float cellSurvivability);

        if (cellSurvivability <= 0)
            return;

        float routeLength = SeaMigrationRoute.Length;
        float routeLengthFactor = Mathf.Pow(routeLength, 2);

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

        SeaMigrationRoute.Used = true;

        SetPopulationMigrationEvent(
            targetCell, migrationDirection, MigrationType.Sea, cellChance, polity?.Id, nextDate);
    }

    /// <summary>
    /// Resets of generates a new population migration event
    /// </summary>
    /// <param name="targetCell">cell which the group population will migrate toward</param>
    /// <param name="migrationDirection">direction toward which the migration will occur</param>
    /// <param name="migrationType">'Land' or 'Sea' migration</param>
    /// <param name="maxProminencePercent">limit to the prominence value to migrate out</param>
    /// <param name="nextDate">the next date on which this event should trigger</param>
    private void SetPopulationMigrationEvent(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        float maxProminencePercent,
        Identifier polityId,
        long nextDate)
    {
        float overflowFactor = 1.2f;

        float randomFactor = GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_PICK_PROMINENCE_PERCENT);

        float prominencePercent = maxProminencePercent * randomFactor * overflowFactor;

//#if DEBUG
//        if (prominencePercent >= 1)
//        {
//            Debug.LogWarning("prominence percent equal or greater than 1: " + prominencePercent);
//        }
//#endif

        prominencePercent = Mathf.Clamp01(prominencePercent);

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
        }

        if (HighestPolityProminence != null)
        {
            HighestPolityProminence.Polity.Territory.RemoveCell(Cell);
        }
    }

#if DEBUG
    public delegate void UpdateCalledDelegate();

    public static UpdateCalledDelegate UpdateCalled = null;
#endif

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

        //Profiler.BeginSample("Update Polity Cultural Prominences");

        UpdatePolityCulturalProminences(timeSpan);

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

    private void UpdatePolityCulturalProminences(long timeSpan)
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            Culture.UpdatePolityCulturalProminence(pi, timeSpan);
        }
    }

    private void PostUpdatePolityCulturalProminences()
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            Culture.PostUpdatePolityCulturalProminence(pi);
        }
    }

    private void PolityUpdateEffects(long timeSpan)
    {
        foreach (PolityProminence polityProminence in _polityProminences.Values)
        {
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
            Cell.GetActivityContribution(Culture, CellCulturalActivity.FarmingActivityId) * Population;

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

        SeaTravelFactor = SeaTravelBaseFactor * seafaringValue * shipbuildingValue * TravelWidthFactor * rangeFactor;
    }

    /// <summary>
    /// Calculates how much pressure there is for unorganized bands to migrate
    /// out of this cell
    /// </summary>
    /// <returns>the migration presure value</returns>
    public float CalculateUBMigrationPressure()
    {
        float populationFactor;

        if (OptimalPopulation > 0)
        {
            populationFactor = Population / (float)OptimalPopulation;
        }
        else
        {
            return 1;
        }

        //#if DEBUG
        //        if (Cell.IsSelected)
        //        {
        //            Debug.LogWarning("Debugging cell " + Cell.Position);
        //        }
        //#endif

        float minPopulationFactor = 0.90f;

        // if the population is not near its optimum then don't add pressure
        if (populationFactor < minPopulationFactor)
            return 0;

        float neighborhoodValue = 0;
        foreach (TerrainCell nCell in Cell.NeighborList)
        {
            neighborhoodValue = Mathf.Max(neighborhoodValue, CalculateUBMigrationValue(nCell));
        }

        // This will reduce the effect that low value cells have
        neighborhoodValue = Mathf.Clamp01(neighborhoodValue - 0.1f);

        neighborhoodValue = 100000 * Mathf.Pow(neighborhoodValue, 4);

        return neighborhoodValue / (1 + neighborhoodValue);
    }

    /// <summary>
    /// Calculates how much pressure there is for population sets to migrate
    /// out of this cell
    /// </summary>
    /// <returns>the migration presure value</returns>
    public float CalculateMigrationPressure()
    {
        // There's low pressure if there's already a migration event occurring
        if (HasMigrationEvent)
            return 0;

        // Get the pressure from unorganized bands
        float pressure = CalculateUBMigrationPressure();

        // Get the pressure from polity populations
        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            // 1 should be the max amount of pressure possible. So no need to calculate further
            if (pressure >= 1)
                return 1;

            pressure = Mathf.Max(pressure, prominence.Polity.CalculateMigrationPressure(this));
        }

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

        float migrationFactor = 1 - CalculateMigrationPressure();
        //migrationFactor = Mathf.Pow(migrationFactor, 4);

        float skillLevelFactor = Culture.MinimumSkillAdaptationLevel();
        float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel();

        float populationFactor = 0.0001f + Mathf.Abs(OptimalPopulation - Population);
        populationFactor = OptimalPopulation / populationFactor;

        populationFactor = Mathf.Min(populationFactor, MaxUpdateSpanFactor);

        float SlownessConstant = 100 * GenerationSpan;

        float mixFactor = SlownessConstant * randomFactor * migrationFactor
            * skillLevelFactor * knowledgeLevelFactor * populationFactor;

        long updateSpan = GenerationSpan + (long)Mathf.Ceil(mixFactor);

        if (updateSpan < 0)
            updateSpan = MaxUpdateSpan;

        updateSpan = (updateSpan < GenerationSpan) ? GenerationSpan : updateSpan;
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

            population = OptimalPopulation * MathUtility.RoundToSixDecimals(1 - Mathf.Pow(populationFactor, geometricTimeFactor));

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
            population = OptimalPopulation + (ExactPopulation - OptimalPopulation) * MathUtility.RoundToSixDecimals(Mathf.Exp(-timeFactor));

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

    public ICollection<PolityProminence> GetPolityProminences()
    {
        return _polityProminences.Values;
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

    public PolityProminence GetPolityProminence(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
            return null;

        return polityProminence;
    }

    public float GetPolityProminenceValue(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
            return 0;

        return polityProminence.Value;
    }

    public float GetFactionCoreDistance(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
            return float.MaxValue;

        return polityProminence.FactionCoreDistance;
    }

    public float GetPolityCoreDistance(Polity polity)
    {
        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            return float.MaxValue;
        }

        return polityProminence.PolityCoreDistance;
    }

    public float CalculateShortestFactionCoreDistance(Polity polity)
    {
        foreach (Faction faction in polity.GetFactions())
        {
            if (faction.CoreGroup == this)
                return 0;
        }

        float shortestDistance = MaxCoreDistance;

        foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors)
        {
            float distanceToCoreFromNeighbor = pair.Value.GetFactionCoreDistance(polity);

            if (distanceToCoreFromNeighbor == float.MaxValue)
                continue;

            float neighborDistance = Cell.NeighborDistances[pair.Key];

            float totalDistance = distanceToCoreFromNeighbor + neighborDistance;

            if (totalDistance < 0)
                continue;

            if (totalDistance < shortestDistance)
                shortestDistance = totalDistance;
        }

        return shortestDistance;
    }

    public float CalculateShortestPolityCoreDistance(Polity polity)
    {
        if (polity.CoreGroup == this)
            return 0;

        float shortestDistance = MaxCoreDistance;

        foreach (KeyValuePair<Direction, CellGroup> pair in Neighbors)
        {
            float distanceToCoreFromNeighbor = pair.Value.GetPolityCoreDistance(polity);

            if (distanceToCoreFromNeighbor == float.MaxValue)
                continue;

            float neighborDistance = Cell.NeighborDistances[pair.Key];

            float totalDistance = distanceToCoreFromNeighbor + neighborDistance;

            if (totalDistance < 0)
                continue;

            if (totalDistance < shortestDistance)
                shortestDistance = totalDistance;
        }

        return shortestDistance;
    }

    public float CalculateAdministrativeCost(PolityProminence pi)
    {
        float polityPopulation = Population * pi.Value;

        float distanceFactor = 500 + pi.FactionCoreDistance;

        float cost = polityPopulation * distanceFactor * 0.001f;

        if (cost < 0)
        {
            Debug.LogWarning("Calculated administrative cost less than 0: " + cost);

            return float.MaxValue;
        }

        return cost;
    }

#if DEBUG
    /// <summary>
    /// Updates polity prominences and values (for unit tests only)
    /// TODO: try get rid of this function without making UpdatePolityProminences public
    /// </summary>
    public void UpdatePolityProminences_test()
    {
        UpdatePolityProminences();
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
    /// Updates polity prominences and values
    /// </summary>
    /// <param name="afterPolityUpdates">
    /// Set to true if this function is being called after polity updates have been done</param>
    private void UpdatePolityProminences(bool afterPolityUpdates = false)
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
    }

    /// <summary>
    /// Calculates new prominence core distances
    /// (should be called in group PostUpdate)
    /// </summary>
    private void CalculatePolityPromCoreDistances()
    {
        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            prominence.CalculateNewCoreDistances();
        }
    }

    /// <summary>
    /// Finalizes polity prominence updates
    /// </summary>
    private void SetPolityPromCoreDistancesAndAdminLoad()
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
            TotalPolityProminenceValue += prominence.Value;
        }

#if DEBUG
        if ((_polityProminences.Count > 0) && (TotalPolityProminenceValue <= 0))
        {
            throw new System.Exception("Invalid state. Group: " + Id);
        }
#endif

        if (TotalPolityProminenceValue > 1.0)
        {
            Debug.LogWarning("Total Polity Prominence Value greater than 1: " +
                TotalPolityProminenceValue + ", Group: " + Id);
        }

        if (TotalPolityProminenceValue <= 0)
        {
            foreach (Faction faction in GetFactionCores())
            {
                if (!faction.BeingRemoved)
                {
                    throw new System.Exception(
                        "Group with no polity prominence has cores for factions " +
                        "not being removed. Group: " + Id + ", Faction: " + faction.Id);
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
    public void AddPolityProminenceValueDelta(Polity polity, float delta)
    {
        //if (Id == "0000000000164772058:1142429918914917756")
        //{
        //    Debug.LogWarning("Debugging Group " + Id);
        //}

        if (delta == 0)
        {
            Debug.LogWarning("Trying to add a prominence delta of 0. Will ignore...");
            return;
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

    /// <summary>
    /// Update all prominence values using all the applied value deltas so far
    /// </summary>
    /// <param name="afterPolityUpdates">
    /// Set to true if this function is being called after polity updates have been done</param>
    /// <returns>'true' if there was a change in prominence values</returns>
    private bool CalculateNewPolityProminenceValues(bool afterPolityUpdates = false)
    {
//#if DEBUG
//        if ((Id == "0000000001403622143:3199357175283981000") && (World.CurrentDate == 1403640616))
//        {
//            Debug.LogWarning("CalculateNewPolityProminenceValues: debugging group: " + Id);
//        }
//#endif

        // NOTE: after polity updates there might be no deltas, bu we might still need
        // to recalculate if the amount of prominences changed
        bool calculateRegardless = afterPolityUpdates && _polityProminences.Count > 0;

        // There was no new deltas so there's nothing to calculate
        if (!calculateRegardless && !_hasPromValueDeltas)
        {
            ResetProminenceValueDeltas();
            return false;
        }

        // add to the prominence deltas the current prominence values
        AddUBandsProminenceValueDelta(1f - TotalPolityProminenceValue);
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

        // get the offset to apply to all deltas so that there are no negative values
        // -----
        // NOTE: This is not a proper solution. A better one would require for every
        // prominence value transfer between polities to be recorded as a transaction,
        // and balancing out those transactions that push a prominence value below zero
        // independently from all others
        // -----
        float polPromDeltaOffset = Mathf.Min(0, _unorgBandsPromDelta);
        foreach (float delta in _polityPromDeltas.Values)
        {
            polPromDeltaOffset = Mathf.Min(polPromDeltaOffset, delta);
        }

        // replace prom values with deltas minus offset, and get the total sum
        float ubProminenceValue = _unorgBandsPromDelta - polPromDeltaOffset;
        float totalValue = ubProminenceValue;

#if DEBUG
        if (totalValue < 0)
        {
            Debug.LogWarning("initial totalValue less than 0: " + totalValue +
                ", polPromDeltaOffset: " + polPromDeltaOffset);
        }

        if (_polityPromDeltas.Count == 0)
        {
            Debug.LogWarning("amount of of polity prominence deltas equals to 0");

            if (totalValue <= 0)
            {
                throw new System.Exception("Unexpected total prominence value of: " + totalValue +
                    ", group: " + Id + ", date: " + World.CurrentDate);
            }
        }
#endif

        foreach (KeyValuePair<Polity, float> pair in _polityPromDeltas)
        {
            Polity polity = pair.Key;
            float newValue = pair.Value;

            newValue -= polPromDeltaOffset;

            if (newValue <= Polity.MinPolityProminenceValue)
            {
                if (afterPolityUpdates)
                {
                    Debug.LogWarning("Trying to remove polity " + polity.Id +
                        " after polity updates have already happened.  Group: " +  Id);
                }

                // try to remove prominences that would end up with a value far too small
                if (!SetPolityProminenceToRemove(pair.Key, false))
                {
                    // if not possible to remove this prominence, set it to a min value
                    newValue = Polity.MinPolityProminenceValue;
                }
                else
                {
                    // We will "transfer" its prominence value to unorganized bands
                    totalValue += newValue;
                    continue;
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

#if DEBUG
            if (newValue <= 0)
            {
                Debug.LogWarning("new value less than 0: " + newValue);
            }

            if (newValue > (totalValue + newValue))
            {
                Debug.LogWarning("new total value less than new value. prev total value: "
                    + totalValue + ", new value: " + newValue);
            }
#endif

            _polityProminences[polity.Id].Value = newValue;
            totalValue += newValue;
        }

        // normalize values
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
        }

        ResetProminenceValueDeltas();

        // remove any prominences set to be removed above
        RemovePolityProminences();

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
        bool throwIfNotPresent = true)
    {
        return SetPolityProminenceToRemove(polity.Id, throwIfNotPresent);
    }

    /// <summary>
    /// add a polity prominence to remove
    /// </summary>
    /// <param name="polityId">id of polity to which the prominence belongs</param>
    /// <param name="throwIfNotPresent">throw if prominence is not present</param>
    /// <returns>'false' if the polity prominence can't be removed</returns>
    public bool SetPolityProminenceToRemove(
        Identifier polityId,
        bool throwIfNotPresent = true)
    {
        if (!_polityProminences.ContainsKey(polityId))
        {
            if (throwIfNotPresent)
            {
                throw new System.ArgumentException(
                    "Prominence of polity " + polityId +
                    " not present in " + Id);
            }

            return true;
        }

        if (_polityProminencesToRemove.Contains(polityId))
        {
            return true;
        }

        // throw warning if this groups was set to become a faction core
        // even if the polity is about to be removed (even more so)
        if ((WillBecomeCoreOfFaction != null) &&
            (WillBecomeCoreOfFaction.PolityId == polityId))
        {
            Debug.LogWarning(
                "Group is set to become a faction core - group: " + Id +
                " - faction: " + WillBecomeCoreOfFaction.Id +
                " - polity: " + polityId + " - Date:" + World.CurrentDate);

            return false;
        }

        _polityProminencesToRemove.Add(polityId);
        return true;
    }

    /// <summary>
    /// Add a new polity prominence
    /// </summary>
    /// <param name="polity">polity to associate the new prominence with</param>
    /// <param name="initialValue">starting prominence value</param>
    /// <param name="originator">'true' if a new cell group is being initialized using
    /// this prominence</param>
    private void AddPolityProminence(Polity polity, float initialValue = 0, bool originator = false)
    {
        PolityProminence polityProminence = new PolityProminence(this, polity, initialValue);

        // Increase polity contacts
        foreach (PolityProminence otherProminence in _polityProminences.Values)
        {
            Polity.IncreaseContactGroupCount(polity, otherProminence.Polity);
        }

        _polityProminences.Add(polity.Id, polityProminence);

        // We want to update the polity if a group is added.
        SetPolityUpdate(polityProminence, true);

        polity.AddGroup(polityProminence);

        if (originator)
        {
            SetHighestPolityProminence(polityProminence);
            TotalPolityProminenceValue = initialValue;
        }
    }

    /// <summary>
    /// Remove all polities that where set to be removed
    /// </summary>
    /// <param name="updatePolity">
    /// Set to false if there's no need to update the removed polities after calling this</param>
    private void RemovePolityProminences(bool updatePolity = true)
    {
        foreach (Identifier polityId in _polityProminencesToRemove)
        {
            PolityProminence polityProminence = _polityProminences[polityId];

            _polityProminences.Remove(polityProminence.PolityId);

            // If the polity is no longer present, then the contacts would have already been removed
            if (polityProminence.Polity.StillPresent)
            {
                // Decrease polity contacts
                foreach (PolityProminence epi in _polityProminences.Values)
                {
                    Polity.DecreaseContactGroupCount(polityProminence.Polity, epi.Polity);
                }
            }

            // Remove all polity faction cores from group
            foreach (Faction faction in GetFactionCores())
            {
                if (faction.PolityId == polityProminence.PolityId)
                {
                    Debug.LogWarning(
                        "Removing polity prominence of faction that had core in group " + Id +
                        ", removing faction " + faction.Id +
                        " - polity: " + polityId + " - Date:" + World.CurrentDate);

                    faction.SetToRemove();
                }
            }

            polityProminence.Polity.RemoveGroup(polityProminence);

            if (updatePolity)
            {
                // We want to update the polity if a group is removed.
                SetPolityUpdate(polityProminence, true);
            }

            _hasRemovedProminences = true;
        }

        _polityProminencesToRemove.Clear();
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
                "there are multiple prominences. Group: " + Id);
        }

        SetHighestPolityProminence(highestProminence);
    }

    public void SetToUpdate()
    {
        World.AddGroupToUpdate(this);
    }

    public void AddProperty(string property)
    {
        _properties.Add(property);
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

    public override void Synchronize()
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

    public void LoadPolityProminences()
    {
        foreach (PolityProminence p in PolityProminences)
        {
            _polityProminences.Add(p.PolityId, p);
        }
    }

    public void PrefinalizeLoad()
    {
        LoadPolityProminences();
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

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

        Cell = World.GetCell(Longitude, Latitude);

        Cell.Group = this;

        Neighbors = new Dictionary<Direction, CellGroup>(8);

        foreach (KeyValuePair<Direction, TerrainCell> pair in Cell.Neighbors)
        {
            if (pair.Value.Group != null)
            {
                CellGroup group = pair.Value.Group;

                Neighbors.Add(pair.Key, group);

                Direction dir = TerrainCell.ReverseDirection(pair.Key);

                group.AddNeighbor(dir, this);
            }
        }

        World.UpdateMostPopulousGroup(this);

        Culture.World = World;
        Culture.Group = this;
        Culture.FinalizeLoad();

        if (Cell == null)
        {
            throw new System.Exception("Cell [" + Longitude + "," + Latitude + "] is null");
        }

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
            if (p.Polity == null)
            {
                throw new System.Exception("Missing polity with id:" + p.PolityId);
            }

            if ((HighestPolityProminence == null) || (HighestPolityProminence.Value < p.Value))
            {
                HighestPolityProminence = p;
            }
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

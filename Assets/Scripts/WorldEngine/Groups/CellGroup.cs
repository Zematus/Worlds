﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using UnityEngine.Profiling;

public class CellGroup : HumanGroup
{
    public const long GenerationSpan = 25 * World.YearLength;

    public const long MaxUpdateSpan = GenerationSpan * 8000;

    public const float MaxUpdateSpanFactor = MaxUpdateSpan / GenerationSpan;

    public const float NaturalDeathRate = 0.03f; // more or less 0.5/half-life (22.87 years for paleolitic life expectancy of 33 years)
    public const float NaturalBirthRate = 0.105f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)
    public const float MinChangeRate = -1.0f; // Should cancel out death rate in perfect circumstances (hunter-gathererers in grasslands)

    public const float NaturalGrowthRate = NaturalBirthRate - NaturalDeathRate;

    public const float PopulationForagingConstant = 10;
    public const float PopulationFarmingConstant = 5;
    public const float PopulationFishingConstant = 5;

    public const float MinKnowledgeTransferValue = 0.25f;

    public const float SeaTravelBaseFactor = 25f;

    public const float MigrationFactor = 0.1f;

    public const float MaxMigrationAltitudeDelta = 1f; // in meters

    public const float MaxCoreDistance = 1000000000000f;

    public static float TravelWidthFactor;

    public static List<ICellGroupEventGenerator> OnSpawnEventGenerators;
    
    [XmlAttribute]
    public long Id;
    
    [XmlAttribute("PMD")]
    public int PreferredMigrationDirectionInt;
    
    [XmlAttribute("PEP")]
    public float PreviousExactPopulation;
    
    [XmlAttribute("EP")]
    public float ExactPopulation; // TODO: Get rid of 'float' population values
    
    [XmlAttribute("P")]
    public bool StillPresent = true;
    
    [XmlAttribute("ID")]
    public long InitDate;
    
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
    
    [XmlAttribute("MV")]
    public float MigrationValue;
    
    [XmlAttribute("TMV")]
    public float TotalMigrationValue;
    
    [XmlAttribute("PE")]
    public float PolityExpansionValue;
    
    [XmlAttribute("TPE")]
    public float TotalPolityExpansionValue;
    
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

    [XmlAttribute("PEEv")]
    public bool HasPolityExpansionEvent = false;
    [XmlAttribute("PED")]
    public long PolityExpansionEventDate;
    [XmlAttribute("EGId")]
    public long ExpansionTargetGroupId;
    [XmlAttribute("EPId")]
    public long ExpandingPolityId;
    
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
    
    public Route SeaMigrationRoute = null;
    
    public List<string> Flags;
    
    public CellCulture Culture;
    
    public List<string> Properties;
    
    public List<long> FactionCoreIds;

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
    public Direction PreferredMigrationDirection;

    [XmlIgnore]
    public Dictionary<long, Faction> FactionCores = new Dictionary<long, Faction>();

    [XmlIgnore]
    public UpdateCellGroupEvent UpdateEvent;

    [XmlIgnore]
    public MigrateGroupEvent MigrationEvent;

    [XmlIgnore]
    public ExpandPolityProminenceEvent PolityExpansionEvent;

    [XmlIgnore]
    public TribeFormationEvent TribeFormationEvent;

    [XmlIgnore]
    public TerrainCell Cell;

    [XmlIgnore]
    public MigratingGroup MigratingGroup = null;

    [XmlIgnore]
    public bool WillBecomeFactionCore = false;

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

    private Dictionary<long, PolityProminence> _polityProminences = new Dictionary<long, PolityProminence>();

    private CellUpdateType _cellUpdateType = CellUpdateType.None;
    private CellUpdateSubType _cellUpdateSubtype = CellUpdateSubType.None;

    private HashSet<long> _polityProminencesToRemove = new HashSet<long>();
    private Dictionary<long, PolityProminence> _polityProminencesToAdd = new Dictionary<long, PolityProminence>();

    private HashSet<string> _flags = new HashSet<string>();

    private bool _alreadyUpdated = false;

    private List<Effect> _deferredEffects = new List<Effect>();

    private HashSet<string> _properties = new HashSet<string>();

    private HashSet<string> _propertiesToAquire = new HashSet<string>();
    private HashSet<string> _propertiesToLose = new HashSet<string>();

    [XmlIgnore]
    public int PreviousPopulation
    {
        get
        {
            return (int)Mathf.Floor(PreviousExactPopulation);
        }
    }

    [XmlIgnore]
    public int Population
    {
        get
        {
            int population = (int)Mathf.Floor(ExactPopulation);
            
            if (population < 0)
            {
                throw new System.Exception("Negative Population: " + population + ", Id: " + Id);
            }

            return population;
        }
    }

    public CellGroup()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    public CellGroup(MigratingGroup migratingGroup, int splitPopulation) : this(migratingGroup.World, migratingGroup.TargetCell, splitPopulation, migratingGroup.Culture, migratingGroup.MigrationDirection)
    {
        for (int i = 0; i < migratingGroup.PolityProminencesCount; i++)
        {
            PolityProminence p = new PolityProminence(this, migratingGroup.PolityProminences[i]);

            _polityProminencesToAdd.Add(p.PolityId, p);

            p.FactionCoreDistance = CalculateShortestFactionCoreDistance(p.Polity);
            p.PolityCoreDistance = CalculateShortestPolityCoreDistance(p.Polity);
            p.NewFactionCoreDistance = p.FactionCoreDistance;
            p.NewPolityCoreDistance = p.PolityCoreDistance;
        }
    }

    public CellGroup(World world, TerrainCell cell, int initialPopulation, Culture baseCulture = null, Direction migrationDirection = Direction.Null) : base(world)
    {
        InitDate = World.CurrentDate;
        LastUpdateDate = InitDate;

        PreviousExactPopulation = 0;
        ExactPopulation = initialPopulation;

        Cell = cell;
        Longitude = cell.Longitude;
        Latitude = cell.Latitude;

        Cell.Group = this;

        Id = Cell.GenerateUniqueIdentifier(World.CurrentDate, 1L, 0);

        if (migrationDirection == Direction.Null)
        {
            int offset = Cell.GetNextLocalRandomInt(RngOffsets.CELL_GROUP_UPDATE_MIGRATION_DIRECTION, TerrainCell.MaxNeighborDirections);

            PreferredMigrationDirection = Cell.TryGetNeighborDirection(offset);
        }
        else
        {
            PreferredMigrationDirection = migrationDirection;
        }

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

        InitializeDefaultPreferences(initialGroup);
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

    public void AddDeferredEffect(Effect effect)
    {
        _deferredEffects.Add(effect);
    }

    public static void ResetEventGenerators()
    {
        OnSpawnEventGenerators = new List<ICellGroupEventGenerator>();
    }

    public void InitializeOnSpawnEvents()
    {
        foreach (ICellGroupEventGenerator generator in OnSpawnEventGenerators)
        {
            if (generator.CanAssignEventTypeToGroup(this))
            {
                generator.GenerateAndAssignEvent(this);
            }
        }
    }

    public void UpdatePreferredMigrationDirection()
    {
        int dir = ((int)PreferredMigrationDirection) + RandomUtility.NoOffsetRange(Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_UPDATE_MIGRATION_DIRECTION));

        PreferredMigrationDirection = Cell.TryGetNeighborDirection(dir);
    }

    public Direction GenerateCoreMigrationDirection()
    {
        int dir = (int)PreferredMigrationDirection;

        float fDir = RandomUtility.PseudoNormalRepeatDistribution(Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_GENERATE_CORE_MIGRATION_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

        return TryGetNeighborDirection((int)fDir);
    }

    public Direction GeneratePolityExpansionDirection()
    {
        int dir = (int)PreferredMigrationDirection;

        float fDir = RandomUtility.PseudoNormalRepeatDistribution(Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_GENERATE_PROMINENCE_TRANSFER_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

        return TryGetNeighborDirection((int)fDir);
    }

    public Direction GenerateGroupMigrationDirection()
    {
        int dir = (int)PreferredMigrationDirection;

        Profiler.BeginSample("RandomUtility.PseudoNormalRepeatDistribution");

        float fDir = RandomUtility.PseudoNormalRepeatDistribution(Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_GENERATE_GROUP_MIGRATION_DIRECTION), 0.05f, dir, TerrainCell.MaxNeighborDirections);

        Profiler.EndSample();

        Profiler.BeginSample("Cell.TryGetNeighborDirection");

        Direction direction = Cell.TryGetNeighborDirection((int)fDir);

        Profiler.EndSample();

        return direction;
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

    public long GenerateUniqueIdentifier(long date, long oom = 1L, long offset = 0L)
    {
        return Cell.GenerateUniqueIdentifier(date, oom, offset);
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
    }

    public void InitializeDefaultEvents()
    {
        InitializeOnSpawnEvents();

        //if (BoatMakingDiscoveryEvent.CanSpawnIn(this))
        //{
        //    long triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate(this);

        //    if (triggerDate > World.MaxSupportedDate)
        //        return;

        //    if (triggerDate == long.MinValue)
        //        return;

        //    World.InsertEventToHappen(new BoatMakingDiscoveryEvent(this, triggerDate));
        //}

        //if (PlantCultivationDiscoveryEvent.CanSpawnIn(this))
        //{
        //    long triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate(this);

        //    if (!triggerDate.IsInsideRange(World.CurrentDate + 1, World.MaxSupportedDate))
        //        return;

        //    World.InsertEventToHappen(new PlantCultivationDiscoveryEvent(this, triggerDate));
        //}
    }

    public void InitializeDefaultPreferences(bool initialGroup)
    {
        if (initialGroup)
        {
            Culture.AddPreferenceToAcquire(CellCulturalPreference.CreateAuthorityPreference(this, 0.5f));
            Culture.AddPreferenceToAcquire(CellCulturalPreference.CreateCohesionPreference(this, 0.5f));
            Culture.AddPreferenceToAcquire(CellCulturalPreference.CreateIsolationPreference(this, 0.5f));
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
            else
            {
                string skillId = BiomeSurvivalSkill.GenerateId(biome);

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

        foreach (TerrainCell neighborCell in Cell.Neighbors.Values)
        {
            foreach (string id in neighborCell.PresentBiomeIds)
            {
                biomes.Add(Biome.Biomes[id]);
            }
        }

        return biomes;
    }

    public void MergeGroup(MigratingGroup group)
    {
        float newPopulation = Population + group.Population;

        float percentage = group.Population / newPopulation;

        if (!percentage.IsInsideRange(0, 1))
        {
            Debug.LogWarning("MergeGroup, percentage increase outside of range (0,1): " + percentage);
        }

        percentage = Mathf.Clamp01(percentage);

        //		#if DEBUG
        //		float oldExactPopulation = ExactPopulation;
        //		#endif

        ExactPopulation = newPopulation;

#if DEBUG
        if (Population < -1000)
        {

            Debug.Break();
            throw new System.Exception("Debug.Break");
        }
#endif

        Culture.MergeCulture(group.Culture, percentage);
        MergePolityProminences(group.PolityProminences, group.PolityProminencesCount, percentage);

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (Id == Manager.TracingData.GroupId) {
        //				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"MergeGroup - Group:" + groupId, 
        //					"CurrentDate: " + World.CurrentDate +
        //					", group.SourceGroupId: " + group.SourceGroupId + 
        //					", oldExactPopulation: " + oldExactPopulation + 
        //					", source group.Population: " + group.Population + 
        //					", newPopulation: " + newPopulation + 
        //					", group.PolityProminences.Count: " + group.PolityProminences.Count + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        TriggerInterference();
    }

    public void MergePolityProminence(PolityProminence sourcePolityProminence, float percentOfTarget)
    {
        Dictionary<long, PolityProminence> targetPolityProminences = new Dictionary<long, PolityProminence>(_polityProminences);

        foreach (PolityProminence pi in _polityProminencesToAdd.Values)
        {

            targetPolityProminences.Add(pi.PolityId, pi);
        }

        MergePolityProminenceInternal_Add(sourcePolityProminence, targetPolityProminences, percentOfTarget);

        MergePolityProminencesInternal_Finalize(targetPolityProminences, percentOfTarget);
    }

    public void MergePolityProminences(List<PolityProminence> sourcePolityProminences, int sourceProminencesCount, float percentOfTarget)
    {
        Dictionary<long, PolityProminence> targetPolityProminences = new Dictionary<long, PolityProminence>(_polityProminences);

        foreach (PolityProminence pi in _polityProminencesToAdd.Values)
        {
            targetPolityProminences.Add(pi.PolityId, pi);
        }

        if (sourcePolityProminences.Count < sourceProminencesCount)
        {
            throw new System.Exception("sourcePolityProminences.Count less than sourceProminencesCount: " + sourcePolityProminences.Count + " < " + sourceProminencesCount);
        }

        for (int i = 0; i < sourceProminencesCount; i++)
        {
            MergePolityProminenceInternal_Add(sourcePolityProminences[i], targetPolityProminences, percentOfTarget);
        }

        MergePolityProminencesInternal_Finalize(targetPolityProminences, percentOfTarget);
    }

    private void MergePolityProminenceInternal_Add(PolityProminence sourcePolityProminence, Dictionary<long, PolityProminence> targetPolityProminences, float percentOfTarget)
    {
        Polity polity = sourcePolityProminence.Polity;
        float prominenceValue = sourcePolityProminence.Value;

        float currentNewValue = 0;

        PolityProminence pTargetPolityProminence = null;

        if (targetPolityProminences.TryGetValue(sourcePolityProminence.PolityId, out pTargetPolityProminence))
        {
            currentNewValue = pTargetPolityProminence.NewValue;
            targetPolityProminences.Remove(pTargetPolityProminence.PolityId);
        }

        float newValue = (currentNewValue * (1 - percentOfTarget)) + (prominenceValue * percentOfTarget);

        //			#if DEBUG
        //			if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //				if (Id == Manager.TracingData.GroupId) {
        //					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //
        //					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //						"MergePolities:Add - Group:" + groupId + 
        //						", pProminence.PolityId: " + pProminence.PolityId,
        //						"CurrentDate: " + World.CurrentDate  +
        //						", currentValue: " + currentValue +
        //						", prominenceValue: " + prominenceValue +
        //						", Polity.TotalGroupProminenceValue: " + pProminence.Polity.TotalGroupProminenceValue + 
        //						", newValue: " + newValue +
        //						", percentOfTarget: " + percentOfTarget +
        //						"");
        //
        //					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //				}
        //			}
        //			#endif

        SetPolityProminence(polity, newValue);
    }

    private void MergePolityProminencesInternal_Finalize(Dictionary<long, PolityProminence> targetPolityProminences, float percentOfTarget)
    {
        foreach (PolityProminence pProminence in targetPolityProminences.Values)
        {
            float prominenceValue = pProminence.NewValue;

            float newValue = prominenceValue * (1 - percentOfTarget);

            //			#if DEBUG
            //			if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
            //				if (Id == Manager.TracingData.GroupId) {
            //
            //					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
            //
            //					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //						"MergePolities:Rescale - Group:" + groupId + 
            //						", pProminence.PolityId: " + pProminence.PolityId,
            //						"CurrentDate: " + World.CurrentDate  +
            //						", prominenceValue: " + prominenceValue + 
            //						", Polity.TotalGroupProminenceValue: " + pProminence.Polity.TotalGroupProminenceValue + 
            //						", newProminenceValue: " + newProminenceValue + 
            //						", percentOfTarget: " + percentOfTarget + 
            //						"");
            //
            //					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
            //				}
            //			}
            //			#endif

            SetPolityProminence(pProminence.Polity, newValue);
        }
    }

    public int SplitGroup(MigratingGroup group)
    {
        int splitPopulation = (int)Mathf.Floor(Population * group.PercentPopulation);

        //		#if DEBUG
        //		float oldExactPopulation = ExactPopulation;
        //		#endif

        ExactPopulation -= splitPopulation;

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if ((Id == Manager.TracingData.GroupId) || 
        //				((group.TargetCell.Group != null) && (group.TargetCell.Group.Id == Manager.TracingData.GroupId))) {
        //				string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //				string targetInfo = "Long:" + group.TargetCell.Longitude + "|Lat:" + group.TargetCell.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"SplitGroup - sourceGroup:" + groupId,
        //					"CurrentDate: " + World.CurrentDate + 
        //					", targetInfo: " + targetInfo + 
        //					", ExactPopulation: " + ExactPopulation + 
        //					", oldExactPopulation: " + oldExactPopulation + 
        //					", migratingGroup.PercentPopulation: " + group.PercentPopulation + 
        //					", splitPopulation: " + splitPopulation + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif
        
        if (Population < 0)
        {
            throw new System.Exception("Population less than 0");
        }

        return splitPopulation;
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

    public void PostUpdate_BeforePolityUpdates()
    {
        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "PostUpdate_BeforePolityUpdates - Group:" + Id,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        _alreadyUpdated = false;

        if (Population < 2)
        {
            World.AddGroupToRemove(this);
            return;
        }

        Profiler.BeginSample("Update Terrain Attributes");

        UpdateTerrainAttributes();

        Profiler.EndSample();

        Profiler.BeginSample("Update Terrain Farmland Percentage");

        UpdateTerrainFarmlandPercentage();

        Profiler.EndSample();

        Profiler.BeginSample("Culture PostUpdate");

        Culture.PostUpdate();

        Profiler.EndSample();

        Profiler.BeginSample("Update Properties");

        UpdateProperties();

        Profiler.EndSample();

        Profiler.BeginSample("Set Faction Updates");

        SetFactionUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("Culture PostUpdate Cleanup");

        Culture.CleanUpAtributesToGet();

        Profiler.EndSample();

        Profiler.BeginSample("Set Polity Updates");

        SetPolityUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("Post Update Polity Prominences");

        PostUpdatePolityProminences_BeforePolityUpdates();

        Profiler.EndSample();

        Profiler.BeginSample("PostUpdate Polity Cultural Prominences");

        PostUpdatePolityCulturalProminences();

        Profiler.EndSample();

        Profiler.BeginSample("Update Polity Prominence Administrative Costs");

        UpdatePolityProminenceAdministrativeCosts();

        Profiler.EndSample();
    }

    public void PostUpdate_AfterPolityUpdates()
    {
        PostUpdatePolityProminences_AfterPolityUpdates();
    }

    public void SetToBecomeFactionCore()
    {
        WillBecomeFactionCore = true;

        World.AddGroupToCleanupAfterUpdate(this);
    }

    public void AfterUpdateCleanup()
    {
        WillBecomeFactionCore = false;
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

        Profiler.BeginSample("Calculate Optimal Population");

        OptimalPopulation = CalculateOptimalPopulation(Cell);

        Profiler.EndSample();

        Profiler.BeginSample("Calculate Local Migration Value");

        CalculateLocalMigrationValue();

        Profiler.EndSample();

        Profiler.BeginSample("Consider Land Migration");

        ConsiderLandMigration();

        Profiler.EndSample();

        Profiler.BeginSample("Consider Sea Migration");

        ConsiderSeaMigration();

        Profiler.EndSample();

        Profiler.BeginSample("Consider Prominence Expansion");

        ConsiderPolityProminenceExpansion();

        Profiler.EndSample();

        Profiler.BeginSample("Calculate Next Update Date");

        NextUpdateDate = CalculateNextUpdateDate();

        Profiler.EndSample();

        LastUpdateDate = World.CurrentDate;

        if (NextUpdateDate == long.MinValue)
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

    public float CalculateAltitudeDeltaFactor(TerrainCell targetCell)
    {
        if (targetCell == Cell)
            return 0.5f;

        float altitudeChange = Mathf.Max(0, targetCell.Altitude) - Mathf.Max(0, Cell.Altitude);
        float altitudeDelta = 2 * altitudeChange / (Cell.Area + targetCell.Area);

        float altitudeDeltaFactor = 1 - (Mathf.Clamp(altitudeDelta, -MaxMigrationAltitudeDelta, MaxMigrationAltitudeDelta) + MaxMigrationAltitudeDelta) / 2 * MaxMigrationAltitudeDelta;

        return altitudeDeltaFactor;
    }

    public float CalculateMigrationValue(TerrainCell cell)
    {
        float areaFactor = cell.MaxAreaPercent;

        float altitudeDeltaFactor = CalculateAltitudeDeltaFactor(cell);
        float altitudeDeltaFactorPow = Mathf.Pow(altitudeDeltaFactor, 4);
        
        if (float.IsNaN(altitudeDeltaFactorPow))
        {
            throw new System.Exception("float.IsNaN(altitudeDeltaFactorPow)");
        }

        int existingPopulation = 0;

        float popDifferenceFactor = 1;

        if (cell.Group != null)
        {
            existingPopulation = cell.Group.Population;

            popDifferenceFactor = (float)Population / (float)(Population + existingPopulation);
            popDifferenceFactor = Mathf.Pow(popDifferenceFactor, 4);
        }

        float noMigrationFactor = 1;

        float optimalPopulation = OptimalPopulation;

        if (cell != Cell)
        {
            noMigrationFactor = MigrationFactor;

            optimalPopulation = CalculateOptimalPopulation(cell);
        }

        float targetOptimalPopulationFactor = 0;

        if (optimalPopulation > 0)
        {
            targetOptimalPopulationFactor = optimalPopulation / (existingPopulation + optimalPopulation);
        }

        float cellValue = altitudeDeltaFactorPow * areaFactor * popDifferenceFactor * noMigrationFactor * targetOptimalPopulationFactor;
        
        if (float.IsNaN(cellValue))
        {
            throw new System.Exception("float.IsNaN(cellValue)");
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                //				if ((Longitude == cell.Longitude) && (Latitude == cell.Latitude)) {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //                string targetCellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;

        //                if (cell.Group != null)
        //                {
        //                    targetCellInfo = "Id:" + cell.Group.Id + "|" + targetCellInfo;
        //                }

        //                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //                System.Reflection.MethodBase method1 = stackTrace.GetFrame(1).GetMethod();
        //                string callingMethod1 = method1.Name;
        //                string callingClass1 = method1.DeclaringType.ToString();

        //                System.Reflection.MethodBase method2 = stackTrace.GetFrame(2).GetMethod();
        //                string callingMethod2 = method2.Name;
        //                string callingClass2 = method2.DeclaringType.ToString();

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "CalculateMigrationValue - Group:" + groupId +
        //                    ", targetCell: " + targetCellInfo,
        //                    ", CurrentDate: " + World.CurrentDate +
        //                    ", altitudeDeltaFactor: " + altitudeDeltaFactor +
        //                    ", ExactPopulation: " + ExactPopulation +
        //                    ", target existingPopulation: " + existingPopulation +
        //                    ", popDifferenceFactor: " + popDifferenceFactor +
        //                    ", OptimalPopulation: " + OptimalPopulation +
        //                    ", target optimalPopulation: " + optimalPopulation +
        //                    ", targetOptimalPopulationFactor: " + targetOptimalPopulationFactor +
        //                    ", Calling method 1: " + callingClass1 + "." + callingMethod1 +
        //                    ", Calling method 2: " + callingClass2 + "." + callingMethod2 +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //                //				}
        //            }
        //        }
        //#endif

        return cellValue;
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

    public void CalculateLocalMigrationValue()
    {
        MigrationValue = CalculateMigrationValue(Cell);

        TotalMigrationValue = MigrationValue;
        
        if (float.IsNaN(TotalMigrationValue))
        {
            throw new System.Exception("float.IsNaN(TotalMigrationValue)");
        }
    }

    private class CellWeight : CollectionUtility.ElementWeightPair<TerrainCell>
    {
        public CellWeight(TerrainCell cell, float weight) : base(cell, weight)
        {

        }
    }

    private class GroupWeight : CollectionUtility.ElementWeightPair<CellGroup>
    {
        public GroupWeight(CellGroup group, float weight) : base(group, weight)
        {

        }
    }

    private class PolityProminenceWeight : CollectionUtility.ElementWeightPair<PolityProminence>
    {
        public PolityProminenceWeight(PolityProminence polityProminence, float weight) : base(polityProminence, weight)
        {

        }
    }

    public void ConsiderLandMigration()
    {
        if (HasMigrationEvent)
            return;

        Profiler.BeginSample("UpdatePreferredMigrationDirection");

        UpdatePreferredMigrationDirection();

        Profiler.EndSample();

        //		int targetCellIndex = Cell.GetNextLocalRandomInt (RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION_TARGET, Cell.Neighbors.Count);
        //
        //		TerrainCell targetCell = Cell.Neighbors.Values.ElementAt (targetCellIndex);

        Profiler.BeginSample("GenerateGroupMigrationDirection");

        Direction migrationDirection = GenerateGroupMigrationDirection();

        TerrainCell targetCell = Cell.Neighbors[migrationDirection];

        Profiler.EndSample();

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                string cellInfo = "No target cell";

        //                if (targetCell != null)
        //                {
        //                    cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
        //                }

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderLandMigration - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", target cell: " + cellInfo +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        Profiler.BeginSample("CalculateMigrationValue");

        float cellValue = CalculateMigrationValue(targetCell);

        TotalMigrationValue += cellValue;

        //#if DEBUG
        //        if (float.IsNaN(TotalMigrationValue))
        //        {
        //            throw new System.Exception("float.IsNaN (TotalMigrationValue)");
        //        }
        //#endif

        float migrationChance = cellValue / TotalMigrationValue;

        Profiler.EndSample();

        float rollValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_LAND_MIGRATION_CHANCE);

        if (rollValue > migrationChance)
            return;

        Profiler.BeginSample("CalculateAdaptionToCell");

        CalculateAdaptionToCell(targetCell, out float cellForagingCapacity, out float cellSurvivability);

        Profiler.EndSample();

        if (cellSurvivability <= 0)
            return;

        Profiler.BeginSample("CalculateAltitudeDeltaFactor");

        float cellAltitudeDeltaFactor = CalculateAltitudeDeltaFactor(targetCell);

        Profiler.EndSample();

        float travelFactor =
            cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
            cellSurvivability * cellSurvivability * targetCell.Accessibility;

        travelFactor = Mathf.Clamp(travelFactor, 0.0001f, 1);

        int travelTime = (int)Mathf.Ceil(World.YearLength * Cell.Width / (TravelWidthFactor * travelFactor));

        long nextDate = World.CurrentDate + travelTime;

        if (nextDate <= World.CurrentDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderLandMigration - nextDate (" + nextDate +
                ") less or equal to World.CurrentDate (" + World.CurrentDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width + 
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }
        else if (nextDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderLandMigration - nextDate (" + nextDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width +
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderLandMigration - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        Profiler.BeginSample("SetMigrationEvent");

        SetMigrationEvent(targetCell, migrationDirection, MigrationType.Land, nextDate);

        Profiler.EndSample();
    }

    public void ConsiderSeaMigration()
    {
        if (SeaTravelFactor <= 0)
            return;

        if (HasMigrationEvent)
            return;

        //#if DEBUG
        //        bool hadMigrationRoute = SeaMigrationRoute != null;

        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderSeaMigration 1 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", has migration route: " + hadMigrationRoute +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

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

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                string cellInfo = "No target cell";

        //                if (targetCell != null)
        //                {
        //                    cellInfo = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
        //                }

        //                //string rngOutputs = "[" + string.Join("|", SeaMigrationRoute.DebugLogs.ToArray()) + "]";

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderSeaMigration 2 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", SeaMigrationRoute.CreationDate: " + SeaMigrationRoute.CreationDate +
        //                    ", target cell: " + cellInfo +
        //                    //", SeaMigrationRoute.RngOutputs: " + rngOutputs +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (targetCell == Cell)
            return;

        if (targetCell == null)
            return;

        TotalMigrationValue += CalculateMigrationValue(targetCell);

        if (float.IsNaN(TotalMigrationValue))
        {
            throw new System.Exception("float.IsNaN (TotalMigrationValue)");
        }

        CalculateAdaptionToCell(targetCell, out float cellForagingCapacity, out float cellSurvivability);

        if (cellSurvivability <= 0)
            return;

        float routeLength = SeaMigrationRoute.Length;
        float routeLengthFactor = Mathf.Pow(routeLength, 2);

        float successChance = SeaTravelFactor / (SeaTravelFactor + routeLengthFactor);

        float attemptValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_SEA_MIGRATION);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                string cellPositions = "";

        //                bool first = true;
        //                foreach (TerrainCell cell in SeaMigrationRoute.Cells)
        //                {
        //                    cellPositions += cell.Position.ToString();

        //                    if (first)
        //                        first = false;
        //                    else
        //                        cellPositions += ",";
        //                }

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderSeaMigration 3 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", attemptValue: " + attemptValue +
        //                    ", successChance: " + successChance +
        //                    ", SeaTravelFactor: " + SeaTravelFactor +
        //                    ", routeLength: " + routeLength +
        //                    ", route CreationDate: " + SeaMigrationRoute.CreationDate +
        //                    ", route positions: " + cellPositions +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (attemptValue > successChance)
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

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderSeaMigration 4 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        SetMigrationEvent(targetCell, migrationDirection, MigrationType.Sea, nextDate);
    }

    private void SetMigrationEvent(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        long nextDate)
    {
        if (MigrationEvent == null)
        {
            MigrationEvent =
                new MigrateGroupEvent(this, targetCell, migrationDirection, migrationType, nextDate);
        }
        else
        {
            MigrationEvent.Reset(targetCell, migrationDirection, migrationType, nextDate);
        }

        World.InsertEventToHappen(MigrationEvent);

        HasMigrationEvent = true;

        MigrationEventDate = nextDate;
        MigrationEventSpawnDate = MigrationEvent.SpawnDate;
        MigrationTargetLongitude = targetCell.Longitude;
        MigrationTargetLatitude = targetCell.Latitude;
        MigrationEventDirectionInt = (int)migrationDirection;
        MigrationEventTypeInt = (int)migrationType;
    }

    public Direction TryGetNeighborDirection(int offset)
    {
        if (Neighbors.Count <= 0)
            return Direction.Null;

        int dir = (int)Mathf.Repeat(offset, TerrainCell.MaxNeighborDirections);

        while (true)
        {
            if (Neighbors.ContainsKey((Direction)dir))
                return (Direction)dir;

            dir = (dir + TerrainCell.NeighborSearchOffset) % TerrainCell.MaxNeighborDirections;
        }
    }

    public void ConsiderPolityProminenceExpansion()
    {
        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderPolityProminenceExpansion Part 1 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", Neighbors.Count: " + Neighbors.Count +
        //                    ", PolityProminences.Count: " + PolityProminences.Count +
        //                    ", LastUpdateDate: " + LastUpdateDate +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        PolityExpansionValue = 0;
        TotalPolityExpansionValue = 0;

        if (_polityProminences.Count <= 0)
            return;

        if (Neighbors.Count <= 0)
            return;

        if (HasPolityExpansionEvent)
            return;

        //		Profiler.BeginSample ("Select Random Polity Prominence");

        List<PolityProminenceWeight> polityProminenceWeights = new List<PolityProminenceWeight>(_polityProminences.Count);

//#if DEBUG
//        string polityProminencesStr = "";
//#endif

        foreach (PolityProminence pi in _polityProminences.Values)
        {
            polityProminenceWeights.Add(new PolityProminenceWeight(pi, pi.Value));

//#if DEBUG
//            polityProminencesStr += "[" + pi.PolityId + "|" + pi.Value + "],";
//#endif
        }

        float selectionValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_POLITY_PROMINENCE_EXPANSION_POLITY);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderPolityProminenceExpansion Part 2 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", Neighbors.Count: " + Neighbors.Count +
        //                    ", polityProminencesStr: " + polityProminencesStr +
        //                    ", selectionValue: " + selectionValue +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        PolityProminence selectedPi = CollectionUtility.WeightedSelection(polityProminenceWeights.ToArray(), TotalPolityProminenceValue, selectionValue);

        //		Profiler.EndSample ();

        PolityExpansionValue = 1;
        TotalPolityExpansionValue = 1;

        //		Profiler.BeginSample ("Select Random Target Group for Polity Expansion");

        //		int targetGroupIndex = Cell.GetNextLocalRandomInt (RngOffsets.CELL_GROUP_CONSIDER_POLITY_PROMINENCE_EXPANSION_TARGET, TerrainCell.MaxNeighborDirections);
        //
        //		CellGroup targetGroup = GetNeighborGroup (targetGroupIndex);

        Direction expansionDirection = GeneratePolityExpansionDirection();

        if (expansionDirection == Direction.Null)
            return;

        CellGroup targetGroup = Neighbors[expansionDirection];

        //		Profiler.EndSample ();

        if (!targetGroup.StillPresent)
            return;

        //		Profiler.BeginSample ("Calculate Polity Expansion Value");

        float groupValue = selectedPi.Polity.CalculateGroupProminenceExpansionValue(this, targetGroup, selectedPi.Value);

        if (groupValue <= 0)
            return;

        TotalPolityExpansionValue += groupValue;

        //		Profiler.EndSample ();

        float expansionChance = groupValue / TotalPolityExpansionValue;

        float rollValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CONSIDER_POLITY_PROMINENCE_EXPANSION_CHANCE);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //                string targetGroupId = "Id:" + targetGroup.Id + "|Long:" + targetGroup.Longitude + "|Lat:" + targetGroup.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderPolityProminenceExpansion Part 3 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", Neighbors.Count: " + Neighbors.Count +
        //                    //", groupValue: " + groupValue +
        //                    //", PolityExpansionValue: " + PolityExpansionValue +
        //                    //", TotalPolityExpansionValue: " + TotalPolityExpansionValue +
        //                    ", rollValue: " + rollValue +
        //                    ", selectedPi.PolityId: " + selectedPi.PolityId +
        //                    ", targetGroup: " + targetGroupId +
        //                    ", rollValue: " + rollValue +
        //                    //", expansionChance: " + expansionChance +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (rollValue > expansionChance)
            return;

        CalculateAdaptionToCell(targetGroup.Cell, out float cellForagingCapacity, out float cellSurvivability);

        if (cellSurvivability <= 0)
            return;

        float cellAltitudeDeltaFactor = CalculateAltitudeDeltaFactor(targetGroup.Cell);

        float travelFactor =
            cellAltitudeDeltaFactor * cellAltitudeDeltaFactor *
            cellSurvivability * cellSurvivability * targetGroup.Cell.Accessibility;

        travelFactor = Mathf.Clamp(travelFactor, 0.0001f, 1);

        int travelTime = (int)Mathf.Ceil(World.YearLength * Cell.Width / (TravelWidthFactor * travelFactor));

        long nextDate = World.CurrentDate + travelTime;

        if (nextDate <= World.CurrentDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderPolityProminenceExpansion - nextDate (" + nextDate +
                ") less or equal to World.CurrentDate (" + World.CurrentDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width +
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }
        else if (nextDate > World.MaxSupportedDate)
        {
            // targetDate is invalid, generate report
            Debug.LogWarning("CellGroup.ConsiderPolityProminenceExpansion - nextDate (" + nextDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). travelTime: " + travelTime + ", Cell.Width: " + Cell.Width +
                ", TravelWidthFactor: " + TravelWidthFactor + ", travelFactor: " + travelFactor);

            // Do not generate event
            return;
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Id == Manager.TracingData.GroupId)
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //                string targetGroupId = "Id:" + targetGroup.Id + "|Long:" + targetGroup.Longitude + "|Lat:" + targetGroup.Latitude;

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ConsiderPolityProminenceExpansion Part 4 - Group:" + groupId,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", Neighbors.Count: " + Neighbors.Count +
        //                    ", groupValue: " + groupValue +
        //                    ", PolityExpansionValue: " + PolityExpansionValue +
        //                    ", TotalPolityExpansionValue: " + TotalPolityExpansionValue +
        //                    ", rollValue: " + rollValue +
        //                    ", travelFactor: " + travelFactor +
        //                    ", nextDate: " + nextDate +
        //                    ", selectedPi.PolityId: " + selectedPi.PolityId +
        //                    ", targetGroup: " + targetGroupId +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        if (PolityExpansionEvent == null)
        {
            PolityExpansionEvent = new ExpandPolityProminenceEvent(this, selectedPi.Polity, targetGroup, nextDate);
        }
        else
        {
            PolityExpansionEvent.Reset(selectedPi.Polity, targetGroup, nextDate);
        }

        World.InsertEventToHappen(PolityExpansionEvent);

        HasPolityExpansionEvent = true;

        PolityExpansionEventDate = nextDate;
        ExpandingPolityId = selectedPi.PolityId;
        ExpansionTargetGroupId = targetGroup.Id;
    }

    public void Destroy()
    {
        StillPresent = false;

        foreach (Faction faction in GetFactionCores())
        {
            //#if DEBUG
            //            Debug.Log("Faction will be removed due to core group dissapearing. faction id: " + faction.Id + ", polity id:" + faction.Polity.Id + ", group id:" + Id + ", date:" + World.CurrentDate);
            //#endif

            World.AddFactionToRemove(faction);
        }

        RemovePolityProminences();

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

    public void RemovePolityProminences() // This should be called only when destroying a group
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
            Debug.LogWarning("Group is no longer present. Id: " + Id);
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

        Profiler.BeginSample("Update Population");

        UpdatePopulation(timeSpan);

        Profiler.EndSample();

        Profiler.BeginSample("Update Culture");
        
        Culture.Update(timeSpan);

        Profiler.EndSample();

        Profiler.BeginSample("Update Polity Cultural Prominences");

        UpdatePolityCulturalProminences(timeSpan);

        Profiler.EndSample();

        Profiler.BeginSample("Polity Update Effects");

        PolityUpdateEffects(timeSpan);

        Profiler.EndSample();

        Profiler.BeginSample("Update Travel Factors");

        UpdateSeaTravelFactor();

        Profiler.EndSample();

        Profiler.BeginSample("Update Shortest Polity Core Distances");

        UpdateShortestPolityCoreDistances();

        Profiler.EndSample();

        Profiler.BeginSample("Update Shortest Faction Core Distances");

        UpdateShortestFactionCoreDistances();

        Profiler.EndSample();

        Profiler.BeginSample("Update Add Updated Group");

        World.AddUpdatedGroup(this);

        Profiler.EndSample();
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
                    "SetPolityUpdate - Group:" + Id,
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

        float rollValue = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_SET_POLITY_UPDATE + unchecked((int)p.Id), registerForTesting: false);

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
            if (_polityProminencesToRemove.Contains(polityProminence.PolityId))
                continue;

            Polity polity = polityProminence.Polity;
            float prominenceValue = polityProminence.NewValue;

            polity.GroupUpdateEffects(this, prominenceValue, TotalPolityProminenceValue, timeSpan);
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

    private float GetActivityContribution(string activityId)
    {
        CellCulturalActivity activity = Culture.GetActivity(activityId) as CellCulturalActivity;

        if (activity == null)
            return 0;

        return activity.Contribution;
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
        float knowledgeValue = 0;
        AgricultureKnowledge knowledge = Culture.GetKnowledge(AgricultureKnowledge.KnowledgeId) as AgricultureKnowledge;

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

        knowledgeValue = knowledge.ScaledValue;

        float techValue = Mathf.Sqrt(knowledgeValue);

        float areaPerFarmWorker = techValue / 5f;

        float terrainFactor = knowledge.TerrainFactor;

        float farmingPopulation = GetActivityContribution(CellCulturalActivity.FarmingActivityId) * Population;

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

    public int CalculateOptimalPopulation(TerrainCell cell)
    {
        int optimalPopulation = 0;

        float foragingCapacity = 0;
        float survivability = 0;

        float foragingContribution = GetActivityContribution(CellCulturalActivity.ForagingActivityId);

        CalculateAdaptionToCell(cell, out foragingCapacity, out survivability);

        float populationCapacityByForaging = foragingContribution * PopulationForagingConstant * cell.Area * foragingCapacity;

        float farmingContribution = GetActivityContribution(CellCulturalActivity.FarmingActivityId);
        float populationCapacityByFarming = 0;

        if (farmingContribution > 0)
        {
            float farmingCapacity = CalculateFarmingCapacity(cell);

            populationCapacityByFarming = farmingContribution * PopulationFarmingConstant * cell.Area * farmingCapacity;
        }

        float fishingContribution = GetActivityContribution(CellCulturalActivity.FishingActivityId);
        float populationCapacityByFishing = 0;

        if (fishingContribution > 0)
        {
            float fishingCapacity = CalculateFishingCapacity(cell);

            populationCapacityByFishing = fishingContribution * PopulationFishingConstant * cell.Area * fishingCapacity;
        }

        float accesibilityFactor = 0.25f + 0.75f * cell.Accessibility;

        float populationCapacity = (populationCapacityByForaging + populationCapacityByFarming + populationCapacityByFishing) * survivability * accesibilityFactor;

        optimalPopulation = (int)Mathf.Floor(populationCapacity);

#if DEBUG
        if (optimalPopulation < -1000)
        {
            Debug.Break();
            throw new System.Exception("Debug.Break");
        }
#endif

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (Id == Manager.TracingData.GroupId) {
        //				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
        //					string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //					string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
        //
        //					SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //						"CalculateOptimalPopulation - Group:" + groupId,
        //						"CurrentDate: " + World.CurrentDate + 
        //						", target cellInfo: " + cellInfo + 
        //						", foragingContribution: " + foragingContribution + 
        ////						", Area: " + cell.Area + 
        //						", modifiedForagingCapacity: " + modifiedForagingCapacity + 
        //						", modifiedSurvivability: " + modifiedSurvivability + 
        //						", accesibilityFactor: " + accesibilityFactor + 
        //						", optimalPopulation: " + optimalPopulation + 
        //						"");
        //
        //					Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //				}
        //			}
        //		}
        //		#endif

        return optimalPopulation;
    }

    public float CalculateFarmingCapacity(TerrainCell cell)
    {
        float capacityFactor = 0;

        float value = 0;

        if (!Culture.TryGetKnowledgeScaledValue(AgricultureKnowledge.KnowledgeId, out value))
        {
            return capacityFactor;
        }

        float techFactor = value;

        capacityFactor = cell.FarmlandPercentage * techFactor;

        return capacityFactor;
    }

    public float CalculateFishingCapacity(TerrainCell cell)
    {
        float noTechBaseValue = 0.5f;

        float value = 0;

        Culture.TryGetKnowledgeScaledValue(ShipbuildingKnowledge.KnowledgeId, out value);
        
        float techFactor = value + noTechBaseValue;

        float capacityFactor = techFactor * cell.NeighborhoodWaterBiomePresence;

        return capacityFactor;
    }

    public void CalculateAdaptionToCell(TerrainCell cell, out float foragingCapacity, out float survivability)
    {
        float modifiedForagingCapacity = 0;
        float modifiedSurvivability = 0;

        //		#if DEBUG
        //		string biomeData = "";
        //		#endif

        //		Profiler.BeginSample ("Get Group Skill Values");

        foreach (string biomeId in cell.PresentBiomeIds)
        {
            //			Profiler.BeginSample ("Try Get Group Biome Survival Skill");

            float biomeRelPresence = cell.GetBiomePresence(biomeId);

            BiomeSurvivalSkill skill = null;

            Biome biome = Biome.Biomes[biomeId];

            if (_biomeSurvivalSkills.TryGetValue(biomeId, out skill))
            {
                //				Profiler.BeginSample ("Evaluate Group Biome Survival Skill");

                modifiedForagingCapacity += biomeRelPresence * biome.ForagingCapacity * skill.Value;
                modifiedSurvivability += biomeRelPresence * (biome.Survivability + skill.Value * (1 - biome.Survivability));

                //				#if DEBUG
                //
                //				if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
                //					biomeData += "\n\tBiome: " + biomeName + 
                //						" ForagingCapacity: " + biome.ForagingCapacity + 
                //						" skillValue: " + skillValue + 
                //						" biomePresence: " + biomePresence;
                //				}
                //
                //				#endif

                //				Profiler.EndSample ();
            }
            else
            {
                modifiedSurvivability += biomeRelPresence * biome.Survivability;
            }

            //			Profiler.EndSample ();
        }

        //		Profiler.EndSample ();

        float altitudeSurvivabilityFactor = 1 - Mathf.Clamp01(cell.Altitude / World.MaxPossibleAltitude);

        modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

        foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
        survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

        if (foragingCapacity > 1)
        {
            throw new System.Exception("ForagingCapacity greater than 1: " + foragingCapacity);
        }

        if (survivability > 1)
        {
            throw new System.Exception("Survivability greater than 1: " + survivability);
        }

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (Id == Manager.TracingData.GroupId) {
        //				if ((cell.Longitude == Longitude) && (cell.Latitude == Latitude)) {
        //					System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        //
        //					System.Reflection.MethodBase method = stackTrace.GetFrame(2).GetMethod();
        //					string callingMethod = method.Name;
        //
        ////					if (callingMethod.Contains ("CalculateMigrationValue")) {
        //						string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;
        //						string cellInfo = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
        //
        //						SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //							"CalculateAdaptionToCell - Group:" + groupId,
        //							"CurrentDate: " + World.CurrentDate + 
        //							", callingMethod(2): " + callingMethod + 
        //							", target cell: " + cellInfo + 
        //							", cell.FarmlandPercentage: " + cell.FarmlandPercentage + 
        //							", foragingCapacity: " + foragingCapacity + 
        //							", survivability: " + survivability + 
        //							", biomeData: " + biomeData + 
        //							"");
        //
        //						Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        ////					}
        //				}
        //			}
        //		}
        //		#endif
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
                    Debug.LogError("Group identifies as faction core when it no longer is. Id: " + Id + ", CoreId: " + faction.CoreGroupId + ", current date: " + World.CurrentDate);
                }
            }
        }
#endif

        float randomFactor = Cell.GetNextLocalRandomFloat(RngOffsets.CELL_GROUP_CALCULATE_NEXT_UPDATE);
        randomFactor = 1f - Mathf.Pow(randomFactor, 4);

        float migrationFactor = 1;

        if (TotalMigrationValue > 0)
        {
            migrationFactor = MigrationValue / TotalMigrationValue;
            migrationFactor = Mathf.Pow(migrationFactor, 4);
        }

        float polityExpansionFactor = 1;

        if (TotalPolityExpansionValue > 0)
        {
            polityExpansionFactor = PolityExpansionValue / TotalPolityExpansionValue;
            polityExpansionFactor = Mathf.Pow(polityExpansionFactor, 4);
        }

        float skillLevelFactor = Culture.MinimumSkillAdaptationLevel();
        float knowledgeLevelFactor = Culture.MinimumKnowledgeProgressLevel();

        float populationFactor = 0.0001f + Mathf.Abs(OptimalPopulation - Population);
        populationFactor = 100 * OptimalPopulation / populationFactor;
        
        populationFactor = Mathf.Min(populationFactor, MaxUpdateSpanFactor);

        float mixFactor = randomFactor * migrationFactor
            * polityExpansionFactor * skillLevelFactor
            * knowledgeLevelFactor * populationFactor;

        long updateSpan = GenerationSpan * (int)mixFactor;

        if (updateSpan < 0)
            updateSpan = MaxUpdateSpan;

        updateSpan = (updateSpan < GenerationSpan) ? GenerationSpan : updateSpan;
        updateSpan = (updateSpan > MaxUpdateSpan) ? MaxUpdateSpan : updateSpan;

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Id == Manager.TracingData.GroupId)
            {
                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "CalculateNextUpdateDate - Group:" + groupId,
                    "CurrentDate: " + World.CurrentDate +
                    ", MigrationValue: " + MigrationValue +
                    ", TotalMigrationValue: " + TotalMigrationValue +
                    ", OptimalPopulation: " + OptimalPopulation +
                    ", ExactPopulation: " + ExactPopulation +
                    ", randomFactor: " + randomFactor +
                    ", migrationFactor: " + migrationFactor +
                    ", polityExpansionFactor: " + polityExpansionFactor +
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
        {
            return float.MaxValue;
        }

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

    private float CalculateShortestFactionCoreDistance(Polity polity)
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

    private float CalculateShortestPolityCoreDistance(Polity polity)
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

    private void UpdateShortestFactionCoreDistances()
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            pi.NewFactionCoreDistance = CalculateShortestFactionCoreDistance(pi.Polity);
        }
    }

    private void UpdateShortestPolityCoreDistances()
    {
        foreach (PolityProminence pi in _polityProminences.Values)
        {
            pi.NewPolityCoreDistance = CalculateShortestPolityCoreDistance(pi.Polity);
        }
    }

    private float CalculateAdministrativeCost(PolityProminence pi)
    {
        float polityPopulation = Population * pi.Value;

        float distanceFactor = 500 + pi.FactionCoreDistance;

        float cost = polityPopulation * distanceFactor * 0.001f;

        if (cost < 0)
            return float.MaxValue;

        return cost;
    }

    private void UpdatePolityProminenceAdministrativeCosts()
    {

        foreach (PolityProminence pi in _polityProminences.Values)
        {

            pi.AdministrativeCost = CalculateAdministrativeCost(pi);
        }
    }

    public void PostUpdatePolityProminences_BeforePolityUpdates()
    {
        TotalPolityProminenceValue = 0;

        foreach (long polityId in _polityProminencesToRemove)
        {
            PolityProminence polityProminence;

            if (!_polityProminences.TryGetValue(polityId, out polityProminence))
            {
                if (!_polityProminencesToAdd.TryGetValue(polityId, out polityProminence))
                {
                    Debug.LogWarning("Trying to remove nonexisting PolityProminence with id: " + polityId + " from group with id: " + Id);
                }

                _polityProminencesToAdd.Remove(polityProminence.PolityId);
            }
            else
            {
                Profiler.BeginSample("Remove Polity Prominence");

                _polityProminences.Remove(polityProminence.PolityId);

                Profiler.EndSample();

                Profiler.BeginSample("Decrease Polity Contacts");

                // Decreate polity contacts
                foreach (PolityProminence epi in _polityProminences.Values)
                {
                    Polity.DecreaseContactGroupCount(polityProminence.Polity, epi.Polity);
                }

                Profiler.EndSample();

                Profiler.BeginSample("Remove Faction Cores");

                // Remove all polity faction cores from group
                foreach (Faction faction in GetFactionCores())
                {
                    if (faction.PolityId == polityProminence.PolityId)
                    {
                        //#if DEBUG
                        //                        Debug.Log("Faction will be removed due to total loss of polity prominence. faction id: " + faction.Id + ", polity id:" + faction.Polity.Id + ", group id:" + Id + ", date:" + World.CurrentDate);
                        //#endif

                        World.AddFactionToRemove(faction);
                    }
                }

                Profiler.EndSample();

                //#if DEBUG
                //                if (this == polityProminence.Polity.CoreGroup)
                //                {
                //                    Debug.LogWarning("Polity has lost it's core group. Group Id: " + Id + ", Polity Id: " + polityProminence.Polity.Id);
                //                }
                //#endif

                Profiler.BeginSample("Remove Group from Polity");

                polityProminence.Polity.RemoveGroup(polityProminence);

                Profiler.EndSample();

                Profiler.BeginSample("Set Polity Update");

                // We want to update the polity if a group is removed.
                SetPolityUpdate(polityProminence, true);

                Profiler.EndSample();
            }
        }

        _polityProminencesToRemove.Clear();

        foreach (PolityProminence prominenceToAdd in _polityProminencesToAdd.Values)
        {
            Profiler.BeginSample("Increase Polity Contacs");

            // Increase polity contacts
            foreach (PolityProminence otherProminence in _polityProminences.Values)
            {
                Polity.IncreaseContactGroupCount(prominenceToAdd.Polity, otherProminence.Polity);
            }

            Profiler.EndSample();

            Profiler.BeginSample("Add Polity Influence");

            _polityProminences.Add(prominenceToAdd.PolityId, prominenceToAdd);

            Profiler.EndSample();

            Profiler.BeginSample("Set Polity Update");

            // We want to update the polity if a group is added.
            SetPolityUpdate(prominenceToAdd, true);

            Profiler.EndSample();

            Profiler.BeginSample("Add Group to Polity");

            prominenceToAdd.Polity.AddGroup(prominenceToAdd);

            Profiler.EndSample();
        }

        _polityProminencesToAdd.Clear();

        foreach (PolityProminence prominence in _polityProminences.Values)
        {
            Profiler.BeginSample("Polity Influence Postupdate");

            prominence.PostUpdate();

            Profiler.EndSample();

            TotalPolityProminenceValue += prominence.Value;
        }

#if DEBUG
        if (TotalPolityProminenceValue > 1.0)
        {
            Debug.LogWarning("Total Polity Prominence Value greater than 1: " + TotalPolityProminenceValue + ", Group Id: " + Id);
        }
#endif

#if DEBUG
        if (TotalPolityProminenceValue <= 0)
        {
            if (GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group with no polity prominence has faction cores. Id: " + Id);
            }
        }
#endif

        Profiler.BeginSample("Find Highest Polity Prominence");

        FindHighestPolityProminence();

        Profiler.EndSample();
    }

    public void PostUpdatePolityProminences_AfterPolityUpdates()
    {
        TotalPolityProminenceValue = 0;

        foreach (long polityId in _polityProminencesToRemove)
        {
            PolityProminence pi;

            if (!_polityProminences.TryGetValue(polityId, out pi))
            {
                if (!_polityProminencesToAdd.TryGetValue(polityId, out pi))
                {
                    Debug.LogWarning("Trying to remove nonexisting PolityProminence with id: " + polityId + " from group with id: " + Id);
                }
            }
            else
            {
                _polityProminences.Remove(pi.PolityId);

                if (pi.Polity.StillPresent)
                {
                    // Decrease polity contacts
                    foreach (PolityProminence epi in _polityProminences.Values)
                    {
                        Polity.DecreaseContactGroupCount(pi.Polity, epi.Polity);
                    }
                }
            }
        }

        _polityProminencesToRemove.Clear();

        foreach (PolityProminence pi in _polityProminences.Values)
        {
            TotalPolityProminenceValue += pi.Value;
        }

#if DEBUG
        if (TotalPolityProminenceValue > 1.0)
        {
            Debug.LogWarning("Total Polity Prominence Value greater than 1: " + TotalPolityProminenceValue + ", Group Id: " + Id);
        }
#endif

#if DEBUG
        if (TotalPolityProminenceValue <= 0)
        {
            if (GetFactionCores().Count > 0)
            {
                Debug.LogWarning("Group with no polity prominence has faction cores. Id: " + Id);
            }
        }
#endif

        FindHighestPolityProminence();
    }

    public PolityProminence SetPolityProminence(Polity polity, float newProminenceValue, float polityCoreDistance = -1, float factionCoreDistance = -1)
    {
        newProminenceValue = MathUtility.RoundToSixDecimals(newProminenceValue);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((Id == Manager.TracingData.GroupId) || (polity.Id == Manager.TracingData.PolityId))
        //            {
        //                string groupId = "Id:" + Id + "|Long:" + Longitude + "|Lat:" + Latitude;

        //                System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

        //                System.Reflection.MethodBase method = stackTrace.GetFrame(1).GetMethod();
        //                string callingMethod = method.Name;

        //                string callingClass = method.DeclaringType.ToString();

        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "SetPolityProminenceValue - Group:" + groupId +
        //                    ", polity.Id: " + polity.Id,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", newProminenceValue: " + newProminenceValue +
        //                    ", caller: " + callingClass + ":" + callingMethod +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif
        
        _polityProminencesToRemove.Remove(polity.Id);

        if (!_polityProminences.TryGetValue(polity.Id, out PolityProminence polityProminence))
        {
            _polityProminencesToAdd.TryGetValue(polity.Id, out polityProminence);
        }

        if (polityProminence == null)
        {
            if (newProminenceValue > Polity.MinPolityProminence)
            {
                polityProminence = new PolityProminence(this, polity, newProminenceValue);

                if (polityCoreDistance == -1)
                    polityCoreDistance = CalculateShortestPolityCoreDistance(polity);

                if (factionCoreDistance == -1)
                    factionCoreDistance = CalculateShortestFactionCoreDistance(polity);

                polityProminence.PolityCoreDistance = polityCoreDistance;
                polityProminence.NewPolityCoreDistance = polityCoreDistance;

                polityProminence.FactionCoreDistance = factionCoreDistance;
                polityProminence.NewFactionCoreDistance = factionCoreDistance;

                _polityProminencesToAdd.Add(polity.Id, polityProminence);
            }

            return polityProminence;
        }

        if (newProminenceValue <= Polity.MinPolityProminence)
        {
            //#if DEBUG
            //            foreach (Faction faction in GetFactionCores())
            //            {
            //                if (faction.PolityId == polityProminence.PolityId)
            //                {
            //                    Debug.LogWarning("Faction belonging to polity to remove has core in cell - group Id: " + Id + " - polity Id: " + polityProminence.PolityId);
            //                }
            //            }
            //#endif

            if (WillBecomeFactionCore)
            {
                throw new System.Exception("Group is set to become a faction core - group Id: " + Id + " - polity Id: " + polityProminence.PolityId + ", Date:" + World.CurrentDate);
            }

            _polityProminencesToRemove.Add(polityProminence.PolityId);

            return null;
        }

        if (polityCoreDistance == -1)
            polityCoreDistance = CalculateShortestPolityCoreDistance(polity);

        if (factionCoreDistance == -1)
            factionCoreDistance = CalculateShortestFactionCoreDistance(polity);

        polityProminence.NewValue = newProminenceValue;

        polityProminence.NewPolityCoreDistance = polityCoreDistance;
        polityProminence.NewFactionCoreDistance = factionCoreDistance;

        return polityProminence;
    }

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

#if DEBUG
        if (highestProminence == null)
        {
            foreach (PolityProminence pi in _polityProminences.Values)
            {
                Debug.LogWarning("pi.Id: " + pi.Id + ", pi.PolityId: " + pi.PolityId + ", pi.Value: " + pi.Value);
            }
        }
#endif

        //Profiler.BeginSample("Set Highest Polity Prominence");

        SetHighestPolityProminence(highestProminence);

        //Profiler.EndSample();
    }

    public void RemovePolityProminence(Polity polity)
    {
        PolityProminence pi = null;

        if (!_polityProminences.TryGetValue(polity.Id, out pi))
        {
            throw new System.Exception("Polity not actually influencing group");
        }

        //		#if DEBUG
        //		foreach (Faction faction in GetFactionCores ()) {
        //
        //			if (faction.PolityId == polity.Id) {
        //
        //				Debug.LogWarning ("Faction belonging to polity to remove has core in cell - group Id: " + Id + " - polity Id: " + polity.Id);
        //			}
        //		}
        //		#endif

        _polityProminencesToRemove.Add(polity.Id);
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

        if (HasPolityExpansionEvent && !PolityExpansionEvent.IsStillValid())
        {
            HasPolityExpansionEvent = false;
        }

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

        FactionCoreIds = new List<long>(FactionCores.Keys);

        PreferredMigrationDirectionInt = (int)PreferredMigrationDirection;

        base.Synchronize();
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
        
        PreferredMigrationDirection = (Direction)PreferredMigrationDirectionInt;

        foreach (long id in FactionCoreIds)
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
            p.NewValue = p.Value;

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
            TerrainCell targetCell = World.GetCell(MigrationTargetLongitude, MigrationTargetLatitude);

            MigrationEvent = new MigrateGroupEvent(
                this,
                targetCell,
                (Direction)MigrationEventDirectionInt,
                (MigrationType)MigrationEventTypeInt,
                MigrationEventDate);
            World.InsertEventToHappen(MigrationEvent);
        }

        // Generate Polity Expansion Event

        if (HasPolityExpansionEvent)
        {
            Polity expandingPolity = World.GetPolity(ExpandingPolityId);

            if (expandingPolity == null)
            {
                throw new System.Exception("Missing polity with id:" + ExpandingPolityId);
            }

            CellGroup targetGroup = World.GetGroup(ExpansionTargetGroupId);

            PolityExpansionEvent = new ExpandPolityProminenceEvent(this, expandingPolity, targetGroup, PolityExpansionEventDate);
            World.InsertEventToHappen(PolityExpansionEvent);
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

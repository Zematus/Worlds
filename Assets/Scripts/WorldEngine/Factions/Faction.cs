﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;
using System;

[XmlInclude(typeof(Clan))]
public abstract class Faction : ISynchronizable, IWorldDateGetter, IFlagHolder
{
    public static List<IFactionEventGenerator> OnSpawnEventGenerators;
    public static List<IFactionEventGenerator> OnStatusChangeEventGenerators;

    [XmlAttribute("Inf")]
    public float InfluenceInternal;

    [XmlIgnore]
    public float Influence
    {
        get
        {
            return InfluenceInternal;
        }
        set
        {
            if (value < 0)
            {
                throw new System.Exception("Influence set to less than zero: " + value);
            }

            if (!Polity.IsBeingUpdated)
            {
                World.AddPolityToUpdate(Polity);
            }

            InfluenceInternal = value;
        }
    }

    [XmlAttribute("StilPres")]
    public bool StillPresent = true;

    [XmlAttribute("IsDom")]
    public bool IsDominant = false;

    [XmlAttribute("LastUpDate")]
    public long LastUpdateDate;

    [XmlAttribute("LeadStDate")]
    public long LeaderStartDate;

    [XmlAttribute("IsCon")]
    public bool IsUnderPlayerGuidance = false;

    public Identifier PolityId;
    public Identifier CoreGroupId;

    [XmlIgnore]
    public bool HasBeenUpdated = false;

    public List<string> Flags;

    public FactionCulture Culture;

    public List<FactionRelationship> Relationships = new List<FactionRelationship>();

    public List<FactionEventData> EventDataList = new List<FactionEventData>();

    // Do not call this property directly, only for serialization
    public Agent LastLeader = null;

    [XmlIgnore]
    public FactionInfo Info;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Polity Polity;

    [XmlIgnore]
    public CellGroup CoreGroup;

    [XmlIgnore]
    public CellGroup NewCoreGroup = null;

    [XmlIgnore]
    public bool IsInitialized = false;

    [XmlIgnore]
    public float AdministrativeLoad => _administrativeLoad.Value;

    // Use this instead to get the leader
    [XmlIgnore]
    public Agent CurrentLeader => _currentLeader.Value;

    [XmlIgnore]
    public bool BeingRemoved = false;

    public string Type => Info.Type;

    public Identifier Id => Info.Id;

    public long FormationDate => Info.FormationDate;

    public Name Name => Info.Name;

    public long CurrentDate => World.CurrentDate;

    [Obsolete]
    protected long _splitFactionEventId;
    [Obsolete]
    protected CellGroup _splitFactionCoreGroup;
    [Obsolete]
    protected float _splitFactionMinInfluence;
    [Obsolete]
    protected float _splitFactionMaxInfluence;

    protected Dictionary<Identifier, FactionRelationship> _relationships =
        new Dictionary<Identifier, FactionRelationship>();

    protected Dictionary<long, FactionEvent> _events =
        new Dictionary<long, FactionEvent>();

    private DatedValue<float> _administrativeLoad;
    private DatedValue<Agent> _currentLeader;

    private HashSet<string> _flags = new HashSet<string>();

    private bool _preupdated = false;

    public Faction()
    {
    }

    public Faction(
        string type,
        Polity polity,
        CellGroup coreGroup,
        float influence,
        Faction parentFaction = null)
    {
        World = polity.World;

        LastUpdateDate = World.CurrentDate;

        long idOffset = 0;

        if (parentFaction != null)
        {
            idOffset = parentFaction.GetHashCode();
        }

        PolityId = polity.Id;
        Polity = polity;

        CoreGroup = coreGroup;
        CoreGroupId = coreGroup.Id;

        long initId = GenerateInitId(idOffset);

        Info = new FactionInfo(this, type, World.CurrentDate, initId);

        Culture = new FactionCulture(this);

        CoreGroup.AddFactionCore(this);

        Influence = influence;

        GenerateName(parentFaction);

        if (parentFaction != null)
        {
            PolityProminence polityProminence = CoreGroup.GetPolityProminence(PolityId);
            World.AddPromToCalculateCoreDistFor(polityProminence);
        }
    }

    public void Initialize()
    {
        _administrativeLoad = new DatedValue<float>(World, CalculateAdministrativeLoad);
        _currentLeader = new DatedValue<Agent>(World, RequestCurrentLeader);

        InitializeInternal();

        InitializeDefaultEvents();

        IsInitialized = true;
    }

    protected virtual void InitializeInternal()
    {

    }

    protected abstract float CalculateAdministrativeLoad();

    public override int GetHashCode()
    {
        return Info.GetHashCode();
    }

    public virtual string GetName()
    {
        return Info.Name.Text;
    }

    public virtual string GetNameBold()
    {
        return Info.Name.BoldText;
    }

    public string GetNameAndTypeString()
    {
        return Info.GetNameAndTypeString();
    }

    public string GetNameAndTypeStringBold()
    {
        return Info.GetNameAndTypeStringBold();
    }

    public string GetNameAndTypeWithPolityString()
    {
        return GetNameAndTypeString() + " of " + Polity.GetNameAndTypeString();
    }

    public string GetNameAndTypeWithPolityStringBold()
    {
        return GetNameAndTypeStringBold() + " of " + Polity.GetNameAndTypeStringBold();
    }

    public void Destroy(bool polityBeingDestroyed = false)
    {
        if (IsUnderPlayerGuidance)
        {
            Manager.SetGuidedFaction(null);
        }

        CoreGroup.RemoveFactionCore(this);

        if (!polityBeingDestroyed)
        {
            Polity.RemoveFaction(this);
        }

        List<FactionRelationship> relationshipsToRemove =
            new List<FactionRelationship>(_relationships.Values);
        foreach (FactionRelationship relationship in relationshipsToRemove)
        {
            relationship.Faction.RemoveRelationship(this);
        }

        Info.Faction = null;

        StillPresent = false;
    }

    /// <summary>
    /// Sets this faction to be removed from the world
    /// </summary>
    public void SetToRemove()
    {
        World.AddFactionToRemove(this);

        BeingRemoved = true;
    }

    public void SetToUpdate()
    {
        World.AddGroupToUpdate(CoreGroup);
        World.AddFactionToUpdate(this);
        World.AddPolityToUpdate(Polity);
    }

    public static void SetRelationship(Faction factionA, Faction factionB, float value)
    {
        factionA.SetRelationship(factionB, value);
        factionB.SetRelationship(factionA, value);
    }

    public void SetRelationship(Faction faction, float value)
    {
        value = Mathf.Clamp01(value);

        if (!_relationships.ContainsKey(faction.Id))
        {
            FactionRelationship relationship = new FactionRelationship(faction, value);

            _relationships.Add(faction.Id, relationship);
            Relationships.Add(relationship);

        }
        else
        {
            _relationships[faction.Id].Value = value;
        }
    }

    public void RemoveRelationship(Faction faction)
    {
        if (!_relationships.ContainsKey(faction.Id))
            throw new System.Exception("(id: " + Id + ") relationship not present: " + faction.Id);

        FactionRelationship relationship = _relationships[faction.Id];

        Relationships.Remove(relationship);
        _relationships.Remove(faction.Id);
    }

    public float GetRelationshipValue(Faction faction)
    {
        // Set a default neutral relationship
        if (!_relationships.ContainsKey(faction.Id))
        {
            Faction.SetRelationship(this, faction, 0.5f);
        }

        return _relationships[faction.Id].Value;
    }

    public bool HasRelationship(Faction faction)
    {
        return _relationships.ContainsKey(faction.Id);
    }

    protected abstract void GenerateName(Faction parentFaction);

    protected Agent RequestCurrentLeader(int leadershipSpan, int minStartAge, int maxStartAge, int offset)
    {
        long spawnDate = CoreGroup.GeneratePastSpawnDate(CoreGroup.LastUpdateDate, leadershipSpan, offset++);

        if ((LastLeader != null) && (spawnDate < LeaderStartDate))
        {
            return LastLeader;
        }

        // Generate a birthdate from the leader spawnDate (when the leader takes over)
        int startAge = minStartAge + CoreGroup.GetLocalRandomInt(spawnDate, offset++, maxStartAge - minStartAge);

        LastLeader = new Agent(CoreGroup, spawnDate - startAge, GetHashCode());
        LeaderStartDate = spawnDate;

        return LastLeader;
    }

    protected Agent RequestNewLeader(int leadershipSpan, int minStartAge, int maxStartAge, int offset)
    {
        long spawnDate = CoreGroup.GeneratePastSpawnDate(CoreGroup.LastUpdateDate, leadershipSpan, offset++);

        // Generate a birthdate from the leader spawnDate (when the leader takes over)
        int startAge = minStartAge + CoreGroup.GetLocalRandomInt(spawnDate, offset++, maxStartAge - minStartAge);

        LastLeader = new Agent(CoreGroup, spawnDate - startAge, GetHashCode());
        LeaderStartDate = spawnDate;

        return LastLeader;
    }

    protected abstract Agent RequestCurrentLeader();
    protected abstract Agent RequestNewLeader();

    public static Faction CreateFaction(
        string type,
        Polity polity,
        CellGroup coreGroup,
        float influence,
        Faction parentFaction = null)
    {

#if DEBUG //TODO: Make sure we don't need to do this for unit tests
        if (parentFaction is TestFaction)
        {
            TestFaction testFaction =
                new TestFaction(
                    "clan",
                    polity,
                    coreGroup,
                    influence,
                    parentFaction,
                    parentFaction.AdministrativeLoad);

            testFaction.Culture.GetPreference("authority").Value =
                parentFaction.Culture.GetPreference("authority").Value;
            testFaction.Culture.GetPreference("cohesion").Value =
                parentFaction.Culture.GetPreference("cohesion").Value;

            return testFaction;
        }
#endif

        switch (type)
        {
            case Clan.FactionType:
                return new Clan(polity, coreGroup, influence, parentFaction);
            default:
                throw new Exception("Unhandled faction type: " + type);
        }
    }

    public void Split(
        string factionType,
        CellGroup newFactionCoreGroup,
        float influenceToTransfer,
        float initialRelationshipValue)
    {
        Influence -= influenceToTransfer;

        if (newFactionCoreGroup == null)
        {
            throw new Exception("_splitFactionCoreGroup is null - Faction Id: " + Id);
        }

        float polityProminenceValue = newFactionCoreGroup.GetPolityProminenceValue(Polity);
        PolityProminence highestPolityProminence = newFactionCoreGroup.HighestPolityProminence;

        if (highestPolityProminence == null)
        {
            throw new Exception(
                "highestPolityProminence is null - Faction Id: " + Id +
                ", Group Id: " + newFactionCoreGroup);
        }

        if (CurrentLeader == null)
        {
            throw new Exception("CurrentLeader is null - Faction Id: " + Id);
        }

        Polity newPolity = Polity;

        if (newPolity == null)
        {
            throw new Exception("newPolity is null - Faction Id: " + Id);
        }

        // If the polity with the highest prominence is different than the source faction's polity and it's value is twice greater switch the new clan's polity to this one.
        // NOTE: This is sort of a hack to avoid issues with faction/polity split coincidences (issue #8 github). Try finding a better solution...
        if (highestPolityProminence.Value > (polityProminenceValue * 2))
        {
            newPolity = highestPolityProminence.Polity;
        }

        Faction newFaction =
            CreateFaction(factionType, newPolity, newFactionCoreGroup, influenceToTransfer, this);

        if (newFaction == null)
        {
            throw new Exception("newFaction is null - Faction Id: " + Id);
        }

#if DEBUG
        Faction existingFaction = World.GetFaction(newFaction.Id);

        if (existingFaction != null)
        {
            throw new Exception("faction Id already exists - new faction Id: " + newFaction.Id);
        }
#endif

        newFaction.Initialize(); // We can initialize right away since the containing polity is already initialized

        // set relationship within parent and child faction
        SetRelationship(this, newFaction, initialRelationshipValue);

        newPolity.AddFaction(newFaction);

        World.AddFactionToUpdate(this);
        World.AddFactionToUpdate(newFaction);

        World.AddPolityToUpdate(newPolity);
        World.AddPolityToUpdate(Polity);

        newFactionCoreGroup.SetToUpdate();
        newFactionCoreGroup.SetToBecomeFactionCore(newFaction);

        newPolity.AddEventMessage(new FactionSplitEventMessage(this, newFaction, World.CurrentDate));
    }

    public virtual void HandleUpdateEvent()
    {

    }

    /// <summary>
    /// Tries to update all the faction properties before being accessed by other entities
    /// </summary>
    public void PreUpdate()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (_preupdated)
        {
            return;
        }

        if (!StillPresent)
        {
            throw new System.Exception(
                "Faction is no longer present. Id: " + Id + ", Date: " + World.CurrentDate);
        }

        if (!Polity.StillPresent)
        {
            throw new System.Exception(
                "Faction's polity is no longer present. Id: " + Id + " Polity Id: " + Polity.Id + ", Date: " + World.CurrentDate);
        }

        RequestCurrentLeader();

        Culture.Update();

        if (!World.FactionsHaveBeenUpdated)
        {
            World.AddFactionToUpdate(this);
        }

        World.AddFactionToCleanup(this);

        _preupdated = true;
    }

    public void Update()
    {
        if (!StillPresent)
            return;

        HasBeenUpdated = true;

        PreUpdate();

        UpdateInternal();

        LastUpdateDate = World.CurrentDate;

        World.AddPolityToUpdate(Polity);
    }

    /// <summary>
    /// Cleans up all state flags
    /// </summary>
    public void Cleanup()
    {
        _preupdated = false;
        HasBeenUpdated = false;
    }

    public void PrepareNewCoreGroup(CellGroup coreGroup)
    {
        NewCoreGroup = coreGroup;
    }

    public void MigrateToNewCoreGroup()
    {
        CoreGroup.RemoveFactionCore(this);

        CoreGroup = NewCoreGroup;
        CoreGroupId = NewCoreGroup.Id;

        CoreGroup.AddFactionCore(this);

        if (IsDominant)
        {
            Polity.SetCoreGroup(CoreGroup);
        }
    }

    protected abstract void UpdateInternal();

    public virtual void Synchronize()
    {
        Flags = new List<string>(_flags);

        EventDataList.Clear();

        foreach (FactionEvent e in _events.Values)
        {
            EventDataList.Add(e.GetData() as FactionEventData);
        }

        Culture.Synchronize();

        Name.Synchronize();
    }

    public virtual void FinalizeLoad()
    {
        Name.World = World;
        Name.FinalizeLoad();

        foreach (string f in Flags)
        {
            _flags.Add(f);
        }

        CoreGroup = World.GetGroup(CoreGroupId);

        Polity = World.GetPolity(PolityId);

        if (Polity == null)
        {
            throw new System.Exception("Missing Polity with Id " + PolityId);
        }

        Culture.World = World;
        Culture.Faction = this;
        Culture.FinalizeLoad();

        foreach (FactionRelationship relationship in Relationships)
        {
            _relationships.Add(relationship.Id, relationship);
            relationship.Faction = World.GetFaction(relationship.Id);

            if (relationship.Faction == null)
            {
                throw new System.Exception("Faction is null, Id: " + relationship.Id);
            }
        }

        GenerateEventsFromData();
    }

    protected abstract void GenerateEventsFromData();

    public void AddEvent(FactionEvent factionEvent)
    {
        if (_events.ContainsKey(factionEvent.TypeId))
            throw new System.Exception("Event of type " + factionEvent.TypeId + " already present");

        _events.Add(factionEvent.TypeId, factionEvent);
        World.InsertEventToHappen(factionEvent);
    }

    public FactionEvent GetEvent(long typeId)
    {
        if (!_events.ContainsKey(typeId))
            return null;

        return _events[typeId];
    }

    public void ResetEvent(long typeId, long newTriggerDate)
    {
        if (!_events.ContainsKey(typeId))
            throw new System.Exception("Unable to find event of type: " + typeId);

        FactionEvent factionEvent = _events[typeId];

        factionEvent.Reset(newTriggerDate);
        World.InsertEventToHappen(factionEvent);
    }

    public long GenerateInitId(long idOffset = 0L)
    {
        return CoreGroup.GenerateInitId(idOffset);
    }

    public float GetNextLocalRandomFloat(int iterationOffset)
    {
        return CoreGroup.GetNextLocalRandomFloat(iterationOffset + unchecked(GetHashCode()));
    }

    public float GetLocalRandomFloat(int date, int iterationOffset)
    {
        return CoreGroup.GetLocalRandomFloat(date, iterationOffset + unchecked(GetHashCode()));
    }

    public int GetNextLocalRandomInt(int iterationOffset, int maxValue)
    {
        return CoreGroup.GetNextLocalRandomInt(iterationOffset + unchecked(GetHashCode()), maxValue);
    }

    public virtual void SetDominant(bool state)
    {
        IsDominant = state;

        SetStatusChange();
    }

    public void SetUnderPlayerGuidance(bool state)
    {
        IsUnderPlayerGuidance = state;
    }

    public void ChangePolity(Polity targetPolity, float targetInfluence)
    {
        if ((targetPolity == null) || (!targetPolity.StillPresent))
            throw new System.Exception("target Polity is null or not Present");

        Polity.RemoveFaction(this);

        Polity = targetPolity;
        PolityId = Polity.Id;
        Influence = targetInfluence;

        targetPolity.AddFaction(this);
    }

    public virtual bool ShouldMigrateFactionCore(CellGroup sourceGroup, CellGroup targetGroup)
    {
        return false;
    }

    public virtual bool ShouldMigrateFactionCore(CellGroup sourceGroup, TerrainCell targetCell, float targetProminence, int targetPopulation)
    {
        return false;
    }

    public void IncreasePreferenceValue(string id, float percentage)
    {
        CulturalPreference preference = Culture.GetPreference(id);

        if (preference == null)
            throw new System.Exception("preference is null: " + id);

        float value = preference.Value;

        preference.Value = MathUtility.IncreaseByPercent(value, percentage);
    }

    public void DecreasePreferenceValue(string id, float percentage)
    {
        CulturalPreference preference = Culture.GetPreference(id);

        if (preference == null)
            throw new System.Exception("preference is null: " + id);

        float value = preference.Value;

        preference.Value = MathUtility.DecreaseByPercent(value, percentage);
    }

    public float GetPreferenceValue(string id)
    {
        CulturalPreference preference = Culture.GetPreference(id);

        if (preference != null)
            return preference.Value;

        return 0;
    }

    public abstract float GetGroupWeight(CellGroup group);

    public bool GroupCanBeCore(CellGroup group)
    {
        return GetGroupWeight(group) > 0;
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

    public static void ResetEventGenerators()
    {
        OnSpawnEventGenerators = new List<IFactionEventGenerator>();
        OnStatusChangeEventGenerators = new List<IFactionEventGenerator>();
    }

    private void InitializeOnSpawnEvents()
    {
        foreach (IFactionEventGenerator generator in OnSpawnEventGenerators)
        {
            generator.TryGenerateEventAndAssign(this);
        }
    }

    /// <summary>
    /// set that the status of the faction has changed (for example when it becomes dominant)
    /// </summary>
    public void SetStatusChange()
    {
        World.AddFactionWithStatusChange(this);
    }

    /// <summary>
    /// Applies the effects of having the faction status changed
    /// </summary>
    public void ApplyStatusChange()
    {
        foreach (IFactionEventGenerator generator in OnStatusChangeEventGenerators)
        {
            //generator.OpenDebugOutput("Evaluating Assigment on Status Change:");

            generator.TryGenerateEventAndAssign(this);

            //generator.CloseDebugOutput("Finished: Evaluating Assigment on Status Change");
        }
    }

    public void InitializeDefaultEvents()
    {
        InitializeOnSpawnEvents();
    }
}

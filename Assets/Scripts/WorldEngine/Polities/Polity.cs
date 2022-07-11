﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;
using System.Linq;
using System.Xml.Schema;

public delegate float GroupValueCalculationDelegate(CellGroup group);
public delegate float FactionValueCalculationDelegate(Faction faction);
public delegate float PolityContactValueCalculationDelegate(PolityContact contact);

[XmlInclude(typeof(Tribe))]
public abstract class Polity : ISynchronizable
{
    public const float TimeEffectConstant = CellGroup.GenerationSpan * 2500;
    public const float CoreDistanceEffectConstant = 10000;
    public const string CanFormPolityAttribute033 = "CAN_FORM_POLITY:";
    public const string CanFormTribeAttribute = "can_form_tribe";

    public const float MaxAdminCost = 1000000000000;

    public static List<IWorldEventGenerator> OnContactChangeEventGenerators;
    public static List<IWorldEventGenerator> OnRegionAccessibilityUpdateEventGenerators;

    [XmlAttribute("AC")]
    public float TotalAdministrativeCost_Internal = 0; // This is public to be XML-serializable

    [XmlAttribute("P")]
    public float TotalPopulation_Internal = 0; // This is public to be XML-serializable

    [XmlAttribute("A")]
    public float ProminenceArea_Internal = 0; // This is public to be XML-serializable

    [XmlAttribute("CRS")]
    public float CoreRegionSaturation_Internal = 0; // This is public to be XML-serializable

    [XmlAttribute("NC")]
    public bool NeedsNewCensus = true;

    [XmlAttribute("SP")]
    public bool StillPresent = true;

    [XmlAttribute("IF")]
    public bool IsUnderPlayerFocus = false;

    #region DominantFactionId
    [XmlAttribute("DFId")]
    public string DominantFactionIdStr
    {
        get { return DominantFactionId; }
        set { DominantFactionId = value; }
    }
    [XmlIgnore]
    public Identifier DominantFactionId;
    #endregion

    #region CoreGroupId
    [XmlAttribute("CGId")]
    public string CoreGroupIdStr
    {
        get { return CoreGroupId; }
        set { CoreGroupId = value; }
    }
    [XmlIgnore]
    public Identifier CoreGroupId;
    #endregion

    public List<Identifier> CoreRegionIds = new List<Identifier>();

    public List<PolityProminenceCluster> ProminenceClusters = new List<PolityProminenceCluster>();

    public Territory Territory;

    public PolityCulture Culture;

    public List<Identifier> FactionIds = null;

    public List<long> EventMessageIds;

    public List<PolityEventData> EventDataList = new List<PolityEventData>();

    [XmlIgnore]
    public bool IsBeingUpdated = false;

#if DEBUG
    [XmlIgnore]
    public long LastClusterAddedDate = -1;
#endif

    [XmlIgnore]
    public Dictionary<Identifier, CellGroup> Groups = new Dictionary<Identifier, CellGroup>();

    [XmlIgnore]
    public PolityInfo Info;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public CellGroup CoreGroup;

    [XmlIgnore]
    public bool CoreGroupIsValid = false;

    [XmlIgnore]
    public HashSet<Region> CoreRegions = new HashSet<Region>();

    [XmlIgnore]
    public Faction DominantFaction;

    [XmlIgnore]
    public bool WillBeUpdated;

    [XmlIgnore]
    public Identifier Id => Info.Id;

    [XmlIgnore]
    public string TypeStr => Info.TypeStr;

    [XmlIgnore]
    public PolityType Type => Info.Type;

    [XmlIgnore]
    public Name Name => Info.Name;

    [XmlIgnore]
    public long FormationDate => Info.FormationDate;

    [XmlIgnore]
    public Agent CurrentLeader => DominantFaction.CurrentLeader;

    [XmlIgnore]
    public float TotalAdministrativeCost
    {
        get
        {
            if (NeedsNewCensus)
            {
                RunCensus();
            }

            return TotalAdministrativeCost_Internal;
        }
    }

    [XmlIgnore]
    public float CoreRegionSaturation
    {
        get
        {
            if (NeedsNewCensus)
            {
                RunCensus();
            }

            return CoreRegionSaturation_Internal;
        }
    }

    [XmlIgnore]
    public HashSet<Region> NeighborRegions
    {
        get
        {
            if (_needsToFindNeighborRegions)
            {
                FindNeighborRegions();
            }

            return _neighborRegions;
        }
    }

    private void FindNeighborRegions()
    {
        _neighborRegions = new HashSet<Region>();

        if (Territory == null)
            throw new System.Exception("Territory is null. Polity: " + Id);

        foreach (Region region in Territory.GetAccessibleRegions())
        {
            if (CoreRegions.Contains(region))
                continue;

            _neighborRegions.Add(region);
        }

        _needsToFindNeighborRegions = false;
    }

    public void AccessibleRegionsUpdate()
    {
        _needsToFindNeighborRegions = true;

        ApplyRegionAccessibilityUpdate();
    }

    public void AddCoreRegion(Region region)
    {
        if (!CoreRegions.Add(region))
        {
            // there's no need to do anything else if it is already part of the
            // core regions
            return;
        }

        CoreRegionIds.Add(region.Id);

        _needsToFindNeighborRegions = true;

        NeedsNewCensus = true;

        Manager.AddedCoreRegion(this, region);
    }

    public void RemoveCoreRegion(Region region)
    {
        if (!CoreRegions.Remove(region))
        {
            // there's no need to do anything else if it was alread removed
            return;
        }

        CoreRegionIds.Remove(region.Id);

        _needsToFindNeighborRegions = true;

        NeedsNewCensus = true;

        Manager.RemovedCoreRegion(this, region);
    }

    /// <summary>
    /// Resets the event generators associated with polities
    /// </summary>
    public static void ResetEventGenerators()
    {
        OnContactChangeEventGenerators = new List<IWorldEventGenerator>();
        OnRegionAccessibilityUpdateEventGenerators = new List<IWorldEventGenerator>();
    }

    public float TotalPopulation
    {
        get
        {
            if (NeedsNewCensus)
            {
                RunCensus();
            }

            return TotalPopulation_Internal;
        }
    }

    public float ProminenceArea
    {
        get
        {
            if (NeedsNewCensus)
            {
                RunCensus();
            }

            return ProminenceArea_Internal;
        }
    }

    protected class WeightedGroup : CollectionUtility.ElementWeightPair<CellGroup>
    {
        public WeightedGroup(CellGroup group, float weight) : base(group, weight)
        {

        }
    }

    protected class WeightedFaction : CollectionUtility.ElementWeightPair<Faction>
    {
        public WeightedFaction(Faction faction, float weight) : base(faction, weight)
        {

        }
    }

    protected class WeightedPolityContact : CollectionUtility.ElementWeightPair<PolityContact>
    {
        public WeightedPolityContact(PolityContact contact, float weight) : base(contact, weight)
        {

        }
    }

    protected Dictionary<long, PolityEvent> _events = new Dictionary<long, PolityEvent>();

    private Dictionary<Identifier, Faction> _factions = new Dictionary<Identifier, Faction>();

    private bool _willBeRemoved = false;

    private HashSet<long> _eventMessageIds = new HashSet<long>();

    private Dictionary<Identifier, PolityContact> _contacts =
        new Dictionary<Identifier, PolityContact>();

    private bool _needsToFindNeighborRegions = true;
    private HashSet<Region> _neighborRegions;

    public Polity()
    {

    }

    protected Polity(string type, CellGroup coreGroup, long idOffset = 0)
    {
        World = coreGroup.World;

        Territory = new Territory(this);

        CoreGroup = coreGroup;
        CoreGroupId = coreGroup.Id;

        long initId = GenerateInitId(idOffset);

        Info = new PolityInfo(this, type, World.CurrentDate, initId);

        Culture = new PolityCulture(this);

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if (CoreGroupId == Manager.TracingData.GroupId) {
        //				string groupId = "Id:" + CoreGroupId + "|Long:" + CoreGroup.Longitude + "|Lat:" + CoreGroup.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"new Polity - Group:" + groupId + 
        //					", Polity.Id: " + Id,
        //					"CurrentDate: " + World.CurrentDate  +
        //					", CoreGroup:" + groupId + 
        //					", Polity.TotalGroupProminenceValue: " + TotalGroupProminenceValue + 
        //					", coreGroupProminenceValue: " + coreGroupProminenceValue + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        //// Make sure there's a region to spawn into
        Region startRegion = coreGroup.Cell.GetRegion(Culture.Language);

        if (startRegion == null)
        {
            throw new System.Exception(
                "No region could be generated with from cell " + coreGroup.Cell);
        }

        AddCoreRegion(startRegion);
    }

    public void Initialize()
    {
        Culture.Initialize();

        foreach (Faction faction in _factions.Values)
        {
            if (!faction.IsInitialized)
            {
                faction.Initialize();
            }
        }
    }

    public float CalculateCoreRegionSaturation()
    {
        float coreRegionArea = 0;
        foreach (Region region in CoreRegions)
        {
            coreRegionArea += region.TotalArea;
        }

        float coreProminenceArea = 0;
        foreach (PolityProminenceCluster cluster in ProminenceClusters)
        {
            if (CoreRegions.Contains(cluster.Region))
            {
                coreProminenceArea += cluster.ProminenceArea;
            }
        }

        return coreProminenceArea / coreRegionArea;
    }

    public void Destroy()
    {
        if (Territory.IsSelected)
        {
            Manager.SetSelectedTerritory(null);
        }

        if (IsUnderPlayerFocus)
        {
            Manager.UnsetFocusOnPolity(this);
        }

        List<PolityContact> contacts = new List<PolityContact>(_contacts.Values);

        List<Faction> factions = new List<Faction>(_factions.Values);

        foreach (Faction faction in factions)
        {
            faction.Destroy(true);
        }

        foreach (CellGroup group in Groups.Values)
        {
            group.SetPolityProminenceToRemove(this);

            World.AddGroupToPostUpdate_AfterPolityUpdate(group);
        }

        Info.Polity = null;

        StillPresent = false;
    }

    public static bool ValidateType(string polityType)
    {
        switch (polityType)
        {
            case Tribe.PolityTypeStr:
                return true;
        }

        return false;
    }

    public void Split(string polityType, Faction splittingFaction)
    {
//#if DEBUG
//        Manager.Debug_PauseSimRequested = true;
//#endif

        Polity newPolity;

        switch (polityType)
        {
            case Tribe.PolityTypeStr:
                newPolity = new Tribe(splittingFaction as Clan);

                AddEventMessage(new TribeSplitEventMessage(
                    splittingFaction as Clan,
                    this as Tribe, newPolity as Tribe, World.CurrentDate));
                break;

            default:
                throw new System.ArgumentException("Unhandled polity type: " + polityType);
        }

        newPolity.Initialize();
        World.AddPolityInfo(newPolity);

        splittingFaction.SetToUpdate();
        DominantFaction.SetToUpdate();
    }

    public override int GetHashCode()
    {
        return Info.GetHashCode();
    }

    public virtual string GetName()
    {
        return Info.Name.Text;
    }

    public string GetNameAndTypeString()
    {
        return Info.GetNameAndTypeString();
    }

    public string GetNameAndTypeStringBold()
    {
        return Info.GetNameAndTypeStringBold();
    }

    public void SetUnderPlayerFocus(bool state, bool setDominantFactionFocused = true)
    {
        IsUnderPlayerFocus = state;
    }

    public void AddEventMessage(WorldEventMessage eventMessage)
    {
        if (IsUnderPlayerFocus)
            World.AddEventMessageToShow(eventMessage);

        _eventMessageIds.Add(eventMessage.Id);
    }

    public bool HasEventMessage(long id)
    {
        return _eventMessageIds.Contains(id);
    }

    public void SetCoreGroup(CellGroup newCoreGroup, bool resetCoreDistances = true)
    {
        if (CoreGroup == newCoreGroup)
            return;

        if ((CoreGroup != null) && CoreGroupIsValid)
        {
            Manager.AddUpdatedCell(CoreGroup.Cell, CellUpdateType.Territory, CellUpdateSubType.Core);

            if (resetCoreDistances)
            {
                CoreGroup.ResetCoreDistances(Id, true);
            }
        }

        CoreGroup = newCoreGroup;
        CoreGroupId = newCoreGroup.Id;
        CoreGroupIsValid = true;

        Manager.AddUpdatedCell(newCoreGroup.Cell, CellUpdateType.Territory, CellUpdateSubType.Core);

        if (resetCoreDistances)
        {
            CoreGroup.ResetCoreDistances(Id, true);
        }
    }

    public long GenerateInitId(long idOffset = 0L)
    {
        return CoreGroup.GenerateInitId(idOffset);
    }

    public float GetNextLocalRandomFloat(int iterationOffset)
    {
        return CoreGroup.GetNextLocalRandomFloat(iterationOffset + unchecked(Id.GetHashCode()));
    }

    public int GetNextLocalRandomInt(int iterationOffset, int maxValue)
    {
        return CoreGroup.GetNextLocalRandomInt(iterationOffset + unchecked(Id.GetHashCode()), maxValue);
    }

    public void AddFaction(Faction faction)
    {
        _factions.Add(faction.Id, faction);

        if (!World.ContainsFactionInfo(faction.Id))
        {
            World.AddFactionInfo(faction.Info);
        }

        World.AddFactionToUpdate(faction);
    }

    public void RemoveFaction(Faction faction)
    {
        _factions.Remove(faction.Id);

        if (_factions.Count <= 0)
        {
            PrepareToRemoveFromWorld();
            return;
        }

        World.AddPolityToUpdate(this);
    }

    public Faction GetFaction(Identifier id)
    {
        if (id == null)
        {
            throw new System.Exception($"faction id can't be null");
        }

        _factions.TryGetValue(id, out Faction faction);

        return faction;
    }

    private void UpdateDominantFaction()
    {
        Faction mostInfluentFaction = null;
        float greatestInfluence = float.MinValue;

        foreach (Faction faction in _factions.Values)
        {
            if (faction.Influence > greatestInfluence)
            {
                mostInfluentFaction = faction;
                greatestInfluence = faction.Influence;
            }
        }

        if ((mostInfluentFaction == null) || (!mostInfluentFaction.StillPresent))
        {
            throw new System.Exception("Faction is null or not present");
        }

        SetDominantFaction(mostInfluentFaction);
    }

    /// <summary>
    /// Sets the most dominant faction within a polity
    /// </summary>
    /// <param name="faction">faction to set as dominant</param>
    /// <param name="resetCoreDistances">indicate if core distances should be
    /// recalculated</param>
    public void SetDominantFaction(Faction faction, bool resetCoreDistances = true)
    {
        if (DominantFaction == faction)
            return;

        if ((DominantFaction != null) && DominantFaction.StillPresent)
        {
            DominantFaction.SetDominant(false);
        }

        if ((faction == null) || (!faction.StillPresent))
            throw new System.Exception("Faction is null or not present");

        if (faction.Polity != this)
            throw new System.Exception("Faction is not part of polity");

//#if DEBUG
//        if ((Id == "176743860:7489493386076493324") || (Id == "215624940:7812196115215947840"))
//        {
//            Debug.LogWarning($"DEBUG: changing dominant faction of polity {Id}, DominantFaction: {DominantFaction?.Id}, faction: {faction?.Id}");

//            if (World.CurrentDate == 215643192)
//            {
//                Debug.LogWarning($"Debugging SetDominantFaction");
//            }
//        }
//#endif

        DominantFaction = faction;

        if (faction != null)
        {
            DominantFactionId = faction.Info.Id;

            faction.SetDominant(true);

            SetCoreGroup(faction.CoreGroup, resetCoreDistances);

            foreach (PolityContact contact in _contacts.Values)
            {
                if (!faction.HasRelationship(contact.NeighborPolity.DominantFaction))
                {
                    Faction.SetRelationship(faction, contact.NeighborPolity.DominantFaction, needFactionsToUpdate: false);
                }
            }
        }

        if (!World.PolitiesHaveBeenUpdated)
        {
            World.AddPolityToUpdate(this);
        }
    }

    public void TransferGroups(
        Polity sourcePolity,
        ICollection<CellGroup> groupsToTransfer)
    {
        foreach (var group in groupsToTransfer)
        {
            var origProminence = group.GetPolityProminence(sourcePolity);
            float origValue = origProminence.Value;

            group.RemovePolityProminence(origProminence, false);

            var prominence = group.IncreasePolityProminenceValue(this, origValue);

            prominence.ResetCoreDistances(addToRecalcs: true);

            group.FindHighestPolityProminence();

            World.AddGroupWithPolityCountChange(group);
            World.AddGroupToUpdate(group);
        }
    }

    private void SetContactUpdatedCells(Polity polity)
    {
        Manager.AddUpdatedCells(polity, CellUpdateType.Territory, CellUpdateSubType.Relationship);
    }

    private void AddContact(Polity polity)
    {
        PolityContact contact = new PolityContact(World, this, polity);

        _contacts.Add(polity.Id, contact);

        if (DominantFaction == null)
        {
            throw new System.Exception($"Dominant faction is null, polity: {Id}");
        }

        if (polity == null)
        {
            throw new System.Exception($"Contact polity is null, polity: {Id}");
        }

        if (!DominantFaction.HasRelationship(polity.DominantFaction))
        {
            DominantFaction.SetRelationship(polity.DominantFaction, needFactionToUpdate: false);
        }

        ApplyPolityContactChange();
    }

    private void RemoveContact(Polity polity)
    {
        _contacts.Remove(polity.Id);

        ApplyPolityContactChange();
    }

    public ICollection<PolityContact> GetContacts()
    {
        return _contacts.Values;
    }

    public PolityContact GetContact(Polity polity)
    {
        if (_contacts.TryGetValue(polity.Id, out PolityContact contact))
        {
            return contact;
        }

        return null;
    }

    public float GetContactStrength(Polity polity)
    {
        if (!_contacts.ContainsKey(polity.Id))
            return 0;

        return _contacts[polity.Id].Strength;
    }

    public int GetContactCount(Polity polity)
    {
        if (!_contacts.ContainsKey(polity.Id))
            return 0;

        return _contacts[polity.Id].Count;
    }

    public void IncreaseContact(Polity polity)
    {
        if (!_contacts.ContainsKey(polity.Id))
        {
            AddContact(polity);
        }

        _contacts[polity.Id].Count++;

        SetContactUpdatedCells(polity);
    }

    public void DecreaseContact(Polity polity)
    {
        if (!_contacts.TryGetValue(polity.Id, out PolityContact contact))
        {
            throw new System.Exception($"(id: {Id}) contact not present: {polity.Id}");
        }

        contact.Count--;

        if (contact.Count <= 0)
        {
            RemoveContact(polity);
        }

        SetContactUpdatedCells(polity);
    }

    public float GetRelationshipValue(Polity polity)
    {
        if (!_contacts.ContainsKey(polity.Id))
            throw new System.Exception("(id: " + Id +
                ") contact not present: " + polity.Id);

        return DominantFaction.GetRelationshipValue(polity.DominantFaction);
    }

    public ICollection<Faction> GetFactions() => _factions.Values;

    public IEnumerable<Faction> GetFactions(string type)
    {
        foreach (Faction faction in _factions.Values)
        {
            if (faction.Type == type)
                yield return faction;
        }
    }

    public IEnumerable<T> GetFactions<T>() where T : Faction
    {
        foreach (T faction in _factions.Values)
        {
            yield return faction;
        }
    }

    public void NormalizeFactionInfluences()
    {
        float totalInfluence = 0;

        foreach (Faction f in _factions.Values)
        {
            totalInfluence += f.Influence;
        }

        if (totalInfluence <= 0)
        {
            throw new System.Exception("Total influence equal or less than zero: " +
                totalInfluence + ", polity id:" + Id);
        }

        foreach (Faction f in _factions.Values)
        {
            f.Influence = f.Influence / totalInfluence;
        }
    }

    public static void TransferInfluence(Faction sourceFaction, Faction targetFaction, float percentage)
    {
        // Can only tranfer influence between factions belonging to the same polity

        if (sourceFaction.PolityId != targetFaction.PolityId)
            throw new System.Exception($"Source faction and target faction do not belong to same polity. " +
                $"source's Polity: {sourceFaction.PolityId}, target's polity: {targetFaction.PolityId}");

        // Always reduce influence of source faction and increase promience of target faction

        if ((percentage < 0f) || (percentage > 1f))
            throw new System.Exception("Invalid percentage: " + percentage);

        float oldSourceInfluenceValue = sourceFaction.Influence;

        sourceFaction.Influence = oldSourceInfluenceValue * (1f - percentage);

        float influenceDelta = oldSourceInfluenceValue - sourceFaction.Influence;

        targetFaction.Influence += influenceDelta;
    }

    public void PrepareToRemoveFromWorld()
    {
        World.AddPolityToRemove(this);

        _willBeRemoved = true;
    }

    public void NormalizeAndUpdateDominantFaction()
    {
        if (_willBeRemoved) 
            return;

        NormalizeFactionInfluences();

        UpdateDominantFaction();
    }

    public void Update()
    {
        if (_willBeRemoved) 
            return;

        if (!StillPresent)
        {
            Debug.LogWarning("Polity is no longer present. Id: " + Id);

            return;
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            Manager.RegisterDebugEvent("DebugMessage",
        //                "Update - Polity:" + Id +
        //                ", CurrentDate: " + World.CurrentDate +
        //                ", ProminencedGroups.Count: " + ProminencedGroups.Count +
        //                ", TotalGroupProminenceValue: " + TotalGroupProminenceValue +
        //                "");
        //        }
        //#endif

        WillBeUpdated = false;

        if (Groups.Count <= 0)
        {
#if DEBUG
            Debug.Log("Polity will be removed due to losing all prominenced groups. polity id: " + Id);
#endif

            PrepareToRemoveFromWorld();
            return;
        }

        IsBeingUpdated = true;

        NormalizeAndUpdateDominantFaction();

        Culture.Update();

        UpdateInternal();

        Manager.AddUpdatedCells(Territory, CellUpdateType.Territory, CellUpdateSubType.Culture);

        IsBeingUpdated = false;
    }

    protected abstract void UpdateInternal();

    public void RunCensus()
    {
        TotalAdministrativeCost_Internal = 0;
        TotalPopulation_Internal = 0;
        ProminenceArea_Internal = 0;

#if DEBUG
        int totalClusterGroupCount = 0;
        int totalUpdatedClusterGroupCount = 0;
        int updatedClusters = 0;
#endif

        foreach (PolityProminenceCluster cluster in ProminenceClusters)
        {
#if DEBUG
            totalClusterGroupCount += cluster.Size;
#endif

            if (cluster.NeedsNewCensus)
            {
#if DEBUG
                totalUpdatedClusterGroupCount += cluster.Size;
                updatedClusters++;
#endif

                cluster.RunCensus();
            }

            TotalAdministrativeCost_Internal = 
                Mathf.Min(TotalAdministrativeCost_Internal + cluster.TotalAdministrativeCost, MaxAdminCost);

            TotalPopulation_Internal += cluster.TotalPopulation;

            ProminenceArea_Internal += cluster.ProminenceArea;
        }

#if DEBUG
        if (Groups.Count != totalClusterGroupCount)
        {
            Debug.LogError("Groups.Count (" + Groups.Count +
                ") not equal to totalClusterGroupCount (" +
                totalClusterGroupCount + "). Polity Id: " + Id);
        }
#endif

        CoreRegionSaturation_Internal = CalculateCoreRegionSaturation();

        NeedsNewCensus = false;
    }

    public void AddGroup(PolityProminence prominence)
    {
        Groups.Add(prominence.Id, prominence.Group);

        AddToCluster(prominence);

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 1))
        {
            if (Manager.TracingData.PolityId == Id)
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "Polity.AddGroup - Polity Id: " + Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", prominence.Id: " + prominence.Id +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif
    }

    public void RemoveGroup(PolityProminence prominence)
    {
        Groups.Remove(prominence.Id);

        PolityProminenceCluster cluster = prominence.Cluster;

        if (cluster != null)
        {
            cluster.RemoveProminence(prominence);

            // Sketchy code. Make sure removing clusters this way is not troublesome
            // for the simulation (and perf)
            if (cluster.Size <= 0)
            {
                ProminenceClusters.Remove(cluster);
            }
        }
#if DEBUG
        else
        {
            Debug.LogError("Removing group with null cluster, id: " + prominence.Id +
                ", init date: " + prominence.Group.InitDate +
                ", polity: " + prominence.Polity.Id);

            // Validate that this prominence was not part of any cluster
            foreach (PolityProminenceCluster c in ProminenceClusters)
            {
                if (c.HasPolityProminence(prominence))
                {
                    throw new System.Exception(
                        "null prominence Cluster - group Id: " + prominence.Id + ", polity Id: " + Id);
                }
            }
        }
#endif

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 1))
        {
            if (Manager.TracingData.PolityId == Id)
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "Polity.RemoveGroup - Polity:" + Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", prominence.Id: " + prominence.Id +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif
    }

    private void AddToCluster(PolityProminence prominence)
    {
        //#if DEBUG
        //            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //            {
        //                if (Manager.TracingData.PolityId == Id)
        //                {
        //                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Polity.ClusterUpdate 2 - Polity:" + Id,
        //                        "CurrentDate: " + World.CurrentDate +
        //                        ", prominence.Id: " + prominence.Id +
        //                        ", prominence.Group.LastUpdateDate: " + prominence.Group.LastUpdateDate +
        //                        " [offset: " + (prominence.Group.LastUpdateDate - Manager.TracingData.LastSaveDate) + "]" +
        //                        "", World.CurrentDate);

        //                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //                }
        //            }
        //#endif

        PolityProminenceCluster clusterToAddTo = null;

        CellGroup group = prominence.Group;

        Region region = group.Cell.GetRegion(Culture.Language);

        if (region == null)
        {
            throw new System.Exception("Region is null. Group Id: " + group.Id);
        }

        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            PolityProminence nProminence = nGroup.GetPolityProminence(this);

            if ((nProminence != null) && (nProminence.Cluster != null))
            {
                Region nRegion = nGroup.Cell.GetRegion(Culture.Language);

                if (nRegion == null)
                {
                    throw new System.Exception("Region is null. Neighbor group Id: " + nGroup.Id);
                }

                if (nRegion != region) break;

                clusterToAddTo = nProminence.Cluster;

                //#if DEBUG
                //                    if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
                //                    {
                //                        if (Manager.TracingData.PolityId == Id)
                //                        {
                //                            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Polity.ClusterUpdate add to cluster - Polity:" + Id,
                //                                "CurrentDate: " + World.CurrentDate +
                //                                ", ProminenceClusters.Count: " + ProminenceClusters.Count +
                //                                ", clusterToAddTo.Id: " + clusterToAddTo.Id +
                //                                ", clusterToAddTo.Size: " + clusterToAddTo.Size +
                //                                ", prominence.Id: " + prominence.Id +
                //                                ", prominence.Group.LastUpdateDate: " + prominence.Group.LastUpdateDate +
                //                                " [offset: " + (prominence.Group.LastUpdateDate - Manager.TracingData.LastSaveDate) + "]" +
                //                                "", World.CurrentDate);

                //                            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                //                        }
                //                    }
                //#endif

                clusterToAddTo.AddProminence(prominence);

                if (clusterToAddTo.Size > PolityProminenceCluster.MaxSize)
                {
#if DEBUG
                    PolityProminenceCluster parentCluster = clusterToAddTo;
                    int oldSize = parentCluster.Size;
#endif

                    clusterToAddTo = clusterToAddTo.Split(prominence);
                    ProminenceClusters.Add(clusterToAddTo);

                    //#if DEBUG
                    //                        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
                    //                        {
                    //                            if (Manager.TracingData.PolityId == Id)
                    //                            {
                    //                                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Polity.ClusterUpdate split cluster - Polity:" + Id,
                    //                                    "CurrentDate: " + World.CurrentDate +
                    //                                    ", _prominencesToAddToClusters.Count: " + _prominencesToAddToClusters.Count +
                    //                                    ", ProminenceClusters.Count: " + ProminenceClusters.Count +
                    //                                    ", clusterToAddTo.Id: " + clusterToAddTo.Id +
                    //                                    ", clusterToAddTo.Size: " + clusterToAddTo.Size +
                    //                                    ", parentCluster.Id: " + parentCluster.Id +
                    //                                    ", parentCluster.Size: " + parentCluster.Size +
                    //                                    ", parentCluster.Size (previous): " + oldSize +
                    //                                    ", prominence.Id: " + prominence.Id +
                    //                                    "", World.CurrentDate);

                    //                                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                    //                            }
                    //                        }

                    //                        LastClusterAddedDate = World.CurrentDate;
                    //#endif
                }
                break;
            }
        }

        if (clusterToAddTo == null)
        {
            clusterToAddTo = new PolityProminenceCluster(prominence);
            ProminenceClusters.Add(clusterToAddTo);

            //#if DEBUG
            //                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //                {
            //                    if (Manager.TracingData.PolityId == Id)
            //                    {
            //                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("Polity.ClusterUpdate null cluster - Polity:" + Id,
            //                            "CurrentDate: " + World.CurrentDate +
            //                            ", _prominencesToAddToClusters.Count: " + _prominencesToAddToClusters.Count +
            //                            ", ProminenceClusters.Count: " + ProminenceClusters.Count +
            //                            ", clusterToAddTo.Id: " + clusterToAddTo.Id +
            //                            ", clusterToAddTo.Size: " + clusterToAddTo.Size +
            //                            ", prominence.Id: " + prominence.Id +
            //                            "", World.CurrentDate);

            //                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                    }
            //                }

            //                LastClusterAddedDate = World.CurrentDate;
            //#endif
        }
    }

    public virtual void Synchronize()
    {
        EventDataList.Clear();

        foreach (PolityEvent e in _events.Values)
        {
            EventDataList.Add(e.GetData() as PolityEventData);
        }

        foreach (PolityProminenceCluster cluster in ProminenceClusters)
        {
            cluster.Synchronize();
        }

        Culture.Synchronize();

        Territory.Synchronize();

        Name.Synchronize();

        FactionIds = new List<Identifier>(_factions.Keys);

        // Reload factions to ensure order is equal to that in the save file
        _factions.Clear();
        LoadFactions();

        EventMessageIds = new List<long>(_eventMessageIds);
    }

    private CellGroup GetGroupOrThrow(Identifier id)
    {
        CellGroup group = World.GetGroup(id);

        if (group == null)
        {
            string message = "Missing Group with Id: " + id + " in polity with Id: " + Id;
            throw new System.Exception(message);
        }

        return group;
    }

    private Faction GetFactionOrThrow(Identifier id)
    {
        Faction faction = World.GetFaction(id);

        if (faction == null)
        {
            string message = "Missing Faction with Id: " + id + " in polity with Id: " + Id;
            throw new System.Exception(message);
        }

        return faction;
    }

    public void LoadFactions()
    {
        foreach (Identifier id in FactionIds)
        {
            _factions.Add(id, GetFactionOrThrow(id));
        }
    }

    public virtual void FinalizeLoad()
    {
        LoadFactions();

        foreach (long messageId in EventMessageIds)
        {
            _eventMessageIds.Add(messageId);
        }

        Name.World = World;
        Name.FinalizeLoad();

        CoreGroup = World.GetGroup(CoreGroupId);

        if (CoreGroup == null)
        {
            string message = "Missing Group with Id " + CoreGroupId +
                " in polity with Id: " + Id;
            throw new System.Exception(message);
        }

        foreach (Identifier regionId in CoreRegionIds)
        {
            RegionInfo regionInfo = World.GetRegionInfo(regionId);

            if (regionInfo == null)
            {
                string message = "Missing Region with Id " + regionId +
                    " in polity with Id: " + Id;
                throw new System.Exception(message);
            }

            CoreRegions.Add(regionInfo.Region);
        }

        foreach (PolityProminenceCluster cluster in ProminenceClusters)
        {
            cluster.Polity = this;
            cluster.FinalizeLoad();

            foreach (PolityProminence p in cluster.GetPolityProminences())
            {
                Groups.Add(p.Id, p.Group);
            }
        }

        //Groups.FinalizeLoad(GetGroupOrThrow);

        DominantFaction = GetFaction(DominantFactionId);

        Territory.World = World;
        Territory.Polity = this;
        Territory.FinalizeLoad();

        Culture.World = World;
        Culture.Polity = this;
        Culture.FinalizeLoad();

        foreach (PolityContact contact in _contacts.Values)
        {
            contact.NeighborPolity = World.GetPolity(contact.Id);

            if (contact.NeighborPolity == null)
            {
                throw new System.Exception("Polity is null, Id: " + contact.Id);
            }
        }
    }

    public void AddEvent(PolityEvent polityEvent)
    {
        if (_events.ContainsKey(polityEvent.TypeId))
            throw new System.Exception("Polity event of type " + polityEvent.TypeId + " already present");

        _events.Add(polityEvent.TypeId, polityEvent);
        World.InsertEventToHappen(polityEvent);
    }

    public PolityEvent GetEvent(long typeId)
    {
        if (!_events.ContainsKey(typeId))
            return null;

        return _events[typeId];
    }

    public void ResetEvent(long typeId, long newTriggerDate)
    {
        if (!_events.ContainsKey(typeId))
            throw new System.Exception("Unable to find event of type: " + typeId);

        PolityEvent polityEvent = _events[typeId];

        polityEvent.Reset(newTriggerDate);
        World.InsertEventToHappen(polityEvent);
    }

    public static bool HasRequiredTribeFormationProperties(CellGroup group)
    {
        return group.HasProperty(CanFormPolityAttribute033 + "tribe") || group.HasProperty(CanFormTribeAttribute);
    }

    public virtual void GroupUpdateEffects(
        CellGroup group,
        float prominenceValue,
        float totalPolityProminenceValue,
        long timeSpan)
    {
        if (totalPolityProminenceValue == 0)
        {
            throw new System.Exception(
                $"totalPolityProminenceValue is 0. Polity Id: {Id}, Group Id: {group}");
        }

        if (!HasRequiredTribeFormationProperties(group))
        {
            group.SetPolityProminenceToRemove(this);

            return;
        }

        float coreFactionDistance = group.GetFactionCoreDistance(this);

        float coreDistancePlusConstant = coreFactionDistance + CoreDistanceEffectConstant;

        float distanceFactor = 0;

        if (coreDistancePlusConstant > 0)
            distanceFactor = CoreDistanceEffectConstant / coreDistancePlusConstant;

        TerrainCell groupCell = group.Cell;

        float maxTargetValue = 1f;
        float minTargetValue = 0.8f * totalPolityProminenceValue;

        int rngOffset = RngOffsets.POLITY_UPDATE_EFFECTS + unchecked(GetHashCode());

        float randomModifier = groupCell.GetNextLocalRandomFloat(rngOffset);
        randomModifier *= distanceFactor;
        float targetValue = ((maxTargetValue - minTargetValue) * randomModifier) + minTargetValue;

        float value =
            (targetValue - totalPolityProminenceValue) * prominenceValue / totalPolityProminenceValue;
        targetValue = prominenceValue + value;

        float timeFactor = timeSpan / (float)(timeSpan + TimeEffectConstant);

        float prominenceValueDelta =
            (prominenceValue * -timeFactor) + (targetValue * timeFactor);

        group.AddPolityProminenceValueDelta(this, prominenceValueDelta);
    }

    public CellGroup GetRandomGroup(int rngOffset)
    {
        if (ProminenceClusters.Count <= 0)
        {
            throw new System.Exception(
                $"Invalid number of prominence clusters in polity {Id}: {ProminenceClusters.Count}");
        }

        // Pick a random cluster
        int clusterIndex = GetNextLocalRandomInt(rngOffset++, ProminenceClusters.Count);
        PolityProminenceCluster cluster = ProminenceClusters[clusterIndex];

        // Pick a random prominence in cluster
        PolityProminence prominence = cluster.GetRandomProminence(rngOffset++);

        if (prominence == null)
        {
            throw new System.Exception("Random picked prominence is null. Polity Id: " + Id);
        }

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Manager.TracingData.PolityId == Id)
            {
                SaveLoadTest.DebugMessage debugMessage =
                    new SaveLoadTest.DebugMessage("Polity.GetRandomGroup - Polity: " + Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", ProminenceClusters.Count: " + ProminenceClusters.Count +
                    ", LastClusterAddedDate: " + LastClusterAddedDate +
                    ", LastClusterAddedDate offset from save: " +
                    (LastClusterAddedDate - Manager.TracingData.LastSaveDate) +
                    ", clusterIndex: " + clusterIndex +
                    ", cluster: " + cluster +
                    ", cluster.Size: " + cluster.Size +
                    ", cluster.LastProminenceChangeDate: " + cluster.LastProminenceChangeDate +
                    ", LastProminenceChangeDate offset from save: " +
                    (cluster.LastProminenceChangeDate - Manager.TracingData.LastSaveDate) +
                    ", prominence.Id: " + prominence.Group +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        return prominence.Group;
    }

    public CellGroup GetRandomWeightedGroup(int rngOffset, GroupValueCalculationDelegate calculateGroupValue, bool nullIfNoValidGroup = false)
    {
        // Instead of this cumbersome sampling mechanism, create a sample list for each polity that adds/removes groups that update 
        // or have been selected by this method

        int maxSampleSize = 20;

        int sampleGroupLength = 1 + (Groups.Count / maxSampleSize);

        int sampleSize = Groups.Count / sampleGroupLength;

        if ((sampleGroupLength > 1) && ((Groups.Count % sampleGroupLength) > 0))
        {
            sampleSize++;
        }

        WeightedGroup[] weightedGroups = new WeightedGroup[sampleSize];

        float totalWeight = 0;

        int index = 0;
        int sampleIndex = 0;
        int nextGroupToPick = GetNextLocalRandomInt(rngOffset++, sampleGroupLength);

        foreach (CellGroup group in Groups.Values)
        {
            bool skipGroup = false;

            if ((sampleGroupLength > 1) && (index != nextGroupToPick))
                skipGroup = true;

            index++;

            if (sampleGroupLength > 1)
            {
                if ((index % sampleGroupLength) == 0)
                {
                    int groupsRemaining = Groups.Count - index;

                    if (groupsRemaining > sampleGroupLength)
                    {
                        nextGroupToPick = index + GetNextLocalRandomInt(rngOffset++, sampleGroupLength);
                    }
                    else if (groupsRemaining > 0)
                    {
                        nextGroupToPick = index + (GetNextLocalRandomInt(rngOffset++, sampleGroupLength) % groupsRemaining);
                    }
                }
            }

            if (skipGroup) continue;

            float weight = calculateGroupValue(group);

            if (weight < 0)
                throw new System.Exception("calculateGroupValue method returned weight value less than zero: " + weight);

            totalWeight += weight;

            weightedGroups[sampleIndex] = new WeightedGroup(group, weight);

            sampleIndex++;
        }

        if (totalWeight < 0)
        {
            throw new System.Exception("Total weight can't be less than zero: " + totalWeight);
        }

        if ((totalWeight == 0) && nullIfNoValidGroup)
        {
            return null;
        }

        return CollectionUtility.WeightedSelection(weightedGroups, totalWeight, GetNextLocalRandomFloat(rngOffset));
    }

    public Faction GetRandomFaction(int rngOffset, FactionValueCalculationDelegate calculateFactionValue, bool nullIfNoValidFaction = false)
    {
        WeightedFaction[] weightedFactions = new WeightedFaction[_factions.Count];

        float totalWeight = 0;

        int index = 0;
        foreach (Faction faction in _factions.Values)
        {
            float weight = calculateFactionValue(faction);

            if (weight < 0)
                throw new System.Exception("calculateFactionValue method returned weight value less than zero: " + weight);

            totalWeight += weight;

            weightedFactions[index] = new WeightedFaction(faction, weight);
            index++;
        }

        if (totalWeight < 0)
        {
            throw new System.Exception("Total weight can't be less than zero: " + totalWeight);
        }

        if ((totalWeight == 0) && nullIfNoValidFaction)
        {
            return null;
        }

        return CollectionUtility.WeightedSelection(weightedFactions, totalWeight, GetNextLocalRandomFloat(rngOffset));
    }

    /// <summary>
    /// Returns a default weight value for all existing contacts
    /// </summary>
    /// <param name="contact">the contact to get the default weight for</param>
    /// <returns>the default weight value (1)</returns>
    private float GetDefaultContactWeight(PolityContact contact)
    {
        return 1;
    }

    /// <summary>
    /// Returns a random contact associated with this polity, using default weighting
    /// </summary>
    /// <param name="rngOffset">the offset to use for the local RNG</param>
    /// <returns>A contact, or null if there are no valid contacts to choose from</returns>
    public PolityContact GetRandomPolityContact(int rngOffset)
    {
        return GetRandomPolityContact(rngOffset, GetDefaultContactWeight);
    }

    /// <summary>
    /// Returns a random contact associated with this polity
    /// </summary>
    /// <param name="rngOffset">the offset to use for the local RNG</param>
    /// <param name="calculateContactValue">delegate to calculate the weight of a contact</param>
    /// <returns>A contact, or null if there are no valid contacts to choose from</returns>
    public PolityContact GetRandomPolityContact(
        int rngOffset, PolityContactValueCalculationDelegate calculateContactValue)
    {
        WeightedPolityContact[] weightedContacts = new WeightedPolityContact[_contacts.Count];

        float totalWeight = 0;

        int index = 0;
        foreach (PolityContact contact in _contacts.Values)
        {
            float weight = calculateContactValue(contact);

            if (weight < 0)
                throw new System.Exception(
                    "calculateContactValue method returned weight value less than zero: " + weight);

            totalWeight += weight;

            weightedContacts[index] = new WeightedPolityContact(contact, weight);
            index++;
        }

        float selectionValue = GetNextLocalRandomFloat(rngOffset);

        if (totalWeight < 0)
        {
            throw new System.Exception("Total weight can't be less than zero: " + totalWeight);
        }

        if (totalWeight == 0)
        {
            return null;
        }

        return CollectionUtility.WeightedSelection(weightedContacts, totalWeight, selectionValue);
    }

    protected abstract void GenerateName();

    [System.Obsolete]
    public float CalculateContactStrength(Polity polity)
    {
        if (!_contacts.ContainsKey(polity.Id))
        {
            return 0;
        }

        return CalculateContactStrength(_contacts[polity.Id]);
    }

    [System.Obsolete]
    public float CalculateContactStrength(PolityContact contact)
    {
        int contacGroupCount = contact.NeighborPolity.Groups.Count;

        float minGroupCount = Mathf.Min(contacGroupCount, Groups.Count);

        float countFactor = contact.Count / minGroupCount;

        return countFactor;
    }

    /// <summary>
    /// Applies the effects of adding or removing a contact from this polity
    /// </summary>
    public void ApplyPolityContactChange()
    {
        foreach (IWorldEventGenerator generator in OnContactChangeEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"Polity.ApplyPolityContactChange: adding '{context.Id}' to list of events to try to assign. Polity: {Id}");
            }

            if (generator is IFactionEventGenerator fGenerator)
            {
                foreach (Faction faction in _factions.Values)
                {
                    faction.AddGeneratorToTestAssignmentFor(fGenerator);
                }
            }
        }
    }

    /// <summary>
    /// Applies the effects of gaining or loosing access to a region
    /// </summary>
    public void ApplyRegionAccessibilityUpdate()
    {
        foreach (IWorldEventGenerator generator in OnRegionAccessibilityUpdateEventGenerators)
        {
            if ((generator is Context context) && context.DebugLogEnabled)
            {
                Debug.Log($"Polity.ApplyRegionAccessibilityUpdate: adding '{context.Id}' to list of events to try to assign");
            }

            if (generator is IFactionEventGenerator fGenerator)
            {
                foreach (Faction faction in _factions.Values)
                {
                    faction.AddGeneratorToTestAssignmentFor(fGenerator);
                }
            }
        }
    }

    public Dictionary<CellGroup, float> GetGroupsAndPromValues(float percentProminence = 1f)
    {
        var groupsToTransfer = new Dictionary<CellGroup, float>();

        foreach (var cluster in ProminenceClusters)
        {
            foreach (var prominence in cluster.GetPolityProminences())
            {
                float sourceProminenceValueDelta = prominence.Value * percentProminence;

                groupsToTransfer.Add(prominence.Group, sourceProminenceValueDelta);
            }
        }

        return groupsToTransfer;
    }

    public void SetToUpdate(bool warnIfUnexpected = true)
    {
        DominantFaction.SetToUpdate(warnIfUnexpected);
    }

    public void MergePolity(Polity polity)
    {
//#if DEBUG
//        Manager.Debug_PauseSimRequested = true;
//        Manager.Debug_BreakRequested = true;
//        Manager.Debug_IdentifierOfInterest = Id;
//        Manager.Debug_IdentifierOfInterest2 = polity.Id;
//        Manager.Debug_IdentifierOfInterest3 = polity.CoreGroupId;
//        Manager.Debug_IdentifierOfInterest4 = CoreGroupId;
//#endif

        World.AddPolityToRemove(polity);

        SetToUpdate();

        float polPopulation = Mathf.Floor(polity.TotalPopulation);

#if DEBUG
        World.PolityMergeCount++;
#endif

        if (polPopulation <= 0)
        {
            Debug.LogWarning("Merged polity with 0 or less population. this.Id: "
                + Id + ", polity.Id: " + polity.Id);

            return;
        }

        float localPopulation = Mathf.Floor(TotalPopulation);

        float populationFactor = polPopulation / localPopulation;

        List<Faction> factionsToMove = new List<Faction>(polity.GetFactions());

        foreach (Faction faction in factionsToMove)
        {
            faction.ChangePolity(this, faction.Influence * populationFactor);

            faction.SetToUpdate();
        }

        foreach (Region region in polity.CoreRegions)
        {
            AddCoreRegion(region);
        }
    }

    public float CalculateAdministrativeLoad()
    {
        Culture.TryGetKnowledgeValue(SocialOrganizationKnowledge.KnowledgeId, out float socialOrganizationValue);

        if (socialOrganizationValue <= 0)
        {
            return Mathf.Infinity;
        }

        float administrativeLoad = TotalAdministrativeCost / socialOrganizationValue;

        if (administrativeLoad < 0)
        {
            Debug.LogWarning("administrativeLoad less than 0: " + administrativeLoad);

            return Mathf.Infinity;
        }

        return administrativeLoad;
    }
}

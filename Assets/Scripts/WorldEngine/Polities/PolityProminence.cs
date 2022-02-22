using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence
{
    public const float MaxCoreDistance = 1000000000000f;

    public const float MaxAdminCost = 
        PolityProminenceCluster.MaxAdminCost / PolityProminenceCluster.MaxSize;

    [XmlAttribute("V")]
    public float Value = 0;
    [XmlAttribute("FCT")]
    public float FactionCoreDistance = -1;
    [XmlAttribute("PD")]
    public float PolityCoreDistance = -1;

    [XmlAttribute("P")]
    public bool StillPresent = true;

    #region ClosestFactionId
    [XmlAttribute("CFId")]
    public string ClosestFactionIdStr
    {
        get { return ClosestFactionId; }
        set { ClosestFactionId = value; }
    }
    [XmlIgnore]
    public Identifier ClosestFactionId = null;
    #endregion

    #region PolityId
    [XmlAttribute("PId")]
    public string PolityIdStr
    {
        get { return PolityId; }
        set { PolityId = value; }
    }
    [XmlIgnore]
    public Identifier PolityId;
    #endregion

    public Identifier Id => Group.Id;

#if DEBUG
    [XmlIgnore]
    public long LastCoreDistanceSet = -1;
    [XmlIgnore]
    public long PrevCoreDistanceSet = -1;
    [XmlIgnore]
    public long LastCoreDistanceReset = -1;
#endif

    [XmlIgnore]
    public PolityProminenceCluster Cluster = null;

    [XmlIgnore]
    public Faction ClosestFaction = null;
    [XmlIgnore]
    public Polity Polity;

    [XmlIgnore]
    public CellGroup Group;
    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public float MigrationPressure = 0;

    [XmlIgnore]
    public float AdministrativeCost
    {
        get
        {
            if (_adminCostUpdateNeeded)
            {
                RecalculateAdministrativeCost();
            }

            return _adminCost;
        }
    }

    private bool _adminCostUpdateNeeded = true;
    private float _adminCost = 0;

    [XmlIgnore]
    public readonly HashSet<PolityProminence> NeighborProminences = new HashSet<PolityProminence>();

    // Not necessarily ordered, do not use during serialization or algorithms that
    // have a dependency on consistent order
    [XmlIgnore]
    public IEnumerable<KeyValuePair<Direction, PolityProminence>> NeighborProminencesInPolity
    {
        get
        {
            foreach (var pair in Group.Neighbors)
            {
                if (pair.Value.TryGetPolityProminence(Polity, out PolityProminence p))
                {
                    yield return new KeyValuePair<Direction, PolityProminence>(pair.Key, p);
                }
            }
        }
    }

    /// <summary>
    /// Constructs a new polity prominence object (only used by XML deserializer)
    /// </summary>
    public PolityProminence()
    {

    }

    /// <summary>
    /// Constructs a new polity prominence object (only used by XML deserializer)
    /// </summary>
    /// <param name="group">group associated with this prominence</param>
    /// <param name="polity">polity associated with this prominence</param>
    /// <param name="initialValue">starting prominence value</param>
    public PolityProminence(CellGroup group, Polity polity, float initialValue = 0)
    {
        Group = group;
        World = group.World;
        Polity = polity;
        PolityId = polity.Id;
        Value = initialValue;

        SetAllNeighborProminences();
    }

    public void Destroy()
    {
        InitDestruction();
        FinishDestruction();
    }

    public void InitDestruction(bool validateFaction = true)
    {
        if (validateFaction && (ClosestFaction != null) && (ClosestFaction.PolityId != PolityId))
        {
            throw new System.Exception(
                $"Closest faction doesn't belong to same polity as prominence, " +
                $"group: {Id}, " +
                $"faction: {ClosestFaction.Id}, " +
                $"faction's polity: {ClosestFaction.PolityId}, " +
                $"prom's polity: {PolityId}");
        }

        StillPresent = false;
    }

    public void FinishDestruction()
    {
        ResetNeighborCoreDistances();

        UnsetAllNeighborProminences();

        ClosestFaction?.RemoveProminence(this);
    }

    public void SetClosestFaction(Faction faction)
    {
        if (ClosestFaction == faction)
            return;

        ClosestFaction?.RemoveProminence(this);

        ClosestFaction = faction;
        ClosestFactionId = faction?.Id;

        faction?.AddProminence(this);
    }

    /// <summary>
    /// Define the new core distances to update this prominence with
    /// </summary>
    /// <returns>'true' iff core distances have changed</returns>
    public bool CalculateNewCoreDistances()
    {
        float newFactionCoreDistance =
            CalculateShortestFactionCoreDistance(out Faction closestFaction);
        float newPolityCoreDistance =
            CalculateShortestPolityCoreDistance();

        // Make sure at least one core distance is actually different
        if ((FactionCoreDistance != newFactionCoreDistance) ||
            (PolityCoreDistance != newPolityCoreDistance) ||
            (ClosestFactionId != closestFaction.Id))
        {
            FactionCoreDistance = newFactionCoreDistance;
            PolityCoreDistance = newPolityCoreDistance;

            SetClosestFaction(closestFaction);

#if DEBUG
            PrevCoreDistanceSet = LastCoreDistanceSet;
            LastCoreDistanceSet = Manager.CurrentWorld.CurrentDate;
#endif

            if (FactionCoreDistance == -1)
            {
                throw new System.Exception("Faction core distance is not properly initialized. " +
                    "Polity id: " + Polity.Id + ", Group id: " + Group.Id);
            }

            if (PolityCoreDistance == -1)
            {
                throw new System.Exception("Polity core distance is not properly initialized. " +
                    "Polity id: " + Polity.Id + ", Group id: " + Group.Id);
            }

            RequireRecalculations();

            Manager.AddUpdatedCell(
                Group.Cell, CellUpdateType.Group, CellUpdateSubType.CoreDistance);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the latest calculated core distance to the polity. Should only be used
    /// during distance recalculations
    /// </summary>
    /// <param name="toTactionCore">look for faction core distance instead of polity
    /// core distance</param>
    /// <returns>the latest calculated faction or polity core distance</returns>
    private float GetCurrentCoreDistance(bool toTactionCore)
    {
        if (toTactionCore)
        {
            return FactionCoreDistance;
        }
        else
        {
            return PolityCoreDistance;
        }
    }

    /// <summary>
    /// Calculates the current shortest faction core distance
    /// </summary>
    /// <param name="closestFaction">the faction that whose core is closest</param>
    /// <returns>the calculated shortest core distance</returns>
    private float CalculateShortestFactionCoreDistance(out Faction closestFaction)
    {
        foreach (Faction faction in Polity.GetFactions())
        {
            if (faction.CoreGroup == Group)
            {
                closestFaction = faction;
                return 0;
            }
        }

        return CalculateShortestCoreDistance(true, out closestFaction);
    }

    /// <summary>
    /// Calculates the current shortest polity core distance
    /// </summary>
    /// <returns>the calculated shortest core distance</returns>
    private float CalculateShortestPolityCoreDistance()
    {
//#if DEBUG
//        if (Manager.Debug_BreakRequested &&
//            (Manager.Debug_IdentifierOfInterest == PolityId) &&
//            (Manager.Debug_IdentifierOfInterest3 == Id))
//        {
//            Debug.LogWarning("Debugging CalculateShortestPolityCoreDistance");
//            Manager.Debug_BreakRequested = false;
//        }
//#endif

        if (Polity.CoreGroup == Group)
            return 0;

        return CalculateShortestCoreDistance(false, out _);
    }

    /// <summary>
    /// Calculates the current shortest polity or faction core distance
    /// </summary>
    /// <param name="toFactionCore">look for shortest faction core distance instead
    /// of polity core distance</param>
    /// <param name="closestFaction">the faction that whose core is closest, or the
    /// polity's dominant faction if 'toFactionCore' is false</param>
    /// <returns>the calculated shortest core distance</returns>
    private float CalculateShortestCoreDistance(
        bool toFactionCore, out Faction closestFaction)
    {
        float shortestDistance = MaxCoreDistance;
        closestFaction = Polity.DominantFaction;

        foreach (var pair in NeighborProminencesInPolity)
        {
            float distanceToCoreFromNeighbor =
                pair.Value.GetCurrentCoreDistance(toFactionCore);

            if (distanceToCoreFromNeighbor == -1)
                continue;

            if (distanceToCoreFromNeighbor >= MaxCoreDistance)
                continue;

            float neighborDistance = Group.Cell.NeighborDistances[pair.Key];

            float totalDistance = distanceToCoreFromNeighbor + neighborDistance;

            if (totalDistance < 0)
                continue;

            if (totalDistance < shortestDistance)
            {
                shortestDistance = totalDistance;
                closestFaction = pair.Value.ClosestFaction;
            }
        }

        return shortestDistance;
    }

    /// <summary>
    /// Recalculates the current administrative cost
    /// </summary>
    private void RecalculateAdministrativeCost()
    {
        float polityPopulation = Group.Population * Value;

        float distConst = 500;
        float distanceFactor = (distConst + FactionCoreDistance) / distConst;

        _adminCost = polityPopulation * distanceFactor;
        _adminCost = Mathf.Min(_adminCost, MaxAdminCost);

        if (_adminCost < 0)
        {
            throw new System.Exception("Calculated administrative cost less than 0: "
                + _adminCost + ", group: " + Group.Id + ", polity: " + Polity.Id);
        }

        _adminCostUpdateNeeded = false;
    }

    /// <summary>
    /// Makes sure the core distances for this prominence's neighbors are reset
    /// </summary>
    public void ResetNeighborCoreDistances()
    {
        Identifier idFactionBeingReset = ClosestFactionId;
        float minFactionDistance = FactionCoreDistance;

        foreach (var pair in NeighborProminencesInPolity)
        {
            PolityProminence prom = pair.Value;

            if (prom.ClosestFactionId != idFactionBeingReset)
                continue;

            if (prom.FactionCoreDistance < minFactionDistance)
                continue;

            prom.ResetCoreDistances(idFactionBeingReset, minFactionDistance);
        }
    }

    /// <summary>
    /// Makes sure the core distances for this prominence and some neighbors are reset
    /// </summary>
    /// <param name="idFactionBeingReset">the faction distances are being reset for</param>
    /// <param name="minFactionDistance">the min distance at which to stop reseting distances</param>
    public void ResetCoreDistances(
        Identifier idFactionBeingReset = null,
        float minFactionDistance = float.MaxValue,
        bool addToRecalcs = false)
    {
        if (idFactionBeingReset == null)
        {
            idFactionBeingReset = ClosestFactionId;
            minFactionDistance = FactionCoreDistance;
        }

        Queue<PolityProminence> prominencesToReset = new Queue<PolityProminence>();
        HashSet<PolityProminence> prominencesToResetSet = new HashSet<PolityProminence>();

        prominencesToReset.Enqueue(this);
        prominencesToResetSet.Add(this);

        while (prominencesToReset.Count > 0)
        {
            PolityProminence prom = prominencesToReset.Dequeue();

            if ((prom.FactionCoreDistance == MaxCoreDistance) &&
                (prom.PolityCoreDistance == MaxCoreDistance))
                continue; // It's already reset

            prom.FactionCoreDistance = MaxCoreDistance;
            prom.PolityCoreDistance = MaxCoreDistance;

            prom.SetClosestFaction(null);

#if DEBUG
            prom.LastCoreDistanceReset = Manager.CurrentWorld.CurrentDate;
#endif

            bool isExpansionLimit = false;

            foreach (var pair in prom.NeighborProminencesInPolity)
            {
                PolityProminence nProm = pair.Value;

                if (!nProm.StillPresent ||
                    (nProm.ClosestFactionId != idFactionBeingReset) || 
                    (nProm.FactionCoreDistance < minFactionDistance))
                {
                    isExpansionLimit = true;
                    continue;
                }

                if (prominencesToResetSet.Contains(nProm))
                    continue;

                prominencesToReset.Enqueue(nProm);
                prominencesToResetSet.Add(nProm);
            }

            if (isExpansionLimit)
            {
                Polity.World.AddPromToCalculateCoreDistFor(prom);
            }
        }

        if (addToRecalcs)
        {
            Polity.World.AddPromToCalculateCoreDistFor(this);
        }
    }

    /// <summary>
    /// Post update operations
    /// </summary>
    public void PostUpdate()
    {
        RequireRecalculations();
    }

    /// <summary>
    /// Indicate that will need to recalculate some values
    /// </summary>
    private void RequireRecalculations()
    {
        // Indicate that the administrative cost of this prominence will need to be
        // recalculated
        _adminCostUpdateNeeded = true;

        if (Cluster != null)
        {
            // Indicate that the cluster this prominence belongs to will require a
            // new census
            Cluster.RequireNewCensus(true);
        }
    }

    public void SetAllNeighborProminences()
    {
        SetNeighborProminencesFromGroup(Group);

        foreach (var nGroup in Group.NeighborGroups)
        {
            SetNeighborProminencesFromGroup(nGroup);
        }
    }

    private void SetNeighborProminencesFromGroup(CellGroup group)
    {
        foreach (PolityProminence p in group.GetPolityProminences())
        {
            if (p == this)
            {
                continue;
            }

            SetProminenceAsNeighbors(p, this);
        }
    }

    private static void SetProminenceAsNeighbors(PolityProminence a, PolityProminence b)
    {
        if (a == b)
        {
            throw new System.ArgumentException($"Both a and b are the same prominence");
        }

        a.AddNeighborProminence(b);
        b.AddNeighborProminence(a);
    }

    private void AddNeighborProminence(PolityProminence p)
    {
        if (NeighborProminences.Add(p))
        {
            throw new System.ArgumentException($"trying to add prominence twice. this:{Id}, p:{p.Id}");
        }
        
        ClosestFaction?.AddNeighborFaction(p.ClosestFaction);
    }

    private void UnsetAllNeighborProminences()
    {
        UnsetNeighborProminencesFromGroup(Group);

        foreach (var nGroup in Group.NeighborGroups)
        {
            UnsetNeighborProminencesFromGroup(nGroup);
        }
    }

    private void UnsetNeighborProminencesFromGroup(CellGroup group)
    {
        foreach (PolityProminence p in group.GetPolityProminences())
        {
            if (p == this)
            {
                continue;
            }

            UnsetProminenceAsNeighbors(p, this);
        }
    }

    private static void UnsetProminenceAsNeighbors(PolityProminence a, PolityProminence b)
    {
        if (a == b)
        {
            throw new System.ArgumentException($"Both a and b are the same prominence");
        }

        a.RemoveNeighborProminence(b);
        b.RemoveNeighborProminence(a);
    }

    private void RemoveNeighborProminence(PolityProminence p)
    {
        if (NeighborProminences.Remove(p))
        {
            throw new System.ArgumentException($"trying to remove prominence twice. this:{Id}, p:{p.Id}");
        }

        ClosestFaction?.RemoveNeighborFaction(p.ClosestFaction);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence// : IKeyedValue<Identifier>
{
    public const float MaxCoreDistance = 1000000000000f;

    [XmlAttribute("V")]
    public float Value = 0;
    [XmlAttribute("FCT")]
    public float FactionCoreDistance = -1;
    [XmlAttribute("PD")]
    public float PolityCoreDistance = -1;

    public Identifier PolityId;

    public Identifier Id => Group.Id;

    [XmlIgnore]
    public PolityProminenceCluster Cluster = null;

    [XmlIgnore]
    public Polity Polity;

    [XmlIgnore]
    public CellGroup Group;

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
        Polity = polity;
        PolityId = polity.Id;
        Value = initialValue;
    }

    // Not necessarily ordered, do not use during serialization or algorithms that
    // have a dependency on consistent order
    [XmlIgnore]
    public IEnumerable<KeyValuePair<Direction, PolityProminence>> NeighborProminences
    {
        get
        {
            foreach (KeyValuePair<Direction, CellGroup> pair in Group.Neighbors)
            {
                if (pair.Value.TryGetPolityProminence(Polity, out PolityProminence p))
                    yield return new KeyValuePair<Direction, PolityProminence>(pair.Key, p);
            }
        }
    }

    /// <summary>
    /// Define the new core distances to update this prominence with
    /// </summary>
    /// <returns>'true' iff core distances have changed</returns>
    public bool CalculateNewCoreDistances()
    {
        float newFactionCoreDistance = CalculateShortestFactionCoreDistance();
        float newPolityCoreDistance = CalculateShortestPolityCoreDistance();

        // Make sure at least one core distance is actually different
        if ((FactionCoreDistance != newFactionCoreDistance) ||
            (PolityCoreDistance != newPolityCoreDistance))
        {
            FactionCoreDistance = newFactionCoreDistance;
            PolityCoreDistance = newPolityCoreDistance;

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
    /// <returns>the calculated shortest core distance</returns>
    public float CalculateShortestFactionCoreDistance()
    {
        foreach (Faction faction in Polity.GetFactions())
        {
            if (faction.CoreGroup == Group)
                return 0;
        }

        return CalculateShortestCoreDistance(true);
    }

    /// <summary>
    /// Calculates the current shortest polity core distance
    /// </summary>
    /// <returns>the calculated shortest core distance</returns>
    public float CalculateShortestPolityCoreDistance()
    {
        if (Polity.CoreGroup == Group)
            return 0;

        return CalculateShortestCoreDistance(false);
    }

    /// <summary>
    /// Calculates the current shortest polity or faction core distance
    /// </summary>
    /// <param name="toFactionCore">look for shortest faction core distance instead
    /// of polity core distance</param>
    /// <returns>the calculated shortest core distance</returns>
    private float CalculateShortestCoreDistance(bool toFactionCore)
    {
        float shortestDistance = MaxCoreDistance;

        foreach (KeyValuePair<Direction, PolityProminence> pair in NeighborProminences)
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
                shortestDistance = totalDistance;
        }

        return shortestDistance;
    }

    /// <summary>
    /// Recalculates the current administrative cost
    /// </summary>
    private void RecalculateAdministrativeCost()
    {
        float polityPopulation = Group.Population * Value;

        float distanceFactor = 500 + FactionCoreDistance;

        _adminCost = polityPopulation * distanceFactor * 0.001f;

        if (_adminCost < 0)
        {
            throw new System.Exception("Calculated administrative cost less than 0: "
                + _adminCost + ", group: " + Group.Id + ", polity: " + Polity.Id);
        }

        _adminCostUpdateNeeded = false;
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
            // Indicate that the cluster this prominence belongs too will require a
            // new census
            Cluster.RequireNewCensus(true);
        }
    }

    //public Identifier GetKey()
    //{
    //    return PolityId;
    //}
}

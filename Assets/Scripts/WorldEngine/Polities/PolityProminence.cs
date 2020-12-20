using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence// : IKeyedValue<Identifier>
{
    [XmlAttribute("V")]
    public float Value = 0;
    [XmlAttribute("FCT")]
    public float FactionCoreDistance = -1;
    [XmlAttribute("PD")]
    public float PolityCoreDistance = -1;

    public Identifier PolityId;

    public Identifier Id => Group.Id;

    [XmlIgnore]
    public float NewFactionCoreDistance = -1;
    [XmlIgnore]
    public float NewPolityCoreDistance = -1;

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

    /// <summary>
    /// Define the new core distances to update this prominence with
    /// </summary>
    /// <returns>'true' iff core distances have changed</returns>
    public bool CalculateNewCoreDistances()
    {
        float newFactionCoreDistance = Group.CalculateShortestFactionCoreDistance(Polity);
        float newPolityCoreDistance = Group.CalculateShortestPolityCoreDistance(Polity);

        // Make sure at least one core distance is actually different
        if ((NewFactionCoreDistance != newFactionCoreDistance) ||
            (NewPolityCoreDistance != newPolityCoreDistance))
        {
            NewFactionCoreDistance = newFactionCoreDistance;
            NewPolityCoreDistance = newPolityCoreDistance;

            return true;
        }

        return false;
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

    /// <summary>
    /// Replace the old values and distances with the new ones
    /// </summary>
    public void PostUpdateCoreDistances()
    {
        if ((FactionCoreDistance == NewFactionCoreDistance) &&
            (PolityCoreDistance == NewPolityCoreDistance))
        {
            // There's no need to do anything else if nothing changed
            return;
        }

        PolityCoreDistance = NewPolityCoreDistance;
        FactionCoreDistance = NewFactionCoreDistance;

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
    }

    //public Identifier GetKey()
    //{
    //    return PolityId;
    //}
}

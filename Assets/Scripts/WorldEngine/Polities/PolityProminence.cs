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
    [XmlAttribute("AC")]
    public float AdministrativeCost = 0;

    public Identifier PolityId;

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

    public Identifier Id => Group.Id;

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
    public void CalculateNewCoreDistances()
    {
        NewFactionCoreDistance = Group.CalculateShortestFactionCoreDistance(Polity);
        NewPolityCoreDistance = Group.CalculateShortestPolityCoreDistance(Polity);
    }

    /// <summary>
    /// Replace the old values and distances with the new ones and recalculated admin cost
    /// </summary>
    public void PostUpdate()
    {
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

        AdministrativeCost = Group.CalculateAdministrativeCost(this);

        if (Cluster != null)
        {
            Cluster.RequireNewCensus(true);
        }
    }

    //public Identifier GetKey()
    //{
    //    return PolityId;
    //}
}

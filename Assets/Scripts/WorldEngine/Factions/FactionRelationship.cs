using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionRelationship : Identifiable
{
    [XmlAttribute("Val")]
    public float Value;

    [XmlIgnore]
    public Faction Faction;

    public FactionRelationship()
    {
    }

    public FactionRelationship(Faction faction, float value) : base(faction.Info)
    {
        Faction = faction;

        Value = value;
    }
}

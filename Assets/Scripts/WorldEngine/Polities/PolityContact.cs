using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityContact : Identifiable
{
    [XmlAttribute("GC")]
    public int GroupCount;

    [XmlIgnore]
    public Polity NeighborPolity;

    [XmlIgnore]
    public Polity ThisPolity;

    [XmlIgnore]
    public float Strength => _strength.Value;

    private readonly DatedValue<float> _strength;

    public PolityContact()
    {
    }

    public PolityContact(
        Polity thisPolity,
        Polity neighborPolity,
        int initialGroupCount = 0) :
        base(neighborPolity.Info)
    {
        ThisPolity = thisPolity;
        NeighborPolity = neighborPolity;

        _strength =
            new DatedValue<float>(ThisPolity.World, CalculateStrength);

        GroupCount = initialGroupCount;
    }

    private float CalculateStrength()
    {
        int thisGroupCount = ThisPolity.Groups.Count;
        int neighborGroupCount = NeighborPolity.Groups.Count;

        float minPolityGroupCount = Mathf.Min(thisGroupCount, neighborGroupCount);

        if (minPolityGroupCount == 0)
        {
            throw new System.Exception("Min polity group count can't be zero");
        }

        return GroupCount / minPolityGroupCount;
    }
}

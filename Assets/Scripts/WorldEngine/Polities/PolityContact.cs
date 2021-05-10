using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityContact : Identifiable, ISynchronizable
{
    [XmlAttribute("GC")]
    public int GroupCount;

    #region NeighborPolityId
    [XmlAttribute("NPId")]
    public string NeighborPolityIdStr
    {
        get { return NeighborPolityId; }
        set { NeighborPolityId = value; }
    }
    [XmlIgnore]
    public Identifier NeighborPolityId;
    #endregion

    #region ThisPolityId
    [XmlAttribute("TPId")]
    public string ThisPolityIdStr
    {
        get { return ThisPolityId; }
        set { ThisPolityId = value; }
    }
    [XmlIgnore]
    public Identifier ThisPolityId;
    #endregion

    [XmlIgnore]
    public Polity NeighborPolity;

    [XmlIgnore]
    public Polity ThisPolity;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public float Strength => _strength.Value;

    private DatedValue<float> _strength;

    public PolityContact()
    {
    }

    public PolityContact(
        World world,
        Polity thisPolity,
        Polity neighborPolity,
        int initialGroupCount = 0) :
        base(neighborPolity.Info)
    {
        World = world;

        ThisPolity = thisPolity;
        ThisPolityId = ThisPolity.Id;

        NeighborPolity = neighborPolity;
        NeighborPolityId = neighborPolity.Id;

        _strength =
            new DatedValue<float>(World, CalculateStrength);

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

    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
        ThisPolity = World.GetPolity(ThisPolityId);
        NeighborPolity = World.GetPolity(NeighborPolityId);

        _strength =
            new DatedValue<float>(World, CalculateStrength);
    }
}

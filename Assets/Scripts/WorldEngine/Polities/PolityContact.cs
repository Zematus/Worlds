using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityContact : Identifiable, ISynchronizable
{
    [XmlAttribute("GC")]
    public int Count = 0;

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
        Polity neighborPolity) :
        base(neighborPolity.Info)
    {
        World = world;

        ThisPolity = thisPolity;
        ThisPolityId = ThisPolity.Id;

        NeighborPolity = neighborPolity;
        NeighborPolityId = neighborPolity.Id;

        InitDatedValues();
    }

    private void InitDatedValues()
    {
        _strength = new DatedValue<float>(World, CalculateStrength);
    }

    private float CalculateStrength()
    {
        int thisGroupCount = ThisPolity.Groups.Count;
        int neighborGroupCount = NeighborPolity.Groups.Count;

        float minCountFactor = Mathf.Min(thisGroupCount, neighborGroupCount) * 9;

        if (minCountFactor == 0)
        {
            throw new System.Exception("Min count can't be zero");
        }

        return Count / minCountFactor;
    }

    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
        ThisPolity = World.GetPolity(ThisPolityId);
        NeighborPolity = World.GetPolity(NeighborPolityId);

        InitDatedValues();
    }
}

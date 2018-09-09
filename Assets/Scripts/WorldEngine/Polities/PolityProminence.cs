using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence : IKeyedValue<long> {

//#if DEBUG
//    public static long CurrentDebugId = 0;

//    public long DebugId = -1;

//    [XmlAttribute("PolityId")]
//    public long PolityId_Debug;

//    [XmlIgnore]
//    public long PolityId
//    {
//        get
//        {
//            return PolityId_Debug;
//        }
//        set
//        {
//            //if (!_isMigratingGroup && (Group.Cell.Latitude == 108) && (Group.Cell.Longitude == 362))
//            //{
//            //    Debug.Log("PolityProminence:PolityId:Set - Group.Cell:" + Group.Cell.Position +
//            //    ", old PolityId: " + PolityId_Debug + ", new PolityId: " + value);
//            //}

//            PolityId_Debug = value;
//        }
//    }
//#else
    [XmlAttribute]
	public long PolityId;
//#endif
    [XmlAttribute("Val")]
	public float Value;
	[XmlAttribute("FctDist")]
	public float FactionCoreDistance;
	[XmlAttribute("PolDist")]
	public float PolityCoreDistance;
	[XmlAttribute("Cost")]
	public float AdministrativeCost;

	[XmlIgnore]
	public float NewValue;
	[XmlIgnore]
	public float NewFactionCoreDistance;
	[XmlIgnore]
	public float NewPolityCoreDistance;

    [XmlIgnore]
    public PolityProminenceCluster Cluster;

    private bool _isMigratingGroup;

	[XmlIgnore]
	public Polity Polity;

    [XmlIgnore]
    public CellGroup Group;

    public long Id
    {
        get
        {
            return Group.Id;
        }
    }

    public PolityProminence () {

    }

    public PolityProminence(PolityProminence polityProminence)
    {
        Group = polityProminence.Group;

        _isMigratingGroup = true;

        Set(polityProminence);
    }

    public PolityProminence (CellGroup group, PolityProminence polityProminence)
    {
        Group = group;

        _isMigratingGroup = false;

//#if DEBUG
//        if (!isMigratingGroup)
//        {
//            DebugId = CurrentDebugId;
//            CurrentDebugId++;
//        }
//#endif

        Set(polityProminence);
	}

	public void Set (PolityProminence polityProminence)
    {
        PolityId = polityProminence.PolityId;
		Polity = polityProminence.Polity;
		Value = polityProminence.Value;
		NewValue = Value;

		AdministrativeCost = 0;
	}

	public PolityProminence (CellGroup group, Polity polity, float value, bool isMigratingGroup = false)
    {
        Group = group;

        _isMigratingGroup = isMigratingGroup;

//#if DEBUG
//        if (!isMigratingGroup)
//        {
//            DebugId = CurrentDebugId;
//            CurrentDebugId++;
//        }
//#endif

        Set(polity, value);
	}

	public void Set (Polity polity, float value)
    {
        PolityId = polity.Id;
		Polity = polity;
		Value = MathUtility.RoundToSixDecimals (value);
		NewValue = Value;

		AdministrativeCost = 0;
    }

    public void PostUpdate () {

		Value = NewValue;
		PolityCoreDistance = NewPolityCoreDistance;
		FactionCoreDistance = NewFactionCoreDistance;

#if DEBUG
		if (FactionCoreDistance == -1) {

			throw new System.Exception ("Core distance is not properly initialized");
		}
#endif

#if DEBUG
		if (PolityCoreDistance == -1) {

			throw new System.Exception ("Core distance is not properly initialized");
		}
#endif
	}

    public long GetKey()
    {
        return PolityId;
    }
}

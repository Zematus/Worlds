using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence {

#if DEBUG
    public static long CurrentDebugId = 0;

    public long DebugId = -1;

    [XmlAttribute("PolityId")]
    public long PolityId_Debug;

    [XmlIgnore]
    public long PolityId
    {
        get
        {
            return PolityId_Debug;
        }
        set
        {
            //if (!_isMigratingGroup && (Group.Cell.Latitude == 108) && (Group.Cell.Longitude == 362))
            //{
            //    Debug.Log("PolityProminence:PolityId:Set - Group.Cell:" + Group.Cell.Position +
            //    ", old PolityId: " + PolityId_Debug + ", new PolityId: " + value);
            //}

            PolityId_Debug = value;
        }
    }
#else
    [XmlAttribute]
	public long PolityId;
#endif
    [XmlAttribute("Val")]
	public float Value;
	[XmlAttribute("FctDist")]
	public float FactionCoreDistance;
	[XmlAttribute("PolDist")]
	public float PolityCoreDistance;
	[XmlAttribute("Cost")]
	public float AdiministrativeCost;

	[XmlIgnore]
	public float NewValue;
	[XmlIgnore]
	public float NewFactionCoreDistance;
	[XmlIgnore]
	public float NewPolityCoreDistance;

	private bool _isMigratingGroup;

	[XmlIgnore]
	public Polity Polity;

    [XmlIgnore]
    public CellGroup Group;

    public PolityProminence () {

	}

	public PolityProminence (PolityProminence polityProminence, bool isMigratingGroup = false)
    {
        Group = polityProminence.Group;

        _isMigratingGroup = isMigratingGroup;

        if (!isMigratingGroup)
        {
            DebugId = CurrentDebugId;
            CurrentDebugId++;
        }
        
        Set (polityProminence);
	}

	public void Set (PolityProminence polityProminence)
    {
        PolityId = polityProminence.PolityId;
		Polity = polityProminence.Polity;
		Value = polityProminence.Value;
		NewValue = Value;

		AdiministrativeCost = 0;

		PolityCoreDistance = polityProminence.PolityCoreDistance;
		NewPolityCoreDistance = PolityCoreDistance;

		FactionCoreDistance = polityProminence.FactionCoreDistance;
		NewFactionCoreDistance = FactionCoreDistance;
	}

	public PolityProminence (CellGroup group, Polity polity, float value, float polityCoreDistance = -1, float factionCoreDistance = -1, bool isMigratingGroup = false)
    {
        Group = group;

        _isMigratingGroup = isMigratingGroup;

        if (!isMigratingGroup)
        {
            DebugId = CurrentDebugId;
            CurrentDebugId++;
        }

        Set (polity, value, polityCoreDistance, factionCoreDistance);
	}

	public void Set (Polity polity, float value, float polityCoreDistance = -1, float factionCoreDistance = -1)
    {
        PolityId = polity.Id;
		Polity = polity;
		Value = MathUtility.RoundToSixDecimals (value);
		NewValue = Value;

		AdiministrativeCost = 0;

		PolityCoreDistance = polityCoreDistance;
		NewPolityCoreDistance = polityCoreDistance;

		FactionCoreDistance = factionCoreDistance;
		NewFactionCoreDistance = factionCoreDistance;
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
}

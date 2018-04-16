using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityProminence {

	[XmlAttribute]
	public long PolityId;
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

	public PolityProminence () {

	}

	public PolityProminence (Polity polity, float value, float polityCoreDistance = -1, float factionCoreDistance = -1) {
	
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

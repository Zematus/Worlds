using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalPreferenceInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;

	[XmlAttribute("RO")]
	public int RngOffset;
	
	public CulturalPreferenceInfo () {
	}
	
	public CulturalPreferenceInfo (string id, string name, int rngOffset) {
		
		Id = id;
		Name = name;
		RngOffset = rngOffset;
	}
	
	public CulturalPreferenceInfo (CulturalPreferenceInfo basePreference) {
		
		Id = basePreference.Id;
		Name = basePreference.Name;
		RngOffset = basePreference.RngOffset;
	}
}

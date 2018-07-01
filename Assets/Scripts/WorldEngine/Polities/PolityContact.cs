using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityContact {

	[XmlAttribute("Id")]
	public long Id;

	[XmlAttribute("GCount")]
	public int GroupCount;

	[XmlIgnore]
	public Polity Polity;

	public PolityContact () {
	}

	public PolityContact (Polity polity, int initialGroupCount = 0) {

		Polity = polity;

		Id = polity.Id;

		GroupCount = initialGroupCount;
	}
}
